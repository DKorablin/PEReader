using System;
using System.ComponentModel;
using System.IO;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Bitmap resource class</summary>
	[DefaultProperty("Header")]
	public class ResourceBitmap : ResourceBase
	{
		private const Int32 BITMAPFILEHEADER_SIZE = 14;
		private const UInt32 BITMAPCOREHEADER_SIZE = 12;
		private const UInt32 BITMAPINFOHEADER_SIZE = 40;

		/// <summary>Bitmap header</summary>
		public WinGdi.BITMAPINFOHEADER Header
			=> PinnedBufferReader.BytesToStructure<WinGdi.BITMAPINFOHEADER>(this.Directory.GetData(), 0);

		/// <summary>Create instance of bitmap resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceBitmap(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP) { }

		/// <summary>Convert DIB to a complete .bmp file stream</summary>
		/// <remarks>RT_BITMAP resources are stored without BITMAPFILEHEADER, so it is prepended to the resource data</remarks>
		/// <exception cref="InvalidOperationException">Resource data is too short to contain a bitmap header</exception>
		/// <returns>Read-only seekable stream for System.Drawing.Bitmap</returns>
		public Stream GetBitmapStream()
		{
			Byte[] dib = this.Directory.GetData();
			if(dib == null || dib.Length < sizeof(UInt32))
				throw new InvalidOperationException("Bitmap resource is too short");

			UInt32 headerSize = BitConverter.ToUInt32(dib, 0);
			Int64 fileSize = BITMAPFILEHEADER_SIZE + dib.Length;
			Int64 offBits = BITMAPFILEHEADER_SIZE + (Int64)headerSize + ResourceBitmap.GetColorTableSize(dib, headerSize);
			if(offBits > fileSize)//Corrupted header or color count
				offBits = fileSize;

			// BITMAPFILEHEADER: bfType, bfSize, bfReserved1, bfReserved2, bfOffBits
			Byte[] file = new Byte[fileSize];
			file[0] = (Byte)'B';
			file[1] = (Byte)'M';
			ResourceBitmap.WriteUInt32(file, 2, (UInt32)fileSize);
			ResourceBitmap.WriteUInt32(file, 10, (UInt32)offBits);
			Buffer.BlockCopy(dib, 0, file, BITMAPFILEHEADER_SIZE, dib.Length);

			return new MemoryStream(file, false);
		}

		/// <summary>Size of the data between the DIB header and the pixel array (color masks + color table)</summary>
		/// <param name="dib">DIB data starting with the bitmap header</param>
		/// <param name="headerSize">Size of the bitmap header from the first DWORD of the DIB</param>
		/// <returns>Size in bytes</returns>
		private static Int64 GetColorTableSize(Byte[] dib, UInt32 headerSize)
		{
			if(headerSize == BITMAPCOREHEADER_SIZE)
			{// OS/2 BITMAPCOREHEADER: bcBitCount at offset 10, color table of RGBTRIPLE entries
				if(dib.Length < BITMAPCOREHEADER_SIZE)
					return 0;

				UInt16 bitCount = BitConverter.ToUInt16(dib, 10);
				return bitCount >= 1 && bitCount <= 8 ? 3L << bitCount : 0;
			}

			if(headerSize < BITMAPINFOHEADER_SIZE || dib.Length < BITMAPINFOHEADER_SIZE)
				return 0;

			WinGdi.BITMAPINFOHEADER header = PinnedBufferReader.BytesToStructure<WinGdi.BITMAPINFOHEADER>(dib, 0);

			// BITMAPINFOHEADER with BI_BITFIELDS is followed by 3 DWORD color masks (V4/V5 headers contain them inside the header)
			Int64 masksSize = headerSize == BITMAPINFOHEADER_SIZE && header.biCompression == WinGdi.BI.BITFIELDS
				? 3 * sizeof(UInt32)
				: 0;

			Int64 colorsCount = header.biClrUsed;
			if(colorsCount == 0 && header.biBitCount >= 1 && header.biBitCount <= 8)
				colorsCount = 1L << header.biBitCount;

			return masksSize + (colorsCount * 4);// RGBQUAD entries
		}

		private static void WriteUInt32(Byte[] buffer, Int32 offset, UInt32 value)
		{// Little-endian regardless of platform
			buffer[offset] = (Byte)value;
			buffer[offset + 1] = (Byte)(value >> 8);
			buffer[offset + 2] = (Byte)(value >> 16);
			buffer[offset + 3] = (Byte)(value >> 24);
		}
	}
}