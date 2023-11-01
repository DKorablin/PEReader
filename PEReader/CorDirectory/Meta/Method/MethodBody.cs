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

		private static Dictionary<Int16, OpCode> _opCodeList;
		private Cor.CorILMethodHeader? _header;
		private MethodDefRow Row { get; }

		private static Dictionary<Int16, OpCode> OpCodeList
		{
			get
			{
				if(MethodBody._opCodeList == null)
				{
					MethodBody._opCodeList = new Dictionary<Int16, OpCode>();
					foreach(FieldInfo field in typeof(OpCodes).GetFields())
					{
						OpCode code = (OpCode)field.GetValue(null);
						checked { MethodBody._opCodeList.Add((Int16)code.Value, code); }
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
			this.Row = row ?? throw new ArgumentNullException(nameof(row));
		}

		/// <summary>Gets method body</summary>
		/// <returns>Byte array describes CIL</returns>
		public Byte[] GetMethodBody()
		{
			PEHeader peHeader = this.Row.Row.Table.Root.Parent.Parent.Parent.Header;

			UInt32 padding = this.Row.RVA + this.Header.HeaderSize;
			UInt32 methodLength = this.Header.CodeSize;

			return peHeader.ReadBytes(padding, methodLength);
		}

		/// <summary>Returns the OpCodes represented by a MSIL byte array</summary>
		/// <returns>An array of the OpCodes representing the MSIL code</returns>
		public IEnumerable<MethodLine> GetMethodBody2()
		{
			MetaCell cell = this.Row.Row.Cells[0];
			Byte[] data = this.GetMethodBody();

			for(Int32 loop = 0; loop < data.Length; loop++)
			{
				OpCode il = MethodBody.OpCodeList[data[loop]];
				switch(il.Name)
				{
				case "prefix1":
					Int16 value = BitConverter.IsLittleEndian
						? BitConverter.ToInt16(new Byte[] { data[loop + 1], data[loop] }, 0)
						: BitConverter.ToInt16(data, loop);
					il = MethodBody.OpCodeList[value];
					loop++; //Actual index is started from previos position, but we have to increment it here once more time. TODO: Find the way to toss real index to output code
					break;
				}

				switch(il.Name)
				{
				case "ldfld":
				case "stfld":
				case "call":
				case "callvirt":
				case "newobj":
				case "newarr":
				case "ldtoken":
				case "ldsfld":
				case "castclass":
				case "isinst":
				case "stelem":
				case "box":
				case "unbox.any":
				case "ldelem":
					UInt32 rawValue = BitConverter.ToUInt32(data, loop + 1);
					MetaCellCodedToken token = new MetaCellCodedToken(cell, rawValue);
					yield return new MethodLine(loop, il, token);
					loop += 4;
					break;
				case "brfalse": //(Вероятно тут не int) Checked on ILMerge.exe CreateTargetAssembly. It's Int32
				case "brtrue":
				case "ldc.i4":
				case "br":
				case "leave":
				case "bne.un":
				case "blt":
				case "ble":
				case "beq":
				case "bge":
				case "bgt":
					Int32 offset = BitConverter.ToInt32(data, loop + 1);
					offset = (loop + 1) + sizeof(UInt32) + offset;
					yield return new MethodLine(loop, il, offset);
					loop += 4;
					break;
				case "brtrue.s":
				case "stloc.s":
				case "ldloc.s":
				case "ldc.i4.s":
				case "ldloca.s":
					Byte vListIndex = data[loop + 1];
					yield return new MethodLine(loop, il, vListIndex);
					loop += 1;
					break;
				case "br.s":
					Byte goToAddr = data[loop + 1];
					yield return new MethodLine(loop, il, loop + 2 + goToAddr);//The br.s instruction unconditionally transfers control to a target instruction. The target instruction is represented as a 1-byte signed offset from the beginning of the instruction (+1 for current instruction) following the current instruction.
					loop += 1;
					break;
				case "blt.s":
					Byte goToAddr2 = data[loop + 1];
					yield return new MethodLine(loop, il, loop + 2 + ((sbyte)goToAddr2));
					loop += 1;
					break;
				case "ldarg.s":
					UInt16 argIndex = data[loop + 1];
					MethodParamRow paramRow = null;
					foreach(var item in this.Row.ParamList)
						if(item.Sequence == argIndex)
						{
							paramRow = item;
							break;
						}
					yield return new MethodLine(loop, il, paramRow);
					loop += 1;
					break;
				case "ldstr":
					Int32 rawValue1 = BitConverter.ToInt32(data, loop + 1);
					Int32 usOffset = rawValue1 & 0xFFFFFF;
					String str = cell.Table.Root.Parent.USHeap[usOffset];
					yield return new MethodLine(loop, il, str);
					loop += 4;
					break;
				case "ldarg.0":
				case "ldarg.1":
				case "ldarg.2":
				case "ldarg.3":
				case "ldnull":
				case "ldc.i4.0":
				case "ldc.i4.1":
				case "ldc.i4.2":
				case "ldc.i4.3":
				case "ldc.i4.5":
				case "ldc.i4.7":
				case "ldc.i4.8":
				case "ret":
				case "nop":
				case "ceq":
				case "throw":
				case "stloc.0":
				case "stloc.1":
				case "stloc.2":
				case "stloc.3":
				case "conv.u8":
				case "dup":
				case "stelem.i1":
				case "add":
				case "ldloc.0":
				case "ldloc.1":
				case "ldloc.2":
				case "ldloc.3":
				case "endfinally":
				case "and":
				case "or":
				case "stelem.ref":
				case "pop":
				case "ldlen":
				case "conv.i4":
				case "shl":
				case "shr":
				case "volatile.":
					//if(il.OpCodeType)
					yield return new MethodLine(loop, il);
					break;
				default:
					throw new NotImplementedException(il.Name);
				}
			}
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