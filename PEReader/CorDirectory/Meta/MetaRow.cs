using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable row class</summary>
	[DefaultProperty("Index")]
	public class MetaRow : IEnumerable<MetaCell>
	{
		#region Fields
		private readonly UInt32 _index;
		private readonly MetaTable _table;
		private readonly MetaCell[] _cells;
		#endregion Fields
		/// <summary>Row index</summary>
		public UInt32 Index { get { return this._index; } }
		/// <summary>Parent table</summary>
		public MetaTable Table { get { return this._table; } }
		/// <summary>Get all cells in this row</summary>
		public MetaCell[] Cells { get { return this._cells; } }
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
		/// <summary>Get table cell by column index</summary>
		/// <param name="columnIndex">Column index</param>
		/// <exception cref="T:ArgumentOutOfRangeException">columnIndex out of row columns</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[UInt32 columnIndex]
		{
			get
			{
				if(columnIndex < this._cells.Length)
					return this._cells[columnIndex];
				else
					throw new ArgumentOutOfRangeException("columnIndex");
			}
		}

		/// <summary>Create instance of MetaTable rows class</summary>
		/// <param name="table">Owner table</param>
		/// <param name="index">Row index</param>
		/// <param name="cells">Row cells array</param>
		public MetaRow(MetaTable table, UInt32 index, MetaCell[] cells)
		{
			this._index = index;
			this._table = table;
			this._cells = cells;
		}
		/// <summary>Get all cells from current row</summary>
		/// <returns>Cells array</returns>
		public IEnumerator<MetaCell> GetEnumerator()
		{
			foreach(MetaCell cell in this.Cells)
				yield return cell;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}