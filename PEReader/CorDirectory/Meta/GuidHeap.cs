using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Guid heap class</summary>
	public class GuidHeap : StreamHeaderTyped<Guid>
	{
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
		public override Guid this[Int32 index]
		{
			get
			{
				if(index == 0)
					return Guid.Empty;
				else
					return base[index - 1];
			}
		}
		/// <summary>Binds the data form stream to guid array</summary>
		protected override SortedList<Int32, Guid> DataBind()
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