using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable cell</summary>
	[DefaultProperty(nameof(Value))]
	[DebuggerDisplay("Column={Column.Name} Value={Value}")]
	public class MetaCell : ICell
	{
		/// <summary>Owner table</summary>
		internal MetaTable Table { get; }

		/// <summary>Owner column</summary>
		public MetaColumn Column { get; }
		IColumn ICell.Column => this.Column;

		/// <summary>Cell row index</summary>
		public UInt32 RowIndex { get; }

		/// <summary>Cell value</summary>
		public Object Value { get; }

		/// <summary>Original value from PE file</summary>
		public UInt32 RawValue { get; }

		/// <summary>Create instance of MetaTable cell class</summary>
		/// <param name="table">Owner table</param>
		/// <param name="column">Owner column</param>
		/// <param name="rowIndex">Owner row index</param>
		/// <param name="reader">Image reader pointed to cell value</param>
		public MetaCell(MetaTable table, MetaColumn column, UInt32 rowIndex, BinaryReader reader)
		{
			_ = reader ?? throw new ArgumentNullException(nameof(reader));

			this.Table = table ?? throw new ArgumentNullException(nameof(table));
			this.Column = column ?? throw new ArgumentNullException(nameof(column));
			this.RowIndex = rowIndex;

			switch(this.Column.ColumnType)
			{
			case MetaColumnType.UInt16:
				this.RawValue = reader.ReadUInt16();
				this.Value = (UInt16)this.RawValue;
				break;
			case MetaColumnType.UInt32:
				this.RawValue = reader.ReadUInt32();
				this.Value = (UInt32)this.RawValue;
				break;
			case MetaColumnType.String:
				this.RawValue = this.Table.Root.StreamTableHeader.StringIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this.Value = this.Table.Root.Parent.StringHeap[(Int32)this.RawValue];
				break;
			case MetaColumnType.Guid:
				this.RawValue = this.Table.Root.StreamTableHeader.GuidIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this.Value = this.Table.Root.Parent.GuidHeap[(Int32)this.RawValue];
				break;
			case MetaColumnType.Blob:
				this.RawValue = this.Table.Root.StreamTableHeader.BlobIndexSize == 2 ? (UInt32)reader.ReadUInt16() : reader.ReadUInt32();
				this.Value = this.Table.Root.Parent.BlobHeap[(Int32)this.RawValue];
				break;
			/*case MetaColumnType.UserString:
				this._value = this.Table.Root.MetaData.USHeap[(Int32)this.RawValue];
			break;*/
			default:
				if(this.Column.IsCellPointer)//Pointer to different table
				{
					MetaTable refTable = this.Table.Root[(Cor.MetaTableType)this.Column.ColumnType];
					this.RawValue = ((refTable.RowsCount < 65536) ? reader.ReadUInt16() : reader.ReadUInt32());
					this.Value = new MetaCellPointer(this, this.RawValue);
					//TODO: Не понял необходимость сдвига исходя из типа таблицы.
					//this.RawValue = (((uint)this.Column.Type << 24) | ((refTable.RowsCount < 65536) ? reader.ReadUInt16() : reader.ReadUInt32()));
				} else if(this.Column.IsCodedToken)//Coded token
				{// Coded token (may need to be uncompressed from 2-byte form)
					UInt32 codedToken = this.Table.SizeOfColumn(this.Column.ColumnType) == 2 ? reader.ReadUInt16() : reader.ReadUInt32();
					MetaColumnType[] columns = MetaTable.GetCodedTokenTypes(this.Column.ColumnType);

					Int32 tableIndex = ((Int32)codedToken) & ~(-1 << MetaCellCodedToken.CodedTokenBits[columns.Length]);
					Int32 index = ((Int32)codedToken) >> MetaCellCodedToken.CodedTokenBits[columns.Length];

					Int32 token = MetaCellCodedToken.ToToken(columns[tableIndex], index);
					this.RawValue = (UInt32)token;

					this.Value = new MetaCellCodedToken(this, (UInt32)index, columns[tableIndex]);
					//this._value = new MetaCellCodedToken(this, this.RawValue);
				}
				break;
			}
		}
		/// <summary>Convert cell value to String</summary>
		/// <returns>value converted to string</returns>
		public override String ToString()
			=> $"{this.GetType().Name} : {{{this.Table.TableType}: {this.Column.Name}}}";
	}
}