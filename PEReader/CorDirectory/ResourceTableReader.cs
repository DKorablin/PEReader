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
		private readonly Byte[] _file;
		private ResourceReader _reader;
		private readonly Object _readerLock = new Object();
		#endregion Fields
		#region Properties
		/// <summary>.resource name</summary>
		public String Name { get; }

		/// <summary>This type of resource fragment can be read by default <see cref="System.Resources.ResourceReader"/> class</summary>
		public Boolean CanRead
		{
			get
			{
				if(this._file.Length < 4)
					return false;

				UInt32 magic = (UInt32)(_file[0] | _file[1] << 8 | _file[2] << 16 | _file[3] << 24);
				return magic == Cor.ResourceManagerHeader.MagicNumberConst;

			}
		}

		/// <summary>Enumerates .resources files and streams, reading sequential resource name and value pairs</summary>
		/// <remarks>Returns <c>null</c> if <see cref="CanRead"/> is <c>false</c>. Reopened on demand after <see cref="Dispose"/></remarks>
		public IResourceReader Reader => this.GetReader();

		#endregion Properties
		internal ResourceTableReader(String name, Byte[] file)
		{
			if(file == null || file.Length == 0)
				throw new ArgumentNullException(nameof(file));

			this.Name = name ?? throw new ArgumentNullException(nameof(name));
			this._file = file;
		}

		internal void GetResourceData(String resourceName, out String resourceType, out Byte[] resourceData)
		{
			if(String.IsNullOrEmpty(resourceName))
				throw new ArgumentNullException(nameof(resourceName));

			ResourceReader reader = this.GetReader()
				?? throw new InvalidOperationException($"This type of resource {this.Name} cannot be read");

			reader.GetResourceData(resourceName, out resourceType, out resourceData);
		}

		/// <summary>Get or lazily create the underlying <see cref="ResourceReader"/></summary>
		/// <returns>Resource reader or <c>null</c> if <see cref="CanRead"/> is <c>false</c></returns>
		private ResourceReader GetReader()
		{
			ResourceReader reader = this._reader;
			if(reader != null || !this.CanRead)
				return reader;

			lock(this._readerLock)
			{
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

		/// <summary>Reads all elements from .resource item</summary>
		/// <exception cref="InvalidOperationException">This type of resource file cannot be read</exception>
		/// <returns>Resource item</returns>
		public IEnumerator<ResourceTableItem> GetEnumerator()
		{
			if(!this.CanRead)
				throw new InvalidOperationException($"This type of resource {this.Name} cannot be read");

			IDictionaryEnumerator enumerator = this.Reader.GetEnumerator();
			while(enumerator.MoveNext())
			{
				String name = enumerator.Key.ToString();
				yield return new ResourceTableItem(this, name);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>Close underlying <see cref="ResourceReader"/></summary>
		public void Dispose()
		{
			ResourceReader reader = this._reader;
			this._reader = null;
			reader?.Close();

			GC.SuppressFinalize(this);
		}
	}
}