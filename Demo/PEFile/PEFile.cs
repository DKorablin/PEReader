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
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using DownloadLibrary.Classes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace DownloadLibrary.PEParsing
{
	//[CLSCompliant(true)]
	public class PEFile
	{
		// Private members to track information at runtime
		private string m_ExePath;
		private UInt32 m_PESignatureOffset;

		const Byte PESignatureOffsetLoc = 0x3C;

		//32Bit Version
		const UInt16 PE32 = 0x010B;
		//64Bit Version
		const UInt16 PE32PLUS = 0x020B;
		// PE file magic number
		const UInt32 PEMAGIC = 0x00004550;

		private String _pdbVersion;
		private String _pdbFullName;
		private Guid _pdbGuid;
		private String _pdbAge;

		public String PdbAge
		{
			get { return this._pdbAge; }
			private set { this._pdbAge = value; }
		}

		public Guid PdbGuid
		{
			get { return this._pdbGuid; }
			private set { this._pdbGuid = value; }
		}
		/// <summary>Pdb Version Used By The Server</summary>
		public string PdbVersion
		{
			get { return this._pdbVersion; }
			private set { this._pdbVersion = value; }
		}

		public String PdbFullName
		{
			get { return this._pdbFullName; }
			private set { this._pdbFullName = value; }
		}


		private String _pdbFileName;
		/// <summary>Pdb FileName Which Will Be Used To Download The File</summary>
		public String PdbFileName
		{
			get
			{
				if(this._pdbFileName == null)
					this._pdbFileName = Path.GetFileName(this.PdbFullName);
				return this._pdbFileName;
			}
		}

		//{{HDN==================================================
		private FileInfo _fileInfo;
		public FileInfo FileInfo
		{
			get { return _fileInfo; }
			private set { _fileInfo = value; }
		}

		private FileVersionInfo _fileVersionInfo;
		public FileVersionInfo FileVersionInfo
		{
			get { return _fileVersionInfo; }
			private set { _fileVersionInfo = value; }
		}


		/// <summary>Simple PE Parser, That Extracts The PDB Location From a PE File, which also checks for PE File Validity.</summary>
		/// <param name="path"></param>
		public PEFile(String path)
		{
			if(String.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			FileInfo peContainer = new FileInfo(path);

			if(!peContainer.Exists)
				throw new FileNotFoundException(path);

			this.FileInfo = peContainer;
			try
			{
				this.FileVersionInfo = FileVersionInfo.GetVersionInfo(path);
			} catch(Exception exc)
			{
				Debug.WriteLine(exc.ToString());
				this.FileVersionInfo = null;
			}

			using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader br = new BinaryReader(fs))
				{
					// Find the address of the PE Header
					try
					{
						br.BaseStream.Seek(PESignatureOffsetLoc, SeekOrigin.Begin);
						// BugFix For Bug Number 677
						// After the MS DOS stub, at the file offset specified at offset 0x3c, 
						// We must read 4 bytes to located the position of the PE Header In The Executable File
						// is a 4-byte signature that identifies the file as a PE format image file. 
						// This signature is “PE\0\0” (the letters “P” and “E” followed by two null bytes).
						// m_PESignatureOffset = br.ReadByte();
						// Thx to Microsofts mdb source code in pdb2xml this was wrongly implemented.
						// Should i trust their code :) 
						// Better Writing The Code Like In Pdb Extractor myself seems to be the right way :)
						m_PESignatureOffset = br.ReadUInt32();
						br.BaseStream.Seek(m_PESignatureOffset, SeekOrigin.Begin);
						// The PE Signature like defined 
						// in Microsoft Portable Executable and Common Object File Format Specification v8.0              
						if(br.ReadByte() != 'P')
							throw new FileLoadException("PE Signature corrupt");
						if(br.ReadByte() != 'E')
							throw new FileLoadException("PE Signature corrupt");
						if(br.ReadByte() != '\0')
							throw new FileLoadException("PE Signature corrupt");
						if(br.ReadByte() != '\0')
							throw new FileLoadException("PE Signature corrupt");
					} catch(EndOfStreamException)
					{
						throw new FileLoadException("Read past end of file");
					}


				}
			}


			m_ExePath = path;

			byte[] Data = System.IO.File.ReadAllBytes(path);

			//This section is unsafe so faster access is achieved :)
			unsafe
			{
				//First Check The Length If it is lesser then dos header + image headers
				if(peContainer.Length < (sizeof(IMAGE_DOS_HEADER)) + sizeof(IMAGE_NT_HEADERS32))
					throw new FileLoadException("Invalid PE File");

				fixed(byte* p_Data = Data)
				{

					// Get the first 64 bytes and turn it into a IMAGE_DOS_HEADER
					IMAGE_DOS_HEADER* idh =
						  (IMAGE_DOS_HEADER*)p_Data;
					//Read Image Nt Headers                      
					IMAGE_NT_HEADERS64* inhs64 =
						(IMAGE_NT_HEADERS64*)(idh->lfanew + p_Data);
					//Check The PE00 signature
					if(inhs64->Signature != PEMAGIC)
						throw new FileLoadException("PE Signature corrupt");

					// The calculated values that are different depending on
					// 32 vs. 64-bit.
					int SizeOfOptionalHeader = 0;
					uint rva = 0;
					uint DebugDirSize = 0;
					ushort NumberOfSections = 0;

					if(inhs64->OptionalHeader.Magic == PE32)
					{
						// Get all the 32-bit offsets.
						IMAGE_NT_HEADERS32* inhs32 =
							(IMAGE_NT_HEADERS32*)(idh->lfanew + p_Data);

						SizeOfOptionalHeader = (int)
									inhs32->FileHeader.SizeOfOptionalHeader;
						//Debug Section Is Always In 7th Member Of The Optional Headers Data Directory
						rva = inhs32->OptionalHeader.DataDirectory7.VirtualAddress;
						DebugDirSize =
							inhs32->OptionalHeader.DataDirectory7.Size;
						NumberOfSections = inhs32->FileHeader.NumberOfSections;
					} else if(inhs64->OptionalHeader.Magic == PE32PLUS)
					{
						SizeOfOptionalHeader = (int)
									inhs64->FileHeader.SizeOfOptionalHeader;
						//Debug Section Is Always In 7th Member Of The Optional Headers Data Directory
						rva =
							inhs64->OptionalHeader.DataDirectory7.VirtualAddress;
						DebugDirSize =
							inhs64->OptionalHeader.DataDirectory7.Size;
						NumberOfSections = inhs64->FileHeader.NumberOfSections;
					}

					//No Debug Section Found So Exit.
					if((rva == 0) || (DebugDirSize == 0))
					{
						throw new NotSupportedException();// NoDebugSectionException();
					}

					//Find out the data section
					Int32 dataSectionsOffset = (idh->lfanew) +
											   4 +
											   sizeof(IMAGE_FILE_HEADER)
											   +
											   (int)SizeOfOptionalHeader;
					bool found = false;

					//Loop through the debug sections, enumerate whole sections try to locate the type 2(CODEVIEW) section
					//with magical header 0x53445352 For PDB 7.0 Entries , my code won't support PDB V2.0 Entries , because none
					//of the .Net Assemblies Use It.
					for(int i = 0;i < NumberOfSections;i++)
					{

						PESectionHeader* myHeader =
							(PESectionHeader*)(dataSectionsOffset + p_Data);

						uint sectionSize =
							myHeader->VirtualSize;

						if(sectionSize == 0)
							sectionSize = myHeader->RawDataSize;

						if((rva >= myHeader->VirtualAddress) &&
							(rva < myHeader->VirtualAddress + sectionSize))
						{
							found = true;
						}

						if(found)
						{

							found = false;

							int diff =
								(int)(myHeader->VirtualAddress - myHeader->RawDataAddress);

							UInt32 fileOffset = rva - (uint)diff;

							_IMAGE_DEBUG_DIRECTORY* debugDirectory =
								(_IMAGE_DEBUG_DIRECTORY*)(fileOffset + p_Data);

							int NumEntries = (int)(DebugDirSize / sizeof(_IMAGE_DEBUG_DIRECTORY));


							for(int ix = 1;ix <= NumEntries;ix++, debugDirectory++)
							{
								if(debugDirectory->Type == 2)
								{
									UInt32 cvSignature = *(UInt32*)(p_Data + debugDirectory->PointerToRawData);
									if(cvSignature == 0x53445352)
									{
										CV_INFO_PDB70* pCvInfo =
											(CV_INFO_PDB70*)(p_Data + debugDirectory->PointerToRawData);

										string hexAge = pCvInfo->Age.ToString("x2");

										if(hexAge != "0")
										{
											hexAge = (hexAge.StartsWith("0") ? hexAge.Substring(1) : hexAge);
										} else
										{
											hexAge = "0";
										}

										this.PdbAge = pCvInfo->Age.ToString();

										string finalHex = String.Empty;

										byte[] firstHeader =
											BitConverter.GetBytes(pCvInfo->firstPart);

										byte[] secondHeader =
											BitConverter.GetBytes(pCvInfo->secondPart);

										byte[] thirdHeader =
											BitConverter.GetBytes(pCvInfo->thirdPart);

										byte[] fourthHeader =
											BitConverter.GetBytes(pCvInfo->fourthPart);


										byte[] finalGuid = new byte[16];

										//Make The Byte Order Correct So We Can Construct The Right Guid Out Of It

										//Guid Buildup Begin
										finalGuid[0] = firstHeader[3];
										finalGuid[1] = firstHeader[2];
										finalGuid[2] = firstHeader[1];
										finalGuid[3] = firstHeader[0];
										//c section
										finalGuid[4] = secondHeader[5 - 4];
										finalGuid[5] = secondHeader[4 - 4];
										//d relocation
										finalGuid[6] = secondHeader[7 - 4];
										finalGuid[7] = secondHeader[6 - 4];

										for(int xx = 8;xx < 12;xx++)
										{
											finalGuid[xx] = thirdHeader[xx - 8];
										}

										for(int x = 12;x < 16;x++)
										{
											finalGuid[x] = fourthHeader[x - 12];
										}
										//Guid Buildup End
										//Get The Original Guid
										this.PdbGuid = new Guid(finalGuid);

										finalHex = Utility.ByteArrayToHex(finalGuid);

										this.PdbVersion = finalHex.ToUpperInvariant() + hexAge.ToUpperInvariant();

										//Locate The Pdb Name Entry, it is a null terminated string.
										uint stringBeginLocation =
											debugDirectory->PointerToRawData + 24;

										byte stringLocator =
											*(p_Data + stringBeginLocation);

										System.Text.StringBuilder resultBuilder =
											new System.Text.StringBuilder();

										MemoryStream stringHolder = new MemoryStream();

										while(stringLocator != 0)
										{
											stringHolder.WriteByte(stringLocator);
											//resultBuilder.Append(Encoding.ASCII.GetString(new byte[]{stringLocator}));

											stringBeginLocation++;

											stringLocator = *(p_Data + stringBeginLocation);



										};

										// Buildup The String And Return It. 
										// We assume always that it is ascii encoded.
										this.PdbFullName = Encoding.ASCII.GetString(stringHolder.ToArray());//resultBuilder.ToString();


										return;

									}
								}
							}
						}

						dataSectionsOffset +=
							sizeof(PESectionHeader);

					}
					throw new NotSupportedException(); //throw new NoPdbSectionException();
				}
			}
		}

		/// <summary>
		/// Simple PE Parser, That Extracts The PDB Location From a PE File, which also checks for PE File Validity.
		/// </summary>
		/// <param name="path"></param>
		[Obsolete("Will Be Removed In Next Version", true)]
		public PEFile(string path, bool oldData)
		{
			m_ExePath = path;

			using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader br = new BinaryReader(fs))
				{
					// Find the address of the PE Header
					try
					{
						br.BaseStream.Seek(PESignatureOffsetLoc, SeekOrigin.Begin);
						// BugFix For Bug Number 677
						// After the MS DOS stub, at the file offset specified at offset 0x3c, 
						// We must read 4 bytes to located the position of the PE Header In The Executable File
						// is a 4-byte signature that identifies the file as a PE format image file. 
						// This signature is “PE\0\0” (the letters “P” and “E” followed by two null bytes).
						// m_PESignatureOffset = br.ReadByte();
						// Thx to Microsofts mdb source code in pdb2xml this was wrongly implemented.
						// Should i trust their code :) 
						// Better Writing The Code Like In Pdb Extractor myself seems to be the right way :)
						m_PESignatureOffset = br.ReadUInt32();
						br.BaseStream.Seek(m_PESignatureOffset, SeekOrigin.Begin);
					} catch(EndOfStreamException)
					{
						throw new FileLoadException("Read past end of file");
					}

					// The PE Signature like defined 
					// in Microsoft Portable Executable and Common Object File Format Specification v8.0              
					if(br.ReadByte() != 'P')
						throw new FileLoadException("PE Signature corrupt");
					if(br.ReadByte() != 'E')
						throw new FileLoadException("PE Signature corrupt");
					if(br.ReadByte() != '\0')
						throw new FileLoadException("PE Signature corrupt");
					if(br.ReadByte() != '\0')
						throw new FileLoadException("PE Signature corrupt");

					//0.1. COFF File Header (Object and Image)
					/*
					At the beginning of an object file, or immediately after the signature of an image file, is a standard COFF file header in the following format. Note that the Windows loader limits the number of sections to 96.
					Offset	Size	Field	Description
					0	2	Machine	The number that identifies the type of target machine. For more information, see section 3.3.1, “Machine Types.”
					2	2	NumberOfSections	The number of sections. This indicates the size of the section table, which immediately follows the headers.
					4	4	TimeDateStamp	The low 32 bits of the number of seconds since 00:00 January 1, 1970 (a C run-time time_t value), that indicates when the file was created.
					8	4	PointerToSymbolTable	The file offset of the COFF symbol table, or zero if no COFF symbol table is present. This value should be zero for an image because COFF debugging information is deprecated.
					12	4	NumberOfSymbols	The number of entries in the symbol table. This data can be used to locate the string table, which immediately follows the symbol table. This value should be zero for an image because COFF debugging information is deprecated.
					16	2	SizeOfOptionalHeader	The size of the optional header, which is required for executable files but not for object files. This value should be zero for an object file. For a description of the header format, see section 3.4, “Optional Header (Image Only).”
					18	2	Characteristics	The flags that indicate the attributes of the file. For specific flag values, see section 3.3.2, “Characteristics.”
					*/
					//m_coffHeader = new COFFHeader();
					//m_coffHeader.Read(br);
				}
			}

			LocatePdbInformation(path);

		}

		[Obsolete("Will Be Removed In Next Version", true)]
		private void LocatePdbInformation(string path)
		{
			string wholeHexString = Utility.ByteArrayToHex(System.IO.File.ReadAllBytes(path));

			// BugFix For Bug Number 677
			// struct CV_INFO_PDB70
			/*
			{
				DWORD CvSignature;  --> Which is RSDS always
				GUID Signature; --> A 16 Byte Structure
				DWORD Age; --> A 4 Byte Structure That Tells About The Age Of The File
				BYTE PdbFileName[];
			} ;
			*/



			int pdbLocatorLocation =
				wholeHexString.IndexOf("52534453") + 8;

			// BugFix For Bug Number 677
			// Check If We Found The Correct Location
			// It should be like this : 
			// DWORD CvSignature So The First 4 Bytes Should Be 52 53 44 53
			// GUID Signature --> A 16 Byte Structure Which Holds The Guid 
			// DWORD Age --> A 4 Byte Structure Which Holds The Guid
			// BYTE PdbFileName --> A null terminated string entry

			if(pdbLocatorLocation <= 0)
			{
				this.PdbVersion = String.Empty;
				throw new FileLoadException();
			} else
			{
				byte[] pdbEncoding =
					Utility.HexToByte(wholeHexString.Substring(pdbLocatorLocation, 17 * 2));

				int pdbFileNameEndOffset =
					wholeHexString.IndexOf(Utility.ToStringHexFromString(".pdb"), pdbLocatorLocation) + 2;

				int pdbFileNameBeginOffset =
					pdbLocatorLocation + (17 * 2);



				// If the beginning is bigger than the offset, we're looking at a 
				// file that does not have a .debug section. It's probably a 
				// resource only DLL.
				if(pdbFileNameEndOffset < pdbFileNameBeginOffset)
				{
					throw new NotSupportedException(); //throw new NoDebugSectionException();
				}

				string pdbFileName = wholeHexString.Substring(pdbFileNameBeginOffset + (2 * 3),
															  pdbFileNameEndOffset - pdbFileNameBeginOffset);

				pdbFileName =
					System.Text.Encoding.ASCII.GetString(Utility.ToByteArray(pdbFileName));

				byte[] finalGuid =
					new byte[16];

				finalGuid[0] = pdbEncoding[3];
				finalGuid[1] = pdbEncoding[2];
				finalGuid[2] = pdbEncoding[1];
				finalGuid[3] = pdbEncoding[0];
				//c section
				finalGuid[4] = pdbEncoding[5];
				finalGuid[5] = pdbEncoding[4];
				//d relocation
				finalGuid[6] = pdbEncoding[7];
				finalGuid[7] = pdbEncoding[6];

				for(int i = 8;i < 16;i++)
				{
					finalGuid[i] = pdbEncoding[i];
				}

				string finalHex = pdbEncoding[16].ToString("x2");

				finalHex = Utility.ByteArrayToHex(finalGuid) + (finalHex.StartsWith("0") ? finalHex.Substring(1, 1) : finalHex);
				this.PdbVersion = finalHex.ToUpperInvariant();

				this.PdbFullName = pdbFileName;

			}
		}
	}


/*	#region [HDN] Xml Serialization
	[XmlRoot("PEFile")]
	public class XmlPEFileItem : SerializableItem
	{

		public XmlPEFileItem() { }
		public XmlPEFileItem(PEFile peFile)
		{
			FileInfo fi = peFile.FileInfo;
			m_fileName = fi.Name;
			m_size = fi.Length.ToString();
			m_lastWriteTimeUtc = fi.LastWriteTimeUtc.ToString();

			FileVersionInfo vi = peFile.FileVersionInfo;
			if(vi != null)
			{
				m_description = vi.FileDescription;
				m_version = vi.FileVersion;
			}

			m_pdbFileName = peFile.PdbFileName;
			m_pdbGuid = peFile.PdbGuid.ToString();
			m_pdbVersion = peFile.PdbVersion;
			m_pdbAge = peFile.PdbAge;
		}

		#region ///// PE /////
		private string m_fileName;
		private string m_description;
		private string m_version;
		private string m_size;
		private string m_lastWriteTimeUtc;

		[XmlElement("FileName")]
		public string FileName
		{
			get { return m_fileName; }
			set { m_fileName = value; }
		}

		[XmlElement("Description")]
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		[XmlElement("Version")]
		public string Version
		{
			get { return m_version; }
			set { m_version = value; }
		}

		[XmlElement("Size")]
		public string Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		[XmlElement("LastWriteTimeUtc")]
		public string LastWriteTimeUtc
		{
			get { return m_lastWriteTimeUtc; }
			set { m_lastWriteTimeUtc = value; }
		}
		#endregion

		#region ///// PDB /////
		private string m_pdbFileName;
		private string m_pdbGuid;
		private string m_pdbVersion;
		private string m_pdbAge;

		[XmlElement("PdbFileName")]
		public string PdbFileName
		{
			get { return m_pdbFileName; }
			set { m_pdbFileName = value; }
		}

		[XmlElement("PdbGuid")]
		public string PdbGuid
		{
			get { return m_pdbGuid; }
			set { m_pdbGuid = value; }
		}

		[XmlElement("PdbVersion")]
		public string PdbVersion
		{
			get { return m_pdbVersion; }
			set { m_pdbVersion = value; }
		}

		[XmlElement("PdbAge")]
		public string PdbAge
		{
			get { return m_pdbAge; }
			set { m_pdbAge = value; }
		}
		#endregion
	}

	[XmlRoot("List")]
	public class XmlPEFileList : SerializableItem
	{
		private ArrayList _items;

		public XmlPEFileList()
		{
			_items = new ArrayList();
		}

		[XmlElement("PEFile", typeof(XmlPEFileItem))]
		public ArrayList Items
		{
			get { return _items; }
			set { _items = value; }
		}
	}
	#endregion*/
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   