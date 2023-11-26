using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Pointer to anoter table cell</summary>
	public class MetaCellPointer : CellPointerBase
	{
		/// <summary>Create instance of pointer cell</summary>
		/// <param name="cell">Source cell</param>
		/// <param name="rawValue">Original value from PE file</param>
		internal MetaCellPointer(MetaCell cell, UInt32 rawValue)
			: base(cell, rawValue, (Cor.MetaTableType)cell.Column.ColumnType) { }

		/// <summary>Получить в итерации ряды с индекса на который указывает текущий указатель</summary>
		/// <remarks>Метод специфичен для определённых таблиц. Поэтому переносить в базовый класс - бессмысленно</remarks>
		/// <returns>Target rows array</returns>
		internal IEnumerable<MetaRow> GetTargetRowsIt()
		{
			if(base.RowIndex.HasValue)
			{
				MetaTable targetTable = base.TargetTable;
				UInt32 index = base.RowIndex.Value;
				UInt32 rowIndex = base.Cell.RowIndex;

				UInt32? nextIndex = base.Cell.Table.RowsCount == rowIndex + 1 ?
					(UInt32?)null :
					((MetaCellPointer)base.Cell.Table[rowIndex + 1][base.Cell.Column].Value).RowIndex;

				while(index < targetTable.RowsCount)
					if(nextIndex == index)
						yield break;
					else
						yield return targetTable[index++];
			}
		}
	}
}