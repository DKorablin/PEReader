using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The ClassLayout table is used to define how the fields of a class or value type
	/// shall be laid out by the CLI.
	/// (Normally, the CLI is free to reorder and/or insert gaps between the fields defined
	/// for a class or value type)
	/// </summary>
	public class ClassLayoutRow : BaseMetaRow
	{
		/// <summary>A 2-byte constant</summary>
		public UInt16 PackingSize { get { return base.GetValue<UInt16>(0); } }

		/// <summary>A 4-byte constant</summary>
		/// <remarks>
		/// ClassSize of zero does not mean the class has zero size.
		/// It means that no .size directive was specified at definition time, in which case,
		/// the actual size is calculated from the field types,
		/// taking account of packing size (default or specified) and natural alignment on the target,
		/// runtime platform
		/// </remarks>
		public UInt32 ClassSize { get { return base.GetValue<UInt32>(1); } }

		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer ParentI { get { return base.GetValue<MetaCellPointer>(2); } }

		/// <summary>TypeDef table row</summary>
		public TypeDefRow Parent
		{
			get { return new TypeDefRow() { Row = this.ParentI.TargetRow, }; }
		}
	}
}