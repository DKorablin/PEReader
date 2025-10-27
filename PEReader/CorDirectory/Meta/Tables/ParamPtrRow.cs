using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>A method-to-parameters lookup table, which does not exists on optimized metadata</summary>
	public class ParamPtrRow : BaseMetaRow
	{
		/// <summary>banana banana banana</summary>
		public Object Param => this.GetValue<Object>(0);

		/// <summary>Create instance of method-to-parameters lookup row</summary>
		public ParamPtrRow()
			: base(Cor.MetaTableType.ParamPtr) { }
	}
}