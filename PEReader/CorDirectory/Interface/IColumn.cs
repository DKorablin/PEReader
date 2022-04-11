using System;

namespace AlphaOmega.Debug
{
	/// <summary>Generic colum for dynamic structures</summary>
	public interface IColumn
	{
		/// <summary>Name of the column. From CONSTANT structures</summary>
		String Name { get; }

		/// <summary>Zero based index from the beggining of structure</summary>
		UInt16 Index { get; }
	}
}