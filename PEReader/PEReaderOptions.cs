using System;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;

namespace AlphaOmega.Debug
{
	/// <summary>Options that control how a <see cref="PEFile"/> is read</summary>
	/// <remarks>Values are copied by <see cref="PEFile"/> constructor, so changing a shared options object later does not affect already opened files</remarks>
	public class PEReaderOptions
	{
		/// <summary>Resolve referenced assemblies when external metadata is required</summary>
		/// <remarks>
		/// Used to get underlying types of enums declared in other assemblies (custom attribute values),
		/// <see cref="TypeRefReader.TypeDef"/> and <see cref="AssemblyRefReader.File"/>.
		/// When disabled no dependent files are opened and <see cref="AssemblyResolver"/> is ignored.
		/// Default: <c>false</c>
		/// </remarks>
		public Boolean ResolveAssemblies { get; set; }

		/// <summary>Custom assembly resolver</summary>
		/// <remarks>
		/// If <c>null</c> and <see cref="ResolveAssemblies"/> is enabled, the built-in <see cref="CorDirectory.Meta.Reader.AssemblyResolver"/> is created and owned by the <see cref="PEFile"/>.
		/// A custom resolver is owned by the caller and is never disposed by the <see cref="PEFile"/>.
		/// </remarks>
		public IAssemblyResolver AssemblyResolver { get; set; }
	}
}
