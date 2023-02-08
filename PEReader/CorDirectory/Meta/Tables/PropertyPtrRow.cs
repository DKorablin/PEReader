using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>A property map-to-properties lookup table, whitch does not exists on optimized metadata (#~ stream)</summary>
	public class PropertyPtrRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public Object Property { get { return base.GetValue<Object>(0); } }
	}
}