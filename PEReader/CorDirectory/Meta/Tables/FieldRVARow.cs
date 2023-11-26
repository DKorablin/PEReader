using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Field-to-data mapping descriptors</summary>
	/// <remarks>
	/// Conceptually, each row in the FieldRVA table is an extension to exactly one row in the Field table, and records the RVA (Relative Virtual Address) within the image file at which this field's initial value is stored.
	/// 
	/// A row in the FieldRVA table is created for each static parent field that has specified the optional data label §II.16).
	/// The RVA column is the relative virtual address of the data in the PE file (§II.16.3).
	/// </remarks>
	public class FieldRVARow : BaseMetaRow
	{
		/// <summary>a 4-byte constant</summary>
		public UInt32 RVA => base.GetValue<UInt32>(0);

		/// <summary>An index into Field table</summary>
		internal MetaCellPointer FieldI => base.GetValue<MetaCellPointer>(1);

		/// <summary>Row from Field table</summary>
		public FieldRow Field
			=> new FieldRow() { Row = this.FieldI.TargetRow, };

		/// <summary>Create instance of Field-to-data mapping descriptor row</summary>
		public FieldRVARow()
			: base(Cor.MetaTableType.FieldRVA) { }
	}
}