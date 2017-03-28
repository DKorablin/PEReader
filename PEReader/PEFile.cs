using System;
using AlphaOmega.Debug;
using AlphaOmega.Debug.NTDirectory;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>PE/PE+ file description</summary>
	public class PEFile : IDisposable
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
				return this.Header.Is64Bit
					? this.Header.HeaderNT64.OptionalHeader[entry]
					: this.Header.HeaderNT32.OptionalHeader[entry];
			}
		}

		/// <summary>Architecture directory</summary>
		public Architecture Architecture
		{
			get
			{
				return this._architecture == null
					? this._architecture = new Architecture(this)
					: this._architecture;
			}
		}

		/// <summary>Получить информацию о экспортируемых функциях в PE файле</summary>
		public Export Export
		{
			get
			{
				return this._export == null
					? this._export = new Export(this)
					: this._export;
			}
		}

		/// <summary>Получить информацию о ипортируемых функциях PE файлом</summary>
		public Import Import
		{
			get
			{
				return this._import == null
					? this._import = new Import(this)
					: this._import;
			}
		}

		/// <summary>Получить информацию о зашитой информации для дебаггера</summary>
		/// <returns>Информация для дебаггера</returns>
		public NTDirectory.Debug Debug
		{
			get
			{
				return this._debug == null
					? this._debug = new NTDirectory.Debug(this)
					: this._debug;
			}
		}

		/// <summary>Получить информацию по ресурсам</summary>
		public Resource Resource
		{
			get
			{
				return this._resource == null
					? this._resource = new Resource(this)
					: this._resource;
			}
		}

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
		{
			get
			{
				return this._loadConfig == null
					? this._loadConfig = new LoadConfig(this)
					: this._loadConfig;
			}
		}

		/// <summary>Certificate directory</summary>
		public Security Certificate
		{
			get
			{
				return this._certificate == null
					? this._certificate = new Security(this)
					: this._certificate;
			}
		}

		/// <summary>Boundimport directory</summary>
		public BoundImport BoundImport
		{
			get
			{
				return this._boundImport == null
					? this._boundImport = new BoundImport(this)
					: this._boundImport;
			}
		}

		/// <summary>Relocation directory</summary>
		public Relocation Relocations
		{
			get
			{
				return this._relocations == null
					? this._relocations = new Relocation(this)
					: this._relocations;
			}
		}

		/// <summary>Delay import modules directory</summary>
		public DelayImport DelayImport
		{
			get
			{
				return this._delayImport == null
					? this._delayImport = new DelayImport(this)
					: this._delayImport;
			}
		}

		/// <summary>Global pointer directory</summary>
		public GlobalPtr GlobalPtr
		{
			get
			{
				return this._globalPtr == null
					? this._globalPtr = new GlobalPtr(this)
					: this._globalPtr;
			}
		}

		/// <summary>Exception table directory</summary>
		public ExceptionTable ExceptionTable
		{
			get
			{
				return this._exceptionTable == null
					? this._exceptionTable = new ExceptionTable(this)
					: this._exceptionTable;
			}
		}

		/// <summary>Import Address Table directory</summary>
		public Iat Iat
		{
			get
			{
				return this._iat == null
					? this._iat = new Iat(this)
					: this._iat;
			}
		}
		/// <summary>Thread local storage directory</summary>
		public Tls Tls
		{
			get
			{
				return this._tls == null
					? this._tls = new Tls(this)
					: this._tls;
			}
		}
		#endregion Properties

		/// <summary>Создание экземпляра класса с указанием пути к PE файлу</summary>
		/// <param name="loader">PE file loader</param>
		public PEFile(IImageLoader loader)
		{
			this._header = new PEHeader(loader);
		}

		/// <summary>Получить массив секций с возможностью получения актуальных данных из секции</summary>
		/// <returns>Массив секций обрамлённых обёртками</returns>
		public IEnumerable<NTSection> GetSections()
		{
			foreach(WinNT.IMAGE_SECTION_HEADER header in this.Header.Sections)
				yield return new NTSection(this, header);
		}

		/// <summary>Получить секцию по наименованию секции</summary>
		/// <param name="section">Наименование требуемой секции</param>
		/// <returns>Найденная секция или null</returns>
		public NTSection GetSection(String section)
		{
			foreach(WinNT.IMAGE_SECTION_HEADER header in this.Header.Sections)
				if(header.Section == section)
					return new NTSection(this, header);

			return null;
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