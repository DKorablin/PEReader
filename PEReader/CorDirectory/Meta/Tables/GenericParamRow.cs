using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The GenericParam table stores the generic parameters used in generic type definitions and generic method definitions.
	/// These generic parameters can be constrained (i.e., generic arguments shall extend some class and/or implement certain interfaces) or unconstrained.
	/// (Such constraints are stored in the GenericParamConstraint table.)
	/// </summary>
	/// <remarks>Conceptually, each row in the GenericParam table is owned by one, and only one, row in either the TypeDef or MethodDef tables</remarks>
	public class GenericParamRow : BaseMetaRow
	{
		/// <summary>Parameter index</summary>
		public UInt16 Number => base.GetValue<UInt16>(0);

		/// <summary>Generic parameter constraint</summary>
		public GenericParameterAttributes Flags => (GenericParameterAttributes)base.GetValue<UInt16>(1);

		/// <summary>
		/// An index into the TypeDef or MethodDef table, specifying the Type or Method
		/// to which this generic parameter applies; more precisely, a TypeOrMethodDef (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken Owner => base.GetValue<MetaCellCodedToken>(2);

		/// <summary>Name for the generic parameter</summary>
		/// <remarks>This is purely descriptive and is used only by source language compilers and by reflection</remarks>
		public String Name => base.GetValue<String>(3);

		/// <summary>Create instance of Generic param row</summary>
		public GenericParamRow()
			: base(Cor.MetaTableType.GenericParam) { }

		/// <summary>Represents generic param row as a string</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> base.ToString(this.Name);
	}
}