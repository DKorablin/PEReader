using System;
using System.Reflection;
using System.Threading;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The rows in the Param table result from the parameters in a method declaration (§II.15.4), or from a .param attribute attached to a method (§II.15.4.1).
	/// </summary>
	/// <remarks>Conceptually, every row in the Param table is owned by one, and only one, row in the MethodDef table</remarks>
	public class ParamRow : BaseMetaRow
	{
		/// <summary>Flags that can be associated with parameter</summary>
		public virtual ParameterAttributes Flags { get { return (ParameterAttributes)base.GetValue<UInt16>(0); } }

		/// <summary>Parameter index in method definition</summary>
		public virtual UInt16 Sequence { get { return base.GetValue<UInt16>(1); } }

		/// <summary>Parameter name</summary>
		public virtual String Name { get { return base.GetValue<String>(2); } }

		/// <summary>Create isntance of Member Param row</summary>
		public ParamRow()
			: base(Cor.MetaTableType.Param) { }
	}

	/// <summary>
	/// The rows in the Param table result from the parameters in a method declaration (§II.15.4), or from a .param attribute attached to a method (§II.15.4.1).
	/// </summary>
	/// <remarks>Conceptually, every row in the Param table is owned by one, and only one, row in the MethodDef table</remarks>
	public class MethodParamRow : ParamRow
	{
		/// <inheritdoc/>
		public override ParameterAttributes Flags => base.Flags;

		/// <inheritdoc/>
		public override string Name => base.Name;

		/// <inheritdoc/>
		public override ushort Sequence => base.Sequence;

		/// <summary>Element type description</summary>
		public virtual ElementType Type { get; }

		internal MethodParamRow(ElementType type)
		{
			this.Type = type;
		}
	}

	/// <summary>The method param row with value stored in CLI</summary>
	public class MethodParamValueRow : MethodParamRow
	{
		/// <inheritdoc/>
		public override ParameterAttributes Flags => base.Flags;

		/// <inheritdoc/>
		public override string Name => base.Name;

		/// <inheritdoc/>
		public override ushort Sequence => base.Sequence;

		/// <inheritdoc/>
		public override ElementType Type => base.Type;

		/// <summary>Value spcified in CLI</summary>
		public virtual Object Value { get; set; }

		internal MethodParamValueRow(MethodParamRow param, Object value)
			: this(param.Type, value)
		{
			base.Row = param.Row;
		}

		private MethodParamValueRow(ElementType element, Object value)
			: base(element)
		{
			this.Value = value;
		}
	}
}