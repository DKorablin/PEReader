using System;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.PESection
{
	/// <summary>Base class of sections header</summary>
	[DefaultProperty("Header")]
	[DebuggerDisplay("Header {Header}")]
	public class SectionHeader : ISectionData
	{
		private readonly PEFile _parent;
		private readonly WinNT.IMAGE_SECTION_HEADER _header;

		/// <summary>PE directory</summary>
		internal PEFile Parent { get { return this._parent; } }

		/// <summary>Section header descriptor</summary>
		public WinNT.IMAGE_SECTION_HEADER Header { get { return this._header; } }

		/// <summary>Section name description</summary>
		public String Description { get { return Resources.Section.GetString(this.Header.Section); } }

		/// <summary>Create instance of section header reader class</summary>
		/// <param name="parent">PE directory</param>
		/// <param name="header">sections header descriptor</param>
		public SectionHeader(PEFile parent, WinNT.IMAGE_SECTION_HEADER header)
		{
			this._parent = parent?? throw new ArgumentNullException(nameof(parent));
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

		/// <summary>Create data reader for data in section</summary>
		/// <returns>Memory pinned data reader</returns>
		public PinnedBufferReader CreateDataReader()
		{
			return new PinnedBufferReader(this.GetData());
		}
	}
}