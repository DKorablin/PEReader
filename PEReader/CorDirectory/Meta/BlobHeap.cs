using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>BLOB heap class</summary>
	public class BlobHeap : StreamHeader
	{
		private SortedList<Int32,Byte[]> _data;

		/// <summary>Create instance of BLOB heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		/// <exception cref="T:InvalidOperationException">Blob heap can read only UnicodeString and Blob heaps</exception>
		public BlobHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(base.Header.Type != Cor.StreamHeaderType.UnicodeSting && base.Header.Type != Cor.StreamHeaderType.Blob)
				throw new InvalidOperationException();
		}
		/// <summary>Get byte array from BLOB heap</summary>
		/// <param name="index">BLOB index in the heap</param>
		/// <returns>byte array by index from heap</returns>
		public Byte[] this[Int32 index] { get { return this.Data[index]; } }

		/// <summary>All BLOB data from heap</summary>
		public SortedList<Int32,Byte[]> Data
		{
			get
			{
				if(this._data == null)
					this._data = this.GetDataI();
				return this._data;
			}
		}
		private SortedList<Int32,Byte[]> GetDataI()
		{
			SortedList<Int32, Byte[]> result = new SortedList<Int32, Byte[]>();

			Int32 ptr = 0;
			Byte[] bytes = base.Bytes;
			for(UInt32 loop = 0;loop < bytes.Length;)
			{
				Int32 padding;
				Int32 length = BlobHeap.GetStreamLength(bytes, loop, out padding);

				Byte[] b;
				if(length == 0)
					b = new Byte[] { };
				else
				{
					b = new Byte[length];
					Array.Copy(bytes, loop + padding, b, 0, length);
				}
				ptr = (Int32)loop;
				result.Add(ptr, b);
				loop += (UInt32)(length + padding);
				//ptr += length + 1;
			}

			/*UInt32 position = base.Loader.Directory.Cor20Header.Value.MetaData.VirtualAddress
				+ base.Header.Offset;
			Int32 start = 0;
			while(start < base.Header.Size)
			{
				Int32 padding;
				Int32 length = base.GetStreamLength(position, out padding);
				position += (UInt32)padding;
				Byte[] bytes;
				if(length > 0)
					bytes = base.Loader.Directory.Root.Header.ReadBytes(position, length);
				else
					bytes = new Byte[] { };
				result.Add(bytes);

				position += (UInt32)length;
				start += length + padding;
			}*/
			return result;
		}
		internal static Int32 GetStreamLength(Byte[] bytes, UInt32 position, out Int32 padding)
		{
			Int32 result = bytes[position];
			if((result & 0x80) == 0)
			{
				padding = 1;
				return result;
			} else if((result & 0xC0) == 0x80)
			{
				padding = 2;
				return ((result & 0x3F) << 8) | bytes[position + 1];
			} else
			{
				padding = 3;
				return ((result & 0x3F) << 24)
				| (bytes[position + 1] << 16)
				| (bytes[position + 2] << 8)
				| bytes[position + 3];
			}
		}
	}
}