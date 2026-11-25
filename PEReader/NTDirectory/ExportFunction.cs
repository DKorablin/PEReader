using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Export function description</summary>
	public struct ExportFunction
	{
		/// <summary>Ordinal of the exported function (biased by <see cref="WinNT.IMAGE_EXPORT_DIRECTORY.OrdinalBase"/>)</summary>
		public UInt16 Ordinal { get; set; }

		/// <summary>Function name or null if the function is exported by ordinal only</summary>
		public String Name { get; set; }

		/// <summary>Relative virtual address of the exported function (or of the forwarder string when <see cref="IsForwarded"/>)</summary>
		public UInt32 Rva { get; set; }

		/// <summary>Function address</summary>
		/// <remarks>Virtual address when the module is mapped into memory; otherwise equals to <see cref="Rva"/></remarks>
		public UInt64 Address { get; set; }

		/// <summary>Forwarder target in "DLL.Function" or "DLL.#Ordinal" form if the function is forwarded to another module; otherwise null</summary>
		public String ForwarderName { get; set; }

		/// <summary>The function is implemented in another module</summary>
		public Boolean IsForwarded => this.ForwarderName != null;
	}
}
