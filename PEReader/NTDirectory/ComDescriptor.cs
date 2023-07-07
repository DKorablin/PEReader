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
		private ManagedNativeHeader _managedNativeHeader;
		#endregion Fields

		/// <summary>.NET application header</summary>
		public WinNT.IMAGE_COR20_HEADER Cor20Header
		{
			get
			{
				return this._cor20Header
					?? (this._cor20Header = this.Parent.Header.PtrToStructure<WinNT.IMAGE_COR20_HEADER>(base.Directory.VirtualAddress)).Value;
			}
		}

		/// <summary>Meta data tables</summary>
		public MetaData MetaData
		{
			get { return this._metaData ?? (this._metaData = new MetaData(this)); }
		}

		/// <summary>VTable directory</summary>
		public VTable VTable
		{//TODO: В начале должен быть заголовок. А в нём должно указываться кол-во и т.п. Пока соотв. файл найти не удалось...
			get { return this._vTable ?? (this._vTable = new VTable(this)); }
		}

		/// <summary>Strong name signature directory</summary>
		public StrongNameSignature StrongNameSignature
		{
			get { return this._strongNameSignature ?? (this._strongNameSignature = new StrongNameSignature(this)); }
		}

		/// <summary>Code manager table directory</summary>
		public CodeManagerTable CodeManagerTable
		{
			get { return this._codeManagerTable ?? (this._codeManagerTable = new CodeManagerTable(this)); }
		}

		/// <summary>Export Address table jumps directory</summary>
		public Eat Eat
		{
			get { return this._eat ?? (this._eat = new Eat(this)); }
		}

		/// <summary>Managed header</summary>
		public ManagedNativeHeader ManagedNativeHeader
		{
			get { return this._managedNativeHeader ?? (this._managedNativeHeader = new ManagedNativeHeader(this)); }
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