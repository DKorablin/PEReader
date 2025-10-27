using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The TypeSpec table has just one column, which indexes the specification of a Type, stored in the Blob heap.
	/// This provides a metadata token for that Type (rather than simply an index into the Blob heap).
	/// This is required, typically, for array operations, such as creating, or calling methods on the array class
	/// </summary>
	public class TypeSpecRow : BaseMetaRow
	{
		/// <summary>Type specification signature</summary>
		public Byte[] Signature => base.GetValue<Byte[]>(0);

		/// <summary>Parsed type specification signature</summary>
		public ElementType SignatureParsed
		{
			get
			{
				UInt32 offset = 0;
				return new ElementType(this.Row.Cells[0], this.Signature, ref offset);
			}
		}

		/// <summary>Create instance of type specification row</summary>
		public TypeSpecRow()
			: base(Cor.MetaTableType.TypeSpec) { }
	}
}