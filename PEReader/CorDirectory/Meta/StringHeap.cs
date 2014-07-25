using System;
using System.Collections.Generic;

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
		/*/// <summary>Get string from string heap</summary>
		/// <param name="index">String index in the heap</param>
		/// <returns>String from heap</returns>
		public String this[Int32 index]
		{
			get
			{
				return this.GetData()[index];

				//
				// The .NET specification allows a string reference to point anywhere in the string heap, not just to thestart of a string. Therefore, it is possible (although probably not very useful) to create an assembly in which some strings overlap with each other. Such an assembly can be read by Asmex if the GetByOffset method of MDHeap is overridded in the MDStringHeap class thus:
				//
				//Int32 originalKey = key;
				//String result = this.Data[key];

				//while(result == null && key >= 0)
				//	result = this.Data[--key];// Locate the previous key (there's probably a more efficient way of doing this).

				//if(originalKey != key)
				//{// re-index into the string
				//	Int32 diff = originalKey - key;
				//	result = result.Substring(diff, result.Length - diff);
				//}
				//return result;
			}
		}*/
		/// <summary>Binds the data form stream to string array</summary>
		protected override SortedList<Int32,String> DataBind()
		{
			SortedList<Int32, String> result = new SortedList<Int32, String>();

			Int32 ptr = 0;
			Byte[] bytes = base.Bytes;
			String str = String.Empty;

			for(Int32 loop = 0; loop < bytes.Length; loop++)
			{
				if(bytes[loop] == 0)
				{
					result.Add(ptr, str);
					ptr = loop + 1;
					str = String.Empty;
				} else
				{
					Char ch = (Char)bytes[loop];
					str += ch;
				}
			}
			return result;
		}
	}
}