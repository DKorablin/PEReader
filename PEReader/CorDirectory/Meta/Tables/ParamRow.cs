using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The rows in the Param table result from the parameters in a method declaration (§II.15.4),
	/// or from a .param attribute attached to a method (§II.15.4.1).
	/// </summary>
	/// <remarks>Conceptually, every row in the Param table is owned by one, and only one, row in the MethodDef table.</remarks>
	public class ParamRow : BaseMetaRow
	{
		/// <summary>Flags that can be associated with parameter</summary>
		public ParameterAttributes Flags { get { return (ParameterAttributes)base.GetValue<UInt16>(0); } }
		/// <summary>Parameter index in method definition</summary>
		public UInt16 Sequence { get { return base.GetValue<UInt16>(1); } }
		/// <summary>Parameter name</summary>
		public String Name { get { return base.GetValue<String>(2); } }
	}
}