using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>String heap class</summary>
	/// <remarks>Strings in the #Strings heap are null-terminated UTF-8 (ECMA-335 II.24.2.3)</remarks>
	public class StringHeap : StreamHeaderTyped<String>
	{
		//Raw heap bytes, required to decode strings referenced from the middle of another string
		private Byte[] _bytes;

		/// <summary>Create instance of String heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		/// <exception cref="InvalidOperationException">StringHeap class can only read String heap</exception>
		public StringHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(base.Header.Type != Cor.StreamHeaderType.String)
				throw new InvalidOperationException();
		}

		/// <summary>
		/// The .NET specification allows a string reference to point anywhere in the string heap, not just to the start of a string.
		/// Therefore, it is possible (although probably not very useful) to create an assembly in which some strings overlap with each other.
		/// </summary>
		/// <param name="pointer">Pointer to heap</param>
		/// <returns>Data by pointer</returns>
		protected override String GetDataByPointer(Int32 pointer)
		{
			base.GetData();//Ensure that heap bytes are loaded
			Byte[] bytes = this._bytes;

			if(pointer < 0 || pointer >= bytes.Length)
				throw new InvalidOperationException($"Pointer: {pointer:X} overflowed header. Size: {base.Header.Size:n0}");

			//The pointer is a byte offset, so the string is decoded from the raw bytes (a char index differs from a byte offset for multi-byte UTF-8 characters)
			Int32 end = Array.IndexOf(bytes, (Byte)0, pointer);
			if(end < 0)
				end = bytes.Length;

			//TODO: Here I can add found string to the SortedList<,>
			return Encoding.UTF8.GetString(bytes, pointer, end - pointer);
		}

		/// <summary>Binds the data form stream to string array</summary>
		protected override SortedList<Int32,String> DataBind()
		{
			SortedList<Int32, String> result = new SortedList<Int32, String>();

			Byte[] bytes = base.Bytes;
			this._bytes = bytes;
			Int32 ptr = 0;

			for(Int32 loop = 0; loop < bytes.Length; loop++)
			{
				if(bytes[loop] == 0)
				{
					String str = Encoding.UTF8.GetString(bytes, ptr, loop - ptr);

					result.Add(ptr, str);
					ptr = loop + 1;
				}
			}
			return result;
		}
	}
}