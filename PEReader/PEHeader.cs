using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>PE/PE+ Header</summary>
	public class PEHeader : IDisposable
	{
		private Boolean? _is64Bit;
		private WinNT.IMAGE_DOS_HEADER? _headerDOS;
		private WinNT.IMAGE_NT_HEADERS32? _headerNT32;
		private WinNT.IMAGE_NT_HEADERS64? _headerNT64;
		private WinNT.IMAGE_SECTION_HEADER[] _sections;

		/// <summary>PE/PE+ loader interface</summary>
		public IImageLoader Loader { get; private set; }

		/// <summary>PE COFF header</summary>
		public WinNT.IMAGE_DOS_HEADER HeaderDos
			=> this._headerDOS ?? (this._headerDOS = this.Loader.PtrToStructure<WinNT.IMAGE_DOS_HEADER>(0)).Value;

		/// <summary>This PE file is extended PE+ file</summary>
		public Boolean Is64Bit
		{
			get
			{
				if(this._is64Bit == null)
					this._is64Bit = this.IsValid && this.HeaderNT64.OptionalHeader.Magic == WinNT.IMAGE_SIGNATURE.IMAGE_NT_OPTIONAL_HDR64_MAGIC;
				return this._is64Bit.Value;
			}
			private set => this._is64Bit = value;
		}

		/// <summary>Loaded file contains valid DOS header</summary>
		public Boolean IsValid
		{
			get
			{
				try
				{
					return this.HeaderDos.IsValid && (this.HeaderNT64.IsValid || this.HeaderNT32.IsValid);
				} catch(ArgumentOutOfRangeException)//TODO: Think about bypass exception handling here...
				{//File is invalid and we can't reader DOS header. See: StreamLoader.ReadBytes for details
					return false;
				}
			}
		}

		/// <summary>NT32 (PE) Header</summary>
		/// <exception cref="InvalidOperationException">Invalid DOS header</exception>
		public WinNT.IMAGE_NT_HEADERS32 HeaderNT32
		{
			get
			{
				if(this._headerNT32 == null)
				{
					if(!this.HeaderDos.IsValid)
						throw new InvalidOperationException("Invalid DOS header");

					this._headerNT32 = this.Loader.PtrToStructure<WinNT.IMAGE_NT_HEADERS32>((UInt32)this.HeaderDos.e_lfanew);
				}
				return this._headerNT32.Value;
			}
		}

		/// <summary>NT64 (PE+) Header</summary>
		/// <exception cref="InvalidOperationException">Invalid DOS header</exception>
		public WinNT.IMAGE_NT_HEADERS64 HeaderNT64
		{
			get
			{
				if(this._headerNT64 == null)
				{
					if(!this.HeaderDos.IsValid)
						throw new InvalidOperationException("Invalid DOS header");

					this._headerNT64 = this.Loader.PtrToStructure<WinNT.IMAGE_NT_HEADERS64>((UInt32)this.HeaderDos.e_lfanew);
				}
				return this._headerNT64.Value;
			}
		}

		/// <summary>Represents the COFF symbols header</summary>
		public WinNT.IMAGE_COFF_SYMBOLS_HEADER? SymbolTable
		{
			get
			{
				WinNT.IMAGE_FILE_HEADER fileHeader=this.Is64Bit
					? this.HeaderNT64.FileHeader
					: this.HeaderNT32.FileHeader;

				return fileHeader.ContainsSymbolTable
					? this.Loader.PtrToStructure<WinNT.IMAGE_COFF_SYMBOLS_HEADER>(fileHeader.PointerToSymbolTable)
					: (WinNT.IMAGE_COFF_SYMBOLS_HEADER?)null;
			}
		}

		/// <summary>PE sections</summary>
		public WinNT.IMAGE_SECTION_HEADER[] Sections
		{
			get
			{
				if(this._sections == null)
				{
					this._sections = new WinNT.IMAGE_SECTION_HEADER[this.HeaderNT32.FileHeader.NumberOfSections];
					UInt16 sizeOfOptionalHeader = this.HeaderNT32.FileHeader.SizeOfOptionalHeader;
					/*if(this.Is64Bit)
					{//It's optional. I.e. FileHeader everywhere the same
						result = new WinNT.IMAGE_SECTION_HEADER[this.HeaderNT64.FileHeader.NumberOfSections];
						sizeOfOptionalHeader=this.HeaderNT64.FileHeader.SizeOfOptionalHeader;
					}
					else
					{
						result = new WinNT.IMAGE_SECTION_HEADER[this.HeaderNT32.FileHeader.NumberOfSections];
						sizeOfOptionalHeader=this.HeaderNT32.FileHeader.SizeOfOptionalHeader;
					}*/

					UInt32 sizeHeader = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_SECTION_HEADER));
					UInt32 offset = (UInt32)(this.HeaderDos.e_lfanew + 4 + Marshal.SizeOf(typeof(WinNT.IMAGE_FILE_HEADER)) + sizeOfOptionalHeader);
					for(Int32 loop = 0;loop < this._sections.Length;loop++)
					{
						this._sections[loop] = this.Loader.PtrToStructure<WinNT.IMAGE_SECTION_HEADER>(offset);
						offset += sizeHeader;
					}
				}
				return this._sections;
			}
		}

		/// <summary>Entry size of PE image</summary>
		public UInt64 ImageSize
		{
			get
			{
				UInt64 result = this.Is64Bit
					? this.HeaderNT64.OptionalHeader.SizeOfHeaders
					: this.HeaderNT32.OptionalHeader.SizeOfHeaders;

				foreach(WinNT.IMAGE_SECTION_HEADER section in this.Sections)
					result += section.SizeOfRawData;
				return result;
			}
		}

		/// <summary>Create instance of PE header reader</summary>
		/// <param name="loader">Image loader</param>
		/// <exception cref="ArgumentNullException">loader is null</exception>
		public PEHeader(IImageLoader loader)
		{
			this.Loader = loader ?? throw new ArgumentNullException(nameof(loader));
			this.Loader.Endianness = EndianHelper.Endian.Little;
		}

		/// <summary>Validate PE headers</summary>
		/// <exception cref="InvalidOperationException">PE image is invalid</exception>
		public virtual void ValidatePeFile()
		{
			if(!this.IsValid)
				throw new InvalidOperationException("Invalid PE File");
		}

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

		/*public UInt32 RvaToOffset(UInt32 rva)
		{
			for(Int32 loop = 0;loop < this.Sections.Length;loop++)
			{
				var section = this.Sections[loop];
				if(section.VirtualAddress<=rva)
					if((section.VirtualAddress + section.VirtualSize) > rva)
					{
						rva -= section.VirtualAddress;
						rva += section.PointerToRawData;
						return rva;
					}
			}
			return rva;
		}*/

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

		/// <summary>Close header and loader</summary>
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