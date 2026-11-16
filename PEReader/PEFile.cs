using System;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;
using AlphaOmega.Debug.NTDirectory;
using AlphaOmega.Debug.PESection;

namespace AlphaOmega.Debug
{
	/// <summary>PE/PE+ file description</summary>
	public class PEFile : IDisposable
	{
		#region Fields
		private Boolean _disposed;
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

		private readonly Boolean _resolveAssemblies;
		private readonly IAssemblyResolver _customResolver;
		private AssemblyResolver _ownedResolver;
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

		/// <summary>Get detailed information for debugging</summary>
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

		/// <summary>Resolver used to read metadata of referenced assemblies</summary>
		/// <remarks>
		/// <c>null</c> when <see cref="PEReaderOptions.ResolveAssemblies"/> is disabled.
		/// Otherwise the custom <see cref="PEReaderOptions.AssemblyResolver"/> or the built-in <see cref="CorDirectory.Meta.Reader.AssemblyResolver"/> owned by this file.
		/// </remarks>
		public IAssemblyResolver AssemblyResolver
		{
			get
			{
				if(!this._resolveAssemblies || this._disposed)
					return null;

				return this._customResolver
					?? this._ownedResolver
					?? (this._ownedResolver = new AssemblyResolver());
			}
		}
		#endregion Properties

		/// <summary>Create instance and specify image loader instance</summary>
		/// <param name="source">Source of the module</param>
		/// <param name="loader">PE file loader</param>
		/// <param name="options">Reading options. Referenced assemblies are not resolved if options are not specified</param>
		public PEFile(String source, IImageLoader loader, PEReaderOptions options = null)
		{
			this.Source = source ?? throw new ArgumentNullException(nameof(source));
			this.Header = new PEHeader(loader);

			if(options != null)
			{
				this._resolveAssemblies = options.ResolveAssemblies;
				this._customResolver = options.AssemblyResolver;
			}
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
			if(this._disposed)
				return;

			if(disposing)
			{//There is no finalizer: loaders protect their own unmanaged resources (file handles, loaded modules) with SafeHandle
				this.Header?.Dispose();
				this.Header = null;

				this._ownedResolver?.Dispose();//Custom resolver is owned by the caller
				this._ownedResolver = null;
			}
			this._disposed = true;
		}
	}
}