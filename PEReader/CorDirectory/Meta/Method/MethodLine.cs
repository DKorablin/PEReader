using System;
using System.Reflection.Emit;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>IL Method instruction description</summary>
	public class MethodLine
	{
		private readonly Int32 _line;
		private readonly OpCode _il;
		private readonly MetaCellCodedToken _token;
		private readonly Int32? _offset;
		private readonly String _strConst;

		/// <summary>Method line index</summary>
		public Int32 Line { get { return this._line; } }

		/// <summary>MSIL instruction</summary>
		public OpCode IL { get { return this._il; } }

		/// <summary>Reference to MetaData</summary>
		public MetaCellCodedToken Token { get { return this._token; } }

		/// <summary>Offset to other line in current stack</summary>
		public Int32? Offset { get { return this._offset; } }

		/// <summary>String constant in the instruction</summary>
		public String StrConst { get { return this._strConst; } }

		internal MethodLine(Int32 line, OpCode il, MetaCellCodedToken token)
			: this(line, il)
		{
			this._token = token;
		}

		internal MethodLine(Int32 line, OpCode il, Int32? offset)
			: this(line, il)
		{
			this._offset = offset;
		}

		internal MethodLine(Int32 line, OpCode il, String strConst)
			: this(line, il)
		{
			this._strConst = strConst;
		}

		internal MethodLine(Int32 line, OpCode il)
		{
			this._line = line;
			this._il = il;
		}
	}
}