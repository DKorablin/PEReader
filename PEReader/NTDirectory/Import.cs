using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Impot directory</summary>
	[DefaultProperty("ImportDescriptor")]
	public class Import : PEDirectoryBase, IEnumerable<ImportModule>
	{
		/// <summary>Первый дескриптор импортируемых функций</summary>
		public WinNT.IMAGE_IMPORT_DESCRIPTOR? ImportDescriptor
		{
			get
			{
				if(base.IsEmpty)
					return null;
				else
					return this.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_DESCRIPTOR>(this.Directory.VirtualAddress);
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
				Int32 count = 0;
				Int32 descriptorSize = Marshal.SizeOf(typeof(WinNT.IMAGE_IMPORT_DESCRIPTOR));
				WinNT.IMAGE_IMPORT_DESCRIPTOR descriptor = this.ImportDescriptor.Value;
				while(!descriptor.IsEmpty)
				{
					yield return new ImportModule(this, descriptor);
					count++;
					descriptor = this.Parent.Header.PtrToStructure<WinNT.IMAGE_IMPORT_DESCRIPTOR>(base.Directory.VirtualAddress + (UInt32)(descriptorSize * count));
				}
			}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}