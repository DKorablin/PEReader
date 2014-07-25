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
		public UInt32 Processor { get { return base.GetValue<UInt32>(0); } }
		/// <summary>an index into the AssemblyRef table</summary>
		internal MetaCellPointer AssemblyRefI { get { return base.GetValue<MetaCellPointer>(1); } }

		/// <summary>Pointer into the AssemblyRef table</summary>
		public AssemblyRefRow AssemblyRef
		{//TODO: Не проверено. Пока ещё не нашёл ни одного файла с данными в такой таблице
			get { return new AssemblyRefRow() { Row = this.AssemblyRefI.TargetRow, }; }
		}
	}
}