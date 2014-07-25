using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Impot Address Table class</summary>
	[DefaultProperty("Count")]
	public class Iat : NTDirectoryBase, IEnumerable<UInt64>
	{
		private UInt32 SizeOfStruct {

			get
			{
				if(base.Parent.Header.Is64Bit)
					return sizeof(UInt64);//Для PE+
				else
					return sizeof(UInt32);
			}
		}
		/// <summary>Pointer count in the directory</summary>
		public UInt32 Count { get { return base.Directory.IsEmpty ? 0 : base.Directory.Size / this.SizeOfStruct; } }

		/// <summary>Create instance of IAT class</summary>
		/// <param name="parent">Data directory</param>
		public Iat(PEDirectory parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.IAT)
		{
		}

		/// <summary>Get pointers array from directory</summary>
		/// <returns>Array of pointers</returns>
		public IEnumerator<UInt64> GetEnumerator()
		{
			UInt32 padding = base.Directory.VirtualAddress;
			UInt32 count = this.Count;
			for(UInt32 loop = 0;loop < count;loop++)
			{
				yield return base.Parent.Header.PtrToStructure<UInt64>(padding);
				padding += this.SizeOfStruct;
			}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}