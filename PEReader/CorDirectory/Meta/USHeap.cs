using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>User string heap class</summary>
	public class USHeap : BlobHeap
	{
		/// <summary>Create instance of user strings heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		public USHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header) { }

		/// <summary>Get string from string heap</summary>
		/// <param name="index">String index in the heap</param>
		/// <returns>String from heap</returns>
		public new String this[Int32 index]
		{
			get
			{
				SortedList<Int32, Byte[]> source = base.GetData();
				Byte[] payload = source[index];
				return Encoding.Unicode.GetString(payload);
			}
		}

		/// <summary>Binds the data form stream to string array</summary>
		public IEnumerable<KeyValuePair<Int32, String>> GetDataString()
		{
			SortedList<Int32, Byte[]> source = base.GetData();
			foreach(KeyValuePair<Int32, Byte[]> item in source)
				yield return new KeyValuePair<Int32, String>(item.Key, Encoding.Unicode.GetString(item.Value));
		}
	}
}