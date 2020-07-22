using System;
using System.ComponentModel;
using System.IO;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Bitmap resource class</summary>
	[DefaultProperty("Header")]
	public class ResourceBitmap : ResourceBase
	{
		/// <summary>Bitmap header</summary>
		public WinGdi.BITMAPINFOHEADER Header
		{
			get { return PinnedBufferReader.BytesToStructure<WinGdi.BITMAPINFOHEADER>(base.Directory.GetData(), 0); }
		}

		/// <summary>Create instance of bitmap resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceBitmap(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP)
		{
		}

		/// <summary>Convert DIB to GDI stream</summary>
		/// <returns>Stream for System.Drawing.Bitmap</returns>
		public Stream GetBitmapStream()
		{
			return new DibStream(base.Directory.GetData());
		}
	}
}