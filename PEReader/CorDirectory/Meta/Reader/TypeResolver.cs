using System;
using System.Collections.Generic;
using System.Reflection;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Resolves type names of the current module to type definitions in the current or referenced assemblies</summary>
	/// <remarks>
	/// Type names are formatted as in serialized custom attributes (ECMA-335 II.23.3):
	/// <c>Namespace.Outer+Inner</c> for types from the current assembly and <c>Namespace.Outer+Inner, AssemblyFullName</c> for external types.
	/// External types are resolved only if <see cref="PEFile.AssemblyResolver"/> is available (see <see cref="PEReaderOptions"/>).
	/// </remarks>
	internal sealed class TypeResolver
	{
		/// <summary>Protection against cyclic references in malformed NestedClass, TypeRef or ExportedType tables</summary>
		private const Int32 MaxNestingDepth = 64;

		/// <summary>Protection against cyclic type forwarding between assemblies</summary>
		private const Int32 MaxForwardingDepth = 8;

		/// <summary>Core library names in the search priority for not qualified type names</summary>
		private static readonly String[] CoreLibraryNames = new String[] { "System.Private.CoreLib", "System.Runtime", "mscorlib", "netstandard", };

		private readonly StreamTables _root;
		private Dictionary<UInt32, TypeDefRow> _typeDefs;
		private Dictionary<UInt32, UInt32> _enclosingTypes;
		private Dictionary<String, TypeDefRow> _localTypes;
		private Dictionary<String, AssemblyRefRow> _forwarders;
		private Dictionary<String, Cor.ELEMENT_TYPE> _enumUnderlyingTypes;
		private String[] _assemblyNames;

		internal TypeResolver(StreamTables root)
			=> this._root = root ?? throw new ArgumentNullException(nameof(root));

		/// <summary>File that contains the current module</summary>
		internal PEFile Owner => this._root.Parent.Parent.Parent;

		/// <summary>Types declared in the current module: serialized name → type definition</summary>
		private Dictionary<String, TypeDefRow> LocalTypes
		{
			get
			{
				if(this._localTypes == null)
				{
					Dictionary<String, TypeDefRow> localTypes = new Dictionary<String, TypeDefRow>();
					foreach(TypeDefRow typeDef in this.TypeDefs.Values)
					{
						String fullName = this.GetFullName(typeDef);
						if(!localTypes.ContainsKey(fullName))
							localTypes.Add(fullName, typeDef);
					}
					this._localTypes = localTypes;
				}
				return this._localTypes;
			}
		}

		/// <summary>Types forwarded from the current assembly to other assemblies: serialized name → target assembly</summary>
		private Dictionary<String, AssemblyRefRow> Forwarders
		{
			get
			{
				if(this._forwarders == null)
				{
					Dictionary<String, AssemblyRefRow> forwarders = new Dictionary<String, AssemblyRefRow>();
					foreach(ExportedTypeRow exportedType in this._root.ExportedType)
					{
						String fullName = this.GetFullName(exportedType, out AssemblyRefRow assemblyRef, 0);
						if(assemblyRef != null && !forwarders.ContainsKey(fullName))
							forwarders.Add(fullName, assemblyRef);
					}
					this._forwarders = forwarders;
				}
				return this._forwarders;
			}
		}

		/// <summary>All types declared in the current module by row index</summary>
		private Dictionary<UInt32, TypeDefRow> TypeDefs
		{
			get
			{
				if(this._typeDefs == null)
				{
					Dictionary<UInt32, TypeDefRow> typeDefs = new Dictionary<UInt32, TypeDefRow>();
					foreach(TypeDefRow typeDef in this._root.TypeDef)
						typeDefs[typeDef.Index] = typeDef;
					this._typeDefs = typeDefs;
				}
				return this._typeDefs;
			}
		}

		/// <summary>Nested type row index → enclosing type row index</summary>
		private Dictionary<UInt32, UInt32> EnclosingTypes
		{
			get
			{
				if(this._enclosingTypes == null)
				{
					Dictionary<UInt32, UInt32> enclosingTypes = new Dictionary<UInt32, UInt32>();
					foreach(NestedClassRow nestedClass in this._root.NestedClass)
						enclosingTypes[nestedClass.NestedClass.Index] = nestedClass.EnclosingClass.Index;
					this._enclosingTypes = enclosingTypes;
				}
				return this._enclosingTypes;
			}
		}

		/// <summary>Names of the current assembly from the Assembly table (empty for a netmodule)</summary>
		private String[] AssemblyNames
		{
			get
			{
				if(this._assemblyNames == null)
				{
					List<String> assemblyNames = new List<String>();
					foreach(AssemblyRow assembly in this._root.Assembly)
						assemblyNames.Add(assembly.Name);
					this._assemblyNames = assemblyNames.ToArray();
				}
				return this._assemblyNames;
			}
		}

		/// <summary>Gets serialized name of the type referenced from a signature (<see cref="Cor.ELEMENT_TYPE.VALUETYPE"/> token)</summary>
		/// <param name="typeDefOrRef">TypeDef or TypeRef pointer</param>
		/// <returns>Serialized type name or null if the type can't be named (TypeSpec)</returns>
		public String GetSerializedName(CellPointerBase typeDefOrRef)
		{
			switch(typeDefOrRef?.TableType)
			{
			case Cor.MetaTableType.TypeDef:
				return this.GetFullName(typeDefOrRef.GetTargetRowTyped<TypeDefRow>());
			case Cor.MetaTableType.TypeRef:
				return this.GetSerializedName(typeDefOrRef.GetTargetRowTyped<TypeRefRow>());
			default:
				return null;
			}
		}

		/// <summary>Gets serialized name of the referenced type</summary>
		/// <param name="typeRef">Type reference from the current module</param>
		/// <returns>Type name qualified with the full name of the referenced assembly if the type is declared in another assembly</returns>
		public String GetSerializedName(TypeRefRow typeRef)
		{
			_ = typeRef ?? throw new ArgumentNullException(nameof(typeRef));

			String typeName = this.GetFullName(typeRef, out AssemblyRefRow assemblyRef, 0);
			return assemblyRef == null
				? typeName
				: typeName + ", " + GetAssemblyFullName(assemblyRef);
		}

		/// <summary>Finds definition of the type in the current or referenced assemblies</summary>
		/// <param name="serializedName">Type name as it stored in custom attribute: <c>Namespace.Outer+Inner[, AssemblyName[, Version=...]]</c></param>
		/// <returns>Type definition or null if the type is not found or referenced assemblies are not resolved</returns>
		public TypeDefRow ResolveTypeDef(String serializedName)
		{
			_ = serializedName ?? throw new ArgumentNullException(nameof(serializedName));

			String typeName = SplitAssemblyName(serializedName, out String assemblyName);
			if(assemblyName == null || this.IsCurrentAssembly(assemblyName))
			{
				TypeDefRow result = this.FindTypeDef(typeName, 0);
				if(result != null || assemblyName != null)
					return result;

				// Not qualified type name refers to the current assembly or to the core library
				return this.ResolveExternal(typeName, this.GetCoreLibraryReference(), 0);
			}

			return this.ResolveExternal(typeName, ParseAssemblyName(assemblyName), 0);
		}

		/// <summary>Gets underlying type of the enum</summary>
		/// <param name="serializedName">Enum type name as it stored in custom attribute: <c>Namespace.Outer+Inner[, AssemblyName[, Version=...]]</c></param>
		/// <returns>Underlying type of the enum or <see cref="Cor.ELEMENT_TYPE.I4"/> (the most common underlying type) if the enum can't be resolved</returns>
		public Cor.ELEMENT_TYPE GetEnumUnderlyingType(String serializedName)
		{
			if(serializedName == null)
				return Cor.ELEMENT_TYPE.I4;
			if(this._enumUnderlyingTypes == null)
				this._enumUnderlyingTypes = new Dictionary<String, Cor.ELEMENT_TYPE>();
			else if(this._enumUnderlyingTypes.TryGetValue(serializedName, out Cor.ELEMENT_TYPE cached))
				return cached;

			Cor.ELEMENT_TYPE result = Cor.ELEMENT_TYPE.I4;
			TypeDefRow typeDef = this.ResolveTypeDef(serializedName);
			if(typeDef != null)
				foreach(FieldRow field in typeDef.FieldList)
					if(field.Name == "value__")
					{// Instance field "value__" is reserved for the enum value (ECMA-335 II.14.3)
						Cor.ELEMENT_TYPE underlyingType = field.ReturnType.Type;
						if(underlyingType >= Cor.ELEMENT_TYPE.BOOLEAN && underlyingType <= Cor.ELEMENT_TYPE.U8)
							result = underlyingType;
						break;
					}

			this._enumUnderlyingTypes[serializedName] = result;
			return result;
		}

		/// <summary>Finds type declared in the current module or forwarded from the current assembly</summary>
		/// <param name="typeName">Type name without assembly name</param>
		/// <param name="depth">Current forwarding depth</param>
		private TypeDefRow FindTypeDef(String typeName, Int32 depth)
		{
			if(this.LocalTypes.TryGetValue(typeName, out TypeDefRow result))
				return result;

			return this.Forwarders.TryGetValue(typeName, out AssemblyRefRow assemblyRef)
				? this.ResolveExternal(typeName, GetAssemblyName(assemblyRef), depth + 1)
				: null;
		}

		/// <summary>Finds type in the referenced assembly using <see cref="PEFile.AssemblyResolver"/></summary>
		/// <param name="typeName">Type name without assembly name</param>
		/// <param name="reference">Referenced assembly name</param>
		/// <param name="depth">Current forwarding depth</param>
		private TypeDefRow ResolveExternal(String typeName, AssemblyName reference, Int32 depth)
		{
			if(reference == null || depth > MaxForwardingDepth)
				return null;

			PEFile owner = this.Owner;
			IAssemblyResolver resolver = owner.AssemblyResolver;
			if(resolver == null)
				return null;

			PEFile target = resolver.Resolve(reference, owner);
			StreamTables tables = target?.ComDescriptor?.MetaData?.StreamTables;
			return tables?.TypeResolver.FindTypeDef(typeName, depth);
		}

		/// <summary>Gets reference to the core library of the current module</summary>
		/// <returns>Core library reference or null if the module doesn't reference any known core library</returns>
		private AssemblyName GetCoreLibraryReference()
		{
			foreach(String coreLibraryName in CoreLibraryNames)
				foreach(AssemblyRefRow assemblyRef in this._root.AssemblyRef)
					if(String.Equals(assemblyRef.Name, coreLibraryName, StringComparison.OrdinalIgnoreCase))
						return GetAssemblyName(assemblyRef);
			return null;
		}

		private Boolean IsCurrentAssembly(String assemblyName)
		{
			Int32 index = assemblyName.IndexOf(',');
			String simpleName = (index < 0 ? assemblyName : assemblyName.Substring(0, index)).Trim();

			return Array.Exists(this.AssemblyNames, a => String.Equals(a, simpleName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>Formats full type name including enclosing types: <c>Namespace.Outer+Inner</c></summary>
		private String GetFullName(TypeDefRow typeDef)
		{
			String result = typeDef.TypeName;
			UInt32 index = typeDef.Index;
			for(Int32 depth = 0; depth < MaxNestingDepth && this.EnclosingTypes.TryGetValue(index, out UInt32 enclosingIndex); depth++)
			{
				if(!this.TypeDefs.TryGetValue(enclosingIndex, out typeDef))
					break;
				result = typeDef.TypeName + "+" + result;
				index = enclosingIndex;
			}

			return typeDef.TypeNamespace == String.Empty
				? result
				: typeDef.TypeNamespace + "." + result;
		}

		/// <summary>Formats full type name including enclosing types: <c>Namespace.Outer+Inner</c></summary>
		/// <param name="typeRef">Type reference</param>
		/// <param name="assemblyRef">Referenced assembly or null if type is declared in the current assembly (or the scope is unknown)</param>
		/// <param name="depth">Current nesting depth</param>
		private String GetFullName(TypeRefRow typeRef, out AssemblyRefRow assemblyRef, Int32 depth)
		{
			assemblyRef = null;
			String typeName = typeRef.TypeNamespace == String.Empty
				? typeRef.TypeName
				: typeRef.TypeNamespace + "." + typeRef.TypeName;

			MetaCellCodedToken scope = typeRef.ResolutionScope;
			if(scope.TargetRow == null)
				return typeName;

			switch(scope.TableType)
			{
			case Cor.MetaTableType.TypeRef:// Nested type: scope is the enclosing type
				if(depth >= MaxNestingDepth)
					return typeName;
				return this.GetFullName(scope.GetTargetRowTyped<TypeRefRow>(), out assemblyRef, depth + 1) + "+" + typeName;
			case Cor.MetaTableType.AssemblyRef:
				assemblyRef = scope.GetTargetRowTyped<AssemblyRefRow>();
				return typeName;
			default:// Module (current module) or ModuleRef (another module of the current assembly)
				return typeName;
			}
		}

		/// <summary>Formats full name of the exported type including enclosing types: <c>Namespace.Outer+Inner</c></summary>
		/// <param name="exportedType">Exported type</param>
		/// <param name="assemblyRef">Assembly where the type is forwarded or null if the type is declared in another module of the current assembly</param>
		/// <param name="depth">Current nesting depth</param>
		private String GetFullName(ExportedTypeRow exportedType, out AssemblyRefRow assemblyRef, Int32 depth)
		{
			assemblyRef = null;
			String typeName = String.IsNullOrEmpty(exportedType.TypeNamespace)
				? exportedType.TypeName
				: exportedType.TypeNamespace + "." + exportedType.TypeName;

			MetaCellCodedToken implementation = exportedType.Implementation;
			if(implementation.TargetRow == null)
				return typeName;

			switch(implementation.TableType)
			{
			case Cor.MetaTableType.ExportedType:// Nested type: implementation is the enclosing type
				if(depth >= MaxNestingDepth)
					return typeName;
				return this.GetFullName(implementation.GetTargetRowTyped<ExportedTypeRow>(), out assemblyRef, depth + 1) + "+" + typeName;
			case Cor.MetaTableType.AssemblyRef:// Type forwarder
				assemblyRef = implementation.GetTargetRowTyped<AssemblyRefRow>();
				return typeName;
			default:// File (another module of the current assembly)
				return typeName;
			}
		}

		/// <summary>Gets assembly name of the reference</summary>
		/// <returns>Assembly name or null if the reference is malformed (unknown culture, etc.)</returns>
		private static AssemblyName GetAssemblyName(AssemblyRefRow assemblyRef)
		{
			try
			{
				return assemblyRef.AssemblyName;
			} catch(ArgumentException)
			{// CultureNotFoundException or invalid public key
				return null;
			}
		}

		/// <summary>Gets full assembly name of the reference or simple name if the reference is malformed</summary>
		private static String GetAssemblyFullName(AssemblyRefRow assemblyRef)
			=> GetAssemblyName(assemblyRef)?.FullName ?? assemblyRef.Name;

		/// <summary>Parses assembly name from the serialized type name</summary>
		/// <returns>Assembly name or null if the name is malformed</returns>
		private static AssemblyName ParseAssemblyName(String assemblyName)
		{
			try
			{
				return new AssemblyName(assemblyName);
			} catch(Exception)
			{// ArgumentException, FileLoadException for malformed names
				return null;
			}
		}

		/// <summary>Splits serialized type name to type name and assembly name</summary>
		/// <param name="serializedName">Serialized type name: <c>Namespace.Type[, AssemblyName[, Version=...]]</c></param>
		/// <param name="assemblyName">Assembly name or null if type name is not qualified</param>
		/// <returns>Type name without assembly name</returns>
		private static String SplitAssemblyName(String serializedName, out String assemblyName)
		{
			Int32 index = serializedName.IndexOf(',');
			if(index < 0)
			{
				assemblyName = null;
				return serializedName.Trim();
			}

			assemblyName = serializedName.Substring(index + 1).Trim();
			return serializedName.Substring(0, index).Trim();
		}
	}
}