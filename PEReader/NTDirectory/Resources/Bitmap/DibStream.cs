using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Device Independed Bitmap stream</summary>
	public class DibStream : Stream
	{
		/// <summary>Bitmap file header (excluded from DIB)</summary>
		[StructLayout(LayoutKind.Sequential, Size = 14, Pack = 1)]
		public struct BitmapHeader
		{
			/// <summary>File header validataion</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public Byte[] magic;
			/// <summary>Total file size</summary>
			public Int32 fileSize;
			/// <summary>banana banana banana</summary>
			public Int32 Unk1;
			/// <summary>header size</summary>
			public Int32 headerSize;
		}

		private Int64 _position = 0;
		private Stream _rtBitmap = null;
		private Byte[] Header { get; }

		/// <summary>Can read from stream</summary>
		public override Boolean CanRead => true;

		/// <summary>Can seek in stream</summary>
		public override Boolean CanSeek => false;

		/// <summary>Can write to stream</summary>
		public override Boolean CanWrite => false;

		/// <summary>Stream length with bitmap header</summary>
		public override Int64 Length => 14 + this._rtBitmap.Length;

		/// <summary>Current position</summary>
		public override Int64 Position
		{
			get => this._position;
			set
			{
				this._position = value;
				if(this._position > 14)
					this._rtBitmap.Position = this._position - 14;
			}
		}

		/// <summary>Create instance of DIB stream</summary>
		/// <param name="buffer"></param>
		public DibStream(Byte[] buffer)
		{
			_ = buffer ?? throw new ArgumentNullException(nameof(buffer));

			this._rtBitmap = new MemoryStream(buffer);
			this.Header = this.CreateHeader();
		}

		/// <summary>Close DIB stream and free all resources</summary>
		public override void Close()
		{
			this._rtBitmap.Close();
			base.Close();
		}

		private Byte[] CreateHeader()
		{
			BinaryReader reader = new BinaryReader(this._rtBitmap);

			Int32 headerSize = reader.ReadInt32();
			Int32 pixelSize = (Int32)this._rtBitmap.Length - headerSize;
			Int32 fileSize = 14 + headerSize + pixelSize;

			/* Get the palette size
			The Palette size is stored as an int32 at offset 32
			Actually stored as number of colours, so multiply by 4*/
			this._rtBitmap.Position = 32;
			Int32 paletteSize = 4 * reader.ReadInt32();

			// Get the palette size from the bbp if none was specified
			if(paletteSize == 0)
			{// Get the bits per pixel. The bits per pixel is store as an int16 at offset 14
				this._rtBitmap.Position = 14;
				Int32 bpp = reader.ReadInt16();

				// Only set the palette size if the bpp < 16
				if(bpp < 16)
					paletteSize = 4 * (2 << (bpp - 1));
			}
			this._rtBitmap.Position = 0;

			BitmapHeader header = new BitmapHeader()
			{
				magic = new Byte[] { (Byte)'B', (Byte)'M' },
				fileSize = fileSize,
				Unk1 = 0,
				headerSize = 14 + headerSize + paletteSize
			};
			return PinnedBufferReader.StructureToArray<BitmapHeader>(header);
		}

		/// <summary>Read data from DIB stream</summary>
		/// <param name="buffer">Target buffer</param>
		/// <param name="offset">offset from current stream position</param>
		/// <param name="count">Count of bytes to read</param>
		/// <returns>Number of bytes</returns>
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			Int32 dibCount = count;
			Int32 dibOffset = offset - 14;
			Int32 result = 0;
			if(this._position < 14)
			{
				Int32 headerCount = Math.Min(count + (Int32)this._position, 14);
				Array.Copy(this.Header, this._position, buffer, offset, headerCount);
				dibCount -= headerCount;
				this._position += headerCount;
				result = headerCount;
			}
			if(this._position >= 14)
			{
				result += this._rtBitmap.Read(buffer, offset + result, dibCount);
				this._position = 14 + _rtBitmap.Position;
			}
			return (Int32)result;
		}

		/// <summary>useless</summary>
		public override void Flush() { }

		/// <summary>not implemented</summary>
		/// <returns></returns>
		public override Int64 Seek(Int64 offset, SeekOrigin origin)
			=> throw new NotImplementedException();

		/// <summary>not implemented</summary>
		public override void SetLength(Int64 value)
			=> throw new NotImplementedException();

		/// <summary>not implemented</summary>
		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
			=> throw new NotImplementedException();
	}
}