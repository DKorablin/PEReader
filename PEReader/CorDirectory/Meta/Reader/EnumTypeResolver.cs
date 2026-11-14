using System;
using System.Collections.Generic;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Resolves underlying types of enums used in custom attribute values</summary>
	/// <remarks>
	/// Enum names are formatted as in serialized custom attributes (ECMA-335 II.23.3):
	/// <c>Namespace.Outer+Inner</c> for types from the current assembly and <c>Namespace.Outer+Inner, AssemblyName</c> for external types.
	/// </remarks>
	internal sealed class EnumTypeResolver
	{
		/// <summary>Protection against cyclic references in malformed NestedClass or TypeRef tables</summary>
		private const Int32 MaxNestingDepth = 64;

		private readonly StreamTables _root;
		private Dictionary<UInt32, TypeDefRow> _typeDefs;
		private Dictionary<UInt32, UInt32> _enclosingTypes;
		private Dictionary<String, Cor.ELEMENT_TYPE> _localEnums;
		private String[] _assemblyNames;

		internal EnumTypeResolver(StreamTables root)
			=> this._root = root ?? throw new ArgumentNullException(nameof(root));

		/// <summary>Enums declared in the current assembly: serialized name → underlying type</summary>
		private Dictionary<String, Cor.ELEMENT_TYPE> LocalEnums
		{
			get
			{
				if(this._localEnums == null)
				{
					Dictionary<String, Cor.ELEMENT_TYPE> localEnums = new Dictionary<String, Cor.ELEMENT_TYPE>();
					foreach(TypeDefRow typeDef in this.TypeDefs.Values)
						foreach(FieldRow field in typeDef.FieldList)
							if(field.Name == "value__")
							{// Instance field "value__" is reserved for the enum value (ECMA-335 II.14.3)
								Cor.ELEMENT_TYPE underlyingType = field.ReturnType.Type;
								if(underlyingType >= Cor.ELEMENT_TYPE.BOOLEAN && underlyingType <= Cor.ELEMENT_TYPE.U8)
									localEnums[this.GetFullName(typeDef)] = underlyingType;
								break;
							}
					this._localEnums = localEnums;
				}
				return this._localEnums;
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
				String typeName = this.GetFullName(typeDefOrRef.GetTargetRowTyped<TypeRefRow>(), out String assemblyName, 0);
				return assemblyName == null
					? typeName
					: typeName + ", " + assemblyName;
			default:
				return null;
			}
		}

		/// <summary>Gets underlying type of the enum</summary>
		/// <param name="serializedName">Enum type name as it stored in custom attribute: <c>Namespace.Outer+Inner[, AssemblyName[, Version=...]]</c></param>
		/// <returns>Underlying type of the enum</returns>
		public Cor.ELEMENT_TYPE GetUnderlyingType(String serializedName)
		{
			String typeName = SplitAssemblyName(serializedName, out String assemblyName);

			// Not qualified type name refers to the current assembly or to the core library
			if((assemblyName == null || this.IsCurrentAssembly(assemblyName))
				&& this.LocalEnums.TryGetValue(typeName, out Cor.ELEMENT_TYPE result))
				return result;

			return GetExternalUnderlyingType(typeName);
		}

		/// <summary>Gets underlying type of the enum declared outside of the current assembly</summary>
		/// <param name="typeName">Enum type name without assembly name</param>
		/// <returns>Underlying type of the enum</returns>
		private static Cor.ELEMENT_TYPE GetExternalUnderlyingType(String typeName)
		{
			_ = typeName;
			//BUG: Size of the enum declared in external assembly is stored only in that assembly. Int32 is the most common underlying type.
			return Cor.ELEMENT_TYPE.I4;
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
		/// <param name="assemblyName">Name of the referenced assembly or null if type is declared in the current assembly (or the scope is unknown)</param>
		/// <param name="depth">Current nesting depth</param>
		private String GetFullName(TypeRefRow typeRef, out String assemblyName, Int32 depth)
		{
			assemblyName = null;
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
				return this.GetFullName(scope.GetTargetRowTyped<TypeRefRow>(), out assemblyName, depth + 1) + "+" + typeName;
			case Cor.MetaTableType.AssemblyRef:
				assemblyName = scope.GetTargetRowTyped<AssemblyRefRow>().Name;
				return typeName;
			default:// Module (current module) or ModuleRef (another module of the current assembly)
				return typeName;
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