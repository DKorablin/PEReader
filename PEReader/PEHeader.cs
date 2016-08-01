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
		private IImageLoader _loader;

		/// <summary>PE/PE+ loader interface</summary>
		public IImageLoader Loader { get { return this._loader; } }

		/// <summary>DOS заголовок PE файла.</summary>
		public WinNT.IMAGE_DOS_HEADER HeaderDos
		{
			get
			{
				if(this._headerDOS == null)
					this._headerDOS = this.Loader.PtrToStructure<WinNT.IMAGE_DOS_HEADER>(0);
				return this._headerDOS.Value;
			}
		}

		/// <summary>Загружаемый PE файл является PE+.</summary>
		public Boolean Is64Bit
		{
			get
			{
				if(!this._is64Bit.HasValue)
				{
					if(this.IsValid)
						this._is64Bit = this.HeaderNT64.OptionalHeader.Magic == WinNT.IMAGE_SIGNATURE.IMAGE_NT_OPTIONAL_HDR64_MAGIC;
					else this._is64Bit = false;
				}
				return this._is64Bit.Value;
			}
			private set { this._is64Bit = value; }
		}

		/// <summary>Загруженный PE файл является валидным</summary>
		public Boolean IsValid
		{
			get
			{
				if(this.HeaderDos.IsValid)
					return this.HeaderNT64.IsValid || this.HeaderNT32.IsValid;
				else
					return false;
			}
		}

		/// <summary>PE Header</summary>
		/// <exception cref="T:InvalidOperationException">Invalid DOS header</exception>
		public WinNT.IMAGE_NT_HEADERS32 HeaderNT32
		{
			get
			{
				if(!this._headerNT32.HasValue)
				{
					if(!this.HeaderDos.IsValid)
						throw new InvalidOperationException("Invalid DOS header");

					this._headerNT32 = this.Loader.PtrToStructure<WinNT.IMAGE_NT_HEADERS32>((UInt32)this.HeaderDos.e_lfanew);
				}
				return this._headerNT32.Value;
			}
		}

		/// <summary>PE+ Header</summary>
		/// <exception cref="T:InvalidOperationException">Invalid DOS header</exception>
		public WinNT.IMAGE_NT_HEADERS64 HeaderNT64
		{
			get
			{
				if(!this._headerNT64.HasValue)
				{
					if(!this.HeaderDos.IsValid)
						throw new InvalidOperationException("Invalid DOS header");

					this._headerNT64 = this.Loader.PtrToStructure<WinNT.IMAGE_NT_HEADERS64>((UInt32)this.HeaderDos.e_lfanew);
				}
				return this._headerNT64.Value;
			}
		}

		/// <summary>Represents the COFF symbols header.</summary>
		public WinNT.IMAGE_COFF_SYMBOLS_HEADER? SymbolTable
		{
			get
			{
				WinNT.IMAGE_FILE_HEADER fileHeader;
				if(this.Is64Bit)
					fileHeader = this.HeaderNT64.FileHeader;
				else
					fileHeader = this.HeaderNT32.FileHeader;
				if(fileHeader.ContainsSymbolTable)
					return this.Loader.PtrToStructure<WinNT.IMAGE_COFF_SYMBOLS_HEADER>(fileHeader.PointerToSymbolTable);
				else
					return null;
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
					{//Оно необязательно. Т.е. FileHeader везде одинаковый
						result = new WinNT.IMAGE_SECTION_HEADER[this.HeaderNT64.FileHeader.NumberOfSections];
						sizeOfOptionalHeader=this.HeaderNT64.FileHeader.SizeOfOptionalHeader;
					}
					else
					{
						result = new WinNT.IMAGE_SECTION_HEADER[this.HeaderNT32.FileHeader.NumberOfSections];
						sizeOfOptionalHeader=this.HeaderNT32.FileHeader.SizeOfOptionalHeader;
					}*/

					Int32 sizeHeader = Marshal.SizeOf(typeof(WinNT.IMAGE_SECTION_HEADER));
					Int32 offset = this.HeaderDos.e_lfanew + 4 + Marshal.SizeOf(typeof(WinNT.IMAGE_FILE_HEADER)) + sizeOfOptionalHeader;
					for(Int32 loop = 0;loop < this._sections.Length;loop++)
					{
						this._sections[loop] = this.Loader.PtrToStructure<WinNT.IMAGE_SECTION_HEADER>((UInt32)offset);
						offset += sizeHeader;
					}
				}
				return this._sections;
			}
		}

		/// <summary>Entry size of a PE image</summary>
		public UInt64 ImageSize
		{
			get
			{
				UInt64 result;
				if(this.Is64Bit)
					result = this.HeaderNT64.OptionalHeader.SizeOfHeaders;
				else
					result = this.HeaderNT32.OptionalHeader.SizeOfHeaders;

				foreach(WinNT.IMAGE_SECTION_HEADER section in this.Sections)
					result += section.SizeOfRawData;
				return result;
			}
		}

		/// <summary>Create instance of PE header reader</summary>
		/// <param name="loader">Image loader</param>
		/// <exception cref="T:ArgumentNullException">loader is null</exception>
		public PEHeader(IImageLoader loader)
		{
			if(loader == null)
				throw new ArgumentNullException("loader");
			this._loader = loader;
		}

		/// <summary>Validata PE headers</summary>
		/// <exception cref="T:InvalidOperationException">PE image is invalid</exception>
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
		/// <param name="length">How mutch to read</param>
		/// <returns>Readed bytes</returns>
		public Byte[] ReadBytes(UInt32 offset, UInt32 length)
		{
			UInt32 rva = offset;
			if(!this.Loader.IsModuleMapped)
				rva = this.OffsetToRva(offset);

			return this.Loader.ReadBytes(rva, length);
		}

		/// <summary>Get structure from specific RVA</summary>
		/// <typeparam name="T">Structure to map</typeparam>
		/// <param name="offset">RVA to the beggining of structure</param>
		/// <returns>Mapped structure</returns>
		public T PtrToStructure<T>(UInt32 offset) where T : struct
		{
			UInt32 rva = offset;
			if(!this.Loader.IsModuleMapped)
				rva = this.OffsetToRva(offset);

			return this.Loader.PtrToStructure<T>(rva);
		}

		/// <summary>Get string from specific RVA</summary>
		/// <param name="offset">RVA to the beggining of string</param>
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
			if(this._loader != null)
			{
				this._loader.Dispose();
				this._loader = null;
			}
		}
	}
}