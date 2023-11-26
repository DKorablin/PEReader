using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary><see cref="WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING"/> resource reader class</summary>
	public class ResourceString : ResourceBase, IEnumerable<ResourceString.StringItem>
	{
		/// <summary>Starting directory string ID</summary>
		public UInt32 Id
		{
			get
			{
				UInt32 directoryName = base.Directory.Parent.DirectoryEntry.NameAddress;
				return (directoryName - 1) * 16;//Starting directory ID
			}
		}

		/// <summary>String item from resource directory</summary>
		public struct StringItem
		{
			/// <summary>ID of string resource</summary>
			public UInt32 ID;

			/// <summary>String value</summary>
			public String Value;
		}

		/// <summary>Create instance of string resource reader class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceString(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING) { }

		/// <summary>Get all strings from resource directory</summary>
		/// <returns>Strings from resource</returns>
		public IEnumerator<ResourceString.StringItem> GetEnumerator()
		{
			UInt32 name = base.Directory.Parent.DirectoryEntry.NameAddress;
			UInt32 id = this.Id;
			UInt32 padding = 0;
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					UInt16 length = reader.BytesToStructure<UInt16>(ref padding);
					length *= 2;

					String value = Encoding.Unicode.GetString(reader.GetBytes(padding, length));
					//if(value.Length > 0)
					yield return new StringItem() { ID = id, Value = value, };

					id++;
					padding += length;
				}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}