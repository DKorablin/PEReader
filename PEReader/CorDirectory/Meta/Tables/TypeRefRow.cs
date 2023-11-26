using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Class reference descriptors</summary>
	public class TypeRefRow : BaseMetaRow
	{
		/// <summary>
		/// Index into a <see cref="ModuleRow"/>, <see cref="ModuleRefRow"/>, <see cref="AssemblyRefRow"/> or <see cref="TypeRefRow"/> table, or null;
		/// more precisely, a ResolutionScope (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken ResolutionScope => base.GetValue<MetaCellCodedToken>(0);

		/// <summary>Type name</summary>
		public String TypeName => base.GetValue<String>(1);

		/// <summary>Type namespace</summary>
		public String TypeNamespace => base.GetValue<String>(2);

		/// <summary>Create instance of Class reference descriptors</summary>
		public TypeRefRow()
			: base(Cor.MetaTableType.TypeRef) { }

		/// <summary>TypeNamespace+"."+TypeName</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			String result = String.IsNullOrEmpty(this.TypeNamespace)
				? this.TypeName
				: this.TypeNamespace + "." + this.TypeName;

			return base.ToString(result);
		}
	}
}