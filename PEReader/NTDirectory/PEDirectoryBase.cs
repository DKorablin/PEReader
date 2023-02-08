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
		private readonly WinNT.IMAGE_DIRECTORY_ENTRY _directory;

		private readonly PEFile _parent;

		/// <summary>Директория PE файла</summary>
		internal PEFile Parent { get { return this._parent; } }

		/// <summary>Директроия пустая</summary>
		public virtual Boolean IsEmpty { get { return this.Directory.IsEmpty; } }

		/// <summary>Данные директории</summary>
		public WinNT.IMAGE_DATA_DIRECTORY Directory { get { return this.Parent[this._directory]; } }

		/// <summary>Create instance</summary>
		/// <param name="parent">Parent PE directory</param>
		/// <param name="directory">Directory type</param>
		/// <exception cref="T:ArgumentNullException">parent directory is null</exception>
		public PEDirectoryBase(PEFile parent, WinNT.IMAGE_DIRECTORY_ENTRY directory)
		{
			this._parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this._directory = directory;
		}

		/// <summary>Получить все данные из директории</summary>
		/// <returns>Массив данных из директории</returns>
		public Byte[] GetData()
		{
			if(this.IsEmpty)
				return new Byte[] { };

			WinNT.IMAGE_DATA_DIRECTORY directory = this.Directory;
			return this.Parent.Header.ReadBytes(directory.VirtualAddress, directory.Size);
		}
	}
}