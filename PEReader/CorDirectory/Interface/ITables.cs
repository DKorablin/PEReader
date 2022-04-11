using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Generic tables collection</summary>
	public interface ITables : IEnumerable<ITable>
	{
		/// <summary>Gets the table by the table type value</summary>
		/// <param name="type">Table type value</param>
		/// <returns>table with structures by table type</returns>
		ITable this[Object type] { get; }

		/// <summary>Total tables count (must be fixed size)</summary>
		UInt32 Count { get; }

		/// <summary>Gets row by the transparent index</summary>
		/// <param name="rowIndex">Row index</param>
		/// <returns>Found row or exception</returns>
		IRow GetRowByIndex(UInt32 rowIndex);
	}
}