using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Fields, properties and other members signatures</summary>
	[Flags]
	public enum CorSignature
	{
		/// <summary>banana</summary>
		DEFAULT = 0x0,
		/// <summary>banana</summary>
		C = 0x1,
		/// <summary>banana</summary>
		STDCALL = 0x2,
		/// <summary>banana</summary>
		THISCALL = 0x3,
		/// <summary>banana</summary>
		FASTCALL = 0x4,
		/// <summary>banana</summary>
		VARARG = 0x5,
		/// <summary>Field signature</summary>
		FIELD = 0x6,
		/// <summary>banana</summary>
		LOCAL_SIG = 0x7,
		/// <summary>Property signature</summary>
		PROPERTY = 0x8,
		/// <summary>Instance member</summary>
		HASTHIS = 0x20,
		/// <summary>banana</summary>
		EXPLICITTHIS = 0x40,
		/// <summary>banana</summary>
		SENTINEL = 0x41,
	}
}