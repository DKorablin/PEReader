using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Generic structure collection</summary>
	public interface ITable
	{
		/// <summary>All rows from the table</summary>
		IEnumerable<IRow> Rows { get; }

		/// <summary>Rows count in the table</summary>
		UInt32 RowsCount { get; }

		/// <summary>Columns from current table</summary>
		IColumn[] Columns { get; }

		/// <summary>Table type</summary>
		Object Type { get; }

		/// <summary>Gets specified row by index and check that row from current table</summary>
		/// <param name="rowIndex">Row index</param>
		/// <returns>Row by index from current table</returns>
		IRow this[UInt32 rowIndex] { get; }
	}
}