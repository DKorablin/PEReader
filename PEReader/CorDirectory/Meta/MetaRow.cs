using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable row class</summary>
	[DefaultProperty("Index")]
	public class MetaRow : IEnumerable<MetaCell>, IRow
	{
		#region Fields
		private readonly UInt32 _rowIndex;
		private readonly MetaTable _table;
		private readonly MetaCell[] _cells;
		#endregion Fields

		/// <summary>Row index</summary>
		public UInt32 Index { get { return this._rowIndex; } }

		/// <summary>Parent table</summary>
		public MetaTable Table { get { return this._table; } }
		ITable IRow.Table { get { return this._table; } }

		/// <summary>Get all cells in this row</summary>
		public MetaCell[] Cells { get { return this._cells; } }
		ICell[] IRow.Cells { get { return this._cells; } }

		/// <summary>Get cell by column</summary>
		/// <param name="column">MetaTable column</param>
		/// <exception cref="T:ArgumentException">Column not from this table</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[MetaColumn column]
		{
			get
			{
				if(column.TableType == this.Table.TableType)
					return this[column.Index];
				else
					throw new ArgumentException("This column does not belong to this table");
			}
		}
		ICell IRow.this[IColumn column] { get { return this[(MetaColumn)column]; } }

		/// <summary>Get table cell by column index</summary>
		/// <param name="columnIndex">Column index</param>
		/// <exception cref="T:ArgumentOutOfRangeException">columnIndex out of row columns</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[UInt16 columnIndex]
		{
			get
			{
				if(columnIndex < this._cells.Length)
					return this._cells[columnIndex];
				else
					throw new ArgumentOutOfRangeException(nameof(columnIndex));
			}
		}
		ICell IRow.this[UInt16 columnIndex] { get { return this[columnIndex]; } }

		/// <summary>Gets the table cell by column name</summary>
		/// <param name="columnName">Column name from current table</param>
		/// <exception cref="ArgumentNullException">columnName is empty or null</exception>
		/// <exception cref="ArgumentOutOfRangeException">column with specified name not found</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[String columnName]
		{
			get
			{
				if(String.IsNullOrEmpty(columnName))
					throw new ArgumentNullException(nameof(columnName));

				foreach(MetaColumn column in this.Table.Columns)
					if(column.Name == columnName)
						return this[column.Index];
				throw new ArgumentOutOfRangeException($"Column '{columnName}' not found in current table");
			}
		}
		ICell IRow.this[String columnName] { get { return this[columnName]; } }

		/// <summary>Create instance of MetaTable rows class</summary>
		/// <param name="table">Owner table</param>
		/// <param name="rowIndex">Row index</param>
		/// <param name="cells">Row cells array</param>
		public MetaRow(MetaTable table, UInt32 rowIndex, MetaCell[] cells)
		{
			this._table = table;
			this._rowIndex = rowIndex;
			this._cells = cells;
		}

		/// <summary>Get all cells from current row</summary>
		/// <returns>Cells array</returns>
		public IEnumerator<MetaCell> GetEnumerator()
		{
			foreach(MetaCell cell in this.Cells)
				yield return cell;
		}

		IEnumerator<ICell> IEnumerable<ICell>.GetEnumerator()
		{
			foreach(ICell cell in this.Cells)
				yield return cell;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}