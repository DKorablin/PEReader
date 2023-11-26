using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Method argument from ParamRow with type and defaut value</summary>
	public class MemberArgument : MemberValueBase
	{
		/// <summary>Flags that can be associated with parameter</summary>
		public ParameterAttributes Flags { get; internal set; } = ParameterAttributes.None;

		/// <summary>Constructor argument sequence index in constructor definition</summary>
		public UInt16 Sequence { get; }

		internal MemberArgument(ElementType type, String name, UInt16 sequence)
			: base(type, name ?? "A_" + sequence)
		{
			this.Sequence = sequence;
		}
	}
}