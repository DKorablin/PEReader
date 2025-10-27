using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Dialog font description</summary>
	public struct DialogFont
	{
		/// <summary>Font size</summary>
		public UInt16 FontSize;

		/// <summary>Font weight</summary>
		public UInt16 FontWeight;

		/// <summary>Italic font</summary>
		public Byte? Italic;

		/// <summary>Character Set</summary>
		public Byte? CharSet;

		/// <summary>Font name</summary>
		public String TypeFace;
	}
}