using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Method body descriptor</summary>
	public class MethodBody
	{
		private static UInt32 SizeOfMethodSection = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodSection));
		private static UInt32 SizeOfMethodExceptionFat = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodExceptionFat));
		private static UInt32 SizeOfMethodExceptionSmall = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodExceptionSmall));

		private readonly MethodDefRow _row;
		private static Dictionary<Byte, OpCode> _opCodeList;
		private Cor.CorILMethodHeader? _header;
		private MethodDefRow Row { get { return this._row; } }

		private static Dictionary<Byte, OpCode> OpCodeList
		{
			get
			{
				if(MethodBody._opCodeList == null)
				{
					MethodBody._opCodeList = new Dictionary<Byte, OpCode>();
					foreach(FieldInfo field in typeof(OpCodes).GetFields())
					{
						OpCode code = (OpCode)field.GetValue(null);
						checked { MethodBody._opCodeList.Add((Byte)code.Value, code); }
					}
				}
				return MethodBody._opCodeList;
			}
		}
		/// <summary>Method header</summary>
		public Cor.CorILMethodHeader Header
		{
			get
			{
				if(this._header == null)
				{
					PEHeader header = this.Row.Row.Table.Root.Parent.Parent.Parent.Header;
					Byte check = header.ReadBytes(this.Row.RVA, 1)[0];
					Boolean isFatFormat = (check & (Byte)Cor.CorILMethod.FatFormat) == (Byte)Cor.CorILMethod.FatFormat;

					if(isFatFormat)
						this._header = header.PtrToStructure<Cor.CorILMethodHeader>(this.Row.RVA);
					else
						this._header = new Cor.CorILMethodHeader()
						{
							Format = Cor.CorILMethod.TinyFormat,
							CodeSize = ((UInt32)check >> 2),//Пропускаю первые 2 бита с флагами
						};
				}
				return this._header.Value;
			}
		}

		internal MethodBody(MethodDefRow row)
		{
			this._row = row;
		}
		/// <summary>Returns the OpCodes represented by a MSIL byte array.</summary>
		/// <param name="data">MSIL byte array.</param>
		/// <returns>An array of the OpCodes representing the MSIL code.</returns>
		private static OpCode[] GetOpCodes(Byte[] data)
		{
			return Array.ConvertAll(data, delegate(Byte opCodeByte) { return MethodBody.OpCodeList[opCodeByte]; });
		}
		/// <summary>Получить тело метода</summary>
		/// <returns>Массив байт описывающий CIL</returns>
		public Byte[] GetMethodBody()
		{
			PEHeader peHeader = this.Row.Row.Table.Root.Parent.Parent.Parent.Header;

			UInt32 padding = this.Row.RVA + this.Header.HeaderSize;
			UInt32 methodLength = this.Header.CodeSize;

			return peHeader.ReadBytes(padding, methodLength);
		}
		/// <summary>Get method body as MSIL instructions array</summary>
		/// <returns>MSIL instructions array</returns>
		public OpCode[] GetMethodBody2()
		{
			return MethodBody.GetOpCodes(this.GetMethodBody());
		}
		/// <summary>Get fat method header sections</summary>
		/// <returns>Method header sections</returns>
		public IEnumerable<MethodSection> GetSections()
		{
			UInt32 padding = this.Row.RVA + this.Header.HeaderSize;
			UInt32 methodLength = this.Header.CodeSize;
			if((this.Header.Format & Cor.CorILMethod.FatFormat) == Cor.CorILMethod.FatFormat
				&& (this.Header.Format & Cor.CorILMethod.MoreSects) == Cor.CorILMethod.MoreSects)
			{
				padding += methodLength;
				PEHeader header = this.Row.Row.Table.Root.Parent.Parent.Parent.Header;
				Boolean moreSections = true;

				while(moreSections)
				{
					//Each section should start on a 4 byte boundary
					//so let's read from the stream until we find the next boundary.
					padding = NativeMethods.AlignToInt(padding);
					Cor.CorILMethodSection section = header.PtrToStructure<Cor.CorILMethodSection>(padding);
					padding += MethodBody.SizeOfMethodSection;

					//I have never seen anything else than an exception handling section...
					//According to the documentation "Currently, the method data sections
					//are only used for exception tables."
					if((section.Kind & Cor.CorILMethod_Sect.EHTable) != Cor.CorILMethod_Sect.EHTable)
						throw new NotImplementedException("Only exception table supported");

					//Check whether more sections follow after this one.
					moreSections = section.HasMoreSections;

					Cor.CorILMethodExceptionFat[] fat = new Cor.CorILMethodExceptionFat[section.IsFatFormat ? section.ClauseNumber : 0];
					Cor.CorILMethodExceptionSmall[] small = new Cor.CorILMethodExceptionSmall[section.IsFatFormat ? 0 : section.ClauseNumber];
					//Let's read the clauses...
					for(Int32 clauseIndex = 0;clauseIndex < section.ClauseNumber;clauseIndex++)
					{
						//The structure of the clauses are the same in both Fat and
						//Small format, only the sizes are different.
						if(section.IsFatFormat)
						{
							fat[clauseIndex] = header.PtrToStructure<Cor.CorILMethodExceptionFat>(padding);
							padding += MethodBody.SizeOfMethodExceptionFat;
						} else
						{
							small[clauseIndex] = header.PtrToStructure<Cor.CorILMethodExceptionSmall>(padding);
							padding += MethodBody.SizeOfMethodExceptionSmall;
						}
					}
					yield return new MethodSection(section, fat, small);
				}
			}
		}
	}
}