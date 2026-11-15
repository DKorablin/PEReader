using System;
using System.ComponentModel;
using System.IO;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Item from .resources file</summary>
	[DefaultProperty(nameof(Name))]
	public class ResourceTableItem
	{
		private readonly ResourceTableReader _reader;
		private readonly UInt32 _offset;
		private readonly UInt32 _length;
		private Byte[] _data;

		/// <summary>Name of resource item</summary>
		public String Name { get; }

		/// <summary>Type code of the resource item</summary>
		/// <remarks>
		/// Values starting from <see cref="Cor.ResourceTypeCode.StartOfUserTypes"/> are serialized user types.
		/// (TypeCode - <see cref="Cor.ResourceTypeCode.StartOfUserTypes"/>) is the index in <see cref="ResourceTableReader.Types"/>.
		/// </remarks>
		public Cor.ResourceTypeCode TypeCode { get; }

		/// <summary>Full type name of the resource item or <c>null</c> for null values</summary>
		public String Type { get; }

		/// <summary>The value is stored as a primitive type, array of bytes or stream and can be decoded without deserialization</summary>
		public Boolean IsPrimitive => this.TypeCode < Cor.ResourceTypeCode.StartOfUserTypes;

		/// <summary>Raw data of resource item without type code</summary>
		/// <remarks>
		/// Raw data is in the same format as returned from <see cref="System.Resources.ResourceReader.GetResourceData"/>.
		/// Length prefixes for strings, arrays and streams are included.
		/// </remarks>
		public Byte[] Data
		{
			get
			{
				if(this._data == null)
				{
					Byte[] data = new Byte[this._length];
					Array.Copy(this._reader.Data, this._offset, data, 0, this._length);
					this._data = data;
				}
				return this._data;
			}
		}

		/// <summary>Decoded value of the resource item</summary>
		/// <remarks>
		/// <see cref="Cor.ResourceTypeCode.ByteArray"/> and <see cref="Cor.ResourceTypeCode.Stream"/> are returned as array of bytes.
		/// Serialized user types are not deserialized and <c>null</c> is returned, use <see cref="Data"/> to get the serialized data.
		/// </remarks>
		/// <exception cref="InvalidDataException">Resource item data is corrupted</exception>
		public Object Value
		{
			get
			{
				if(!this.IsPrimitive || this.TypeCode == Cor.ResourceTypeCode.Null)
					return null;

				try
				{
					using(ResourceBinaryReader reader = new ResourceBinaryReader(this.Data))
						switch(this.TypeCode)
						{
						case Cor.ResourceTypeCode.String: return reader.ReadString();
						case Cor.ResourceTypeCode.Boolean: return reader.ReadBoolean();
						case Cor.ResourceTypeCode.Char: return (Char)reader.ReadUInt16();
						case Cor.ResourceTypeCode.Byte: return reader.ReadByte();
						case Cor.ResourceTypeCode.SByte: return reader.ReadSByte();
						case Cor.ResourceTypeCode.Int16: return reader.ReadInt16();
						case Cor.ResourceTypeCode.UInt16: return reader.ReadUInt16();
						case Cor.ResourceTypeCode.Int32: return reader.ReadInt32();
						case Cor.ResourceTypeCode.UInt32: return reader.ReadUInt32();
						case Cor.ResourceTypeCode.Int64: return reader.ReadInt64();
						case Cor.ResourceTypeCode.UInt64: return reader.ReadUInt64();
						case Cor.ResourceTypeCode.Single: return reader.ReadSingle();
						case Cor.ResourceTypeCode.Double: return reader.ReadDouble();
						case Cor.ResourceTypeCode.Decimal: return reader.ReadDecimal();
						case Cor.ResourceTypeCode.DateTime: return DateTime.FromBinary(reader.ReadInt64());
						case Cor.ResourceTypeCode.TimeSpan: return new TimeSpan(reader.ReadInt64());
						case Cor.ResourceTypeCode.ByteArray:
						case Cor.ResourceTypeCode.Stream:
							Int32 length = reader.ReadInt32();
							if(length < 0 || length > reader.BaseStream.Length - reader.Position)
								throw new EndOfStreamException($"Invalid length {length}");
							return reader.ReadBytes(length);
						default:
							throw new InvalidDataException($"Unknown type code 0x{(Int32)this.TypeCode:X}");
						}
				} catch(IOException exc)//Includes EndOfStreamException
				{
					throw new InvalidDataException($"Resource item {this.Name} is corrupted", exc);
				} catch(FormatException exc)//Invalid 7-bit encoded string length
				{
					throw new InvalidDataException($"Resource item {this.Name} is corrupted", exc);
				} catch(ArgumentException exc)//Invalid decimal or DateTime value
				{
					throw new InvalidDataException($"Resource item {this.Name} is corrupted", exc);
				}
			}
		}

		/// <summary>Create instance of a resource item</summary>
		/// <param name="reader">Embedded resource that contains this item</param>
		/// <param name="name">Resource name in the .resources file</param>
		/// <param name="typeCode">Type code of the resource item</param>
		/// <param name="type">Full type name of the resource item</param>
		/// <param name="offset">Offset of the item data from the beginning of the .resources file</param>
		/// <param name="length">Length of the item data</param>
		internal ResourceTableItem(ResourceTableReader reader, String name, Cor.ResourceTypeCode typeCode, String type, UInt32 offset, UInt32 length)
		{
			this._reader = reader ?? throw new ArgumentNullException(nameof(reader));
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
			this.TypeCode = typeCode;
			this.Type = type;
			this._offset = offset;
			this._length = length;
		}

		/// <summary>String representation of the resource item</summary>
		/// <returns>Name and type of the resource item</returns>
		public override String ToString()
			=> $"{this.Name} ({this.Type ?? "null"})";

		/// <summary>Get type name for primitive type code</summary>
		/// <param name="typeCode">Primitive type code</param>
		/// <returns>Full type name or <c>null</c> if type code is unknown</returns>
		internal static String GetPrimitiveTypeName(Cor.ResourceTypeCode typeCode)
		{
			switch(typeCode)
			{
			case Cor.ResourceTypeCode.Null: return null;
			case Cor.ResourceTypeCode.String: return typeof(String).FullName;
			case Cor.ResourceTypeCode.Boolean: return typeof(Boolean).FullName;
			case Cor.ResourceTypeCode.Char: return typeof(Char).FullName;
			case Cor.ResourceTypeCode.Byte: return typeof(Byte).FullName;
			case Cor.ResourceTypeCode.SByte: return typeof(SByte).FullName;
			case Cor.ResourceTypeCode.Int16: return typeof(Int16).FullName;
			case Cor.ResourceTypeCode.UInt16: return typeof(UInt16).FullName;
			case Cor.ResourceTypeCode.Int32: return typeof(Int32).FullName;
			case Cor.ResourceTypeCode.UInt32: return typeof(UInt32).FullName;
			case Cor.ResourceTypeCode.Int64: return typeof(Int64).FullName;
			case Cor.ResourceTypeCode.UInt64: return typeof(UInt64).FullName;
			case Cor.ResourceTypeCode.Single: return typeof(Single).FullName;
			case Cor.ResourceTypeCode.Double: return typeof(Double).FullName;
			case Cor.ResourceTypeCode.Decimal: return typeof(Decimal).FullName;
			case Cor.ResourceTypeCode.DateTime: return typeof(DateTime).FullName;
			case Cor.ResourceTypeCode.TimeSpan: return typeof(TimeSpan).FullName;
			case Cor.ResourceTypeCode.ByteArray: return typeof(Byte[]).FullName;
			case Cor.ResourceTypeCode.Stream: return typeof(Stream).FullName;
			default: return null;
			}
		}
	}
}