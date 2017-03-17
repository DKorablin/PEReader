using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable cell</summary>
	[DefaultProperty("Value")]
	[DebuggerDisplay("Column={Column.Name} Value={Value}")]
	public class MetaCell : ICell
	{
		private readonly MetaTable _table;
		private readonly MetaColumn _column;
		private readonly UInt32 _rowIndex;

		private readonly UInt32 _rawValue;
		private readonly Object _value;

		/// <summary>Owner table</summary>
		internal MetaTable Table { get { return this._table; } }

		/// <summary>Owner column</summary>
		public MetaColumn Column { get { return this._column; } }
		IColumn ICell.Column { get { return this._column; } }

		/// <summary>Cell row index</summary>
		public UInt32 RowIndex { get { return this._rowIndex; } }

		/// <summary>Cell value</summary>
		public Object Value { get { return this._value; } }

		/// <summary>Original value from PE file</summary>
		public UInt32 RawValue { get { return this._rawValue; } }

		/// <summary>Create instance of MetaTable cell class</summary>
		/// <param name="table">Owner table</param>
		/// <param name="column">Owner column</param>
		/// <param name="rowIndex">Owner row index</param>
		/// <param name="reader">Image reader pointed to cell value</param>
		public MetaCell(MetaTable table, MetaColumn column, UInt32 rowIndex, BinaryReader reader)
		{
			if(table == null)
				throw new ArgumentNullException("table");
			if(column == null)
				throw new ArgumentNullException("column");
			if(reader == null)
				throw new ArgumentNullException("reader");

			this._table = table;
			this._column = column;
			this._rowIndex = rowIndex;

			switch(this.Column.ColumnType)
			{
			case MetaColumnType.UInt16:
				this._rawValue = reader.ReadUInt16();
				this._value = (UInt16)this.RawValue;
				break;
			case MetaColumnType.UInt32:
				this._rawValue = reader.ReadUInt32();
				this._value = (UInt32)this.RawValue;
				break;
			case MetaColumnType.String:
				this._rawValue = this.Table.Root.StreamTableHeader.StringIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this._value = this.Table.Root.Parent.StringHeap[(Int32)this.RawValue];
				break;
			case MetaColumnType.Guid:
				this._rawValue = this.Table.Root.StreamTableHeader.GuidIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this._value = this.Table.Root.Parent.GuidHeap[(Int32)this.RawValue];
				break;
			case MetaColumnType.Blob:
				this._rawValue = this.Table.Root.StreamTableHeader.BlobIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this._value = this.Table.Root.Parent.BlobHeap[(Int32)this.RawValue];
				break;
			/*case MetaColumnType.UserString:
				this._value = this.Table.Root.MetaData.USHeap[(Int32)this.RawValue];
			break;*/
			default:
				if(this.Column.IsCellPointer)//Указатель на другую таблицу
				{
					MetaTable refTable = this.Table.Root[(Cor.MetaTableType)this.Column.ColumnType];
					this._rawValue = ((refTable.RowsCount < 65536) ? reader.ReadUInt16() : reader.ReadUInt32());
					this._value = new MetaCellPointer(this, this.RawValue);
					//TODO: Не понял необходимость сдвига исходя из типа таблицы.
					//this.RawValue = (((uint)this.Column.Type << 24) | ((refTable.RowsCount < 65536) ? reader.ReadUInt16() : reader.ReadUInt32()));
				} else if(this.Column.IsCodedToken)//Coded token
				{// Coded token (may need to be uncompressed from 2-byte form)
					UInt32 codedToken = this.Table.SizeOfColumn(this.Column.ColumnType) == 2 ? reader.ReadUInt16() : reader.ReadUInt32();
					MetaColumnType[] columns = MetaTable.GetCodedTokenTypes(this.Column.ColumnType);

					Int32 tableIndex = ((Int32)codedToken) & ~(-1 << MetaCellCodedToken.CodedTokenBits[columns.Length]);
					Int32 index = ((Int32)codedToken) >> MetaCellCodedToken.CodedTokenBits[columns.Length];

					Int32 token = MetaCellCodedToken.ToToken(columns[tableIndex], index);
					this._rawValue = (UInt32)token;

					this._value = new MetaCellCodedToken(this, (UInt32)index, columns[tableIndex]);
					//this._value = new MetaCellCodedToken(this, this.RawValue);
				}
				break;
			}
		}
		/// <summary>Convert cell value to String</summary>
		/// <returns>value converted to string</returns>
		public override String ToString()
		{
			return String.Format("{0} : {{{1}: {2}}}", this.GetType().Name, this.Table.TableType, this.Column.Name);
		}
	}
}