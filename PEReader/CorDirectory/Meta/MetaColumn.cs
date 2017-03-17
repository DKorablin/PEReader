using System;
using System.ComponentModel;
using System.Diagnostics;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable column class</summary>
	[DefaultProperty("Name")]
	[DebuggerDisplay("Type={ColumnType} Name={Name}")]
	public class MetaColumn : IColumn
	{
		private readonly Cor.MetaTableType _tableType;
		private readonly MetaColumnType _columnType;
		private readonly String _columnName;
		private readonly UInt16 _columnIndex;

		/// <summary>Owner table type</summary>
		public Cor.MetaTableType TableType { get { return this._tableType; } }

		/// <summary>Column type</summary>
		public MetaColumnType ColumnType { get { return this._columnType; } }

		/// <summary>Column name</summary>
		public String Name { get { return this._columnName; } }

		/// <summary>Column index in table</summary>
		public UInt16 Index { get { return this._columnIndex; } }

		/// <summary>Cell pointer column</summary>
		public Boolean IsCellPointer { get { return MetaColumn.IsColumnCellPointer(this.ColumnType); } }

		/// <summary>Coded token column</summary>
		public Boolean IsCodedToken { get { return MetaColumn.IsColumnCodedToken(this.ColumnType); } }

		/// <summary>Create instance of MetaTable column class</summary>
		/// <param name="tableType">Owner table type</param>
		/// <param name="columnType">Column type</param>
		/// <param name="columnName">Column name</param>
		/// <param name="columnIndex">Column index in table</param>
		public MetaColumn(Cor.MetaTableType tableType, MetaColumnType columnType, String columnName, UInt16 columnIndex)
		{
			this._tableType = tableType;
			this._columnType = columnType;
			this._columnName = columnName;
			this._columnIndex = columnIndex;
		}

		/// <summary>Cell pointer column</summary>
		/// <param name="type">MetaData column type</param>
		/// <returns>This column contains cell pointer</returns>
		public static Boolean IsColumnCellPointer(MetaColumnType type)
		{
			return (Int32)type < (Int32)MetaColumnType.TypeDefOrRef;
		}

		/// <summary>Coded token column</summary>
		/// <param name="type">MetaData column type</param>
		/// <returns>This column contains coded token</returns>
		public static Boolean IsColumnCodedToken(MetaColumnType type)
		{
			return (Int32)type >= (Int32)MetaColumnType.TypeDefOrRef && (Int32)type < (Int32)MetaColumnType.UInt16;
		}
	}
}