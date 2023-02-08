using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The rows in the ModuleRef table result from .module extern directives in the Assembly (§II.6.5)</summary>
	public class ModuleRefRow : BaseMetaRow
	{
		/// <summary>
		/// This string shall enable the CLI to locate the target module
		/// (typically, it might name the file used to hold the module).
		/// </summary>
		public String Name { get { return base.GetValue<String>(0); } }

		/// <summary>Name</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return base.ToString(this.Name);
		}
	}
}