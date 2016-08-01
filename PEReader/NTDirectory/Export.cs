using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Export directory class</summary>
	[DefaultProperty("DllName")]
	public class Export : PEDirectoryBase
	{
		/// <summary>Directory is empty</summary>
		public override Boolean IsEmpty
		{
			get
			{
				return base.IsEmpty || this.ExportDirectory.Value.NumberOfNames == 0;
			}
		}
		/// <summary>Директория экспортируемых функций</summary>
		public WinNT.IMAGE_EXPORT_DIRECTORY? ExportDirectory
		{
			get
			{
				if(base.IsEmpty) return null;
				else
					return this.Parent.Header.PtrToStructure<WinNT.IMAGE_EXPORT_DIRECTORY>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>Module name</summary>
		public String DllName
		{
			get
			{
				return base.Directory.IsEmpty ? null : this.Parent.Header.PtrToStringAnsi(this.ExportDirectory.Value.NameRva);
			}
		}
		/// <summary>Create instance of Export class</summary>
		/// <param name="root">Data directory</param>
		public Export(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.EXPORT)
		{
		}
		/// <summary>Получить список экспортируемых функций</summary>
		/// <returns>Массив с адресами экспортируемых функций</returns>
		public IEnumerable<ExportFunction> GetExportFunctions()
		{
			//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			//public delegate void SomeExportedFunctionInTheDll();
			//SomeExportedFunctionInTheDll invoke =
			//(SomeExportedFunctionInTheDll)Marshal.GetDelegateForFunctionPointer((IntPtr)address, typeof(SomeExportedFunctionInTheDll));
			//invoke();

			if(!this.IsEmpty)
			{
				var directory = this.ExportDirectory.Value;
				if(directory.NumberOfNames > 0)
				{
					Int64 hModule = this.Parent.Header.Loader.BaseAddress;
					UInt32 sizeOfInt = sizeof(UInt32);
					UInt32 sizeOfShort = sizeof(UInt16);
					for(UInt32 loop = 0;loop < directory.NumberOfNames;loop++)
					{
						UInt32 position = directory.AddressOfNames + sizeOfInt * loop;
						UInt32 namePtr = base.Parent.Header.PtrToStructure<UInt32>(position);
						String name = base.Parent.Header.PtrToStringAnsi(namePtr);

						position = directory.AddressOfNameOrdinals + sizeOfShort * loop;
						UInt16 ordinal = base.Parent.Header.PtrToStructure<UInt16>(position);

						position = directory.AddressOfFunctions + sizeOfInt * loop;
						UInt32 addressPtr = base.Parent.Header.PtrToStructure<UInt32>(position);
						UInt32 address = (UInt32)hModule + addressPtr;
						yield return new ExportFunction { Ordinal = ordinal, Name = name, Address = address, };
					}
					/*
					InfoExportFunction[] result = new InfoExportFunction[] { };
					result = new InfoExportFunction[directory.NumberOfNames];
					unsafe
					{
						UInt32* nameRef = (UInt32*)new IntPtr(hModule + directory.AddressOfNames);
						UInt16* ordinal = (UInt16*)new IntPtr(hModule + directory.AddressOfNameOrdinals);

						for(UInt32 i = 0;i < directory.NumberOfNames;i++, nameRef++, ordinal++)
						{
							UInt32 position1 = directory.AddressOfNames + (UInt32)Marshal.SizeOf(typeof(UInt32)) * i;
							UInt32 testFuncPtr = base.Info.Header.Loader.PtrToStructure<UInt32>(position1);
							String testFunc = base.Info.Header.PtrToStringAnsi(testFuncPtr);

							UInt32 position2 = directory.AddressOfNameOrdinals + (UInt32)Marshal.SizeOf(typeof(UInt16)) * i;
							UInt16 testOrdinal = base.Info.Header.Loader.PtrToStructure<UInt16>(position2);

							UInt32 position3 = directory.AddressOfFunctions + (UInt32)Marshal.SizeOf(typeof(UInt32)) * i;
							UInt32 testAddrPtr = base.Info.Header.Loader.PtrToStructure<UInt32>(position3);
							UInt32 testFuncAddr = (UInt32)hModule + testAddrPtr;

							IntPtr str = new IntPtr(hModule + (Int32)(*nameRef));
							String funcName = Marshal.PtrToStringAnsi(str);

							Int32 idx = *ordinal;
							UInt32* tmpaa = (UInt32*)(hModule + (directory.AddressOfFunctions + (idx * 4)));
							UInt32 funcAddr = (UInt32)((hModule) + (*tmpaa));
							result[i] = new InfoExportFunction { Ordinal = (UInt16)idx, Name = funcName, Address = funcAddr, };
						}
					}
					return result;
					*/
				}
			}
		}
	}
}