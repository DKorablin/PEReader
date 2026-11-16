using System;
using System.Collections.Generic;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Assembly reference reader based on strongly typed metadata <see cref="Cor.MetaTableType.AssemblyRef"/> table</summary>
	public class AssemblyRefReader
	{
		/// <summary>Referenced assembly information</summary>
		public AssemblyRefRow AssemblyRef { get; }

		/// <summary>File that contains the assembly reference</summary>
		private PEFile Owner => this.AssemblyRef.Row.Table.Root.Parent.Parent.Parent;

		/// <summary>Referenced assembly found by <see cref="PEFile.AssemblyResolver"/></summary>
		/// <remarks>The file is owned by the resolver and must not be disposed by the caller</remarks>
		/// <returns>Referenced assembly or null if the assembly is not found or <see cref="PEReaderOptions.ResolveAssemblies"/> is disabled</returns>
		public PEFile File
		{
			get
			{
				PEFile owner = this.Owner;
				return owner.AssemblyResolver?.Resolve(this.AssemblyRef.AssemblyName, owner);
			}
		}

		/// <summary>Path to the current assembly where dependent assemblies will be searched</summary>
		public String SourceAssemblyPath => this.Owner.Source;

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
	}
}
