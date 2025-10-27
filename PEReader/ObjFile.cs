using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>OBJ/COFF file reader</summary>
	public class ObjFile : IDisposable
	{
		private WinNT.IMAGE_FILE_HEADER? _fileHeader;
		private WinNT.IMAGE_SECTION_HEADER[] _sections;
		private WinNT.IMAGE_COFF_SYMBOL[] _symbols;
		private String[] _stringTable;

		private IImageLoader Loader { get; set; }

		/// <summary>OBJ File header structure</summary>
		public WinNT.IMAGE_FILE_HEADER FileHeader
			=> this._fileHeader ?? (this._fileHeader = this.Loader.PtrToStructure<WinNT.IMAGE_FILE_HEADER>(0)).Value;

		/// <summary>Checking if OBJ file is valid</summary>
		public Boolean IsValid => this.FileHeader.IsValid;

		/*/// <summary>Represents the COFF symbols header</summary>
		public WinNT.IMAGE_COFF_SYMBOLS_HEADER? SymbolTable
		{
			get
			{
				if(this.FileHeader.ContainsSymbolTable)
					return this.Loader.PtrToStructure<WinNT.IMAGE_COFF_SYMBOLS_HEADER>(this.FileHeader.PointerToSymbolTable);
				else
					return null;
			}
		}*/

		/// <summary>OBJ symbol table structure</summary>
		public WinNT.IMAGE_COFF_SYMBOL[] Symbols
		{
			get
			{
				if(this.FileHeader.ContainsSymbolTable)
				{
					this._symbols = new WinNT.IMAGE_COFF_SYMBOL[this.FileHeader.NumberOfSymbols];
					UInt32 sizeOfSymbols = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_COFF_SYMBOL));
					UInt32 sizeOfAux = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_AUX_SYMBOL));
					UInt32 offset = this.FileHeader.PointerToSymbolTable;
					for(Int32 loop = 0; loop < this._symbols.Length; loop++)
					{
						WinNT.IMAGE_COFF_SYMBOL symbol;
						this._symbols[loop] = symbol = this.Loader.PtrToStructure<WinNT.IMAGE_COFF_SYMBOL>(offset);
						offset += sizeOfSymbols;

						for(Byte innerLoop = 0; innerLoop < symbol.NumberOfAuxSymbols; innerLoop++)
						{
							WinNT.IMAGE_AUX_SYMBOL auxSymbol = this.Loader.PtrToStructure<WinNT.IMAGE_AUX_SYMBOL>(offset);
							offset += sizeOfAux;
							loop++;
						}
					}
				}
				return this._symbols;
			}
		}

		/// <summary>PE sections</summary>
		public WinNT.IMAGE_SECTION_HEADER[] Sections
		{
			get
			{
				if(this._sections == null)
				{
					this._sections = new WinNT.IMAGE_SECTION_HEADER[this.FileHeader.NumberOfSections];
					UInt16 sizeOfOptionalHeader = this.FileHeader.SizeOfOptionalHeader;

					UInt32 sizeHeader = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_SECTION_HEADER));
					UInt32 offset = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_FILE_HEADER)) + sizeOfOptionalHeader;
					for(Int32 loop = 0; loop < this._sections.Length; loop++)
					{
						this._sections[loop] = this.Loader.PtrToStructure<WinNT.IMAGE_SECTION_HEADER>(offset);
						offset += sizeHeader;
					}
				}
				return this._sections;
			}
		}

		/// <summary>Constant strings</summary>
		public String[] StringTable
		{
			get
			{
				if(this._stringTable == null)
				{
					// TODO: Need to find offset to the string table
					UInt32 startOffset = this.FileHeader.PointerToSymbolTable + this.FileHeader.NumberOfSymbols * (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_COFF_SYMBOL));
					UInt32 sizeOfTable = this.PtrToStructure<UInt32>(startOffset);
					UInt32 offset = startOffset + sizeof(UInt32);

					List<String> stringTable = new List<String>();

					while(offset < startOffset + sizeOfTable)
					{
						String data = this.PtrToStringAnsi(offset);
						stringTable.Add(data);

						offset += (UInt32)data.Length + 1;/*+ 0x00*/
					}

					this._stringTable = stringTable.ToArray();
				}

				return this._stringTable;
			}
		}

		/// <summary>Create instance of OBJ file reader</summary>
		/// <param name="loader">Image loader</param>
		public ObjFile(IImageLoader loader)
			=> this.Loader = loader ?? throw new ArgumentNullException(nameof(loader));

		/// <summary>Convert relative address to virtual address</summary>
		/// <param name="offset">RVA in PE image</param>
		/// <returns>VA in image</returns>
		public UInt32 OffsetToRva(UInt32 offset)
		{
			for(Int32 loop = 0; loop < this.Sections.Length; loop++)
			{
				var section = this.Sections[loop];
				UInt32 size = section.VirtualSize;
				if(size == 0)
					size = section.SizeOfRawData;

				if(offset >= section.VirtualAddress
					&& offset < section.VirtualAddress + size)
					return offset - (section.VirtualAddress - section.PointerToRawData);
				/*if(offset >= this.Sections[loop].PointerToRawData
					&& offset < this.Sections[loop].PointerToRawData + this.Sections[loop].SizeOfRawData)
					return offset + this.Sections[loop].VirtualAddress - this.Sections[loop].PointerToRawData;*/
			}
			return offset;
		}

		/// <summary>Read bytes from image</summary>
		/// <param name="offset">RVA to start address</param>
		/// <param name="length">How much to read</param>
		/// <returns>Read bytes</returns>
		public Byte[] ReadBytes(UInt32 offset, UInt32 length)
		{
			UInt32 rva = offset;
			if(!this.Loader.IsModuleMapped)
				rva = this.OffsetToRva(offset);

			return this.Loader.ReadBytes(rva, length);
		}

		/// <summary>Get structure from specific RVA</summary>
		/// <typeparam name="T">Structure to map</typeparam>
		/// <param name="offset">RVA to the beginning of structure</param>
		/// <returns>Mapped structure</returns>
		public T PtrToStructure<T>(UInt32 offset) where T : struct
		{
			UInt32 rva = offset;
			if(!this.Loader.IsModuleMapped)
				rva = this.OffsetToRva(offset);

			return this.Loader.PtrToStructure<T>(rva);
		}

		/// <summary>Get string from specific RVA</summary>
		/// <param name="offset">RVA to the beginning of string</param>
		/// <returns>Mapped string</returns>
		public String PtrToStringAnsi(UInt32 offset)
		{
			UInt32 rva = offset;
			if(!this.Loader.IsModuleMapped)
				rva = this.OffsetToRva(offset);

			return this.Loader.PtrToStringAnsi(rva);
		}

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
	}
}