using System;
using AlphaOmega.Debug.NTDirectory;
using AlphaOmega.Debug.PESection;

namespace AlphaOmega.Debug
{
	/// <summary>PE/PE+ file description</summary>
	public class PEFile : IDisposable
	{
		#region Fields
		private Sections _sections;

		private Architecture _architecture;
		private Export _export;
		private Import _import;
		private NTDirectory.Debug _debug;
		private Resource _resource;
		private ComDescriptor _comDescriptor;
		private LoadConfig _loadConfig;
		private Security _certificate;
		private BoundImport _boundImport;
		private Relocation _relocations;
		private DelayImport _delayImport;
		private GlobalPtr _globalPtr;
		private ExceptionTable _exceptionTable;
		private Iat _iat;
		private Tls _tls;
		#endregion Fields
		#region Properties
		/// <summary>File source</summary>
		public String Source { get; }

		/// <summary>PE/PE+ Headers</summary>
		public PEHeader Header { get; private set; }

		/// <summary>Get directory from optional header</summary>
		/// <param name="entry">Directory entry type</param>
		/// <returns>Directory</returns>
		public WinNT.IMAGE_DATA_DIRECTORY this[WinNT.IMAGE_DIRECTORY_ENTRY entry]
			=> this.Header.Is64Bit
					? this.Header.HeaderNT64.OptionalHeader[entry]
					: this.Header.HeaderNT32.OptionalHeader[entry];

		/// <summary>PE file sections</summary>
		public Sections Sections
			=> this._sections ?? (this._sections = new Sections(this));

		/// <summary>Architecture directory</summary>
		public Architecture Architecture
			=> this._architecture ?? (this._architecture = new Architecture(this));

		/// <summary>Get all exported functions from PE file</summary>
		/// <remarks>Export directory</remarks>
		public Export Export
			=> this._export ?? (this._export = new Export(this));

		/// <summary>Get all Imported functions from PE file</summary>
		/// <returns>Import directory</returns>
		public Import Import
			=> this._import ?? (this._import = new Import(this));

		/// <summary>Get detailed information for debugginig</summary>
		/// <returns>Debugging directory</returns>
		public NTDirectory.Debug Debug
			=> this._debug ?? (this._debug = new NTDirectory.Debug(this));

		/// <summary>Get resources from PE file</summary>
		/// <returns>Resource directory</returns>
		public Resource Resource
			=> this._resource ?? (this._resource = new Resource(this));

		/// <summary>.NET Directory</summary>
		public ComDescriptor ComDescriptor
		{
			get
			{
				if(this._comDescriptor == null)
					this._comDescriptor = new ComDescriptor(this);

				return this._comDescriptor.IsEmpty
					? null
					: this._comDescriptor;
			}
		}

		/// <summary>Load config directory</summary>
		public LoadConfig LoadConfig
			=> this._loadConfig ?? (this._loadConfig = new LoadConfig(this));

		/// <summary>Certificate directory</summary>
		public Security Certificate
			=> this._certificate ?? (this._certificate = new Security(this));

		/// <summary>Boundimport directory</summary>
		public BoundImport BoundImport
			=> this._boundImport ?? (this._boundImport = new BoundImport(this));

		/// <summary>Relocation directory</summary>
		public Relocation Relocations
			=> this._relocations ?? (this._relocations = new Relocation(this));

		/// <summary>Delay import modules directory</summary>
		public DelayImport DelayImport
			=> this._delayImport ?? (this._delayImport = new DelayImport(this));

		/// <summary>Global pointer directory</summary>
		public GlobalPtr GlobalPtr
			=> this._globalPtr ?? (this._globalPtr = new GlobalPtr(this));

		/// <summary>Exception table directory</summary>
		public ExceptionTable ExceptionTable
			=> this._exceptionTable ?? (this._exceptionTable = new ExceptionTable(this));

		/// <summary>Import Address Table directory</summary>
		public Iat Iat
			=> this._iat ?? (this._iat = new Iat(this));

		/// <summary>Thread local storage directory</summary>
		public Tls Tls
			=> this._tls ?? (this._tls = new Tls(this));
		#endregion Properties

		/// <summary>Create instance and specify image loader instance</summary>
		/// <param name="source">Source of the module</param>
		/// <param name="loader">PE file loader</param>
		public PEFile(String source, IImageLoader loader)
		{
			this.Source = source ?? throw new ArgumentNullException(nameof(source));
			this.Header = new PEHeader(loader);
		}

		/// <summary>Close loader</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Dispose managed objects</summary>
		/// <param name="disposing">Dispose managed objects</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this.Header != null)
			{
				this.Header.Dispose();
				this.Header = null;
			}
		}
	}
}