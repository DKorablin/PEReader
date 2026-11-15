using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Embedded managed resource</summary>
	/// <remarks>
	/// Embedded resource can contain any data. If it's a .resources file (see <see cref="CanRead"/>),
	/// then it can be enumerated to get all <see cref="ResourceTableItem"/> elements.
	/// The .resources format is parsed without <see cref="System.Resources.ResourceReader"/>, so values are never deserialized
	/// and files written by any resource writer (Including System.Resources.Extensions) can be read.
	/// </remarks>
	[DefaultProperty(nameof(Name))]
	public class ResourceTableReader : IEnumerable<ResourceTableItem>
	{
		/// <summary>Version 1 type name to type code mapping. In version 1 primitive types are stored without serialization</summary>
		private static readonly Dictionary<String, Cor.ResourceTypeCode> V1PrimitiveTypes = new Dictionary<String, Cor.ResourceTypeCode>()
		{
			{ "System.String", Cor.ResourceTypeCode.String },
			{ "System.Int32", Cor.ResourceTypeCode.Int32 },
			{ "System.Byte", Cor.ResourceTypeCode.Byte },
			{ "System.SByte", Cor.ResourceTypeCode.SByte },
			{ "System.Int16", Cor.ResourceTypeCode.Int16 },
			{ "System.Int64", Cor.ResourceTypeCode.Int64 },
			{ "System.UInt16", Cor.ResourceTypeCode.UInt16 },
			{ "System.UInt32", Cor.ResourceTypeCode.UInt32 },
			{ "System.UInt64", Cor.ResourceTypeCode.UInt64 },
			{ "System.Single", Cor.ResourceTypeCode.Single },
			{ "System.Double", Cor.ResourceTypeCode.Double },
			{ "System.DateTime", Cor.ResourceTypeCode.DateTime },
			{ "System.TimeSpan", Cor.ResourceTypeCode.TimeSpan },
			{ "System.Decimal", Cor.ResourceTypeCode.Decimal },
		};

		private Byte[] _data;
		private ResourceSet _set;

		/// <summary>ManifestResource metadata row that describes this resource</summary>
		public ManifestResourceRow Row { get; }

		/// <summary>Resource name</summary>
		public String Name => this.Row.Name;

		/// <summary>Resource visibility flags</summary>
		public ResourceAttributes Flags => this.Row.Flags;

		/// <summary>Contents of the embedded resource</summary>
		/// <exception cref="InvalidDataException">Resource is located outside of the managed resources directory</exception>
		public Byte[] Data => this._data ?? (this._data = this.Row.File);

		/// <summary>Resource is a .resources file and can be enumerated</summary>
		public Boolean CanRead
		{
			get
			{
				Byte[] data = this.Data;
				return data.Length >= Marshal.SizeOf(typeof(Cor.ResourceManagerHeader))
					&& BitConverter.ToUInt32(data, 0) == Cor.ResourceManagerHeader.MagicNumberConst;
			}
		}

		/// <summary>Resource manager header of the .resources file</summary>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		public Cor.ResourceManagerHeader Header => this.GetResourceSet().Header;

		/// <summary>Name of the type that is used to read this .resources file</summary>
		/// <remarks><c>null</c> if <see cref="Cor.ResourceManagerHeader.HeaderVersionNumber"/> is greater than 1</remarks>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		public String ReaderType => this.GetResourceSet().ReaderType;

		/// <summary>Name of the resource set type that is used to read this .resources file</summary>
		/// <remarks><c>null</c> if <see cref="Cor.ResourceManagerHeader.HeaderVersionNumber"/> is greater than 1</remarks>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		public String ResourceSetType => this.GetResourceSet().ResourceSetType;

		/// <summary>Runtime resource set header of the .resources file</summary>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		public Cor.ResourceSetHeader RuntimeHeader => this.GetResourceSet().RuntimeHeader;

		/// <summary>Type names of all non primitive values stored in .resources file</summary>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		public ReadOnlyCollection<String> Types => this.GetResourceSet().TypesReadOnly;

		/// <summary>Get item from .resources file by name</summary>
		/// <param name="name">Item name</param>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		/// <returns>Resource item or <c>null</c> if item is not found</returns>
		public ResourceTableItem this[String name]
		{
			get
			{
				foreach(ResourceTableItem item in this.GetResourceSet().Items)
					if(item.Name == name)
						return item;
				return null;
			}
		}

		internal ResourceTableReader(ManifestResourceRow row)
			=> this.Row = row ?? throw new ArgumentNullException(nameof(row));

		/// <summary>Reads all elements from .resources file</summary>
		/// <exception cref="InvalidOperationException">Resource is not a .resources file</exception>
		/// <exception cref="InvalidDataException">.resources file is corrupted</exception>
		/// <returns>Resource items</returns>
		public IEnumerator<ResourceTableItem> GetEnumerator()
		{
			foreach(ResourceTableItem item in this.GetResourceSet().Items)
				yield return item;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>String representation of the resource</summary>
		/// <returns>Resource name</returns>
		public override String ToString()
			=> this.Name;

		private ResourceSet GetResourceSet()
		{
			if(this._set == null)
			{
				if(!this.CanRead)
					throw new InvalidOperationException($"Resource {this.Name} is not a .resources file");

				try
				{
					this._set = this.ParseResourceSet();
				} catch(IOException exc)//Includes EndOfStreamException
				{
					throw new InvalidDataException($"Resource {this.Name} is corrupted", exc);
				} catch(FormatException exc)//Invalid 7-bit encoded integer
				{
					throw new InvalidDataException($"Resource {this.Name} is corrupted", exc);
				} catch(ArgumentException exc)//Invalid string encoding or invalid seek position
				{
					throw new InvalidDataException($"Resource {this.Name} is corrupted", exc);
				}
			}
			return this._set;
		}

		/// <summary>Parse .resources file</summary>
		/// <remarks>
		/// Format (see System.Resources.ResourceWriter):
		/// ResourceManagerHeader, [ReaderType, ResourceSetType], ResourceSetHeader, Types[], PAD to 8 bytes,
		/// NameHashes[], NamePositions[], DataSectionOffset,
		/// Name section: { Name (UTF-16), DataOffset },
		/// Data section: { TypeCode or TypeIndex (7-bit encoded), Value }
		/// </remarks>
		private ResourceSet ParseResourceSet()
		{
			Byte[] data = this.Data;
			ResourceSet result = new ResourceSet();

			using(ResourceBinaryReader reader = new ResourceBinaryReader(data))
			{
				result.Header.MagicNumber = reader.ReadUInt32();
				result.Header.HeaderVersionNumber = reader.ReadUInt32();
				result.Header.SizeOfReaderType = reader.ReadUInt32();

				if(result.Header.HeaderVersionNumber == 1)
				{
					result.ReaderType = reader.ReadString();
					result.ResourceSetType = reader.ReadString();
				} else
					reader.Seek(reader.Position + result.Header.SizeOfReaderType);

				result.RuntimeHeader.Version = reader.ReadUInt32();
				result.RuntimeHeader.NumberOfResources = reader.ReadUInt32();
				result.RuntimeHeader.NumberOfTypes = reader.ReadUInt32();

				UInt32 version = result.RuntimeHeader.Version;
				if(version != 1 && version != 2)
					throw new InvalidDataException($"Resource {this.Name} has unsupported resource set version {version}");

				//Each type name and resource uses at least 1 byte, so this is a cheap sanity check against huge allocations
				UInt32 numberOfTypes = result.RuntimeHeader.NumberOfTypes;
				UInt32 numberOfResources = result.RuntimeHeader.NumberOfResources;
				if(numberOfTypes > data.Length || numberOfResources > data.Length / 8)
					throw new InvalidDataException($"Resource {this.Name} has invalid number of types ({numberOfTypes}) or resources ({numberOfResources})");

				result.Types = new String[numberOfTypes];
				for(UInt32 loop = 0; loop < numberOfTypes; loop++)
					result.Types[loop] = reader.ReadString();

				//Name hashes are aligned to 8 bytes with "PAD" bytes
				Int64 alignBytes = reader.Position & 7;
				if(alignBytes != 0)
					reader.Seek(reader.Position + 8 - alignBytes);

				reader.Seek(reader.Position + numberOfResources * sizeof(Int32));//Skip name hashes

				Int32[] namePositions = new Int32[numberOfResources];
				for(UInt32 loop = 0; loop < numberOfResources; loop++)
					namePositions[loop] = reader.ReadInt32();

				Int64 dataSectionOffset = reader.ReadInt32();
				Int64 nameSectionOffset = reader.Position;
				if(dataSectionOffset < nameSectionOffset || dataSectionOffset > data.Length)
					throw new InvalidDataException($"Resource {this.Name} has invalid data section offset 0x{dataSectionOffset:X}");

				String[] names = new String[numberOfResources];
				Int64[] dataPositions = new Int64[numberOfResources];
				for(UInt32 loop = 0; loop < numberOfResources; loop++)
				{
					Int64 namePosition = nameSectionOffset + namePositions[loop];
					if(namePositions[loop] < 0 || namePosition >= dataSectionOffset)
						throw new InvalidDataException($"Resource {this.Name} has invalid name position 0x{namePositions[loop]:X}");

					reader.Seek(namePosition);
					names[loop] = reader.ReadUnicodeString();

					Int32 dataOffset = reader.ReadInt32();
					dataPositions[loop] = dataSectionOffset + dataOffset;
					if(dataOffset < 0 || dataPositions[loop] >= data.Length)
						throw new InvalidDataException($"Resource {this.Name} item {names[loop]} has invalid data offset 0x{dataOffset:X}");
				}

				//Data length is not stored in the file. It's calculated as a distance to the next data element
				Int64[] sortedPositions = (Int64[])dataPositions.Clone();
				Array.Sort(sortedPositions);

				result.Items = new ResourceTableItem[numberOfResources];
				for(UInt32 loop = 0; loop < numberOfResources; loop++)
				{
					Int64 dataPosition = dataPositions[loop];
					Int32 nextIndex = Array.BinarySearch(sortedPositions, dataPosition);
					while(nextIndex < sortedPositions.Length && sortedPositions[nextIndex] == dataPosition)
						nextIndex++;
					Int64 dataEnd = nextIndex < sortedPositions.Length ? sortedPositions[nextIndex] : data.Length;

					reader.Seek(dataPosition);
					Int32 typeCode = reader.Read7BitEncodedInt32();

					Cor.ResourceTypeCode resourceTypeCode;
					String typeName;
					if(version == 1)
						this.GetTypeV1(result.Types, typeCode, out resourceTypeCode, out typeName);
					else
						this.GetTypeV2(result.Types, typeCode, out resourceTypeCode, out typeName);

					Int64 valuePosition = reader.Position;
					if(valuePosition > dataEnd)
						throw new InvalidDataException($"Resource {this.Name} item {names[loop]} has invalid data length");

					result.Items[loop] = new ResourceTableItem(this, names[loop], resourceTypeCode, typeName, (UInt32)valuePosition, (UInt32)(dataEnd - valuePosition));
				}
			}

			return result;
		}

		private void GetTypeV1(String[] types, Int32 typeIndex, out Cor.ResourceTypeCode typeCode, out String typeName)
		{
			if(typeIndex == -1)
			{
				typeCode = Cor.ResourceTypeCode.Null;
				typeName = null;
				return;
			}

			if(typeIndex < 0 || typeIndex >= types.Length)
				throw new InvalidDataException($"Resource {this.Name} has invalid type index {typeIndex}");

			typeName = types[typeIndex];
			Int32 separator = typeName.IndexOf(',');
			String shortTypeName = separator < 0 ? typeName : typeName.Substring(0, separator);
			if(!V1PrimitiveTypes.TryGetValue(shortTypeName, out typeCode))
				typeCode = Cor.ResourceTypeCode.StartOfUserTypes + typeIndex;
		}

		private void GetTypeV2(String[] types, Int32 code, out Cor.ResourceTypeCode typeCode, out String typeName)
		{
			typeCode = (Cor.ResourceTypeCode)code;
			if(code >= (Int32)Cor.ResourceTypeCode.StartOfUserTypes)
			{
				Int32 typeIndex = code - (Int32)Cor.ResourceTypeCode.StartOfUserTypes;
				if(typeIndex >= types.Length)
					throw new InvalidDataException($"Resource {this.Name} has invalid type index {typeIndex}");
				typeName = types[typeIndex];
			} else if(typeCode == Cor.ResourceTypeCode.Null)
				typeName = null;
			else
				typeName = ResourceTableItem.GetPrimitiveTypeName(typeCode)
					?? throw new InvalidDataException($"Resource {this.Name} has unknown type code 0x{code:X}");
		}

		/// <summary>Parsed .resources file</summary>
		private sealed class ResourceSet
		{
			public Cor.ResourceManagerHeader Header;
			public Cor.ResourceSetHeader RuntimeHeader;
			public String ReaderType;
			public String ResourceSetType;
			public String[] Types;
			public ResourceTableItem[] Items;
			private ReadOnlyCollection<String> _typesReadOnly;
			public ReadOnlyCollection<String> TypesReadOnly
				=> this._typesReadOnly ?? (this._typesReadOnly = new ReadOnlyCollection<String>(this.Types));
		}
	}

	/// <summary>Binary reader with access to 7-bit encoded integers and length prefixed UTF-16 strings</summary>
	internal sealed class ResourceBinaryReader : BinaryReader
	{
		public Int64 Position => this.BaseStream.Position;

		public ResourceBinaryReader(Byte[] data)
			: base(new MemoryStream(data, false), Encoding.UTF8) { }

		/// <summary>Move to absolute position in the stream</summary>
		/// <param name="position">Absolute position</param>
		/// <exception cref="EndOfStreamException">Position is outside of the stream</exception>
		public void Seek(Int64 position)
		{
			if(position < 0 || position > this.BaseStream.Length)
				throw new EndOfStreamException($"Position 0x{position:X} is outside of the stream (Length: 0x{this.BaseStream.Length:X})");
			this.BaseStream.Position = position;
		}

		public Int32 Read7BitEncodedInt32()
			=> base.Read7BitEncodedInt();

		/// <summary>Read string where 7-bit encoded length is in bytes and string is stored in UTF-16</summary>
		/// <returns>String</returns>
		public String ReadUnicodeString()
		{
			Int32 length = base.Read7BitEncodedInt();
			if(length < 0 || length > this.BaseStream.Length - this.BaseStream.Position)
				throw new EndOfStreamException($"Invalid string length {length}");

			Byte[] bytes = base.ReadBytes(length);
			return Encoding.Unicode.GetString(bytes);
		}
	}
}