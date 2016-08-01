using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Export directory class</summary>
	[DefaultProperty("Header")]
	[DebuggerDisplay("Header = {Header}")]
	public class Export : PEDirectoryBase
	{
		/// <summary>Directory is empty</summary>
		public override Boolean IsEmpty
		{
			get
			{
				return base.IsEmpty || this.Header.Value.NumberOfNames == 0;
			}
		}

		/// <summary>Директория экспортируемых функций</summary>
		public WinNT.IMAGE_EXPORT_DIRECTORY? Header
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
				return base.Directory.IsEmpty ? null : this.Parent.Header.PtrToStringAnsi(this.Header.Value.NameRva);
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
				var directory = this.Header.Value;
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
				}
			}
		}
	}
}