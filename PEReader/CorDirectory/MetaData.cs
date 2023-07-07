using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>.NET MetaData directory class</summary>
	[DefaultProperty("Header")]
	public class MetaData : CorDirectoryBase, IEnumerable<StreamHeader>
	{
		private static UInt32 SizeOfCor20Metadata1 = (UInt32)Marshal.SizeOf(typeof(Cor.IMAGE_COR20_METADATA1));
		private static UInt32 SizeOfCor20MetaData2 = (UInt32)Marshal.SizeOf(typeof(Cor.IMAGE_COR20_METADATA2));

		private Cor.IMAGE_COR20_METADATA? _metaData;
		private Dictionary<Cor.StreamHeaderType, StreamHeader> _streams;

		/// <summary>Create instance of MetaData directory class</summary>
		/// <param name="parent">.NET directory</param>
		public MetaData(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.MetaData)
		{
		}

		/// <summary>MetaData header</summary>
		public Cor.IMAGE_COR20_METADATA? Header
		{
			get
			{
				if(this._metaData != null)
					return this._metaData.Value;
				if(base.IsEmpty)
					return null;

				Cor.IMAGE_COR20_METADATA1 meta1;
				Cor.IMAGE_COR20_METADATA2 meta2;
				String version;
				//Первый блок структуры описания метаданных
				meta1 = base.Parent.Parent.Header.PtrToStructure<Cor.IMAGE_COR20_METADATA1>(base.Directory.VirtualAddress);

				//Версия блока структуры описания метаданных
				UInt32 position = base.Directory.VirtualAddress + MetaData.SizeOfCor20Metadata1;
				version = this.Parent.Parent.Header.PtrToStringAnsi(position);

				//Второй блок структуры описания метаданных
				position = base.Directory.VirtualAddress + MetaData.SizeOfCor20Metadata1 + meta1.Length;
				meta2 = this.Parent.Parent.Header.PtrToStructure<Cor.IMAGE_COR20_METADATA2>(position);

				this._metaData = new Cor.IMAGE_COR20_METADATA
				{
					Signature = meta1.Signature,
					MajorVersion = meta1.MajorVersion,
					MinorVersion = meta1.MinorVersion,
					Reserved = meta1.Reserved,
					Length = meta1.Length,
					Version = version,
					Flags = meta2.Flags,
					Streams = meta2.Streams,
				};
				return this._metaData.Value;
			}
		}

		private Dictionary<Cor.StreamHeaderType, StreamHeader> Streams
		{
			get
			{
				if(this._streams == null)
				{
					this._streams = new Dictionary<Cor.StreamHeaderType, StreamHeader>();
					foreach(var stream in this.GetStreams())
						this._streams.Add(stream.Header.Type, stream);
				}
				return this._streams;
			}
		}

		/// <summary>MetaData tables</summary>
		public StreamTables StreamTables
		{
			get
			{

				StreamHeader result;
				if(this.Streams.TryGetValue(Cor.StreamHeaderType.StreamTable, out result))
					return (StreamTables)result;
				else if(this.Streams.TryGetValue(Cor.StreamHeaderType.StreamTableUnoptimized, out result))
					return (StreamTables)result;
				else
					return null;
			}
		}
		/// <summary>Guid heap</summary>
		public GuidHeap GuidHeap
		{
			get
			{
				StreamHeader result;
				return this.Streams.TryGetValue(Cor.StreamHeaderType.Guid, out result)
					? (GuidHeap)result
					: null;
			}
		}

		/// <summary>Bloab heap</summary>
		public BlobHeap BlobHeap
		{
			get
			{
				StreamHeader result;
				return this.Streams.TryGetValue(Cor.StreamHeaderType.Blob, out result)
					? (BlobHeap)result
					: null;
			}
		}

		/// <summary>String heap</summary>
		public StringHeap StringHeap
		{
			get
			{
				StreamHeader result;
				return this.Streams.TryGetValue(Cor.StreamHeaderType.String, out result)
					? (StringHeap)result
					: null;
			}
		}

		/// <summary>User String heap</summary>
		public USHeap USHeap
		{
			get
			{
				StreamHeader result;
				return this.Streams.TryGetValue(Cor.StreamHeaderType.UnicodeSting, out result)
					? (USHeap)result
					: null;
			}
		}

		/// <summary>Get array of all heaps in MedaData directory</summary>
		/// <returns>Heaps in MetaData directory</returns>
		public IEnumerator<StreamHeader> GetEnumerator()
		{
			foreach(var stream in this.Streams)
				yield return stream.Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private IEnumerable<StreamHeader> GetStreams()
		{
			Cor.IMAGE_COR20_METADATA? meta = this.Header;
			if(meta == null)
				yield break;

			UInt32 position = base.Directory.VirtualAddress +
				MetaData.SizeOfCor20Metadata1 +
				meta.Value.Length +
				MetaData.SizeOfCor20MetaData2;

			for(Int32 loop = 0; loop < meta.Value.Streams; loop++)
			{
				Cor.STREAM_HEADER header = base.Parent.Parent.Header.PtrToStructure<Cor.STREAM_HEADER>(position);
				switch(header.Type)
				{
				case Cor.StreamHeaderType.StreamTable:
				case Cor.StreamHeaderType.StreamTableUnoptimized://TODO: Не проверено. Пока ещё не нашёл ни одного файла с таким заголовком
					yield return new StreamTables(this, header);
					break;
				case Cor.StreamHeaderType.Guid:
					yield return new GuidHeap(this, header);
					break;
				case Cor.StreamHeaderType.Blob:
					yield return new BlobHeap(this, header);
					break;
				case Cor.StreamHeaderType.String:
					yield return new StringHeap(this, header);
					break;
				case Cor.StreamHeaderType.UnicodeSting:
					yield return new USHeap(this, header);
					break;
				default:
					yield return new StreamHeader(this, header);
					break;
				}

				Int32 length = header.Name.Length + 1;
				Int32 padding = ((length % 4) != 0) ? (4 - (length % 4)) : 0;
				position += sizeof(UInt32) * 2 + (UInt32)length + (UInt32)padding;
			}
		}
	}
}