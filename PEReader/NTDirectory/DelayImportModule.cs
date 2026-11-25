using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Delay import module description</summary>
	[DefaultProperty(nameof(ModuleName))]
	public class DelayImportModule: IEnumerable<WinNT.IMAGE_IMPORT_BY_NAME>
	{
		/// <summary>PE directory</summary>
		private DelayImport Directory { get; }

		/// <summary>Delay load descriptor</summary>
		public WinNT.ImgDelayDescr Descriptor { get; }

		/// <summary>Name of the imported DLL</summary>
		/// <exception cref="NotImplementedException">Unknown address type specified</exception>
		public String ModuleName
		{
			get
			{
				UInt64 rvaDllName;
				switch(this.Descriptor.grAttrs)
				{
				case WinNT.DLAttr.Rva:
					rvaDllName = this.Descriptor.rvaDLLName;
					break;
				case WinNT.DLAttr.Va:
					rvaDllName = this.Descriptor.rvaDLLName - this.ImageBase;
					break;
				default: throw new NotImplementedException();
				}
				return this.Directory.Parent.Header.PtrToStringAnsi((UInt32)rvaDllName);
			}
		}
		/// <summary>Base address of PE file</summary>
		private UInt64 ImageBase
		{
			get
			{
				var header = this.Directory.Parent.Header;
				return header.Is64Bit
					? header.HeaderNT64.OptionalHeader.ImageBase
					: header.HeaderNT32.OptionalHeader.ImageBase;
			}
		}

		/// <summary>Create instance of delay import module description class</summary>
		/// <param name="directory">PE directory</param>
		/// <param name="descriptor">Base info</param>
		public DelayImportModule(DelayImport directory, WinNT.ImgDelayDescr descriptor)
		{
			this.Directory = directory;
			this.Descriptor = descriptor;
		}

		/// <summary>Get a list of addresses where you can get the names of procedures that will be imported from the module</summary>
		/// <exception cref="NotImplementedException">Unknown address type specified</exception>
		/// <remarks>Each entry is 4 bytes wide in PE images and 8 bytes wide in PE+ images</remarks>
		/// <returns>Address array</returns>
		public IEnumerable<UInt64> GetIntAddress()
		{
			UInt64 rvaINT;
			switch(this.Descriptor.grAttrs)
			{
			case WinNT.DLAttr.Rva:
				rvaINT = this.Descriptor.rvaINT;
				break;
			case WinNT.DLAttr.Va:
				rvaINT = this.Descriptor.rvaINT - this.ImageBase;
				break;
			default: throw new NotImplementedException();
			}

			PEHeader header = this.Directory.Parent.Header;
			Boolean is64Bit = header.Is64Bit;
			UInt32 sizeOfThunk = is64Bit ? (UInt32)sizeof(UInt64) : sizeof(UInt32);//PE+ thunks (IMAGE_THUNK_DATA64) are 8 bytes wide

			UInt32 padding = (UInt32)rvaINT;
			while(true)
			{
				UInt64 addrOfImport = is64Bit
					? header.PtrToStructure<UInt64>(padding)
					: header.PtrToStructure<UInt32>(padding);
				if(addrOfImport == 0)
					yield break;

				yield return addrOfImport;
				padding += sizeOfThunk;
			}
		}

		/// <summary>Get Import Name Table array</summary>
		/// <returns>Array of Imported Procedures</returns>
		public IEnumerator<WinNT.IMAGE_IMPORT_BY_NAME> GetEnumerator()
		{
			UInt64 imageBase = this.Descriptor.grAttrs == WinNT.DLAttr.Va
				? this.ImageBase
				: 0;
			UInt64 ordinalFlag = this.Directory.Parent.Header.Is64Bit
				? 0x8000000000000000UL//IMAGE_ORDINAL_FLAG64
				: 0x80000000UL;//IMAGE_ORDINAL_FLAG32

			foreach(UInt64 addrOfImport in this.GetIntAddress())
			{
				if((addrOfImport & ordinalFlag) != 0)
					yield return new WinNT.IMAGE_IMPORT_BY_NAME()
					{
						Hint = (UInt16)(addrOfImport & 0xFFFF),//Ordinal is stored in the low 16 bits
						Name = null,
					};
				else
					yield return this.Directory.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_BY_NAME>((UInt32)(addrOfImport - imageBase));
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}