using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Accelerators class</summary>
	public class ResourceAccelerator : ResourceBase, IEnumerable<WinUser.ACCELTABLEENTRY>
	{
		/// <summary>Create instance of accelerators resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceAccelerator(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR) { }

		/// <summary>Get a list of used accelerators</summary>
		/// <returns>Array of used accelerators</returns>
		public IEnumerator<WinUser.ACCELTABLEENTRY> GetEnumerator()
		{
			UInt32 position = 0;
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(position < reader.Length)
					yield return reader.BytesToStructure<WinUser.ACCELTABLEENTRY>(ref position);
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}