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
			=> throw new NotImplementedException();

		/// <summary>Binds the data form stream to byte array</summary>
		protected override SortedList<Int32,Byte[]> DataBind()
		{
			SortedList<Int32, Byte[]> result = new SortedList<Int32, Byte[]>();

			Byte[] bytes = base.Bytes;
			for(UInt32 loop = 0;loop < bytes.Length;)
			{
				UInt32 length = NativeMethods.GetPackedValue(bytes, loop, out Int32 padding);

				Byte[] b;
				if(length == 0)
					b = new Byte[] { };
				else
				{
					b = new Byte[length];
					Array.Copy(bytes, loop + padding, b, 0, length);
				}
				Int32 ptr = (Int32)loop;
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
					bytes = Array.Empty<Byte>();
				result.Add(bytes);

				position += (UInt32)length;
				start += length + padding;
			}*/
			return result;
		}
	}
}