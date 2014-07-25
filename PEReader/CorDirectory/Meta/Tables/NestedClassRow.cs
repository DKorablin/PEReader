using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>NestedClass is defined as lexically "inside" the text of its enclosing Type.</summary>
	/// <remarks>
	/// The NestedClass table records which Type definitions are nested within which other Type definition.
	/// In a typical high-level language, the nested class is defined as lexically "inside" the text of its enclosing Type
	/// </remarks>
	public class NestedClassRow : BaseMetaRow
	{
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer NestedClassI { get { return base.GetValue<MetaCellPointer>(0); } }
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer EnclosingClassI { get { return base.GetValue<MetaCellPointer>(1); } }

		/// <summary>Child class row from TypeDef table</summary>
		public TypeDefRow NestedClass
		{
			get { return new TypeDefRow() { Row = this.NestedClassI.TargetRow, }; }
		}
		/// <summary>Parent class row from TypeDef table</summary>
		public TypeDefRow EnclosingClass
		{
			get { return new TypeDefRow() { Row = this.EnclosingClassI.TargetRow, }; }
		}
	}
}