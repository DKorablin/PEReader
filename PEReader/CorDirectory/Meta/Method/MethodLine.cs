using System;
using System.Reflection.Emit;

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
		public Int32? Offset { get; }

		/// <summary>String constant in the instruction</summary>
		public String StrConst { get; }

		internal MethodLine(Int32 line, OpCode il, MetaCellCodedToken token)
			: this(line, il)
		{
			this.Token = token;
		}

		internal MethodLine(Int32 line, OpCode il, Int32? offset)
			: this(line, il)
		{
			this.Offset = offset;
		}

		internal MethodLine(Int32 line, OpCode il, String strConst)
			: this(line, il)
		{
			this.StrConst = strConst;
		}

		internal MethodLine(Int32 line, OpCode il)
		{
			this.Line = line;
			this.IL = il;
		}
	}
}