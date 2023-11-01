using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>An event map-to-events lookup table, whitch does not exists on optimized metadata (#~ stream)</summary>
	public class EventPtrRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Object Event { get { return base.GetValue<Object>(0); } }

		/// <summary>Create instance of EventPtr row</summary>
		public EventPtrRow()
			: base(Cor.MetaTableType.EventPtr) { }
	}
}