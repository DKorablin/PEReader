using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Managed resource table reader</summary>
	public class ResourceTableReader : IEnumerable<ResourceTableItem>, IDisposable
	{
		#region Fields
		private readonly String _name;
		private readonly Byte[] _file;
		private ResourceReader _reader;
		private readonly Object _readerLock = new Object();
		#endregion Fields
		#region Properties
		/// <summary>.resource name</summary>
		public String Name { get { return this._name; } }
		/// <summary>Enumerates .resources files and streams, reading sequential resource name and value pairs.</summary>
		public IResourceReader Reader
		{
			get
			{
				if(this._reader == null)
					lock(_readerLock)
						if(this._reader == null)
						{
							MemoryStream stream = new MemoryStream(this._file);
							try
							{
								this._reader = new ResourceReader(stream);
							} catch
							{
								stream.Dispose();
								throw;
							}
						}
				return this._reader;
			}
		}
		#endregion Properties
		internal ResourceTableReader(String name, Byte[] file)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			if(file == null || file.Length==0)
				throw new ArgumentNullException("file");

			this._name = name;
			this._file = file;
		}
		internal void GetResourceData(String resourceName, out String resourceType, out Byte[] resourceData)
		{
			if(String.IsNullOrEmpty(resourceName))
				throw new ArgumentNullException("resourceName");
			if(this._reader == null)
				throw new ArgumentNullException("_reader");

			this._reader.GetResourceData(resourceName, out resourceType, out resourceData);
		}
		/// <summary>Reads all elements from .resource item</summary>
		/// <returns>Resource item</returns>
		public IEnumerator<ResourceTableItem> GetEnumerator()
		{
			IDictionaryEnumerator enumerator = this.Reader.GetEnumerator();
			while(enumerator.MoveNext())
			{
				String name = enumerator.Key.ToString();
				yield return new ResourceTableItem(this, name);
			}
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		/// <summary>Close underlying <see cref="ResourceReader"/></summary>
		public void Dispose()
		{
			ResourceReader reader = this._reader;
			this._reader = null;
			if(reader != null)
				reader.Close();
		}
	}
}