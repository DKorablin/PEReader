using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Version management functions, types, and definitions</summary>
	/// <remarks>
	/// Include file for VER.DLL.
	/// This library is designed to allow version stamping of Windows executable files and of special .VER files for DOS executable files
	/// </remarks>
	public struct WinVer
	{
		/// <summary>Contains version information for a file</summary>
		/// <remarks>This information is language and code page independent</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct VS_FIXEDFILEINFO
		{
			/// <summary>Contains the value 0xFEEF04BD</summary>
			/// <remarks>This is used with the szKey member of the <see cref="WinNT.Resource.VS_VERSIONINFO"/> structure when searching a file for the <see cref="VS_FIXEDFILEINFO"/> structure</remarks>
			public UInt32 dwSignature;

			/// <summary>The binary version number of this structure</summary>
			/// <remarks>The high-order word of this member contains the major version number, and the low-order word contains the minor version number</remarks>
			public UInt32 dwStructVersion;

			/// <summary>The most significant 32 bits of the file's binary version number</summary>
			/// <remarks>This member is used with dwFileVersionLS to form a 64-bit value used for numeric comparisons</remarks>
			public UInt32 dwFileVersionMS;

			/// <summary>The least significant 32 bits of the file's binary version number</summary>
			/// <remarks>This member is used with dwFileVersionMS to form a 64-bit value used for numeric comparisons</remarks>
			public UInt32 dwFileVersionLS;

			/// <summary>The most significant 32 bits of the binary version number of the product with which this file was distributed</summary>
			/// <remarks>This member is used with dwProductVersionLS to form a 64-bit value used for numeric comparisons</remarks>
			public UInt32 dwProductVersionMS;

			/// <summary>The least significant 32 bits of the binary version number of the product with which this file was distributed</summary>
			/// <remarks>This member is used with dwProductVersionMS to form a 64-bit value used for numeric comparisons</remarks>
			public UInt32 dwProductVersionLS;

			/// <summary>Contains a bitmask that specifies the valid bits in dwFileFlags</summary>
			/// <remarks>A bit is valid only if it was defined when the file was created</remarks>
			public UInt32 dwFileFlagMask;

			/// <summary>Contains a bitmask that specifies the Boolean attributes of the file</summary>
			/// <remarks>This member can include one or more of the following values</remarks>
			public VS_FF dwFileFlags;

			/// <summary>The operating system for which this file was designed</summary>
			/// <remarks>
			/// An application can combine these values to indicate that the file was designed for one operating system running on another.
			/// The following dwFileOS values are examples of this, but are not a complete list
			/// </remarks>
			public VOS dwFileOS;

			/// <summary>The general type of file</summary>
			/// <remarks>All other values are reserved</remarks>
			public VFT dwFileType;

			/// <summary>The function of the file</summary>
			/// <remarks>
			/// The possible values depend on the value of dwFileType.
			/// For all values of dwFileType not described in the following list, dwFileSubtype is zero.
			/// If dwFileType is VFT_DRV, dwFileSubtype can be one of the following values
			/// </remarks>
			public VFT2 dwFileSubtype;

			/// <summary>The most significant 32 bits of the file's 64-bit binary creation date and time stamp</summary>
			public UInt32 dwFileDateMS;

			/// <summary>The least significant 32 bits of the file's 64-bit binary creation date and time stamp</summary>
			public UInt32 dwFileDateLS;

			/// <summary>Structure is valid</summary>
			public Boolean IsValid { get { return this.dwSignature == 0xFEEF04BD; } }

			/// <summary>File version number</summary>
			public Version FileVersion
			{
				get
				{
					return new Version(
						NativeMethods.HiWord(this.dwFileVersionMS),
						NativeMethods.LoWord(this.dwFileVersionMS),
						NativeMethods.HiWord(this.dwFileVersionLS),
						NativeMethods.LoWord(this.dwFileVersionLS));
				}
			}

			/// <summary>Product version number</summary>
			public Version ProductVersion
			{
				get
				{
					return new Version(
						NativeMethods.HiWord(this.dwProductVersionMS),
						NativeMethods.LoWord(this.dwProductVersionMS),
						NativeMethods.HiWord(this.dwProductVersionLS),
						NativeMethods.LoWord(this.dwProductVersionLS)
						);
				}
			}
		}

		/// <summary>Contains a bitmask that specifies the Boolean attributes of the file</summary>
		[Flags]
		public enum VS_FF
		{
			/// <summary>The file contains debugging information or is compiled with debugging features enabled</summary>
			DEBUG = 0x00000001,
			/// <summary>The file's version structure was created dynamically; therefore, some of the members in this structure may be empty or incorrect. This flag should never be set in a file's VS_VERSIONINFO data</summary>
			INFOINFERRED = 0x00000010,
			/// <summary>The file has been modified and is not identical to the original shipping file of the same version number</summary>
			PATCHED = 0x00000004,
			/// <summary>The file is a development version, not a commercially released product</summary>
			PRERELEASE = 00000002,
			/// <summary>The file was not built using standard release procedures</summary>
			/// <remarks>If this flag is set, the StringFileInfo structure should contain a PrivateBuild entry</remarks>
			PRIVATEBUILD = 00000008,
			/// <summary>The file was built by the original company using standard release procedures but is a variation of the normal file of the same version number</summary>
			/// <remarks>If this flag is set, the StringFileInfo structure should contain a SpecialBuild entry</remarks>
			SPECIALBUILD = 00000020,
		}

		/// <summary>The operating system for which this file was designed</summary>
		[Flags]
		public enum VOS
		{
			/// <summary>The file was designed for MS-DOS</summary>
			DOS = 0x00010000,
			/// <summary>The file was designed for Windows NT</summary>
			NT = 0x00040000,
			/// <summary>The file was designed for 16-bit Windows</summary>
			WINDOWS16 = 0x00000001,
			/// <summary>The file was designed for 32-bit Windows</summary>
			WINDOWS32 = 0x00000004,
			/// <summary>The file was designed for 16-bit OS/2</summary>
			OS216 = 0x00020000,
			/// <summary>The file was designed for 32-bit OS/2</summary>
			OS232 = 0x00030000,
			/// <summary>The file was designed for 16-bit Presentation Manager</summary>
			PM16 = 0x00000002,
			/// <summary>The file was designed for 32-bit Presentation Manager</summary>
			PM32 = 0x00000003,
			/// <summary>The operating system for which the file was designed is unknown to the system</summary>
			UNKNOWN = 0x00000000,
			/// <summary>The file was designed for 16-bit Windows running on MS-DOS</summary>
			DOS_WINDOWS16 = 0x00010001,
			/// <summary>The file was designed for 32-bit Windows running on MS-DOS</summary>
			DOS_WINDOWS32 = 0x00010004,
			/// <summary>The file was designed for Windows NT</summary>
			NT_WINDOWS32 = 0x00040004,
			/// <summary>The file was designed for 16-bit Presentation Manager running on 16-bit OS/2</summary>
			OS216_PM16 = 0x00020002,
			/// <summary>The file was designed for 32-bit Presentation Manager running on 32-bit OS/2</summary>
			OS232_PM32 = 0x00030003,
		}

		/// <summary>The general type of file</summary>
		[Flags]
		public enum VFT
		{
			/// <summary>The file contains an application</summary>
			APP = 0x00000001,
			/// <summary>The file contains a DLL</summary>
			DLL = 0x00000002,
			/// <summary>The file contains a device driver</summary>
			/// <remarks>If dwFileType is VFT_DRV, dwFileSubtype contains a more specific description of the driver</remarks>
			DRV = 0x00000003,
			/// <summary>The file contains a font</summary>
			/// <remarks>If dwFileType is VFT_FONT, dwFileSubtype contains a more specific description of the font file</remarks>
			FONT = 0x00000004,
			/// <summary>The file contains a static-link library</summary>
			STATIC_LIB = 0x00000007,
			/// <summary>The file type is unknown to the system</summary>
			UNKNOWN = 0x00000000,
			/// <summary>The file contains a virtual device</summary>
			VXD = 0x00000005,
		}

		/// <summary>The function of the file</summary>
		/// <remarks>
		/// The possible values depend on the value of dwFileType.
		/// For all values of dwFileType not described in the following list, dwFileSubtype is zero
		/// </remarks>
		[Flags]
		public enum VFT2
		{
			/// <summary>The file contains a communications driver</summary>
			DRV_COMM = 0x0000000A,
			/// <summary>The file contains a display driver</summary>
			DRV_DISPLAY = 0x00000004,
			/// <summary>The file contains an installable driver</summary>
			DRV_INSTALLABLE = 0x00000008,
			/// <summary>The file contains a keyboard driver</summary>
			DRV_KEYBOARD = 0x00000002,
			/// <summary>The file contains a language driver</summary>
			DRV_LANGUAGE = 0x00000003,
			/// <summary>The file contains a mouse driver</summary>
			DRV_MOUSE = 0x00000005,
			/// <summary>The file contains a network driver</summary>
			DRV_NETWORK = 0x00000006,
			/// <summary>The file contains a printer driver</summary>
			DRV_PRINTER = 0x00000001,
			/// <summary>The file contains a sound driver</summary>
			DRV_SOUND = 0x00000009,
			/// <summary>The file contains a system driver</summary>
			DRV_SYSTEM = 0x00000007,
			/// <summary>The file contains a versioned printer driver</summary>
			DRV_VERSIONED_PRINTER = 0x0000000C,
			/// <summary>The driver type is unknown by the system</summary>
			UNKNOWN = 0x00000000,
		}
	}
}