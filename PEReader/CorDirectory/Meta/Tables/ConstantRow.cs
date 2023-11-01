using System;
using System.Text;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The Constant table is used to store compile-time, constant values for fields, parameters, and properties</summary>
	public class ConstantRow : BaseMetaRow
	{
		/// <summary>A 1-byte constant, followed by a 1-byte padding zero</summary>
		/// <remarks>
		/// The encoding of Type for the nullref value for FieldInit in ilasm (§II.16.2)
		/// is <see cref="Cor.ELEMENT_TYPE.CLASS"/> with a Value of a 4-byte zero.
		/// Unlike uses of <see cref="Cor.ELEMENT_TYPE.CLASS"/> in signatures, this one is not followed by a type token
		/// </remarks>
		public UInt16 Type { get { return base.GetValue<UInt16>(0); } }

		/// <summary>Strongly typed type desctiption</summary>
		public TypeCode TypeCode
		{
			get
			{
				switch(this.Type)
				{
				case 2://Boolean
					return TypeCode.Boolean;
				case 3://Char
					return TypeCode.Char;
				case 4://SByte
					return TypeCode.SByte;
				case 5://Byte
					return TypeCode.Byte;
				case 6://Int16
					return TypeCode.Int16;
				case 7://UInt16
					return TypeCode.UInt16;
				case 8://Int32
					return TypeCode.Int32;
				case 9://UInt32
					return TypeCode.UInt32;
				case 10://Int64
					return TypeCode.Int64;
				case 11://UInt64
					return TypeCode.UInt64;
				case 12://Single
					return TypeCode.Single;
				case 13://Double
					return TypeCode.Double;
				case 14://String
					return TypeCode.String;
				case 18://Null
					return TypeCode.Empty;
				default://Unknown
					throw new NotSupportedException();
				}
			}
		}

		/// <summary>
		/// An index into the Param, Field, or Property table;
		/// more precisely, a HasConstant (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken Parent { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>Constant value</summary>
		public Byte[] Value { get { return base.GetValue<Byte[]>(2); } }

		/// <summary>Constant value converted to object</summary>
		public Object ValueTyped
		{
			get
			{
				switch(this.TypeCode)
				{
				case TypeCode.Boolean://Boolean
					return BitConverter.ToBoolean(this.Value, 0);
				case TypeCode.Char://Char
					return BitConverter.ToChar(this.Value, 0);
				case TypeCode.SByte://SByte
					return (SByte)this.Value[0];
				case TypeCode.Byte://Byte
					return this.Value[0];
				case TypeCode.Int16://Int16
					return BitConverter.ToInt16(this.Value, 0);
				case TypeCode.UInt16://UInt16
					return BitConverter.ToUInt16(this.Value, 0);
				case TypeCode.Int32://Int32
					return BitConverter.ToInt32(this.Value, 0);
				case TypeCode.UInt32://UInt32
					return BitConverter.ToUInt32(this.Value, 0);
				case TypeCode.Int64://Int64
					return BitConverter.ToInt64(this.Value, 0);
				case TypeCode.UInt64://UInt64
					return BitConverter.ToUInt64(this.Value, 0);
				case TypeCode.Single://Single
					return BitConverter.ToSingle(this.Value, 0);
				case TypeCode.Double://Double
					return BitConverter.ToDouble(this.Value, 0);
				case TypeCode.String://String
					return UTF8Encoding.Unicode.GetString(this.Value);
				case TypeCode.Empty://Null
					return null;
				default://Unknown
					throw new NotSupportedException();
				}
			}
		}
	}
}