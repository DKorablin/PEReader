using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Exception table class</summary>
	[DefaultProperty("FirstEntry")]
	public class ExceptionTable : PEDirectoryBase, IEnumerable<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY>
	{
		/// <summary>First entry in exception table directory</summary>
		public WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY? FirstEntry
		{
			get
			{
				if(base.IsEmpty)
					return null;
				else
					return base.Parent.Header.PtrToStructure<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>Create instance of exception table class</summary>
		/// <param name="parent">Data directory</param>
		public ExceptionTable(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.EXCEPTION)
		{
		}
		/// <summary>Get all exception procedure pointers</summary>
		/// <returns>Entries</returns>
		public IEnumerator<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY> GetEnumerator()
		{//TODO: Не правильно читаются структуры при закгрузке через StreamLoader?
			WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY? first = this.FirstEntry;
			if(first.HasValue)
			{
				WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY entry = first.Value;
				yield return entry;
				if(
					base.Parent.Header.Is64Bit && base.Parent.Header.HeaderNT64.FileHeader.Machine == WinNT.IMAGE_FILE_MACHINE.AMD64
					||
					!base.Parent.Header.Is64Bit && base.Parent.Header.HeaderNT32.FileHeader.Machine == WinNT.IMAGE_FILE_MACHINE.AMD64)
				{
					while(!entry.IsLatEntry)
					{
						entry = base.Parent.Header.PtrToStructure<WinNT.IMAGE_RUNTIME_FUNCTION_ENTRY>(entry.UnwindInfoAddress);
						yield return entry;
					}
				}
			}
			yield break;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}