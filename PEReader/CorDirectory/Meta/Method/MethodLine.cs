using System;
using System.Reflection.Emit;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>IL Method instruction description</summary>
	public class MethodLine
	{
		/// <summary>Method line index</summary>
		public Int32 Line { get; }

		/// <summary>MSIL instruction</summary>
		public OpCode IL { get; }

		/// <summary>Reference to MetaData</summary>
		public MetaCellCodedToken Token { get; }

		/// <summary>Offset to other line in current stack</summary>
		public Int32[] Offset { get; }

		/// <summary>Reference to method param index</summary>
		public MemberArgument ParamIndexRow { get; }

		/// <summary>Constant value in the instruction</summary>
		public Object ConstantValue { get; }

		internal MethodLine(Int32 line, OpCode il, MetaCellCodedToken token)
			: this(line, il)
		{
			this.Token = token;
		}

		internal MethodLine(Int32 line, OpCode il, Int32[] offset)
			: this(line, il)
		{
			this.Offset = offset;
		}

		internal MethodLine(Int32 line, OpCode il, Object constantValue)
			: this(line, il)
		{
			this.ConstantValue = constantValue;
		}

		internal MethodLine(Int32 line, OpCode il, MemberArgument paramRow)
	: this(line, il)
		{
			this.ParamIndexRow = paramRow;
		}

		internal MethodLine(Int32 line, OpCode il)
		{
			this.Line = line;
			this.IL = il;
		}
	}
}