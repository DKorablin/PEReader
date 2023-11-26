using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The PropertyMap and Property tables result from putting the .property directive on a class (§II.17)</summary>
	public class PropertyMapRow : BaseMetaRow
	{
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer ParentI => base.GetValue<MetaCellPointer>(0);

		/// <summary>An index into the Property table</summary>
		internal MetaCellPointer PropertyListI => base.GetValue<MetaCellPointer>(1);

		/// <summary>Parent TypeDef table row</summary>
		public TypeDefRow Parent => new TypeDefRow() { Row = this.ParentI.TargetRow, };

		/// <summary>Property table list</summary>
		public IEnumerable<PropertyRow> PropertyList
		{
			get
			{
				foreach(MetaRow row in this.PropertyListI.GetTargetRowsIt())
					yield return new PropertyRow() { Row = row, };
			}
		}

		/// <summary>Create isntance of Property map row</summary>
		public PropertyMapRow()
			: base(Cor.MetaTableType.PropertyMap) { }
	}
}