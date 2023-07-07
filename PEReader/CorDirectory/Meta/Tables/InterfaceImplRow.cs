using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The InterfaceImpl table records the interfaces a type implements explicitly</summary>
	/// <remarks>Conceptually, each row in the InterfaceImpl table indicates that Class implements Interface</remarks>
	public class InterfaceImplRow : BaseMetaRow
	{
		/// <summary>Index into the TypeDef table</summary>
		internal MetaCellPointer ClassI { get { return base.GetValue<MetaCellPointer>(0); } }

		/// <summary>
		/// An index into the TypeDef, TypeRef, or TypeSpec table; more precisely, a TypeDefOrRef (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken Interface { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>TypeDef table row that implements current interface</summary>
		public TypeDefRow Class
		{
			get { return new TypeDefRow() { Row = this.ClassI.TargetRow, }; }
		}
	}
}