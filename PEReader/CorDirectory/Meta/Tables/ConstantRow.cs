using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The Constant table is used to store compile-time, constant values for fields, parameters, and properties.</summary>
	public class ConstantRow : BaseMetaRow
	{
		/// <summary>A 1-byte constant, followed by a 1-byte padding zero.</summary>
		/// <remarks>
		/// The encoding of Type for the nullref value for FieldInit in ilasm (§II.16.2)
		/// is <see cref="Cor.ELEMENT_TYPE.CLASS"/> with a Value of a 4-byte zero.
		/// Unlike uses of <see cref="Cor.ELEMENT_TYPE.CLASS"/> in signatures, this one is not followed by a type token.
		/// </remarks>
		public UInt16 Type { get { return base.GetValue<UInt16>(0); } }

		/// <summary>
		/// An index into the Param, Field, or Property table;
		/// more precisely, a HasConstant (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Parent { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>Constant value</summary>
		public Byte[] Value { get { return base.GetValue<Byte[]>(2); } }
	}
}