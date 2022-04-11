using System;

namespace AlphaOmega.Debug
{
	/// <summary>Basic row for the strongly typed generic row</summary>
	public interface IBaseRow
	{
		/// <summary>Generic interface row</summary>
		IRow Row { get; }
	}
}