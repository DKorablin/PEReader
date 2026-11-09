using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Exception table class</summary>
	[DefaultProperty(nameof(FirstEntry))]
	public class ExceptionTable : PEDirectoryBase, IEnumerable<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY>
	{
		private static readonly UInt32 SizeOfEntry = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY));

		/// <summary>First entry in exception table directory</summary>
		public WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY? FirstEntry
			=> base.IsEmpty
					? (WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY?)null
					: base.Parent.Header.PtrToStructure<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY>(base.Directory.VirtualAddress);

		/// <summary>The exception table of this image is an array of <see cref="WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY"/> structures</summary>
		/// <remarks>Other architectures (ARM, MIPS, etc.) use different entry layouts in the exception directory</remarks>
		private Boolean IsRuntimeFunctionTable
		{
			get
			{
				PEHeader header = base.Parent.Header;
				WinNT.IMAGE_FILE_HEADER fileHeader = header.Is64Bit
					? header.HeaderNT64.FileHeader
					: header.HeaderNT32.FileHeader;

				switch(fileHeader.Machine)
				{
				case WinNT.IMAGE_FILE_MACHINE.AMD64:
				case WinNT.IMAGE_FILE_MACHINE.IA64:
					return true;
				default:
					return false;
				}
			}
		}

		/// <summary>Create instance of exception table class</summary>
		/// <param name="parent">Data directory</param>
		public ExceptionTable(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.EXCEPTION) { }

		/// <summary>Get all exception procedure pointers</summary>
		/// <remarks>For architectures other than AMD64 and IA64 only the first entry is returned</remarks>
		/// <returns>Entries</returns>
		public IEnumerator<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY> GetEnumerator()
		{
			if(base.IsEmpty)
				yield break;

			if(!this.IsRuntimeFunctionTable)
			{
				yield return this.FirstEntry.Value;
				yield break;
			}

			//The exception directory is a contiguous array of entries (UnwindInfoAddress points to UNWIND_INFO, not to the next entry)
			WinNT.IMAGE_DATA_DIRECTORY directory = base.Directory;
			UInt32 count = directory.Size / ExceptionTable.SizeOfEntry;
			Byte[] data = base.Parent.Header.ReadBytes(directory.VirtualAddress, count * ExceptionTable.SizeOfEntry);

			for(Int32 offset = 0; offset < data.Length; offset += (Int32)ExceptionTable.SizeOfEntry)
				yield return new WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY()
				{
					BeginAddress = BitConverter.ToUInt32(data, offset),
					EndAddress = BitConverter.ToUInt32(data, offset + sizeof(UInt32)),
					UnwindInfoAddress = BitConverter.ToUInt32(data, offset + (sizeof(UInt32) * 2)),
				};
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}