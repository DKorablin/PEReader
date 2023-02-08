using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Coded Token class</summary>
	public class MetaCellCodedToken : CellPointerBase
	{
		/// <summary>Number of bits in coded token tag for a coded token that refers to n tables</summary>
		/// <remarks>values 5-17 are not used :I</remarks>
		internal static readonly Int32[] CodedTokenBits = new Int32[] { 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, };

		/// <summary>Create instance of coded token class</summary>
		/// <param name="cell">Source cell</param>
		/// <param name="rawValue">Original value from PE file</param>
		internal MetaCellCodedToken(MetaCell cell, UInt32 rawValue)
			: this(cell, (rawValue & 0xFFFFFF), (MetaColumnType)(rawValue >> 24))
		{
		}

		internal MetaCellCodedToken(MetaCell cell, UInt32 rowIndex, MetaColumnType columnType)
			: base(cell,
			(UInt32)rowIndex,
			(Cor.MetaTableType)columnType)
		{
		}

		internal static Int32 ToToken(MetaColumnType column, Int32 index)
		{
			if(index < 0)
				throw new IndexOutOfRangeException($"Invalid coded token index ({index})");
				//return -1;
			Int32 result = (Int32)column << 24 | index;
			/*MetaColumnType revColumn = (MetaColumnType)(result >> 24);
			Int32 revIndex = result & (0xFFFFFF);*/
			return result;
		}
	}
}