using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>.NET Managed native header class</summary>
	public class ManagedNativeHeader : CorDirectoryBase
	{
		/// <summary>Create instance of Managed native header class</summary>
		/// <param name="parent">.NET directory</param>
		public ManagedNativeHeader(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.ManagedNativeHeader)
		{
		}
	}
}