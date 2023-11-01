using System;
using System.ComponentModel;

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

		/// <summary>Target row</summary>
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

		/// <summary>Display type of the class and type of referenced table</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return $"{this.GetType().Name}: {{{this.TableType}}}";
		}
	}
}