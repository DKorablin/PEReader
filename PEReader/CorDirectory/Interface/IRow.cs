using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Generic row for variable structure collection</summary>
	public interface IRow : IEnumerable<ICell>
	{
		/// <summary>Transparent row index</summary>
		UInt32 Index { get; }

		/// <summary>Row cells</summary>
		ICell[] Cells { get; }

		/// <summary>Row owner table</summary>
		ITable Table { get; }

		/// <summary>Get the cell by column index</summary>
		/// <param name="columnIndex">Column index</param>
		/// <returns>Cell in specified column index</returns>
		ICell this[UInt16 columnIndex] { get; }

		/// <summary>Get the cell by column name</summary>
		/// <param name="columnName">Column name</param>
		/// <returns>Cell in specified column name</returns>
		ICell this[String columnName] { get; }

		/// <summary>Get the cell by column name</summary>
		/// <param name="column">Column</param>
		/// <returns>Cell in specified column name</returns>
		ICell this[IColumn column] { get; }
	}
}