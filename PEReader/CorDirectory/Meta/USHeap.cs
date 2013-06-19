using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>User string heap class</summary>
	public class USHeap : BlobHeap
	{
		/// <summary>Create instance of user strings heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		public USHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
		}
		/// <summary>Get string from string heap</summary>
		/// <param name="index">String index in the heap</param>
		/// <returns>String from heap</returns>
		public new String this[Int32 index] { get { return this.DataString[index]; } }

		/// <summary>All strings from heap</summary>
		public String[] DataString
		{
			get
			{
				SortedList<Int32,Byte[]> data = base.Data;
				String[] result = new String[data.Count];
				for(Int32 loop = 0; loop < result.Length; loop++)
					result[loop] = System.Text.Encoding.Unicode.GetString(data.Values[loop]);
				return result;
			}
		}
	}
}