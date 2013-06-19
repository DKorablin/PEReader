using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Forwarded DLL reference</summary>
	public struct BoundImportReference
	{
		/// <summary>Reference</summary>
		public WinNT.IMAGE_BOUND_FORWARDER_REF FfwdRef { get; set; }
		/// <summary>Module name</summary>
		public String ModuleName { get; set; }
	}
}