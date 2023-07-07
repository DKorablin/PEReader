using System;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Resource item</summary>
	public class ResourceTableItem
	{
		#region Fields
		private ResourceTableReader Reader { get; }
		private String _type;
		private Byte[] _data;
		#endregion Fields
		#region Properties
		/// <summary>Name of resource item</summary>
		public String Name { get; }

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
			this.Reader = reader ?? throw new ArgumentNullException(nameof(reader));
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		/// <summary>Create instance of streamed resource item</summary>
		/// <param name="name">Reasource name in the .resource item</param>
		/// <param name="type">Resource type</param>
		/// <param name="data">Resource data</param>
		internal ResourceTableItem(String name, String type, Byte[] data)
		{
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
			this._type = type;
			this._data = data;
		}

		/// <summary>Read resource data from resource stream</summary>
		private void Initialize()
		{
			if(this._type == null && this._data == null)
				this.Reader.GetResourceData(this.Name, out this._type, out this._data);
		}
	}
}