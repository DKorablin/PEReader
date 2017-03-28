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
		/// <summary>Получить поток для передачи в <see cref="T:System.Data.DataSet"/></summary>
		/// <remarks>Получаемый поток необходимо отпустить после выполнения действий</remarks>
		/// <returns>Поток для загрузки в DataSet</returns>
		public Stream GetXmlStream()
		{
			return new MemoryStream(base.Directory.GetData());
		}
	}
}