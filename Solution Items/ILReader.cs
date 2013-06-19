// Reflection based managed IL Reader
// Copyright (C) 2000-2002 Lutz Roeder. All rights reserved.
// http://www.aisto.com/roeder
// roeder@aisto.com
namespace System.Reflection.ILReader
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	public interface IAssemblyLoader
	{
		Assembly Load(string assemblyName);
		Assembly LoadFrom(string fileName);
	}

	// This code is intermediate until System.Reflection exposes its own IL reader.
	// Performance is very bad due to a non-linear walk to create MemberInfo mappings.
	// However, better than nothing.
	public class ILReader : IDisposable
	{
		Stream stream = null;
		BinaryReader reader = null;
		PEHeader header = null;
		MetaData metaData = null;

		public ILReader(Module module, IAssemblyLoader assemblyLoader)
		{
			if(module == null) throw new ArgumentNullException();
			String fileName = module.FullyQualifiedName;
			FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			this.reader = new BinaryReader(stream);
			this.header = new PEHeader(reader);
			reader.BaseStream.Position = header.MetaDataRoot;
			this.metaData = new MetaData(reader, module, assemblyLoader);
		}

		~ILReader()
		{
			Dispose();
		}

		public void Dispose()
		{
			if(stream != null) stream.Close();
			stream = null;
			reader = null;
			header = null;
			metaData = null;
		}

		public Module Module
		{
			get { return metaData.Module; }
		}

		public MethodBody GetMethodBody(MethodBase methodBase)
		{
			if((metaData == null) || (this.reader == null))
				throw new Exception("Invalid call after dispose.");

			// Native, Runtime, OPTIL can not be disassembled
			MethodImplAttributes attributes = methodBase.GetMethodImplementationFlags();
			if((attributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Native) return null;
			if((attributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.Runtime) return null;
			if((attributes & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.OPTIL) return null;

			// Seek RVA address for method body
			uint rva = metaData.RvaFromMethodBase(methodBase);
			if(rva == 0) return null;
			this.reader.BaseStream.Position = header.RvaToVa(rva);

			// Read method body
			MethodReader method = new MethodReader(this.reader);

			// Decode IL instructions
			ArrayList list = new ArrayList();
			if(method.Code.Length != 0)
			{
				MemoryStream stream = new MemoryStream(method.Code);
				BinaryReader reader = new BinaryReader(stream);
				ParameterInfo[] parameters = methodBase.GetParameters();
				while(reader.BaseStream.Position < reader.BaseStream.Length)
				{
					Instruction instruction = metaData.DecodeInstruction(reader);

					// Turn arg operand into parameter info.
					String name = instruction.OpCode.Name;
					if((name == "ldarg.s") || (name == "ldarga.s") || (name == "starg.s") || (name == "ldarg") || (name == "ldarga") || (name == "starg"))
					{
						int operand = (int)instruction.Operand;

						if((!methodBase.IsStatic) && (operand == 0))
						{
							// Some COM events seem to use ldarg 0 instead of ldarg.0 so we have to fake a "this" parameter to keep the API consistent.
							instruction.Operand = new ThisParameterInfo(methodBase.DeclaringType);
						} else
						{
							if(!methodBase.IsStatic) operand--;
							if((operand < 0) || ((parameters != null) && (operand >= parameters.Length))) throw new ILReaderException("Argument index out of range.");
							instruction.Operand = parameters[operand];
						}
					}

					list.Add(instruction);
				}
			}

			// Decode exception handler clauses
			ExceptionHandler[] exceptions = null;
			Clause[] clauses = method.Clauses;
			if(clauses != null)
			{
				exceptions = new ExceptionHandler[clauses.Length];
				for(int i = 0;i < clauses.Length;i++)
				{
					exceptions[i] = null;
					Block tryBlock = new Block();
					tryBlock.Offset = clauses[i].TryOffset;
					tryBlock.Length = clauses[i].TryLength;
					Block handlerBlock = new Block();
					handlerBlock.Offset = clauses[i].HandlerOffset;
					handlerBlock.Length = clauses[i].HandlerLength;
					if((clauses[i].Flags & 0x07) == 0x00)
						exceptions[i] = new Catch(tryBlock, handlerBlock, (Type)metaData.TypeFromTypeDefOrRefToken((int)clauses[i].Value));
					else if((clauses[i].Flags & 0x07) == 0x01)
						exceptions[i] = new Filter(tryBlock, handlerBlock, clauses[i].Value);
					else if((clauses[i].Flags & 0x07) == 0x02)
						exceptions[i] = new Finally(tryBlock, handlerBlock);
					else if((clauses[i].Flags & 0x07) == 0x04)
						exceptions[i] = new Fault(tryBlock, handlerBlock);
					else
						new ILReaderException("Unknown exception handler type.");
				}
			}

			Type[] locals = metaData.DecodeLocalVariableSignature(method.LocalVarSigToken);
			Instruction[] instructions = (Instruction[])list.ToArray(typeof(Instruction));
			return new MethodBody(method.Code.Length, method.MaxStack, locals, exceptions, instructions);
		}

		class ThisParameterInfo : ParameterInfo
		{
			Type parameterType;

			public ThisParameterInfo(Type parameterType)
			{
				this.parameterType = parameterType;
			}

			public override Type ParameterType
			{
				get { return parameterType; }
			}

			public override String Name
			{
				get { return "0"; }
			}

			public override int Position
			{
				get { return -1; }
			}
		}

		class MethodReader
		{
			ushort maxStack = 0;
			int localVarSigToken = 0;
			Byte[] code = new Byte[0];
			Section section = null;

			public MethodReader(BinaryReader reader)
			{
				Byte first = reader.ReadByte();

				if((first & Format.Mask) == Format.Tiny)
				{
					maxStack = 8;
					int codeSize = first >> 2;
					code = reader.ReadBytes(codeSize);
				} else if((first & Format.Mask) == Format.Fat)
				{
					Byte second = reader.ReadByte();
					Byte headerSize = (Byte)(second >> 4);
					ushort flags = (ushort)(first | ((second & 0x0F) << 8));
					maxStack = reader.ReadUInt16();
					int codeSize = reader.ReadInt32();
					localVarSigToken = reader.ReadInt32();
					code = reader.ReadBytes(codeSize);

					if((flags & Flags.MoreSects) != 0)
						section = new Section(reader);
				} else
					throw new ILReaderException("Unknown method header format.");
			}

			public ushort MaxStack
			{
				get { return maxStack; }
			}

			public int LocalVarSigToken
			{
				get { return localVarSigToken; }
			}

			public Byte[] Code
			{
				get { return code; }
			}

			public Clause[] Clauses
			{
				get
				{
					ArrayList list = new ArrayList();
					Section current = section;
					while(current != null)
					{
						list.AddRange(current.Clauses);
						current = current.Next;
					}
					return (Clause[])list.ToArray(typeof(Clause));
				}
			}

			struct Format
			{
				public const Byte Tiny = 2;
				public const Byte Fat = 3;
				public const Byte Mask = 3;
			}

			struct Flags
			{
				public const ushort InitLocals = 0x10;
				public const ushort MoreSects = 0x08;
			}

			class Section
			{
				Section next = null;
				Clause[] clauses = new Clause[0];

				public Section(BinaryReader reader)
				{
					if((reader.BaseStream.Position % 4) != 0) reader.BaseStream.Position += (4 - (reader.BaseStream.Position % 4));
					Byte flags = reader.ReadByte();

					if((flags & Flags.FatFormat) == 0)
					{
						Byte size = reader.ReadByte();
						reader.ReadBytes(2);
						clauses = new Clause[size / 12];
						for(int i = 0;i < clauses.Length;i++)
						{
							clauses[i] = new Clause();
							clauses[i].Flags = reader.ReadInt16();
							clauses[i].TryOffset = reader.ReadInt16();
							clauses[i].TryLength = reader.ReadByte();
							clauses[i].HandlerOffset = reader.ReadInt16();
							clauses[i].HandlerLength = reader.ReadByte();
							clauses[i].Value = reader.ReadInt32();
						}
					} else
					{
						Byte[] data = reader.ReadBytes(3);
						int size = (int)((data[2] << 12) | (data[1] << 8) | (data[0]));
						if((flags & Flags.EHTable) == 0)
						{
							reader.ReadBytes(size);
						} else
						{
							clauses = new Clause[size / 24];
							for(int i = 0;i < clauses.Length;i++)
							{
								clauses[i] = new Clause();
								clauses[i].Flags = reader.ReadInt32();
								clauses[i].TryOffset = reader.ReadInt32();
								clauses[i].TryLength = reader.ReadInt32();
								clauses[i].HandlerOffset = reader.ReadInt32();
								clauses[i].HandlerLength = reader.ReadInt32();
								clauses[i].Value = reader.ReadInt32();
							}
						}
					}

					if((flags & Flags.MoreSects) != 0)
						next = new Section(reader);
				}

				public Section Next
				{
					get { return next; }
				}

				public Clause[] Clauses
				{
					get { return clauses; }
				}

				struct Flags
				{
					public const ushort EHTable = 0x01;
					public const ushort OptILTable = 0x02;
					public const ushort FatFormat = 0x40;
					public const ushort MoreSects = 0x80;
				}
			}
		}

		struct Clause
		{
			public int Flags;
			public int TryOffset;
			public int TryLength;
			public int HandlerOffset;
			public int HandlerLength;
			public int Value;
		}

		class PEHeader
		{
			IMAGE_SECTION_HEADER[] sectionHeaders;
			IMAGE_COR20_HEADER corHeader;

			public PEHeader(BinaryReader reader)
			{
				// Skip DOS Header and seek to PE signature
				if(reader.ReadUInt16() != 0x5A4D) throw new ILReaderException("Invalid DOS header.");
				reader.ReadBytes(58);
				reader.BaseStream.Position = reader.ReadUInt32();

				// Read "PE\0\0" signature
				if(reader.ReadUInt32() != 0x00004550) throw new ILReaderException("File is not a portable executable.");

				// Read COFF header
				IMAGE_COFF_HEADER coffHeader = new IMAGE_COFF_HEADER();
				coffHeader.Machine = reader.ReadUInt16();
				coffHeader.NumberOfSections = reader.ReadUInt16();
				coffHeader.TimeDateStamp = reader.ReadUInt32();
				coffHeader.SymbolTablePointer = reader.ReadUInt32();
				coffHeader.NumberOfSymbols = reader.ReadUInt32();
				coffHeader.OptionalHeaderSize = reader.ReadUInt16();
				coffHeader.Characteristics = reader.ReadUInt16();

				// Compute data sections offset
				long dataSectionsOffset = reader.BaseStream.Position + coffHeader.OptionalHeaderSize;

				// Skip Standard fields
				reader.ReadBytes(28);

				// Read NT-specific fields
				IMAGE_OPTIONAL_HEADER_NT ntHeader = new IMAGE_OPTIONAL_HEADER_NT();
				ntHeader.ImageBase = reader.ReadUInt32();
				ntHeader.SectionAlignment = reader.ReadUInt32();
				ntHeader.FileAlignment = reader.ReadUInt32();
				ntHeader.OsMajor = reader.ReadUInt16();
				ntHeader.OsMinor = reader.ReadUInt16();
				ntHeader.UserMajor = reader.ReadUInt16();
				ntHeader.UserMinor = reader.ReadUInt16();
				ntHeader.SubSysMajor = reader.ReadUInt16();
				ntHeader.SubSysMinor = reader.ReadUInt16();
				ntHeader.Reserved = reader.ReadUInt32();
				ntHeader.ImageSize = reader.ReadUInt32();
				ntHeader.HeaderSize = reader.ReadUInt32();
				ntHeader.FileChecksum = reader.ReadUInt32();
				ntHeader.SubSystem = reader.ReadUInt16();
				ntHeader.DllFlags = reader.ReadUInt16();
				ntHeader.StackReserveSize = reader.ReadUInt32();
				ntHeader.StackCommitSize = reader.ReadUInt32();
				ntHeader.HeapReserveSize = reader.ReadUInt32();
				ntHeader.HeapCommitSize = reader.ReadUInt32();
				ntHeader.LoaderFlags = reader.ReadUInt32();
				ntHeader.NumberOfDataDirectories = reader.ReadUInt32();
				if(ntHeader.NumberOfDataDirectories < 16) throw new ILReaderException("Invalid number of data directories in file header.");

				// Read data directories
				IMAGE_DATA_DIRECTORY exportTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY importTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY resourceTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY exceptionTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY certificateTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY baseRelocationTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY debug = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY copyright = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY globalPtr = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY tlsTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY loadConfigTable = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY boundImport = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY iat = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY delayImportDescriptor = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY runtimeHeader = ReadDataDirectory(reader);
				IMAGE_DATA_DIRECTORY reserved = ReadDataDirectory(reader);

				// Read data sections
				reader.BaseStream.Position = dataSectionsOffset;
				sectionHeaders = new IMAGE_SECTION_HEADER[coffHeader.NumberOfSections];
				for(int i = 0;i < sectionHeaders.Length;i++)
				{
					reader.ReadBytes(12);
					sectionHeaders[i].VirtualAddress = (uint)reader.ReadUInt32();
					sectionHeaders[i].SizeOfRawData = (uint)reader.ReadUInt32();
					sectionHeaders[i].PointerToRawData = (uint)reader.ReadUInt32();
					reader.ReadBytes(16);
				}

				// Read COR20 Header
				reader.BaseStream.Position = RvaToVa(runtimeHeader.Rva);
				corHeader = new IMAGE_COR20_HEADER();
				corHeader.Size = reader.ReadUInt32();
				corHeader.MajorRuntimeVersion = reader.ReadUInt16();
				corHeader.MinorRuntimeVersion = reader.ReadUInt16();
				corHeader.MetaData = ReadDataDirectory(reader);
				corHeader.Flags = reader.ReadUInt32();
				corHeader.EntryPointToken = reader.ReadUInt32();
				corHeader.Resources = ReadDataDirectory(reader);
				corHeader.StrongNameSignature = ReadDataDirectory(reader);
				corHeader.CodeManagerTable = ReadDataDirectory(reader);
				corHeader.VTableFixups = ReadDataDirectory(reader);
				corHeader.ExportAddressTableJumps = ReadDataDirectory(reader);
			}

			IMAGE_DATA_DIRECTORY ReadDataDirectory(BinaryReader reader)
			{
				IMAGE_DATA_DIRECTORY directory = new IMAGE_DATA_DIRECTORY();
				directory.Rva = reader.ReadUInt32();
				directory.Size = reader.ReadUInt32();
				return directory;
			}

			public long RvaToVa(long rva)
			{
				for(int i = 0;i < sectionHeaders.Length;i++)
					if((sectionHeaders[i].VirtualAddress <= rva) && (sectionHeaders[i].VirtualAddress + sectionHeaders[i].SizeOfRawData > rva))
						return (sectionHeaders[i].PointerToRawData + (rva - sectionHeaders[i].VirtualAddress));

				throw new ILReaderException("Invalid RVA address.");
			}

			public long MetaDataRoot
			{
				get { return RvaToVa(corHeader.MetaData.Rva); }
			}

			struct IMAGE_COFF_HEADER
			{
				public ushort Machine;
				public ushort NumberOfSections;
				public uint TimeDateStamp;
				public uint SymbolTablePointer;
				public uint NumberOfSymbols;
				public ushort OptionalHeaderSize;
				public ushort Characteristics;
			}

			struct IMAGE_OPTIONAL_HEADER_NT
			{
				public uint ImageBase;
				public uint SectionAlignment;
				public uint FileAlignment;
				public ushort OsMajor;
				public ushort OsMinor;
				public ushort UserMajor;
				public ushort UserMinor;
				public ushort SubSysMajor;
				public ushort SubSysMinor;
				public uint Reserved;
				public uint ImageSize;
				public uint HeaderSize;
				public uint FileChecksum;
				public ushort SubSystem;
				public ushort DllFlags;
				public uint StackReserveSize;
				public uint StackCommitSize;
				public uint HeapReserveSize;
				public uint HeapCommitSize;
				public uint LoaderFlags;
				public uint NumberOfDataDirectories;
			}

			struct IMAGE_DATA_DIRECTORY
			{
				public uint Rva;
				public uint Size;
			}

			struct IMAGE_SECTION_HEADER
			{
				public uint VirtualAddress;
				public uint SizeOfRawData;
				public uint PointerToRawData;
			}

			struct IMAGE_COR20_HEADER
			{
				public uint Size;
				public ushort MajorRuntimeVersion;
				public ushort MinorRuntimeVersion;
				public IMAGE_DATA_DIRECTORY MetaData;
				public uint Flags;
				public uint EntryPointToken;
				public IMAGE_DATA_DIRECTORY Resources;
				public IMAGE_DATA_DIRECTORY StrongNameSignature;
				public IMAGE_DATA_DIRECTORY CodeManagerTable;
				public IMAGE_DATA_DIRECTORY VTableFixups;
				public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
			}
		}

		class MetaData
		{
			static Hashtable categories = new Hashtable();
			Hashtable opCodes = new Hashtable();
			BinaryReader reader = null;
			Module module = null;
			IAssemblyLoader assemblyLoader = null;
			long tables = 0;
			long strings = 0;
			long blobs = 0;
			long userStrings = 0;
			long guids = 0;
			TokenEncoding tokenEncoding = TokenEncoding.ReadOnly;
			TableHeader tableHeader;
			Table[] tableData = new Table[64];
			Hashtable stringCache = new Hashtable();
			Hashtable memberInfoCache = new Hashtable();

			public MetaData(BinaryReader reader, Module module, IAssemblyLoader assemblyLoader)
			{
				categories[Types.TypeDefOrRef] = new Types[] { Types.TypeDef, Types.TypeRef, Types.TypeSpec };
				categories[Types.HasConstant] = new Types[] { Types.FieldDef, Types.ParamDef, Types.PropertyDef };
				categories[Types.CustomAttributeType] = new Types[] { Types.TypeRef, Types.TypeDef, Types.MethodDef, Types.MemberRef, Types.UserString };
				categories[Types.HasSemantic] = new Types[] { Types.EventDef, Types.PropertyDef };
				categories[Types.ResolutionScope] = new Types[] { Types.ModuleDef, Types.ModuleRef, Types.AssemblyRef, Types.TypeRef };
				categories[Types.HasFieldMarshal] = new Types[] { Types.FieldDef, Types.ParamDef };
				categories[Types.HasDeclSecurity] = new Types[] { Types.TypeDef, Types.MethodDef, Types.AssemblyDef };
				categories[Types.MemberRefParent] = new Types[] { Types.TypeDef, Types.TypeRef, Types.ModuleRef, Types.MethodDef, Types.TypeSpec };
				categories[Types.MethodDefOrRef] = new Types[] { Types.MethodDef, Types.MemberRef };
				categories[Types.MemberForwarded] = new Types[] { Types.FieldDef, Types.MethodDef };
				categories[Types.Implementation] = new Types[] { Types.File, Types.AssemblyRef, Types.ExportedType };
				categories[Types.HasCustomAttribute] = new Types[] { Types.MethodDef, Types.FieldDef, Types.TypeRef, Types.TypeDef, Types.ParamDef, Types.InterfaceImpl, Types.MemberRef, Types.ModuleDef, Types.Permission, Types.PropertyDef, Types.EventDef, Types.Signature, Types.ModuleRef, Types.TypeSpec, Types.AssemblyDef, Types.AssemblyRef, Types.File, Types.ExportedType, Types.ManifestResource };

				FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				foreach(FieldInfo fieldInfo in fields)
					if(fieldInfo.FieldType == typeof(OpCode))
					{
						OpCode opCode = (OpCode)fieldInfo.GetValue(null);
						opCodes[(ushort)opCode.Value] = opCode;
					}

				opCodes[(ushort)0xFE00] = OpCodes.Arglist;
				opCodes[(ushort)0xFE01] = OpCodes.Ceq;
				opCodes[(ushort)0xFE02] = OpCodes.Cgt;
				opCodes[(ushort)0xFE03] = OpCodes.Cgt_Un;
				opCodes[(ushort)0xFE04] = OpCodes.Clt;
				opCodes[(ushort)0xFE05] = OpCodes.Clt_Un;
				opCodes[(ushort)0xFE06] = OpCodes.Ldftn;
				opCodes[(ushort)0xFE07] = OpCodes.Ldvirtftn;
				opCodes[(ushort)0xFE09] = OpCodes.Ldarg;
				opCodes[(ushort)0xFE0A] = OpCodes.Ldarga;
				opCodes[(ushort)0xFE0B] = OpCodes.Starg;
				opCodes[(ushort)0xFE0C] = OpCodes.Ldloc;
				opCodes[(ushort)0xFE0D] = OpCodes.Ldloca;
				opCodes[(ushort)0xFE0E] = OpCodes.Stloc;
				opCodes[(ushort)0xFE0F] = OpCodes.Localloc;
				opCodes[(ushort)0xFE11] = OpCodes.Endfilter;
				opCodes[(ushort)0xFE12] = OpCodes.Unaligned;
				opCodes[(ushort)0xFE13] = OpCodes.Volatile;
				opCodes[(ushort)0xFE14] = OpCodes.Tailcall;
				opCodes[(ushort)0xFE15] = OpCodes.Initobj;
				opCodes[(ushort)0xFE17] = OpCodes.Cpblk;
				opCodes[(ushort)0xFE18] = OpCodes.Initblk;
				opCodes[(ushort)0xFE1A] = OpCodes.Rethrow;
				opCodes[(ushort)0xFE1C] = OpCodes.Sizeof;
				opCodes[(ushort)0xFE1D] = OpCodes.Refanytype;

				this.reader = reader;
				this.module = module;
				this.assemblyLoader = assemblyLoader;

				long baseOffset = reader.BaseStream.Position;

				// Read meta-data root
				uint signature = reader.ReadUInt32();
				if(signature != 0x424A5342) throw new ILReaderException("Incorrect meta-data signature.");
				ushort majorVersion = reader.ReadUInt16();
				ushort minorVersion = reader.ReadUInt16();
				reader.ReadUInt32();
				int versionLength = (int)reader.ReadUInt32();
				if((versionLength % 4) != 0) versionLength += 4 - (versionLength % 4);
				reader.ReadBytes(versionLength);

				// Read meta-data stream headers
				ushort flags = reader.ReadUInt16();
				ushort numberOfStreams = reader.ReadUInt16();
				for(int i = 0;i < numberOfStreams;i++)
				{
					uint offset = reader.ReadUInt32();
					uint size = reader.ReadUInt32();
					char[] chars = new char[32];
					int index = 0;
					byte character = 0;
					while((character = reader.ReadByte()) != 0)
						chars[index++] = (char)character;

					index++;
					int padding = ((index % 4) != 0) ? (4 - (index % 4)) : 0;
					reader.ReadBytes(padding);

					String name = new String(chars).Trim(new Char[] { '\0' });

					if(name == "#Strings") strings = baseOffset + offset;
					if(name == "#Blob") blobs = baseOffset + offset;
					if(name == "#US") userStrings = baseOffset + offset;
					if(name == "#GUID") guids = baseOffset + offset;

					if(name == "#~")
					{
						tables = baseOffset + offset;
						tokenEncoding = TokenEncoding.ReadOnly;
					}

					if(name == "#-")
					{
						tables = baseOffset + offset;
						tokenEncoding = TokenEncoding.ReadWrite;
					}
				}

				ReadTableStream();
			}

			void ReadTableStream()
			{
				if(tables == 0) throw new ILReaderException("Unable to read meta-data tables.");
				reader.BaseStream.Position = tables;

				// Create tables and assign schemas
				tableData = new Table[64];
				tableData[0x00] = new Table(Types.ModuleDef, new Types[] { Types.UInt16, Types.String, Types.Guid, Types.Guid, Types.Guid }, new String[] { "Generation", "Name", "Mvid", "EncId", "EncBaseId" }, this);
				tableData[0x01] = new Table(Types.TypeRef, new Types[] { Types.ResolutionScope, Types.String, Types.String }, new String[] { "ResolutionScope", "Name", "Namespace" }, this);
				tableData[0x02] = new Table(Types.TypeDef, new Types[] { Types.UInt32, Types.String, Types.String, Types.TypeDefOrRef, Types.FieldDef, Types.MethodDef }, new String[] { "Flags", "Name", "Namespace", "Extends", "FieldList", "MethodList" }, this);
				tableData[0x03] = new Table(Types.FieldPtr, new Types[] { Types.FieldDef }, new String[] { "Field" }, this);
				tableData[0x04] = new Table(Types.FieldDef, new Types[] { Types.UInt16, Types.String, Types.Blob }, new String[] { "Flags", "Name", "Signature" }, this);
				tableData[0x05] = new Table(Types.MethodPtr, new Types[] { Types.MethodDef }, new String[] { "Method" }, this);
				tableData[0x06] = new Table(Types.MethodDef, new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.String, Types.Blob, Types.ParamDef }, new String[] { "RVA", "ImplFlags", "Flags", "Name", "Signature", "ParamList" }, this);
				tableData[0x07] = new Table(Types.ParamPtr, new Types[] { Types.ParamDef }, new String[] { "Param" }, this);
				tableData[0x08] = new Table(Types.ParamDef, new Types[] { Types.UInt16, Types.UInt16, Types.String }, new String[] { "Flags", "Sequence", "Name" }, this);
				tableData[0x09] = new Table(Types.InterfaceImpl, new Types[] { Types.TypeDef, Types.TypeDefOrRef }, new String[] { "Class", "Interface" }, this);
				tableData[0x0A] = new Table(Types.MemberRef, new Types[] { Types.MemberRefParent, Types.String, Types.Blob }, new String[] { "Class", "Name", "Signature" }, this);
				tableData[0x0B] = new Table(Types.Constant, new Types[] { Types.UInt16, Types.HasConstant, Types.Blob }, new String[] { "Type", "Parent", "Value" }, this);
				tableData[0x0C] = new Table(Types.CustomAttribute, new Types[] { Types.HasCustomAttribute, Types.CustomAttributeType, Types.Blob }, new String[] { "Type", "Parent", "Value" }, this);
				tableData[0x0D] = new Table(Types.FieldMarshal, new Types[] { Types.HasFieldMarshal, Types.Blob }, new String[] { "Parent", "Native" }, this);
				tableData[0x0E] = new Table(Types.Permission, new Types[] { Types.UInt16, Types.HasDeclSecurity, Types.Blob }, new String[] { "Action", "Parent", "PermissionSet" }, this);
				tableData[0x0F] = new Table(Types.ClassLayout, new Types[] { Types.UInt16, Types.UInt32, Types.TypeDef }, new String[] { "PackingSize", "ClassSize", "Parent" }, this);
				tableData[0x10] = new Table(Types.FieldLayout, new Types[] { Types.UInt32, Types.FieldDef }, new String[] { "Offset", "Field" }, this);
				tableData[0x11] = new Table(Types.Signature, new Types[] { Types.Blob }, new String[] { "Signature" }, this);
				tableData[0x12] = new Table(Types.EventMap, new Types[] { Types.TypeDef, Types.EventDef }, new String[] { "Parent", "EventList" }, this);
				tableData[0x13] = new Table(Types.EventPtr, new Types[] { Types.EventDef }, new String[] { "Event" }, this);
				tableData[0x14] = new Table(Types.EventDef, new Types[] { Types.UInt16, Types.String, Types.TypeDefOrRef }, new String[] { "EventFlags", "Name", "EventType" }, this);
				tableData[0x15] = new Table(Types.PropertyMap, new Types[] { Types.TypeDef, Types.PropertyDef }, new String[] { "Parent", "PropertyList" }, this);
				tableData[0x16] = new Table(Types.PropertyPtr, new Types[] { Types.PropertyDef }, new String[] { "Property" }, this);
				tableData[0x17] = new Table(Types.PropertyDef, new Types[] { Types.UInt16, Types.String, Types.Blob }, new String[] { "PropFlags", "Name", "Type" }, this);
				tableData[0x18] = new Table(Types.MethodSemantics, new Types[] { Types.UInt16, Types.MethodDef, Types.HasSemantic }, new String[] { "Semantic", "Method", "Association" }, this);
				tableData[0x19] = new Table(Types.MethodImpl, new Types[] { Types.TypeDef, Types.MethodDefOrRef, Types.MethodDefOrRef }, new String[] { "Class", "MethodBody", "MethodDeclaration" }, this);
				tableData[0x1A] = new Table(Types.ModuleRef, new Types[] { Types.String }, new String[] { "Name" }, this);
				tableData[0x1B] = new Table(Types.TypeSpec, new Types[] { Types.Blob }, new String[] { "Signature" }, this);
				tableData[0x1C] = new Table(Types.ImplMap, new Types[] { Types.UInt16, Types.MemberForwarded, Types.String, Types.ModuleRef }, new String[] { "MappingFlags", "MemberForwarded", "ImportName", "ImportScope" }, this);
				tableData[0x1D] = new Table(Types.FieldRVA, new Types[] { Types.UInt32, Types.FieldDef }, new String[] { "RVA", "Field" }, this);
				tableData[0x1E] = new Table(Types.ENCLog, new Types[] { Types.UInt32, Types.UInt32 }, new String[] { "Token", "FuncCode" }, this);
				tableData[0x1F] = new Table(Types.ENCMap, new Types[] { Types.UInt32 }, new String[] { "Token" }, this);
				tableData[0x20] = new Table(Types.AssemblyDef, new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String }, new String[] { "HashAlgId", "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKey", "Name", "Locale" }, this);
				tableData[0x21] = new Table(Types.AssemblyProcessor, new Types[] { Types.UInt32 }, new String[] { "Processor" }, this);
				tableData[0x22] = new Table(Types.AssemblyOS, new Types[] { Types.UInt32, Types.UInt32, Types.UInt32 }, new String[] { "OSPlatformId", "MajorVersion", "MinorVersion" }, this);
				tableData[0x23] = new Table(Types.AssemblyRef, new Types[] { Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String, Types.Blob }, new String[] { "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKeyOrToken", "Name", "Locale", "HashValue" }, this);
				tableData[0x24] = new Table(Types.AssemblyRefProcessor, new Types[] { Types.UInt32, Types.AssemblyRef }, new String[] { "Processor", "AssemblyRef" }, this);
				tableData[0x25] = new Table(Types.AssemblyRefOS, new Types[] { Types.UInt32, Types.UInt32, Types.UInt32, Types.AssemblyRef }, new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", "AssemblyRef" }, this);
				tableData[0x26] = new Table(Types.File, new Types[] { Types.UInt32, Types.String, Types.Blob }, new String[] { "Flags", "Name", "HashValue" }, this);
				tableData[0x27] = new Table(Types.ExportedType, new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.String, Types.Implementation }, new String[] { "Flags", "TypeDefId", "TypeName", "TypeNamespace", "TypeImplementation" }, this);
				tableData[0x28] = new Table(Types.ManifestResource, new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.Implementation }, new String[] { "Offset", "Flags", "Name", "Implementation" }, this);
				tableData[0x29] = new Table(Types.NestedClass, new Types[] { Types.TypeDef, Types.TypeDef }, new String[] { "Nested", "Enclosing" }, this);
				tableData[0x2A] = new Table(Types.TypeTyPar, new Types[] { Types.UInt16, Types.TypeDef, Types.TypeDefOrRef, Types.String }, new String[] { "Number", "Class", "Bound", "Name" }, this);
				tableData[0x2B] = new Table(Types.MethodTyPar, new Types[] { Types.UInt16, Types.MethodDef, Types.TypeDefOrRef, Types.String }, new String[] { "Number", "Method", "Bound", "Name" }, this);

				// Read tables
				tableHeader = new TableHeader();
				tableHeader.Reserved = reader.ReadUInt32();
				tableHeader.MajorVersion = reader.ReadByte(); // major
				tableHeader.MinorVersion = reader.ReadByte(); // minor
				tableHeader.HeapSizes = reader.ReadByte();
				tableHeader.Padding = reader.ReadByte();
				tableHeader.Valid = reader.ReadUInt64();
				tableHeader.Sorted = reader.ReadUInt64();

				for(int i = 0;i < tableData.Length;i++)
				{
					int count = (((tableHeader.Valid >> i) & 1) == 0) ? 0 : reader.ReadInt32();
					if(tableData[i] != null) tableData[i].Count = count;
				}

				for(int i = 0;i < tableData.Length;i++)
					if(tableData[i] != null)
						tableData[i].Fill(reader);
			}

			public Module Module
			{
				get { return module; }
			}

			public uint RvaFromMethodBase(MethodBase methodBase)
			{
				int token = MethodDefTokenFromMethodBase(methodBase);
				Row row = ToRow(DecodeToken(token));
				if(row == null) throw new ILReaderException("Invalid row in meta-data table.");
				return row["RVA"].UInt32;
			}

			int MethodDefTokenFromMethodBase(MethodBase methodBase)
			{
				int typeDefToken = ToToken(Types.TypeDef, 0);
				Type declaringType = methodBase.DeclaringType;
				if(declaringType != null) typeDefToken = TypeDefTokenFromType(declaringType);

				Row typeDef = ToRow(typeDefToken);
				if(typeDef == null) throw new ILReaderException("Unable to resolve row in method declaration table.");

				Table typeDefTable = GetTable(Types.TypeDef);
				Table methodDefTable = GetTable(Types.MethodDef);

				int startToken = typeDef["MethodList"].Token;
				int endToken = ToToken(Types.MethodDef, methodDefTable.Count);

				int typeDefMaxToken = ToToken(typeDefTable.Type, typeDefTable.Count);
				int typeDefNextToken = typeDefToken + 1;
				if(typeDefNextToken < typeDefMaxToken)
				{
					Row typeDefNextRow = ToRow(typeDefNextToken);
					if(typeDefNextRow == null) throw new ILReaderException("Unable to find meta-data for method type.");
					endToken = typeDefNextRow["MethodList"].Token;
				}

				for(int token = startToken;token < endToken;token++)
				{
					Row row = ToRow(DecodeToken(token));
					if(row == null) throw new ILReaderException("Invalid row in method table.");
					if(methodBase.Name == row["Name"].String)
						if(MemberInfoSignatureCheck(methodBase, row["Signature"].Blob))
							return token;
				}

				throw new ILReaderException("Unable to find meta-data for this method.");
			}

			int TypeDefTokenFromType(Type type)
			{
				// Lookup declaring type
				Type declaringType = type.DeclaringType;
				int declaringTypeToken = -1;
				if(declaringType != null) declaringTypeToken = TypeDefTokenFromType(declaringType);

				String typeNamespace = ((type.DeclaringType != null) || (type.Namespace == null)) ? String.Empty : type.Namespace;
				String typeName = type.Name;

				Table table = GetTable(Types.TypeDef);
				for(int index = 0;index < table.Count;index++)
				{
					int token = ToToken(table.Type, index);
					Row row = table[index];
					if((row["Namespace"].String == typeNamespace) && (row["Name"].String == typeName))
					{
						if(declaringTypeToken == -1) return token;
						foreach(Row nested in GetTable(Types.NestedClass))
							if((nested["Enclosing"].Token == declaringTypeToken) && (nested["Nested"].Token == token))
								return token;
					}
				}

				throw new ILReaderException("Unable to find meta-data for " + type.FullName + ".");
			}

			Assembly AssemblyFromAssemblyRefToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in assembly reference table.");

				StringBuilder builder = new StringBuilder();
				builder.Append(row["Name"].String);
				builder.Append(", ");
				builder.Append("Version=" + row["MajorVersion"].UInt16 + "." + row["MinorVersion"].UInt16 + "." + row["BuildNumber"].UInt16 + "." + row["RevisionNumber"].UInt16);

				String locale = row["Locale"].String;
				if(locale == String.Empty) locale = "neutral";
				builder.Append(", ");
				builder.Append("Culture=" + locale);

				Byte[] publicKeyToken = row["PublicKeyOrToken"].Blob;
				if(publicKeyToken != null && publicKeyToken.Length != 0)
				{
					String publicKeyString = String.Empty;
					foreach(Byte b in publicKeyToken) publicKeyString += b.ToString("x2");
					builder.Append(", ");
					builder.Append("PublicKeyToken=" + publicKeyString);
				}

				return assemblyLoader.Load(builder.ToString());
			}

			Module ModuleFromModuleRefToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in module reference table.");
				String moduleName = row["Name"].String.ToLower(); ;
				foreach(Module module in Module.Assembly.GetModules())
					if(module.Name == moduleName)
						return module;

				throw new ILReaderException("Module reference cannot be found.");
			}

			Type TypeFromTypeDefToken(int token)
			{
				if(token == ToToken(Types.TypeDef, 0)) return null;

				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in type declaration table.");

				String typeNamespace = row["Namespace"].String;
				String typeName = row["Name"].String;

				Type declaringType = null;
				foreach(Row nested in GetTable(Types.NestedClass))
					if(nested["Nested"].Token == token)
					{
						declaringType = TypeFromTypeDefToken(nested["Enclosing"].Token);
						if(declaringType == null) throw new ILReaderException("Unable to find meta-data for declaring type.");
						break;
					}

				// Find type in module
				foreach(Type type in module.GetTypes())
				{
					String currentNamespace = ((type.DeclaringType != null) || (type.Namespace == null)) ? String.Empty : type.Namespace;
					String currentName = type.Name;
					if((currentNamespace == typeNamespace) && (currentName == typeName))
						if(type.DeclaringType == declaringType)
							return type;
				}

				throw new ILReaderException("Unable to resolve type definition.");
			}

			Type TypeFromTypeRefToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in type reference table.");

				String typeNamespace = (row["Namespace"].String == String.Empty) ? null : row["Namespace"].String;
				String typeName = row["Name"].String;
				String typeFullName = ((typeNamespace == null) ? String.Empty : (typeNamespace + ".")) + typeName;

				int scopeToken = row["ResolutionScope"].Token;
				Types scopeType = GetTable(scopeToken).Type;

				if(scopeType == Types.ModuleDef)
					throw new ILReaderException("Type references scoped in module definitions are not supported."); //TODO: More spec needed

				if(scopeType == Types.ModuleRef)
				{
					Module module = ModuleFromModuleRefToken(scopeToken);
					if(module == null) throw new ILReaderException("Unable to find module reference.");
					Type type = module.GetType(typeFullName);
					if(type == null) throw new ILReaderException("Unable to find type " + typeFullName + " in module.");
					return type;
				}

				if(scopeType == Types.AssemblyRef)
				{
					Assembly assembly = AssemblyFromAssemblyRefToken(scopeToken);
					if(assembly == null) throw new ILReaderException("Unable to find assembly reference.");
					Type type = assembly.GetType(typeFullName);
					if(type == null) throw new ILReaderException("Unable to find type " + typeFullName + " in assembly.");
					return type;
				}

				if(scopeType == Types.TypeRef)
				{
					Type declaringType = TypeFromTypeRefToken(scopeToken);
					if(declaringType == null) throw new ILReaderException("Unable to resolve declaring type of type reference.");
					Assembly assembly = declaringType.Assembly;
					foreach(Type type in assembly.GetTypes())
						if((type.DeclaringType == declaringType) && (type.Name == typeName))
							return type;
					throw new ILReaderException("Unable to resolve type reference.");
				}

				throw new ILReaderException("Type reference cannot be resolved.");
			}

			Hashtable assemblyTypeCache = new Hashtable();

			Type TypeFromTypeSpecToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in type signature table.");
				Byte[] signature = row["Signature"].Blob;
				if(signature == null) throw new ILReaderException("Invalid empty type signature.");
				MemoryStream stream = new MemoryStream(signature);
				BinaryReader reader = new BinaryReader(stream);
				Type type = DecodeTypeSignature(reader);
				if(type == null) throw new ILReaderException("Unable to decode type specification.");
				stream.Close();
				return type;
			}

			public Type TypeFromTypeDefOrRefToken(int token)
			{
				Type type = (Type)memberInfoCache[token];
				if(type != null) return type;
				if(token == ToToken(Types.TypeDef, 0)) return null;
				Types tableType = GetTable(token).Type;
				if(tableType == Types.TypeDef) type = TypeFromTypeDefToken(token);
				if(tableType == Types.TypeRef) type = TypeFromTypeRefToken(token);
				if(tableType == Types.TypeSpec) type = TypeFromTypeSpecToken(token);
				if(type == null) throw new ILReaderException("Type is not a definition, reference or specification.");
				memberInfoCache[token] = type;
				return type;
			}

			MethodBase MethodBaseFromMethodDefToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in method declaration table.");
				Type type = TypeFromMemberDefToken(token, "MethodList");
				MemberInfo memberInfo = MemberInfoFromMemberData(type, row["Name"].String, row["Signature"].Blob);
				return (memberInfo is MethodBase) ? ((MethodBase)memberInfo) : null;
			}

			FieldInfo FieldInfoFromFieldDefToken(int token)
			{
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Unable to resolve row in field declaration table.");
				Type type = TypeFromMemberDefToken(token, "FieldList");
				MemberInfo memberInfo = MemberInfoFromMemberData(type, row["Name"].String, row["Signature"].Blob);
				return (memberInfo is FieldInfo) ? ((FieldInfo)memberInfo) : null;
			}

			MemberInfo MemberInfoFromMemberRefToken(int token)
			{
				Row row = ToRow(token);
				int parentToken = row["Class"].Token;

				Types parentType = GetTable(parentToken).Type;

				if((parentType == Types.TypeDef) || (parentType == Types.TypeRef) || (parentType == Types.TypeSpec))
				{
					Type type = TypeFromTypeDefOrRefToken(parentToken);
					return MemberInfoFromMemberData(type, row["Name"].String, row["Signature"].Blob);
				}

				if(parentType == Types.ModuleDef)
					throw new ILReaderException("Module definition is not supported in member references."); //TODO: More spec needed

				if(parentType == Types.MethodDef)
					throw new ILReaderException("Method definition is not supported in member references."); //TODO: More spec needed

				throw new ILReaderException("Unknown parent in member reference.");
			}

			Type TypeFromMemberDefToken(int token, String attributeName)
			{
				token = EncodeToken(token);
				Table table = GetTable(Types.TypeDef);
				for(int i = 0;i < table.Count;i++)
				{
					int typeDefToken = ToToken(table.Type, i);
					Row row = ToRow(typeDefToken);
					if(row == null) throw new ILReaderException("Unable to resolve row in type declaration table.");
					if(token >= row[attributeName].UInt32)
					{
						if(i == (table.Count - 1)) return TypeFromTypeDefOrRefToken(typeDefToken);
						Row nextRow = ToRow(typeDefToken + 1);
						if(nextRow == null) throw new ILReaderException("Unable to resolve row in type declaration table.");
						if(token < nextRow[attributeName].UInt32) return TypeFromTypeDefOrRefToken(typeDefToken);
					}
				}

				throw new ILReaderException("Type definition cannot be resolved.");
			}

			MemberInfo MemberInfoFromMemberData(Type type, String name, Byte[] signature)
			{
				if((signature != null) && (signature.Length > 0) && (signature[0] == 0x06))
				{
					// Field
					if(type != null)
						return type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					else
						return module.GetField(name);
				} else
				{
					// Method	
					if(type != null)
					{
						foreach(MethodBase methodBase in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
							if((name == methodBase.Name) && (MemberInfoSignatureCheck(methodBase, signature)))
								return methodBase;

						foreach(MethodBase methodBase in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
							if((name == methodBase.Name) && (MemberInfoSignatureCheck(methodBase, signature)))
								return methodBase;
					} else
					{
						foreach(MethodBase methodBase in module.GetMethods())
							if((name == methodBase.Name) && (MemberInfoSignatureCheck(methodBase, signature)))
								return methodBase;
					}
				}

				// Recursive walk up the hierarchy
				if((type != null) && (type.BaseType != null))
					return MemberInfoFromMemberData(type.BaseType, name, signature);

				throw new ILReaderException("Member reference cannot be resolved.");
			}

			int DecodeSignatureToken(BinaryReader reader)
			{
				int codedToken = DecodeInt32(reader);
				Types[] category = (Types[])categories[Types.TypeDefOrRef];
				if(category == null) return -1;
				Types type = category[codedToken & 0x03];
				int index = (codedToken >> 2) - 1;
				if(index < 0) return -1;
				return ToToken(type, index);
			}

			Type DecodeTypeSignature(BinaryReader reader)
			{
				int elementType = DecodeInt32(reader);
				if(elementType == ElementType.Void) return typeof(void);
				if(elementType == ElementType.Boolean) return typeof(Boolean);
				if(elementType == ElementType.Char) return typeof(Char);
				if(elementType == ElementType.I1) return typeof(SByte);
				if(elementType == ElementType.U1) return typeof(Byte);
				if(elementType == ElementType.I2) return typeof(Int16);
				if(elementType == ElementType.U2) return typeof(UInt16);
				if(elementType == ElementType.I4) return typeof(Int32);
				if(elementType == ElementType.U4) return typeof(UInt32);
				if(elementType == ElementType.I8) return typeof(Int64);
				if(elementType == ElementType.U8) return typeof(UInt64);
				if(elementType == ElementType.R4) return typeof(Single);
				if(elementType == ElementType.R8) return typeof(Double);
				if(elementType == ElementType.String) return typeof(String);

				if(elementType == ElementType.Ptr)
				{
					Type type = DecodeTypeSignature(reader);
					if(type == null) throw new ILReaderException("Unable to decode type of 'Ptr' signature.");
					return type.Assembly.GetType(type.FullName + "*");
				}

				if(elementType == ElementType.ByRef)
				{
					Type type = DecodeTypeSignature(reader);
					if(type == null) throw new ILReaderException("Unable to decode type of 'ByRef' signature.");
					Type returnType = type.Assembly.GetType(type.FullName + "&");
					if(returnType == null) throw new ILReaderException("Unable to find type of 'ByRef' signature.");
					return returnType;
				}

				if((elementType == ElementType.ValueType) || (elementType == ElementType.Class))
				{
					int token = DecodeSignatureToken(reader);
					if(token == -1) throw new ILReaderException("Unable to decode value type or class signature.");
					return TypeFromTypeDefOrRefToken(token);
				}

				if(elementType == ElementType.Array)
				{
					Type type = DecodeTypeSignature(reader);
					if(type == null) throw new ILReaderException("Unable to decode type of array.");
					int rank = DecodeInt32(reader);
					int boundsCount = DecodeInt32(reader);
					int[] bounds = new int[boundsCount];
					for(int i = 0;i < boundsCount;i++) bounds[i] = DecodeInt32(reader);
					int lowerBoundsCount = DecodeInt32(reader);
					int[] lowerBounds = new int[lowerBoundsCount];
					for(int i = 0;i < lowerBoundsCount;i++) lowerBounds[i] = DecodeInt32(reader);
					StringBuilder builder = new StringBuilder();
					builder.Append(type.FullName);
					if(rank > 0)
					{
						builder.Append("[");
						for(int i = 0;i < rank;i++)
						{
							Boolean lowerBound = ((i < lowerBoundsCount) && (lowerBounds[i] > 0));
							Boolean bound = (i < boundsCount);
							if(lowerBound && bound) builder.Append(lowerBounds[i] + ".." + (lowerBounds[i] + bounds[i] - 1));
							if(lowerBound && !bound) builder.Append(lowerBounds[i] + "...");
							if(!lowerBound && bound) builder.Append(bounds[i] - 1);
							if(i < (rank - 1)) builder.Append(",");
						}
						builder.Append("]");
					}
					Type returnType = type.Assembly.GetType(builder.ToString());
					if(returnType == null) throw new ILReaderException("Unable to find array type in assembly.");
					return returnType;
				}

				if(elementType == ElementType.TypedReference) return typeof(TypedReference);

				if(elementType == ElementType.UIntPtr) return typeof(UIntPtr);
				if(elementType == ElementType.IntPtr) return typeof(IntPtr);

				if(elementType == ElementType.FnPtr)
				{
					// FnPtr is same as IntPtr when using System.Reflection
					MethodBaseSignatureCheck(null, reader);
					return typeof(IntPtr);
				}

				if(elementType == ElementType.Object) return typeof(Object);

				if(elementType == ElementType.SzArray)
				{
					Type type = DecodeTypeSignature(reader);
					if(type == null) throw new ILReaderException("Unable to decode type of single-dimensional array signature.");
					return type.Assembly.GetType(type.FullName + "[]");
				}

				if((elementType == ElementType.CustomModOpt) || (elementType == ElementType.CustomModReqd))
				{
					int token = DecodeSignatureToken(reader);
					return DecodeTypeSignature(reader);
				}

				if(elementType == ElementType.Sentinel) return DecodeTypeSignature(reader);
				if(elementType == ElementType.Pinned) return DecodeTypeSignature(reader);

				throw new ILReaderException("Unknown element type.");
			}

			public Type[] DecodeLocalVariableSignature(int token)
			{
				if(token == 0) return null;
				Row row = ToRow(token);
				if(row == null) throw new ILReaderException("Local variable signature block cannot be found.");
				Byte[] data = row["Signature"].Blob;

				MemoryStream stream = new MemoryStream(data);
				BinaryReader reader = new BinaryReader(stream);

				int localVarSig = DecodeInt32(reader);
				if(localVarSig != 0x07) throw new ILReaderException("Invalid local variable signature.");

				int count = DecodeInt32(reader);
				Type[] types = new Type[count];
				for(int i = 0;i < count;i++)
					types[i] = DecodeTypeSignature(reader);

				stream.Close();

				return types;
			}

			Boolean MemberInfoSignatureCheck(MemberInfo memberInfo, Byte[] signature)
			{
				Boolean result = false;

				MemoryStream stream = new MemoryStream(signature);
				BinaryReader reader = new BinaryReader(stream);

				if(memberInfo is FieldInfo)
					result = FieldInfoSignatureCheck((FieldInfo)memberInfo, reader);

				if(memberInfo is MethodBase)
					result = MethodBaseSignatureCheck((MethodBase)memberInfo, reader);

				stream.Close();
				return result;
			}

			Boolean FieldInfoSignatureCheck(FieldInfo fieldInfo, BinaryReader reader)
			{
				if(DecodeInt32(reader) != 0x06) throw new ILReaderException("Invalid field signature.");
				Type type = DecodeTypeSignature(reader);
				if(type == null) throw new ILReaderException("Invalid field signature.");
				return (type == fieldInfo.FieldType);
			}

			Boolean MethodBaseSignatureCheck(MethodBase methodBase, BinaryReader reader)
			{
				Byte first = reader.ReadByte();
				int count = DecodeInt32(reader);
				ParameterInfo[] parameters = null;

				if(methodBase != null)
				{
					int upper = first & 0xF0;
					int lower = first & 0x0F;
					if(lower > 0x05) throw new Exception("Invalid method signature.");
					CallingConventions conventions = methodBase.CallingConvention;
					if((upper == 0x40) && ((conventions & CallingConventions.ExplicitThis) == 0)) return false;
					if((upper == 0x20) && ((conventions & CallingConventions.HasThis) == 0)) return false;
					if((lower == 0x05) && ((conventions & CallingConventions.Any) != CallingConventions.VarArgs)) return false;
					if((lower == 0x00) && ((conventions & CallingConventions.Any) != CallingConventions.Standard)) return false;
					parameters = methodBase.GetParameters();
					if(count != parameters.Length) return false;
				}

				DecodeTypeSignature(reader);
				for(int i = 0;i < count;i++)
				{
					Type type = DecodeTypeSignature(reader);
					if((parameters != null) && (!TypeHelper.Compare(parameters[i].ParameterType, type)))
						return false;
				}

				return true;
			}

			MemberInfo ResolveToken(int token)
			{
				MemberInfo memberInfo = (MemberInfo)memberInfoCache[token];
				if(memberInfo != null) return memberInfo;

				Table table = GetTable(token);
				if(table == null) return null;
				Types tableType = table.Type;

				if((tableType == Types.TypeDef) || (tableType == Types.TypeRef) || (tableType == Types.TypeSpec))
					memberInfo = TypeFromTypeDefOrRefToken(token);

				if(tableType == Types.MethodDef)
					memberInfo = MethodBaseFromMethodDefToken(token);

				if(tableType == Types.FieldDef)
					memberInfo = FieldInfoFromFieldDefToken(token);

				if(tableType == Types.MemberRef)
					memberInfo = MemberInfoFromMemberRefToken(token);

				if(tableType == Types.Signature)
					return null;

				if(memberInfo == null) throw new ILReaderException("Token " + token.ToString("X8") + " cannot be reolved.");

				memberInfoCache[token] = memberInfo;
				return memberInfo;
			}

			int DecodeInt32(BinaryReader reader)
			{
				int length = reader.ReadByte();
				if((length & 0x80) == 0) return length;
				if((length & 0xC0) == 0x80) return ((length & 0x3F) << 8) | reader.ReadByte();
				return ((length & 0x3F) << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();
			}

			public Instruction DecodeInstruction(BinaryReader reader)
			{
				Instruction instruction = new Instruction();
				instruction.Offset = (int)reader.BaseStream.Position;

				ushort code = reader.ReadByte();
				if(code == 0xFE) code = (ushort)((code << 8) | reader.ReadByte());
				if(!opCodes.Contains(code)) throw new ILReaderException("Unknown IL op-code " + "0x" + code.ToString("X2"));

				instruction.OpCode = (OpCode)opCodes[code];
				instruction.Operand = null;
				instruction.OperandData = null;

				long operandSize = reader.BaseStream.Position;

				switch(instruction.OpCode.OperandType)
				{
				case OperandType.InlineNone:
					break;

				case OperandType.ShortInlineBrTarget:
					SByte shortDelta = reader.ReadSByte();
					instruction.Operand = (int)(reader.BaseStream.Position + shortDelta);
					break;

				case OperandType.InlineBrTarget:
					int delta = reader.ReadInt32();
					instruction.Operand = (int)(reader.BaseStream.Position + delta);
					break;

				case OperandType.ShortInlineI:
					instruction.Operand = reader.ReadSByte();
					break;

				case OperandType.InlineI:
					instruction.Operand = reader.ReadInt32();
					break;

				case OperandType.InlineI8:
					instruction.Operand = reader.ReadInt64();
					break;

				case OperandType.ShortInlineR:
					instruction.Operand = reader.ReadSingle();
					break;

				case OperandType.InlineR:
					instruction.Operand = reader.ReadDouble();
					break;

				case OperandType.InlineString:
					instruction.Operand = GetUserString(reader.ReadInt32() & 0x00FFFFFF);
					break;

				case OperandType.ShortInlineVar:
					instruction.Operand = (int)reader.ReadByte();
					break;

				case OperandType.InlineVar:
					instruction.Operand = (int)reader.ReadUInt16();
					break;

				case OperandType.InlineSig:
				case OperandType.InlineMethod:
				case OperandType.InlineField:
				case OperandType.InlineType:
				case OperandType.InlineTok:
					instruction.Operand = ResolveToken(reader.ReadInt32());
					break;

				case OperandType.InlineSwitch:
					int cases = reader.ReadInt32();
					int[] deltas = new int[cases];
					for(int i = 0;i < cases;i++) deltas[i] = reader.ReadInt32();
					int[] targets = new int[cases];
					for(int i = 0;i < cases;i++) targets[i] = (int)(reader.BaseStream.Position + deltas[i]);
					instruction.Operand = targets;
					break;

				case OperandType.InlinePhi: //TODO: More spec needed
				default:
					throw new ILReaderException("Unknown operand type.");
				}

				operandSize = reader.BaseStream.Position - operandSize;
				if(operandSize != 0)
				{
					reader.BaseStream.Position -= operandSize;
					instruction.OperandData = reader.ReadBytes((int)operandSize);
				}

				return instruction;
			}

			String GetString(int offset)
			{
				if(strings == 0) throw new ILReaderException("Unable to read heap string.");

				String cache = (String)stringCache[offset];
				if(cache != null) return cache;
				reader.BaseStream.Position = strings + offset;

				ArrayList chars = new ArrayList();
				Byte character;
				while((character = reader.ReadByte()) != 0) chars.Add(character);
				Byte[] data = (Byte[])chars.ToArray(typeof(Byte));
				UTF8Encoding decoder = new UTF8Encoding();
				String result = decoder.GetString(data);

				stringCache[offset] = result;
				return result;
			}

			Byte[] GetBlob(int offset)
			{
				if(blobs == 0) throw new ILReaderException("Unable to read heap blob.");
				reader.BaseStream.Position = blobs + offset;
				int count = DecodeInt32(reader);
				return reader.ReadBytes(count);
			}

			Guid GetGuid(int offset)
			{
				if(offset == 0) return Guid.Empty;
				reader.BaseStream.Position = guids + offset;
				Byte[] guid = reader.ReadBytes(16);
				return new Guid(guid);
			}

			String GetUserString(int offset)
			{
				if(userStrings == 0) throw new ILReaderException("Unable to read heap user string.");
				if(offset == 0) throw new ILReaderException("Unable to read heap user string.");
				reader.BaseStream.Position = userStrings + offset;
				Byte[] data = reader.ReadBytes(DecodeInt32(reader));
				UnicodeEncoding encoding = new UnicodeEncoding();
				return encoding.GetString(data);
			}

			int ToToken(Types tableType, int index)
			{
				int type = (int)tableType;
				index++;
				if(index < 0) return -1;
				return ((type << 24) | index);
			}

			int DecodeToken(int token)
			{
				if(tokenEncoding == TokenEncoding.ReadWrite)
				{
					if(token == -1) return -1;
					Table table = GetTable(token);
					int index = token & 0xFFFFFF;
					index--;
					if(index == -1) return -1;

					if(table.Type == Types.MethodDef)
					{
						Row row = ToRow(ToToken(Types.MethodPtr, index));
						if(row == null) return -1;
						token = row["Method"].Token;
					} else if(table.Type == Types.FieldDef)
					{
						Row row = ToRow(ToToken(Types.FieldPtr, index));
						if(row == null) return -1;
						token = row["Field"].Token;
					}
				}

				return token;
			}

			int EncodeToken(int token)
			{
				if(tokenEncoding == TokenEncoding.ReadWrite)
				{
					if(token == -1) return -1;
					Table table = GetTable(token);
					if(table.Type == Types.MethodDef)
					{
						Table ptrTable = GetTable(Types.MethodPtr);
						for(int i = 0;i < ptrTable.Count;i++)
							if(ptrTable[i]["Method"].Token == token)
								return ToToken(Types.MethodDef, i);
					} else if(table.Type == Types.FieldDef)
					{
						Table ptrTable = GetTable(Types.FieldPtr);
						for(int i = 0;i < ptrTable.Count;i++)
							if(ptrTable[i]["Field"].Token == token)
								return ToToken(Types.FieldDef, i);
					}
				}

				return token;
			}

			Row ToRow(int token)
			{
				if(token == -1) return null;
				Table table = GetTable(token);
				if(table == null) return null;
				int index = token & 0xFFFFFF;
				index--;
				if(index == -1) return null;
				if(index > table.Count) return null;
				return table[index];
			}

			int GetStringIndexSize()
			{
				return ((tableHeader.HeapSizes & 0x01) != 0) ? 4 : 2;
			}

			int GetGuidIndexSize()
			{
				return ((tableHeader.HeapSizes & 0x02) != 0) ? 4 : 2;
			}

			int GetBlobIndexSize()
			{
				return ((tableHeader.HeapSizes & 0x04) != 0) ? 4 : 2;
			}

			Table GetTable(int token)
			{
				int index = token >> 24;
				if(index >= tableData.Length) return null;
				return tableData[index];
			}

			Table GetTable(Types type)
			{
				int index = (int)type;
				if(index >= tableData.Length) return null;
				return tableData[index];
			}

			enum Types
			{
				ModuleDef = 0, TypeRef = 1, TypeDef = 2, FieldPtr = 3,
				FieldDef = 4, MethodPtr = 5, MethodDef = 6, ParamPtr = 7,
				ParamDef = 8, InterfaceImpl = 9, MemberRef = 10, Constant = 11,
				CustomAttribute = 12, FieldMarshal = 13, Permission = 14, ClassLayout = 15,
				FieldLayout = 16, Signature = 17, EventMap = 18, EventPtr = 19,
				EventDef = 20, PropertyMap = 21, PropertyPtr = 22, PropertyDef = 23,
				MethodSemantics = 24, MethodImpl = 25, ModuleRef = 26, TypeSpec = 27,
				ImplMap = 28, FieldRVA = 29, ENCLog = 30, ENCMap = 31,
				AssemblyDef = 32, AssemblyProcessor = 33, AssemblyOS = 34, AssemblyRef = 35,
				AssemblyRefProcessor = 36, AssemblyRefOS = 37, File = 38, ExportedType = 39,
				ManifestResource = 40, NestedClass = 41, TypeTyPar = 42, MethodTyPar = 43,
				TypeDefOrRef = 64, HasConstant = 65, CustomAttributeType = 66, HasSemantic = 67,
				ResolutionScope = 68, HasFieldMarshal = 69, HasDeclSecurity = 70, MemberRefParent = 71,
				MethodDefOrRef = 72, MemberForwarded = 73, Implementation = 74, HasCustomAttribute = 75,
				UInt16 = 97, UInt32 = 99,
				String = 101, Blob = 102, Guid = 103,
				UserString = 112
			}

			struct TableHeader
			{
				public uint Reserved;
				public Byte MajorVersion;
				public Byte MinorVersion;
				public Byte HeapSizes;
				public Byte Padding;
				public UInt64 Valid;
				public UInt64 Sorted;
			}

			struct Attribute
			{
				public Types Type;
				public String Name;
			}

			class Table : IEnumerable
			{
				int[] codedTokenBits = new int[] { 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
				Types type;
				Attribute[] schema;
				MetaData image;
				Row[] rows = null;
				int rowSize = -1;
				Byte[] data = null;

				public Table(Types type, Types[] attributeTypes, String[] attributeNames, MetaData image)
				{
					this.type = type;
					this.image = image;

					schema = new Attribute[attributeTypes.Length];
					for(int i = 0;i < schema.Length;i++)
					{
						schema[i] = new Attribute();
						schema[i].Type = attributeTypes[i];
						schema[i].Name = attributeNames[i];
					}
				}

				public Types Type
				{
					get { return type; }
				}

				public Attribute[] Schema
				{
					get { return schema; }
				}

				public int Count
				{
					set { rows = new Row[value]; }
					get { return rows.Length; }
				}

				public void Fill(BinaryReader reader)
				{
					rowSize = 0;
					foreach(Attribute attribute in schema)
						rowSize += SizeOf(attribute.Type);
					data = reader.ReadBytes(rowSize * Count);
				}

				public IEnumerator GetEnumerator()
				{
					return new Enumerator(this);
				}

				public Row this[int index]
				{
					get { if(rows[index] == null) Expand(index); return rows[index]; }
				}

				int SizeOf(Types type)
				{
					// Fixed
					if(type == Types.UInt16) return 2;
					if(type == Types.UInt32) return 4;

					// Heap
					if(type == Types.String) return image.GetStringIndexSize();
					if(type == Types.Blob) return image.GetBlobIndexSize();
					if(type == Types.Guid) return image.GetGuidIndexSize();

					// RID
					Table ridTable = image.GetTable(type);
					if(ridTable != null)
						return (ridTable.Count < 65536) ? 2 : 4;

					// CodedToken
					Types[] category = (Types[])categories[type];
					if(category != null)
					{
						int rows = 0;
						foreach(Types subType in category)
							if(subType != Types.UserString)
							{
								Table table = image.GetTable(subType);
								if(table != null) rows = (rows > table.Count) ? rows : table.Count;
							}

						rows = rows << codedTokenBits[category.Length];
						return (rows < 65536) ? 2 : 4;
					}

					throw new ILReaderException("Invalid token type.");
				}

				uint ReadColumn(Types type, BinaryReader reader)
				{
					// Fixed
					if(type == Types.UInt16) return reader.ReadUInt16();
					if(type == Types.UInt32) return reader.ReadUInt32();

					// Heap
					if(type == Types.String) return ((image.GetStringIndexSize() == 2) ? (uint)reader.ReadUInt16() : reader.ReadUInt32());
					if(type == Types.Guid) return ((image.GetGuidIndexSize() == 2) ? (uint)reader.ReadUInt16() : reader.ReadUInt32());
					if(type == Types.Blob) return ((image.GetBlobIndexSize() == 2) ? (uint)reader.ReadUInt16() : reader.ReadUInt32());

					// Rid
					Table table = image.GetTable(type);
					if(table != null)
						return (uint)(((uint)type << 24) | ((table.Count < 65536) ? (uint)reader.ReadUInt16() : reader.ReadUInt32()));

					// Coded token
					int size = SizeOf(type);
					int codedToken = (size == 2) ? reader.ReadUInt16() : reader.ReadInt32();
					Types[] category = (Types[])categories[type];
					if(category == null) return 0;
					int tableIndex = (int)(codedToken & ~(-1 << codedTokenBits[category.Length]));
					int index = (int)(codedToken >> codedTokenBits[category.Length]);
					int token = image.ToToken(category[tableIndex], index - 1);
					return (uint)token;
				}

				void Expand(int index)
				{
					MemoryStream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					reader.BaseStream.Position = rowSize * index;

					Column[] columns = new Column[schema.Length];
					for(int i = 0;i < schema.Length;i++)
					{
						Attribute attribute = schema[i];
						uint value = ReadColumn(schema[i].Type, reader);
						columns[i] = new Column(value, image);
					}

					rows[index] = new Row(columns, this);
					stream.Close();
				}

				public override String ToString()
				{
					return type + "(" + rows.Length + ")";
				}

				class Enumerator : IEnumerator
				{
					Table table;
					int current;

					public Enumerator(Table table)
					{
						this.table = table;
						Reset();
					}

					public void Reset()
					{
						current = -1;
					}

					public Boolean MoveNext()
					{
						current++;
						return (current < table.Count);
					}

					public Object Current
					{
						get { return table[current]; }
					}
				}
			}

			class Row
			{
				Table table;
				Column[] columns;

				public Row(Column[] columns, Table table)
				{
					this.columns = columns;
					this.table = table;
				}

				public Table Table
				{
					get { return table; }
				}

				public Column this[int index]
				{
					get { return columns[index]; }
				}

				public Column this[String name]
				{
					get
					{
						Attribute[] schema = table.Schema;
						for(int i = 0;i < schema.Length;i++)
							if(schema[i].Name == name)
								return this[i];
						return null;
					}
				}

				public override String ToString()
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(table.Type.ToString());
					builder.Append(" := ");
					Attribute[] schema = table.Schema;
					for(int i = 0;i < schema.Length;i++)
					{
						builder.Append(schema[i].Name);
						builder.Append("='");

						Types type = schema[i].Type;
						if(type == Types.UInt16)
							builder.Append(this[i].UInt16);
						else if(type == Types.UInt32)
							builder.Append(this[i].UInt32);
						else if(type == Types.String)
							builder.Append(this[i].String);
						else if(type == Types.Guid)
							builder.Append(this[i].Guid.ToString());
						else if(type == Types.Blob)
						{
							Byte[] blob = this[i].Blob;
							foreach(Byte b in blob) builder.Append(b.ToString("X2") + " ");
						} else
							builder.Append(this[i].Token.ToString("X8"));

						builder.Append("' ");
					}
					return builder.ToString();
				}
			}

			class Column
			{
				MetaData image;
				uint data;

				public Column(uint data, MetaData image)
				{
					this.data = data;
					this.image = image;
				}

				public ushort UInt16
				{
					get { return (ushort)data; }
				}

				public uint UInt32
				{
					get { return data; }
				}

				public String String
				{
					get { return image.GetString((int)data); }
				}

				public Byte[] Blob
				{
					get { return image.GetBlob((int)data); }
				}

				public Guid Guid
				{
					get { return image.GetGuid((int)data); }
				}

				public int Token
				{
					get { return (int)data; }
				}
			}

			enum TokenEncoding
			{
				ReadOnly,
				ReadWrite
			}

			struct ElementType
			{
				public const Byte End = 0x00;
				public const Byte Void = 0x01;
				public const Byte Boolean = 0x02;
				public const Byte Char = 0x03;
				public const Byte I1 = 0x04;
				public const Byte U1 = 0x05;
				public const Byte I2 = 0x06;
				public const Byte U2 = 0x07;
				public const Byte I4 = 0x08;
				public const Byte U4 = 0x09;
				public const Byte I8 = 0x0a;
				public const Byte U8 = 0x0b;
				public const Byte R4 = 0x0c;
				public const Byte R8 = 0x0d;
				public const Byte String = 0x0e;
				public const Byte Ptr = 0x0f;
				public const Byte ByRef = 0x10;
				public const Byte ValueType = 0x11;
				public const Byte Class = 0x12;
				public const Byte Array = 0x14;
				public const Byte TypedReference = 0x16;
				public const Byte IntPtr = 0x18;
				public const Byte UIntPtr = 0x19;
				public const Byte FnPtr = 0x1b;
				public const Byte Object = 0x1c;
				public const Byte SzArray = 0x1d;
				public const Byte CustomModReqd = 0x1f;
				public const Byte CustomModOpt = 0x20;
				public const Byte Internal = 0x21;
				public const Byte Modifier = 0x40;
				public const Byte Sentinel = 0x41;
				public const Byte Pinned = 0x45;
			}

			class TypeHelper
			{
				public static String GetResolutionScope(Type type)
				{
					if(type.DeclaringType != null)
					{
						String resolutionScope = GetResolutionScope(type.DeclaringType);
						if(resolutionScope != String.Empty) resolutionScope += ".";
						return resolutionScope + type.DeclaringType.Name;
					}

					return (type.Namespace != null) ? type.Namespace : String.Empty;
				}

				public static String GetFullName(Type type)
				{
					if(type == null) return String.Empty;
					String resolutionScope = GetResolutionScope(type);
					if(resolutionScope != String.Empty) resolutionScope += ".";
					return resolutionScope + type.Name;
				}

				public static String GetUniqueName(Type type)
				{
					if(type == null) return String.Empty;
					return type.FullName + ", " + type.Assembly.FullName;
				}

				public static Boolean Compare(Type t1, Type t2)
				{
					return (GetUniqueName(t1) == GetUniqueName(t2));
				}
			}
		}
	}

	public class MethodBody : IEnumerable
	{
		int codeSize;
		ushort maxStack;
		Type[] locals;
		Instruction[] instructions;
		ExceptionHandler[] exceptions;

		public MethodBody(int codeSize, ushort maxStack, Type[] locals, ExceptionHandler[] exceptions, Instruction[] instructions)
		{
			this.codeSize = codeSize;
			this.maxStack = maxStack;
			this.locals = locals;
			this.exceptions = exceptions;
			this.instructions = instructions;
		}

		public IEnumerator GetEnumerator()
		{
			return instructions.GetEnumerator();
		}

		public ushort MaxStack
		{
			get { return maxStack; }
		}

		public int CodeSize
		{
			get { return codeSize; }
		}

		public Type[] Locals
		{
			get { return locals; }
		}

		public ExceptionHandler[] Exceptions
		{
			get { return exceptions; }
		}

		public int Count
		{
			get { return instructions.Length; }
		}

		public Instruction this[int index]
		{
			get { return instructions[index]; }
		}
	}

	public struct Instruction
	{
		public int Offset;
		public OpCode OpCode;
		public Object Operand;
		public Byte[] OperandData;
	}

	public class ExceptionHandler
	{
		Block tryBlock;
		Block handlerBlock;

		protected ExceptionHandler(Block tryBlock, Block handlerBlock)
		{
			this.tryBlock = tryBlock;
			this.handlerBlock = handlerBlock;
		}

		public Block Try
		{
			get { return tryBlock; }
		}

		public Block Handler
		{
			get { return handlerBlock; }
		}
	}

	public class Catch : ExceptionHandler
	{
		Type type;

		public Catch(Block tryBlock, Block handlerBlock, Type type)
			: base(tryBlock, handlerBlock)
		{
			this.type = type;
		}

		public Type Type
		{
			get { return type; }
		}
	}

	public class Fault : ExceptionHandler
	{
		public Fault(Block tryBlock, Block handlerBlock)
			: base(tryBlock, handlerBlock)
		{
		}
	}

	public class Finally : ExceptionHandler
	{
		public Finally(Block tryBlock, Block handlerBlock)
			: base(tryBlock, handlerBlock)
		{
		}
	}

	public class Filter : ExceptionHandler
	{
		int expression;

		public Filter(Block tryBlock, Block handlerBlock, int expression)
			: base(tryBlock, handlerBlock)
		{
			this.expression = expression;
		}

		public int Expression
		{
			get { return expression; }
		}
	}

	public struct Block
	{
		public int Offset;
		public int Length;
	}

	public class ILReaderException : Exception
	{
		public ILReaderException(String message)
			: base(message)
		{
		}
	}
}