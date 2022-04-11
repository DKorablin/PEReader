using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Variable utils</summary>
	public struct EndianHelper
	{
		private static readonly Dictionary<TypeCode, Int32> SizeTypes = new Dictionary<TypeCode, Int32>()
		{
			{ TypeCode.Boolean, sizeof(Boolean) },
			{ TypeCode.Byte, sizeof(Byte) },
			{ TypeCode.Char, sizeof(Char) },
			{ TypeCode.Decimal, sizeof(Decimal) },
			{ TypeCode.Double, sizeof(Double) },
			{ TypeCode.Int16, sizeof(Int16) },
			{ TypeCode.Int32, sizeof(Int32) },
			{ TypeCode.Int64, sizeof(Int64) },
			{ TypeCode.SByte, sizeof(SByte) },
			{ TypeCode.Single, sizeof(Single) },
			{ TypeCode.UInt16, sizeof(UInt16) },
			{ TypeCode.UInt32, sizeof(UInt32) },
			{ TypeCode.UInt64, sizeof(UInt64) },
		};

		/// <summary>Endianness, big or little</summary>
		public enum Endian
		{
			/// <summary>BigEndian</summary>
			Big,
			/// <summary>LittleEndian</summary>
			Little,
		}

		/// <summary>Get system default endiannesss</summary>
		public static EndianHelper.Endian Endianness { get { return BitConverter.IsLittleEndian ? Endian.Little : Endian.Big; } }

		internal static void AdjustEndianness(Type type, Byte[] data, EndianHelper.Endian endian, Int32 startOffset = 0)
		{
			if(EndianHelper.Endianness == endian)
				return;

			TypeCode code = Type.GetTypeCode(type);

			switch(code)
			{
			default:
				Array.Reverse(data, startOffset, EndianHelper.SizeTypes[code]);
				break;
			case TypeCode.String:
				return;//Ignore strings
			case TypeCode.Object:
				foreach(FieldInfo field in type.GetFields())
				{
					Type fieldType = field.FieldType;
					if(field.IsStatic)// Ignore static fields
						continue;
					/*else if(fieldType.IsArray)// array fields
					{
						//handle arrays, assuming fixed length
						MarshalAsAttribute[] attr = (MarshalAsAttribute[])fieldType.GetCustomAttributes(typeof(MarshalAsAttribute), false);
						if(attr.Length == 0 || attr[0].SizeConst == 0)
							throw new NotSupportedException("Array fields must be decorated with a MarshalAsAttribute with SizeConst specified.");

						var arrayLength = attr[0].SizeConst;
						var elementType = fieldType.GetElementType();
						var elementSize = Marshal.SizeOf(elementType);
						var arrayOffset = Marshal.OffsetOf(type, field.Name).ToInt32();

						for(int i = arrayOffset; i < arrayOffset + elementSize * arrayLength; i += elementSize)
						{
							MaybeAdjustEndianness(elementType, data, endianness, i);
						}
					}*/ else
					{
						Int32 offset = Marshal.OffsetOf(type, field.Name).ToInt32();

						Int32 fieldOffset = startOffset + offset;

						EndianHelper.AdjustEndianness(fieldType, data, endian, fieldOffset);
					}
				}
				break;
			}
		}
	}
}