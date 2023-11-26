using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// These records should not be emitted into any PE file.
	/// However, if present in a PE file, they should be treated as-if their fields were zero.
	/// They should be ignored by the CLI.
	/// </summary>
	public class AssemblyRefProcessorRow : BaseMetaRow
	{
		/// <summary>a 4-byte constant</summary>
		public UInt32 Processor => base.GetValue<UInt32>(0);

		/// <summary>an index into the AssemblyRef table</summary>
		internal MetaCellPointer AssemblyRefI => base.GetValue<MetaCellPointer>(1);

		/// <summary>Pointer into the AssemblyRef table</summary>
		public AssemblyRefRow AssemblyRef//TODO: Not tested. I don't find any file with data in this table
			=> new AssemblyRefRow() { Row = this.AssemblyRefI.TargetRow, };

		/// <summary>Create instance of AssemblyRefProcessor row</summary>
		public AssemblyRefProcessorRow()
			: base(Cor.MetaTableType.AssemblyRefProcessor) { }
	}
}