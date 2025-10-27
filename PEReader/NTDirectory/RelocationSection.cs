using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Relocation section</summary>
	public struct RelocationSection
	{//TODO: In fact, each processor has its own RELOCATION instructions (see Reference: http://msdn.microsoft.com/library/windows/hardware/gg463125)
		/// <summary>type of relocation</summary>
		public WinNT.IMAGE_REL_BASED Type { get; }

		/// <summary>Offset</summary>
		public Int32 Offset { get; }

		/// <summary>Create instance of relocation section</summary>
		/// <param name="section">Offset from beginning of directory</param>
		public RelocationSection(UInt16 section)
		{
			this.Type = (WinNT.IMAGE_REL_BASED)((section & 0xf000) >> 12);
			this.Offset = section & 0x0fff;
		}
	}
}