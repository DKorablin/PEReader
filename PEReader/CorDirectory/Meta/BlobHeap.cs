using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>BLOB heap class</summary>
	public class BlobHeap : StreamHeaderTyped<Byte[]>
	{
		/// <summary>Create instance of BLOB heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		/// <exception cref="InvalidOperationException">Blob heap can read only UnicodeString and Blob heaps</exception>
		public BlobHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(base.Header.Type != Cor.StreamHeaderType.UnicodeSting && base.Header.Type != Cor.StreamHeaderType.Blob)
				throw new InvalidOperationException();
		}

		/// <summary>This method is not implemented in BLOB heap</summary>
		/// <param name="pointer">Pointer to start of a heap</param>
		/// <exception cref="NotImplementedException">This method is not implemented</exception>
		/// <returns>Array of bytes from BLOB heap</returns>
		protected override Byte[] GetDataByPointer(Int32 pointer)
		{
			throw new NotImplementedException();
		}

		/// <summary>Binds the data form stream to byte array</summary>
		protected override SortedList<Int32,Byte[]> DataBind()
		{
			SortedList<Int32, Byte[]> result = new SortedList<Int32, Byte[]>();

			Int32 ptr = 0;
			Byte[] bytes = base.Bytes;
			for(UInt32 loop = 0;loop < bytes.Length;)
			{
				Int32 padding;
				Int32 length = BlobHeap.GetPackedLength(bytes, loop, out padding);

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
		internal static Int32 GetPackedLength(Byte[] bytes, ref Int32 offset)
		{
			Int32 result = GetPackedLength(bytes, (UInt32)offset, out Int32 padding);
			offset += padding;
			return result;

		}

		internal static Int32 GetPackedLength(Byte[] bytes, UInt32 position, out Int32 padding)
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
			} else if((result & 0xE0) == 0xC0)
			{
				padding = 4;
				return ((result & 0x1F) << 24)//0x3F
				| (bytes[position + 1] << 16)
				| (bytes[position + 2] << 8)
				| bytes[position + 3];
			} else throw new InvalidOperationException();
		}
	}
}