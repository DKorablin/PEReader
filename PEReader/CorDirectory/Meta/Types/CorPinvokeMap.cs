using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Specifies options for a PInvoke call</summary>
	/// <remarks>http://msdn.microsoft.com/en-us/library/ms233461.aspx</remarks>
	[Flags]
	public enum CorPinvokeMap : UInt16
	{
		/// <summary>Use each member name as specified</summary>
		pmNoMangle = 0x0001,
		/// <summary>Reserved</summary>
		pmCharSetMask = 0x0006,
		/// <summary>Reserved</summary>
		pmCharSetNotSpec = 0x0000,
		/// <summary>Marshal strings as multiple-byte character strings</summary>
		pmCharSetAnsi = 0x0002,
		/// <summary>Marshal strings as Unicode 2-byte characters</summary>
		pmCharSetUnicode = 0x0004,
		/// <summary>Automatically marshal strings appropriately for the target operating system</summary>
		/// <remarks>
		/// The default is Unicode on Windows NT, Windows 2000, Windows XP, and the Windows Server 2003 family;
		/// the default is ANSI on Windows 98 and Windows ME
		/// </remarks>
		pmCharSetAuto = 0x0006,

		/// <summary>Reserved</summary>
		pmBestFitUseAssem = 0x0000,
		/// <summary>Perform best-fit mapping of Unicode characters that lack an exact match in the ANSI character set</summary>
		pmBestFitEnabled = 0x0010,
		/// <summary>Do not perform best-fit mapping of Unicode characters. In this case, all unmappable characters will be replaced by a ‘?’</summary>
		pmBestFitDisabled = 0x0020,
		/// <summary>Reserved</summary>
		pmBestFitMask = 0x0030,

		/// <summary>Reserved</summary>
		pmThrowOnUnmappableCharUseAssem = 0x0000,
		/// <summary>Throw an exception when the interop marshaler encounters an unmappable character</summary>
		pmThrowOnUnmappableCharEnabled = 0x1000,
		/// <summary>Do not throw an exception when the interop marshaler encounters an unmappable character</summary>
		pmThrowOnUnmappableCharDisabled = 0x2000,
		/// <summary>Reserved</summary>
		pmThrowOnUnmappableCharMask = 0x3000,

		/// <summary>Allow the callee to call the Win32 SetLastError function before returning from the attributed method</summary>
		pmSupportsLastError = 0x0040,

		/// <summary>Reserved</summary>
		pmCallConvMask = 0x0700,
		/// <summary>Use the default platform calling convention</summary>
		/// <remarks>For example, on Windows the default is StdCall and on Windows CE .NET it is Cdecl</remarks>
		pmCallConvWinapi = 0x0100,
		/// <summary>
		/// Use the Cdecl calling convention.
		/// In this case, the caller cleans the stack.
		/// This enables calling functions with varargs (that is, functions that accept a variable number of parameters)
		/// </summary>
		pmCallConvCdecl = 0x0200,
		/// <summary>
		/// Use the StdCall calling convention.
		/// In this case, the callee cleans the stack.
		/// This is the default convention for calling unmanaged functions with platform invoke
		/// </summary>
		pmCallConvStdcall = 0x0300,
		/// <summary>
		/// Use the ThisCall calling convention.
		/// In this case, the first parameter is the this pointer and is stored in register ECX.
		/// Other parameters are pushed on the stack.
		/// The ThisCall calling convention is used to call methods on classes exported from an unmanaged DLL
		/// </summary>
		pmCallConvThiscall = 0x0400,
		/// <summary>Reserved</summary>
		pmCallConvFastcall = 0x0500,
		/// <summary>Reserved</summary>
		pmMaxValue = 0xFFFF,
	}
}