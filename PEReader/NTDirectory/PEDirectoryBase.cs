using System;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Base class of PE directory</summary>
	[DefaultProperty("Directory")]
	[DebuggerDisplay("Directory={Directory}")]
	public class PEDirectoryBase : IDirectory, ISectionData
	{
		private WinNT.IMAGE_DIRECTORY_ENTRY DirectoryI { get; }

		/// <summary>PE file directory</summary>
		internal PEFile Parent { get; }

		/// <summary>Directory is empty</summary>
		public virtual Boolean IsEmpty { get { return this.Directory.IsEmpty; } }

		/// <summary>Directory data</summary>
		public WinNT.IMAGE_DATA_DIRECTORY Directory { get { return this.Parent[this.DirectoryI]; } }

		/// <summary>Create instance</summary>
		/// <param name="parent">Parent PE directory</param>
		/// <param name="directory">Directory type</param>
		/// <exception cref="ArgumentNullException">parent directory is null</exception>
		public PEDirectoryBase(PEFile parent, WinNT.IMAGE_DIRECTORY_ENTRY directory)
		{
			this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.DirectoryI = directory;
		}

		/// <summary>Get all data from a directory</summary>
		/// <returns>Array of data from a directory</returns>
		public Byte[] GetData()
		{
			if(this.IsEmpty)
				return new Byte[] { };

			WinNT.IMAGE_DATA_DIRECTORY directory = this.Directory;
			return this.Parent.Header.ReadBytes(directory.VirtualAddress, directory.Size);
		}
	}
}