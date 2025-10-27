using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Relocation table class</summary>
	public class Relocation : PEDirectoryBase, IEnumerable<RelocationBlock>
	{
		/// <summary>Create instance of Relocation table class</summary>
		/// <param name="parent">Data directory</param>
		public Relocation(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.BASERELOC) { }

		/// <summary>Get all relocations blocks from directory</summary>
		/// <returns>Relocation blocks</returns>
		public IEnumerator<RelocationBlock> GetEnumerator()
		{
			if(!this.IsEmpty)
			{
				UInt32 position = base.Directory.VirtualAddress;
				UInt32 end = position + base.Directory.Size;
				UInt32 sizeOfBaseRelocation = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_BASE_RELOCATION));
				while(position < end)
				{
					WinNT.IMAGE_BASE_RELOCATION block = base.Parent.Header.PtrToStructure<WinNT.IMAGE_BASE_RELOCATION>(position);

					RelocationSection[] sections = new RelocationSection[block.TypeOffest];
					for(UInt32 loop = 0;loop < block.TypeOffest;loop++)
					{
						UInt32 sectionPosition = position + sizeOfBaseRelocation + sizeof(UInt16) * loop;
						sections[loop] = new RelocationSection(base.Parent.Header.PtrToStructure<UInt16>(sectionPosition));
					}
					//UInt16 section = base.Info.Header.PtrToStructure<UInt16>(position);
					yield return new RelocationBlock(block, sections);
					position += block.SizeOfBlock;
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}