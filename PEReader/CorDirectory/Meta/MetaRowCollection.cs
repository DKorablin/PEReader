using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable rows collection</summary>
	public class MetaRowCollection : IEnumerable<MetaRow>
	{
		private readonly MetaTable _table;
		private MetaTable Table { get { return this._table; } }

		/// <summary>Create instance of rows collection class</summary>
		/// <param name="table">Table from whitch taken all rows</param>
		public MetaRowCollection(MetaTable table)
		{
			this._table = table;
		}

		/// <summary>Get all rows from table</summary>
		/// <returns>Rows collection</returns>
		public IEnumerator<MetaRow> GetEnumerator()
		{
			for(UInt32 rowIndex = 0; rowIndex < this.Table.RowsCount; rowIndex++)
				yield return this.Table[rowIndex];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}