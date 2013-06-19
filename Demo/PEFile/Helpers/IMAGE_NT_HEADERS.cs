/*------------------------------------------------------------------------------
 * SourceCodeDownloader - Kerem Kusmezer (keremskusmezer@gmail.com)
 *                        John Robbins (john@wintellect.com)
 * 
 * Download all the .NET Reference Sources and PDBs to pre-populate your 
 * symbol server! No more waiting as each file downloads individually while
 * debugging.
 * ---------------------------------------------------------------------------*/
#region Licence Information
/*       
 * http://www.codeplex.com/NetMassDownloader To Get The Latest Version
 *     
 * Copyright 2008 Kerem Kusmezer(keremskusmezer@gmail.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Taken From The Following Project Also Written By Kerem Kusmezer 
 * PdbParser in C# http://www.codeplex.com/pdbparser 
 * 
*/
#endregion

#region Imported Libraries
using System.Runtime.InteropServices;
using System;
#endregion

namespace DownloadLibrary.PEParsing
{

	//    #define IMAGE_DIRECTORY_ENTRY_EXPORT          0   // Export Directory
	//#define IMAGE_DIRECTORY_ENTRY_IMPORT          1   // Import Directory
	//#define IMAGE_DIRECTORY_ENTRY_RESOURCE        2   // Resource Directory
	//#define IMAGE_DIRECTORY_ENTRY_EXCEPTION       3   // Exception Directory
	//#define IMAGE_DIRECTORY_ENTRY_SECURITY        4   // Security Directory
	//#define IMAGE_DIRECTORY_ENTRY_BASERELOC       5   // Base Relocation Table
	//#define IMAGE_DIRECTORY_ENTRY_DEBUG           6   // Debug Directory
	////      IMAGE_DIRECTORY_ENTRY_COPYRIGHT       7   // (X86 usage)
	//#define IMAGE_DIRECTORY_ENTRY_ARCHITECTURE    7   // Architecture Specific Data
	//#define IMAGE_DIRECTORY_ENTRY_GLOBALPTR       8   // RVA of GP
	//#define IMAGE_DIRECTORY_ENTRY_TLS             9   // TLS Directory
	//#define IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG    10   // Load Configuration Directory
	//#define IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT   11   // Bound Import Directory in headers
	//#define IMAGE_DIRECTORY_ENTRY_IAT            12   // Import Address Table
	//#define IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT   13   // Delay Load Import Descriptors
	//#define IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR 14   // COM Runtime descriptor


	//    typedef struct _IMAGE_DEBUG_DIRECTORY {
	//    DWORD   Characteristics;
	//    DWORD   TimeDateStamp;
	//    WORD    MajorVersion;
	//    WORD    MinorVersion;
	//    DWORD   Type;
	//    DWORD   SizeOfData;
	//    DWORD   AddressOfRawData;
	//    DWORD   PointerToRawData;
	//} IMAGE_DEBUG_DIRECTORY, *PIMAGE_DEBUG_DIRECTORY;

	[StructLayout(LayoutKind.Explicit)]
	public struct _IMAGE_DEBUG_DIRECTORY
	{
		[FieldOffset(0)]
		public UInt32 Characteristics;
		[FieldOffset(4)]
		public UInt32 TimeDateStamp;
		[FieldOffset(8)]
		public UInt16 MajorVersion;
		[FieldOffset(10)]
		public UInt16 MinorVersion;
		[FieldOffset(12)]
		public UInt32 Type;
		[FieldOffset(16)]
		public UInt32 SizeOfData;
		[FieldOffset(20)]
		public UInt32 AddressOfRawData;
		[FieldOffset(24)]
		public UInt32 PointerToRawData;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct PESectionHeader
	{
		[FieldOffset(0)]
		public UInt64 Name; // just skip over the name (8 bytes)
		[FieldOffset(8)]
		public UInt32 VirtualSize;
		[FieldOffset(12)]
		public UInt32 VirtualAddress;
		[FieldOffset(16)]
		public UInt32 RawDataSize;
		[FieldOffset(20)]
		public UInt32 RawDataAddress;
		[FieldOffset(24)]
		public UInt32 RelocationAddress;
		[FieldOffset(28)]
		public UInt32 LineNumbersAddress;
		[FieldOffset(32)]
		public UInt16 RelocationCount;
		[FieldOffset(34)]
		public UInt16 LineNumbersCount;
		[FieldOffset(36)]
		public UInt32 Characteristics;
	}


	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_NT_HEADERS32
	{
		[FieldOffset(0)]
		public uint Signature;
		[FieldOffset(4)]
		public IMAGE_FILE_HEADER FileHeader;
		[FieldOffset(24)]
		public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_NT_HEADERS64
	{
		[FieldOffset(0)]
		public uint Signature;
		[FieldOffset(4)]
		public IMAGE_FILE_HEADER FileHeader;
		[FieldOffset(24)]
		public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_FILE_HEADER
	{
		[FieldOffset(0)]
		public ushort Machine;
		[FieldOffset(2)]
		public ushort NumberOfSections;
		[FieldOffset(4)]
		public uint TimeDateStamp;
		[FieldOffset(8)]
		public uint PointerToSymbolTable;
		[FieldOffset(12)]
		public uint NumberOfSymbols;
		[FieldOffset(16)]
		public ushort SizeOfOptionalHeader;
		[FieldOffset(18)]
		public ushort Characteristics;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_OPTIONAL_HEADER64
	{
		[FieldOffset(0)]
		public UInt16 Magic;
		[FieldOffset(2)]
		public Byte MajorLinkerVersion;
		[FieldOffset(3)]
		public Byte MinorLinkerVersion;
		[FieldOffset(4)]
		public UInt32 SizeOfCode;
		[FieldOffset(8)]
		public UInt32 SizeOfInitializedData;
		[FieldOffset(12)]
		public UInt32 SizeOfUninitializedData;
		[FieldOffset(16)]
		public UInt32 AddressOfEntryPoint;
		[FieldOffset(20)]
		public UInt32 BaseOfCode;
		[FieldOffset(24)]
		public UInt32 BaseOfData;
		[FieldOffset(28)]
		public UInt64 ImageBase;
		[FieldOffset(36)]
		public UInt32 SectionAlignment;
		[FieldOffset(40)]
		public UInt32 FileAlignment;
		[FieldOffset(44)]
		public UInt16 MajorOperatingSystemVersion;
		[FieldOffset(46)]
		public UInt16 MinorOperatingSystemVersion;
		[FieldOffset(48)]
		public UInt16 MajorImageVersion;
		[FieldOffset(50)]
		public UInt16 MinorImageVersion;
		[FieldOffset(48)]
		public UInt16 MajorSubsystemVersion;
		[FieldOffset(50)]
		public UInt16 MinorSubsystemVersion;
		[FieldOffset(52)]
		public UInt32 Win32VersionValue;
		[FieldOffset(56)]
		public UInt32 SizeOfImage;
		[FieldOffset(60)]
		public UInt32 SizeOfHeaders;
		[FieldOffset(64)]
		public UInt32 CheckSum;
		[FieldOffset(68)]
		public UInt16 Subsystem;
		[FieldOffset(70)]
		public UInt16 DllCharacteristics;
		[FieldOffset(72)]
		public UInt64 SizeOfStackReserve;
		[FieldOffset(80)]
		public UInt64 SizeOfStackCommit;
		[FieldOffset(88)]
		public UInt64 SizeOfHeapReserve;
		[FieldOffset(96)]
		public UInt64 SizeOfHeapCommit;
		[FieldOffset(104)]
		public UInt32 LoaderFlags;
		[FieldOffset(108)]
		public UInt32 NumberOfRvaAndSizes;
		[FieldOffset(112)]
		public IMAGE_DATA_DIRECTORY DataDirectory1;
		[FieldOffset(120)]
		public IMAGE_DATA_DIRECTORY DataDirectory2;
		[FieldOffset(128)]
		public IMAGE_DATA_DIRECTORY DataDirectory3;
		/// <summary>
		/// Exception Table
		/// </summary>
		[FieldOffset(136)]
		public IMAGE_DATA_DIRECTORY DataDirectory4;
		/// <summary>
		/// Certificate Table
		/// </summary>
		[FieldOffset(144)]
		public IMAGE_DATA_DIRECTORY DataDirectory5;
		/// <summary>
		/// Relocation Table
		/// </summary>
		[FieldOffset(152)]
		public IMAGE_DATA_DIRECTORY DataDirectory6;
		/// <summary>
		/// Debug Data
		/// </summary>
		[FieldOffset(160)]
		public IMAGE_DATA_DIRECTORY DataDirectory7;
		[FieldOffset(168)]
		public IMAGE_DATA_DIRECTORY DataDirectory8;
		[FieldOffset(176)]
		public IMAGE_DATA_DIRECTORY DataDirectory9;
		[FieldOffset(184)]
		public IMAGE_DATA_DIRECTORY DataDirectory10;
		[FieldOffset(192)]
		public IMAGE_DATA_DIRECTORY DataDirectory11;
		[FieldOffset(200)]
		public IMAGE_DATA_DIRECTORY DataDirectory12;
		[FieldOffset(208)]
		public IMAGE_DATA_DIRECTORY DataDirectory13;
		[FieldOffset(216)]
		public IMAGE_DATA_DIRECTORY DataDirectory14;
		[FieldOffset(224)]
		public IMAGE_DATA_DIRECTORY DataDirectory15;
		[FieldOffset(232)]
		public IMAGE_DATA_DIRECTORY DataDirectory16;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_OPTIONAL_HEADER32
	{
		[FieldOffset(0)]
		public UInt16 Magic;
		[FieldOffset(2)]
		public Byte MajorLinkerVersion;
		[FieldOffset(3)]
		public Byte MinorLinkerVersion;
		[FieldOffset(4)]
		public UInt32 SizeOfCode;
		[FieldOffset(8)]
		public UInt32 SizeOfInitializedData;
		[FieldOffset(12)]
		public UInt32 SizeOfUninitializedData;
		[FieldOffset(16)]
		public UInt32 AddressOfEntryPoint;
		[FieldOffset(20)]
		public UInt32 BaseOfCode;
		[FieldOffset(24)]
		public UInt32 BaseOfData;
		[FieldOffset(28)]
		public UInt32 ImageBase;
		[FieldOffset(32)]
		public UInt32 SectionAlignment;
		[FieldOffset(36)]
		public UInt32 FileAlignment;
		[FieldOffset(40)]
		public UInt16 MajorOperatingSystemVersion;
		[FieldOffset(42)]
		public UInt16 MinorOperatingSystemVersion;
		[FieldOffset(44)]
		public UInt16 MajorImageVersion;
		[FieldOffset(46)]
		public UInt16 MinorImageVersion;
		[FieldOffset(48)]
		public UInt16 MajorSubsystemVersion;
		[FieldOffset(50)]
		public UInt16 MinorSubsystemVersion;
		[FieldOffset(52)]
		public UInt32 Win32VersionValue;
		[FieldOffset(56)]
		public UInt32 SizeOfImage;
		[FieldOffset(60)]
		public UInt32 SizeOfHeaders;
		[FieldOffset(64)]
		public UInt32 CheckSum;
		[FieldOffset(68)]
		public UInt16 Subsystem;
		[FieldOffset(70)]
		public UInt16 DllCharacteristics;
		[FieldOffset(72)]
		public UInt32 SizeOfStackReserve;
		[FieldOffset(76)]
		public UInt32 SizeOfStackCommit;
		[FieldOffset(80)]
		public UInt32 SizeOfHeapReserve;
		[FieldOffset(84)]
		public UInt32 SizeOfHeapCommit;
		[FieldOffset(88)]
		public UInt32 LoaderFlags;
		[FieldOffset(92)]
		public UInt32 NumberOfRvaAndSizes;
		[FieldOffset(96)]
		public IMAGE_DATA_DIRECTORY DataDirectory1;
		[FieldOffset(104)]
		public IMAGE_DATA_DIRECTORY DataDirectory2;
		[FieldOffset(112)]
		public IMAGE_DATA_DIRECTORY DataDirectory3;
		/// <summary>
		/// Exception Table
		/// </summary>
		[FieldOffset(120)]
		public IMAGE_DATA_DIRECTORY DataDirectory4;
		/// <summary>
		/// Certificate Table
		/// </summary>
		[FieldOffset(128)]
		public IMAGE_DATA_DIRECTORY DataDirectory5;
		/// <summary>
		/// Relocation Table
		/// </summary>
		[FieldOffset(136)]
		public IMAGE_DATA_DIRECTORY DataDirectory6;
		/// <summary>
		/// Debug Data
		/// </summary>
		[FieldOffset(144)]
		public IMAGE_DATA_DIRECTORY DataDirectory7;
		[FieldOffset(152)]
		public IMAGE_DATA_DIRECTORY DataDirectory8;
		[FieldOffset(160)]
		public IMAGE_DATA_DIRECTORY DataDirectory9;
		[FieldOffset(168)]
		public IMAGE_DATA_DIRECTORY DataDirectory10;
		[FieldOffset(176)]
		public IMAGE_DATA_DIRECTORY DataDirectory11;
		[FieldOffset(184)]
		public IMAGE_DATA_DIRECTORY DataDirectory12;
		[FieldOffset(192)]
		public IMAGE_DATA_DIRECTORY DataDirectory13;
		[FieldOffset(200)]
		public IMAGE_DATA_DIRECTORY DataDirectory14;
		[FieldOffset(208)]
		public IMAGE_DATA_DIRECTORY DataDirectory15;
		[FieldOffset(216)]
		public IMAGE_DATA_DIRECTORY DataDirectory16;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_DATA_DIRECTORY
	{
		public UInt32 VirtualAddress;
		public UInt32 Size;
	}

	//    struct CV_INFO_PDB70 
	//{
	//    DWORD      CvSignature; 
	//    GUID       Signature;       // unique identifier 
	//    DWORD      Age;             // an always-incrementing value 
	//    BYTE       PdbFileName[1];  // zero terminated string with the name of the PDB file 
	//};
	[StructLayout(LayoutKind.Explicit)]
	public struct CV_INFO_PDB70
	{
		[FieldOffset(0)]
		public UInt32 CvSignature;
		[FieldOffset(4)]
		public UInt32 firstPart;
		[FieldOffset(8)]
		public UInt32 secondPart;
		[FieldOffset(12)]
		public UInt32 thirdPart;
		[FieldOffset(16)]
		public UInt32 fourthPart;
		[FieldOffset(20)]
		public UInt32 Age;
		[FieldOffset(24)]
		public byte pdbFileName;
	}
}