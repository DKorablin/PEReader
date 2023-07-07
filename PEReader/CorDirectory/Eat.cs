using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Export Address table jumps class</summary>
	public class Eat : CorDirectoryBase
	{
		/// <summary>Create instance of EAT class</summary>
		/// <param name="parent">Data directory</param>
		public Eat(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.ExportAddressTableJumps)
		{
		}
	}
}