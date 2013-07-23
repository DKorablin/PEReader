using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Базовый класс для детального описания ряда таблицы в метаданных</summary>
	public class BaseMetaRow
	{
		private MetaRow _row;

		/// <summary>Базовый ряд для обработки</summary>
		internal MetaRow Row
		{
			get { return this._row; }
			set
			{
				if(value == null)
					throw new ArgumentNullException("value");
				this._row = value;
			}
		}
		/// <summary>Получить значение из колонки ряда по значению</summary>
		/// <typeparam name="T">Тип данных в колонке</typeparam>
		/// <param name="columnIndex">Индекс колонки в таблице метаданных</param>
		/// <returns>Значение колонки в таблице метаданных</returns>
		protected T GetValue<T>(UInt32 columnIndex)
		{
			return (T)this.Row[columnIndex].Value;
		}
		/// <summary>Проверка на наличие бита дабы уменьшить синтаксис</summary>
		/// <param name="flags">Флаги</param>
		/// <param name="enumValue">Значение бита, которое надо проверить</param>
		/// <returns>Бит проставлен</returns>
		protected static Boolean IsBitSet(UInt32 flags, UInt32 enumValue)
		{
			return (flags & enumValue) == enumValue;
		}
		/// <summary>Отобразить текущий объект ввиде строки</summary>
		/// <param name="args">Ключевой объект от наследуемого класса</param>
		/// <returns>Строка</returns>
		protected internal String ToString(Object args)
		{
			return String.Format("{0} : {{{1}}}", this.GetType().Name, args);
		}
	}
}