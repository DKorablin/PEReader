using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>Структуры .NET Framework</summary>
	/// <remarks>Описание в CorHdr.h</remarks>
	public struct Cor
	{
		#region IMAGE_COR20_METADATA
		/// <summary>Метаданные .NET сборки</summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct IMAGE_COR20_METADATA1
		{
			/// <summary>Magic signature for physical metadata : 0x424A5342.</summary>
			public UInt32 Signature;
			/// <summary>Major version, 1 (ignore on read).</summary>
			public UInt16 MajorVersion;
			/// <summary>Minor version, 1 (ignore on read).</summary>
			public UInt16 MinorVersion;
			/// <summary>Reserved, always 0.</summary>
			public UInt32 Reserved;
			/// <summary>Length of version string in bytes, say  m (&lt;= 255), rounded up to a multiple of four.</summary>
			public UInt32 Length;
			/// <summary>MetaData signature is valid</summary>
			public Boolean IsValid { get { return this.Signature == 0x424A5342; } }
		}
		[StructLayout(LayoutKind.Sequential)]
		internal struct IMAGE_COR20_METADATA2
		{
			/// <summary>Reserved, always 0.</summary>
			public UInt16 Flags;
			/// <summary>Number of streams, say n.</summary>
			public UInt16 Streams;
			/*/// <summary>Array of n StreamHdr structures.</summary>
			public UInt32 StreamHeaders;*/
		}
		/// <summary>
		/// The root of the physical metadata starts with a magic signature,
		/// several bytes of version and other miscellaneous information,
		/// followed by a count and an array of stream headers, one for each stream that is present.
		/// </summary>
		public struct IMAGE_COR20_METADATA
		{
			/// <summary>Magic signature for physical metadata : 0x424A5342.</summary>
			public UInt32 Signature;
			/// <summary>Major version, 1 (ignore on read).</summary>
			public UInt16 MajorVersion;
			/// <summary>Minor version, 1 (ignore on read).</summary>
			public UInt16 MinorVersion;
			/// <summary>Reserved, always 0.</summary>
			public UInt32 Reserved;
			/// <summary>Length of version string in bytes, say  m (&lt;= 255), rounded up to a multiple of four.</summary>
			public UInt32 Length;
			/// <summary>UTF8-encoded version string of length m (see below).</summary>
			public String Version;
			/// <summary>Reserved, always 0.</summary>
			public UInt16 Flags;
			/// <summary>Number of streams, say n.</summary>
			public UInt16 Streams;
			/*/// <summary>Array of n StreamHdr structures.</summary>
			public UInt32 StreamHeaders;*/
			/// <summary>MetaData header valid</summary>
			public Boolean IsValid { get { return this.Signature == 0x424A5342; } }
			/// <summary>String representation of the signature</summary>
			public String SignatureStr { get { return System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(this.Signature)); } }
			/// <summary>MetaData struct version</summary>
			public Version StructVersion { get { return new Version(this.MajorVersion, this.MinorVersion); } }
		}
		#endregion IMAGE_COR20_METADATA
		/// <summary>
		/// Each entry in this array describes a contiguous array of v-table slots of the specified size.
		/// Each slot starts out initialized to the metadata token value for the method they need to call.
		/// At image load time, the runtime Loader will turn each entry into a pointer to machine code for the CPU and can be called directly.
		/// </summary>
		/// <remarks>
		/// Certain languages, which choose not to follow the common type system runtime model, can have
		/// virtual functions which need to be represented in a v-table.
		/// These v-tables are laid out by the compiler, not by the runtime.
		/// Finding the correct v-table slot and calling indirectly through the value held in that slot is also done by the compiler.
		/// The VtableFixups field in the runtime header contains the location and size of an array of Vtable Fixups (§II.15.5.1).
		/// V-tables shall be emitted into a read-write section of the PE file.
		/// </remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_COR20_VTABLE
		{
			/// <summary>RVA of VTable.</summary>
			public UInt32 VirtualAddress;
			/// <summary>Number of entries in VTable.</summary>
			public UInt16 Size;
			/// <summary>Type of the entries.</summary>
			public COR_VTABLE Type;
		}
		/// <summary>Managed resources header</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct ResourceManagerHeader
		{
			/// <summary>Unknown</summary>
			public UInt32 Unknown;
			/// <summary>Magic Number (0xBEEFCACE)</summary>
			public UInt32 MagicNumber;
			/// <summary>Resource Manager header version</summary>
			public UInt32 HeaderVersionNumber;
			/// <summary>Num bytes to skip from here to get past this header</summary>
			public UInt32 SizeOfReaderType;
			//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
			//public String ReaderType;
			/// <summary>Header is valid</summary>
			public Boolean IsValid { get { return this.MagicNumber == 0xBEEFCACE; } }
			/// <summary>Magin number as string</summary>
			public String MagicNumberStr { get { return BitConverter.ToString(BitConverter.GetBytes(this.MagicNumber)); } }
		}
		/// <summary>Managed resources header</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct ResourceSetHeader
		{
			/// <summary>Resource set header version</summary>
			public UInt32 Version;
			/// <summary>Number of Resources</summary>
			public UInt32 NumberOfResources;
			/// <summary>Number of types</summary>
			public UInt32 NumberOfTypes;
		}
		/// <summary>Type of the entries.</summary>
		public enum COR_VTABLE
		{
			/// <summary>Vtable slots are 32 bits.</summary>
			_32BIT = 0x01,
			/// <summary>Vtable slots are 64 bits.</summary>
			_64BIT = 0x02,
			/// <summary>Transition from unmanaged to managed code.</summary>
			FROM_UNMANAGED = 0x04,
			/// <summary>Call most derived method described by the token (only valid for virtual methods).</summary>
			CALL_MOST_DERIVED = 0x10,
		}
		/// <summary>Stream types</summary>
		public enum StreamHeaderType
		{
			/// <summary>#Strings</summary>
			String,
			/// <summary>#US</summary>
			UnicodeSting,
			/// <summary>#Blob</summary>
			Blob,
			/// <summary>#GUID</summary>
			Guid,
			/// <summary>#~</summary>
			StreamTable,
			/// <summary>#-</summary>
			StreamTableUnoptimized,
		}
		/// <summary>A stream header gives the names, and the position and length of a particular table or heap.</summary>
		public struct STREAM_HEADER
		{
			/// <summary>Memory offset to start of this stream from start of the metadata root.</summary>
			public UInt32 Offset;
			/// <summary>Size of this stream in bytes, shall be a multiple of 4.</summary>
			public UInt32 Size;
			/// <summary>
			/// Name of the stream as null-terminated variable length array of ASCII characters, padded to the next 4-byte boundary with \0 characters.
			/// The name is limited to 32 characters.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public String Name;
			/// <summary>Stream header type</summary>
			/// <exception cref="NotImplementedException">Stream header type is unknown</exception>
			public StreamHeaderType Type
			{
				get
				{
					switch(this.Name)
					{
					case "#Strings":
						return StreamHeaderType.String;
					case "#US":
						return StreamHeaderType.UnicodeSting;
					case "#Blob":
						return StreamHeaderType.Blob;
					case "#GUID":
						return StreamHeaderType.Guid;
					case "#~":
						return StreamHeaderType.StreamTable;
					case "#-":
						return StreamHeaderType.StreamTableUnoptimized;
					default: throw new NotImplementedException(this.Name);
					}
				}
			}
		}
		/// <summary>.NET Stream table header</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct STREAM_TABLE_HEADER
		{
			/// <summary>Кол-во таблиц. В коде Daniel Pistelli было 64, а на самом деле, их 46.</summary>
			public const Int32 TablesCount = 64;
			/// <summary>Reserved, always 0</summary>
			public UInt32 Reserved1;
			/// <summary>Major version of table schemata; shall be 2</summary>
			public Byte MajorVersion;
			/// <summary>Minor version of table schemata; shall be 0</summary>
			public Byte MinorVersion;
			/// <summary>Bit vector for heap sizes.</summary>
			public Byte HeapSizes;
			/// <summary>Reserved, always 1</summary>
			public Byte Reserved2;
			/// <summary>Bit vector of present tables, let n be the number of bits that are 1.</summary>
			public UInt64 Valid;
			/// <summary>Bit vector of sorted tables.</summary>
			public UInt64 Sorted;
			/// <summary>Version of table schemata</summary>
			public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }
			/// <summary>Size</summary>
			public UInt32 StringIndexSize { get { return (UInt32)((this.HeapSizes & 0x01) != 0 ? 4 : 2); } }
			/// <summary>Size</summary>
			public UInt32 GuidIndexSize { get { return (UInt32)((this.HeapSizes & 0x02) != 0 ? 4 : 2); } }
			/// <summary>Size</summary>
			public UInt32 BlobIndexSize { get { return (UInt32)((this.HeapSizes & 0x04) != 0 ? 4 : 2); } }
			/// <summary>Count of tables in stream</summary>
			public UInt32 PresentTablesCount
			{
				get
				{
					UInt32 result = 0;
					for(Int32 loop = 0;loop < STREAM_TABLE_HEADER.TablesCount;loop++)
						if(this.IsTablePresent(loop))
							result++;
					return result;
				}
			}
			/// <summary>If specific table presents in stream</summary>
			/// <param name="tableIndex">Index of table</param>
			/// <returns>True/False</returns>
			public Boolean IsTablePresent(Int32 tableIndex)
			{
				return ((this.Valid >> tableIndex) & 1) != 0;
			}
			/*public UInt32[] Rows;
			public UInt32 Tables;*/
		}
		/// <summary>Flags for method header.</summary>
		/// <remarks>
		/// The first byte of a method header can also contain the following flags,
		/// valid only for the Fat format, that indicate how the method is to be executed.
		/// </remarks>
		[Flags]
		public enum CorILMethod : byte
		{
			/// <summary>Method header is fat.</summary>
			FatFormat = 0x3,
			/// <summary>Method header is tiny.</summary>
			TinyFormat = 0x2,
			/// <summary>More sections follow after this header (§II.25.4.5).</summary>
			MoreSects = 0x8,
			/// <summary>Call default constructor on all local variables.</summary>
			InitLocals = 0x10,
		}
		/// <summary>IL method header descriptor</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorILMethodHeader
		{
			/// <summary>Flags (CorILMethod_FatFormat shall be set in bits 0:1, see §II.25.4.4)</summary>
			public CorILMethod Format;
			/// <summary>Size of this header expressed as the count of 4-byte integers occupied (currently 3)</summary>
			/// <remarks>TODO: This must be 12+4bits from structure start</remarks>
			public Byte Size;
			/// <summary>Maximum number of items on the operand stack.</summary>
			public UInt16 MaxStack;
			/// <summary>Size in bytes of the actual method body.</summary>
			public UInt32 CodeSize;
			/// <summary>
			/// Meta Data token for a signature describing the layout of the local variables for the method.
			/// 0 means there are no local variables present.
			/// </summary>
			public UInt32 LocalVarSigTok;
			/// <summary>Method header size</summary>
			public UInt32 HeaderSize
			{
				get
				{
					if((this.Format & CorILMethod.FatFormat) == CorILMethod.FatFormat)
						return (UInt32)Marshal.SizeOf(typeof(CorILMethodHeader));//Fat format
					else if((this.Format & CorILMethod.TinyFormat) == CorILMethod.TinyFormat)
						return sizeof(Byte);//Tiny format
					else throw new NotSupportedException();
				}
			}
			//public Byte HeaderSize { get { return (Byte)(this.Format2 >> 4 * 4); } }
		}
		/// <summary>Fat method section descriptor</summary>
		[Flags]
		public enum CorILMethod_Sect : byte
		{
			/// <summary>Exception handling data.</summary>
			EHTable = 0x1,
			/// <summary>Reserved, shall be 0.</summary>
			OptILTable = 0x2,
			/// <summary>
			/// Data format is of the fat variety, meaning there is a 3 - byte length least - significant byte first format.
			/// If not set, the header is small with a 1 - byte length.
			/// </summary>
			FatFormat = 0x40,
			/// <summary>Another data section occurs after this current section.</summary>
			MoreSects = 0x80,
		}
		/// <summary>Currently, the method data sections are only used for exception tables (§II.19).</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct CorILMethodSection
		{
			/// <summary>Data section type</summary>
			public CorILMethod_Sect Kind;
			/// <summary>Raw data size</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
			public Byte[] DataSize;
			/// <summary>Count of clause numbers</summary>
			public UInt32 ClauseNumber
			{
				get
				{
					Int32 dataSize;
					if(this.IsFatFormat)
					{
						dataSize = this.DataSize[0] + this.DataSize[1] * 0x100 + this.DataSize[2] * 0x10000;
						dataSize = dataSize / Marshal.SizeOf(typeof(CorILMethodExceptionFat));
					} else
						dataSize = this.DataSize[0] / Marshal.SizeOf(typeof(CorILMethodExceptionSmall));
					return (UInt32)dataSize;
				}
			}
			/// <summary>Section in Fat format.</summary>
			public Boolean IsFatFormat
			{
				get { return (this.Kind & CorILMethod_Sect.FatFormat) == CorILMethod_Sect.FatFormat; }
			}
			/// <summary>More sections follow after this one.</summary>
			public Boolean HasMoreSections
			{
				get { return (this.Kind & CorILMethod_Sect.MoreSects) == CorILMethod_Sect.MoreSects; }
			}
		}
		/// <summary>Type of exception-handling clause.</summary>
		[Flags]
		public enum COR_ILEXCEPTION_CLAUSE : ushort
		{
			/// <summary>A typed exception clause.</summary>
			/// <remarks>
			/// If the clause is a typed exception clause
			/// then read the token of the exception that will be handled by it.
			/// </remarks>
			EXCEPTION = 0x0000,
			/// <summary>An exception filter and handler clause.</summary>
			/// <remarks>
			/// If the clause is a filter clause then read the offset of the filter.
			/// (e.g.: VB.NET can generate such filter for the "Catch exc As Exception When value = True" code).
			/// </remarks>
			FILTER = 0x0001,
			/// <summary>A finally clause.</summary>
			FINALLY = 0x0002,
			/// <summary>Fault clause (finally that is called on exception only).</summary>
			FAULT = 0x0004,
		}
		/// <summary>
		/// The small form of the exception clause should be used whenever the code sizes
		/// for the try block and the handler code are both smaller than 256 bytes and both their offsets are smaller than 65536.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorILMethodExceptionSmall
		{
			private UInt16 FlagsI;
			/// <summary>Offset in bytes of try block from start of method body.</summary>
			public UInt16 TryOffset;
			/// <summary>Length in bytes of the try block.</summary>
			public Byte TryLength;
			/// <summary>Location of the handler for this try block.</summary>
			public UInt16 HandlerOffset;
			/// <summary>Size of the handler code in bytes.</summary>
			public Byte HandlerLength;
			/// <summary>
			/// Meta data token for a type-based exception handler.
			/// Or
			/// Offset in method body for filter-based exception handler.
			/// </summary>
			public UInt32 ClassTokenOrFilterOffset;
			/// <summary>Type of exception clause</summary>
			public COR_ILEXCEPTION_CLAUSE Flags { get { return (COR_ILEXCEPTION_CLAUSE)this.FlagsI; } }
		}
		/// <summary>Fat exception clause header section</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CorILMethodExceptionFat
		{
			private UInt32 FlagsI;
			/// <summary>Offset in bytes of try block from start of method body.</summary>
			public UInt32 TryOffset;
			/// <summary>Length in bytes of the try block.</summary>
			public UInt32 TryLength;
			/// <summary>Location of the handler for this try block.</summary>
			public UInt32 HandlerOffset;
			/// <summary>Size of the handler code in bytes.</summary>
			public UInt32 HandlerLength;
			/// <summary>
			/// Meta data token for a type-based exception handler.
			/// Or
			/// Offset in method body for filter-based exception handler.
			/// </summary>
			public UInt32 ClassTokenOrFilterOffset;
			/// <summary>Type of exception clause</summary>
			public COR_ILEXCEPTION_CLAUSE Flags { get { return (COR_ILEXCEPTION_CLAUSE)this.FlagsI; } }
		}
		/// <summary>Specifies a common language runtime Type, a type modifier, or information about a type in a metadata type signature.</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/ms232600%28v=vs.110%29.aspx</remarks>
		public enum ELEMENT_TYPE
		{
			/// <summary>Marks end of a list.</summary>
			END = 0x00,
			/// <summary>A void type. <see cref="System.Void"/></summary>
			VOID = 0x01,
			/// <summary>A Boolean type. <see cref="System.Boolean"/></summary>
			BOOLEAN = 0x02,
			/// <summary>A character type. <see cref="System.Char"/></summary>
			CHAR = 0x03,
			/// <summary>A signed 1-byte integer. <see cref="System.SByte"/></summary>
			I1 = 0x04,
			/// <summary>An unsigned 1-byte integer. <see cref="System.Byte"/></summary>
			U1 = 0x05,
			/// <summary>A signed 2-byte integer. <see cref="System.Int16"/></summary>
			I2 = 0x06,
			/// <summary>An unsigned 2-byte integer. <see cref="System.UInt16"/></summary>
			U2 = 0x07,
			/// <summary>A signed 4-byte integer. <see cref="System.Int32"/></summary>
			I4 = 0x08,
			/// <summary>An unsigned 4-byte integer. <see cref="System.UInt32"/></summary>
			U4 = 0x09,
			/// <summary>A signed 8-byte integer. <see cref="System.Int64"/></summary>
			I8 = 0x0a,
			/// <summary>An unsigned 8-byte integer. <see cref="System.UInt64"/></summary>
			U8 = 0x0b,
			/// <summary>A 4-byte floating point. <see cref="System.Single"/></summary>
			R4 = 0x0c,
			/// <summary>An 8-byte floating point. <see cref="System.Double"/></summary>
			R8 = 0x0d,
			/// <summary>A System.String type. <see cref="System.String"/></summary>
			STRING = 0x0e,
			/// <summary>A pointer type modifier.</summary>
			/// <remarks>Unmanaged pointer, followed by the Type element.</remarks>
			PTR = 0x0f,
			/// <summary>A reference type modifier.</summary>
			/// <remarks>Managed pointer, followed by the Type element.</remarks>
			BYREF = 0x10,
			/// <summary>A value type modifier.</summary>
			/// <remarks>A value type modifier, followed by TypeDef or TypeRef token.</remarks>
			VALUETYPE = 0x11,
			/// <summary>A class type modifier.</summary>
			/// <remarks>A class type modifier, followed by TypeDef or TypeRef token.</remarks>
			CLASS = 0x12,
			/// <summary>A class variable type modifier.</summary>
			/// <remarks>Generic parameter in a generic type definition, represented as number.</remarks>
			VAR = 0x13,
			/// <summary>A multi-dimensional array type modifier.</summary>
			ARRAY = 0x14,
			/// <summary>A type modifier for generic types.</summary>
			/// <remarks>Generic type instantiation. Followed by type type-arg-count type-1 ... type-n</remarks>
			GENERICINST = 0x15,
			/// <summary>A typed reference.</summary>
			TYPEDBYREF = 0x16,

			/// <summary>Size of a native integer. <see cref="System.IntPtr"/></summary>
			I = 0x18,
			/// <summary>Size of an unsigned native integer. <see cref="System.UIntPtr"/></summary>
			U = 0x19,
			/// <summary>A pointer to a function.</summary>
			/// <remarks>Followed by full method signature.</remarks>
			FNPTR = 0x1B,
			/// <summary>A System.Object type. <see cref="System.Object"/></summary>
			OBJECT = 0x1C,
			/// <summary>A single-dimensional, zero lower-bound array type modifier.</summary>
			SZARRAY = 0x1D,
			/// <summary>A method variable type modifier.</summary>
			/// <remarks>Generic parameter in a generic method definition, represented as number</remarks>
			MVAR = 0x1E,

			/// <summary>A C language required modifier.</summary>
			/// <remarks>Required modifier, followed by a TypeDef or TypeRef token</remarks>
			CMOD_REQ = 0x1F,
			/// <summary>A C language optional modifier.</summary>
			/// <remarks>Optional modifier, followed by a TypeDef or TypeRef token</remarks>
			CMOD_OPT = 0x20,

			/// <summary>Implemented within the CLI</summary>
			/// <remarks>Used internally.</remarks>
			INTERNAL = 0x21,
			/// <summary>An invalid type.</summary>
			MAX = 0x22,

			/// <summary>ORed with following element types</summary>
			/// <remarks>Used internally.</remarks>
			MODIFIER = 0x40,
			/// <summary>A type modifier that is a sentinel for a list of a variable number of parameters.</summary>
			/// <remarks>Sentinel for vararg method signature.</remarks>
			SENTINEL = 0x01 | ELEMENT_TYPE.MODIFIER,
			/// <summary>Denotes a local variable that points at a pinned object.</summary>
			/// <remarks>Used internally.</remarks>
			PINNED = 0x05 | ELEMENT_TYPE.MODIFIER,

			/// <summary>Indicates an argument of type <see cref="System.Type"/></summary>
			TYPE = 0x50,
			/// <summary>Used in custom attributes to specify a boxed object (Ecma-335: #II.23.3).</summary>
			BOXED = 0x51,
			/// <summary>Reserved</summary>
			RESERVED1 = 0x52,
			/// <summary>Used in custom attributes to indicate a FIELD(Ecma-335: #II.22.10, II.23.3).</summary>
			FIELD = 0x53,
			/// <summary>Used in custom attributes to indicate a PROPERTY (Ecma-335: #II.22.10, II.23.3).</summary>
			PROPERTY = 0x54,
			/// <summary>Used in custom attributes to specify an enum (Ecma-335: #II.23.3)</summary>
			ENUM = 0x55,
		}
		/// <summary>MetaData Table Types</summary>
		public enum MetaTableType : int
		{
			/// <summary>The rows in the Module table result from .module directives in the Assembly.</summary>
			Module = 0,
			/// <summary>Class reference descriptors.</summary>
			TypeRef = 1,
			/// <summary>
			/// The first row of the TypeDef table represents the pseudo class that acts as parent for functions and variables 
			/// defined at module scope.
			/// If a type is generic, its parameters are defined in the GenericParam table (§22.20). Entries in the 
			/// GenericParam table reference entries in the TypeDef table; there is no reference from the TypeDef table to the 
			/// GenericParam table.
			/// </summary>
			TypeDef = 2,
			/// <summary>A class-to-fields lookup table, whitch does not exist on optimized metadata (#~ stream).</summary>
			FieldPtr = 3,
			/// <summary>
			/// Each row in the Field table results from a top-level .field directive, or a .field directive inside a Type.
			/// </summary>
			Field = 4,
			/// <summary>A class-to-methods lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
			MethodPtr = 5,
			/// <summary>
			/// Conceptually, every row in the MethodDef table is owned by one, and only one, row in the TypeDef table.
			/// The rows in the MethodDef table result from .method directives (§15). The RVA column is computed when 
			/// the image for the PE file is emitted and points to the COR_ILMETHOD structure for the body of the method.
			/// </summary>
			MethodDef = 6,
			/// <summary>A method-to-parameters lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
			ParamPtr = 7,
			/// <summary>
			/// Conceptually, every row in the Param table is owned by one, and only one, row in the MethodDef table.
			/// The rows in the Param table result from the parameters in a method declaration (§15.4), or from a .param
			/// attribute attached to a method.
			/// </summary>
			Param = 8,
			/// <summary>Interface implementation descriptors.</summary>
			InterfaceImpl = 9,
			/// <summary>
			/// Combines two sorts of references, to Methods and to Fields of a class, known as 'MethodRef' and 'FieldRef', respectively.
			/// An entry is made into the MemberRef table whenever a reference is made in the CIL code to a method or field 
			/// which is defined in another module or assembly.  (Also, an entry is made for a call to a method with a VARARG
			/// signature, even when it is defined in the same module as the call site.) 
			/// </summary>
			MemberRef = 10,
			/// <summary>Used to store compile-time, constant values for fields, parameters, and properties.</summary>
			Constant = 11,
			/// <summary>
			/// Stores data that can be used to instantiate a Custom Attribute (more precisely, an 
			/// object of the specified Custom Attribute class) at runtime.
			/// A row in the CustomAttribute table for a parent is created by the .custom attribute, which gives the value of 
			/// the Type column and optionally that of the Value column.
			/// </summary>
			CustomAttribute = 12,
			/// <summary>
			/// The FieldMarshal table  'links' an existing row in the Field or Param table, to information 
			/// in the Blob heap that defines how that field or parameter (which, as usual, covers the method return, as 
			/// parameter number 0) shall be marshalled when calling to or from unmanaged code via PInvoke dispatch.
			/// A row in the FieldMarshal table is created if the .field directive for the parent field has specified a marshal attribute.
			/// </summary>
			FieldMarshal = 13,
			/// <summary>
			/// The rows of the DeclSecurity table are filled by attaching a .permission or .permissionset directive 
			/// that specifies the Action and PermissionSet on a parent assembly or parent type or method.
			/// </summary>
			DeclSecurity = 14,
			/// <summary>
			/// Used to define how the fields of a class or value type shall be laid out by the CLI.
			/// (Normally, the CLI is free to reorder and/or insert gaps between the fields defined for a class or value type.)
			/// </summary>
			ClassLayout = 15,
			/// <summary>A row in the FieldLayout table is created if the .field directive for the parent field has specified a field offset.</summary>
			FieldLayout = 16,
			/// <summary>
			/// Signatures are stored in the metadata Blob heap.  In most cases, they are indexed by a column in some table —
			/// Field.Signature, Method.Signature, MemberRef.Signature, etc.  However, there are two cases that require a 
			/// metadata token for a signature that is not indexed by any metadata table.  The StandAloneSig table fulfils this 
			/// need.  It has just one column, which points to a Signature in the Blob heap.
			/// </summary>
			StandAloneSig = 17,
			/// <summary>
			/// A class-to-events mapping table.
			/// This is not an intermidate lookup table, and it does not exist in optimized metadata.
			/// </summary>
			EventMap = 18,
			/// <summary>An event map-to-events lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
			EventPtr = 19,
			/// <summary>The EventMap and Event tables result from putting the .event directive on a class.</summary>
			Event = 20,
			/// <summary>The PropertyMap and Property tables result from putting the .property directive on a class.</summary>
			PropertyMap = 21,
			/// <summary>A property map-to-properties lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
			PropertyPtr = 22,
			/// <summary>Does a little more than group together existing rows from other tables.</summary>
			Property = 23,
			/// <summary>The rows of the MethodSemantics table are filled by .property and .event directives.</summary>
			MethodSemantics = 24,
			/// <summary>
			/// s let a compiler override the default inheritance rules provided by the CLI. Their original use 
			/// was to allow a class C, that inherited method M from both interfaces I and J, to provide implementations for 
			/// both methods (rather than have only one slot for M in its vtable). However, MethodImpls can be used for other 
			/// reasons too, limited only by the compiler writer‘s ingenuity within the constraints defined in the Validation rules.
			/// ILAsm uses the .override directive to specify the rows of the MethodImpl table.
			/// </summary>
			MethodImpl = 25,
			/// <summary>The rows in the ModuleRef table result from .module extern directives in the Assembly.</summary>
			ModuleRef = 26,
			/// <summary>
			/// The TypeSpec table has just one column, which indexes the specification of a Type, stored in the Blob heap.
			/// This provides a metadata token for that Type (rather than simply an index into the Blob heap).
			/// This is required, typically, for array operations, such as creating, or calling methods on the array class.
			/// </summary>
			/// <remarks>
			/// Note that TypeSpec tokens can be used with any of the CIL instructions that take a TypeDef or TypeRef token;
			/// specifically, castclass, cpobj, initobj, isinst, ldelema, ldobj, mkrefany, newarr, refanyval, sizeof, stobj, box, and unbox.
			/// </remarks>
			TypeSpec = 27,
			/// <summary>
			/// Holds information about unmanaged methods that can be reached from managed code, 
			/// using PInvoke dispatch. 
			/// A row is entered in the ImplMap table for each parent Method (§15.5) that is defined with a .pinvokeimpl
			/// interoperation attribute specifying the MappingFlags, ImportName, and ImportScope.
			/// </summary>
			ImplMap = 28,
			/// <summary>
			/// Conceptually, each row in the FieldRVA table is an extension to exactly one row in the Field table, and records 
			/// the RVA (Relative Virtual Address) within the image file at which this field‘s initial value is stored.
			/// A row in the FieldRVA table is created for each static parent field that has specified the optional data
			/// label. The RVA column is the relative virtual address of the data in the PE file.
			/// </summary>
			FieldRVA = 29,
			/// <summary>
			/// Edit-and-continue log descriptors that hold information about what changes have been made to specific metadata items during in-memory editing.
			/// This table does not exist in optimized metadata (#~ stream).
			/// </summary>
			ENCLog = 30,
			/// <summary>
			/// Edit-and-continue mapping descriptors.
			/// This table does not exist in optimized metadata (#~ stream).
			/// </summary>
			ENCMap = 31,
			/// <summary>The current assembly descriptor, whitch sould appear only in the prime moduel metadata.</summary>
			Assembly = 32,
			/// <summary>
			/// These records should not be emitted into any PE file.
			/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
			/// </summary>
			AssemblyProcessor = 33,
			/// <summary>
			/// These records should not be emitted into any PE file.
			/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
			/// </summary>
			AssemblyOS = 34,
			/// <summary>
			/// The table is defined by the .assembly extern directive (§6.3). Its columns are filled using directives 
			/// similar to those of the Assembly table except for the PublicKeyOrToken column, which is defined using the 
			/// .publickeytoken directive.
			/// </summary>
			AssemblyRef = 35,
			/// <summary>
			/// These records should not be emitted into any PE file.
			/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
			/// </summary>
			AssemblyRefProcessor = 36,
			/// <summary>
			/// These records should not be emitted into any PE file.
			/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
			/// </summary>
			AssemblyRefOS = 37,
			/// <summary>The rows of the File table result from .file directives in an Assembly.</summary>
			File = 38,
			/// <summary>
			/// Holds a row for each type:
			/// a. Defined within other modules of this Assembly; that is exported out of this Assembly.
			/// In essence, it  stores TypeDef row numbers of all types that are marked public in other modules that this Assembly comprises.
			/// The actual target row in a TypeDef table is given by the combination of TypeDefId (in effect, row 
			/// number) and Implementation (in effect, the module that holds the target TypeDef table). Note that this 
			/// is the only occurrence in metadata of foreign tokens; that is, token values that have a meaning in 
			/// another module. (A regular token value is an index into a table in the current module); OR
			/// b. Originally defined in this Assembly but now moved to another Assembly. Flags must have 
			/// IsTypeForwarder set and Implementation is an AssemblyRef indicating the Assembly the type may now be found in.
			/// </summary>
			ExportedType = 39,
			/// <summary>The rows in the table result from .mresource directives on the Assembly.</summary>
			ManifestResource = 40,
			/// <summary>NestedClass is defined as lexically 'inside' the text of its enclosing Type.</summary>
			NestedClass = 41,
			/// <summary>
			/// Stores the generic parameters used in generic type definitions and generic method definitions.
			/// These generic parameters can be constrained (i.e., generic arguments shall extend some class 
			/// and/or implement certain interfaces) or unconstrained.  (Such constraints are stored in the GenericParamConstraint table.)
			/// Conceptually, each row in the GenericParam table is owned by one, and only one, row in either the TypeDef or 
			/// MethodDef tables.
			/// </summary>
			GenericParam = 42,
			/// <summary>
			/// Records the signature of an instantiated generic method.
			/// Each unique instantiation of a generic method (i.e., a combination of Method and Instantiation)
			/// shall be represented by a single row in the table.
			/// </summary>
			MethodSpec = 43,
			/// <summary>
			/// Records the constraints for each generic parameter.
			/// Each generic parameter can be constrained to derive from zero or one class.
			/// Each generic parameter can be constrained to implement zero or more interfaces.
			/// Conceptually, each row in the GenericParamConstraint table is owned by a row in the GenericParam table.
			/// </summary>
			GenericParamConstraint = 44,
		}
	}
}