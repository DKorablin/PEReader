using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Basic class to describe any metadata table</summary>
	public class BaseMetaTable<R> : IEnumerable<R> where R : BaseMetaRow, new()
	{
		/// <summary>Metadata table</summary>
		public MetaTable Table { get; }

		/// <summary>Strongly typed metadata table</summary>
		public Cor.MetaTableType TableType { get; }

		/// <summary>Get detailed row from metadata table by index</summary>
		/// <param name="rowIndex">Row index from metadata table</param>
		/// <returns>Strongly typed detailed row from metadata</returns>
		public R this[UInt32 rowIndex] => new R() { Row = this.Table[rowIndex], };

		/// <summary>Creating an instance of the base class of a detailed description of a table in metadata</summary>
		/// <param name="stream">Metadata stream</param>
		/// <param name="type">Stronglt typed table type</param>
		internal BaseMetaTable(StreamTables stream, Cor.MetaTableType type)
		{
			_ = stream ?? throw new ArgumentNullException(nameof(stream));

			this.TableType = type;
			this.Table = stream[this.TableType];
		}

		/// <summary>Get in iteration a list of all rows in a metadata table</summary>
		/// <returns>A set of metadata detailing the structure of a table</returns>
		public IEnumerator<R> GetEnumerator()
		{
			foreach(var row in this.Table.Rows)
				yield return new R() { Row = row, };
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}