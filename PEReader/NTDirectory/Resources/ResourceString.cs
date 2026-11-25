using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary><see cref="WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING"/> resource reader class</summary>
	public class ResourceString : ResourceBase, IEnumerable<ResourceString.StringItem>
	{
		/// <summary>Each RT_STRING block always contains exactly 16 length-prefixed strings</summary>
		private const Int32 StringsPerBlock = 16;

		/// <summary>Starting directory string ID</summary>
		/// <exception cref="InvalidOperationException">Block directory is named or has zero ID</exception>
		public UInt32 Id
		{
			get
			{
				WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY entry = base.Directory.Parent.DirectoryEntry;
				if(entry.IsNameString || entry.NameAddress == 0)
					throw new InvalidOperationException("RT_STRING block must have a non-zero integer ID");

				return (entry.NameAddress - 1) * StringsPerBlock;//Starting directory ID
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
		/// <remarks>Returns all 16 slots of the block. Undefined string IDs are returned with an empty <see cref="StringItem.Value"/>. Trailing alignment padding after the 16th slot is ignored.</remarks>
		/// <returns>Strings from resource</returns>
		public IEnumerator<ResourceString.StringItem> GetEnumerator()
		{
			UInt32 id = this.Id;
			UInt32 padding = 0;
			using(PinnedBufferReader reader = base.CreateDataReader())
				for(Int32 i = 0; i < StringsPerBlock && padding + sizeof(UInt16) <= reader.Length; i++, id++)
				{
					UInt32 length = reader.BytesToStructure<UInt16>(ref padding) * 2u;//Length in WCHARs → bytes

					String value = length == 0
						? String.Empty
						: Encoding.Unicode.GetString(reader.GetBytes(padding, length));
					padding += length;

					yield return new StringItem() { ID = id, Value = value, };
				}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}