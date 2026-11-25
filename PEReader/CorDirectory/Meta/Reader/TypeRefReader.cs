using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Class reader based on strongly typed metadata <see cref="Cor.MetaTableType.TypeRef"/> table</summary>
	[DebuggerDisplay(nameof(FullName) + " = {" + nameof(FullName) + "}")]
	public class TypeRefReader
	{
		private TypeReader _typeDef;
		private AssemblyRefReader _assemblyRef;

		/// <summary>Class reference from current assembly</summary>
		public TypeRefRow TypeRef { get; }

		/// <summary>Class from referenced assembly</summary>
		/// <remarks>Requires <see cref="PEReaderOptions.ResolveAssemblies"/> for types declared in other assemblies. Type forwarders are followed.</remarks>
		public TypeReader TypeDef
		{
			get => this._typeDef ?? (this._typeDef = new TypeReader(this.ReadDefRow()));
		}

		/// <summary>Gets the fully qualified name of the <see cref="System.Type"/>, including the namespace of the <see cref="System.Type"/> but not the assembly.</summary>
		public String FullName
		{
			get
			{
				if(this.TypeRef != null)
					return this.TypeRef.TypeNamespace == String.Empty
						? this.TypeRef.TypeName
						: this.TypeRef.TypeNamespace + "." + this.TypeRef.TypeName;
				else
					return this.TypeDef.FullName;
			}
		}

		/// <summary>Gets an assembly from which this type is referenced.</summary>
		/// <returns>Referenced assembly information</returns>
		public AssemblyRefReader AssemblyRef
		{
			get
			{
				if(this.TypeRef.ResolutionScope.TableType != Cor.MetaTableType.AssemblyRef)
					return null;//TODO: Add TypeRef handling

				if(this._assemblyRef == null)
					foreach(AssemblyRefRow row in this.TypeRef.Row.Table.Root.AssemblyRef)
						if(this.TypeRef.ResolutionScope.TargetRow == row)
							this._assemblyRef = new AssemblyRefReader(row);

				return this._assemblyRef;
			}
		}

		/// <summary>Create instance of <see cref="TypeRefReader"/></summary>
		/// <param name="typeRef">Strongly typed metadata <see cref="Cor.MetaTableType.TypeRef"/> row</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeRef"/> is required</exception>
		public TypeRefReader(TypeRefRow typeRef)
			=> this.TypeRef = typeRef ?? throw new ArgumentNullException(nameof(typeRef));

		/// <summary>Create instance of <see cref="TypeRefReader"/> with TypeDefOrRef pointer.</summary>
		/// <param name="cellPointer">Cell pointer to <see cref="TypeRefRow"/>.</param>
		/// <exception cref="ArgumentException"></exception>
		public TypeRefReader(CellPointerBase cellPointer)
		{
			_ = cellPointer ?? throw new ArgumentNullException(nameof(cellPointer));
			switch(cellPointer.TableType)
			{
			case Cor.MetaTableType.TypeDef:
				TypeDefRow defRow = new TypeDefRow() { Row = cellPointer.TargetRow, };
				this._typeDef = new TypeReader(defRow);
				break;
			case Cor.MetaTableType.TypeRef:
				this.TypeRef = new TypeRefRow() { Row = cellPointer.TargetRow, };
				break;
			default:
				throw new ArgumentException($"TypeReader suitable only for {nameof(TypeDefRow)} or {nameof(TypeRefRow)}. TableType: {cellPointer.TableType}");
			}
		}

		/// <summary>Gets list of all members in the current type (Including properties)</summary>
		/// <returns>List of <see cref="MemberRefRow"/> instances related to current reference type</returns>
		public IEnumerable<MemberRefRow> GetMembers()
		{
			foreach(MemberRefRow row in this.TypeRef.Row.Table.Root.MemberRef)
				if(row.Class.TargetRow == this.TypeRef)
					yield return row;
		}

		/// <summary>Reads <see cref="TypeDefRow"/> from the current or referenced assembly</summary>
		/// <exception cref="InvalidOperationException">Assembly resolution is disabled or the type is not found</exception>
		/// <exception cref="FileNotFoundException">Can't find referenced assembly</exception>
		/// <returns>Definition row from the current or referenced assembly</returns>
		private TypeDefRow ReadDefRow()
		{
			TypeResolver resolver = this.TypeRef.Row.Table.Root.TypeResolver;
			String serializedName = resolver.GetSerializedName(this.TypeRef);
			TypeDefRow result = resolver.ResolveTypeDef(serializedName);
			if(result != null)
				return result;

			PEFile owner = resolver.Owner;
			if(owner.AssemblyResolver == null)
				throw new InvalidOperationException($"Type {serializedName} is declared in another assembly. Set {nameof(PEReaderOptions)}.{nameof(PEReaderOptions.ResolveAssemblies)} to resolve referenced assemblies");

			AssemblyRefReader assemblyRef = this.AssemblyRef;
			if(assemblyRef != null && assemblyRef.File == null)
				throw new FileNotFoundException($"Referenced assembly file not found. {nameof(assemblyRef.SourceAssemblyPath)}:{assemblyRef.SourceAssemblyPath}", assemblyRef.AssemblyRef.Name);

			throw new InvalidOperationException($"Type {serializedName} not found");
		}
	}
}