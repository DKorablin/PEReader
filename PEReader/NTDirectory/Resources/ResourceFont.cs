using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Font resource class</summary>
	[DefaultProperty(nameof(Font))]
	public class ResourceFont : ResourceBase
	{
		/// <summary>Font info</summary>
		[DefaultProperty(nameof(szFaceName))]
		public class FontEntry
		{
			/// <summary>Contains information about an individual font in a font resource group</summary>
			public WinGdi.FONTDIRENTRY Font;

			/// <summary>The name of the device if this font file is designated for a specific device</summary>
			public String szDeviceName;

			/// <summary>The typeface name of the font</summary>
			public String szFaceName;
		}

		/// <summary>Font in resource directory</summary>
		public FontEntry Font
		{
			get
			{
				UInt32 padding = 0;
				using(PinnedBufferReader reader = base.CreateDataReader())
				{
					FontEntry result = new FontEntry();
					ResourceFont.GetFont(reader, ref result, ref padding);
					return result;
				}
			}
		}

		/// <summary>Create instance of font resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceFont(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT) { }

		internal static void GetFont<T>(PinnedBufferReader reader, ref T result, ref UInt32 padding) where T : FontEntry
		{
			WinGdi.FONTDIRENTRY font = reader.BytesToStructure<WinGdi.FONTDIRENTRY>(ref padding);
			String szDeviceName;
			String szFaceName;

			szDeviceName = font.dfDevice > 0 && font.dfDevice < reader.Length
				? reader.BytesToStringAnsi(font.dfDevice)
				: reader.BytesToStringAnsi(ref padding);

			szFaceName = font.dfFace > 0 && font.dfFace < reader.Length
				? reader.BytesToStringAnsi(font.dfFace)
				: reader.BytesToStringAnsi(ref padding);

			result.Font = font;
			result.szDeviceName = szDeviceName;
			result.szFaceName = szFaceName;
		}
	}
}