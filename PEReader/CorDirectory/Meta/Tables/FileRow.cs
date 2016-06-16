using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The rows of the File table result from .file directives in an Assembly (§II.6.2.3).</summary>
	public class FileRow : BaseMetaRow
	{
		/// <summary>A 4-byte bitmask of type FileAttributes, §II.23.1.6</summary>
		public CorFileFlags Flags { get { return (CorFileFlags)base.GetValue<UInt32>(0); } }

		/// <summary>File name with name and extension.</summary>
		public String Name { get { return base.GetValue<String>(1); } }

		/// <summary>Banana</summary>
		public Byte[] HashValue { get { return base.GetValue<Byte[]>(2); } }
	}
}