using System;
using System.Collections.Generic;
using System.IO;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Assembly reference reader based on strongly typed metadata <see cref="Cor.MetaTableType.AssemblyRef"/> table</summary>
	public class AssemblyRefReader : IDisposable
	{
		private PEFile _file;
		private String _assemblyRefPath;

		/// <summary>Referenced assembly information</summary>
		public AssemblyRefRow AssemblyRef { get; }

		/// <summary>The link to found referenced assembly on file system</summary>
		public PEFile File
		{
			get
			{
				if(this._file == null && this.AssemblyRefPath != null)
					this._file = this.Open(this.AssemblyRefPath);
				return this._file;
			}
		}

		/// <summary>Path to the current assembly where dependent assemblies will be searched</summary>
		public String SourceAssemblyPath
		{
			get => this.AssemblyRef.Row.Table.Root.Parent.Parent.Parent.Source;
		}

		/// <summary>Trying to find referenced assembly at current path</summary>
		/// <returns>Path to referenced assembly or null if assembly not found</returns>
		public String AssemblyRefPath
		{
			get
			{
				if(this._assemblyRefPath == null)
				{
					String source = this.SourceAssemblyPath;
					if(System.IO.File.Exists(source))
					{
						String rootFolder = Path.GetDirectoryName(source);
						IEnumerable<String> probing = Directory.GetFiles(rootFolder, this.AssemblyRef.AssemblyName.Name + ".dll", SearchOption.AllDirectories);
						foreach(String filePath in probing)
						{
							using(PEFile file = this.Open(filePath))
								if(file.Header.IsValid
									&& file.ComDescriptor?.IsEmpty == false
									&& file.ComDescriptor.MetaData.StreamTables.Assembly.Table.RowsCount > 0)
								{
									AssemblyRow row = file.ComDescriptor.MetaData.StreamTables.Assembly[0];
									if(this.AssemblyRef.AssemblyName.FullName == row.AssemblyName.FullName)
									{
										this._assemblyRefPath = filePath;
										break;
									}
								}
						}
					}//TODO: Add Fusion.dll to search in GAC or shared assemblies folders if available
				}

				return this._assemblyRefPath;
			}
		}

		/// <summary>Create instance of <see cref="AssemblyRefReader"/></summary>
		/// <param name="assemblyRef">Strongly typed metadata <see cref="Cor.MetaTableType.AssemblyRef"/></param>
		/// <exception cref="ArgumentNullException"><paramref name="assemblyRef"/> is required</exception>
		public AssemblyRefReader(AssemblyRefRow assemblyRef)
			=> this.AssemblyRef = assemblyRef ?? throw new ArgumentNullException(nameof(assemblyRef));

		/// <summary>Gets list of all typed members referenced from referenced assembly</summary>
		/// <returns></returns>
		public IEnumerable<TypeRefReader> GetReferencedMembers()
		{
			foreach(TypeRefRow row in this.AssemblyRef.Row.Table.Root.TypeRef)
				if(row.ResolutionScope.TargetRow == this.AssemblyRef)
					yield return new TypeRefReader(row);
		}

		/// <summary>Opens referenced assembly by specifying exact path to PR file that should be opened</summary>
		/// <param name="filePath">Path to PE file to open</param>
		/// <returns>Opened PE file</returns>
		protected virtual PEFile Open(String filePath)
			=> new PEFile(filePath, StreamLoader.FromFile(filePath));

		/// <summary>Destructor for <see cref="AssemblyRefReader"/> if user forgets to dispose this object</summary>
		~AssemblyRefReader()
			=> this.Dispose(false);

		/// <summary><see cref="IDisposable"/> pattern implementation to close referenced assembly file</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary><see cref="IDisposable"/> pattern implementation</summary>
		/// <param name="disposing">Is object disposing or called from destructor</param>
		protected virtual void Dispose(Boolean disposing)
			=> this._file?.Dispose();
	}
}