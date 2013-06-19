using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>A class-to-methods lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
	public class MethodPtrRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Object Method { get { return base.GetValue<Object>(0); } }
	}
}