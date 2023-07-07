using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>CodeView PDB v2 class</summary>
	public struct DebugPdb2
	{
		/// <summary>PDB v2 Info</summary>
		public readonly WinNT.Debug.CV_INFO_PDB20 Info;

		/// <summary>Null-terminated name of the PDB file</summary>
		/// <remarks>It can also contain full or partial path to the file</remarks>
		public readonly String PdbFileName;

		/// <summary>Create instance of CodeView PDB v2 class</summary>
		/// <param name="info">CodeView v2 Info</param>
		/// <param name="pdbFileName">PDB file name saved in PE file</param>
		public DebugPdb2(WinNT.Debug.CV_INFO_PDB20 info, String pdbFileName)
		{
			this.Info = info;
			this.PdbFileName = pdbFileName;
		}
	}
}