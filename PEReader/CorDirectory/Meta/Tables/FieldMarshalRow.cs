using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The FieldMarshal table has two columns.
	/// It "links" an existing row in the Field or Param table,
	/// to information in the Blob heap that defines how that field or parameter
	/// (which, as usual, covers the method return, as parameter number 0)
	/// shall be marshalled when calling to or from unmanaged code via PInvoke dispatch.
	/// </summary>
	/// <remarks>
	/// Note that FieldMarshal information is used only by code paths that arbitrate operation with unmanaged code.
	/// In order to execute such paths, the caller, on most platforms, would be installed with elevated security permission.
	/// Once it invokes unmanaged code, it lies outside the regime that the CLI can check - it is simply trusted not to violate the type system.
	/// </remarks>
	public class FieldMarshalRow : BaseMetaRow
	{
		/// <summary>An index into Field or Param table; more precisely, a HasFieldMarshal (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Parent { get { return base.GetValue<MetaCellCodedToken>(0); } }

		/// <summary>An index into the Blob heap</summary>
		public Byte[] Native { get { return base.GetValue<Byte[]>(1); } }

		/// <summary>Create instance of FieldMarshal row</summary>
		public FieldMarshalRow()
			: base(Cor.MetaTableType.FieldMarshal) { }
	}
}