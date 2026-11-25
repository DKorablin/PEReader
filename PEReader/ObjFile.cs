using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>OBJ/COFF file reader</summary>
	/// <remarks>OBJ files are never mapped into memory and have no RVAs, so every offset used by this class is a raw file offset</remarks>
	public class ObjFile : IDisposable
	{
		private static readonly UInt32 SizeOfFileHeader = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_FILE_HEADER));
		private static readonly UInt32 SizeOfSectionHeader = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_SECTION_HEADER));
		/// <summary>Size of a symbol table record. Auxiliary records have the same size as standard records</summary>
		private static readonly UInt32 SizeOfSymbol = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_COFF_SYMBOL));

		/// <summary>Base64 alphabet used for "//nnnnnn" long section names</summary>
		private const String SectionNameBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		private WinNT.IMAGE_FILE_HEADER? _fileHeader;
		private WinNT.IMAGE_SECTION_HEADER[] _sections;
		private WinNT.IMAGE_COFF_SYMBOL[] _symbols;
		private Byte[] _stringTableData;
		private String[] _stringTable;

		private IImageLoader Loader { get; set; }

		/// <summary>OBJ File header structure</summary>
		public WinNT.IMAGE_FILE_HEADER FileHeader
			=> this._fileHeader ?? (this._fileHeader = this.Loader.PtrToStructure<WinNT.IMAGE_FILE_HEADER>(0)).Value;

		/// <summary>Checking if OBJ file is valid</summary>
		/// <remarks>
		/// Rejects unknown machine types, anonymous object headers (/bigobj, /GL) and short import objects (Machine=0, NumberOfSections=0xFFFF),
		/// and files whose section or symbol table does not fit inside the file.
		/// </remarks>
		public Boolean IsValid
		{
			get
			{
				WinNT.IMAGE_FILE_HEADER header = this.FileHeader;
				if(!header.IsValid)
					return false;

				// ANON_OBJECT_HEADER / IMPORT_OBJECT_HEADER: Sig1 = IMAGE_FILE_MACHINE_UNKNOWN, Sig2 = 0xFFFF
				if(header.Machine == WinNT.IMAGE_FILE_MACHINE.UNKNOWN && header.NumberOfSections == 0xFFFF)
					return false;

				Int64 length = this.GetImageLength();
				Int64 endOfSections = SizeOfFileHeader + (Int64)header.SizeOfOptionalHeader + (Int64)header.NumberOfSections * SizeOfSectionHeader;
				if(endOfSections > length)
					return false;

				if(header.ContainsSymbolTable)
				{
					Int64 endOfSymbols = header.PointerToSymbolTable + (Int64)header.NumberOfSymbols * SizeOfSymbol;
					if(endOfSymbols > length)
						return false;
				}

				return true;
			}
		}

		/// <summary>OBJ symbol table structure</summary>
		/// <remarks>
		/// The array is indexed by raw symbol table index, the same index used by relocations and auxiliary records.
		/// Slots occupied by auxiliary records are left as <c>default</c>; use <see cref="WinNT.IMAGE_COFF_SYMBOL.NumberOfAuxSymbols"/> of the preceding symbol to skip them.
		/// Returns an empty array if the file has no symbol table.
		/// </remarks>
		public WinNT.IMAGE_COFF_SYMBOL[] Symbols
		{
			get
			{
				if(this._symbols == null)
				{
					WinNT.IMAGE_FILE_HEADER header = this.FileHeader;
					UInt32 count = 0;
					if(header.ContainsSymbolTable)
					{// Do not trust NumberOfSymbols beyond what physically fits in the file
						Int64 available = (this.GetImageLength() - header.PointerToSymbolTable) / SizeOfSymbol;
						count = available < header.NumberOfSymbols
							? (UInt32)Math.Max(0, available)
							: header.NumberOfSymbols;
					}

					WinNT.IMAGE_COFF_SYMBOL[] symbols = new WinNT.IMAGE_COFF_SYMBOL[count];
					UInt32 offset = header.PointerToSymbolTable;
					for(Int32 loop = 0; loop < symbols.Length; loop++)
					{
						WinNT.IMAGE_COFF_SYMBOL symbol = this.Loader.PtrToStructure<WinNT.IMAGE_COFF_SYMBOL>(offset);
						symbols[loop] = symbol;

						// Skip auxiliary records, but never past the end of the table
						Int32 auxCount = Math.Min(symbol.NumberOfAuxSymbols, symbols.Length - loop - 1);
						loop += auxCount;
						offset += SizeOfSymbol * (UInt32)(1 + auxCount);
					}

					this._symbols = symbols;
				}
				return this._symbols;
			}
		}

		/// <summary>COFF sections</summary>
		public WinNT.IMAGE_SECTION_HEADER[] Sections
		{
			get
			{
				if(this._sections == null)
				{
					WinNT.IMAGE_FILE_HEADER header = this.FileHeader;
					WinNT.IMAGE_SECTION_HEADER[] sections = new WinNT.IMAGE_SECTION_HEADER[header.NumberOfSections];

					UInt32 offset = SizeOfFileHeader + header.SizeOfOptionalHeader;
					for(Int32 loop = 0; loop < sections.Length; loop++)
					{
						sections[loop] = this.Loader.PtrToStructure<WinNT.IMAGE_SECTION_HEADER>(offset);
						offset += SizeOfSectionHeader;
					}

					this._sections = sections;
				}
				return this._sections;
			}
		}

		/// <summary>All strings stored in the COFF string table, in file order</summary>
		/// <remarks>
		/// Symbols and sections reference the string table by byte offset, not by index in this array.
		/// Use <see cref="GetString"/>, <see cref="GetSymbolName"/> or <see cref="GetSectionName"/> to resolve those references.
		/// </remarks>
		public String[] StringTable
		{
			get
			{
				if(this._stringTable == null)
				{
					Byte[] data = this.StringTableData;
					List<String> result = new List<String>();

					Int32 offset = sizeof(UInt32);
					while(offset < data.Length)
					{
						result.Add(ReadString(data, offset, out Int32 end));
						offset = end + 1;/*+ 0x00*/
					}

					this._stringTable = result.ToArray();
				}

				return this._stringTable;
			}
		}

		/// <summary>Raw string table bytes, including the leading 4-byte size field, so that string table offsets can be used as indexes</summary>
		private Byte[] StringTableData
		{
			get
			{
				if(this._stringTableData == null)
				{
					Byte[] data = new Byte[0];
					WinNT.IMAGE_FILE_HEADER header = this.FileHeader;
					if(header.ContainsSymbolTable)
					{// The string table immediately follows the symbol table
						Int64 length = this.GetImageLength();
						Int64 start = header.PointerToSymbolTable + (Int64)header.NumberOfSymbols * SizeOfSymbol;
						if(start + sizeof(UInt32) <= length && start + sizeof(UInt32) <= UInt32.MaxValue)
						{
							UInt32 size = this.Loader.PtrToStructure<UInt32>((UInt32)start);
							if(start + size > length)
								size = (UInt32)(length - start);// Truncated file
							if(size > sizeof(UInt32))
								data = this.Loader.ReadBytes((UInt32)start, size);
						}
					}

					this._stringTableData = data;
				}
				return this._stringTableData;
			}
		}

		/// <summary>Create instance of OBJ file reader</summary>
		/// <param name="loader">Image loader</param>
		public ObjFile(IImageLoader loader)
		{
			this.Loader = loader ?? throw new ArgumentNullException(nameof(loader));
			this.Loader.Endianness = EndianHelper.Endian.Little;// COFF is always little-endian
		}

		/// <summary>Get string from the COFF string table</summary>
		/// <param name="offset">Byte offset from the beginning of the string table (the 4-byte size field is included, so the first string is at offset 4)</param>
		/// <returns>String at the specified offset or null if offset is out of the string table bounds</returns>
		public String GetString(UInt32 offset)
		{
			Byte[] data = this.StringTableData;
			return offset < sizeof(UInt32) || offset >= data.Length
				? null
				: ReadString(data, (Int32)offset, out _);
		}

		/// <summary>Get full name of the symbol</summary>
		/// <param name="symbol">Symbol from <see cref="Symbols"/></param>
		/// <returns>Short name stored inside the symbol or long name from the string table</returns>
		public String GetSymbolName(WinNT.IMAGE_COFF_SYMBOL symbol)
			=> symbol.Name.Short == 0
				? this.GetString(symbol.Name.Long)
				: symbol.SymbolTabeShortName;

		/// <summary>Get full name of the section</summary>
		/// <param name="section">Section from <see cref="Sections"/></param>
		/// <returns>Section name, with "/nnn" (decimal) and "//nnnnnn" (base64) long names resolved from the string table</returns>
		public String GetSectionName(WinNT.IMAGE_SECTION_HEADER section)
		{
			String name = section.Section;
			UInt64 offset = 0;

			if(name.StartsWith("//", StringComparison.Ordinal) && name.Length > 2)
			{
				for(Int32 loop = 2; loop < name.Length; loop++)
				{
					Int32 digit = SectionNameBase64.IndexOf(name[loop]);
					if(digit < 0)
						return name;
					offset = offset * 64 + (UInt32)digit;
				}
			} else if(name.StartsWith("/", StringComparison.Ordinal) && name.Length > 1)
			{
				for(Int32 loop = 1; loop < name.Length; loop++)
				{
					Char c = name[loop];
					if(c < '0' || c > '9')
						return name;
					offset = offset * 10 + (UInt32)(c - '0');
				}
			} else
				return name;

			return offset <= UInt32.MaxValue
				? this.GetString((UInt32)offset) ?? name
				: name;
		}

		/// <summary>Read bytes from image</summary>
		/// <param name="offset">File offset to start reading from</param>
		/// <param name="length">How much to read</param>
		/// <returns>Read bytes</returns>
		public Byte[] ReadBytes(UInt32 offset, UInt32 length)
			=> this.Loader.ReadBytes(offset, length);

		/// <summary>Get structure from specific file offset</summary>
		/// <typeparam name="T">Structure to map</typeparam>
		/// <param name="offset">File offset to the beginning of structure</param>
		/// <returns>Mapped structure</returns>
		public T PtrToStructure<T>(UInt32 offset) where T : struct
			=> this.Loader.PtrToStructure<T>(offset);

		/// <summary>Get string from specific file offset</summary>
		/// <param name="offset">File offset to the beginning of string</param>
		/// <returns>Mapped string</returns>
		public String PtrToStringAnsi(UInt32 offset)
			=> this.Loader.PtrToStringAnsi(offset);

		/// <summary>Close loader</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Dispose managed objects</summary>
		/// <param name="disposing">Dispose managed objects</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this.Loader != null)
			{
				this.Loader.Dispose();
				this.Loader = null;
			}
		}

		/// <summary>Gets image length or <see cref="Int64.MaxValue"/> if the loader can't report it</summary>
		private Int64 GetImageLength()
		{
			try
			{
				return this.Loader.Length;
			} catch(NotSupportedException)
			{
				return Int64.MaxValue;
			}
		}

		/// <summary>Decode null-terminated string by scanning bytes, so multi-byte characters can't break alignment of the following strings</summary>
		/// <param name="data">Source buffer</param>
		/// <param name="offset">Start of the string</param>
		/// <param name="end">Index of the terminating null byte (or buffer length if the string is not terminated)</param>
		/// <returns>Decoded string</returns>
		private static String ReadString(Byte[] data, Int32 offset, out Int32 end)
		{
			end = Array.IndexOf<Byte>(data, 0, offset);
			if(end < 0)
				end = data.Length;
			return Encoding.UTF8.GetString(data, offset, end - offset);
		}
	}
}