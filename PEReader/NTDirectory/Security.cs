using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Certificate class</summary>
	[DefaultProperty("Certificate")]
	public class Security : PEDirectoryBase
	{
		/// <summary>PE file contains certificate</summary>
		public override Boolean IsEmpty
		{
			get
			{
				return base.IsEmpty || base.Parent.Header.Loader.IsModuleMapped;
			}
		}
		/// <summary>Certificate header</summary>
		/// <exception cref="T:ArgumentOutOfRangeException">Directory VA out of file size</exception>
		public WinNT.WIN_CERTIFICATE? Certificate
		{
			get
			{
				if(this.IsEmpty)
					return null;//TODO: Читается только из физического файла. Т.к. сертификат находится в файле. И ссылка идёт на VA, а не на RVA.
				else return base.Parent.Header.Loader.PtrToStructure<WinNT.WIN_CERTIFICATE>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>X.509 certificate</summary>
		public X509Certificate2 X509
		{
			get
			{
				var cert = this.Certificate;
				if(cert.HasValue &&
					(cert.Value.wCertificateType == WinNT.WIN_CERT_TYPE.X509
					|| cert.Value.wCertificateType==WinNT.WIN_CERT_TYPE.PKCS_SIGNED_DATA))
				{
					UInt32 sizeOfStruct = (UInt32)Marshal.SizeOf(typeof(WinNT.WIN_CERTIFICATE));
					UInt32 offset = base.Directory.VirtualAddress + sizeOfStruct;
					return new X509Certificate2(base.Parent.Header.Loader.ReadBytes(offset, checked(cert.Value.dwLength - sizeOfStruct)));
				} else
					return null;
			}
		}
		/// <summary>Create instance of certificate class</summary>
		/// <param name="root">Data directory</param>
		public Security(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.CERTIFICATE)
		{
		}
	}
}