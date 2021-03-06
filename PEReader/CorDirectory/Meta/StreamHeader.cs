﻿using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Stream header class</summary>
	public class StreamHeader
	{
		private readonly MetaData _meta;
		private readonly Cor.STREAM_HEADER _header;
		/// <summary>Parent MetaData directory</summary>
		protected internal MetaData Parent { get { return this._meta; } }
		/// <summary>Stream header</summary>
		public Cor.STREAM_HEADER Header { get { return this._header; } }

		/// <summary>Стартовая позиция с которой начинается куча</summary>
		public virtual UInt32 Position
		{
			get
			{
				return this.Parent.Directory.VirtualAddress + this.Header.Offset;
			}
		}
		/// <summary>Массив байт, в куче</summary>
		public Byte[] Bytes { get { return this.Parent.Parent.Parent.Header.ReadBytes(this.Position, this.Header.Size); } }
		
		/// <summary>Create instance of stream header class</summary>
		/// <param name="loader">MetaData</param>
		/// <param name="header">Stream header</param>
		public StreamHeader(MetaData loader, Cor.STREAM_HEADER header)
		{
			this._meta = loader;
			this._header = header;
		}
	}
}