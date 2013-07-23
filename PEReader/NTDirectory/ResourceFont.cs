using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Font resource class</summary>
	[DefaultProperty("Font")]
	public class ResourceFont : ResourceBase
	{
		/// <summary>Font info</summary>
		[DefaultProperty("szFaceName")]
		public class FontEntry
		{
			/// <summary>Contains information about an individual font in a font resource group.</summary>
			public WinNT.Resource.FONTDIRENTRY Font;
			/// <summary>The name of the device if this font file is designated for a specific device.</summary>
			public String szDeviceName;
			/// <summary>The typeface name of the font.</summary>
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
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT)
		{
		}
		internal static void GetFont<T>(PinnedBufferReader reader, ref T result, ref UInt32 padding) where T : FontEntry
		{
			WinNT.Resource.FONTDIRENTRY font = reader.BytesToStructure<WinNT.Resource.FONTDIRENTRY>(ref padding);
			String szDeviceName;
			String szFaceName;
			if(font.dfDevice > 0 && font.dfDevice<reader.Length)
				szDeviceName = reader.BytesToStringAnsi(font.dfDevice);
			else szDeviceName = reader.BytesToStringAnsi(ref padding);

			if(font.dfFace > 0 && font.dfFace < reader.Length)
				szFaceName = reader.BytesToStringAnsi(font.dfFace);
			else
				szFaceName = reader.BytesToStringAnsi(ref padding);

			result.Font = font;
			result.szDeviceName = szDeviceName;
			result.szFaceName = szFaceName;
		}
	}
}