using System;
using System.Collections.Generic;
using System.Reflection;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Method argument from ParamRow with type and defaut value</summary>
	public class MemberArgument : MemberValueBase
	{
		/// <summary>The rows in the Param table result from the parameters in a method declaration (§II.15.4), or from a .param attribute attached to a method (§II.15.4.1).</summary>
		public ParamRow ParamRow { get; }

		/// <summary>Flags that can be associated with parameter</summary>
		public ParameterAttributes Flags { get; internal set; } = ParameterAttributes.None;

		/// <summary>Constructor argument sequence index in constructor definition</summary>
		public UInt16 Sequence { get; }

		internal MemberArgument(ElementType type, ParamRow paramRow)
			: this(type, paramRow.Name, paramRow.Sequence)
		{//TODO: We have one place where we're using MemberRefRow instead of MethodDefRow. See .ctor references
			this.ParamRow = paramRow;
		}

		internal MemberArgument(ElementType type, String name, UInt16 sequence)
			: base(type, name ?? "A_" + sequence)
		{
			this.Sequence = sequence;
		}

		/// <summary>Gets list of attributes applied to current method</summary>
		/// <returns>List of <see cref="AttributeReader"/> instances related to current method</returns>
		public IEnumerable<AttributeReader> GetCustomAttributes()
		{
			if(this.ParamRow == null)
				yield break;

			foreach(CustomAttributeRow row in this.ParamRow.Row.Table.Root.CustomAttribute)
				if(row.Parent.TableType == Cor.MetaTableType.Param
					&& row.Parent.TargetRow == this.ParamRow)
						yield return new AttributeReader(row);
		}
	}
}