using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Базовый класс для детализированного описания таблицы в метаданных</summary>
	public class BaseMetaTable<T> : IEnumerable<T> where T : BaseMetaRow, new()
	{
		#region Fields
		private readonly MetaTable _table;
		private readonly Cor.MetaTableType _tableType;
		#endregion Fields

		/// <summary>Таблица в метаданных</summary>
		public MetaTable Table { get { return this._table; } }

		/// <summary>Тип таблицы из метаданных</summary>
		public Cor.MetaTableType TableType { get { return this._tableType; } }

		/// <summary>Получить детализированный ряд из таблицы метаданных</summary>
		/// <param name="rowIndex">Индекс ряда из таблицы</param>
		/// <returns>Детализированный ряд таблицы из метаданных</returns>
		public T this[UInt32 rowIndex] { get { return new T() { Row = this.Table[rowIndex], }; } }

		/// <summary>Создание экземпляра базового класса детализированного описания таблицы в метаданных</summary>
		/// <param name="stream">Поток</param>
		/// <param name="type">Тип таблицы</param>
		internal BaseMetaTable(StreamTables stream, Cor.MetaTableType type)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");

			this._tableType = type;
			this._table = stream[this._tableType];
		}

		/// <summary>Получить в итерации список всех рядов в таблице метаданных</summary>
		/// <returns>Ряд метаданных детально описывающий структуру таблицы</returns>
		public IEnumerator<T> GetEnumerator()
		{
			foreach(var row in this.Table.Rows)
				yield return new T() { Row = row, };
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}