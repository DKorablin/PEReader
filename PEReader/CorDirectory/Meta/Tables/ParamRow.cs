using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The rows in the Param table result from the parameters in a method declaration (§II.15.4), or from a .param attribute attached to a method (§II.15.4.1).
	/// </summary>
	/// <remarks>Conceptually, every row in the Param table is owned by one, and only one, row in the MethodDef table</remarks>
	public class ParamRow : BaseMetaRow
	{
		/// <summary>Flags that can be associated with parameter</summary>
		public virtual ParameterAttributes Flags => (ParameterAttributes)base.GetValue<UInt16>(0);

		/// <summary>Parameter index in method definition</summary>
		public virtual UInt16 Sequence => base.GetValue<UInt16>(1);

		/// <summary>Parameter name</summary>
		public virtual String Name => base.GetValue<String>(2);

		/// <summary>Create isntance of Member Param row</summary>
		public ParamRow()
			: base(Cor.MetaTableType.Param) { }
	}
}