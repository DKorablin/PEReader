using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>A class-to-fields lookup table, whitch does not exist on optimized metadata (#~ stream)</summary>
	public class FieldPtrRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Object Field { get { return base.GetValue<Object>(0); } }

		/// <summary>Create instance of FieldPtr row</summary>
		public FieldPtrRow()
			: base(Cor.MetaTableType.FieldPtr) { }
	}
}