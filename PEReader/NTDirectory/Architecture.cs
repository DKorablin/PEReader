using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Architecture class</summary>
	public class Architecture : NTDirectoryBase
	{
		/// <summary>Create instance of Architecture class</summary>
		/// <param name="root">Data directory</param>
		public Architecture(PEDirectory root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.ARCHITECTURE)
		{
		}
	}
}