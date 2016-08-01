using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace AlphaOmega.Debug
{
	/// <summary>Структуры COFF/PE файла</summary>
	/// <remarks>Описание в WinNT.h</remarks>
	public struct WinNT
	{
		/// <summary>4 byte packing is the default</summary>
		public enum Signature4b : ushort
		{
			/// <summary>DOS signature</summary>
			IMAGE_DOS_SIGNATURE = 0x5A4D,// MZ
			/// <summary>OS/2 signature</summary>
			IMAGE_OS2_SIGNATURE = 0x454E,// NE
			/// <summary>Little Endian signature</summary>
			IMAGE_OS2_SIGNATURE_LE = 0x454C,// LE
			//IMAGE_VXD_SIGNATURE = 0x454C,// LE
			/// <summary>NT signature</summary>
			IMAGE_NT_SIGNATURE = 0x00004550,// PE00
		}

		/// <summary>The architecture type of the computer. An image file can only be run on the specified computer or a system that emulates the specified computer.</summary>
		public enum IMAGE_FILE_MACHINE : ushort
		{
			/// <summary>The contents of this field are assumed to be applicable to any machine type</summary>
			UNKNOWN = 0,
			/// <summary>Intel 860.</summary>
			I860 = 0x14d,
			/// <summary>Intel 386 or later processors and compatible processors</summary>
			I386 = 0x014c,
			/// <summary>MIPS little-endian, 0x160 big-endian</summary>
			R3000 = 0x0162,
			/// <summary>MIPS little endian</summary>
			R4000 = 0x0166,
			/// <summary>MIPS little-endian</summary>
			R10000 = 0x0168,
			/// <summary>MIPS little-endian WCE v2</summary>
			WCEMIPSV2 = 0x0169,
			/// <summary>Alpha_AXP</summary>
			ALPHA = 0x0184,
			/// <summary>Hitachi SH3</summary>
			SH3 = 0x01a2,
			/// <summary>Hitachi SH3 DSP</summary>
			SH3DSP = 0x01a3,
			/// <summary>SH3E little-endian</summary>
			SH3E = 0x01a4,
			/// <summary>Hitachi SH4</summary>
			SH4 = 0x01a6,
			/// <summary>Hitachi SH5</summary>
			SH5 = 0x01a8,
			/// <summary>ARM little endian</summary>
			ARM = 0x01c0,
			/// <summary>ARM or Thumb (“interworking”)</summary>
			THUMB = 0x01c2,
			/// <summary>ARMv7 (or higher) Thumb mode only</summary>
			ARMV7 = 0x01c4,
			/// <summary>Matsushita AM33</summary>
			AM33 = 0x01d3,
			/// <summary>Power PC little endian</summary>
			POWERPC = 0x01F0,
			/// <summary>Power PC with floating point support</summary>
			POWERPCFP = 0x01f1,
			/// <summary>Intel Itanium processor family</summary>
			IA64 = 0x0200,
			/// <summary>MIPS16</summary>
			MIPS16 = 0x0266,
			/// <summary>ALPHA64</summary>
			ALPHA64 = 0x0284,
			/// <summary>MIPS with FPU</summary>
			MIPSFPU = 0x0366,
			/// <summary>MIPS16 with FPU</summary>
			MIPSFPU16 = 0x0466,
			/// <summary>Infineon</summary>
			TRICORE = 0x0520,
			/// <summary>CEF</summary>
			CEF = 0x0CEF,
			/// <summary>EFI byte code</summary>
			EBC = 0x0ebc,
			/// <summary>AMD64 (K8)</summary>
			AMD64 = 0x8664,
			/// <summary>Mitsubishi M32R little endian</summary>
			M32R = 0x9041,
			/// <summary>CEE</summary>
			CEE = 0xC0EE,
		}

		/// <summary>The state of the image file.</summary>
		public enum IMAGE_SIGNATURE : ushort
		{
			/// <summary>The file is an executable 32bit version image.</summary>
			IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
			/// <summary>The file is an executable 64bit version image.</summary>
			/// <remarks>
			/// PE32+ images allow for a 64-bit address space while limiting the image size to 2 gigabytes.
			/// Other PE32+ modifications are addressed in their respective sections.
			/// </remarks>
			IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b,
			/// <summary>The file is a ROM image.</summary>
			IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107,
		}

		/// <summary>The subsystem required to run this image.</summary>
		public enum IMAGE_SUBSYSTEM : ushort
		{
			/// <summary>An unknown subsystem.</summary>
			UNKNOWN = 0,
			/// <summary>Device drivers and native Windows processes.</summary>
			NATIVE = 1,
			/// <summary>The Windows graphical user interface (GUI) subsystem.</summary>
			WINDOWS_GUI = 2,
			/// <summary>The Windows character subsystem.</summary>
			WINDOWS_CUI = 3,
			/// <summary>OS/2 CUI subsystem.</summary>
			OS2_CUI = 5,
			/// <summary>The Posix character (CUI) subsystem.</summary>
			POSIX_CUI = 7,
			/// <summary>Windows CE.</summary>
			WINDOWS_CE_GUI = 9,
			/// <summary>An Extensible Firmware Interface (EFI) application.</summary>
			EFI_APPLICATION = 10,
			/// <summary>An EFI driver with boot services.</summary>
			EFI_BOOT_SERVICE_DRIVER = 11,
			/// <summary>An EFI driver with run-time services.</summary>
			EFI_RUNTIME_DRIVER = 12,
			/// <summary>An EFI ROM image.</summary>
			EFI_ROM = 13,
			/// <summary>XBOX system.</summary>
			XBOX = 14,
			/// <summary>Boot application.</summary>
			WINDOWS_BOOT_APPLICATION = 16,
		}

		/// <summary>The DLL characteristics of the image.</summary>
		[Flags]
		public enum IMAGE_DLLCHARACTERISTICS : ushort
		{
			/// <summary>Reserved, must be zero.</summary>
			RES_0 = 0x0001,
			/// <summary>Reserved, must be zero.</summary>
			RES_1 = 0x0002,
			/// <summary>Reserved, must be zero.</summary>
			RES_2 = 0x0004,
			/// <summary>Reserved, must be zero.</summary>
			RES_3 = 0x0008,
			/// <summary>DLL can be relocated at load time.</summary>
			DYNAMIC_BASE = 0x0040,
			/// <summary>
			/// Code Integrity checks are enforced.
			/// If you set this flag and a section contains only uninitialized data,
			/// set the PointerToRawData member of <see cref="T:IMAGE_SECTION_HEADER"/> for that section to zero;
			/// otherwise, the image will fail to load because the digital signature cannot be verified.
			/// </summary>
			FORCE_INTEGRITY = 0x0080,
			/// <summary>The image is compatible with data execution prevention (DEP).</summary>
			NX_COMPAT = 0x0100,
			/// <summary>The image is isolation aware, but should not be isolated.</summary>
			NO_ISOLATION = 0x0200,
			/// <summary>The image does not use structured exception handling (SEH). No handlers can be called in this image.</summary>
			NO_SEH = 0x0400,
			/// <summary>Do not bind the image.</summary>
			NO_BIND = 0x0800,
			/// <summary>Reserved.</summary>
			RES_4 = 0x1000,
			/// <summary>A WDM driver.</summary>
			WDM_DRIVER = 0x2000,
			/// <summary>Reserved.</summary>
			RES_5 = 0x4000,
			/// <summary>The image is terminal server aware.</summary>
			TERMINAL_SERVER_AWARE = 0x8000,
		}

		/// <summary>The format of the debugging information</summary>
		public enum IMAGE_DEBUG_TYPE : uint
		{
			/// <summary>Unknown value, ignored by all tools.</summary>
			UNKNOWN = 0,
			/// <summary>COFF debugging information (line numbers, symbol table, and string table). This type of debugging information is also pointed to by fields in the file headers.</summary>
			COFF = 1,
			/// <summary>CodeView debugging information. The format of the data block is described by the CodeView 4.0 specification.</summary>
			CODEVIEW = 2,
			/// <summary>Frame pointer omission (FPO) information. This information tells the debugger how to interpret nonstandard stack frames, which use the EBP register for a purpose other than as a frame pointer.</summary>
			FPO = 3,
			/// <summary>The location of DBG file.</summary>
			MISC = 4,
			/// <summary>A copy of .pdata section.</summary>
			EXCEPTION = 5,
			/// <summary>Reserved.</summary>
			FIXUP = 6,
			/// <summary>The mapping from an RVA in image to an RVA in source image.</summary>
			OMAP_TO_SRC = 7,
			/// <summary>The mapping from an RVA in source image to an RVA in image.</summary>
			OMAP_FROM_SRC = 8,
			/// <summary>Reserved for Borland.</summary>
			BORLAND = 9,
			/// <summary>Reserved.</summary>
			RESERVED10 = 10,
			/// <summary>Reserved.</summary>
			CLSID = 11,
		}

		/// <summary>COM+ Header entry point flags.</summary>
		[Flags]
		public enum COMIMAGE_FLAGS : uint
		{
			/// <summary>
			/// The image contains IL code only, with no embedded native unmanaged code except the start-up stub (whitch simply executes an indirect jump to the CLR entry point).
			/// Common language runtime-aware operating systems (such as Windows XP and newer) ignore the start-up stub and invoke the CLR automatically, so for all practical purposes the file can be considered pure IL.
			/// Howewer, setting this flag can cause certain problems when running under Windows XP and newer.
			/// If this flag is set, the OS loader of Windows XP and newer ignores not only the start-up stub but also the .reloc section, whitch in this case contains single relocation (or single pair of relocations in IA64-specific images) for the CLR entry point.
			/// However, the .reloc section can contain relocations for the beginning and end of the .tls section as well as relocations for what is referred to as "data on data" (that is, data constants that are pointers to other data constants).
			/// Among existing managed compilers, only the VC++ and the IL assembler can produce this items.
			/// The VC++ of v7.0 and v7.1 (corresponding to CLR version 1.0 and 1.1) newer set this flag because the image file it generated was newer pure IL.
			/// In v2.0 this situation has changed, and currently, th VC++ and IL assembler are the only two capable of producing pure-IL image files that might require additional relocations in the .reloc section.
			/// To resolve this problem, the IL assembler, if TLS-based data or data on data is emitted, clears this flag and, if the target platform is 32-bit, sets the <see cref="T:_32BITREQUIRED"/> flag instead.
			/// </summary>
			ILONLY = 0x00000001,
			/// <summary>
			/// The image file can be loaded only into a 32-bit process.
			/// This flag is set alone when native unmanaged code is embedded in the PE file or when the .reloc section contains additional relocations or is set in combination with <see cref="T:ILONLY"/> when the executable does not contain additional relocations but is in some way 32-bit specific
			/// (for example, invokes an unmanaged 32-bit specific API or uses 4-byte integers to store pinters).
			/// </summary>
			_32BITREQUIRED = 0x00000002,
			/// <summary>
			/// This flag is obsolete and should not be set.
			/// Setting it as the IL assembler allows, using the .corflags directive - will render your module unloadable.
			/// </summary>
			IL_LIBRARY = 0x00000004,
			/// <summary>
			/// The image file is protected with strong name signature.
			/// The strong name signature includes the public key and the signature hash and is a part of an assembly's identity, along with the assembly name, version number, and culture information.
			/// This flag is set when the strong name signing procedure is applied to the image file.
			/// No compiler, including ILAsm, can set this flag explicity.
			/// </summary>
			STRONGNAMESIGNED = 0x00000008,
			/// <remarks>
			/// The executable's entry point is an unmanaged method.
			/// The EntryPointToken/EntryPointRVA field of CLR header contains the RVA of this native method.
			/// This flag was introduced in version 2.0 of the CLR.
			/// </remarks>
			NATIVE_ENTRYPOINT = 0x00000010,
			/// <summary>
			/// The CLR loader and the JIT compiler are required to track debug information about the methods.
			/// This flag is not used.
			/// </summary>
			TRACKDEBUGDATA = 0x00010000,
		}

		/// <summary>The characteristics of the image.</summary>
		[Flags]
		public enum IMAGE_FILE : ushort
		{
			/// <summary>
			/// Image only, Windows CE, and Windows NT® and later.
			/// This indicates that the file does not contain base relocations and must therefore be loaded at its preferred base address.
			/// If the base address is not available, the loader reports an error.
			/// The default behavior of the linker is to strip base relocations from executable (EXE) files.
			/// </summary>
			RELOCS_STRIPPED = 0x0001,
			/// <summary>
			/// Image only. This indicates that the image file is valid and can be run.
			/// If this flag is not set, it indicates a linker error.
			/// </summary>
			EXECUTABLE_IMAGE = 0x0002,
			/// <summary>COFF line numbers have been removed. This flag is deprecated and should be zero.</summary>
			LINE_NUMS_STRIPPED = 0x0004,
			/// <summary>COFF symbol table entries for local symbols have been removed. This flag is deprecated and should be zero.</summary>
			LOCAL_SYMS_STRIPPED = 0x0008,
			/// <summary>Obsolete. Aggressively trim working set. This flag is deprecated for Windows 2000 and later and must be zero.</summary>
			AGGRESIVE_WS_TRIM = 0x0010,
			/// <summary>Application can handle > 2 GB addresses.</summary>
			LARGE_ADDRESS_AWARE = 0x0020,
			/// <summary>This flag is reserved for future use.</summary>
			Reserved1=0x0040,
			/// <summary>Little endian: the least significant bit (LSB) precedes the most significant bit (MSB) in memory. This flag is deprecated and should be zero.</summary>
			BYTES_REVERSED_LO = 0x0080,
			/// <summary>Machine is based on a 32-bit-word architecture.</summary>
			_32BIT_MACHINE = 0x0100,
			/// <summary>Debugging information is removed from the image file.</summary>
			DEBUG_STRIPPED = 0x0200,
			/// <summary>If the image is on removable media, fully load it and copy it to the swap file.</summary>
			REMOVABLE_RUN_FROM_SWAP = 0x0400,
			/// <summary>If the image is on network media, fully load it and copy it to the swap file.</summary>
			NET_RUN_FROM_SWAP = 0x0800,
			/// <summary>The image file is a system file, not a user program.</summary>
			SYSTEM = 0x1000,
			/// <summary>The image file is a dynamic-link library (DLL). Such files are considered executable files for almost all purposes, although they cannot be directly run.</summary>
			DLL = 0x2000,
			/// <summary>The file should be run only on a uniprocessor machine.</summary>
			UP_SYSTEM_ONLY = 0x4000,
			/// <summary>Big endian: the MSB precedes the LSB in memory. This flag is deprecated and should be zero.</summary>
			BYTES_REVERSED_HI = 0x8000,
		}

		/// <summary>NT Directory types</summary>
		public enum IMAGE_DIRECTORY_ENTRY
		{
			/// <summary>The export table address and size.</summary>
			/// <remarks>Points to the exports (an <see cref="T:IMAGE_EXPORT_DIRECTORY"/> structure).</remarks>
			EXPORT = 0,
			/// <summary>The import table address and size.</summary>
			/// <remarks>Points to the imports (an array of <see cref="T:IMAGE_IMPORT_DESCRIPTOR"/> structures).</remarks>
			IMPORT = 1,
			/// <summary>The resource table address and size.</summary>
			/// <remarks>Points to the resources (an <see cref="T:IMAGE_RESOURCE_DIRECTORY"/> structure).</remarks>
			RESOURCE = 2,
			/// <summary>The exception table address and size.</summary>
			/// <remarks>
			/// Points to the exception handler table (an array of <see cref="T:IMAGE_RUNTIME_FUNCTION_ENTRY"/> structures).
			/// CPU-specific and for table-based exception handling. Used on every CPU except the x86.
			/// </remarks>
			EXCEPTION = 3,
			/// <summary>The attribute certificate table address and size.</summary>
			/// <remarks>
			/// Points to a list of <see cref="T:WIN_CERTIFICATE"/> structures, defined in WinTrust.H.
			/// Not mapped into memory as part of the image. Therefore, the VirtualAddress field is a file offset, rather than an RVA.
			/// </remarks>
			CERTIFICATE = 4,
			/// <summary>The base relocation table address and size.</summary>
			/// <remarks>Points to the base relocation information.</remarks>
			BASERELOC = 5,
			/// <summary>The debug data starting address and size.</summary>
			/// <remarks>
			/// Points to an array of <see cref="T:IMAGE_DEBUG_DIRECTORY"/> structures, each describing some debug information for the image.
			/// Early Borland linkers set the Size field of this <see cref="T:IMAGE_DATA_DIRECTORY"/> entry to the number of structures, rather than the size in bytes. To get the number of <see cref="T:IMAGE_DEBUG_DIRECTORY"/>s, divide the Size field by the size of an <see cref="T:IMAGE_DEBUG_DIRECTORY"/>.
			/// </remarks>
			DEBUG = 6,
			/// <summary>Reserved, must be 0</summary>
			/// <remarks>
			/// Points to architecture-specific data, which is an array of IMAGE_ARCHITECTURE_HEADER structures.
			/// Not used for x86 or IA-64, but appears to have been used for DEC/Compaq Alpha.
			/// </remarks>
			ARCHITECTURE = 7,
			/// <summary>
			/// The RVA of the value to be stored in the global pointer register.
			/// The size member of this structure must be set to zero.
			/// </summary>
			/// <remarks>
			/// The VirtualAddress field is the RVA to be used as the global pointer (gp) on certain architectures.
			/// Not used on x86, but is used on IA-64. The Size field isn't used.
			/// See the November 2000 Under The Hood column for more information on the IA-64 gp.
			/// </remarks>
			GLOBALPTR = 8,
			/// <summary>The thread local storage (TLS) table address and size.</summary>
			/// <remarks>Points to the Thread Local Storage initialization section.</remarks>
			TLS = 9,
			/// <summary>The load configuration table address and size.</summary>
			/// <remarks>
			/// Points to an IMAGE_LOAD_CONFIG_DIRECTORY structure.
			/// The information in an IMAGE_LOAD_CONFIG_DIRECTORY is specific to Windows NT, Windows 2000, and Windows XP (for example, the GlobalFlag value).
			/// To put this structure in your executable, you need to define a global structure with the name __load_config_used, and of type IMAGE_LOAD_CONFIG_DIRECTORY.
			/// For non-x86 architectures, the symbol name needs to be _load_config_used (with a single underscore).
			/// If you do try to include an IMAGE_LOAD_CONFIG_DIRECTORY, it can be tricky to get the name right in your C++ code.
			/// The symbol name that the linker sees must be exactly: __load_config_used (with two underscores).
			/// The C++ compiler adds an underscore to global symbols.
			/// In addition, it decorates global symbols with type information.
			/// So, to get everything right, in your C++ code, you'd have something like this: extern "C" IMAGE_LOAD_CONFIG_DIRECTORY _load_config_used = {...}
			/// </remarks>
			LOAD_CONFIG = 10,
			/// <summary>The bound import table address and size.</summary>
			/// <remarks>
			/// Points to an array of <see cref="T:IMAGE_BOUND_IMPORT_DESCRIPTOR"/>s, one for each DLL that this image has bound against.
			/// The timestamps in the array entries allow the loader to quickly determine whether the binding is fresh.
			/// If stale, the loader ignores the binding information and resolves the imported APIs normally.
			/// </remarks>
			BOUND_IMPORT = 11,
			/// <summary>The import address table address and size.</summary>
			/// <remarks>
			/// Points to the beginning of the first Import Address Table (IAT).
			/// The IATs for each imported DLL appear sequentially in memory.
			/// The Size field indicates the total size of all the IATs.
			/// The loader uses this address and size to temporarily mark the IATs as read-write during import resolution.
			/// </remarks>
			IAT = 12,
			/// <summary>The delay import descriptor address and size.</summary>
			/// <remarks>
			/// Points to the delayload information, which is an array of <see cref="T:ImgDelayDescr"/> structures, defined in DELAYIMP.H from Visual C++.
			/// Delayloaded DLLs aren't loaded until the first call to an API in them occurs.
			/// It's important to note that Windows has no implicit knowledge of delay loading DLLs.
			/// The delayload feature is completely implemented by the linker and runtime library.
			/// </remarks>
			DELAY_IMPORT = 13,
			/// <summary>The CLR runtime header address and size.</summary>
			/// <remarks>
			/// This value has been renamed to IMAGE_DIRECTORY_ENTRY_COMHEADER in more recent updates to the system header files.
			/// It points to the top-level information for .NET information in the executable, including metadata.
			/// This information is in the form of an <see cref="T:IMAGE_COR20_HEADER"/> structure.
			/// </remarks>
			CLR_HEADER = 14,
		}

		/// <summary>.NET Directory types</summary>
		public enum COR20_DIRECTORY_ENTRY
		{
			/// <summary>The RVA and size of the .NET resources.</summary>
			Resources = 0,
			/// <summary>The RVA of the strong name hash data.</summary>
			StrongNameSignature = 1,
			/// <summary>
			/// The RVA of the code manager table.
			/// A code manager contains the code required to obtain the state of a running program (such as tracing the stack and track GC references).
			/// </summary>
			CodeManagerTable = 2,
			/// <summary>
			/// The RVA of an array of function pointers that need fixups.
			/// This is for support of unmanaged C++ vtables.
			/// </summary>
			VTableFuxups = 3,
			/// <summary>
			/// The RVA to an array of RVAs where export JMP thunks are written.
			/// These thunks allow managed methods to be exported so that unmanaged code can call them.
			/// </summary>
			ExportAddressTableJumps = 4,
			/// <summary>For internal use of the .NET runtime in memory. Set to 0 in the executable.</summary>
			ManagedNativeHeaer = 5,
			/// <summary>
			/// The RVA to the metadata tables.
			/// Symbol table and startup information.</summary>
			/// <remarks>Pointer to <see cref="T:IMAGE_COR20_METADATA"/> section.</remarks>
			MetaData = 6,
		}

		/// <summary>Image section characteristics</summary>
		[Flags]
		public enum IMAGE_SCN : uint
		{
			/// <summary>Reserved for future use.</summary>
			TYPE_REG = 0x00000000,
			/// <summary>Reserved for future use.</summary>
			TYPE_DSECT = 0x00000001,
			/// <summary>Reserved for future use.</summary>
			TYPE_NOLOAD = 0x00000002,
			/// <summary>Reserved for future use.</summary>
			TYPE_GROUP = 0x00000004,
			/// <summary>
			/// The section should not be padded to the next boundary.
			/// This flag is obsolete and is replaced by <see cref="T:IMAGE_SCN.ALIGN_1BYTES"/>.
			/// This is valid only for object files.
			/// </summary>
			TYPE_NO_PAD = 0x00000008,
			/// <summary>Reserved for future use.</summary>
			TYPE_COPY = 0x00000010,
			/// <summary>The section contains executable code.</summary>
			CNT_CODE = 0x00000020,
			/// <summary>The section contains initialized data.</summary>
			CNT_INITIALIZED_DATA = 0x00000040,
			/// <summary>The section contains uninitialized data.</summary>
			CNT_UNINITIALIZED_DATA = 0x00000080,
			/// <summary>Reserved for future use.</summary>
			LNK_OTHER = 0x00000100,
			/// <summary>The section contains comments or other information. The .drectve section has this type. This is valid for object files only.</summary>
			LNK_INFO = 0x00000200,
			/// <summary>Reserved for future use.</summary>
			TYPE_OVER = 0x00000400,
			/// <summary>The section will not become part of the image. This is valid only for object files.</summary>
			LNK_REMOVE = 0x00000800,
			/// <summary>The section contains COMDAT data. For more information, see section 5.5.6, COMDAT Sections (Object Only). This is valid only for object files.</summary>
			LNK_COMDAT = 0x00001000,
			/// <summary>Reset speculative exceptions handling bits in the TLB entries for this section.</summary>
			NO_DEFER_SPEC_EXC = 0x00004000,
			/// <summary>The section contains data referenced through the global pointer (GP).</summary>
			MEM_FARDATA = 0x00008000,
			/// <summary>Obsolete</summary>
			MEM_SYSHEAP=0x00010000,
			/// <summary>Reserved for future use.</summary>
			MEM_PURGEABLE = 0x00020000,
			/// <summary>Reserved for future use.</summary>
			MEM_16BIT = 0x00020000,
			/// <summary>Reserved for future use.</summary>
			MEM_LOCKED = 0x00040000,
			/// <summary>Reserved for future use.</summary>
			MEM_PRELOAD = 0x00080000,
			/// <summary>Align data on a 1-byte boundary. Valid only for object files.</summary>
			ALIGN_1BYTES = 0x00100000,
			/// <summary>Align data on a 2-byte boundary. Valid only for object files.</summary>
			ALIGN_2BYTES = 0x00200000,
			/// <summary>Align data on a 4-byte boundary. Valid only for object files.</summary>
			ALIGN_4BYTES = 0x00300000,
			/// <summary>Align data on an 8-byte boundary. Valid only for object files.</summary>
			ALIGN_8BYTES = 0x00400000,
			/// <summary>Align data on a 16-byte boundary. Valid only for object files.</summary>
			/// <remarks>Default alignment if no others are specified.</remarks>
			ALIGN_16BYTES = 0x00500000,
			/// <summary>Align data on a 32-byte boundary. Valid only for object files.</summary>
			ALIGN_32BYTES = 0x00600000,
			/// <summary>Align data on a 64-byte boundary. Valid only for object files.</summary>
			ALIGN_64BYTES = 0x00700000,
			/// <summary>Align data on a 128-byte boundary. Valid only for object files.</summary>
			ALIGN_128BYTES = 0x00800000,
			/// <summary>Align data on a 256-byte boundary. Valid only for object files.</summary>
			ALIGN_256BYTES = 0x00900000,
			/// <summary>Align data on a 512-byte boundary. Valid only for object files.</summary>
			ALIGN_512BYTES = 0x00A00000,
			/// <summary>Align data on a 1024-byte boundary. Valid only for object files.</summary>
			ALIGN_1024BYTES = 0x00B00000,
			/// <summary>Align data on a 2048-byte boundary. Valid only for object files.</summary>
			ALIGN_2048BYTES = 0x00C00000,
			/// <summary>Align data on a 4096-byte boundary. Valid only for object files.</summary>
			ALIGN_4096BYTES = 0x00D00000,
			/// <summary>Align data on an 8192-byte boundary. Valid only for object files.</summary>
			ALIGN_8192BYTES = 0x00E00000,
			/// <summary>Mask?</summary>
			ALIGN_MASK=0x00F00000,
			/// <summary>The section contains extended relocations.</summary>
			LNK_NRELOC_OVFL = 0x01000000,
			/// <summary>The section can be discarded from the final executable. Used to hold information for the linker's use, including the .debug$ sections.</summary>
			MEM_DISCARDABLE = 0x02000000,
			/// <summary>The section cannot be cached.</summary>
			MEM_NOT_CACHED = 0x04000000,
			/// <summary>The section is not pageable, so it should always be physically present in memory. Often used for kernel-mode drivers.</summary>
			MEM_NOT_PAGED = 0x08000000,
			/// <summary>
			/// The physical pages containing this section's data will be shared between all processes that have this executable loaded.
			/// Thus, every process will see the exact same values for data in this section.
			/// Useful for making global variables shared between all instances of a process.
			/// To make a section shared, use the /section:name,S linker switch.
			/// </summary>
			MEM_SHARED = 0x10000000,
			/// <summary>The section can be executed as code.</summary>
			MEM_EXECUTE = 0x20000000,
			/// <summary>The section is readable. Almost always set.</summary>
			MEM_READ = 0x40000000,
			/// <summary>The section can be written to.</summary>
			MEM_WRITE = 0x80000000
		}
		/// <summary>Certificate revision number type</summary>
		public enum WIN_CERT_REVISION : ushort
		{
			/// <summary>
			/// Version 1, legacy version of the Win_Certificate structure.
			/// It is supported only for purposes of verifying legacy Authenticode signatures.
			/// </summary>
			REVISION_1_0 = 0x0100,
			/// <summary>Version 2 is the current version of the Win_Certificate structure.</summary>
			REVISION_2_0 = 0x0200,
		}
		/// <summary>Specifies the type of certificate.</summary>
		public enum WIN_CERT_TYPE : ushort
		{
			/// <summary>bCertificate contains an X.509 Certificate. Not supported.</summary>
			X509 = 1,
			/// <summary>bCertificate contains a PKCS#7 SignedData structure</summary>
			PKCS_SIGNED_DATA = 2,
			/// <summary>Reserved</summary>
			RESERVED_1 = 3,
			/// <summary>Terminal Server Protocol Stack Certificate signing.</summary>
			TS_STACK_SIGNED = 4,
			/// <summary>bCertificate contains PKCS1_MODULE_SIGN fields.</summary>
			PKCS1_SIGN = 9,
		}
		/// <summary>Delay Load Attributes</summary>
		public enum DLAttr : uint
		{
			/// <summary>Virtual Addresses used</summary>
			Va = 0,
			/// <summary>RVAs are used instead of pointers Having this set indicates a VC7.0 and above delay load descriptor.</summary>
			Rva = 1,
		}
		/// <summary>Based relocation types</summary>
		public enum IMAGE_REL_BASED
		{
			/// <summary>The base relocation is skipped. This type can be used to pad a block.</summary>
			/// <remarks>
			/// You will often see a relocation of type IMAGE_REL_BASED_ABSOLUTE at the end of a group of relocations.
			/// These relocations do nothing, and are there just to pad things so that the next IMAGE_BASE_RELOCATION is aligned on a 4-byte boundary.</remarks>
			ABSOLUTE = 0,
			/// <summary>
			/// The base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
			/// The 16-bit field represents the high value of a 32-bit word.
			/// </summary>
			/// <remarks>
			/// We must assume that high and low fixups occur in pairs,
			/// specifically a low fixup immediately follows a high fixup (normally separated by two bytes).
			/// We have to process the two fixups together,
			/// to find out the full pointer value and decide whether to apply the fixup.
			/// </remarks>
			HIGH = 1,
			/// <summary>
			/// The base relocation adds the low 16 bits of the difference to the 16-bit field at offset.
			/// The 16-bit field represents the low half of a 32-bit word.
			/// </summary>
			/// <remarks>Unless our assumption is wrong, all low word fixups should immediately follow a high fixup.</remarks>
			LOW = 2,
			/// <summary>The base relocation applies all 32 bits of the difference to the 32-bit field at offset.</summary>
			/// <remarks>
			/// For x86 executables, all base relocations are of type IMAGE_REL_BASED_HIGHLOW.
			/// Docs imply two words in big-endian order, so perhaps this is only used on big-endian platforms,
			/// in which case the obvious code will work.
			/// </remarks>
			HIGHLOW = 3,
			/// <summary>
			/// The base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
			/// The 16-bit field represents the high value of a 32-bit word.
			/// The low 16 bits of the 32-bit value are stored in the 16-bit word that follows this base relocation.
			/// This means that this base relocation occupies two slots.
			/// </summary>
			HIGHADJ = 4,
			/// <summary>For MIPS machine types, the base relocation applies to a MIPS jump instruction.</summary>
			MIPS_JMPADDR = 5,
			/// <summary>For ARM machine types, the base relocation applies the difference to the 32-bit value encoded in the immediate fields of a contiguous MOVW+MOVT pair in ARM mode at offset.</summary>
			ARM_MOV32A=5,
			/// <summary>Reserved, must be zero.</summary>
			RES1 = 6,
			/// <summary>The base relocation applies the difference to the 32-bit value encoded in the immediate fields of a contiguous MOVW+MOVT pair in Thumb mode at offset.</summary>
			ARM_MOV32T = 7,
			/// <summary>The base relocation applies to a MIPS16 jump instruction.</summary>
			MIPS_JMPADDR16 = 9,
			/// <summary>
			/// The fixup adds the high 16 bits of the delta to the 16-bit field at the offset.
			/// The 16-bit field is the high one-third of a 48-bit address.
			/// The low 32 bits of the address are stored in the 32-bit double word that follows this relocation.
			/// A fixup of this type occupies three slots.
			/// </summary>
			/// <remarks>For IA-64 executables, the relocations seem to always be of type IMAGE_REL_BASED_DIR64.</remarks>
			DIR64 = 10,
			/// <summary>
			/// Similar to IMAGE_REL_BASED_HIGHADJ except this is the third word.
			/// Adjust low half of high ULONG of an address and adjust for sign extension of the low ULONG.
			/// </summary>
			HIGH3ADJ = 11,
		}

		/// <summary>PE Dos header</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_DOS_HEADER
		{
			/// <summary>Magic number</summary>
			public Signature4b e_magic;

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

			/// <summary>String representation fo signature field</summary>
			public String SignatureStr { get { return Encoding.ASCII.GetString(BitConverter.GetBytes((UInt16)this.e_magic)).Replace("\0", "\\0"); } }

			/// <summary>Dos header is valid</summary>
			public Boolean IsValid { get { return this.e_magic == Signature4b.IMAGE_DOS_SIGNATURE; } }
		}

		/// <summary>OS/2 .EXE header</summary>
		/// <remarks>http://research.microsoft.com/en-us/um/redmond/projects/invisible/include/loaders/pe_image.h.htm</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IMAGE_OS2_HEADER
		{
			/// <summary>Magic number</summary>
			UInt16 ne_magic;
			/// <summary>Version number</summary>
			Byte ne_ver;
			/// <summary>Revision number</summary>
			Byte ne_rev;
			/// <summary>Offset of Entry Table</summary>
			UInt16 ne_enttab;
			/// <summary>Number of bytes in Entry Table</summary>
			UInt16 ne_cbenttab;
			/// <summary>Checksum of whole file</summary>
			Int32 ne_crc;
			/// <summary>Flag word</summary>
			UInt16 ne_flags;
			/// <summary>Automatic data segment number</summary>
			UInt16 ne_autodata;
			/// <summary>Initial heap allocation</summary>
			UInt16 ne_heap;
			/// <summary>Initial stack allocation</summary>
			UInt16 ne_stack;
			/// <summary>Initial CS:IP setting</summary>
			Int32 ne_csip;
			/// <summary>Initial SS:SP setting</summary>
			Int32 ne_sssp;
			/// <summary>Count of file segments</summary>
			UInt16 ne_cseg;
			/// <summary>Entries in Module Reference Table</summary>
			UInt16 ne_cmod;
			/// <summary>Size of non-resident name table</summary>
			UInt16 ne_cbnrestab;
			/// <summary>Offset of Segment Table</summary>
			UInt16 ne_segtab;
			/// <summary>Offset of Resource Table</summary>
			UInt16 ne_rsrctab;
			/// <summary>Offset of resident name table</summary>
			UInt16 ne_restab;
			/// <summary>Offset of Module Reference Table</summary>
			UInt16 ne_modtab;
			/// <summary>Offset of Imported Names Table</summary>
			UInt16 ne_imptab;
			/// <summary>Offset of Non-resident Names Table</summary>
			Int32 ne_nrestab;
			/// <summary>Count of movable entries</summary>
			UInt16 ne_cmovent;
			/// <summary>Segment alignment shift count</summary>
			UInt16 ne_align;
			/// <summary>Count of resource segments</summary>
			UInt16 ne_cres;
			/// <summary>Target Operating system</summary>
			Byte ne_exetyp;
			/// <summary>Other .EXE flags</summary>
			Byte ne_flagsothers;
			/// <summary>offset to return thunks</summary>
			UInt16 ne_pretthunks;
			/// <summary>offset to segment ref. bytes</summary>
			UInt16 ne_psegrefbytes;
			/// <summary>Minimum code swap area size</summary>
			UInt16 ne_swaparea;
			/// <summary>Expected Windows version number</summary>
			UInt16 ne_expver;
		};

		/// <summary>Represents the COFF header format.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_FILE_HEADER
		{
			/// <summary>
			/// The architecture type of the computer.
			/// An image file can only be run on the specified computer or a system that emulates the specified computer.
			/// </summary>
			public IMAGE_FILE_MACHINE Machine;

			/// <summary>The number of sections. This indicates the size of the section table, which immediately follows the headers. Note that the Windows loader limits the number of sections to 96.</summary>
			/// <remarks>The section table immediately follows the IMAGE_NT_HEADERS.</remarks>
			public UInt16 NumberOfSections;

			/// <summary>
			/// The low 32 bits of the time stamp of the image.
			/// This represents the date and time the image was created by the linker.
			/// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
			/// </summary>
			/// <remarks>
			/// This value is the number of seconds since January 1, 1970, Greenwich Mean Time (GMT).
			/// This value is a more accurate indicator of when the file was created than is the file system date/time.
			/// An easy way to translate this value into a human-readable string is with the _ctime function (which is time-zone-sensitive!).
			/// Another useful function for working with this field is gmtime.
			/// </remarks>
			public UInt32 TimeDateStamp;

			/// <summary>
			/// The file offset of the COFF symbol table, described in section 5.4 of the Microsoft specification.
			/// COFF symbol tables are relatively rare in PE files, as newer debug formats have taken over.
			/// Prior to Visual Studio .NET, a COFF symbol table could be created by specifying the linker switch /DEBUGTYPE:COFF.
			/// COFF symbol tables are almost always found in OBJ files. Set to 0 if no symbol table is present.
			/// </summary>
			public UInt32 PointerToSymbolTable;

			/// <summary>
			/// Number of symbols in the COFF symbol table, if present.
			/// COFF symbols are a fixed size structure, and this field is needed to find the end of the COFF symbols.
			/// Immediately following the COFF symbols is a string table used to hold longer symbol names.
			/// </summary>
			public UInt32 NumberOfSymbols;

			/// <summary>
			/// The size of the optional data that follows the <see cref="T:IMAGE_FILE_HEADER"/>.
			/// In PE files, this data is the IMAGE_OPTIONAL_HEADER.
			/// This size is different depending on whether it's a 32 or 64-bit file.
			/// For 32-bit PE files, this field is usually 224. For 64-bit PE32+ files, it's usually 240.
			/// However, these sizes are just minimum values, and larger values could appear.
			/// This value should be 0 for object files.
			/// </summary>
			public UInt16 SizeOfOptionalHeader;

			/// <summary>
			/// A set of bit flags indicating attributes of the file.
			/// Valid values of these flags are the <see cref="T:IMAGE_FILE"/> values defined in WINNT.H.
			/// </summary>
			public IMAGE_FILE Characteristics;

			/// <summary>This value is a more accurate indicator of when the file was created than is the file system date/time.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }

			/// <summary>File contains COFF symbol table.</summary>
			public Boolean ContainsSymbolTable { get { return this.PointerToSymbolTable > 0; } }

			/// <summary>OBJ file validation</summary>
			/// <remarks>OBJ file does not contains DOS or NT file header</remarks>
			public Boolean IsValid { get { return Enum.IsDefined(typeof(IMAGE_FILE_MACHINE), this.Machine); } }
		}

		/// <summary>obj symbol table structure</summary>
		[StructLayout(LayoutKind.Sequential,Pack=1)]
		[DebuggerDisplay("ShortName={SymbolTabeShortName}")]
		public struct IMAGE_COFF_SYMBOL
		{//http://research.microsoft.com/en-us/um/redmond/projects/invisible/include/loaders/pe_image.h.htm

			/// <summary>Short name</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public Char[] ShortName;

			/*union {
				UINT8   ShortName[8];
				struct {
					UINT32   Short;     // if 0, use LongName
					UINT32   Long;      // offset into string table
				} Name;
				PUINT8  LongName[2];
			} N;*/

			/// <summary>value</summary>
			public UInt32 Value;

			/// <summary>Section number</summary>
			public IMAGE_SYM SectionNumber;

			/// <summary>Type</summary>
			public IMAGE_SYM_TYPE Type;

			/// <summary>Storage class</summary>
			public IMAGE_SYM_CLASS StorageClass;

			/// <summary>Number of aux symbols</summary>
			public Byte NumberOfAuxSymbols;

			/// <summary>
			/// The ASCII name of the section.
			/// A section name is not guaranteed to be null-terminated.
			/// If you specify a section name longer than eight characters, the linker truncates it to eight characters in the executable.
			/// A mechanism exists for allowing longer section names in OBJ files.
			/// Section names often start with a period, but this is not a requirement.
			/// Section names with a $ in the name get special treatment from the linker.
			/// Sections with identical names prior to the $ character are merged.
			/// The characters following the $ provide an alphabetic ordering for how the merged sections appear in the final section.
			/// There's quite a bit more to the subject of sections with $ in the name and how they're combined, but the details are outside the scope of this article.
			/// </summary>
			public String SymbolTabeShortName { get { return new String(ShortName); } }
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		public struct IMAGE_AUX_SYMBOL
		{
			/// <summary></summary>
			[FieldOffset(0)]
			public UInt32 TagIndex;
			/*union {
				struct {
					UINT16  Linenumber;             // declaration line number
					UINT16  Size;                   // size of struct, union, or enum
				} LnSz;
			UINT32    TotalSize;
			} Misc;*/
			[FieldOffset(4)]
			public UInt32 TotalSize;
			/*union {
				struct {                            // if ISFCN, tag, or .bb
					UINT32    PointerToLinenumber;
					UINT32    PointerToNextFunction;
				} Function;
				struct {                            // if ISARY, up to 4 dimen.
					UINT16   Dimension[4];
				} Array;
			} FcnAry;*/
			[FieldOffset(8)]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public UInt16[] Dimention;

			[FieldOffset(16)]
			public UInt16 TvIndex;
		}

		/// <summary>Symbols have a section number of the section in which they are defined. Otherwise, section numbers have the following meanings</summary>
		public enum IMAGE_SYM : short
		{
			/// <summary>Symbol is undefined or is common</summary>
			UNDEFINED = 0,
			/// <summary>Symbol is an absolute value</summary>
			ABSOLUTE = -1,
			/// <summary>Symbol is a special debug item</summary>
			DEBUG = -2,
		}

		/// <summary>Type (fundamental) values.</summary>
		public enum IMAGE_SYM_TYPE : ushort
		{
			/// <summary>No type information or unknown base type.</summary>
			NULL = 0,
			/// <summary>Used with void pointers and functions.</summary>
			VOID = 1,
			/// <summary>A character (signed byte).</summary>
			CHAR = 2,
			/// <summary>A 2-byte signed integer.</summary>
			SHORT = 3,
			/// <summary>A natural integer type on the target.</summary>
			INT = 4,
			/// <summary>A 4-byte signed integer</summary>
			LONG = 5,
			/// <summary>A 4-byte floating-point number.</summary>
			FLOAT = 6,
			/// <summary>An 8-byte floating-point number.</summary>
			DOUBLE = 7,
			/// <summary>A structure.</summary>
			STRUCT = 8,
			/// <summary>An union.</summary>
			UNION = 9,
			/// <summary>An enumerated type.</summary>
			ENUM = 10,
			/// <summary>A member of enumeration (a specific value).</summary>
			MOE = 11,
			/// <summary>A byte; unsigned 1-byte integer.</summary>
			BYTE = 12,
			/// <summary>A word; unsigned 2-byte integer.</summary>
			WORD = 13,
			/// <summary>An unsigned integer of natural size.</summary>
			UINT = 14,
			/// <summary>An unsigned 4-byte integer.</summary>
			DWORD = 15,
		}

		/// <summary>Storage class tells where and what the symbol represents</summary>
		public enum IMAGE_SYM_CLASS : byte
		{
			// Physical end of function.
			//END_OF_FUNCTION = (Byte)(-1),
			/// <summary>No symbol.</summary>
			NULL = 0,
			/// <summary>Stack variable.</summary>
			AUTOMATIC = 1,
			/// <summary>External symbol.</summary>
			EXTERNAL = 2,
			/// <summary>Static.</summary>
			STATIC = 3,
			/// <summary>Register variable</summary>
			REGISTER = 4,
			/// <summary>External definition</summary>
			EXTERNAL_DEF = 5,
			/// <summary>Label</summary>
			LABEL = 6,
			/// <summary>Undefined label</summary>
			UNDEFINED_LABEL = 7,
			/// <summary>Member of structure</summary>
			MEMBER_OF_STRUCT = 8,
			/// <summary>Function argument</summary>
			ARGUMENT = 9,
			/// <summary>Structure tag</summary>
			STRUCT_TAG = 10,
			/// <summary>Member of union</summary>
			MEMBER_OF_UNION = 11,
			/// <summary>Union tag</summary>
			UNION_TAG = 12,
			/// <summary>Type definition</summary>
			TYPE_DEFINITION = 13,
			/// <summary>Undefined static</summary>
			UNDEFINED_STATIC = 14,
			/// <summary>Enumeration tag</summary>
			ENUM_TAG = 15,
			/// <summary>Member of enumeration</summary>
			MEMBER_OF_ENUM = 16,
			/// <summary>Register parameter</summary>
			REGISTER_PARAM = 17,
			/// <summary>Bit field</summary>
			BIT_FIELD = 18,
			/// <summary>".bb" or ".eb" - beginning or end of block</summary>
			BLOCK = 100,
			/// <summary>".bf" or ".ef" - beginning or end of function</summary>
			FUNCTION = 101,
			/// <summary>End of structure</summary>
			END_OF_STRUCT = 102,
			/// <summary>File name</summary>
			FILE = 103,
			/// <summary>Line number, reformatted as symbol</summary>
			SECTION = 104,
			/// <summary>Duplicate tag</summary>
			WEAK_EXTERNAL = 105,
			/// <summary>External symbol in dmert public lib</summary>
			CLR_TOKEN = 106,
		}

		/// <summary>Represents the COFF symbols header.</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms680301(v=vs.85).aspx</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_COFF_SYMBOLS_HEADER
		{
			/// <summary>The number of symbols.</summary>
			public UInt32 NumberOfSymbols;

			/// <summary>The virtual address of the first symbol.</summary>
			public UInt32 LvaToFirstSymbol;

			/// <summary>The number of line-number entries.</summary>
			public UInt32 NumberOfLinenumbers;

			/// <summary>The virtual address of the first line-number entry.</summary>
			public UInt32 LvaToFirstLinenumber;

			/// <summary>The relative virtual address of the first byte of code.</summary>
			public UInt32 RvaToFirstByteOfCode;

			/// <summary>The relative virtual address of the last byte of code.</summary>
			public UInt32 RvaToLastByteOfCode;

			/// <summary>The relative virtual address of the first byte of data.</summary>
			public UInt32 RvaToFirstByteOfData;

			/// <summary>The relative virtual address of the last byte of data.</summary>
			public UInt32 RvaToLastByteOfData;
		}

		/// <summary>Represents the image section header format.</summary>
		[StructLayout(LayoutKind.Explicit)]
		[DebuggerDisplay("Name={Section}")]
		public struct IMAGE_SECTION_HEADER
		{
			/// <summary>
			/// The ASCII name of the section.
			/// A section name is not guaranteed to be null-terminated.
			/// If you specify a section name longer than eight characters, the linker truncates it to eight characters in the executable.
			/// A mechanism exists for allowing longer section names in OBJ files.
			/// Section names often start with a period, but this is not a requirement.
			/// Section names with a $ in the name get special treatment from the linker.
			/// Sections with identical names prior to the $ character are merged.
			/// The characters following the $ provide an alphabetic ordering for how the merged sections appear in the final section.
			/// There's quite a bit more to the subject of sections with $ in the name and how they're combined, but the details are outside the scope of this article.
			/// </summary>
			[FieldOffset(0)]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public Char[] Name;

			/// <summary>
			/// Indicates the actual, used size of the section.
			/// This field may be larger or smaller than the SizeOfRawData field.
			/// If the VirtualSize is larger, the SizeOfRawData field is the size of the initialized data from the executable, and the remaining bytes up to the VirtualSize should be zero-padded.
			/// This field is set to 0 in OBJ files.
			/// </summary>
			[FieldOffset(8)]
			public UInt32 VirtualSize;

			/// <summary>In executables, indicates the RVA where the section begins in memory. Should be set to 0 in OBJs.</summary>
			[FieldOffset(12)]
			public UInt32 VirtualAddress;

			/// <summary>
			/// The size (in bytes) of data stored for the section in the executable or OBJ.
			/// For executables, this must be a multiple of the file alignment given in the PE header.
			/// If set to 0, the section is uninitialized data.
			/// </summary>
			[FieldOffset(16)]
			public UInt32 SizeOfRawData;

			/// <summary>
			/// The file offset where the data for the section begins.
			/// For executables, this value must be a multiple of the file alignment given in the PE header.
			/// </summary>
			[FieldOffset(20)]
			public UInt32 PointerToRawData;

			/// <summary>
			/// The file offset of relocations for this section.
			/// This is only used in OBJs and set to zero for executables.
			/// In OBJs, it points to an array of IMAGE_RELOCATION structures if non-zero.
			/// </summary>
			[FieldOffset(24)]
			public UInt32 PointerToRelocations;

			/// <summary>
			/// The file offset for COFF-style line numbers for this section.
			/// Points to an array of IMAGE_LINENUMBER structures if non-zero. Only used when COFF line numbers are emitted.
			/// </summary>
			[FieldOffset(28)]
			public UInt32 PointerToLinenumbers;

			/// <summary>
			/// The number of relocations pointed to by the PointerToRelocations field.
			/// Should be 0 in executables.
			/// </summary>
			[FieldOffset(32)]
			public UInt16 NumberOfRelocations;

			/// <summary>
			/// The number of line numbers pointed to by the NumberOfRelocations field.
			/// Only used when COFF line numbers are emitted.
			/// </summary>
			[FieldOffset(34)]
			public UInt16 NumberOfLinenumbers;

			/// <summary>
			/// Flags OR'ed together, indicating the attributes of this section.
			/// Many of these flags can be set with the linker's /SECTION option.
			/// </summary>
			[FieldOffset(36)]
			public IMAGE_SCN Characteristics;

			/// <summary>
			/// The ASCII name of the section.
			/// A section name is not guaranteed to be null-terminated.
			/// If you specify a section name longer than eight characters, the linker truncates it to eight characters in the executable.
			/// A mechanism exists for allowing longer section names in OBJ files.
			/// Section names often start with a period, but this is not a requirement.
			/// Section names with a $ in the name get special treatment from the linker.
			/// Sections with identical names prior to the $ character are merged.
			/// The characters following the $ provide an alphabetic ordering for how the merged sections appear in the final section.
			/// There's quite a bit more to the subject of sections with $ in the name and how they're combined, but the details are outside the scope of this article.
			/// </summary>
			public String Section { get { return new String(Name); } }
		}

		/// <summary>Represents the debug directory format.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_DEBUG_DIRECTORY
		{
			/// <summary>Reserved, must be zero.</summary>
			public UInt32 Characteristics;

			/// <summary>The time and date that the debug data was created.</summary>
			public UInt32 TimeDateStamp;

			/// <summary>The major version number of the debugging information format.</summary>
			public UInt16 MajorVersion;

			/// <summary>The minor version number of the debugging information format.</summary>
			public UInt16 MinorVersion;

			/// <summary>The format of debugging information. This field enables support of multiple debuggers. For more information, see section 6.1.2, "Debug Type".</summary>
			public IMAGE_DEBUG_TYPE Type;

			/// <summary>The size of the debug data (not including the debug directory itself).</summary>
			public UInt32 SizeOfData;

			/// <summary>The address of the debug data when loaded, relative to the image base.</summary>
			public UInt32 AddressOfRawData;

			/// <summary>The file pointer to the debug data.</summary>
			public UInt32 PointerToRawData;

			/// <summary>The time and date that the debug data was created.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }

			/// <summary>Version number of the debugging information format</summary>
			public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }
		}

		/// <summary>Native Resource directory</summary>
		public struct Resource
		{
			/// <summary>Resource directory</summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_RESOURCE_DIRECTORY
			{
				/// <summary>Unused</summary>
				public UInt32 Characteristics;

				/// <summary>Unused</summary>
				public UInt32 TimeDateStamp;

				/// <summary>Major directory version</summary>
				public UInt16 MajorVersion;

				/// <summary>Minor directory version</summary>
				public UInt16 MinorVersion;

				/// <summary>Number of subdirectories with names</summary>
				public UInt16 NumberOfNamedEntries;

				/// <summary>Number of subdirectories with IDs</summary>
				public UInt16 NumberOfIdEntries;

				/// <summary>Unused</summary>
				public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }

				/// <summary>Directory version</summary>
				public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }

				/// <summary>Contains subdirectories</summary>
				public Boolean ContainsEntries { get { return this.NumberOfEntries > 0; } }

				/// <summary>Number of subdirectories</summary>
				public UInt16 NumberOfEntries { get { return checked((UInt16)(this.NumberOfIdEntries + this.NumberOfNamedEntries)); } }
			}

			/// <summary>Resource directory description</summary>
			/// <remarks>
			/// Each directory contains the 32-bit Name of the entry and an offset,
			/// relative to the beginning of the resource directory of the data associated
			/// with this directory entry. If the name of the entry is an actual text
			/// string instead of an integer Id, then the high order bit of the name field
			/// is set to one and the low order 31-bits are an offset, relative to the
			/// beginning of the resource directory of the string, which is of type <see cref="T:IMAGE_RESOURCE_DIRECTORY_STRING"/>.
			/// Otherwise the high bit is clear and the low-order 16-bits are the integer Id that identify this resource directory
			/// entry. If the directory entry is yet another resource directory (i.e. a
			/// subdirectory), then the high order bit of the offset field will be
			/// set to indicate this. Otherwise the high bit is clear and the offset field points to a resource data entry.
			/// </remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_RESOURCE_DIRECTORY_ENTRY
			{
				/// <summary>Directory name offset</summary>
				public UInt32 NameOffset;

				/// <summary>Directory data offset</summary>
				public UInt32 OffsetToData;

				/// <summary>Directory name is string</summary>
				public Boolean IsNameString { get { return (this.NameOffset & 0x80000000) > 0; } }

				/// <summary>Адрес на структуру <see cref="T:IMAGE_RESOURCE_DIRECTORY_STRING"/> или идентификатор папки.</summary>
				public UInt32 NameAddress { get { return this.NameOffset & 0x7FFFFFFF; } }

				/// <summary>Тип корневой директории</summary>
				public RESOURCE_DIRECTORY_TYPE NameType { get { return this.IsNameString ? RESOURCE_DIRECTORY_TYPE.Undefined : (RESOURCE_DIRECTORY_TYPE)this.NameAddress; } }

				/// <summary>Entry is directory</summary>
				public Boolean IsDirectory { get { return (this.OffsetToData & 0x80000000) > 0; } }

				/// <summary>Адрес на <see cref="T:IMAGE_RESOURCE_DIRECTORY"/> или на <see cref="T:WinNT.IMAGE_RESOURCE_DATA_ENTRY"/></summary>
				public UInt32 DirectoryAddress { get { return this.OffsetToData & 0x7FFFFFFF; } }

				/// <summary>Адрес на структуру <see cref="T:WinNT.IMAGE_RESOURCE_DATA_ENTRY"/></summary>
				public Boolean IsDataEntry { get { return !this.IsNameString && !this.IsDirectory; } }
			}

			/// <summary>The following are the predefined resource types.</summary>
			public enum RESOURCE_DIRECTORY_TYPE
			{
				/// <summary>Undefined</summary>
				Undefined = 0,
				/// <summary>Hardware-dependent cursor resource.</summary>
				RT_CURSOR = 1,
				/// <summary>Bitmap resource.</summary>
				RT_BITMAP = 2,
				/// <summary>Hardware-dependent icon resource.</summary>
				RT_ICON = 3,
				/// <summary>Menu resource.</summary>
				RT_MENU = 4,
				/// <summary>Dialog box.</summary>
				RT_DIALOG = 5,
				/// <summary>String-table entry.</summary>
				RT_STRING = 6,
				/// <summary>Font directory resource.</summary>
				RT_FONTDIR = 7,
				/// <summary>Font resource.</summary>
				RT_FONT = 8,
				/// <summary>Accelerator table.</summary>
				RT_ACCELERATOR = 9,
				/// <summary>Application-defined resource (raw data).</summary>
				RT_RCDATA = 10,
				/// <summary>Message-table entry.</summary>
				RT_MESSAGETABLE = 11,
				/// <summary>Hardware-independent cursor resource.</summary>
				RT_GROUP_CURSOR2 = 12,
				/// <summary>Hardware-independent cursor resource.</summary>
				RT_GROUP_CURSOR4 = 14,
				/// <summary>Version resource.</summary>
				RT_VERSION = 16,
				/// <summary>
				/// Allows a resource editing tool to associate a string with an .rc file.
				/// Typically, the string is the name of the header file that provides symbolic names.
				/// The resource compiler parses the string but otherwise ignores the value.
				/// For example, 1 DLGINCLUDE "MyFile.h"
				/// </summary>
				RT_DLGINCLUDE = 17,
				/// <summary>Plug and Play resource.</summary>
				RT_PLUGPLAY = 19,
				/// <summary>VXD.</summary>
				RT_VXD = 20,
				/// <summary>Animated cursor.</summary>
				RT_ANICURSOR = 21,
				/// <summary>Animated icon.</summary>
				RT_ANIICON = 22,
				/// <summary>HTML resource.</summary>
				RT_HTML = 23,
				/// <summary>Side-by-Side Assembly Manifest.</summary>
				RT_MANIFEST = 24,
				/// <summary>MFC CDialog</summary>
				RT_DLGINIT = 240,
				/// <summary>MFC CToolBarCtrl</summary>
				RT_TOOLBAR = 241,
			}

			/// <summary></summary>
			/// <remarks>
			/// Each resource data entry describes a leaf node in the resource directory tree.
			/// It contains an offset, relative to the beginning of the resource
			/// directory of the data for the resource, a size field that gives the number
			/// of bytes of data at that offset, a CodePage that should be used when
			/// decoding code point values within the resource data.
			/// Typically for new applications the code page would be the unicode code page.
			/// </remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_RESOURCE_DATA_ENTRY
			{
				/// <summary>
				/// The OffsetToData and Size fields specify the location (as a relative virtual address within the resource section) and size (in bytes) of the resource data.
				/// Although an RVA is not the same as a file offset, the equivalent file offset can be calculated by subtracting the resource section's RVA from OffsetToData's RVA value, and adding the difference to the offset of the root directory.
				/// </summary>
				public UInt32 OffsetToData;

				/// <summary>Size of resource directory</summary>
				public UInt32 Size;

				/// <summary>
				/// The CodePage field identifies the code page (a coded character set) used to decode code points (code page values) within the resource data.
				/// Although any valid code page number can appear in this field (such as 437, which describes the original IBM PC's character set, or 65501, which describes Unicode UTF-8),
				/// this field often contains 0 (standard Roman alphabet, numerals, punctuation, accented characters).
				/// </summary>
				public UInt32 CodePage;

				/// <summary>Reserved. Must be zero.</summary>
				public UInt32 Reserved;

				/// <summary>CodePage string representation</summary>
				public String CodePageString { get { return Encoding.GetEncoding((Int32)this.CodePage).EncodingName; } }
			}
			/// <summary>Directory name</summary>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct IMAGE_RESOURCE_DIRECTORY_STRING
			{
				/// <summary>Name length</summary>
				public UInt16 Length;
				/// <summary>Name string</summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
				public String NameString;
				/// <summary>NameString может закончится НЕ \0. Поэтому правильно отрезать лишнюю информацию</summary>
				public String Name { get { return this.NameString.Substring(0, this.Length); } }
			}

			/// <summary>Represents the organization of data in a file-version resource. It is the root structure that contains all other file-version information structures.</summary>
			/// <remarks>
			/// This structure is not a true C-language structure because it contains variable-length members.
			/// This structure was created solely to depict the organization of data in a version resource and does not appear in any of the header files shipped with the Windows Software Development Kit (SDK).
			/// </remarks>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct VS_VERSIONINFO
			{
				/// <summary>The length, in bytes, of the VS_VERSIONINFO structure. This length does not include any padding that aligns any subsequent version resource data on a 32-bit boundary.</summary>
				public UInt16 wLength;
				/// <summary>The length, in bytes, of the Value member. This value is zero if there is no Value member associated with the current version structure. </summary>
				public UInt16 wValueLength;
				/// <summary>The type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. </summary>
				public VersionDataType wType;
				/// <summary>The Unicode string L"VS_VERSION_INFO". </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
				public String szKey;
				/// <summary>Contains as many zero words as necessary to align the Value member on a 32-bit boundary. </summary>
				public UInt16 Padding1;
				/*/// <summary>As many zero words as necessary to align the Children member on a 32-bit boundary. These bytes are not included in wValueLength. This member is optional.</summary>
				public UInt16 Padding2;
				/// <summary>An array of zero or one StringFileInfo structures, and zero or one VarFileInfo structures that are children of the current VS_VERSIONINFO structure.</summary>
				public UInt16 Children;*/
			}

			/// <summary>Типы данных в версии</summary>
			public enum VersionDataType : short
			{
				/// <summary>Бинарные данных</summary>
				Binary = 0,
				/// <summary>Текстовые данных</summary>
				Text = 1,
			}

			/// <summary>Represents the organization of data in a file-version resource. It contains version information that can be displayed for a particular language and code page.</summary>
			[StructLayout(LayoutKind.Sequential, Pack = 2)]
			public struct VarFileInfo
			{
				/// <summary>The length, in bytes, of the entire StringFileInfo block, including all structures indicated by the Children member. </summary>
				public UInt16 wLength;
				/// <summary>This member is always equal to zero. </summary>
				public UInt16 wValueLength;
				/// <summary>The type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. </summary>
				public VersionDataType wType;
				/// <summary>Size of next value</summary>
				/// <exception cref="T:NotImplementedException">Only Text data type supported</exception>
				public UInt16 ValueLength
				{
					get
					{
						switch(this.wType)
						{
						case VersionDataType.Text:
							return (UInt16)(this.wValueLength * 2);
						default: throw new NotImplementedException();
						}
					}
				}
			}
			/// <summary>Represents the organization of data in a file-version resource. It contains version information that can be displayed for a particular language and code page.</summary>
			[StructLayout(LayoutKind.Sequential, Pack = 2)]
			public struct StringTable
			{
				/// <summary>The length, in bytes, of this StringTable structure, including all structures indicated by the Children member.</summary>
				public UInt16 wLength;
				/// <summary>This member is always equal to zero.</summary>
				public UInt16 wValueLength;
				/// <summary>The type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. </summary>
				public VersionDataType wType;
				// <summary>
				// An 8-digit hexadecimal number stored as a Unicode string.
				// The four most significant digits represent the language identifier.
				// The four least significant digits represent the code page for which the data is formatted.
				// Each Microsoft Standard Language identifier contains two parts:
				// the low-order 10 bits specify the major language, and the high-order 6 bits specify the sublanguage.
				// For a table of valid identifiers see .
				// </summary>
				//public String szKey;
				// <summary>As many zero words as necessary to align the Children member on a 32-bit boundary.</summary>
				//public UInt16 Padding;
			}

			/// <summary>
			/// Represents the organization of data in a file-version resource.
			/// It contains a string that describes a specific aspect of a file, for example, a file's version, its copyright notices, or its trademarks.
			/// </summary>
			[StructLayout(LayoutKind.Sequential, Pack = 2)]
			public struct V_STRING
			{
				/// <summary>The length, in bytes, of this String structure. </summary>
				public UInt16 wLength;
				/// <summary>The size, in words, of the Value member. </summary>
				public UInt16 wValueLength;
				/// <summary>
				/// The type of data in the version resource.
				/// This member is 1 if the version resource contains text data and 0 if the version resource contains binary data.
				/// </summary>
				public VersionDataType wType;
				// <summary>An arbitrary Unicode string.</summary>
				//public String szKey;
				// <summary>As many zero words as necessary to align the Value member on a 32-bit boundary.</summary>
				//public UInt16 Padding;
				// <summary>A zero-terminated string. See the szKey member description for more information.</summary>
				//public Byte[] Value;
				/// <summary>Length of the value</summary>
				/// <exception cref="T:NotImplementedException">Only Text data type supported</exception>
				public UInt16 ValueLength
				{
					get
					{
						switch(this.wType)
						{
						case VersionDataType.Text:
							return (UInt16)(this.wValueLength * 2);
						default: throw new NotImplementedException();
						}
					}
				}
			}

			/// <summary>Contains information about message strings with identifiers in the range indicated by the LowId and HighId members.</summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct MESSAGE_RESOURCE_BLOCK
			{
				/// <summary>The lowest message identifier contained within this structure.</summary>
				public UInt32 LowId;
				/// <summary>The highest message identifier contained within this structure.</summary>
				public UInt32 HighId;
				/// <summary>
				/// The offset, in bytes, from the beginning of the MESSAGE_RESOURCE_DATA structure to the MESSAGE_RESOURCE_ENTRY structures in this MESSAGE_RESOURCE_BLOCK.
				/// The MESSAGE_RESOURCE_ENTRY structures contain the message strings.</summary>
				public UInt32 OffsetToEntries;
			}

			/// <summary>Contains the error message or message box display text for a message table resource. </summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct MESSAGE_RESOURCE_ENTRY
			{
				/// <summary>The length, in bytes, of the MESSAGE_RESOURCE_ENTRY structure. </summary>
				public UInt16 Length;
				/// <summary>
				/// Indicates that the string is encoded in Unicode, if equal to the value 0x0001.
				/// Indicates that the string is encoded in ANSI, if equal to the value 0x0000.
				/// </summary>
				public ResourceEncodingType Flags;
				/// <summary>Pointer to an array that contains the error message or message box display text.</summary>
				//public Byte[] Text;
				public UInt16 MessageLength { get { return (UInt16)(this.Length - Marshal.SizeOf(this)); } }
			}

			/// <summary>Type of resource encoding</summary>
			public enum ResourceEncodingType : short
			{
				/// <summary>ANSI encoding</summary>
				Ansi = 0,
				/// <summary>Unicode encoding</summary>
				Unicode = 1,
			}
		}

		/// <summary>COM+ 2.0 header structure.</summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_COR20_HEADER
		{
			/// <summary>Size of the header in bytes.</summary>
			[FieldOffset(0)]
			public UInt32 cb;
			/// <summary>The minimum version of the runtime required to run this program. For the first release of .NET, this value is 1.</summary>
			[FieldOffset(4)]
			public UInt16 MajorRuntimeVersion;
			/// <summary>The minor portion of the version. Currently 0.</summary>
			[FieldOffset(6)]
			public UInt16 MinorRuntimeVersion;
			/// <summary>
			/// The RVA to the metadata tables.
			/// Symbol table and startup information.</summary>
			/// <remarks>Pointer to <see cref="T:IMAGE_COR20_METADATA"/> section.</remarks>
			[FieldOffset(8)]
			public IMAGE_DATA_DIRECTORY MetaData;
			/// <summary>Flag values containing attributes for this image.</summary>
			[FieldOffset(16)]
			public COMIMAGE_FLAGS Flags;
			/// <summary>Token for the MethodDef of the entry point for the image. The .NET runtime calls this method to begin managed execution in the file.</summary>
			/// <remarks>
			/// If COMIMAGE_FLAGS.NATIVE_ENTRYPOINT is not set, EntryPointToken represents a managed entrypoint.
			/// If COMIMAGE_FLAGS.NATIVE_ENTRYPOINT is set, EntryPointRVA represents an RVA to a native entrypoint.</remarks>
			[FieldOffset(20)]
			public UInt32 EntryPointToken;
			/// <summary>DataDirectories</summary>
			[FieldOffset(24)]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public IMAGE_DATA_DIRECTORY[] DataDirectory;
			/// <summary>DataDirectories</summary>
			public IMAGE_DATA_DIRECTORY this[WinNT.COR20_DIRECTORY_ENTRY entry]
			{
				get
				{
					switch(entry)
					{
					case COR20_DIRECTORY_ENTRY.MetaData:
						return this.MetaData;
					default:
						return this.DataDirectory[(Int32)entry];
					}
				}
			}
			/// <summary>Version of the runtime required to run this program.</summary>
			public Version RuntimeVersion { get { return new Version(this.MajorRuntimeVersion, this.MinorRuntimeVersion); } }
		}

		/// <summary>Import directory header</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_IMPORT_DESCRIPTOR
		{
			/// <summary>
			/// At one time, this may have been a set of flags.
			/// However, Microsoft changed its meaning and never bothered to update WINNT.H. This field is really an offset (an RVA) to an array of pointers. Each of these pointers points to an <see cref="T:IMAGE_IMPORT_BY_NAME"/> structure.
			/// </summary>
			public UInt32 Characteristics;
			/// <summary>The time/date stamp indicating when the file was built.</summary>
			public UInt32 TimeDateStamp;
			/// <summary>
			/// This field relates to forwarding.
			/// Forwarding involves one DLL sending on references to one of its functions to another DLL.
			/// For example, in Windows NT, NTDLL.DLL appears to forward some of its exported functions to KERNEL32.DLL.
			/// An application may think it's calling a function in NTDLL.DLL, but it actually ends up calling into KERNEL32.DLL.
			/// This field contains an index into FirstThunk array (described momentarily).
			/// The function indexed by this field will be forwarded to another DLL.
			/// Unfortunately, the format of how a function is forwarded isn't documented, and examples of forwarded functions are hard to find.
			/// </summary>
			public UInt32 ForwarderChain;
			/// <summary>This is an RVA to a NULL-terminated ASCII string containing the imported DLL's name. Common examples are "KERNEL32.DLL" and "USER32.DLL".</summary>
			public UInt32 Name;
			/// <summary>
			/// This field is an offset (an RVA) to an IMAGE_THUNK_DATA union.
			/// In almost every case, the union is interpreted as a pointer to an <see cref="T:IMAGE_IMPORT_BY_NAME"/> structure.
			/// If the field isn't one of these pointers, then it's supposedly treated as an export ordinal value for the DLL that's being imported.
			/// It's not clear from the documentation if you really can import a function by ordinal rather than by name. 
			/// </summary>
			public UInt32 FirstThunk;
			/// <summary>The time/date stamp indicating when the file was built.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }
			/// <summary>Structure is empty</summary>
			public Boolean IsEmpty { get { return this.Name == 0; } }
		}

		/// <summary>Imported function from the image</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_THUNK_DATA32
		{
			/// <summary>RVA to a forwarder string.</summary>
			public UInt32 ForwarderString;
			/// <summary>Memory address of the imported function.</summary>
			public UInt32 Function;
			/// <summary>Ordinal value of imported API.</summary>
			public UInt32 Ordinal;
			/// <summary>RVA to an <see cref="T:IMAGE_IMPORT_BY_NAME"/> with the imported API name.</summary>
			public UInt32 AddressOfData;
		}
		/// <summary>Imported function from the image</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_THUNK_DATA64
		{
			/// <summary>RVA to a forwarder string.</summary>
			public UInt64 ForwarderString;
			/// <summary>Memory address of the imported function.</summary>
			public UInt64 Function;
			/// <summary>Ordinal value of imported API.</summary>
			public UInt64 Ordinal;
			/// <summary>RVA to an <see cref="T:IMAGE_IMPORT_BY_NAME"/> with the imported API name.</summary>
			public UInt64 AddressOfData;
		}
		/// <summary>Import Format</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_IMPORT_BY_NAME
		{
			/// <summary>Ordinal index in exported module.</summary>
			public UInt16 Hint;
			/// <summary>ASCII string with the name of the imported function.</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
			public String Name;
			/// <summary>Function is loaded by Ordinal index</summary>
			public Boolean IsByOrdinal { get { return this.Name == null; } }
		}

		/// <summary>
		/// Some architectures (including the IA-64) don't use frame-based exception handling, like the x86 does; instead, they used table-based exception handling in which there is a table containing information about every function that might be affected by exception unwinding.
		/// The data for each function includes the starting address, the ending address, and information about how and where the exception should be handled.
		/// When an exception occurs, the system searches through the tables to locate the appropriate entry and handles it.
		/// The exception table is an array of <see cref="T:IMAGE_RUNTIME_FUNCTION_ENTRY"/> structures.
		/// The array is pointed to by the IMAGE_DIRECTORY_ENTRY_EXCEPTION entry in the DataDirectory.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-US/library/ft9x1kdx%28v=vs.80%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_RUNTIME_FUNCTION_ENTRY
		{
			/// <summary>Address of the first instruction in the function. It is the function's entry address.</summary>
			public UInt32 BeginAddress;
			/// <summary>Address of the last instruction in the function. It is the function's end address.</summary>
			public UInt32 EndAddress;
			/// <summary>
			/// The UnwindInfoAddress can have two different meanings.
			/// First, it can be a reference to another runtime function entry in .pdata.
			/// This is indicated, that the LSB is set to one.
			/// </summary>
			public UInt32 UnwindInfoAddress;
			/// <summary>Этот ряд последний.</summary>
			public Boolean IsLatEntry { get { return (this.UnwindInfoAddress & 0xF) > 0; } }
		}

		/// <summary>Represents the data directory.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_DATA_DIRECTORY
		{
			/// <summary>The relative virtual address of the table.</summary>
			public UInt32 VirtualAddress;
			/// <summary>The size of the table, in bytes.</summary>
			public UInt32 Size;
			/// <summary>Data directory contains information.</summary>
			/// <remarks>Директория проверяется по полю Size. У некоторых директорий поде Size игнорируется. См. документацию.</remarks>
			public Boolean IsEmpty { get { return this.Size == 0; } }
		}

		/// <summary>Optional header format</summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_OPTIONAL_HEADER32
		{
			/// <summary>
			/// A signature WORD, identifying what type of header this is.
			/// The two most common values are IMAGE_NT_OPTIONAL_HDR32_MAGIC 0x10b and IMAGE_NT_OPTIONAL_HDR64_MAGIC 0x20b.
			/// </summary>
			[FieldOffset(0)]
			public IMAGE_SIGNATURE Magic;

			/// <summary>
			/// The major version of the linker used to build this executable.
			/// For PE files from the Microsoft linker, this version number corresponds to the Visual Studio version number (for example, version 6 for Visual Studio 6.0).
			/// </summary>
			[FieldOffset(2)]
			public Byte MajorLinkerVersion;

			/// <summary>The minor version of the linker used to build this executable.</summary>
			[FieldOffset(3)]
			public Byte MinorLinkerVersion;

			/// <summary>The combined total size of all sections with the IMAGE_SCN_CNT_CODE attribute.</summary>
			[FieldOffset(4)]
			public UInt32 SizeOfCode;

			/// <summary>The combined size of all initialized data sections.</summary>
			[FieldOffset(8)]
			public UInt32 SizeOfInitializedData;

			/// <summary>
			/// The size of all sections with the uninitialized data attributes.
			/// This field will often be 0, since the linker can append uninitialized data to the end of regular data sections.
			/// </summary>
			[FieldOffset(12)]
			public UInt32 SizeOfUninitializedData;

			/// <summary>
			/// The RVA of the first code byte in the file that will be executed.
			/// For DLLs, this entrypoint is called during process initialization and shutdown and during thread creations/destructions.
			/// In most executables, this address doesn't directly point to main, WinMain, or DllMain.
			/// Rather, it points to runtime library code that calls the aforementioned functions.
			/// This field can be set to 0 in DLLs, and none of the previous notifications will be received.
			/// The linker /NOENTRY switch sets this field to 0.
			/// </summary>
			[FieldOffset(16)]
			public UInt32 AddressOfEntryPoint;

			/// <summary>The RVA of the first byte of code when loaded in memory.</summary>
			[FieldOffset(20)]
			public UInt32 BaseOfCode;

			/// <summary>
			/// Theoretically, the RVA of the first byte of data when loaded into memory.
			/// However, the values for this field are inconsistent with different versions of the Microsoft linker.
			/// This field is not present in 64-bit executables.
			/// </summary>
			/// <remarks>PE32 contains this additional field</remarks>
			[FieldOffset(24)]
			public UInt32 BaseOfData;//TODO: Это поле пропущено в IMAGE_OPTIONAL_HEADER64

			/// <summary>
			/// The preferred load address of this file in memory.
			/// The loader attempts to load the PE file at this address if possible (that is, if nothing else currently occupies that memory, it's aligned properly and at a legal address, and so on).
			/// If the executable loads at this address, the loader can skip the step of applying base relocations (described in Part 2 of this article).
			/// For EXEs, the default ImageBase is 0x400000. For DLLs, it's 0x10000000.
			/// The ImageBase can be set at link time with the /BASE switch, or later with the REBASE utility.
			/// </summary>
			[FieldOffset(28)]
			public UInt32 ImageBase;

			/// <summary>
			/// The alignment of sections when loaded into memory.
			/// The alignment must be greater or equal to the file alignment field (mentioned next).
			/// The default alignment is the page size of the target CPU.
			/// For user mode executables to run under Windows 9x or Windows Me, the minimum alignment size is a page (4KB).
			/// This field can be set with the linker /ALIGN switch.
			/// </summary>
			[FieldOffset(32)]
			public UInt32 SectionAlignment;

			/// <summary>
			/// The alignment of sections within the PE file.
			/// For x86 executables, this value is usually either 0x200 or 0x1000.
			/// The default has changed with different versions of the Microsoft linker.
			/// This value must be a power of 2, and if the SectionAlignment is less than the CPU's page size, this field must match the SectionAlignment.
			/// The linker switch /OPT:WIN98 sets the file alignment on x86 executables to 0x1000, while /OPT:NOWIN98 sets the alignment to 0x200.
			/// </summary>
			[FieldOffset(36)]
			public UInt32 FileAlignment;

			/// <summary>
			/// The major version number of the required operating system.
			/// With the advent of so many versions of Windows, this field has effectively become irrelevant.
			/// </summary>
			[FieldOffset(40)]
			public UInt16 MajorOperatingSystemVersion;

			/// <summary>The minor version number of the required OS.</summary>
			[FieldOffset(42)]
			public UInt16 MinorOperatingSystemVersion;

			/// <summary>
			/// The major version number of this file.
			/// Unused by the system and can be 0. It can be set with the linker /VERSION switch.
			/// </summary>
			[FieldOffset(44)]
			public UInt16 MajorImageVersion;

			/// <summary>The minor version number of this file.</summary>
			[FieldOffset(46)]
			public UInt16 MinorImageVersion;

			/// <summary>
			/// The major version of the operating subsystem needed for this executable.
			/// At one time, it was used to indicate that the newer Windows 95 or Windows NT 4.0 user interface was required, as opposed to older versions of the Windows NT interface.
			/// Today, because of the proliferation of the various versions of Windows, this field is effectively unused by the system and is typically set to the value 4.
			/// Set with the linker /SUBSYSTEM switch.
			/// </summary>
			[FieldOffset(48)]
			public UInt16 MajorSubsystemVersion;

			/// <summary>The minor version of the operating subsystem needed for this executable.</summary>
			[FieldOffset(50)]
			public UInt16 MinorSubsystemVersion;

			/// <summary>Reserved, must be zero.</summary>
			[FieldOffset(52)]
			public UInt32 Win32VersionValue;

			/// <summary>
			/// SizeOfImage contains the RVA that would be assigned to the section following the last section if it existed.
			/// This is effectively the amount of memory that the system needs to reserve when loading this file into memory.
			/// This field must be a multiple of the section alignment.
			/// </summary>
			[FieldOffset(56)]
			public UInt32 SizeOfImage;

			/// <summary>
			/// The combined size of the MS-DOS header, PE headers, and section table.
			/// All of these items will occur before any code or data sections in the PE file.
			/// The value of this field is rounded up to a multiple of the file alignment.
			/// </summary>
			[FieldOffset(60)]
			public UInt32 SizeOfHeaders;

			/// <summary>
			/// The checksum of the image.
			/// The CheckSumMappedFile API in IMAGEHLP.DLL can calculate this value.
			/// Checksums are required for kernel-mode drivers and some system DLLs.
			/// Otherwise, this field can be 0.
			/// The checksum is placed in the file when the /RELEASE linker switch is used.
			/// </summary>
			[FieldOffset(64)]
			public UInt32 CheckSum;

			/// <summary>
			/// An enum value indicating what subsystem (user interface type) the executable expects. This field is only important for EXEs. Important values include:
			/// IMAGE_SUBSYSTEM_NATIVE       // Image doesn't require a subsystem
			/// IMAGE_SUBSYSTEM_WINDOWS_GUI  // Use the Windows GUI
			/// IMAGE_SUBSYSTEM_WINDOWS_CUI  // Run as a console mode application
			/// When run, the OS creates a console window for it, and provides stdin, stdout, and stderr file handles.
			/// </summary>
			[FieldOffset(68)]
			public IMAGE_SUBSYSTEM Subsystem;

			/// <summary>
			/// Flags indicating characteristics of this DLL. These correspond to the IMAGE_DLLCHARACTERISTICS_xxx fields #defines. Current values are:
			/// IMAGE_DLLCHARACTERISTICS_NO_BIND - Do not bind this image
			/// IMAGE_DLLCHARACTERISTICS_WDM_DRIVER - Driver uses WDM model
			/// IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE - When the terminal server loads an application that is not Terminal- Services-aware, it also loads a DLL that contains compatibility code.
			/// </summary>
			[FieldOffset(70)]
			public IMAGE_DLLCHARACTERISTICS DllCharacteristics;

			/// <summary>
			/// The number of bytes to reserve for the stack.
			/// Only the memory specified by the SizeOfStackCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
			/// </summary>
			/// <remarks>
			/// In EXE files, the maximum size the initial thread in the process can grow to.
			/// This is 1MB by default. Not all this memory is committed initially.
			/// </remarks>
			[FieldOffset(72)]
			public UInt32 SizeOfStackReserve;

			/// <summary>The number of bytes to commit for the stack.</summary>
			/// <remarks>In EXE files, the amount of memory initially committed to the stack. By default, this field is 4KB.</remarks>
			[FieldOffset(76)]
			public UInt32 SizeOfStackCommit;

			/// <summary>
			/// The number of bytes to reserve for the local heap.
			/// Only the memory specified by the SizeOfHeapCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
			/// </summary>
			/// <remarks>
			/// In EXE files, the initial reserved size of the default process heap.
			/// This is 1MB by default.
			/// In current versions of Windows, however, the heap can grow beyond this size without intervention by the user.
			/// </remarks>
			[FieldOffset(80)]
			public UInt32 SizeOfHeapReserve;

			/// <summary>The number of bytes to commit for the local heap.</summary>
			/// <remarks>In EXE files, the size of memory committed to the heap. By default, this is 4KB.</remarks>
			[FieldOffset(84)]
			public UInt32 SizeOfHeapCommit;

			/// <summary>Reserved, must be zero.</summary>
			[FieldOffset(88)]
			public UInt32 LoaderFlags;

			/// <summary>
			/// The number of directory entries in the remainder of the optional header.
			/// Each entry describes a location and size.
			/// </summary>
			/// <remarks>
			/// At the end of the IMAGE_NT_HEADERS structure is an array of IMAGE_DATA_DIRECTORY structures.
			/// This field contains the number of entries in the array.
			/// This field has been 16 since the earliest releases of Windows NT.
			/// </remarks>
			[FieldOffset(92)]
			public UInt32 NumberOfRvaAndSizes;

			/// <summary>DataDirectories</summary>
			[FieldOffset(96)]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public IMAGE_DATA_DIRECTORY[] DataDirectory;

			/// <summary>DataDirectories</summary>
			public IMAGE_DATA_DIRECTORY this[WinNT.IMAGE_DIRECTORY_ENTRY entry]
			{
				get { return this.DataDirectory[(Int32)entry]; }
			}

			/// <summary>Version of the linker used to build this executable</summary>
			public Version LinkerVersion { get { return new Version(this.MajorLinkerVersion, this.MinorLinkerVersion); } }

			/// <summary>Version number of the required operating system</summary>
			public Version OperatingSystemVersion { get { return new Version(this.MajorOperatingSystemVersion, this.MinorOperatingSystemVersion); } }

			/// <summary>Version number of this file</summary>
			public Version ImageVersion { get { return new Version(this.MajorImageVersion, this.MinorImageVersion); } }

			/// <summary>Version of the operating subsystem needed for this executable</summary>
			public Version SubsystemVersion { get { return new Version(this.MajorSubsystemVersion, this.MinorSubsystemVersion); } }
		}

		/// <summary>Optional header format</summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_OPTIONAL_HEADER64
		{
			/// <summary>
			/// A signature WORD, identifying what type of header this is.
			/// The two most common values are IMAGE_NT_OPTIONAL_HDR32_MAGIC 0x10b and IMAGE_NT_OPTIONAL_HDR64_MAGIC 0x20b.
			/// </summary>
			[FieldOffset(0)]
			public IMAGE_SIGNATURE Magic;

			/// <summary>
			/// The major version of the linker used to build this executable.
			/// For PE files from the Microsoft linker, this version number corresponds to the Visual Studio version number (for example, version 6 for Visual Studio 6.0).
			/// </summary>
			[FieldOffset(2)]
			public Byte MajorLinkerVersion;

			/// <summary>The minor version of the linker used to build this executable.</summary>
			[FieldOffset(3)]
			public Byte MinorLinkerVersion;

			/// <summary>The combined total size of all sections with the IMAGE_SCN_CNT_CODE attribute.</summary>
			[FieldOffset(4)]
			public UInt32 SizeOfCode;

			/// <summary>The combined size of all initialized data sections.</summary>
			[FieldOffset(8)]
			public UInt32 SizeOfInitializedData;

			/// <summary>
			/// The size of all sections with the uninitialized data attributes.
			/// This field will often be 0, since the linker can append uninitialized data to the end of regular data sections.
			/// </summary>
			[FieldOffset(12)]
			public UInt32 SizeOfUninitializedData;

			/// <summary>
			/// The RVA of the first code byte in the file that will be executed.
			/// For DLLs, this entrypoint is called during process initialization and shutdown and during thread creations/destructions.
			/// In most executables, this address doesn't directly point to main, WinMain, or DllMain.
			/// Rather, it points to runtime library code that calls the aforementioned functions.
			/// This field can be set to 0 in DLLs, and none of the previous notifications will be received.
			/// The linker /NOENTRY switch sets this field to 0.
			/// </summary>
			[FieldOffset(16)]
			public UInt32 AddressOfEntryPoint;

			/// <summary>The RVA of the first byte of code when loaded in memory.</summary>
			[FieldOffset(20)]
			public UInt32 BaseOfCode;

			/// <summary>
			/// The preferred load address of this file in memory.
			/// The loader attempts to load the PE file at this address if possible (that is, if nothing else currently occupies that memory, it's aligned properly and at a legal address, and so on).
			/// If the executable loads at this address, the loader can skip the step of applying base relocations (described in Part 2 of this article).
			/// For EXEs, the default ImageBase is 0x400000. For DLLs, it's 0x10000000.
			/// The ImageBase can be set at link time with the /BASE switch, or later with the REBASE utility.
			/// </summary>
			[FieldOffset(24)]
			public UInt64 ImageBase;

			/// <summary>
			/// The alignment of sections when loaded into memory.
			/// The alignment must be greater or equal to the file alignment field (mentioned next).
			/// The default alignment is the page size of the target CPU.
			/// For user mode executables to run under Windows 9x or Windows Me, the minimum alignment size is a page (4KB).
			/// This field can be set with the linker /ALIGN switch.
			/// </summary>
			[FieldOffset(32)]
			public UInt32 SectionAlignment;

			/// <summary>
			/// The alignment of sections within the PE file.
			/// For x86 executables, this value is usually either 0x200 or 0x1000.
			/// The default has changed with different versions of the Microsoft linker.
			/// This value must be a power of 2, and if the SectionAlignment is less than the CPU's page size, this field must match the SectionAlignment.
			/// The linker switch /OPT:WIN98 sets the file alignment on x86 executables to 0x1000, while /OPT:NOWIN98 sets the alignment to 0x200.
			/// </summary>
			[FieldOffset(36)]
			public UInt32 FileAlignment;

			/// <summary>
			/// The major version number of the required operating system.
			/// With the advent of so many versions of Windows, this field has effectively become irrelevant.
			/// </summary>
			[FieldOffset(40)]
			public UInt16 MajorOperatingSystemVersion;

			/// <summary>The minor version number of the required OS.</summary>
			[FieldOffset(42)]
			public UInt16 MinorOperatingSystemVersion;

			/// <summary>
			/// The major version number of this file.
			/// Unused by the system and can be 0. It can be set with the linker /VERSION switch.
			/// </summary>
			[FieldOffset(44)]
			public UInt16 MajorImageVersion;

			/// <summary>The minor version number of this file.</summary>
			[FieldOffset(46)]
			public UInt16 MinorImageVersion;

			/// <summary>
			/// The major version of the operating subsystem needed for this executable.
			/// At one time, it was used to indicate that the newer Windows 95 or Windows NT 4.0 user interface was required, as opposed to older versions of the Windows NT interface.
			/// Today, because of the proliferation of the various versions of Windows, this field is effectively unused by the system and is typically set to the value 4.
			/// Set with the linker /SUBSYSTEM switch.
			/// </summary>
			[FieldOffset(48)]
			public UInt16 MajorSubsystemVersion;

			/// <summary>The minor version of the operating subsystem needed for this executable.</summary>
			[FieldOffset(50)]
			public UInt16 MinorSubsystemVersion;

			/// <summary>Another field that never took off. Typically set to 0.</summary>
			[FieldOffset(52)]
			public UInt32 Win32VersionValue;

			/// <summary>
			/// SizeOfImage contains the RVA that would be assigned to the section following the last section if it existed.
			/// This is effectively the amount of memory that the system needs to reserve when loading this file into memory.
			/// This field must be a multiple of the section alignment.
			/// </summary>
			[FieldOffset(56)]
			public UInt32 SizeOfImage;

			/// <summary>
			/// The combined size of the MS-DOS header, PE headers, and section table.
			/// All of these items will occur before any code or data sections in the PE file.
			/// The value of this field is rounded up to a multiple of the file alignment.
			/// </summary>
			[FieldOffset(60)]
			public UInt32 SizeOfHeaders;

			/// <summary>
			/// The checksum of the image.
			/// The CheckSumMappedFile API in IMAGEHLP.DLL can calculate this value.
			/// Checksums are required for kernel-mode drivers and some system DLLs.
			/// Otherwise, this field can be 0.
			/// The checksum is placed in the file when the /RELEASE linker switch is used.
			/// </summary>
			[FieldOffset(64)]
			public UInt32 CheckSum;

			/// <summary>
			/// An enum value indicating what subsystem (user interface type) the executable expects. This field is only important for EXEs. Important values include:
			/// IMAGE_SUBSYSTEM_NATIVE       // Image doesn't require a subsystem
			/// IMAGE_SUBSYSTEM_WINDOWS_GUI  // Use the Windows GUI
			/// IMAGE_SUBSYSTEM_WINDOWS_CUI  // Run as a console mode application
			/// When run, the OS creates a console window for it, and provides stdin, stdout, and stderr file handles.
			/// </summary>
			[FieldOffset(68)]
			public IMAGE_SUBSYSTEM Subsystem;

			/// <summary>
			/// Flags indicating characteristics of this DLL. These correspond to the IMAGE_DLLCHARACTERISTICS_xxx fields #defines. Current values are:
			/// IMAGE_DLLCHARACTERISTICS_NO_BIND - Do not bind this image
			/// IMAGE_DLLCHARACTERISTICS_WDM_DRIVER - Driver uses WDM model
			/// IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE - When the terminal server loads an application that is not Terminal- Services-aware, it also loads a DLL that contains compatibility code.
			/// </summary>
			[FieldOffset(70)]
			public IMAGE_DLLCHARACTERISTICS DllCharacteristics;

			/// <summary>
			/// The number of bytes to reserve for the stack.
			/// Only the memory specified by the SizeOfStackCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
			/// </summary>
			/// <remarks>
			/// In EXE files, the maximum size the initial thread in the process can grow to.
			/// This is 1MB by default. Not all this memory is committed initially.
			/// </remarks>
			[FieldOffset(72)]
			public UInt64 SizeOfStackReserve;

			/// <summary>The number of bytes to commit for the stack.</summary>
			/// <remarks>In EXE files, the amount of memory initially committed to the stack. By default, this field is 4KB.</remarks>
			[FieldOffset(80)]
			public UInt64 SizeOfStackCommit;

			/// <summary>
			/// The number of bytes to reserve for the local heap.
			/// Only the memory specified by the SizeOfHeapCommit member is committed at load time; the rest is made available one page at a time until this reserve size is reached.
			/// </summary>
			/// <remarks>
			/// In EXE files, the initial reserved size of the default process heap.
			/// This is 1MB by default.
			/// In current versions of Windows, however, the heap can grow beyond this size without intervention by the user.
			/// </remarks>
			[FieldOffset(88)]
			public UInt64 SizeOfHeapReserve;

			/// <summary>The number of bytes to commit for the local heap.</summary>
			/// <remarks>In EXE files, the size of memory committed to the heap. By default, this is 4KB.</remarks>
			[FieldOffset(96)]
			public UInt64 SizeOfHeapCommit;

			/// <summary>This member is obsolete.</summary>
			[FieldOffset(104)]
			public UInt32 LoaderFlags;

			/// <summary>
			/// The number of directory entries in the remainder of the optional header.
			/// Each entry describes a location and size.
			/// </summary>
			/// <remarks>
			/// At the end of the IMAGE_NT_HEADERS structure is an array of IMAGE_DATA_DIRECTORY structures.
			/// This field contains the number of entries in the array.
			/// This field has been 16 since the earliest releases of Windows NT.
			/// </remarks>
			[FieldOffset(108)]
			public UInt32 NumberOfRvaAndSizes;

			/// <summary>DataDirectories</summary>
			[FieldOffset(112)]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public IMAGE_DATA_DIRECTORY[] DataDirectory;

			/// <summary>DataDirectories</summary>
			public IMAGE_DATA_DIRECTORY this[WinNT.IMAGE_DIRECTORY_ENTRY entry]
			{
				get { return this.DataDirectory[(Int32)entry]; }
			}

			/// <summary>Version of the linker used to build this executable</summary>
			public Version LinkerVersion { get { return new Version(this.MajorLinkerVersion, this.MinorLinkerVersion); } }

			/// <summary>Version number of the required operating system</summary>
			public Version OperatingSystemVersion { get { return new Version(this.MajorOperatingSystemVersion, this.MinorOperatingSystemVersion); } }

			/// <summary>Version number of this file</summary>
			public Version ImageVersion { get { return new Version(this.MajorImageVersion, this.MinorImageVersion); } }

			/// <summary>Version of the operating subsystem needed for this executable</summary>
			public Version SubsystemVersion { get { return new Version(this.MajorSubsystemVersion, this.MinorSubsystemVersion); } }
		}

		/// <summary>Represents the PE header format.</summary>
		/// <remarks>The actual structure in Winnt.h is named IMAGE_NT_HEADERS32 and IMAGE_NT_HEADERS is defined as IMAGE_NT_HEADERS32. However, if _WIN64 is defined, then IMAGE_NT_HEADERS is defined as IMAGE_NT_HEADERS64.</remarks>
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_NT_HEADERS32
		{
			/// <summary>A 4-byte signature identifying the file as a PE image. The bytes are "PE\0\0".</summary>
			[FieldOffset(0)]
			public UInt32 Signature;
			/*[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public Char[] Signature;*/

			/// <summary>An IMAGE_FILE_HEADER structure that specifies the file header.</summary>
			[FieldOffset(4)]
			public IMAGE_FILE_HEADER FileHeader;

			/// <summary>An IMAGE_OPTIONAL_HEADER structure that specifies the optional file header.</summary>
			[FieldOffset(24)]
			public IMAGE_OPTIONAL_HEADER32 OptionalHeader;

			/// <summary>Valid NT header</summary>
			public Boolean IsValid
			{
				get { return this.Signature == 0x00004550/*"PE\0\0"*/ && (this.OptionalHeader.Magic == WinNT.IMAGE_SIGNATURE.IMAGE_NT_OPTIONAL_HDR32_MAGIC/* || this.OptionalHeader.Magic == ImageHlp.ImageSignatureType.IMAGE_NT_OPTIONAL_HDR64_MAGIC*/); }
			}

			/// <summary>Signature as String</summary>
			public String SignatureStr
			{
				get { return Encoding.ASCII.GetString(BitConverter.GetBytes(this.Signature)).Replace("\0", "\\0"); }
			}
		}

		/// <summary>Represents the PE header format.</summary>
		/// <remarks>The actual structure in Winnt.h is named IMAGE_NT_HEADERS32 and IMAGE_NT_HEADERS is defined as IMAGE_NT_HEADERS32. However, if _WIN64 is defined, then IMAGE_NT_HEADERS is defined as IMAGE_NT_HEADERS64.</remarks>
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_NT_HEADERS64
		{
			/// <summary>A 4-byte signature identifying the file as a PE image. The bytes are "PE\0\0".</summary>
			[FieldOffset(0)]
			public UInt32 Signature;
			/*[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public Char[] Signature;*/

			/// <summary>An IMAGE_FILE_HEADER structure that specifies the file header.</summary>
			[FieldOffset(4)]
			public IMAGE_FILE_HEADER FileHeader;

			/// <summary>An IMAGE_OPTIONAL_HEADER structure that specifies the optional file header.</summary>
			[FieldOffset(24)]
			public IMAGE_OPTIONAL_HEADER64 OptionalHeader;

			/// <summary>Valid NT header</summary>
			public Boolean IsValid
			{
				get { return this.Signature == 0x00004550/*"PE\0\0"*/ && (/*this.OptionalHeader.Magic == ImageHlp.ImageSignatureType.IMAGE_NT_OPTIONAL_HDR32_MAGIC || */this.OptionalHeader.Magic == WinNT.IMAGE_SIGNATURE.IMAGE_NT_OPTIONAL_HDR64_MAGIC); }
			}

			/// <summary>Signature as String</summary>
			public String SignatureStr
			{
				get { return Encoding.ASCII.GetString(BitConverter.GetBytes(this.Signature)).Replace("\0", "\\0"); }
			}
		}

		/// <summary>Export Format</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_EXPORT_DIRECTORY
		{
			/// <summary>Reserved, must be 0.</summary>
			public UInt32 ExportFlags;

			/// <summary>The time and date that the export data was created.</summary>
			public UInt32 TimeDateStamp;

			/// <summary>The major version number.</summary>
			/// <remarks>The major and minor version numbers can be set by the user.</remarks>
			public UInt16 MajorVersion;

			/// <summary>The minor version number.</summary>
			/// /// <remarks>The major and minor version numbers can be set by the user.</remarks>
			public UInt16 MinorVersion;

			/// <summary>The address of the ASCII string that contains the name of the DLL. This address is relative to the image base.</summary>
			public UInt32 NameRva;

			/// <summary>
			/// This field contains the starting ordinal value to be used for this executable's exports.
			/// Normally, this value is 1, but it's not required to be so.
			/// When looking up an export by ordinal, the value of this field is subtracted from the ordinal, with the result used as a zero-based index into the Export Address Table (EAT).
			/// </summary>
			public UInt32 OrdinalBase;

			/// <summary>The number of entries in the EAT. Note that some entries may be 0, indicating that no code/data is exported with that ordinal value.</summary>
			public UInt32 NumberOfFunctions;

			/// <summary>The number of entries in the Export Names Table (ENT). This value will always be less than or equal to the NumberOf-Functions field. It will be less when there are symbols exported by ordinal only. It can also be less if there are numeric gaps in the assigned ordinals. This field is also the size of the export ordinal table (below).</summary>
			public UInt32 NumberOfNames;

			/// <summary>The RVA of the EAT. The EAT is an array of RVAs. Each nonzero RVA in the array corresponds to an exported symbol.</summary>
			public UInt32 AddressOfFunctions;

			/// <summary>The RVA of the ENT. The ENT is an array of RVAs to ASCII strings. Each ASCII string corresponds to a symbol exported by name. This table is sorted so that the ASCII strings are in order. This allows the loader to do a binary search when looking for an exported symbol. The sorting of the names is binary (like the C++ RTL strcmp function provides), rather than a locale-specific alphabetic ordering.</summary>
			public UInt32 AddressOfNames;

			/// <summary>The RVA of the export ordinal table. This table is an array of WORDs. This table maps an array index from the ENT into the corresponding export address table entry.</summary>
			public UInt32 AddressOfNameOrdinals;

			/// <summary>The time and date that the export data was created.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }

			/// <summary>Version number</summary>
			public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }
		}

		/// <summary>Represents the time/date stamp of one imported DLL that has been bound against.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_BOUND_IMPORT_DESCRIPTOR
		{
			/// <summary>Contains the time/date stamp of the imported DLL.</summary>
			public UInt32 TimeDateStamp;

			/// <summary>Offset to a string with the name of the imported DLL. </summary>
			public UInt16 OffsetModuleName;

			/// <summary>Number of <see cref="T:IMAGE_BOUND_FORWARDER_REF"/> structures that immediately follow this structure.</summary>
			public UInt16 NumberOfModuleForwarderRefs;

			/// <summary>Contains the time/date stamp of the imported DLL.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }
		}

		/// <summary>Forwarded DLL references</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_BOUND_FORWARDER_REF
		{
			/// <summary>Contains the time/date stamp of the imported DLL.</summary>
			public UInt32 TimeDateStamp;

			/// <summary>Offset to a string with the name of the imported DLL.</summary>
			public UInt16 OffsetModuleName;

			/// <summary>Reserved</summary>
			public UInt16 Reserved;

			/// <summary>Contains the time/date stamp of the imported DLL.</summary>
			public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }
		}
		/// <summary>This structure encapsulates a signature used in verifying executable files.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct WIN_CERTIFICATE
		{
			/// <summary>Specifies the length, in bytes, of the signature.</summary>
			public UInt32 dwLength;

			/// <summary>Specifies the certificate revision.</summary>
			/// <remarks>The only defined certificate revision is WIN_CERT_REVISION_1_0 (0x0100).</remarks>
			public WIN_CERT_REVISION wRevision;

			/// <summary>Specifies the type of certificate.</summary>
			public WIN_CERT_TYPE wCertificateType;   // WIN_CERT_TYPE_xxx

			// <summary>An array of certificates.</summary>
			// <remarks>The format of this member depends on the value of wCertificateType.</remarks>
			//public Byte bCertificate;
		}
		/// <summary>Delay Load info</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct ImgDelayDescr
		{
			/// <summary>
			/// The attributes for this structure.
			/// Currently, the only flag defined is dlattrRva (1), indicating that the address fields in the structure should be treated as RVAs, rather than virtual addresses.
			/// </summary>
			public DLAttr grAttrs;

			/// <summary>An RVA to a string with the name of the imported DLL. This string is passed to LoadLibrary.</summary>
			public UInt32 rvaDLLName;

			/// <summary>
			/// An RVA to an HMODULE-sized memory location.
			/// When the Delayloaded DLL is brought into memory, its HMODULE is stored at this location.
			/// </summary>
			public UInt32 rvaHmod;

			/// <summary>
			/// An RVA to the Import Address Table for this DLL.
			/// This is the same format as a regular IAT.
			/// </summary>
			public UInt32 rvaIAT;

			/// <summary>
			/// An RVA to the Import Name Table for this DLL.
			/// This is the same format as a regular INT.
			/// </summary>
			public UInt32 rvaINT;

			/// <summary>
			/// An RVA of the optional bound IAT.
			/// An RVA to a bound copy of an Import Address Table for this DLL.
			/// This is the same format as a regular IAT.
			/// Currently, this copy of the IAT is not actually bound, but this feature may be added in future versions of the BIND program.
			/// </summary>
			public UInt32 rvaBoundIAT;

			/// <summary>
			/// An RVA of the optional copy of the original IAT.
			/// An RVA to an unbound copy of an Import Address Table for this DLL.
			/// This is the same format as a regular IAT. Currently always set to 0.
			/// </summary>
			public UInt32 rvaUnloadIAT;

			/// <summary>0 if not bound, O.W. date/time stamp of DLL bound to (Old BIND)</summary>
			public UInt32 dwTimeStamp;

			/// <summary>0 if not bound, O.W. date/time stamp of DLL bound to (Old BIND)</summary>
			public DateTime? TimeStamp { get { return NativeMethods.ConvertTimeDateStamp(this.dwTimeStamp); } }

			/// <summary>Empty structure</summary>
			public Boolean IsEmpty { get { return this.rvaDLLName == 0 && this.rvaINT == 0; } }
		}
		/// <summary>Based relocation format</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGE_BASE_RELOCATION
		{
			/// <summary>
			/// This field contains the starting RVA for this chunk of relocations.
			/// The offset of each relocation that follows is added to this value to form the actual RVA where the relocation needs to be applied.
			/// </summary>
			public UInt32 VirtualAddress;

			/// <summary>
			/// The size of this structure plus all the WORD relocations that follow.
			/// To determine the number of relocations in this block, subtract the size of an IMAGE_BASE_RELOCATION (8 bytes) from the value of this field, and then divide by 2 (the size of a WORD).
			/// </summary>
			public UInt32 SizeOfBlock;

			/// <summary>Relocation type offset</summary>
			public UInt16 TypeOffest
			{
				get { return checked((UInt16)(((UInt16)this.SizeOfBlock - (UInt16)Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION))) / (UInt16)sizeof(UInt16))); }
			}
		}
		/// <summary>Load config headers</summary>
		public struct LoadConfig
		{
			/// <summary>Load Configuration Directory Entry</summary>
			/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms680328%28v=vs.85%29.aspx</remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_LOAD_CONFIG_DIRECTORY32
			{
				/// <summary>The size of the structure. For Windows XP, the size must be specified as 64 for x86 images.</summary>
				public UInt32 Size;
				/// <summary>
				/// The date and time stamp value.
				/// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
				/// The time stamp can be printed using the C run-time (CRT) function ctime.
				/// </summary>
				public UInt32 TimeDateStamp;
				/// <summary>The major version number.</summary>
				public UInt16 MajorVersion;
				/// <summary>The minor version number.</summary>
				public UInt16 MinorVersion;
				/// <summary>The global flags that control system behavior. For more information, see Gflags.exe.</summary>
				public UInt32 GlobalFlagsClear;
				/// <summary>The global flags that control system behavior. For more information, see Gflags.exe.</summary>
				public UInt32 GlobalFlagsSet;
				/// <summary>The critical section default time-out value.</summary>
				public UInt32 CriticalSectionDefaultTimeout;
				/// <summary>The size of the minimum block that must be freed before it is freed (de-committed), in bytes. This value is advisory.</summary>
				public UInt64 DeCommitFreeBlockThreshold;
				/// <summary>
				/// The size of the minimum total memory that must be freed in the process heap before it is freed (de-committed), in bytes.
				/// This value is advisory.
				/// </summary>
				public UInt64 DeCommitTotalFreeThreshold;
				/// <summary>
				/// The VA of a list of addresses where the LOCK prefix is used.
				/// These will be replaced by NOP on single-processor systems.
				/// This member is available only for x86.
				/// </summary>
				public UInt64 LockPrefixTable;
				/// <summary>The maximum allocation size, in bytes. This member is obsolete and is used only for debugging purposes.</summary>
				public UInt64 MaximumAllocationSize;
				/// <summary>The maximum block size that can be allocated from heap segments, in bytes.</summary>
				public UInt64 VirtualMemoryThreshold;
				/// <summary>The process affinity mask. For more information, see GetProcessAffinityMask. This member is available only for .exe files.</summary>
				public UInt64 ProcessAffinityMask;
				/// <summary>The process heap flags. For more information, see HeapCreate.</summary>
				public UInt32 ProcessHeapFlags;
				/// <summary>The service pack version.</summary>
				public UInt16 CSDVersion;
				/// <summary>Reserved for use by the operating system.</summary>
				public UInt16 Reserved1;
				/// <summary>Reserved for use by the system.</summary>
				public UInt64 EditList;
				/// <summary>A pointer to a cookie that is used by Visual C++ or GS implementation.</summary>
				public UInt32 SecurityCookie;
				/// <summary>The VA of the sorted table of RVAs of each valid, unique handler in the image. This member is available only for x86.</summary>
				public UInt32 SEHandlerTable;
				/// <summary>The count of unique handlers in the table. This member is available only for x86.</summary>
				public UInt32 SEHandlerCount;
				/// <summary>The date and time stamp value.</summary>
				public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }
				/// <summary>Version number</summary>
				public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }
			}
			/// <summary>Load Configuration Directory Entry</summary>
			/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms680328%28v=vs.85%29.aspx</remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_LOAD_CONFIG_DIRECTORY64
			{
				/// <summary>The size of the structure. For Windows XP, the size must be specified as 64 for x86 images.</summary>
				public UInt32 Size;
				/// <summary>
				/// The date and time stamp value.
				/// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
				/// The time stamp can be printed using the C run-time (CRT) function ctime.
				/// </summary>
				public UInt32 TimeDateStamp;
				/// <summary>The major version number.</summary>
				public UInt16 MajorVersion;
				/// <summary>The minor version number.</summary>
				public UInt16 MinorVersion;
				/// <summary>The global flags that control system behavior. For more information, see Gflags.exe.</summary>
				public UInt32 GlobalFlagsClear;
				/// <summary>The global flags that control system behavior. For more information, see Gflags.exe.</summary>
				public UInt32 GlobalFlagsSet;
				/// <summary>The critical section default time-out value.</summary>
				public UInt32 CriticalSectionDefaultTimeout;
				/// <summary>The size of the minimum block that must be freed before it is freed (de-committed), in bytes. This value is advisory.</summary>
				public UInt64 DeCommitFreeBlockThreshold;
				/// <summary>
				/// The size of the minimum total memory that must be freed in the process heap before it is freed (de-committed), in bytes.
				/// This value is advisory.
				/// </summary>
				public UInt64 DeCommitTotalFreeThreshold;
				/// <summary>
				/// The VA of a list of addresses where the LOCK prefix is used.
				/// These will be replaced by NOP on single-processor systems.
				/// This member is available only for x86.
				/// </summary>
				public UInt64 LockPrefixTable;
				/// <summary>The maximum allocation size, in bytes. This member is obsolete and is used only for debugging purposes.</summary>
				public UInt64 MaximumAllocationSize;
				/// <summary>The maximum block size that can be allocated from heap segments, in bytes.</summary>
				public UInt64 VirtualMemoryThreshold;
				/// <summary>The process affinity mask. For more information, see GetProcessAffinityMask. This member is available only for .exe files.</summary>
				public UInt64 ProcessAffinityMask;
				/// <summary>The process heap flags. For more information, see HeapCreate.</summary>
				public UInt32 ProcessHeapFlags;
				/// <summary>The service pack version.</summary>
				public UInt16 CSDVersion;
				/// <summary>Reserved for use by the operating system.</summary>
				public UInt16 Reserved1;
				/// <summary>Reserved for use by the system.</summary>
				public UInt64 EditList;
				/// <summary>A pointer to a cookie that is used by Visual C++ or GS implementation.</summary>
				public UInt64 SecurityCookie;
				/// <summary>The VA of the sorted table of RVAs of each valid, unique handler in the image. This member is available only for x86.</summary>
				public UInt64 SEHandlerTable;
				/// <summary>The count of unique handlers in the table. This member is available only for x86.</summary>
				public UInt64 SEHandlerCount;
				/// <summary>The date and time stamp value.</summary>
				public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.TimeDateStamp); } }
				/// <summary>Version number</summary>
				public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion); } }
			}
		}
		/// <summary>Tls headers</summary>
		public struct Tls
		{
			/// <remarks>
			/// It's important to note that the addresses in the IMAGE_TLS_DIRECTORY structure are virtual addresses, not RVA's.
			/// Thus, they will get modified by base relocations if the executable doesn't load at its preferred load address.
			/// Also, the IMAGE_TLS_DIRECTORY itself is not in the .tls section; it resides in the .rdata section.
			/// </remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_TLS_DIRECTORY32
			{
				/// <summary>The beginning address of a range of memory used to initialize a new thread's TLS data in memory.</summary>
				public UInt32 StartAddressOfRawDataVA;
				/// <summary>The ending address of the range of memory used to initialize a new thread's TLS data in memory.</summary>
				public UInt32 EndAddressOfRawDataVA;
				/// <summary>
				/// When the executable is brought into memory and a .tls section is present, the loader allocates a TLS handle via TlsAlloc.
				/// It stores the handle at the address given by this field. The runtime library uses this index to locate the thread local data.
				/// </summary>
				public UInt32 AddressOfIndex;
				/// <summary>
				/// Address of an array of PIMAGE_TLS_CALLBACK function pointers.
				/// When a thread is created or destroyed, each function in the list is called.
				/// The end of the list is indicated by a pointer-sized variable set to 0.
				/// In normal Visual C++ executables, this list is empty.
				/// </summary>
				public UInt32 AddressOfCallBacks;
				/// <summary>
				/// The size in bytes of the initialization data, beyond the initialized data delimited by the StartAddressOfRawData and EndAddressOfRawData fields.
				/// All per-thread data after this range is initialized to 0.
				/// </summary>
				public UInt32 SizeOfZeroFill;
				/// <summary>Reserved for possible future use by TLS flags.</summary>
				public UInt32 Characteristics;
			}
			/// <remarks>
			/// It's important to note that the addresses in the IMAGE_TLS_DIRECTORY structure are virtual addresses, not RVA's.
			/// Thus, they will get modified by base relocations if the executable doesn't load at its preferred load address.
			/// Also, the IMAGE_TLS_DIRECTORY itself is not in the .tls section; it resides in the .rdata section.
			/// </remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_TLS_DIRECTORY64
			{
				/// <summary>The beginning address of a range of memory used to initialize a new thread's TLS data in memory.</summary>
				public UInt64 StartAddressOfRawDataVA;
				/// <summary>The ending address of the range of memory used to initialize a new thread's TLS data in memory.</summary>
				public UInt64 EndAddressOfRawDataVA;
				/// <summary>
				/// When the executable is brought into memory and a .tls section is present, the loader allocates a TLS handle via TlsAlloc.
				/// It stores the handle at the address given by this field. The runtime library uses this index to locate the thread local data.
				/// </summary>
				public UInt64 AddressOfIndex;
				/// <summary>
				/// Address of an array of PIMAGE_TLS_CALLBACK function pointers.
				/// When a thread is created or destroyed, each function in the list is called.
				/// The end of the list is indicated by a pointer-sized variable set to 0.
				/// In normal Visual C++ executables, this list is empty.
				/// </summary>
				public UInt64 AddressOfCallBacks;
				/// <summary>
				/// The size in bytes of the initialization data, beyond the initialized data delimited by the StartAddressOfRawData and EndAddressOfRawData fields.
				/// All per-thread data after this range is initialized to 0.
				/// </summary>
				public UInt32 SizeOfZeroFill;
				/// <summary>Reserved for possible future use by TLS flags.</summary>
				public UInt32 Characteristics;
			}
		}
		/// <summary>Debug information headers</summary>
		public struct Debug
		{
			//struct CV_INFO_PDB70
			//{
			//	DWORD	CvSignature;	// CodeView signature, equal to "RSDS" 
			//	GUID	Signature;		// A unique identifier, which changes with every rebuild of the executable and PDB file.
			//	DWORD	Age;			// Ever-incrementing value, which is initially set to 1 and incremented every time when a part of the PDB file is updated without rewriting the whole file.
			//	BYTE	PdbFileName[];	// Null-terminated name of the PDB file. It can also contain full or partial path to the file.
			//};
			/// <summary>PDB v7 Info</summary>
			/// <remarks>Note that the structure does not include Offset field (and thus does not start with CV_HEADER structure), while CodeView signature is still present. The absence of Offset field makes this structure an unusual member of CodeView family.</remarks>
			[StructLayout(LayoutKind.Sequential)]
			public struct CV_INFO_PDB70
			{
				/// <summary>CodeView signature, equal to "RSDS" </summary>
				public const UInt32 PDB70Signature = 0x53445352;
				/// <summary>CodeView signature, equal to "RSDS" </summary>
				public UInt32 CvSignature;
				/// <summary>Signature first part</summary>
				public UInt32 firstPart;
				/// <summary>Signature second part</summary>
				public UInt32 secondPart;
				/// <summary>Signature third part</summary>
				public UInt32 thirdPart;
				/// <summary>Signature fourth part</summary>
				public UInt32 fourthPart;
				/// <summary>Ever-incrementing value, which is initially set to 1 and incremented every time when a part of the PDB file is updated without rewriting the whole file.</summary>
				public UInt32 Age;
				/// <summary>PDV v7 signature is valid</summary>
				public Boolean IsValid { get { return this.CvSignature == CV_INFO_PDB70.PDB70Signature; } }
				/// <summary>CodeView signature, equal to "RSDS"</summary>
				public String CvSigString { get { return Encoding.ASCII.GetString(BitConverter.GetBytes(this.CvSignature)); } }
				/// <summary>A unique identifier, which changes with every rebuild of the executable and PDB file.</summary>
				public Guid Signature
				{
					get
					{
						Byte[] firstHeader = BitConverter.GetBytes(this.firstPart);
						Byte[] secondHeader = BitConverter.GetBytes(this.secondPart);
						Byte[] thirdHeader = BitConverter.GetBytes(this.thirdPart);
						Byte[] fourthHeader = BitConverter.GetBytes(this.fourthPart);


						Byte[] finalGuid = new Byte[16];

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

						for(Int32 xx = 8;xx < 12;xx++)
							finalGuid[xx] = thirdHeader[xx - 8];

						for(Int32 x = 12;x < 16;x++)
							finalGuid[x] = fourthHeader[x - 12];

						return new Guid(finalGuid);
					}
				}
			}
			/// <summary>CodeView PDB v2 header</summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct CV_HEADER
			{
				/// <summary>CodeView sugnature, equal to "NB10"</summary>
				public const UInt32 PDB20Signature = 0x3031424e;
				/// <summary>CodeView signature, equals to "NB10"</summary>
				public UInt32 Signature;
				/// <summary>CodeView offset. Set to 0, because debug information is stored in a separate file. </summary>
				public UInt32 Offset;
				/// <summary>PDV v2 signature is valid</summary>
				public Boolean IsValid { get { return this.Signature == CV_HEADER.PDB20Signature; } }
				/// <summary>CodeView signature, equals to "NB10"</summary>
				public String CvSigString { get { return Encoding.ASCII.GetString(BitConverter.GetBytes(this.Signature)); } }
			}
			/// <summary>PDB v2 Info</summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct CV_INFO_PDB20
			{
				/// <summary>PDB v2 Header</summary>
				public CV_HEADER CvHeader;
				/// <summary>The time when debug information was created (in seconds since 01.01.1970)</summary>
				public UInt32 Signature;
				/// <summary>Ever-incrementing value, which is initially set to 1 and incremented every time when a part of the PDB file is updated without rewriting the whole file.</summary>
				public UInt32 Age;
				/// <summary>The time when debug information was created</summary>
				public DateTime? TimeDate { get { return NativeMethods.ConvertTimeDateStamp(this.Signature); } }
			}
			/*struct IMAGE_DEBUG_MISC
		{
			DWORD		DataType;	// type of misc data, see defines
			DWORD		Length;		// total length of record, rounded to four byte multiple.
			BOOLEAN		Unicode;	// TRUE if data is unicode string
			BYTE		Reserved[3];
			BYTE		Data[1];	// Actual data
		};*/
			/// <summary>Type of misc data</summary>
			public enum IMAGE_DEBUG_MISC_TYPE : uint
			{
				/// <summary></summary>
				EXENAME = 1,
			}
			/// <summary>Misc debug Info</summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGE_DEBUG_MISC
			{//TODO: Необходимо прочитать блок Data.
				/// <summary>Type of misc data, see defines.</summary>
				public IMAGE_DEBUG_MISC_TYPE DataType;
				/// <summary>Total length of record, rounded to four byte multiple.</summary>
				public UInt32 Length;
				/// <summary>TRUE if data is unicode string.</summary>
				public Boolean Unicode;
				/// <summary>Reserved</summary>
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
				public Byte[] Reserved;
				// <summary>Actual data.</summary>
				//public Byte[] Data;
			}
		}
	}
}