using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Accelerators class</summary>
	public class ResourceAccelerator : ResourceBase, IEnumerable<WinNT.Resource.ACCELTABLEENTRY>
	{
		/// <summary>Create instance of accelerators resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceAccelerator(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR)
		{

		}
		/// <summary>Получить список используемых акселераторов</summary>
		/// <returns>Массив используемых акселераторов</returns>
		public IEnumerator<WinNT.Resource.ACCELTABLEENTRY> GetEnumerator()
		{
			UInt32 position = 0;
			using(BytesReader reader = base.CreateDataReader())
				while(position < reader.Length)
					yield return reader.BytesToStructure<WinNT.Resource.ACCELTABLEENTRY>(ref position);
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}