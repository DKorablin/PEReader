using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// These records should not be emitted into any PE file.
	/// However, if present in a PE file, they should be treated as-if their fields were zero.
	/// They should be ignored by the CLI.
	/// </summary>
	public class AssemblyRefOSRow : BaseMetaRow
	{
		/// <summary>A 4-byte constant</summary>
		public UInt32 OSPlatformId { get { return base.GetValue<UInt32>(0); } }

		/// <summary>Operating system major version.</summary>
		public UInt32 OSMajorVersion { get { return base.GetValue<UInt32>(1); } }

		/// <summary>Operating system minor version.</summary>
		public UInt32 OSMinorVersion { get { return base.GetValue<UInt32>(2); } }

		/// <summary>An index into the AssemblyRef table.</summary>
		internal MetaCellPointer AssemblyRefI { get { return base.GetValue<MetaCellPointer>(3); } }

		/// <summary>Operating system version.</summary>
		public Version OSVersion { get { return new Version((Int32)this.OSMajorVersion, (Int32)this.OSMinorVersion); } }

		/// <summary>AssemblyRef table row.</summary>
		public AssemblyRefRow AssemblyRef
		{//TODO: Не проверено. Пока ещё не нашёл ни одного файла с данными в такой таблице
			get { return new AssemblyRefRow() { Row = this.AssemblyRefI.TargetRow, }; }
		}
	}
}