using System;
using System.IO;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Xml resource Manifest reader class</summary>
	public class ResourceManifest : ResourceBase
	{
		/// <summary>Manifest xml</summary>
		public String Xml
		{
			get
			{
				Byte[] bytes = base.Directory.GetData();
				return System.Text.Encoding.GetEncoding((Int32)base.Directory.DataEntry.Value.CodePage).GetString(bytes);
			}
		}
		/// <summary>Create instance of manifest resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceManifest(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST)
		{
		}
		/// <summary>Get stream to send to System.Data.DataSet</summary>
		/// <remarks>The received stream must be released after the actions are completed</remarks>
		/// <returns>Stream to download to DataSet</returns>
		public Stream GetXmlStream()
		{
			return new MemoryStream(base.Directory.GetData());
		}
	}
}