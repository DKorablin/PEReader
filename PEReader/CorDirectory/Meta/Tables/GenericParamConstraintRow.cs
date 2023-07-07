using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The GenericParamConstraint table records the constraints for each generic parameter.
	/// Each generic parameter can be constrained to derive from zero or one class.
	/// Each generic parameter can be constrained to implement zero or more interfaces.
	/// </summary>
	/// <remarks>Conceptually, each row in the GenericParamConstraint table is "owned" by a row in the GenericParam table</remarks>
	public class GenericParamConstraintRow : BaseMetaRow
	{
		/// <summary>An index into the GenericParam table, specifying to which generic parameter this row refers</summary>
		internal MetaCellPointer OwnerI { get { return base.GetValue<MetaCellPointer>(0); } }

		/// <summary>
		/// An index into the TypeDef, TypeRef, or TypeSpec tables, specifying from which class this generic parameter is constrained to derive;
		/// or which interface this generic parameter is constrained to implement;
		/// more precisely, a TypeDefOrRef (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken Constraint { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>GenericParam table row</summary>
		public GenericParamRow Owner
		{
			get { return new GenericParamRow() { Row = this.OwnerI.TargetRow, }; }
		}
	}
}