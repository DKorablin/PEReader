using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Guid heap class</summary>
	public class GuidHeap : StreamHeader
	{
		private SortedList<Int32,Guid> _data;

		/// <summary>Create instance of Guid heap class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">.NET stream header</param>
		/// <exception cref="T:InvalidOperationException">GuidHeap can only read Guid heaps</exception>
		public GuidHeap(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(base.Header.Type != Cor.StreamHeaderType.Guid)
				throw new InvalidOperationException();
		}
		/// <summary>Получить Guid по сдвигу.</summary>
		/// <param name="index">Сдвиг с начала кучи.</param>
		/// <returns>Guid по сдвигу.</returns>
		public Guid this[Int32 index]
		{
			get
			{
				if(index == 0)
					return Guid.Empty;
				else
					return this.Data[index - 1];
			}
		}
		/// <summary>heap data</summary>
		public SortedList<Int32,Guid> Data
		{
			get
			{
				if(this._data == null)
					this._data = this.GetDataI();
				return this._data;
			}
		}
		private SortedList<Int32, Guid> GetDataI()
		{
			Int32 ptr = 0;
			Byte[] bytes = base.Bytes;

			UInt32 sizeOfGuid = (UInt32)Marshal.SizeOf(typeof(Guid));
			UInt32 length = base.Header.Size / sizeOfGuid;
			SortedList<Int32, Guid> result = new SortedList<Int32, Guid>((Int32)length);

			for(Int32 loop = 0;loop < length;loop++)
			{
				Byte[] bTemp = new Byte[sizeOfGuid];
				Array.Copy(bytes, loop * sizeOfGuid, bTemp, 0, sizeOfGuid);

				result.Add(ptr, new Guid(bTemp));
				ptr = loop;
			}

			return result;
		}
	}
}