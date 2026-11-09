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
				return USHeap.DecodeUserString(payload);
			}
		}

		/// <summary>Binds the data form stream to string array</summary>
		public IEnumerable<KeyValuePair<Int32, String>> GetDataString()
		{
			SortedList<Int32, Byte[]> source = base.GetData();
			foreach(KeyValuePair<Int32, Byte[]> item in source)
				yield return new KeyValuePair<Int32, String>(item.Key, USHeap.DecodeUserString(item.Value));
		}

		/// <summary>Decode user string blob to string</summary>
		/// <remarks>
		/// Each #US blob contains UTF-16 characters followed by one terminal byte (ECMA-335 II.24.2.4).
		/// The terminal byte is a flag, not a part of the string, so only an even number of bytes is decoded.
		/// String literals may contain unpaired surrogates (e.g. "\uD800"), so the code units are copied as is
		/// instead of using <see cref="System.Text.Encoding.Unicode"/>, which would replace them with U+FFFD.
		/// </remarks>
		/// <param name="payload">User string blob</param>
		/// <returns>Decoded string</returns>
		private static String DecodeUserString(Byte[] payload)
		{
			Char[] result = new Char[payload.Length / 2];
			for(Int32 loop = 0; loop < result.Length; loop++)
				result[loop] = (Char)(payload[loop * 2] | (payload[(loop * 2) + 1] << 8));//UTF-16LE code unit
			return new String(result);
		}
	}
}