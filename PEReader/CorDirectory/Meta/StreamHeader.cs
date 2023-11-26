using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Stream header class</summary>
	public class StreamHeader
	{
		/// <summary>Parent MetaData directory</summary>
		protected internal MetaData Parent { get; }

		/// <summary>Stream header</summary>
		public Cor.STREAM_HEADER Header { get; }

		/// <summary>Headp stream start position</summary>
		public virtual UInt32 Position
			=> this.Parent.Directory.VirtualAddress + this.Header.Offset;

		/// <summary>Byte array in Heap Stream</summary>
		public Byte[] Bytes => this.Parent.Parent.Parent.Header.ReadBytes(this.Position, this.Header.Size);
		
		/// <summary>Create instance of stream header class</summary>
		/// <param name="loader">MetaData</param>
		/// <param name="header">Stream header</param>
		public StreamHeader(MetaData loader, Cor.STREAM_HEADER header)
		{
			this.Parent = loader ?? throw new ArgumentNullException(nameof(loader));
			this.Header = header;
		}
	}
}