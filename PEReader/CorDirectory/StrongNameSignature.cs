using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>.NET Strong name class</summary>
	public class StrongNameSignature : CorDirectoryBase
	{
		/// <summary>Create instance of String name hash class</summary>
		/// <param name="parent">.NET directory</param>
		public StrongNameSignature(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.StrongNameSignature) { }
	}
}