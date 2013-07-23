using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>String resource class</summary>
	public class ResourceString : ResourceBase, IEnumerable<String>
	{
		/// <summary>Create instance of string resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceString(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING)
		{

		}
		/// <summary>Get all strings from resource directory</summary>
		/// <returns>Strings from resource</returns>
		public IEnumerator<String> GetEnumerator()
		{
			UInt32 padding = 0;
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					UInt16 length = reader.BytesToStructure<UInt16>(ref padding);
					length *= 2;
					yield return System.Text.Encoding.Unicode.GetString(reader.GetBytes(padding, length));
					padding += length;
				}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}