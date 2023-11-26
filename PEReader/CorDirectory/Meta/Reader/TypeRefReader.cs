using System;
using System.Collections.Generic;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Class reader based on strongly typed metadata <see cref="Cor.MetaTableType.TypeRef"/> table</summary>
	public class TypeRefReader
	{
		/// <summary>Class from reference assembly</summary>
		public TypeRefRow TypeRef { get; }

		/// <summary>Gets the fully qualified name of the <see cref="System.Type"/>, including the namespace of the <see cref="System.Type"/> but not the assembly.</summary>
		public String FullName
			=> this.TypeRef.TypeNamespace == String.Empty
				? this.TypeRef.TypeName
				: this.TypeRef.TypeNamespace + "." + this.TypeRef.TypeName;

		/// <summary>Create instance of <see cref="TypeRefReader"/></summary>
		/// <param name="typeRef">Strongly typed metadata <see cref="Cor.MetaTableType.TypeRef"/> row</param>
		/// <exception cref="ArgumentNullException"><c>typeRef</c> is required</exception>
		public TypeRefReader(TypeRefRow typeRef)			=> this.TypeRef = typeRef ?? throw new ArgumentNullException(nameof(TypeRef));

		/// <summary>Gets list of all members in the current type (Including properties)</summary>
		/// <returns>List of <see cref="MemberRefRow"/> instances related to current reference type</returns>
		public IEnumerable<MemberRefRow> GetMembers()
		{
			foreach(MemberRefRow row in this.TypeRef.Row.Table.Root.MemberRef)
				if(row.Class.TargetRow == this.TypeRef)
					yield return row;
		}
	}
}