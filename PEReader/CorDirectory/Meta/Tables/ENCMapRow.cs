using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Edit-and-continue mapping descriptors</summary>
	/// <remarks>This table does not exist in optimized metadata (#~ stream)</remarks>
	public class ENCMapRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public UInt32 Token { get { return base.GetValue<UInt32>(0); } }

		/// <summary>Create instance of Edit-and-continue mapping descriptors row</summary>
		public ENCMapRow()
			: base(Cor.MetaTableType.ENCMap) { }
	}
}