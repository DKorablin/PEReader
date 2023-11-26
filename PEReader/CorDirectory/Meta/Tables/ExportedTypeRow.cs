using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Exported type descriptors that contain information about public classes exported by the current assembly, whitch are declared in other modules of the assembly.
	/// Only the prime module of the assembly sould carry this table
	/// </summary>
	public class ExportedTypeRow : BaseMetaRow
	{
		/// <summary>Specifies type attributes</summary>
		public TypeAttributes Flags => (TypeAttributes)base.GetValue<UInt32>(0);

		/// <summary>
		/// A 4-byte index into a TypeDef table of another module in this Assembly).
		/// This column is used as a hint only.
		/// If the entry in the target TypeDef table matches the TypeName and TypeNamespace entries in this table, resolution has succeeded.
		/// But if there is a mismatch, the CLI shall fall back to a search of the target TypeDef table.
		/// Ignored and should be zero if Flags has IsTypeForwarder set.
		/// </summary>
		public UInt32 TypeDefId => base.GetValue<UInt32>(1);

		/// <summary>Type name</summary>
		public String TypeName => base.GetValue<String>(2);

		/// <summary>Type namespace</summary>
		public String TypeNamespace => base.GetValue<String>(3);

		/// <summary>
		/// This is an index (more precisely, an Implementation (§II.24.2.6) coded index) into either of the following tables:
		///		File table, where that entry says which module in the current assembly holds the TypeDef
		///		ExportedType table, where that entry is the enclosing Type of the current nested Type.
		///		AssemblyRef table, where that entry says in which assembly the type may now be found (Flags must have the IsTypeForwarder flag set).
		/// </summary>
		public MetaCellCodedToken Implementation => base.GetValue<MetaCellCodedToken>(4);

		/// <summary>Create instance of ExportedType row</summary>
		public ExportedTypeRow()
			: base(Cor.MetaTableType.ExportedType) { }

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