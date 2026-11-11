using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Icon resource reader class</summary>
	/// <remarks>
	/// RT_ICON resource contains a single icon image without the ICO file header.
	/// The image is stored either as DIB (BITMAPINFOHEADER + XOR mask + AND mask) or, since Windows Vista, as a complete PNG file.
	/// </remarks>
	[DefaultProperty(nameof(Header))]
	public class ResourceIcon : ResourceBase
	{
		/// <summary>Icon image is stored as PNG file instead of DIB</summary>
		public Boolean IsPng
			=> WinGdi.PNGIMAGEHEADER.IsPng(base.Directory.GetData());

		/// <summary>DIB icon image header</summary>
		/// <remarks>null if icon image is stored as PNG. See <see cref="PngHeader"/></remarks>
		public WinGdi.BITMAPINFOHEADER? Header
		{
			get
			{
				Byte[] data = base.Directory.GetData();
				return WinGdi.PNGIMAGEHEADER.IsPng(data)
					? (WinGdi.BITMAPINFOHEADER?)null
					: PinnedBufferReader.BytesToStructure<WinGdi.BITMAPINFOHEADER>(data, 0);
			}
		}

		/// <summary>PNG icon image header</summary>
		/// <remarks>null if icon image is stored as DIB. See <see cref="Header"/></remarks>
		/// <exception cref="System.IO.InvalidDataException">PNG image is corrupted</exception>
		public WinGdi.PNGIMAGEHEADER? PngHeader
		{
			get
			{
				Byte[] data = base.Directory.GetData();
				return WinGdi.PNGIMAGEHEADER.IsPng(data)
					? WinGdi.PNGIMAGEHEADER.FromBytes(data)
					: (WinGdi.PNGIMAGEHEADER?)null;
			}
		}

		/// <summary>Create instance of <see cref="WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON"/> resource reader class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceIcon(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON) { }
	}
}