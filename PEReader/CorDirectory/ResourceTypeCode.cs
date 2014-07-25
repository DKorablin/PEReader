using System;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Default resource type codes</summary>
	/// <remarks>Cloe of internal visible <see cref="System.Resources.ResourceTypeCode"/></remarks>
	public enum ResourceTypeCode
	{
		/// <summary>NULL type</summary>
		Null = 0,
		/// <summary>String</summary>
		String = 1,
		/// <summary>Boolean</summary>
		Boolean = 2,
		/// <summary>Char</summary>
		Char = 3,
		/// <summary>Byte</summary>
		Byte = 4,
		/// <summary>Single byte</summary>
		SByte = 5,
		/// <summary>Int16</summary>
		Int16 = 6,
		/// <summary>Unsigned Int16</summary>
		UInt16 = 7,
		/// <summary>Int32</summary>
		Int32 = 8,
		/// <summary>Insigned Int32</summary>
		UInt32 = 9,
		/// <summary>Int64</summary>
		Int64 = 10,
		/// <summary>Unsigned Int64</summary>
		UInt64 = 11,
		/// <summary>Single</summary>
		Single = 12,
		/// <summary>Double</summary>
		Double = 13,
		/// <summary>Decimal</summary>
		Decimal = 14,
		/// <summary>DateTime</summary>
		DateTime = 15,
		/// <summary>TimeSpan</summary>
		TimeSpan = 16,
		/// <summary>Last primitive</summary>
		LastPrimitive = 16,
		/// <summary>Byte array</summary>
		ByteArray = 32,
		/// <summary>Steam</summary>
		Stream = 33,
		/// <summary>User type</summary>
		StartOfUserTypes = 64,
	}
}