using System;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Thread Local Storage directory</summary>
	public class Tls : PEDirectoryBase
	{
		/// <summary>TLS PE directory</summary>
		public WinNT.Tls.IMAGE_TLS_DIRECTORY32? TlsDirectory32
		{
			get
			{
				if(base.IsEmpty || base.Parent.Header.Is64Bit)
					return null;
				else
					return base.Parent.Header.PtrToStructure<WinNT.Tls.IMAGE_TLS_DIRECTORY32>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>TLS PE+ directory</summary>
		public WinNT.Tls.IMAGE_TLS_DIRECTORY64? TlsDirectory64
		{
			get
			{
				if(base.IsEmpty || !base.Parent.Header.Is64Bit)
					return null;
				else
					return base.Parent.Header.PtrToStructure<WinNT.Tls.IMAGE_TLS_DIRECTORY64>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>Create instance of TLS class</summary>
		/// <param name="root">Data directory</param>
		public Tls(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.TLS)
		{

		}
	}
}