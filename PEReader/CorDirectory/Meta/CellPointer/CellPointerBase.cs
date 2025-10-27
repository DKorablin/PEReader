using System;
using System.ComponentModel;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Base class for Cell or CodedToken pointer</summary>
	[DefaultProperty(nameof(TableType))]
	public class CellPointerBase
	{
		/// <summary>Source cell</summary>
		public MetaCell Cell { get; }

		/// <summary>Row index in target table</summary>
		public UInt32? RowIndex { get; }

		/// <summary>Target table type</summary>
		public Cor.MetaTableType TableType { get; }

		/// <summary>Target table</summary>
		public MetaTable TargetTable => this.Cell.Table.Root[this.TableType];

		/// <summary>Target row</summary>
		public MetaRow TargetRow => this.RowIndex == null ? null : this.TargetTable[this.RowIndex.Value];

		internal CellPointerBase(MetaCell cell, UInt32 rawValue)
		{
			this.Cell = cell ?? throw new ArgumentNullException(nameof(cell));
			UInt32 tableId = rawValue & 0x03;
			this.RowIndex = checked((rawValue >> 2) - 1);//TODO: Why -1? (The code crashes in Overflow in the TypeRef table when accessing the last element)
			switch(tableId)
			{
			case 0://TypeDef
				this.TableType = Cor.MetaTableType.TypeDef;
				break;
			case 1://TypeRef
				this.TableType = Cor.MetaTableType.TypeRef;
				break;
			case 2://TypeSpec
				this.TableType = Cor.MetaTableType.TypeSpec;
				break;
			default:
				throw new NotSupportedException();
			}
		}

		/// <summary>Create instance of base cell pointer class</summary>
		/// <param name="cell">Source cell</param>
		/// <param name="rowIndex">Target row index</param>
		/// <param name="tableType">Target table type</param>
		internal CellPointerBase(MetaCell cell, UInt32 rowIndex, Cor.MetaTableType tableType)
		{
			this.Cell = cell ?? throw new ArgumentNullException(nameof(cell));
			this.RowIndex = rowIndex == 0 ? (UInt32?)null : checked(rowIndex - 1);//TODO: Why -1? (The code crashes in Overflow in the TypeDef table at TypeName=<Module>)
			this.TableType = tableType;
		}

		/// <summary>Gets row where coded token points to</summary>
		/// <typeparam name="R">Strongly typed metadata row. This must be equivalent to <see cref="CellPointerBase.TableType" /></typeparam>
		/// <exception cref="InvalidOperationException">Table {result.TableType} not belongs to table {this.TableType} where pointed</exception>
		/// <returns>Strongly typed target row</returns>
		public R GetTargetRowTyped<R>() where R : BaseMetaRow, new()
		{
			BaseMetaTable<R> table = new BaseMetaTable<R>(this.TargetTable.Root, this.TableType);
			R result = table[this.RowIndex.Value];
			if(result.TableType != this.TableType)
				throw new InvalidOperationException($"Table {result.TableType} not belongs to table {this.TableType} where pointed");
			return result;
		}

		/// <summary>Display type of the class and type of referenced table</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> $"{this.GetType().Name}: {{{this.TableType}}}";
	}
}