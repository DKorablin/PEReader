using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The TypeSpec table has just one column, which indexes the specification of a Type, stored in the Blob heap.
	/// This provides a metadata token for that Type (rather than simply an index into the Blob heap).
	/// This is required, typically, for array operations, such as creating, or calling methods on the array class.
	/// </summary>
	public class TypeSpecRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Byte[] Signature { get { return base.GetValue<Byte[]>(0); } }
	}
}