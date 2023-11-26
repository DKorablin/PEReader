using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable row class</summary>
	[DefaultProperty(nameof(Index))]
	public class MetaRow : IEnumerable<MetaCell>, IEquatable<MetaRow>, IRow
	{
		/// <summary>Row index</summary>
		public UInt32 Index { get; }

		/// <summary>Parent table</summary>
		public MetaTable Table { get; }
		ITable IRow.Table => this.Table;

		/// <summary>Get all cells in this row</summary>
		public MetaCell[] Cells { get; }
		ICell[] IRow.Cells => this.Cells;

		/// <summary>Get cell by column</summary>
		/// <param name="column">MetaTable column</param>
		/// <exception cref="ArgumentException">Column not from this table</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[MetaColumn column]
			=> column.TableType == this.Table.TableType
				? this[column.Index]
				: null;

		ICell IRow.this[IColumn column] => this[(MetaColumn)column];

		/// <summary>Get table cell by column index</summary>
		/// <param name="columnIndex">Column index</param>
		/// <exception cref="ArgumentOutOfRangeException">The column index is out of the range of available columns in the row</exception>
		/// <returns>Table cell</returns>
		public MetaCell this[UInt16 columnIndex]
			=> columnIndex < this.Cells.Length
				? this.Cells[columnIndex]
				: throw new ArgumentOutOfRangeException(nameof(columnIndex));

		ICell IRow.this[UInt16 columnIndex] => this[columnIndex];

		/// <summary>Gets the table cell by column name</summary>
		/// <param name="columnName">The column name from current table columns</param>
		/// <exception cref="ArgumentNullException">A column with the specified name <c>columnname</c> is empty or null</exception>
		/// <exception cref="ArgumentOutOfRangeException">A column with the specified name <c>columnName</c> was not found</exception>
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
				throw new ArgumentOutOfRangeException(nameof(columnName), $"Column '{columnName}' not found in current table");
			}
		}

		ICell IRow.this[String columnName] => this[columnName];

		/// <summary>Create instance of MetaTable rows class</summary>
		/// <param name="table">Owner table</param>
		/// <param name="rowIndex">Row index</param>
		/// <param name="cells">Row cells array</param>
		public MetaRow(MetaTable table, UInt32 rowIndex, MetaCell[] cells)
		{
			this.Table = table ?? throw new ArgumentNullException(nameof(table));
			this.Index = rowIndex;
			this.Cells = cells ?? throw new ArgumentNullException(nameof(table));
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
			=> this.GetEnumerator();

		/// <summary>Compare two rows by table type and index fields</summary>
		/// <param name="obj">Object to compare with current field</param>
		/// <returns>Objects are equals</returns>
		public override Boolean Equals(Object obj)
			=> Equals(obj as MetaRow);

		/// <summary>Compare two rows by table type and index fields</summary>
		/// <param name="row">Row to compare with current row</param>
		/// <returns>Rows are equals</returns>
		public Boolean Equals(MetaRow row)
		{
			if(ReferenceEquals(row, null))
				return false;
			if(ReferenceEquals(this, row))
				return true;

			return this.Index == row.Index
				&& this.Table.TableType == row.Table.TableType;
		}

		/// <summary>Gets unique identifier for current row in current table by combining tableType and row index</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
			=> (Int32)this.Table.TableType.GetHashCode() ^ (Int32)this.Index;

		/// <summary>Compare two rows by table type and index field</summary>
		/// <param name="a">First row to compare</param>
		/// <param name="b">Second row to compare</param>
		/// <returns>Rows are equals</returns>
		public static Boolean operator ==(MetaRow a, MetaRow b)
		{
			if(ReferenceEquals(a, b))
				return true;
			if(ReferenceEquals(a, null))
				return false;
			if(ReferenceEquals(b, null))
				return false;
			return a.Index == b.Index;
		}

		/// <summary>Compare two rows by table type and index field</summary>
		/// <param name="a">First row to compare</param>
		/// <param name="b">Second row to compare</param>
		/// <returns>Rows are NOT equals</returns>
		public static Boolean operator !=(MetaRow a, MetaRow b)
			=> !(a == b);
	}
}