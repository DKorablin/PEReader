using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>.NET Code Manager table class</summary>
	public class CodeManagerTable : CorDirectoryBase
	{
		/// <summary>Create instance of Code Manger table class</summary>
		/// <param name="parent">.NET directory</param>
		public CodeManagerTable(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.CodeManagerTable) { }
	}
}