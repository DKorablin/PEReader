using System;
using AlphaOmega.Debug;
using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug
{
	/// <summary>NT Directory</summary>
	public class PEDirectory : IDisposable
	{//http://code.cheesydesign.com/?p=572 - .NET PE Reader

		#region Fields
		private PEHeader _header;

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
		/// <summary>PE/PE+ Headers</summary>
		public PEHeader Header { get { return this._header; } }

		/// <summary>Get directory from optional header</summary>
		/// <param name="entry">Directory entry type</param>
		/// <returns>Directory</returns>
		public WinNT.IMAGE_DATA_DIRECTORY this[WinNT.IMAGE_DIRECTORY_ENTRY entry]
		{
			get
			{
				if(this.Header.Is64Bit)
					return this.Header.HeaderNT64.OptionalHeader[entry];
				else
					return this.Header.HeaderNT32.OptionalHeader[entry];
			}
		}
		/// <summary>Architecture directory</summary>
		public Architecture Architecture
		{
			get
			{
				if(this._architecture == null)
					this._architecture = new Architecture(this);
				return this._architecture;
			}
		}
		/// <summary>Получить информацию о экспортируемых функциях в PE файле</summary>
		public Export Export
		{
			get
			{
				if(this._export == null)
					this._export = new Export(this);
				return this._export;
			}
		}
		/// <summary>Получить информацию о ипортируемых функциях PE файлом</summary>
		public Import Import
		{
			get
			{
				if(this._import == null)
					this._import = new Import(this);
				return this._import;
			}
		}
		/// <summary>Получить информацию о зашитой информации для дебаггера</summary>
		/// <returns>Информация для дебаггера</returns>
		public NTDirectory.Debug Debug
		{
			get
			{
				if(this._debug == null)
					this._debug = new NTDirectory.Debug(this);
				return this._debug;
			}
		}
		/// <summary>Получить информацию по ресурсам</summary>
		public Resource Resource
		{
			get
			{
				if(this._resource == null)
					this._resource = new Resource(this);
				return this._resource;
			}
		}
		/// <summary>.NET Directory</summary>
		public ComDescriptor ComDescriptor
		{
			get
			{
				if(this._comDescriptor == null)
					this._comDescriptor = new ComDescriptor(this);

				if(this._comDescriptor.IsEmpty)
					return null;
				else
					return this._comDescriptor;
			}
		}
		/// <summary>Load config directory</summary>
		public LoadConfig LoadConfig
		{
			get
			{
				if(this._loadConfig == null)
					this._loadConfig = new LoadConfig(this);
				return this._loadConfig;
			}
		}
		/// <summary>Certificate directory</summary>
		public Security Certificate
		{
			get
			{
				if(this._certificate == null)
					this._certificate = new Security(this);
				return this._certificate;
			}
		}
		/// <summary>Boundimport directory</summary>
		public BoundImport BoundImport
		{
			get
			{
				if(this._boundImport == null)
					this._boundImport = new BoundImport(this);
				return this._boundImport;
			}
		}
		/// <summary>Relocation directory</summary>
		public Relocation Relocations
		{
			get
			{
				if(this._relocations == null)
					this._relocations = new Relocation(this);
				return this._relocations;
			}
		}
		/// <summary>Delay import modules directory</summary>
		public DelayImport DelayImport
		{
			get
			{
				if(this._delayImport == null)
					this._delayImport = new DelayImport(this);
				return this._delayImport;
			}
		}
		/// <summary>Global pointer directory</summary>
		public GlobalPtr GlobalPtr
		{
			get
			{
				if(this._globalPtr == null)
					this._globalPtr = new GlobalPtr(this);
				return this._globalPtr;
			}
		}
		/// <summary>Exception table directory</summary>
		public ExceptionTable ExceptionTable
		{
			get
			{
				if(this._exceptionTable == null)
					this._exceptionTable = new ExceptionTable(this);
				return this._exceptionTable;
			}
		}
		/// <summary>Import Address Table directory</summary>
		public Iat Iat
		{
			get
			{
				if(this._iat == null)
					this._iat = new Iat(this);
				return this._iat;
			}
		}
		/// <summary>Thread local storage directory</summary>
		public Tls Tls
		{
			get
			{
				if(this._tls == null)
					this._tls = new Tls(this);
				return this._tls;
			}
		}
		#endregion Properties
		/// <summary>Создание экземпляра класса с указанием пути к PE файлу</summary>
		/// <param name="loader">PE file loader</param>
		public PEDirectory(IImageLoader loader)
		{
			this._header = new PEHeader(loader);
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
			if(disposing && this._header != null)
			{
				this._header.Dispose();
				this._header = null;
			}
		}
	}
}