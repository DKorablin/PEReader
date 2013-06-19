using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>CodeView PDB v7 class</summary>
	public struct DebugPdb7
	{
		/// <summary>PDB v7 Info</summary>
		public readonly WinNT.Debug.CV_INFO_PDB70 Info;
		/// <summary>Null-terminated name of the PDB file. It can also contain full or partial path to the file.</summary>
		public readonly String PdbFileName;
		/// <summary>Create instance of CodeView PDB v7 class</summary>
		/// <param name="info">CodeView v7 Info</param>
		/// <param name="pdbFileName">PDB file name saved in PE file</param>
		public DebugPdb7(WinNT.Debug.CV_INFO_PDB70 info, String pdbFileName)
		{
			this.Info = info;
			this.PdbFileName = pdbFileName;
		}
	}
}