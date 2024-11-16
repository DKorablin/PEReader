using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Class reader based on strongly typed metadata <see cref="Cor.MetaTableType.TypeRef"/> table</summary>
	[DebuggerDisplay(nameof(FullName) + " = {" + nameof(FullName) + "}")]
	public class TypeRefReader : IDisposable
	{
		private TypeReader _typeDef;
		private AssemblyRefReader _assemblyRef;

		/// <summary>Class refernce from current assembly</summary>
		public TypeRefRow TypeRef { get; }

		/// <summary>Class from referenced assembly</summary>
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
		/// <exception cref="ArgumentNullException"><c>typeRef</c> is required</exception>
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

		/// <summary>Reads <see cref="TypeDefRow"/> from referenced assembly</summary>
		/// <exception cref="InvalidOperationException">Assembly reference not found</exception>
		/// <exception cref="ArgumentException">Row not found</exception>
		/// <exception cref="FileNotFoundException">Can't find assembly in the current path</exception>
		/// <returns>Pointer to definition row from referenced assembly</returns>
		private TypeDefRow ReadDefRow()
		{
			_ = this.AssemblyRef ?? throw new InvalidOperationException($"{nameof(this.AssemblyRef)} not found for {this.FullName} type");

			PEFile file = this.AssemblyRef.File
				?? throw new FileNotFoundException($"Referenced assembly file not found. {nameof(this.AssemblyRef.SourceAssemblyPath)}:{this.AssemblyRef.SourceAssemblyPath}", this.AssemblyRef.AssemblyRef.AssemblyName.Name);
			
			var typeRefRow = this.TypeRef;
			return file.ComDescriptor.MetaData.StreamTables.TypeDef.Single(p => p.TypeNamespace == typeRefRow.TypeNamespace && p.TypeName == typeRefRow.TypeName);
		}

		#region IDisposable
		/// <summary>Desctructor for <see cref="TypeRefReader"/> if user forgets to dispose this object</summary>
		~TypeRefReader()
			=> this.Dispose(false);

		/// <summary><see cref="IDisposable"/> pattern implementation to close <see cref="AssemblyRef"/> file</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary><see cref="IDisposable"/> pattern implemntation</summary>
		/// <param name="disposing">Is object disposing or called from destructor</param>
		protected virtual void Dispose(Boolean disposing)
			=> this._assemblyRef?.Dispose();
		#endregion IDisposable
	}
}