using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Edit-and-continue log descriptors that hold information about what changes have been made to specific metadata items during in-memory editing.
	/// This table does not exist in optimized metadata (#~ stream).
	/// </summary>
	public class ENCLogRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public UInt32 Token => base.GetValue<UInt32>(0);

		/// <summary>Banana</summary>
		public UInt32 FuncCode => base.GetValue<UInt32>(1);

		/// <summary>Create instance of Edit-and-continue log descriptors row</summary>
		public ENCLogRow()
			: base(Cor.MetaTableType.ENCLog) { }
	}
}