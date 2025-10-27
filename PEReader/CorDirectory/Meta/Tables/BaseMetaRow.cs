using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Basic class to describe any metadata table row</summary>
	public class BaseMetaRow : IEquatable<BaseMetaRow>
	{
		private MetaRow _row;

		internal Cor.MetaTableType TableType { get; }

		/// <summary>Base metadata row for processing</summary>
		/// <exception cref="ArgumentNullException">value is null</exception>
		internal MetaRow Row
		{
			get => this._row;
			set => this._row = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>Row index</summary>
		public UInt32 Index => this.Row.Index;

		internal BaseMetaRow(Cor.MetaTableType type)
			=> this.TableType = type;

		/// <summary>Get cell value by index from metadata row</summary>
		/// <typeparam name="T">Cell data type</typeparam>
		/// <param name="columnIndex">Column index from metadata table</param>
		/// <returns>Cell value from table metadata</returns>
		protected T GetValue<T>(UInt16 columnIndex)
			=> (T)this.Row[columnIndex].Value;

		/// <summary>Bit check to reduce syntax</summary>
		/// <param name="flags">Flags</param>
		/// <param name="enumValue">Bit index to check</param>
		/// <returns>Bit is set</returns>
		protected static Boolean IsBitSet(UInt32 flags, UInt32 enumValue)
			=> (flags & enumValue) == enumValue;

		/// <summary>Shows current object as string</summary>
		/// <param name="args">Key value to show as string</param>
		/// <returns>String representation</returns>
		protected internal String ToString(Object args)
			=> $"{this.GetType().Name} : {{{args}}}";

		/// <summary>Compare two rows by table type and index fields</summary>
		/// <param name="obj">Object to compare with current field</param>
		/// <returns>Objects are equals</returns>
		public override Boolean Equals(Object obj)
			=> this.Equals(obj as BaseMetaRow);

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
		public static Boolean operator !=(BaseMetaRow a, BaseMetaRow b)
			=> !(a == b);

		/// <summary>Converts base row for strongly typed rows to generic row</summary>
		/// <param name="row">The base row</param>
		public static implicit operator MetaRow(BaseMetaRow row)
			=> row.Row;

		/// <summary>Gets unique identifier for current row in current table</summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
			=> this.Row.Table.TableType.GetHashCode() ^ (Int32)this.Index;
	}
}