using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Delay import class</summary>
	public class DelayImport : PEDirectoryBase, IEnumerable<DelayImportModule>
	{
		/// <summary>Create instance of delay import class</summary>
		/// <param name="parent">Data directory</param>
		public DelayImport(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.DELAY_IMPORT)
		{
		}
		/// <summary>Get array of delay import modules and procedures</summary>
		/// <returns>Delay import modules</returns>
		public IEnumerator<DelayImportModule> GetEnumerator()
		{
			if(!IsEmpty)
			{
				UInt32 sizeOfDescr = (UInt32)Marshal.SizeOf(typeof(WinNT.ImgDelayDescr));
				UInt32 count = (base.Directory.Size / sizeOfDescr) - 1;//Значение чисто абстрактное, т.к. Delphi пишет в последнюю структуру одни нули. А в count указывает левое значение (только Delphi).

				UInt32 position = base.Directory.VirtualAddress;
				for(UInt32 loop = 0;loop < count;loop++)
				{
					//UInt32 position = base.Directory.VirtualAddress + sizeOfDescr * loop;
					WinNT.ImgDelayDescr descriptor = base.Parent.Header.PtrToStructure<WinNT.ImgDelayDescr>(position);
					position += sizeOfDescr;

					if(descriptor.IsEmpty)
						yield break;//Delphi заканчивает блок не кол-вом элементов, а структурой с нулевыми данными
					yield return new DelayImportModule(this, descriptor);
				}
			}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}