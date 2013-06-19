using System;
using System.ComponentModel;
using ComDescriptor = AlphaOmega.Debug.NTDirectory.ComDescriptor;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Base .NET directory</summary>
	[DefaultProperty("Directory")]
	public class CorDirectoryBase : IDirectory, ISectionData
	{
		private readonly WinNT.COR20_DIRECTORY_ENTRY _directory;
		private readonly ComDescriptor _parent;

		/// <summary>Parent PE directory</summary>
		internal ComDescriptor Parent { get { return this._parent; } }
		/// <summary>Directory is empty</summary>
		public virtual Boolean IsEmpty { get { return this.Directory.IsEmpty; } }
		/// <summary>Data in directory</summary>
		public WinNT.IMAGE_DATA_DIRECTORY Directory { get { return this.Parent.Cor20Header[this._directory]; } }

		/// <summary>Create instance of base .NET directory</summary>
		/// <param name="parent">PE directory</param>
		/// <param name="directory">.NET directory type</param>
		public CorDirectoryBase(ComDescriptor parent, WinNT.COR20_DIRECTORY_ENTRY directory)
		{
			this._parent = parent;
			this._directory = directory;
		}

		/// <summary>Get all data from directory</summary>
		/// <returns>Data from directory</returns>
		public Byte[] GetData()
		{
			if(this.IsEmpty)
				return new Byte[] { };
			else
				return this.Parent.Parent.Header.ReadBytes(this.Directory.VirtualAddress, this.Directory.Size);
		}
	}
}