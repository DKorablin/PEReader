using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable rows collection</summary>
	public class MetaRowCollection : IEnumerable<MetaRow>
	{
		private MetaTable Table { get; }

		/// <summary>Create instance of rows collection class</summary>
		/// <param name="table">Table from which taken all rows</param>
		public MetaRowCollection(MetaTable table)
			=> this.Table = table ?? throw new ArgumentNullException(nameof(table));

		/// <summary>Get all rows from table</summary>
		/// <returns>Rows collection</returns>
		public IEnumerator<MetaRow> GetEnumerator()
		{
			for(UInt32 rowIndex = 0; rowIndex < this.Table.RowsCount; rowIndex++)
				yield return this.Table[rowIndex];
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}