using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Icon resource reader class</summary>
	[DefaultProperty("Header")]
	public class ResourceIcon : ResourceBase
	{
		/// <summary>Icon header</summary>
		public WinGdi.GRPICONDIRENTRY Header
		{
			get { return PinnedBufferReader.BytesToStructure<WinGdi.GRPICONDIRENTRY>(base.Directory.GetData(), 0); }
		}

		/// <summary>Create instance of RT_ICON resource reader class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceIcon(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON)
		{
		}
	}
}