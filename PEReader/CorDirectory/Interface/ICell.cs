using System;

namespace AlphaOmega.Debug
{
	/// <summary>Generic cell for dynamic structures</summary>
	public interface ICell
	{
		/// <summary>Abstract value stored in the column</summary>
		Object Value { get; }

		/// <summary>Here can be cell value or value length or ondes to the different table</summary>
		UInt32 RawValue { get; }

		/// <summary>Description of the column owner</summary>
		IColumn Column { get; }
	}
}