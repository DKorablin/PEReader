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
		/// <param name="type">Strongly typed table type</param>
		internal BaseMetaTable(StreamTables stream, Cor.MetaTableType type)
		{
			_ = stream ?? throw new ArgumentNullException(nameof(stream));

			this.TableType = type;
			this.Table = stream[type] ?? throw new ArgumentException($"Table type {type} not found in the stream", "stream[this.TableType]");
		}

		/// <summary>Search for row</summary>
		/// <param name="predicate">Search conditions</param>
		/// <remarks>Remove this after the end of support .NET Framework 2.0</remarks>
		/// <returns>Found row or exception</returns>
		/// <exception cref="ArgumentException">Row that meets exact requirements is not found</exception>
		public R Single(Predicate<R> predicate)
		{
			foreach(var row in this)
				if(predicate(row))
					return row;

			throw new ArgumentException($"Row {typeof(R)} not found");
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