using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Contains values that describe the type of file defined in a call to IMetaDataAssemblyEmit::DefineFile.</summary>
	/// <remarks>http://msdn.microsoft.com/en-us/library/ms232970.aspx</remarks>
	[Flags]
	public enum CorFileFlags
	{
		/// <summary>Indicates that the file is not a resource file.</summary>
		ffContainsMetaData = 0x0000,
		/// <summary>Indicates that the file, possibly a resource file, does not contain metadata.</summary>
		ffContainsNoMetaData = 0x0001,
	}
}