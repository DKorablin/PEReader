namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>The RVA of the value to be stored in the global pointer register</summary>
	public class GlobalPtr : PEDirectoryBase
	{
		/// <summary>Create instance of TLS class</summary>
		/// <param name="root">Data directory</param>
		public GlobalPtr(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.GLOBALPTR) { }
	}
}