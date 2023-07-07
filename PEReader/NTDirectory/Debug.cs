using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Debug directory class</summary>
	[DefaultProperty("Count")]
	public class Debug : PEDirectoryBase, IEnumerable<WinNT.IMAGE_DEBUG_DIRECTORY>
	{//TODO: Необходимо прочитать тип FPO (Если найду такой файл). Спецификация по FPO в 5.1.2. Debug Type (pecoff_v8.docx)
		private static UInt32 SizeOfDebugDirectory = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_DEBUG_DIRECTORY));

		/// <summary>CodeView PDB v2 Header</summary>
		public DebugPdb2? Pdb2CodeView
		{
			get
			{
				WinNT.IMAGE_DEBUG_DIRECTORY? directory = this.FindDebugType(WinNT.IMAGE_DEBUG_TYPE.CODEVIEW);
				if(directory.HasValue && directory.Value.PointerToRawData > 0)
				{
					UInt32 cvSignature = this.Parent.Header.PtrToStructure<UInt32>(directory.Value.PointerToRawData);
					if(cvSignature == WinNT.Debug.CV_HEADER.PDB20Signature)
						return new DebugPdb2(
							base.Parent.Header.PtrToStructure<WinNT.Debug.CV_INFO_PDB20>(directory.Value.PointerToRawData),
							base.Parent.Header.PtrToStringAnsi(directory.Value.PointerToRawData + (UInt32)Marshal.SizeOf(typeof(WinNT.Debug.CV_INFO_PDB20))));
				}
				return null;
			}
		}
		/// <summary>CodeView PDB v7 Header</summary>
		public DebugPdb7? Pdb7CodeView
		{
			get
			{
				WinNT.IMAGE_DEBUG_DIRECTORY? directory = this.FindDebugType(WinNT.IMAGE_DEBUG_TYPE.CODEVIEW);
				if(directory.HasValue && directory.Value.AddressOfRawData > 0)
				{
					UInt32 cvSignature = this.Parent.Header.PtrToStructure<UInt32>(directory.Value.AddressOfRawData);
					if(cvSignature == WinNT.Debug.CV_INFO_PDB70.PDB70Signature)
						return new DebugPdb7(
							base.Parent.Header.PtrToStructure<WinNT.Debug.CV_INFO_PDB70>(directory.Value.AddressOfRawData),
							base.Parent.Header.PtrToStringAnsi(directory.Value.AddressOfRawData + (UInt32)Marshal.SizeOf(typeof(WinNT.Debug.CV_INFO_PDB70))));
				}
				return null;
			}
		}
		/// <summary>Debug misc header</summary>
		public WinNT.Debug.IMAGE_DEBUG_MISC? Misc
		{
			get
			{
				var directory = this.FindDebugType(WinNT.IMAGE_DEBUG_TYPE.MISC);
				return directory.HasValue && directory.Value.AddressOfRawData > 0
					? this.Parent.Header.PtrToStructure<WinNT.Debug.IMAGE_DEBUG_MISC>(directory.Value.AddressOfRawData)
					: (WinNT.Debug.IMAGE_DEBUG_MISC?)null;
			}
		}

		/// <summary>Number of arrays in directory</summary>
		public UInt32 Count { get { return base.IsEmpty ? 0 : base.Directory.Size / Debug.SizeOfDebugDirectory; } }

		/// <summary>Create instance of debug directory</summary>
		/// <param name="root">Data directory</param>
		public Debug(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.DEBUG)
		{
		}

		/// <summary>Find debug header by type</summary>
		/// <param name="type">Type of debug header</param>
		/// <returns>Found header or null</returns>
		public WinNT.IMAGE_DEBUG_DIRECTORY? FindDebugType(WinNT.IMAGE_DEBUG_TYPE type)
		{
			foreach(var debug in this)
				if(debug.Type == type)
					return debug;
			return null;
		}

		/// <summary>Get all headers in directory</summary>
		/// <returns>Debug header</returns>
		public IEnumerator<WinNT.IMAGE_DEBUG_DIRECTORY> GetEnumerator()
		{
			UInt32 count = this.Count;
			if(count > 0)
			{
				UInt32 padding = base.Directory.VirtualAddress;
				for(UInt32 loop = 0;loop < count;loop++)
				{
					yield return this.Parent.Header.PtrToStructure<WinNT.IMAGE_DEBUG_DIRECTORY>(padding);
					padding += Debug.SizeOfDebugDirectory;
					//(UInt32)(directory.VirtualAddress + (structSize * loop)));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}