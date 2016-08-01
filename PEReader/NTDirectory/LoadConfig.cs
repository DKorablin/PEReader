using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>The load configuration table address and size.</summary>
	public class LoadConfig : PEDirectoryBase
	{
		/// <summary>PE load configuration directory entry</summary>
		public WinNT.LoadConfig.IMAGE_LOAD_CONFIG_DIRECTORY32? Directory32
		{
			get
			{
				if(!base.Directory.IsEmpty && !base.Parent.Header.Is64Bit)
					return base.Parent.Header.PtrToStructure<WinNT.LoadConfig.IMAGE_LOAD_CONFIG_DIRECTORY32>(base.Directory.VirtualAddress);
				else
					return null;
			}
		}
		/// <summary>PE+ load configuration directory entry</summary>
		public WinNT.LoadConfig.IMAGE_LOAD_CONFIG_DIRECTORY64? Directory64
		{
			get
			{
				if(!base.Directory.IsEmpty && base.Parent.Header.Is64Bit)
					return base.Parent.Header.PtrToStructure<WinNT.LoadConfig.IMAGE_LOAD_CONFIG_DIRECTORY64>(base.Directory.VirtualAddress);
				else
					return null;
			}
		}
		/// <summary>Create instance of LoadConfig class</summary>
		/// <param name="parent">Data directory</param>
		public LoadConfig(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.LOAD_CONFIG)
		{
		}
	}
}