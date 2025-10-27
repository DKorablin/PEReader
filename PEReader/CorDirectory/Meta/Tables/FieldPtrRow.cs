using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>A class-to-fields lookup table, which does not exist on optimized metadata (#~ stream)</summary>
	public class FieldPtrRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Object Field => base.GetValue<Object>(0);

		/// <summary>Create instance of <see cref="FieldPtrRow"/></summary>
		public FieldPtrRow()
			: base(Cor.MetaTableType.FieldPtr) { }
	}
}