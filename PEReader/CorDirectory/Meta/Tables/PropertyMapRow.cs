using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The PropertyMap and Property tables result from putting the .property directive on a class (§II.17)</summary>
	public class PropertyMapRow : BaseMetaRow
	{
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer ParentI { get { return base.GetValue<MetaCellPointer>(0); } }

		/// <summary>An index into the Property table</summary>
		internal MetaCellPointer PropertyListI { get { return base.GetValue<MetaCellPointer>(1); } }

		/// <summary>Parent TypeDef table row</summary>
		public TypeDefRow Parent
		{
			get { return new TypeDefRow() { Row = this.ParentI.TargetRow, }; }
		}
		/// <summary>Property table list</summary>
		public IEnumerable<PropertyRow> Properties
		{
			get
			{
				foreach(MetaRow row in this.PropertyListI.GetTargetRowsIt())
					yield return new PropertyRow() { Row = row, };
			}
		}
	}
}