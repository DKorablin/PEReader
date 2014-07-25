#region Imported Libraries
using System.Runtime.InteropServices;
using System;
#endregion

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
namespace DownloadLibrary.PEParsing
{
	//typedef struct _IMAGE_DOS_HEADER {      // DOS .EXE header
	//WORD   e_magic;                     // Magic number
	//WORD   e_cblp;                      // Bytes on last page of file
	//WORD   e_cp;                        // Pages in file
	//WORD   e_crlc;                      // Relocations
	//WORD   e_cparhdr;                   // Size of header in paragraphs
	//WORD   e_minalloc;                  // Minimum extra paragraphs needed
	//WORD   e_maxalloc;                  // Maximum extra paragraphs needed
	//WORD   e_ss;                        // Initial (relative) SS value
	//WORD   e_sp;                        // Initial SP value
	//WORD   e_csum;                      // Checksum
	//WORD   e_ip;                        // Initial IP value
	//WORD   e_cs;                        // Initial (relative) CS value
	//WORD   e_lfarlc;                    // File address of relocation table
	//WORD   e_ovno;                      // Overlay number
	//WORD   e_res[4];                    // Reserved words
	//WORD   e_oemid;                     // OEM identifier (for e_oeminfo)
	//WORD   e_oeminfo;                   // OEM information; e_oemid specific
	//WORD   e_res2[10];                  // Reserved words
	//LONG   e_lfanew;                    // File address of new exe header
	//} IMAGE_DOS_HEADER, *PIMAGE_DOS_HEADER;
	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_DOS_HEADER
	{
		public UInt16 e_magic;       // Magic number
		public UInt16 e_cblp;        // Bytes on last page of file
		public UInt16 e_cp;          // Pages in file
		public UInt16 e_crlc;        // Relocations
		public UInt16 e_cparhdr;     // Size of header in paragraphs
		public UInt16 e_minalloc;    // Minimum extra paragraphs needed
		public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
		public UInt16 e_ss;          // Initial (relative) SS value
		public UInt16 e_sp;          // Initial SP value
		public UInt16 e_csum;        // Checksum
		public UInt16 e_ip;          // Initial IP value
		public UInt16 e_cs;          // Initial (relative) CS value
		public UInt16 e_lfarlc;      // File address of relocation table
		public UInt16 e_ovno;        // Overlay number
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public UInt16[] e_res1;        // Reserved words
		public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
		public UInt16 e_oeminfo;     // OEM information; e_oemid specific
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		public UInt16[] e_res2;        // Reserved words
		public Int32 e_lfanew;      // File address of new exe header
	}
	 */

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_DOS_HEADER
	{
		[FieldOffset(0)]
		public ushort magic;      // WORD - Magic number

		[FieldOffset(2)]
		public ushort cblp;       // WORD - Bytes on last page of file

		[FieldOffset(4)]
		public ushort cp;         // WORD - Pages in file

		[FieldOffset(6)]
		public ushort crlc;       // WORD - Relocations

		[FieldOffset(8)]
		public ushort cparhdr;    // WORD - Size of header in paragraphs

		[FieldOffset(10)]
		public ushort minalloc;   // WORD - Minimum extra paragraphs needed 

		[FieldOffset(12)]
		public ushort maxalloc;   // WORD - Maximum extra paragraphs needed 

		[FieldOffset(14)]
		public ushort ss;         // WORD - Initial (relative) SS value

		[FieldOffset(16)]
		public ushort sp;         // WORD - Initial SP value

		[FieldOffset(18)]
		public ushort csum;       // WORD - Checksum

		[FieldOffset(20)]
		public ushort ip;         // WORD - Initial IP value

		[FieldOffset(22)]
		public ushort cs;         // WORD - Initial (relative) CS value

		[FieldOffset(24)]
		public ushort lfarlc;     // WORD - File address of relocation table

		[FieldOffset(26)]
		public ushort ovno;       // WORD - Overlay number

		//[FieldOffset(28)]       // WORD[4] = 2x4 = 8
		// public ushort[] e_res;      // WORD[4] - Reserved words

		[FieldOffset(36)]
		public ushort oemid;      // WORD - OEM identifier (for e_oeminfo)

		[FieldOffset(38)]
		public ushort oeminfo;    // WORD - OEM information (e_oemid specific)

		//[FieldOffset(40)]       // WORD[10] = 2x10 = 20
		//public ushort[] d_res2;     // WORD[10] - Reserved words 

		[FieldOffset(60)]
		public int lfanew;        // LONG - File address of new exe header

	}
}