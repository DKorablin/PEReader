using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{//http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/309563-c-native-dll-mapper.html
	public class DynamicDllLoader
	{
		internal class Win32Constants
		{
			public static UInt32 MEM_COMMIT = 0x1000;

			public static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
			public static UInt32 PAGE_READWRITE = 0x04;

			public static UInt32 MEM_RELEASE = 0x8000;
			public static UInt32 MEM_RESERVE = 0x2000;

		}
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_EXPORT_DIRECTORY
		{
			public UInt32 Characteristics;
			public UInt32 TimeDateStamp;
			public UInt16 MajorVersion;
			public UInt16 MinorVersion;
			public UInt32 Name;
			public UInt32 Base;
			public UInt32 NumberOfFunctions;
			public UInt32 NumberOfNames;
			public UInt32 AddressOfFunctions;     // RVA from base of image
			public UInt32 AddressOfNames;     // RVA from base of image
			public UInt32 AddressOfNameOrdinals;  // RVA from base of image
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_IMPORT_BY_NAME
		{
			public Int16 Hint;
			public Byte Name;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct MEMORYMODULE
		{
			public IMAGE_NT_HEADERS headers;
			public IntPtr codeBase;
			public IntPtr modules;
			public Int32 numModules;
			public Int32 initialized;

		}
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_BASE_RELOCATION
		{
			public UInt32 VirtualAddress;
			public UInt32 SizeOfBlock;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_IMPORT_DESCRIPTOR
		{
			public UInt32 CharacteristicsOrOriginalFirstThunk;    // 0 for terminating null import descriptor; RVA to original unbound IAT (PIMAGE_THUNK_DATA)
			public UInt32 TimeDateStamp;                          // 0 if not bound, -1 if bound, and real date\time stamp in IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT (new BIND); O.W. date/time stamp of DLL bound to (Old BIND)
			public UInt32 ForwarderChain;                         // -1 if no forwarders
			public UInt32 Name;
			public UInt32 FirstThunk;                             // RVA to IAT (if bound this IAT has actual addresses)
		}

		/// <summary>Represents the image section header format.</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms680341(v=vs.85).aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_SECTION_HEADER
		{
			/// <summary>An 8-byte, null-padded UTF-8 string. There is no terminating null character if the string is exactly eight characters long. For longer names, this member contains a forward slash (/) followed by an ASCII representation of a decimal number that is an offset into the string table. Executable images do not use a string table and do not support section names longer than eight characters.</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public Byte[] Name;
			//union 
			//{
			//	/// <summary>The file address.</summary>
			//	DWORD PhysicalAddress;
			//	/// <summary>The total size of the section when loaded into memory, in bytes. If this value is greater than the SizeOfRawData member, the section is filled with zeroes. This field is valid only for executable images and should be set to 0 for object files.</summary>
			//	DWORD VirtualSize;
			//} Misc;
			/// <summary>The file address.</summary>
			public UInt32 PhysicalAddress;
			//public uint VirtualSize;
			/// <summary>The address of the first byte of the section when loaded into memory, relative to the image base. For object files, this is the address of the first byte before relocation is applied.</summary>
			public UInt32 VirtualAddress;
			/// <summary>The size of the initialized data on disk, in bytes. This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure. If this value is less than the VirtualSize member, the remainder of the section is filled with zeroes. If the section contains only uninitialized data, the member is zero.</summary>
			public UInt32 SizeOfRawData;
			/// <summary>A file pointer to the first page within the COFF file. This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure. If a section contains only uninitialized data, set this member is zero.</summary>
			public UInt32 PointerToRawData;
			/// <summary>A file pointer to the beginning of the relocation entries for the section. If there are no relocations, this value is zero.</summary>
			public UInt32 PointerToRelocations;
			/// <summary>A file pointer to the beginning of the line-number entries for the section. If there are no COFF line numbers, this value is zero.</summary>
			public UInt32 PointerToLinenumbers;
			/// <summary>The number of relocation entries for the section. This value is zero for executable images.</summary>
			public Int16 NumberOfRelocations;
			/// <summary>The number of line-number entries for the section.</summary>
			public Int16 NumberOfLinenumbers;
			/// <summary>The characteristics of the image. The following values are defined. </summary>
			public UInt32 Characteristics;
		}


		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public unsafe struct IMAGE_DOS_HEADER
		{
			/// <summary>Magic number</summary>
			public UInt16 e_magic;
			/// <summary>Bytes on last page of file</summary>
			public UInt16 e_cblp;
			/// <summary>Pages in file</summary>
			public UInt16 e_cp;
			/// <summary>Relocations</summary>
			public UInt16 e_crlc;
			/// <summary>Size of header in paragraphs</summary>
			public UInt16 e_cparhdr;
			/// <summary>Minimum extra paragraphs needed</summary>
			public UInt16 e_minalloc;
			/// <summary>Maximum extra paragraphs needed</summary>
			public UInt16 e_maxalloc;
			/// <summary>Initial (relative) SS value</summary>
			public UInt16 e_ss;
			/// <summary>Initial SP value</summary>
			public UInt16 e_sp;
			/// <summary>Checksum</summary>
			public UInt16 e_csum;
			/// <summary>Initial IP value</summary>
			public UInt16 e_ip;
			/// <summary>Initial (relative) CS value</summary>
			public UInt16 e_cs;
			/// <summary>File address of relocation table</summary>
			public UInt16 e_lfarlc;
			/// <summary>Overlay number</summary>
			public UInt16 e_ovno;
			/// <summary>Reserved words</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public UInt16[] e_res1;
			/// <summary>OEM identifier (for e_oeminfo)</summary>
			public UInt16 e_oemid;
			/// <summary>OEM information; e_oemid specific</summary>
			public UInt16 e_oeminfo;
			/// <summary>Reserved words</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
			public UInt16[] e_res2;
			/// <summary>File address of new exe header</summary>
			public Int32 e_lfanew;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_DATA_DIRECTORY
		{
			public UInt32 VirtualAddress;
			public UInt32 Size;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_OPTIONAL_HEADER32
		{
			//
			// Standard fields.
			//
			public UInt16 Magic;
			public Byte MajorLinkerVersion;
			public Byte MinorLinkerVersion;
			public UInt32 SizeOfCode;
			public UInt32 SizeOfInitializedData;
			public UInt32 SizeOfUninitializedData;
			public UInt32 AddressOfEntryPoint;
			public UInt32 BaseOfCode;
			public UInt32 BaseOfData;
			//
			// NT additional fields.
			//
			public UInt32 ImageBase;
			public UInt32 SectionAlignment;
			public UInt32 FileAlignment;
			public UInt16 MajorOperatingSystemVersion;
			public UInt16 MinorOperatingSystemVersion;
			public UInt16 MajorImageVersion;
			public UInt16 MinorImageVersion;
			public UInt16 MajorSubsystemVersion;
			public UInt16 MinorSubsystemVersion;
			public UInt32 Win32VersionValue;
			public UInt32 SizeOfImage;
			public UInt32 SizeOfHeaders;
			public UInt32 CheckSum;
			public UInt16 Subsystem;
			public UInt16 DllCharacteristics;
			public UInt32 SizeOfStackReserve;
			public UInt32 SizeOfStackCommit;
			public UInt32 SizeOfHeapReserve;
			public UInt32 SizeOfHeapCommit;
			public UInt32 LoaderFlags;
			public UInt32 NumberOfRvaAndSizes;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public IMAGE_DATA_DIRECTORY[] DataDirectory;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_FILE_HEADER
		{
			public UInt16 Machine;
			public UInt16 NumberOfSections;
			public UInt32 TimeDateStamp;
			public UInt32 PointerToSymbolTable;
			public UInt32 NumberOfSymbols;
			public UInt16 SizeOfOptionalHeader;
			public UInt16 Characteristics;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_NT_HEADERS
		{
			public UInt32 Signature;
			public IMAGE_FILE_HEADER FileHeader;
			public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
		}

		internal static class Win32Imports
		{
			[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			public static extern UInt32 GetProcAddress(IntPtr hModule, String procName);

			[DllImport("kernel32")]
			public static extern Int32 LoadLibrary(String lpFileName);

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

			[DllImport("kernel32")]
			public static extern UInt32 VirtualAlloc(UInt32 lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern Boolean VirtualFree(IntPtr lpAddress, UIntPtr dwSize, UInt32 dwFreeType);

			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern Boolean VirtualProtect(IntPtr lpAddress, UInt32 dwSize, UInt32 flNewProtect, out UInt32 lpflOldProtect);

		}
		internal static class PointerHelpers
		{
			public static T ToStruct<T>(Byte[] data) where T : struct
			{
				unsafe
				{
					fixed(Byte* p = &data[0])
					{
						return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
					}
				}
			}

			public static T ToStruct<T>(Byte[] data, UInt32 from) where T : struct
			{
				unsafe
				{
					fixed(Byte* p = &data[from])
					{
						return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
					}
				}
			}

			public static T ToStruct<T>(IntPtr ptr, UInt32 from) where T : struct
			{
				return (T)Marshal.PtrToStructure(new IntPtr(ptr.ToInt64() + (Int64)from), typeof(T));
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		unsafe delegate Boolean fnDllEntry(Int32 instance, UInt32 reason, void* reserved);

		internal unsafe Boolean LoadLibrary(Byte[] data)
		{
			//fnDllEntry dllEntry;
			IMAGE_DOS_HEADER dosHeader = PointerHelpers.ToStruct<IMAGE_DOS_HEADER>(data);

			IMAGE_NT_HEADERS oldHeader = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(data, (UInt32)dosHeader.e_lfanew);

			IntPtr code = (IntPtr)(Win32Imports.VirtualAlloc(oldHeader.OptionalHeader.ImageBase, oldHeader.OptionalHeader.SizeOfImage, Win32Constants.MEM_RESERVE, Win32Constants.PAGE_READWRITE));

			if(code == IntPtr.Zero)
				code = (IntPtr)(Win32Imports.VirtualAlloc((UInt32)code, oldHeader.OptionalHeader.SizeOfImage, Win32Constants.MEM_RESERVE, Win32Constants.PAGE_READWRITE));

			_module = new MEMORYMODULE { codeBase = code, numModules = 0, modules = new IntPtr(0), initialized = 0 };

			Win32Imports.VirtualAlloc((UInt32)code, oldHeader.OptionalHeader.SizeOfImage, Win32Constants.MEM_COMMIT, Win32Constants.PAGE_READWRITE);

			IntPtr headers = (IntPtr)(Win32Imports.VirtualAlloc((UInt32)code, oldHeader.OptionalHeader.SizeOfHeaders, Win32Constants.MEM_COMMIT, Win32Constants.PAGE_READWRITE));

			Marshal.Copy(data, 0, headers, (Int32)(dosHeader.e_lfanew + oldHeader.OptionalHeader.SizeOfHeaders));

			_module.headers = PointerHelpers.ToStruct<IMAGE_NT_HEADERS>(headers, (UInt32)dosHeader.e_lfanew);
			_module.headers.OptionalHeader.ImageBase = (UInt32)code;

			this.CopySections(data, oldHeader, headers, dosHeader);

			UInt32 locationDelta = (UInt32)(code.ToInt32() - oldHeader.OptionalHeader.ImageBase);

			if(locationDelta != 0)
				this.PerformBaseRelocation(locationDelta);

			this.BuildImportTable();
			this.FinalizeSections(headers, dosHeader, oldHeader);

			Boolean success = false;

			try
			{
				fnDllEntry dllEntry =
					(fnDllEntry)Marshal.GetDelegateForFunctionPointer(
						new IntPtr(_module.codeBase.ToInt32() + (Int32)_module.headers.OptionalHeader.AddressOfEntryPoint),
						typeof(fnDllEntry));
				success = dllEntry(code.ToInt32(), 1, (void*)0);
			} catch(Exception exc)
			{
				System.Diagnostics.Trace.WriteLine(exc.Message);
				return false;
			}
			return success;
		}

		public Int32 GetModuleCount()
		{
			Int32 count = 0;
			IntPtr codeBase = this._module.codeBase;
			IMAGE_DATA_DIRECTORY directory = this._module.headers.OptionalHeader.DataDirectory[1];
			if(directory.Size > 0)
			{
				IMAGE_IMPORT_DESCRIPTOR importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, directory.VirtualAddress);
				while(importDesc.Name > 0)
				{
					Int32 str = codeBase.ToInt32() + (Int32)importDesc.Name;
					String tmp = Marshal.PtrToStringAnsi(new IntPtr(str));
					Int32 handle = Win32Imports.LoadLibrary(tmp);

					if(handle == -1)
						break;

					count++;
					importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, (UInt32)(directory.VirtualAddress + (Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * (count))));
				}
			}
			return count;
		}

		public Int32 BuildImportTable()
		{
			Int32 ucount = this.GetModuleCount();
			this._module.modules = Marshal.AllocHGlobal((ucount) * sizeof(Int32));
			Int32 pcount = 0;
			Int32 result = 1;
			IntPtr codeBase = _module.codeBase;
			IMAGE_DATA_DIRECTORY directory = _module.headers.OptionalHeader.DataDirectory[1];
			if(directory.Size > 0)
			{
				IMAGE_IMPORT_DESCRIPTOR importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, directory.VirtualAddress);
				while(importDesc.Name > 0)
				{
					IntPtr str = new IntPtr(codeBase.ToInt32() + (Int32)importDesc.Name);
					String tmp = Marshal.PtrToStringAnsi(str);
					unsafe
					{
						UInt32* thunkRef;
						UInt32* funcRef;

						Int32 handle = Win32Imports.LoadLibrary(tmp);

						if(handle == -1)
						{
							result = 0;
							break;
						}

						if(importDesc.CharacteristicsOrOriginalFirstThunk > 0)
						{
							IntPtr thunkRefAddr = new IntPtr(codeBase.ToInt32() + (Int32)importDesc.CharacteristicsOrOriginalFirstThunk);
							thunkRef = (UInt32*)thunkRefAddr;
							funcRef = (UInt32*)(codeBase.ToInt32() + (Int32)importDesc.FirstThunk);
						} else
						{
							thunkRef = (UInt32*)(codeBase.ToInt32() + (Int32)importDesc.FirstThunk);
							funcRef = (UInt32*)(codeBase.ToInt32() + (Int32)importDesc.FirstThunk);
						}
						for(;*thunkRef > 0;thunkRef++, funcRef++)
						{
							if((*thunkRef & 0x80000000) != 0)
								*funcRef = (UInt32)Win32Imports.GetProcAddress(new IntPtr(handle), new IntPtr(*thunkRef & 0xffff));
							else
							{
								IntPtr str2 = new IntPtr(codeBase.ToInt32() + (Int32)(*thunkRef) + 2);
								String tmpaa = Marshal.PtrToStringAnsi(str2);
								*funcRef = Win32Imports.GetProcAddress(new IntPtr(handle), tmpaa);
							}
							if(*funcRef == 0)
							{
								result = 0;
								break;
							}
						}


						pcount++;
						importDesc = PointerHelpers.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(codeBase, directory.VirtualAddress + (uint)(Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * pcount));
					}
				}
			}
			return result;
		}

		private static readonly Int32[][][] ProtectionFlags = new Int32[2][][];

		public void FinalizeSections(IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS oldHeaders)
		{
			ProtectionFlags[0] = new Int32[2][];
			ProtectionFlags[1] = new Int32[2][];
			ProtectionFlags[0][0] = new Int32[2];
			ProtectionFlags[0][1] = new Int32[2];
			ProtectionFlags[1][0] = new Int32[2];
			ProtectionFlags[1][1] = new Int32[2];
			ProtectionFlags[0][0][0] = 0x01;
			ProtectionFlags[0][0][1] = 0x08;
			ProtectionFlags[0][1][0] = 0x02;
			ProtectionFlags[0][1][1] = 0x04;
			ProtectionFlags[1][0][0] = 0x10;
			ProtectionFlags[1][0][1] = 0x80;
			ProtectionFlags[1][1][0] = 0x20;
			ProtectionFlags[1][1][1] = 0x40;

			IMAGE_SECTION_HEADER section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (UInt32)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader));
			for(Int32 i = 0;i < _module.headers.FileHeader.NumberOfSections;i++)
			{
				//Console.WriteLine("Finalizing " + Encoding.UTF8.GetString(section.Name));
				Int32 executable = (section.Characteristics & 0x20000000) != 0 ? 1 : 0;
				Int32 readable = (section.Characteristics & 0x40000000) != 0 ? 1 : 0;
				Int32 writeable = (section.Characteristics & 0x80000000) != 0 ? 1 : 0;

				if((section.Characteristics & 0x02000000) > 0)
				{
					Boolean aa = Win32Imports.VirtualFree(new IntPtr(section.PhysicalAddress), (UIntPtr)section.SizeOfRawData, 0x4000);
					continue;
				}

				UInt32 protect = (UInt32)ProtectionFlags[executable][readable][writeable];

				if((section.Characteristics & 0x04000000) > 0)
					protect |= 0x200;
				Int32 size = (Int32)section.SizeOfRawData;
				if(size == 0)
				{
					if((section.Characteristics & 0x00000040) > 0)
						size = (Int32)_module.headers.OptionalHeader.SizeOfInitializedData;
					else if((section.Characteristics & 0x00000080) > 0)
						size = (Int32)_module.headers.OptionalHeader.SizeOfUninitializedData;

				}

				if(size > 0)
				{
					UInt32 oldProtect;
					if(!Win32Imports.VirtualProtect(new IntPtr(section.PhysicalAddress), section.SizeOfRawData, protect, out oldProtect))
					{
					}
				}

				section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (UInt32)((24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1))));
			}

		}

		public void PerformBaseRelocation(UInt32 delta)
		{
			IntPtr codeBase = _module.codeBase;
			Int32 sizeOfBase = Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
			IMAGE_DATA_DIRECTORY directory = _module.headers.OptionalHeader.DataDirectory[5];
			Int32 cnt = 0;
			if(directory.Size > 0)
			{
				IMAGE_BASE_RELOCATION relocation = PointerHelpers.ToStruct<IMAGE_BASE_RELOCATION>(codeBase, directory.VirtualAddress);
				while(relocation.VirtualAddress > 0)
				{
					unsafe
					{
						IntPtr dest = (IntPtr)(codeBase.ToInt32() + (int)relocation.VirtualAddress);
						UInt16* relInfo = (UInt16*)(codeBase.ToInt32() + (int)directory.VirtualAddress + sizeOfBase);
						UInt16 i;
						for(i = 0;i < ((relocation.SizeOfBlock - Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION))) / 2);i++, relInfo++)
						{
							Int32 type = *relInfo >> 12;
							Int32 offset = (*relInfo & 0xfff);
							switch(type)
							{
							case 0x00:
								break;
							case 0x03:
								UInt32* patchAddrHl = (UInt32*)((dest.ToInt32()) + (offset));
								*patchAddrHl += delta;
								break;
							}
						}
					}
					cnt += (Int32)relocation.SizeOfBlock;
					relocation = PointerHelpers.ToStruct<IMAGE_BASE_RELOCATION>(codeBase, (UInt32)(directory.VirtualAddress + cnt));

				}
			}
		}

		public String[] GetProcedures()
		{
			unsafe
			{
				IntPtr codeBase = this._module.codeBase;
				IMAGE_DATA_DIRECTORY directory = this._module.headers.OptionalHeader.DataDirectory[0];
				if(directory.Size == 0)
					return new String[] { };

				IMAGE_EXPORT_DIRECTORY exports = PointerHelpers.ToStruct<IMAGE_EXPORT_DIRECTORY>(codeBase, directory.VirtualAddress);
				UInt32* nameRef = (UInt32*)new IntPtr(codeBase.ToInt32() + exports.AddressOfNames);
				UInt16* ordinal = (UInt16*)new IntPtr(codeBase.ToInt32() + exports.AddressOfNameOrdinals);

				String[] result = new String[exports.NumberOfNames];
				for(UInt32 i = 0;i < exports.NumberOfNames;i++, nameRef++, ordinal++)
				{
					IntPtr str = new IntPtr(codeBase.ToInt32() + (Int32)(*nameRef));
					result[i] = Marshal.PtrToStringAnsi(str);
				}
				return result;
			}
		}
		private MEMORYMODULE _module;
		public UInt32? GetProcAddress(String name)
		{
			unsafe
			{
				IntPtr codeBase = this._module.codeBase;
				Int32? idx = null;
				IMAGE_DATA_DIRECTORY directory = this._module.headers.OptionalHeader.DataDirectory[0];
				if(directory.Size == 0)
					return null;
				IMAGE_EXPORT_DIRECTORY exports = PointerHelpers.ToStruct<IMAGE_EXPORT_DIRECTORY>(codeBase, directory.VirtualAddress);
				UInt32* nameRef = (UInt32*)new IntPtr(codeBase.ToInt32() + exports.AddressOfNames);
				UInt16* ordinal = (UInt16*)new IntPtr(codeBase.ToInt32() + exports.AddressOfNameOrdinals);

				for(UInt32 i = 0;i < exports.NumberOfNames;i++, nameRef++, ordinal++)
				{
					IntPtr str = new IntPtr(codeBase.ToInt32() + (Int32)(*nameRef));
					String tmp = Marshal.PtrToStringAnsi(str);
					if(tmp == name)
					{
						idx = *ordinal;
						break;
					}
				}
				if(idx.HasValue)
				{
					UInt32* tmpaa = (UInt32*)(codeBase.ToInt32() + (exports.AddressOfFunctions + (idx.Value * 4)));
					UInt32 addr = (UInt32)((codeBase.ToInt32()) + (*tmpaa));
					return addr;
				} else return null;
			}
		}

		public void CopySections(Byte[] data, IMAGE_NT_HEADERS oldHeaders, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
		{
			Int32 i;
			IntPtr codebase = _module.codeBase;
			IMAGE_SECTION_HEADER section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (UInt32)(24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader));
			for(i = 0;i < _module.headers.FileHeader.NumberOfSections;i++)
			{
				IntPtr dest;
				if(section.SizeOfRawData == 0)
				{
					UInt32 size = oldHeaders.OptionalHeader.SectionAlignment;
					if(size > 0)
					{
						dest = new IntPtr((Win32Imports.VirtualAlloc((UInt32)(codebase.ToInt32() + (Int32)section.VirtualAddress), size, Win32Constants.MEM_COMMIT, Win32Constants.PAGE_READWRITE)));

						section.PhysicalAddress = (UInt32)dest;
						IntPtr write = new IntPtr(headers.ToInt32() + (32 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i)));
						Marshal.WriteInt32(write, (Int32)dest);
						Byte[] datazz = new Byte[size + 1];
						Marshal.Copy(datazz, 0, dest, (Int32)size);
					}
					section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (UInt32)((24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1))));
					continue;
				}

				dest = new IntPtr((Win32Imports.VirtualAlloc((UInt32)(codebase.ToInt32() + (Int32)section.VirtualAddress), section.SizeOfRawData, Win32Constants.MEM_COMMIT,
											 Win32Constants.PAGE_READWRITE)));
				Marshal.Copy(data, (Int32)section.PointerToRawData, dest, (Int32)section.SizeOfRawData);
				section.PhysicalAddress = (UInt32)dest;
				IntPtr write2 = new IntPtr(headers.ToInt32() + (32 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i)));
				Marshal.WriteInt32(write2, (Int32)dest);
				section = PointerHelpers.ToStruct<IMAGE_SECTION_HEADER>(headers, (UInt32)((24 + dosHeader.e_lfanew + oldHeaders.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1))));
			}
		}
	}
}