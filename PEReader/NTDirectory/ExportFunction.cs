using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Export function description</summary>
	public struct ExportFunction
	{
		/// <summary>Ordinal function index</summary>
		public UInt16 Ordinal { get; set; }

		/// <summary>Function name</summary>
		public String Name { get; set; }

		/// <summary>Function address</summary>
		public UInt32 Address { get; set; }
	}
}