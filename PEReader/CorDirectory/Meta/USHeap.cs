using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>User string heap class</summary>
	public class USHeap : BlobHeap
	{
		// TODO: Неизвестно как правильно обращаться к элементам массива. См. комментарий к методу DataString
		//private SortedList<Int32, String> _data;
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
		{//TODO: Индексы исчезли. Надо поизучать документацию. Может индексы и не нужны. (Если нужны, то использовать GetDataString)
			get
			{
				SortedList<Int32,Byte[]> data = base.GetData();
				String[] result = new String[data.Count];
				for(Int32 loop = 0; loop < result.Length; loop++)
					result[loop] = System.Text.Encoding.Unicode.GetString(data.Values[loop]);
				return result;
			}
		}
		/// <summary>Binds the data form stream to string array</summary>
		public SortedList<Int32, String> GetDataString()
		{
			SortedList<Int32, Byte[]> source = base.GetData();
			SortedList<Int32, String> result = new SortedList<Int32, String>(source.Count);
			foreach(KeyValuePair<Int32,Byte[]> item in source)
				result.Add(item.Key, System.Text.Encoding.Unicode.GetString(item.Value));
			return result;
		}
	}
}