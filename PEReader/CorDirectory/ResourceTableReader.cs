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

		/// <summary>This type of resource fragment can be read by default <see cref="System.Resources.ResourceReader"/> class.</summary>
		public Boolean CanRead
		{
			get
			{
				Int32 magicNumber;
				/*if(BitConverter.IsLittleEndian)
				{
					Byte[] bytes = new Byte[4];
					Array.Copy(this._file, bytes, 4);
					Array.Reverse(bytes);
					magicNumber = checked((UInt32)BitConverter.ToInt32(bytes, 0));
				} else*/
					magicNumber = BitConverter.ToInt32(this._file, 0);

				return (UInt32)magicNumber == Cor.ResourceManagerHeader.MagicNumberConst;
			}
		}

		/// <summary>Enumerates .resources files and streams, reading sequential resource name and value pairs.</summary>
		public IResourceReader Reader
		{
			get
			{
				if(this._reader == null && this.CanRead)
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
		/// <exception cref="InvalidOperationException">This type of resource file cannot be read</exception>
		/// <returns>Resource item</returns>
		public IEnumerator<ResourceTableItem> GetEnumerator()
		{
			if(!this.CanRead)
				throw new InvalidOperationException(String.Format("This type of resource {0} cannot be read.", this.Name));

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

			GC.SuppressFinalize(this);
		}
	}
}