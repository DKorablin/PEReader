using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Impot directory</summary>
	[DefaultProperty("Header")]
	[DebuggerDisplay("Header={Header}")]
	public class Import : PEDirectoryBase, IEnumerable<ImportModule>
	{
		private static readonly UInt32 SizeOfHeader = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_IMPORT_DESCRIPTOR));

		/// <summary>Первый дескриптор импортируемых функций</summary>
		public WinNT.IMAGE_IMPORT_DESCRIPTOR? Header
		{
			get
			{
				return base.IsEmpty
					? (WinNT.IMAGE_IMPORT_DESCRIPTOR?)null
					: this.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_DESCRIPTOR>(this.Directory.VirtualAddress);
			}
		}

		/// <summary>Create instance of Import directory class</summary>
		/// <param name="parent">Data directory</param>
		public Import(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.IMPORT)
		{
		}

		/// <summary>Get all import modules from image</summary>
		/// <returns>Import modules</returns>
		public IEnumerator<ImportModule> GetEnumerator()
		{
			if(!IsEmpty)
			{
				UInt32 count = 0;
				WinNT.IMAGE_IMPORT_DESCRIPTOR descriptor = this.Header.Value;
				while(!descriptor.IsEmpty)
				{
					yield return new ImportModule(this, descriptor);
					count++;
					descriptor = this.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_DESCRIPTOR>(base.Directory.VirtualAddress + (Import.SizeOfHeader * count));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}