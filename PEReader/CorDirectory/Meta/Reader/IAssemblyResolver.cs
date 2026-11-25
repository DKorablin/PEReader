using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Resolves assemblies referenced from a <see cref="PEFile"/></summary>
	/// <remarks>Implement this interface to plug in a custom lookup (GAC, package cache, in-memory images, etc.) and pass it via <see cref="PEReaderOptions.AssemblyResolver"/></remarks>
	public interface IAssemblyResolver
	{
		/// <summary>Finds and opens the referenced assembly</summary>
		/// <param name="reference">Name of the referenced assembly</param>
		/// <param name="requestingFile">File that references the assembly (used as the probing base)</param>
		/// <returns>Opened assembly or <c>null</c> if the assembly can't be found. The returned file is owned by the resolver: callers must not dispose it.</returns>
		PEFile Resolve(AssemblyName reference, PEFile requestingFile);
	}
}
