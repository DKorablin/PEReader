using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>String heap class</summary>
	public class StringHeap : StreamHeaderTyped<String>
	{
		/// <summary>Create instance of String heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		/// <exception cref="T:InvalidOperationException">StringHeap class can only read String heap</exception>
		public StringHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(base.Header.Type != Cor.StreamHeaderType.String)
				throw new InvalidOperationException();
		}

		/// <summary>
		/// The .NET specification allows a string reference to point anywhere in the string heap, not just to thestart of a string.
		/// Therefore, it is possible (although probably not very useful) to create an assembly in which some strings overlap with each other.
		/// </summary>
		/// <param name="pointer">Pointer in the heap</param>
		/// <returns>Data by pointer</returns>
		protected override String GetDataByPointer(Int32 pointer)
		{
			SortedList<Int32, String> data = base.GetData();
			Int32 key = pointer;
			String nearestString = null;

			if(pointer > base.Header.Size)
				throw new InvalidOperationException($"Pointer: {pointer:X} overflowed header. Size: {base.Header.Size:n0}");

			while(key >= 0 && !data.TryGetValue(key, out nearestString))
				key--;

			Int32 diff = pointer - key;

			//TODO: Here i can add found string to the SortedList<,>
			String result = nearestString.Substring(diff, nearestString.Length - diff);
			return result;
		}

		/// <summary>Binds the data form stream to string array</summary>
		protected override SortedList<Int32,String> DataBind()
		{
			SortedList<Int32, String> result = new SortedList<Int32, String>();

			Byte[] bytes = base.Bytes;
			Int32 ptr = 0;

			for(Int32 loop = 0; loop < bytes.Length; loop++)
			{
				if(bytes[loop] == 0)
				{
					String str = Encoding.ASCII.GetString(bytes, ptr, loop - ptr);

					result.Add(ptr, str);
					ptr = loop + 1;
				}
			}
			return result;
		}
	}
}