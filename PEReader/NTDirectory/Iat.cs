using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Impot Address Table class</summary>
	[DefaultProperty(nameof(Count))]
	public class Iat : PEDirectoryBase, IEnumerable<UInt64>
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
		public UInt32 Count => base.Directory.IsEmpty ? 0 : base.Directory.Size / this.SizeOfStruct;

		/// <summary>Create instance of IAT class</summary>
		/// <param name="parent">Data directory</param>
		public Iat(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.IAT) { }

		/// <summary>Get pointers array from directory</summary>
		/// <remarks>Entries are 4 bytes wide in PE images and 8 bytes wide in PE+ images. 4 byte entries are zero-extended</remarks>
		/// <returns>Array of pointers</returns>
		public IEnumerator<UInt64> GetEnumerator()
		{
			UInt32 count = this.Count;
			if(count == 0)
				yield break;

			UInt32 sizeOfStruct = this.SizeOfStruct;
			Byte[] data = base.Parent.Header.ReadBytes(base.Directory.VirtualAddress, count * sizeOfStruct);
			for(Int32 offset = 0; offset < data.Length; offset += (Int32)sizeOfStruct)
				yield return sizeOfStruct == sizeof(UInt64)
					? BitConverter.ToUInt64(data, offset)
					: BitConverter.ToUInt32(data, offset);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}