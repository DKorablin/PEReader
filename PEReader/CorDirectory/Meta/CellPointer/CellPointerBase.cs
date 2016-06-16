using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Base class for Cell or CodedToken pointer</summary>
	[DefaultProperty("TableType")]
	public class CellPointerBase
	{
		#region Fields
		private readonly MetaCell _cell;
		private readonly UInt32? _rowIndex;
		private readonly Cor.MetaTableType _tableType;
		#endregion Fields

		/// <summary>Source cell</summary>
		public MetaCell Cell { get { return this._cell; } }

		/// <summary>Row index in target table</summary>
		public UInt32? RowIndex { get { return this._rowIndex; } }

		/// <summary>Target table type</summary>
		public Cor.MetaTableType TableType { get { return this._tableType; } }

		/// <summary>Target table</summary>
		public MetaTable TargetTable { get { return this.Cell.Table.Root[this.TableType]; } }

		/// <summary>Target row</summary>
		public MetaRow TargetRow { get { return this.RowIndex == null ? null : this.TargetTable[this.RowIndex.Value]; } }

		/// <summary>Create instance of base cell pointer class</summary>
		/// <param name="cell">Source cell</param>
		/// <param name="rowIndex">Target row index</param>
		/// <param name="tableType">Target table type</param>
		internal CellPointerBase(MetaCell cell, UInt32 rowIndex, Cor.MetaTableType tableType)
		{
			this._cell = cell;
			this._rowIndex = rowIndex == 0 ? (UInt32?)null : checked(rowIndex - 1);//TODO: Почему -1? (Код вылетает в Overflow в таблице TypeDef в TypeName=<Module>)
			this._tableType = tableType;
		}
		/// <summary>Display type of the class and type of referenced table</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return String.Format("{0}: {{{1}}}", this.GetType().Name, this.TableType);
		}
	}
}