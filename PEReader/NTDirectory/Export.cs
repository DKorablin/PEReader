using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Export directory class</summary>
	[DefaultProperty(nameof(Header))]
	[DebuggerDisplay("Header = {"+nameof(Header)+"}")]
	public class Export : PEDirectoryBase
	{
		/// <summary>Directory is empty</summary>
		public override Boolean IsEmpty
			=> base.IsEmpty || this.Header.Value.NumberOfFunctions == 0;

		/// <summary>Exported functions directory header information</summary>
		public WinNT.IMAGE_EXPORT_DIRECTORY? Header
			=> base.IsEmpty
					? (WinNT.IMAGE_EXPORT_DIRECTORY?)null
					: this.Parent.Header.PtrToStructure<WinNT.IMAGE_EXPORT_DIRECTORY>(base.Directory.VirtualAddress);

		/// <summary>Module name</summary>
		public String DllName
			=> base.Directory.IsEmpty
					? null
					: this.Parent.Header.PtrToStringAnsi(this.Header.Value.NameRva);

		/// <summary>Create instance of Export class</summary>
		/// <param name="root">Data directory</param>
		public Export(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.EXPORT) { }

		/// <summary>Get a list of exported functions</summary>
		/// <remarks>
		/// Functions are returned in ordinal order.
		/// Functions exported by ordinal only are returned with <see cref="ExportFunction.Name"/> set to null.
		/// A function exported under several names is returned once per name.
		/// </remarks>
		/// <returns>Array with addresses of exported functions</returns>
		public IEnumerable<ExportFunction> GetExportFunctions()
		{
			//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			//public delegate void SomeExportedFunctionInTheDll();
			//SomeExportedFunctionInTheDll invoke =
			//(SomeExportedFunctionInTheDll)Marshal.GetDelegateForFunctionPointer((IntPtr)address, typeof(SomeExportedFunctionInTheDll));
			//invoke();

			if(this.IsEmpty)
				yield break;

			PEHeader header = base.Parent.Header;
			WinNT.IMAGE_DATA_DIRECTORY exportDirectory = base.Directory;
			WinNT.IMAGE_EXPORT_DIRECTORY directory = this.Header.Value;
			UInt64 baseAddress = (UInt64)header.Loader.BaseAddress;

			//Export Address Table (EAT): one RVA per ordinal, indexed by (ordinal - OrdinalBase)
			Byte[] functions = header.ReadBytes(directory.AddressOfFunctions, checked(directory.NumberOfFunctions * sizeof(UInt32)));
			//Export Name Pointer Table and Export Ordinal Table are parallel arrays. The ordinal table holds an index into the EAT for each name
			Byte[] namePointers = header.ReadBytes(directory.AddressOfNames, checked(directory.NumberOfNames * sizeof(UInt32)));
			Byte[] nameOrdinals = header.ReadBytes(directory.AddressOfNameOrdinals, checked(directory.NumberOfNames * sizeof(UInt16)));

			Dictionary<UInt32, List<String>> names = new Dictionary<UInt32, List<String>>();
			for(UInt32 loop = 0; loop < directory.NumberOfNames; loop++)
			{
				UInt32 functionIndex = BitConverter.ToUInt16(nameOrdinals, (Int32)(loop * sizeof(UInt16)));
				if(functionIndex >= directory.NumberOfFunctions)
					continue;//Malformed image: name refers outside of the EAT

				UInt32 nameRva = BitConverter.ToUInt32(namePointers, (Int32)(loop * sizeof(UInt32)));
				if(!names.TryGetValue(functionIndex, out List<String> functionNames))
					names.Add(functionIndex, functionNames = new List<String>());
				functionNames.Add(header.PtrToStringAnsi(nameRva));
			}

			for(UInt32 functionIndex = 0; functionIndex < directory.NumberOfFunctions; functionIndex++)
			{
				UInt32 rva = BitConverter.ToUInt32(functions, (Int32)(functionIndex * sizeof(UInt32)));
				if(rva == 0)
					continue;//Unused ordinal slot

				//If the RVA points inside the export directory, it is a forwarder string ("DLL.Function" or "DLL.#Ordinal") instead of code or data
				String forwarderName = rva >= exportDirectory.VirtualAddress && rva < exportDirectory.VirtualAddress + exportDirectory.Size
					? header.PtrToStringAnsi(rva)
					: null;

				ExportFunction function = new ExportFunction()
				{
					Ordinal = unchecked((UInt16)(directory.OrdinalBase + functionIndex)),
					Rva = rva,
					Address = baseAddress + rva,
					ForwarderName = forwarderName,
				};

				if(names.TryGetValue(functionIndex, out List<String> functionNames))
					foreach(String name in functionNames)
					{
						function.Name = name;
						yield return function;
					}
				else
					yield return function;//Exported by ordinal only
			}
		}
	}
}