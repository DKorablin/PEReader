using System;
using AlphaOmega.Debug.CorDirectory;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>.NET directory class</summary>
	[DefaultProperty("Cor20Header")]
	public class ComDescriptor : PEDirectoryBase
	{
		#region Fields
		private WinNT.IMAGE_COR20_HEADER? _cor20Header;
		private MetaData _metaData;
		private ResourceTable _resource;
		private VTable _vTable;
		private StrongNameSignature _strongNameSignature;
		private CodeManagerTable _codeManagerTable;
		private Eat _eat;
		private ManagedNativeHeaer _managedNativeHeaer;
		#endregion Fields

		/// <summary>Заголовок .NET приложения</summary>
		public WinNT.IMAGE_COR20_HEADER Cor20Header
		{
			get
			{
				return (this._cor20Header == null
					? this._cor20Header = this.Parent.Header.PtrToStructure<WinNT.IMAGE_COR20_HEADER>(base.Directory.VirtualAddress)
					: this._cor20Header).Value;
			}
		}

		/// <summary>Meta data tables</summary>
		public MetaData MetaData
		{
			get
			{
				return this._metaData == null
					? this._metaData = new MetaData(this)
					: this._metaData;
			}
		}

		/// <summary>VTable directory</summary>
		public VTable VTable
		{//TODO: В начале должен быть заголовок. А в нём должно указываться кол-во и т.п. Пока соотв. файл найти не удалось...
			get
			{
				return this._vTable == null
					? this._vTable = new VTable(this)
					: this._vTable;
			}
		}

		/// <summary>Strong name signature directory</summary>
		public StrongNameSignature StrongNameSignature
		{
			get
			{
				return this._strongNameSignature == null
					? this._strongNameSignature = new StrongNameSignature(this)
					: this._strongNameSignature;
			}
		}

		/// <summary>Code manager table directory</summary>
		public CodeManagerTable CodeManagerTable
		{
			get
			{
				return this._codeManagerTable == null
					? this._codeManagerTable = new CodeManagerTable(this)
					: this._codeManagerTable;
			}
		}

		/// <summary>Export Address table jumps directory</summary>
		public Eat Eat
		{
			get
			{
				return this._eat == null
					? this._eat = new Eat(this)
					: this._eat;
			}
		}

		/// <summary>Managed header</summary>
		public ManagedNativeHeaer ManagedNativeHeaer
		{
			get
			{
				return this._managedNativeHeaer == null
					? this._managedNativeHeaer = new ManagedNativeHeaer(this)
					: this._managedNativeHeaer;
			}
		}

		/// <summary>Managed resources</summary>
		public ResourceTable Resources
		{
			get
			{
				if(this._resource == null)
					this._resource = new ResourceTable(this);

				return this._resource.IsEmpty
					? null
					: this._resource;
			}
		}

		/// <summary>Create instance of .NET directory</summary>
		/// <param name="root">Data directory</param>
		public ComDescriptor(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.CLR_HEADER)
		{
		}
	}
}