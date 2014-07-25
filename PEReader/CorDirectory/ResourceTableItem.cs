using System;
using System.Resources;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Resource item</summary>
	public class ResourceTableItem
	{
		#region Fields
		private readonly ResourceTableReader _reader;
		private readonly String _name;
		private String _type;
		private Byte[] _data;
		#endregion Fields
		#region Properties
		/// <summary>Name of resource item</summary>
		public String Name { get { return this._name; } }
		/// <summary>Type of resource item</summary>
		public String Type
		{
			get
			{
				if(this._type == null)
					this.Initialize();
				return this._type;
			}
		}
		/// <summary>Data of resource item</summary>
		public Byte[] Data
		{
			get
			{
				if(this._data == null)
					this.Initialize();
				return this._data;
			}
		}
		#endregion Properties
		/// <summary>Create instance of a resource item</summary>
		/// <param name="reader">Resource reader</param>
		/// <param name="name">Reasource name in the .resource item</param>
		internal ResourceTableItem(ResourceTableReader reader, String name)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this._reader = reader;
			this._name = name;
		}
		/// <summary>Create instance of streamed resource item</summary>
		/// <param name="name">Resource name</param>
		/// <param name="type">Resource type</param>
		/// <param name="data">Resource data</param>
		internal ResourceTableItem(String name, String type, Byte[] data)
		{
			this._name = name;
			this._type = type;
			this._data = data;
		}
		/// <summary>Read resource data from resource stream</summary>
		private void Initialize()
		{
			if(this._type == null && this._data == null)
				this._reader.GetResourceData(this.Name, out this._type, out this._data);
		}
	}
}