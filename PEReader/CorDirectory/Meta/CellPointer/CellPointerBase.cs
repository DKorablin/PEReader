using System;
using System.ComponentModel;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Base class for Cell or CodedToken pointer</summary>
	[DefaultProperty("TableType")]
	public class CellPointerBase
	{
		/// <summary>Source cell</summary>
		public MetaCell Cell { get; }

		/// <summary>Row index in target table</summary>
		public UInt32? RowIndex { get; }

		/// <summary>Target table type</summary>
		public Cor.MetaTableType TableType { get; }

		/// <summary>Target table</summary>
		public MetaTable TargetTable { get { return this.Cell.Table.Root[this.TableType]; } }

		/// <summary>Target metadata row</summary>
		public MetaRow TargetRow { get { return this.RowIndex == null ? null : this.TargetTable[this.RowIndex.Value]; } }

		/// <summary>Create instance of base cell pointer class</summary>
		/// <param name="cell">Source cell</param>
		/// <param name="rowIndex">Target row index</param>
		/// <param name="tableType">Target table type</param>
		internal CellPointerBase(MetaCell cell, UInt32 rowIndex, Cor.MetaTableType tableType)
		{
			this.Cell = cell ?? throw new ArgumentNullException(nameof(cell));
			this.RowIndex = rowIndex == 0 ? (UInt32?)null : checked(rowIndex - 1);//TODO: Почему -1? (Код вылетает в Overflow в таблице TypeDef в TypeName=<Module>)
			this.TableType = tableType;
		}

		/// <summary>Gets row where coded token points to</summary>
		/// <typeparam name="R">Strongly typed metadata row. This must be equevalent to <see cref="CellPointerBase.TableType"/></typeparam>
		/// <exception cref="InvalidOperationException">Invalid strongly typed TableType row passed as generic parameter. Check <see cref="CellPointerBase.TableType"/> before casting.</exception>
		/// <returns>Strongly typed target row</returns>
		public R GetTargetRowTyped<R>() where R : BaseMetaRow, new()
		{//TODO: We somehow need to add table validation because right now we can pass TypeRefRow but TableType will be passed to TypeSpec
			var table = new Tables.BaseMetaTable<R>(this.TargetTable.Root, this.TableType);
			R result = table[this.RowIndex.Value];
			if(result.TableType != this.TableType)
				throw new InvalidOperationException($"TableRow {result.TableType} is not of type {this.TableType}");

			return result;
		}

		/// <summary>Display type of the class and type of referenced table</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return $"{this.GetType().Name}: {{{this.TableType}}}";
		}
	}
}