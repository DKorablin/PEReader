using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>GDI procedure declarations, constant definitions and macros</summary>
	public struct WinGdi
	{
		/// <summary>Bitmap info type</summary>
		public enum BI
		{
			/// <summary>An uncompressed format</summary>
			RGB = 0,
			/// <summary>
			/// A run-length encoded (RLE) format for bitmaps with 8 bpp.
			/// The compression format is a 2-byte format consisting of a count byte followed by a byte containing a color index
			/// </summary>
			RLE8 = 1,
			/// <summary>
			/// An RLE format for bitmaps with 4 bpp.
			/// The compression format is a 2-byte format consisting of a count byte followed by two word-length color indexes
			/// </summary>
			RLE4 = 2,
			/// <summary>Specifies that the bitmap is not compressed and that the color table consists of three DWORD color masks that specify the red, green, and blue components, respectively, of each pixel</summary>
			/// <remarks>This is valid when used with 16- and 32-bpp bitmaps</remarks>
			BITFIELDS = 3,
			/// <summary>Indicates that the image is a JPEG image</summary>
			JPEG = 4,
			/// <summary>Indicates that the image is a PNG image</summary>
			PNG = 5,
		}

		/// <summary>The BITMAPINFOHEADER structure contains information about the dimensions and color format of a DIB</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/dd183376.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct BITMAPINFOHEADER
		{
			/// <summary>The number of bytes required by the structure</summary>
			public UInt32 biSize;

			/// <summary>
			/// The width of the bitmap, in pixels</summary>
			/// <remarks>If biCompression is BI_JPEG or BI_PNG, the biWidth member specifies the width of the decompressed JPEG or PNG image file, respectively</remarks>
			public Int32 biWidth;

			/// <summary>
			/// The height of the bitmap, in pixels.
			/// If biHeight is positive, the bitmap is a bottom-up DIB and its origin is the lower-left corner. If biHeight is negative, the bitmap is a top-down DIB and its origin is the upper-left corner
			/// </summary>
			/// <remarks>
			/// If biHeight is negative, indicating a top-down DIB, biCompression must be either BI_RGB or BI_BITFIELDS. Top-down DIBs cannot be compressed.
			/// If biCompression is BI_JPEG or BI_PNG, the biHeight member specifies the height of the decompressed JPEG or PNG image file, respectively
			/// </remarks>
			public Int32 biHeight;

			/// <summary>The number of planes for the target device. This value must be set to 1</summary>
			public UInt16 biPlanes;

			/// <summary>The number of bits-per-pixel. The biBitCount member of the BITMAPINFOHEADER structure determines the number of bits that define each pixel and the maximum number of colors in the bitmap</summary>
			public UInt16 biBitCount;

			/// <summary>The type of compression for a compressed bottom-up bitmap (top-down DIBs cannot be compressed)</summary>
			public BI biCompression;

			/// <summary>The size, in bytes, of the image. This may be set to zero for BI_RGB bitmaps</summary>
			/// <remarks>If biCompression is BI_JPEG or BI_PNG, biSizeImage indicates the size of the JPEG or PNG image buffer, respectively</remarks>
			public UInt32 biSizeImage;

			/// <summary>
			/// The horizontal resolution, in pixels-per-meter, of the target device for the bitmap.
			/// An application can use this value to select a bitmap from a resource group that best matches the characteristics of the current device
			/// </summary>
			public Int32 biXPelsPerMeter;

			/// <summary>The vertical resolution, in pixels-per-meter, of the target device for the bitmap</summary>
			public Int32 biYPelsPerMeter;

			/// <summary>
			/// The number of color indexes in the color table that are actually used by the bitmap.
			/// If this value is zero, the bitmap uses the maximum number of colors corresponding to the value of the biBitCount member for the compression mode specified by biCompression
			/// </summary>
			/// <remarks>
			/// If biClrUsed is nonzero and the biBitCount member is less than 16, the biClrUsed member specifies the actual number of colors the graphics engine or device driver accesses.
			/// If biBitCount is 16 or greater, the biClrUsed member specifies the size of the color table used to optimize performance of the system color palettes.
			/// If biBitCount equals 16 or 32, the optimal color palette starts immediately following the three DWORD masks
			/// 
			/// When the bitmap array immediately follows the BITMAPINFO structure, it is a packed bitmap.
			/// Packed bitmaps are referenced by a single pointer.
			/// Packed bitmaps require that the biClrUsed member must be either zero or the actual size of the color table
			/// </remarks>
			public UInt32 biClrUsed;

			/// <summary>The number of color indexes that are required for displaying the bitmap</summary>
			/// <remarks>If this value is zero, all colors are required</remarks>
			public UInt32 biClrImportant;

			/// <summary>Number of colors in palette</summary>
			public UInt32 ColorsCount
			{
				get
				{
					if(this.biClrUsed != 0)
						return this.biClrUsed;
					else if(this.biBitCount <= 8)
						return (UInt32)(1 << this.biBitCount);
					else
						return 0;
				}
			}

			/// <summary>Xor Image Size</summary>
			public Int32 XorImageSize
			{
				get
				{
					return (Int32)(this.biHeight / 2 *
					BITMAPINFOHEADER.GetDibRowWidth(this.biWidth * this.biBitCount * this.biPlanes));
				}
			}

			/// <summary>Size of the image mask</summary>
			public Int32 MaskImageSize
				=> (Int32)(this.biHeight / 2 * BITMAPINFOHEADER.GetDibRowWidth(this.biWidth));

			/// <summary>
			/// Returns the width of a row in a DIB Bitmap given the number of bits.
			/// DIB Bitmap rows always align on a DWORD boundary
			/// </summary>
			/// <param name="width">Number of bits</param>
			/// <returns>Width of a row in bytes</returns>
			public static Int32 GetDibRowWidth(Int32 width)
				=> (Int32)((width + 31) / 32) * 4;
		}

		/// <summary>Contains information about an individual font in a font resource group</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms648014%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
		public struct FONTDIRENTRY
		{
			/// <summary>A user-defined version number for the resource data that tools can use to read and write resource files</summary>
			public UInt16 dfVersion;

			/// <summary>The size of the file, in bytes</summary>
			public UInt32 dfSize;

			/// <summary>The font supplier's copyright information</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
			public String dfCopyright;

			/// <summary>The type of font file</summary>
			public UInt16 dfType;

			/// <summary>The point size at which this character set looks best</summary>
			public UInt16 dfPoints;

			/// <summary>The vertical resolution, in dots per inch, at which this character set was digitized</summary>
			public UInt16 dfVertRes;

			/// <summary>The horizontal resolution, in dots per inch, at which this character set was digitized</summary>
			public UInt16 dfHorizRes;

			/// <summary>The distance from the top of a character definition cell to the baseline of the typographical font</summary>
			public UInt16 dfAscent;

			/// <summary>The amount of leading inside the bounds set by the dfPixHeight member</summary>
			/// <remarks>Accent marks and other diacritical characters can occur in this area</remarks>
			public UInt16 dfInternalLeading;

			/// <summary>The amount of extra leading that the application adds between rows</summary>
			public UInt16 dfExternalLeading;

			/// <summary>An italic font if not equal to zero</summary>
			public Byte dfItalic;

			/// <summary>An underlined font if not equal to zero</summary>
			public Byte dfUnderline;

			/// <summary>A strikeout font if not equal to zero</summary>
			public Byte dfStrikeOut;

			/// <summary>
			/// The weight of the font in the range 0 through 1000.
			/// For example, 400 is roman and 700 is bold. If this value is zero, a default weight is used
			/// </summary>
			public FW dfWeight;

			/// <summary>The character set of the font</summary>
			public CHARSET dfCharSet;

			/// <summary>
			/// The width of the grid on which a vector font was digitized.
			/// For raster fonts, if the member is not equal to zero, it represents the width for all the characters in the bitmap.
			/// If the member is equal to zero, the font has variable-width characters
			/// </summary>
			public UInt16 dfPixWidth;

			/// <summary>The height of the character bitmap for raster fonts or the height of the grid on which a vector font was digitized</summary>
			public UInt16 dfPixHeight;

			/// <summary>The pitch and the family of the font</summary>
			public Byte dfPitchAndFamily;

			/// <summary>
			/// The average width of characters in the font (generally defined as the width of the letter x).
			/// This value does not include the overhang required for bold or italic characters
			/// </summary>
			public UInt16 dfAvgWidth;

			/// <summary>The width of the widest character in the font</summary>

			public UInt16 dfMaxWidth;

			/// <summary>The first character code defined in the font</summary>
			public Byte dfFirstChar;

			/// <summary>The last character code defined in the font</summary>
			public Byte dfLastChar;

			/// <summary>The character to substitute for characters not in the font</summary>
			public Byte dfDefaultChar;

			/// <summary>The character that will be used to define word breaks for text justification</summary>
			public Byte dfBreakChar;

			/// <summary>
			/// The number of bytes in each row of the bitmap.
			/// This value is always even so that the rows start on word boundaries.
			/// For vector fonts, this member has no meaning
			/// </summary>
			public UInt16 dfWidthBytes;

			/// <summary>The offset in the file to a null-terminated string that specifies a device name</summary>
			/// <remarks>For a generic font, this value is zero</remarks>
			public UInt32 dfDevice;

			/// <summary>The offset in the file to a null-terminated string that names the typeface</summary>
			public UInt32 dfFace;

			/// <summary>This member is reserved</summary>
			public UInt32 dfReserved;

			//public Char szDeviceName;

			//public Char szFaceName;

			/// <summary>The font is italic</summary>
			public Boolean IsItalic => this.dfItalic > 0;

			/// <summary>The font is underline</summary>
			public Boolean IsUnderline => this.dfUnderline > 0;

			/// <summary>The font is strikeout</summary>
			public Boolean IsStrikeOut => this.dfStrikeOut > 0;
		}

		/// <summary>Font weight</summary>
		public enum FW : UInt16
		{
			/// <summary>Ignore</summary>
			DONTCARE = 0,
			/// <summary>Thin</summary>
			THIN = 100,
			/// <summary>Extra light</summary>
			EXTRALIGHT = 200,
			/// <summary>Light</summary>
			LIGHT = 300,
			/// <summary>Normal</summary>
			NORMAL = 400,
			/// <summary>Medium</summary>
			MEDIUM = 500,
			/// <summary>Semi bold</summary>
			SEMIBOLD = 600,
			/// <summary>Bold</summary>
			BOLD = 700,
			/// <summary>Extra bold</summary>
			EXTRABOLD = 800,
			/// <summary>Heavy</summary>
			HEAVY = 900,
		}

		/// <summary>The character set</summary>
		public enum CHARSET : Byte
		{
			/// <summary>ANSI charset</summary>
			ANSI = 0,
			/// <summary>
			/// DEFAULT_CHARSET is set to a value based on the current system locale.
			/// For example, when the system locale is English (United States), it is set as ANSI_CHARSET
			/// </summary>
			DEFAULT = 1,
			/// <summary>Symbol charset</summary>
			SYMBOL = 2,
			/// <summary>Mac charset</summary>
			MAC = 77,
			/// <summary>Shiftjis charset</summary>
			SHIFTJIS = 128,
			/// <summary>Hangeul charset</summary>
			HANGEUL = 129,
			/// <summary>Johab charset</summary>
			JOHAB = 130,
			/// <summary>Gb2312 charset</summary>
			GB2312 = 134,
			/// <summary>Chinesebig5 charset</summary>
			CHINESEBIG5 = 136,
			/// <summary>Greek charset</summary>
			GREEK = 161,
			/// <summary>Turkish charset</summary>
			TURKISH = 162,
			/// <summary>Vietnamese charset</summary>
			VIETNAMESE = 163,
			/// <summary>Hebrew charset</summary>
			HEBREW = 177,
			/// <summary>Arabic charset</summary>
			ARABIC = 178,
			/// <summary>Baltic charset</summary>
			BALTIC = 186,
			/// <summary>Thai charset</summary>
			THAI = 222,
			/// <summary>Easteurope charset</summary>
			EASTEUROPE = 238,
			/// <summary>Russian charset</summary>
			RUSSIAN = 204,
			/// <summary>The OEM_CHARSET value specifies a character set that is operating-system dependent</summary>
			OEM = 255,
		}

		/// <summary>Test data (not used)</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct GRPICONDIR
		{
			/// <summary>Reserved (must be 0)</summary>
			public UInt16 idReserved;
			
			/// <summary>Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid</summary>
			public UInt16 idType;
			
			/// <summary>Specifies number of images in the file</summary>
			public UInt16 idCount;
		}

		/// <summary>There exists one GRPICONDIRENTRY for each icon image in the resource, providing details about its size and color depth</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/ms997538.aspx</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct GRPICONDIRENTRY
		{
			/// <summary>Width, in pixels, of the image</summary>
			public Byte bWidth;

			/// <summary>Height, in pixels, of the image</summary>
			public Byte bHeight;

			/// <summary>Number of colors in image (0 if >=8bpp)</summary>
			public Byte bColorCount;

			/// <summary>Reserved</summary>
			public Byte bReserved;

			/// <summary>
			/// In ICO format: Specifies color planes. Should be 0 or 1.
			/// In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left</summary>
			public UInt16 wPlanes;

			/// <summary>
			/// In ICO format: Specifies bits per pixel.
			/// In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top
			/// </summary>
			public UInt16 wBitCount;

			/// <summary>The dwBytesInRes member indicates the total size of the RT_ICON resource referenced by the nID member</summary>
			public UInt32 dwBytesInRes;

			/// <summary>nID is the RT_ICON identifier that can be passed to FindResource, LoadResource and LockResource to obtain a pointer to the ICONIMAGE structure (defined above) for this image</summary>
			public UInt16 nID;
		}

		/// <summary>There exists one ICONDIRENTRY for each icon image in the file, providing details about its location in the file, size and color depth</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/ms997538.aspx</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public struct ICONDIRENTRY
		{
			/// <summary>Width, in pixels, of the image</summary>
			public Byte bWidth;
			/// <summary>Height, in pixels, of the image</summary>
			public Byte bHeight;
			/// <summary>Number of colors in image (0 if >=8bpp)</summary>
			public Byte bColorCount;
			/// <summary>Reserved ( must be 0)</summary>
			public Byte bReserved;
			/// <summary>Color Planes</summary>
			public UInt16 wPlanes;
			/// <summary>Bits per pixel</summary>
			public UInt16 wBitCount;
			/// <summary>How many bytes in this resource?</summary>
			public UInt32 dwBytesInRes;
			/// <summary>Where in the file is this image?</summary>
			public UInt32 dwImageOffset;
		}
	}
}