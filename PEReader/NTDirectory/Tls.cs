namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Thread Local Storage directory</summary>
	public class Tls : PEDirectoryBase
	{
		/// <summary>TLS PE directory</summary>
		public WinNT.Tls.IMAGE_TLS_DIRECTORY32? TlsDirectory32
			=> this.IsEmpty || this.Parent.Header.Is64Bit
				? (WinNT.Tls.IMAGE_TLS_DIRECTORY32?)null
				: this.Parent.Header.PtrToStructure<WinNT.Tls.IMAGE_TLS_DIRECTORY32>(this.Directory.VirtualAddress);

		/// <summary>TLS PE+ directory</summary>
		public WinNT.Tls.IMAGE_TLS_DIRECTORY64? TlsDirectory64
			=> this.IsEmpty || !this.Parent.Header.Is64Bit
				? (WinNT.Tls.IMAGE_TLS_DIRECTORY64?)null
				: this.Parent.Header.PtrToStructure<WinNT.Tls.IMAGE_TLS_DIRECTORY64>(this.Directory.VirtualAddress);

		/// <summary>Create instance of TLS class</summary>
		/// <param name="root">Data directory</param>
		public Tls(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.TLS) { }
	}
}