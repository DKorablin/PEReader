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
		public Cor.ELEMENT_TYPE Type => (Cor.ELEMENT_TYPE)base.GetValue<UInt16>(0);

		/// <summary>
		/// An index into the Param, Field, or Property table;
		/// more precisely, a HasConstant (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken Parent => base.GetValue<MetaCellCodedToken>(1);

		/// <summary>Constant value</summary>
		public Byte[] Value => base.GetValue<Byte[]>(2);

		/// <summary>Constant value converted to object</summary>
		public Object ValueTyped
		{
			get
			{
				switch(this.Type)
				{
				case Cor.ELEMENT_TYPE.BOOLEAN://Boolean
					return BitConverter.ToBoolean(this.Value, 0);
				case Cor.ELEMENT_TYPE.CHAR://Char
					return BitConverter.ToChar(this.Value, 0);
				case Cor.ELEMENT_TYPE.I1://SByte
					return (SByte)this.Value[0];
				case Cor.ELEMENT_TYPE.U1://Byte
					return this.Value[0];
				case Cor.ELEMENT_TYPE.I2://Int16
					return BitConverter.ToInt16(this.Value, 0);
				case Cor.ELEMENT_TYPE.U2://UInt16
					return BitConverter.ToUInt16(this.Value, 0);
				case Cor.ELEMENT_TYPE.I4://Int32
					return BitConverter.ToInt32(this.Value, 0);
				case Cor.ELEMENT_TYPE.U4://UInt32
					return BitConverter.ToUInt32(this.Value, 0);
				case Cor.ELEMENT_TYPE.I8://Int64
					return BitConverter.ToInt64(this.Value, 0);
				case Cor.ELEMENT_TYPE.U8://UInt64
					return BitConverter.ToUInt64(this.Value, 0);
				case Cor.ELEMENT_TYPE.R4://Single
					return BitConverter.ToSingle(this.Value, 0);
				case Cor.ELEMENT_TYPE.R8://Double
					return BitConverter.ToDouble(this.Value, 0);
				case Cor.ELEMENT_TYPE.STRING://String
					return Encoding.Unicode.GetString(this.Value);
				case Cor.ELEMENT_TYPE.CLASS://Null
					return null;
				default://Unknown
					throw new NotSupportedException();
				}
			}
		}

		/// <summary>Create instance of Constant value row</summary>
		public ConstantRow()
			: base(Cor.MetaTableType.Constant) { }
	}
}