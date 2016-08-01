using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Bitmap resource class</summary>
	[DefaultProperty("Header")]
	public class ResourceBitmap : ResourceBase
	{
		/// <summary>Bitmap header</summary>
		public WinNT.Resource.BITMAPINFOHEADER Header
		{
			get { return PinnedBufferReader.BytesToStructure<WinNT.Resource.BITMAPINFOHEADER>(base.Directory.GetData(), 0); }
		}
		/// <summary>Create instance of bitmap resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceBitmap(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP)
		{
		}
	}
}