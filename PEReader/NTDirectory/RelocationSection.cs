using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Relocation section</summary>
	public struct RelocationSection
	{//TODO: На самом деле, для каждого процессора свои RELOCATION инструкции (см. Reference: http://msdn.microsoft.com/library/windows/hardware/gg463125)
		private readonly WinNT.IMAGE_REL_BASED _type;
		private readonly Int32 _offset;
		/// <summary>type of relocation</summary>
		public WinNT.IMAGE_REL_BASED Type { get { return this._type; } }
		/// <summary>Offset</summary>
		public Int32 Offset { get { return this._offset; } }
		/// <summary>Create instance of relocation section</summary>
		/// <param name="section">Offset from beggining of directory</param>
		public RelocationSection(UInt16 section)
		{
			this._type = (WinNT.IMAGE_REL_BASED)((section & 0xf000) >> 12);
			this._offset = section & 0x0fff;
		}
	}
}