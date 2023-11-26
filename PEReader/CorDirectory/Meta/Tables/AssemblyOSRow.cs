using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// These records should not be emitted into any PE file.
	/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
	/// </summary>
	public class AssemblyOSRow : BaseMetaRow
	{
		/// <summary>A 4-byte constant</summary>
		public UInt32 OSPlatformId => base.GetValue<UInt32>(0);

		/// <summary>Operating system major version</summary>
		public UInt32 OSMajorVersion => base.GetValue<UInt32>(1);

		/// <summary>Operating system minor version</summary>
		public UInt32 OSMinorVersion => base.GetValue<UInt32>(2);

		/// <summary>Operating system version</summary>
		public Version OSVersion => new Version((Int32)this.OSMajorVersion, (Int32)this.OSMinorVersion);

		/// <summary>Create instance of AssemblyOSRow</summary>
		public AssemblyOSRow()
			: base(Cor.MetaTableType.AssemblyOS) { }

		/// <summary>OSVersion</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> base.ToString(this.OSVersion);
	}
}