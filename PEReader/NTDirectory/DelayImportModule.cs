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
		/// <returns>Address array</returns>
		public IEnumerable<UInt32> GetIntAddress()
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

			UInt32 addrOfImport = 0;
			UInt32 padding = (UInt32)rvaINT;
			UInt32 sizeOfStruct = sizeof(UInt32);
			do
			{
				addrOfImport = this.Directory.Parent.Header.PtrToStructure<UInt32>(padding);//TODO: Необходимо проверить что в 64битной версии адрес занимает 4 байта, а не 8
				padding += sizeOfStruct;

				if(addrOfImport > 0)
					yield return addrOfImport;
			} while(addrOfImport != 0);
		}

		/// <summary>Get Import Name Table array</summary>
		/// <returns>Array of Imported Procedures</returns>
		public IEnumerator<WinNT.IMAGE_IMPORT_BY_NAME> GetEnumerator()
		{
			UInt32 imageBase = 0;
			if(this.Descriptor.grAttrs == WinNT.DLAttr.Va)
				imageBase = (UInt32)this.ImageBase;

			foreach(UInt32 addrOfImport in this.GetIntAddress())
			{
				if((addrOfImport & 0x80000000) != 0)
					yield return new WinNT.IMAGE_IMPORT_BY_NAME()
					{//TODO: Тут на 64битную ещё проверить надо...
						Hint = (UInt16)(addrOfImport & 0x7FFFFFFF),
						Name = null,
					};
				else
					yield return this.Directory.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_BY_NAME>(addrOfImport - imageBase);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}