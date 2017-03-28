using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Шрифт</summary>
	public struct DialogFont
	{
		/// <summary>Размер шрифта</summary>
		public UInt16 FontSize;
		/// <summary>Толщина шрифта</summary>
		public UInt16 FontWeight;
		/// <summary>Наклонный</summary>
		public Byte? Italic;
		/// <summary>Character Set</summary>
		public Byte? CharSet;
		/// <summary>Наименование шрифта</summary>
		public String TypeFace;
	}
}