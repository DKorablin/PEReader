using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The rows of the MethodSemantics table are filled by .property (§II.17) and .event directives (§II.18)</summary>
	/// <remarks>(See §II.22.13 for more information)</remarks>
	public class MethodSemanticsRow : BaseMetaRow
	{
		/// <summary>2-byte bitmask of type MethodSemanticsAttributes, §II.23.1.12</summary>
		public CorMethodSemanticsAttr Semantic { get { return (CorMethodSemanticsAttr)base.GetValue<UInt16>(0); } }

		/// <summary>An index into the MethodDef table</summary>
		internal MetaCellPointer MethodI { get { return base.GetValue<MetaCellPointer>(1); } }

		/// <summary>An index into the Event or Property table; more precisely, a HasSemantics (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Association { get { return base.GetValue<MetaCellCodedToken>(2); } }

		/// <summary>MethodDef table row</summary>
		public MethodDefRow Method
		{
			get { return new MethodDefRow() { Row = this.MethodI.TargetRow, }; }
		}

		/// <summary>Create instance of MethodSemantics row</summary>
		public MethodSemanticsRow()
			: base(Cor.MetaTableType.MethodSemantics) { }
	}
}