using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// These records should not be emitted into any PE file.
	/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
	/// </summary>
	public class AssemblyProcessorRow : BaseMetaRow
	{
		/// <summary>Banana</summary>
		public UInt32 Processor => base.GetValue<UInt32>(0);

		/// <summary>Create instance of AssemblyProcessor row</summary>
		public AssemblyProcessorRow()
			: base(Cor.MetaTableType.AssemblyProcessor) { }
	}
}