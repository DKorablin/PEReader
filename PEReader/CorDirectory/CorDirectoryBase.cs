using System;
using System.ComponentModel;
using ComDescriptor = AlphaOmega.Debug.NTDirectory.ComDescriptor;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Base .NET directory</summary>
	[DefaultProperty(nameof(Directory))]
	public class CorDirectoryBase : IDirectory, ISectionData
	{
		private WinNT.COR20_DIRECTORY_ENTRY DirectoryI { get; }

		/// <summary>Parent PE directory</summary>
		internal ComDescriptor Parent { get; }

		/// <summary>Directory is empty</summary>
		public virtual Boolean IsEmpty => this.Directory.IsEmpty;

		/// <summary>Data in directory</summary>
		public WinNT.IMAGE_DATA_DIRECTORY Directory => this.Parent.Cor20Header[this.DirectoryI];

		/// <summary>Create instance of base .NET directory</summary>
		/// <param name="parent">PE directory</param>
		/// <param name="directory">.NET directory type</param>
		public CorDirectoryBase(ComDescriptor parent, WinNT.COR20_DIRECTORY_ENTRY directory)
		{
			this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.DirectoryI = directory;
		}

		/// <summary>Get all data from directory</summary>
		/// <returns>Data from directory</returns>
		public Byte[] GetData()
			=> this.IsEmpty
				? new Byte[] { }
				: this.Parent.Parent.Header.ReadBytes(this.Directory.VirtualAddress, this.Directory.Size);
	}
}