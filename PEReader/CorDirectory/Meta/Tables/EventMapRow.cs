using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The EventMap and Event tables result from putting the .event directive on a class (§II.18)</summary>
	public class EventMapRow : BaseMetaRow
	{
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer ParentI => base.GetValue<MetaCellPointer>(0);

		/// <summary>An index into the Event table</summary>
		internal MetaCellPointer EventListI => base.GetValue<MetaCellPointer>(1);

		/// <summary>Row from TypeDef table</summary>
		public TypeDefRow Parent => new TypeDefRow() { Row = this.ParentI.TargetRow, };

		/// <summary>Rows from Event table</summary>
		public IEnumerable<EventRow> EventList
		{
			get
			{
				foreach(MetaRow row in this.EventListI.GetTargetRowsIt())
					yield return new EventRow() { Row = row, };
			}
		}

		/// <summary>Create instance of EventMap row</summary>
		public EventMapRow()
			: base(Cor.MetaTableType.EventMap) { }
	}
}