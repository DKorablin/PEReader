using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Базовый класс для детального описания ряда таблицы в метаданных</summary>
	public class BaseMetaRow : IEquatable<BaseMetaRow>
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

		/// <summary>Row index</summary>
		public UInt32 Index { get { return this.Row.Index; } }

		/// <summary>Получить значение из колонки ряда по значению</summary>
		/// <typeparam name="T">Тип данных в колонке</typeparam>
		/// <param name="columnIndex">Индекс колонки в таблице метаданных</param>
		/// <returns>Значение колонки в таблице метаданных</returns>
		protected T GetValue<T>(UInt16 columnIndex)
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

		/// <summary>Compare two rows by table type and index fields</summary>
		/// <param name="obj">Object to compare with current field</param>
		/// <returns>Objects are equals</returns>
		public override Boolean Equals(Object obj)
		{
			return Equals(obj as BaseMetaRow);
		}

		/// <summary>Compare two rows by table type and index fields</summary>
		/// <param name="row">Row to compare with current row</param>
		/// <returns>Rows are equals</returns>
		public Boolean Equals(BaseMetaRow row)
		{
			if(ReferenceEquals(row, null))
				return false;
			if(ReferenceEquals(this, row))
				return true;

			return this.Index == row.Index
				&& this.Row.Table.TableType == row.Row.Table.TableType;
		}

		/// <summary>Compare two rows by table type and index field</summary>
		/// <param name="a">First row to compare</param>
		/// <param name="b">Second row to compare</param>
		/// <returns>Rows are equals</returns>
		public static Boolean operator ==(BaseMetaRow a,BaseMetaRow b)
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
		public static Boolean operator !=(BaseMetaRow a,BaseMetaRow b)
		{
			return !(a == b);
		}

		/// <summary>Gets unique identifier for current row in current table</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
		{
			return (Int32)this.Row.Table.TableType.GetHashCode() ^ (Int32)this.Index;
		}
	}
}