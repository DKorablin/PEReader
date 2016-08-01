using System;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Base class of sections header</summary>
	[DefaultProperty("Header")]
	[DebuggerDisplay("Header Name={Header}")]
	public class NTSection : ISectionData
	{
		private readonly PEFile _parent;
		private readonly WinNT.IMAGE_SECTION_HEADER _header;

		/// <summary>PE directory</summary>
		internal PEFile Parent { get { return this._parent; } }

		/// <summary>Section header descriptor</summary>
		public WinNT.IMAGE_SECTION_HEADER Header { get { return this._header; } }

		/// <summary>Create instance of section header reader class</summary>
		/// <param name="parent">PE directory</param>
		/// <param name="header">sections header descriptor</param>
		public NTSection(PEFile parent, WinNT.IMAGE_SECTION_HEADER header)
		{
			if(parent == null)
				throw new ArgumentNullException("parent");

			this._parent = parent;
			this._header = header;
		}

		/// <summary>Get RAW data from section</summary>
		/// <returns>RAW data from section</returns>
		public Byte[] GetData()
		{
			UInt32 size = this.Header.VirtualSize > this.Header.SizeOfRawData
				? this.Header.SizeOfRawData
				: this.Header.VirtualSize;

			return this.Parent.Header.ReadBytes(this.Header.VirtualAddress, size);
		}
	}
}