using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>Win32 methods and helpers</summary>
	internal static class NativeMethods
	{
		/// <summary>The max number of bits in short </summary>
		private const Int32 BIT_SIZE_SHORT = 16;
		/// <summary>The max number of bits in int</summary>
		private const Int32 BIT_SIZE_INT = 32;
		/// <summary>The max number of bits in long</summary>
		private const Int32 BIT_SIZE_LONG = 64;

		private static DateTime StartDate = new DateTime(1970, 1, 1, 0, 0, 0);

		/// <summary>Convert an unsigned int to a date by adding seconds to Jan 1, 1970 0:0:0</summary>
		/// <remarks>http://jasonhaley.com/blog/post/2006/01/07/Get-a-DateTime-from-a-Coff-Headers-TimeDateStamp.aspx</remarks>
		/// <param name="timeDateStamp">Number of seconds to add to start date</param>
		/// <returns>DateTime timeDateStamp seconds after the start date</returns>
		public static DateTime? ConvertTimeDateStamp(UInt32 timeDateStamp)
		{
			if(timeDateStamp == 0)
				return null;

			TimeZone timeZone = TimeZone.CurrentTimeZone;
			return timeZone.ToLocalTime(NativeMethods.StartDate.AddSeconds((Double)timeDateStamp));
		}

		/// <summary>Retrieves the high-order word from the specified 32-bit value</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The return value is the high-order word of the specified value</returns>
		public static UInt16 HiWord(UInt32 value)
		{
			return (UInt16)((value & 0xFFFF0000) >> 16);
		}

		/// <summary>Retrieves the low-order word from the specified value</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The return value is the low-order word of the specified value</returns>
		public static UInt16 LoWord(UInt32 value)
		{
			return (UInt16)(value & 0x0000FFFF);
		}

		/// <summary>The return value is the high-order byte of the specified value</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The return value is the high-order byte of the specified value</returns>
		public static Byte HiByte(Int16 value)
		{
			return ((Byte) (((Int16) (value) >> 8) & 0xFF));
		}

		/// <summary>The return value is the low-order byte of the specified value</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The return value is the low-order byte of the specified value</returns>
		public static Byte LoByte(Int16 value)
		{
			return ((Byte)value);
		}

		/// <summary>Makes a 64 bit long from two 32 bit integers</summary>
		/// <param name="low">The low order value</param>
		/// <param name="high">The high order value</param>
		/// <returns></returns>
		public static Int64 MakeLong(Int32 low, Int32 high)
		{
			if(high == 0)
				return (Int64)low;
			Int64 temp = SizeOf(high);
			temp = (high << ((NativeMethods.BIT_SIZE_LONG) - ((Int32)temp + 1)));
			return (Int64)(low | (Int32)temp);
		}

		/// <summary>Makes a 32 bit integer from two 16 bit shorts</summary>
		/// <param name="low">The low order value</param>
		/// <param name="high">The high order value</param>
		/// <returns></returns>
		public static Int32 MakeDword(Int16 low, Int16 high)
		{
			return (low + (high << 16));
			/*if(high == 0)
				return (Int32)low;
			Int32 temp = SizeOf(high);
			temp = high << ((BIT_SIZE_INT) - (temp + 1));
			return (Int32)(temp | low);*/
		}

		/// <summary>Makes a 16 bit short from two bytes</summary>
		/// <param name="low">The low order value</param>
		/// <param name="high">The high order value</param>
		/// <returns></returns>
		public static Int16 MakeWord(Byte low, Byte high)
		{
			if(high == 0)
				return (Int16)low;
			Int32 temp = NativeMethods.SizeOf(high);
			temp = high << ((NativeMethods.BIT_SIZE_SHORT) - (temp + 1));
			return (Int16)(low | temp);
		}

		/// <summary>Gets the size of the input value in bits</summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		private static Int32 SizeOf(Int16 pInput)
		{
			Int32 iRetval = 0;
			if(pInput == 0)
				iRetval = 0;
			else if(pInput == 1)
				iRetval = 1;
			else if(pInput < 0)
				iRetval = NativeMethods.BIT_SIZE_SHORT;
			else
			{
				Int32 lTemp = 0;
				for(Int32 i = NativeMethods.BIT_SIZE_SHORT - 1; i > 1; i--)
				{
					lTemp = 1 << i - 1;
					if((pInput & lTemp) == lTemp)
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}

		/// <summary>Gets the size of the input value in bits</summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		private static Int32 SizeOf(Int32 pInput)
		{
			Int32 iRetval = 0;
			if(pInput == 0)
				iRetval = 0;
			else if( pInput == 1)
				iRetval = 1;
			else if(pInput < 0)
				iRetval = NativeMethods.BIT_SIZE_INT;
			else
			{
				Int32 lTemp = 0;
				for(Int32 i = BIT_SIZE_INT -1; i > 1; i-- )
				{
					lTemp = 1 << i-1;
					if((pInput & lTemp) == lTemp)
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}

		/// <summary>Gets the size of the input value in bits</summary>
		/// <param name="pInput">The input value</param>
		/// <returns></returns>
		private static Int32 SizeOf(Int64 pInput)
		{
			Int32 iRetval = 0;
			if(pInput == 0)
				iRetval = 0;
			else if(pInput == 1)
				iRetval = 1;
			else if(pInput < 0)
				iRetval = NativeMethods.BIT_SIZE_LONG;
			else
			{
				Int64 lTemp = 0;
				for(Int32 i = NativeMethods.BIT_SIZE_LONG - 1; i > 1; i--)
				{
					lTemp = 1 << i - 1;
					if((pInput & lTemp) == lTemp)
					{
						iRetval = i;
						break;
					}
				}
			}
			return iRetval;
		}

		/// <summary>Align padding to DWORD</summary>
		/// <param name="padding">Original padding</param>
		/// <returns>Aligned padding</returns>
		public static UInt32 AlignToInt(UInt32 padding)
		{
			/*UInt32 bytesToRead = padding % 4;
			if(bytesToRead > 0)
				padding += 4 - bytesToRead;
			return padding;*/
			return (UInt32)((padding + 3) & ~3);
		}

		/// <summary>Align padding to WORD</summary>
		/// <param name="padding">Original padding</param>
		/// <returns>Aligned padding</returns>
		public static UInt32 AlignToShort(UInt32 padding)
		{
			return (UInt32)((padding + 1) & ~1);
		}

		/// <summary>Retrieves a string that represents the name of a key</summary>
		/// <param name="lParam">The second parameter of the keyboard message (such as WM_KEYDOWN) to be processed</param>
		/// <param name="lpString">The buffer that will receive the key name</param>
		/// <param name="nSize">
		/// The maximum length, in characters, of the key name, including the terminating null character.
		/// (This parameter should be equal to the size of the buffer pointed to by the lpString parameter.)
		/// </param>
		/// <remarks>
		/// The format of the key-name string depends on the current keyboard layout.
		/// The keyboard driver maintains a list of names in the form of character strings for keys with names longer than a single character.
		/// The key name is translated according to the layout of the currently installed keyboard, thus the function may give different results for different input locales.
		/// The name of a character key is the character itself.
		/// The names of dead keys are spelled out in full
		/// </remarks>
		/// <returns>If the function succeeds, a null-terminated string is copied into the specified buffer, and the return value is the length of the string, in characters, not counting the terminating null character</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("user32.dll", EntryPoint = "GetKeyNameTextW", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern Int32 GetKeyNameText(UInt32 lParam, [Out] StringBuilder lpString, Int32 nSize);

		/// <summary>The translation to be performed</summary>
		public enum MAPVK : uint
		{
			/// <summary>
			/// uCode is a virtual-key code and is translated into an unshifted character value in the low-order word of the return value.
			/// Dead keys (diacritics) are indicated by setting the top bit of the return value.
			/// If there is no translation, the function returns 0
			/// </summary>
			VK_TO_CHAR = 2,
			/// <summary>
			/// uCode is a virtual-key code and is translated into a scan code.
			/// If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned.
			/// If there is no translation, the function returns 0
			/// </summary>
			VK_TO_VSC = 0,
			/// <summary>
			/// uCode is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys.
			/// If there is no translation, the function returns 0
			/// </summary>
			VSC_TO_VK = 1,
			/// <summary>
			/// uCode is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys.
			/// If there is no translation, the function returns 0
			/// </summary>
			VSC_TO_VK_EX = 3,
		}

		/// <summary>Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into a virtual-key code</summary>
		/// <param name="uCode">The virtual key code or scan code for a key. How this value is interpreted depends on the value of the uMapType parameter</param>
		/// <param name="uMapType">The translation to be performed</param>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms646306%28v=vs.85%29.aspx</remarks>
		/// <returns>The return value is either a scan code, a virtual-key code, or a character value, depending on the value of uCode and uMapType.
		/// If there is no translation, the return value is zero
		/// </returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("user32.dll", EntryPoint = "MapVirtualKeyW", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern UInt32 MapVirtualKey(UInt32 uCode, MAPVK uMapType);

		/// <summary>Retrieves a module handle for the specified module. The module must have been loaded by the calling process</summary>
		/// <param name="lpModuleName">
		/// The name of the loaded module (either a .dll or .exe file). If the file name extension is omitted, the default library extension .dll is appended.
		/// The file name string can include a trailing point character (.) to indicate that the module name has no extension.
		/// The string does not have to specify a path. When specifying a path, be sure to use backslashes (\), not forward slashes (/).
		/// The name is compared (case independently) to the names of modules currently mapped into the address space of the calling process.
		/// If this parameter is NULL, GetModuleHandle returns a handle to the file used to create the calling process (.exe file)
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is a handle to the specified module.
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError
		/// </returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(String lpModuleName);

		/// <summary>
		/// The action to be taken when loading the module.
		/// If no flags are specified, the behavior of this function is identical to that of the LoadLibrary function.
		/// This parameter can be one of the following values
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms684179%28v=vs.85%29.aspx</remarks>
		[Flags]
		public enum LoadLibraryFlags : uint
		{
			/// <summary>
			/// If this value is used, and the executable module is a DLL, the system does not call DllMain for process and thread initialization and termination.
			/// Also, the system does not load additional executable modules that are referenced by the specified module
			/// </summary>
			DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
			/// <summary>
			/// If this value is used, the system does not check AppLocker rules or apply Software Restriction Policies for the DLL.
			/// This action applies only to the DLL being loaded and not to its dependencies.
			/// This value is recommended for use in setup programs that must run extracted DLLs during installation
			/// </summary>
			LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
			/// <summary>
			/// If this value is used, the system maps the file into the calling process's virtual address space as if it were a data file.
			/// Nothing is done to execute or prepare to execute the mapped file.
			/// Therefore, you cannot call functions like GetModuleFileName, GetModuleHandle or GetProcAddress with this DLL.
			/// Using this value causes writes to read-only memory to raise an access violation. Use this flag when you want to load a DLL only to extract messages or resources from it.
			/// This value can be used with <see cref="T:LOAD_LIBRARY_AS_IMAGE_RESOURCE"/>.
			/// </summary>
			LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
			/// <summary>
			/// Similar to <see cref="T:LOAD_LIBRARY_AS_DATAFILE"/>, except that the DLL file is opened with exclusive write access for the calling process.
			/// Other processes cannot open the DLL file for write access while it is in use.
			/// However, the DLL can still be opened by other processes.
			/// This value can be used with <see cref="T:LOAD_LIBRARY_AS_IMAGE_RESOURCE"/>.
			/// </summary>
			LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
			/// <summary>
			/// If this value is used, the system maps the file into the process's virtual address space as an image file.
			/// However, the loader does not load the static imports or perform the other usual initialization steps.
			/// Use this flag when you want to load a DLL only to extract messages or resources from it.
			/// If forced integrity checking is desired for the loaded file then <see cref="T:LOAD_LIBRARY_AS_IMAGE"/> is recommended instead.
			/// Unless the application depends on the image layout, this value should be used with either <see cref="T:LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE"/> or <see cref="T:LOAD_LIBRARY_AS_DATAFILE"/>.
			/// </summary>
			LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
			/// <summary>
			/// If this value is used, the application's installation directory is searched for the DLL and its dependencies.
			/// Directories in the standard search path are not searched.
			/// This value cannot be combined with <see cref="T:LOAD_WITH_ALTERED_SEARCH_PATH"/>.
			/// </summary>
			LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
			/// <summary>
			/// This value is a combination of <see cref="T:LOAD_LIBRARY_SEARCH_APPLICATION_DIR"/>, <see cref="T:LOAD_LIBRARY_SEARCH_SYSTEM32"/>, and <see cref="T:LOAD_LIBRARY_SEARCH_USER_DIRS"/>.
			/// Directories in the standard search path are not searched. This value cannot be combined with <see cref="T:LOAD_WITH_ALTERED_SEARCH_PATH"/>.
			/// This value represents the recommended maximum number of directories an application should include in its DLL search path
			/// </summary>
			LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
			/// <summary>
			/// If this value is used, the directory that contains the DLL is temporarily added to the beginning of the list of directories that are searched for the DLL's dependencies.
			/// Directories in the standard search path are not searched.
			/// The lpFileName parameter must specify a fully qualified path.
			/// This value cannot be combined with <see cref="T:LOAD_WITH_ALTERED_SEARCH_PATH"/>.
			/// For example, if Lib2.dll is a dependency of C:\Dir1\Lib1.dll, loading Lib1.dll with this value causes the system to search for Lib2.dll only in C:\Dir1.
			/// To search for Lib2.dll in C:\Dir1 and all of the directories in the DLL search path, combine this value with <see cref="T:LOAD_LIBRARY_DEFAULT_DIRS"/>
			/// </summary>
			LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
			/// <summary>
			/// If this value is used, %windows%\system32 is searched for the DLL and its dependencies.
			/// Directories in the standard search path are not searched.
			/// This value cannot be combined with <see cref="T:LOAD_WITH_ALTERED_SEARCH_PATH"/>
			/// </summary>
			LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
			/// <summary>
			/// If this value is used, directories added using the AddDllDirectory or the SetDllDirectory function are searched for the DLL and its dependencies.
			/// If more than one directory has been added, the order in which the directories are searched is unspecified.
			/// Directories in the standard search path are not searched.
			/// This value cannot be combined with <see cref="T:LOAD_WITH_ALTERED_SEARCH_PATH"/>
			/// </summary>
			LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
			/// <summary>
			/// If this value is used and lpFileName specifies an absolute path, the system uses the alternate file search strategy discussed in the Remarks section to find associated executable modules that the specified module causes to be loaded.
			/// If this value is used and lpFileName specifies a relative path, the behavior is undefined.
			/// If this value is not used, or if lpFileName does not specify a path, the system uses the standard search strategy discussed in the Remarks section to find associated executable modules that the specified module causes to be loaded.
			/// This value cannot be combined with any <see cref="T:LOAD_LIBRARY_SEARCH"/> flag
			/// </summary>
			LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,
		}

		/// <summary>Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms684179%28v=vs.85%29.aspx</remarks>
		/// <param name="lpFileName">
		/// A string that specifies the file name of the module to load. This name is not related to the name stored in a library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.
		/// The module can be a library module (a .dll file) or an executable module (an .exe file). If the specified module is an executable module, static imports are not loaded; instead, the module is loaded as if DONT_RESOLVE_DLL_REFERENCES was specified. See the dwFlags parameter for more information.
		/// If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string.
		/// If the string specifies a fully qualified path, the function searches only that path for the module. When specifying a path, be sure to use backslashes (\), not forward slashes (/). For more information about paths, see Naming Files, Paths, and Namespaces.
		/// If the string specifies a module name without a path and more than one loaded module has the same base name and extension, the function returns a handle to the module that was loaded first.
		/// If the string specifies a module name without a path and a module of the same name is not already loaded, or if the string specifies a module name with a relative path, the function searches for the specified module. The function also searches for modules if loading the specified module causes the system to load other associated modules (that is, if the module has dependencies). The directories that are searched and the order in which they are searched depend on the specified path and the dwFlags parameter. For more information, see Remarks.
		/// If the function cannot find the module or one of its dependencies, the function fails.
		/// </param>
		/// <param name="hReservedNull">This parameter is reserved for future use. It must be NULL</param>
		/// <param name="dwFlags">
		/// The action to be taken when loading the module.
		/// If no flags are specified, the behavior of this function is identical to that of the LoadLibrary function
		/// </param>
		/// <returns>If the function succeeds, the return value is a handle to the loaded module</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibraryEx(String lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

		/// <summary>Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms684175%28v=vs.85%29.aspx</remarks>
		/// <param name="lpFileName">
		/// The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.
		/// If the string specifies a full path, the function searches only that path for the module.
		/// If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module; for more information, see the Remarks.
		/// If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\), not forward slashes (/). For more information about paths, see Naming a File or Directory.
		/// If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string
		/// </param>
		/// <returns>If the function succeeds, the return value is a handle to the module</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibrary(String lpFileName);

		/// <summary>
		/// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
		/// When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid
		/// </summary>
		/// <param name="hModule">
		/// A handle to the loaded library module.
		/// The LoadLibrary, LoadLibraryEx, GetModuleHandle, or GetModuleHandleEx function returns this handle
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero.
		/// To get extended error information, call the GetLastError function
		/// </returns>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms683152%28v=vs.85%29.aspx</remarks>
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern Boolean FreeLibrary(IntPtr hModule);

		/// <summary>Locates a relative virtual address (RVA) within the image header of a file that is mapped as a file and returns the virtual address of the corresponding byte in the file</summary>
		/// <param name="NtHeaders">A pointer to an IMAGE_NT_HEADERS structure. This structure can be obtained by calling the ImageNtHeader function</param>
		/// <param name="Base">The base address of an image that is mapped into memory through a call to the MapViewOfFile function</param>
		/// <param name="Rva">The relative virtual address to be located</param>
		/// <param name="LastRvaSection">
		/// A pointer to an IMAGE_SECTION_HEADER structure that specifies the last RVA section. This is an optional parameter.
		/// When specified, it points to a variable that contains the last section value used for the specified image to translate an RVA to a VA
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is the virtual address in the mapped file.
		/// If the function fails, the return value is NULL. To retrieve extended error information, call GetLastError
		/// </returns>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms680218%28v=vs.85%29.aspx</remarks>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("DbgHelp.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr ImageRvaToVa(IntPtr NtHeaders, IntPtr Base, UInt32 Rva, IntPtr LastRvaSection);

		/// <summary>Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL)</summary>
		/// <param name="hModule">
		/// A handle to the DLL module that contains the function or variable. The LoadLibrary, LoadLibraryEx, LoadPackagedLibrary, or GetModuleHandle function returns this handle.
		/// The GetProcAddress function does not retrieve addresses from modules that were loaded using the LOAD_LIBRARY_AS_DATAFILE flag. For more information, see LoadLibraryEx
		/// </param>
		/// <param name="lpProcName">The function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero</param>
		/// <returns>
		/// If the function succeeds, the return value is the address of the exported function or variable.
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError
		/// </returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr lpProcName);

		/// <summary>Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL)</summary>
		/// <param name="hModule">
		/// A handle to the DLL module that contains the function or variable. The LoadLibrary, LoadLibraryEx, LoadPackagedLibrary, or GetModuleHandle function returns this handle.
		/// The GetProcAddress function does not retrieve addresses from modules that were loaded using the LOAD_LIBRARY_AS_DATAFILE flag. For more information, see LoadLibraryEx.
		/// </param>
		/// <param name="lpProcName">
		/// The function or variable name, or the function's ordinal value.
		/// If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is the address of the exported function or variable.
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError
		/// </returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, String lpProcName);

		/// <summary>
		/// Reserves or commits a region of pages in the virtual address space of the calling process.
		/// Memory allocated by this function is automatically initialized to zero, unless MEM_RESET is specified
		/// </summary>
		/// <param name="lpStartAddr">
		/// The starting address of the region to allocate.
		/// If the memory is being reserved, the specified address is rounded down to the nearest multiple of the allocation granularity.
		/// If the memory is already reserved and is being committed, the address is rounded down to the next page boundary.
		/// To determine the size of a page and the allocation granularity on the host computer, use the GetSystemInfo function.
		/// If this parameter is NULL, the system determines where to allocate the region
		/// </param>
		/// <param name="size">
		/// The size of the region, in bytes.
		/// If the lpAddress parameter is NULL, this value is rounded up to the next page boundary.
		/// Otherwise, the allocated pages include all pages containing one or more bytes in the range from lpAddress to lpAddress+dwSize.
		/// This means that a 2-byte range straddling a page boundary causes both pages to be included in the allocated region
		/// </param>
		/// <param name="flAllocationType">The type of memory allocation</param>
		/// <param name="flProtect">
		/// The memory protection for the region of pages to be allocated.
		/// If the pages are being committed, you can specify any one of the memory protection constants
		/// </param>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/aa366887%28v=vs.85%29.aspx</remarks>
		/// <returns>
		/// If the function succeeds, the return value is the base address of the allocated region of pages.
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError
		/// </returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("kernel32.dll")]
		public static extern UInt32 VirtualAlloc(UInt32 lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);
	}
}