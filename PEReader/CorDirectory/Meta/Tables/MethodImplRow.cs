using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// MethodImpl tables let a compiler override the default inheritance rules provided by the CLI.
	/// Their original use was to allow a class C, that inherited method M from both interfaces I and J, to provide implementations for both methods (rather than have only one slot for M in its vtable).
	/// However, MethodImpls can be used for other reasons too, limited only by the compiler writer's ingenuity within the constraints defined in the Validation rules below.
	/// </summary>
	public class MethodImplRow : BaseMetaRow
	{
		/// <summary>An index into the TypeDef table</summary>
		internal MetaCellPointer ClassI { get { return base.GetValue<MetaCellPointer>(0); } }

		/// <summary>An index into the MethodDef or MemberRef table; more precisely, a MethodDefOrRef (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken MethodBody { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>An index into the MethodDef or MemberRef table; more precisely, a MethodDefOrRef (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken MethodDeclaration { get { return base.GetValue<MetaCellCodedToken>(2); } }

		/// <summary>TypeDef table row</summary>
		public TypeDefRow Class
		{
			get { return new TypeDefRow() { Row = this.ClassI.TargetRow, }; }
		}

		/// <summary>Create instance of Method implementation row</summary>
		public MethodImplRow()
			: base(Cor.MetaTableType.MethodImpl) { }
	}
}