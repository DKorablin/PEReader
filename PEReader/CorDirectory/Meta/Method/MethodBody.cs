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
		private static readonly UInt32 SizeOfMethodSection = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodSection));
		private static readonly UInt32 SizeOfMethodExceptionFat = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodExceptionFat));
		private static readonly UInt32 SizeOfMethodExceptionSmall = (UInt32)Marshal.SizeOf(typeof(Cor.CorILMethodExceptionSmall));

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

					if((check & (Byte)Cor.CorILMethod.FatFormat) == (Byte)Cor.CorILMethod.FatFormat)
						this._header = header.PtrToStructure<Cor.CorILMethodHeader>(this.Row.RVA);
					else if((check & (Byte)Cor.CorILMethod.TinyFormat) == (Byte)Cor.CorILMethod.TinyFormat)
						this._header = new Cor.CorILMethodHeader()
						{
							Format = Cor.CorILMethod.TinyFormat,
							CodeSize = ((UInt32)check >> 2),//Skip first 2 bits with flags
						};
					else
					{//Empty method without body (Yes. It could be like that)
						this._header = new Cor.CorILMethodHeader()
						{
							Format = (Cor.CorILMethod)0,
							CodeSize = 0,
						};
					}
				}
				return this._header.Value;
			}
		}

		internal MethodBody(MethodDefRow row)
			=> this.Row = row ?? throw new ArgumentNullException(nameof(row));

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
				Int32 line = loop;//Instruction line index
				OpCode il = MethodBody.OpCodeList[data[loop]];
				switch(il.Name)
				{
				case "prefix1":
					Int16 value = BitConverter.IsLittleEndian
						? BitConverter.ToInt16(new Byte[] { data[loop + 1], data[loop] }, 0)
						: BitConverter.ToInt16(data, loop);
					il = MethodBody.OpCodeList[value];
					loop++;
					break;
				}

				switch(il.Name)
				{//This is a test code of all tested OpCodes
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
				case "stsfld":
				case "ldftn":
				case "ldelema":
				case "stobj":
				case "ldflda":
				case "initobj":
				case "ldobj":
				case "ldsflda":
				case "ldvirtftn":
				case "constrained.":
					UInt32 rawValue = BitConverter.ToUInt32(data, loop + 1);
					MetaCellCodedToken token = new MetaCellCodedToken(cell, rawValue);
					yield return new MethodLine(line, il, token);
					loop += 4;
					break;
				case "brfalse":
				case "brtrue":
				case "br":
				case "leave":
				case "bne.un":
				case "ble.un":
				case "blt.un":
				case "bge.un":
				case "bgt.un":
				case "blt":
				case "ble":
				case "beq":
				case "bge":
				case "bgt":
					Int32 offset = BitConverter.ToInt32(data, loop + 1);
					offset = (loop + 1) + sizeof(UInt32) + offset;
					yield return new MethodLine(line, il, new Int32[] { offset });
					loop += 4;
					break;
				case "ldc.i4.s":
				case "ldloca.s":
				case "stloc.s":
				case "ldloc.s":
					yield return new MethodLine(line, il, (SByte)data[loop + 1]);
					loop++;
					break;
				case "ldc.i4":
					Int32 i4Value = BitConverter.ToInt32(data, loop + 1);
					yield return new MethodLine(line, il, i4Value);
					loop += sizeof(Int32);
					break;
				case "ldc.i8":
					Int64 i8Value = BitConverter.ToInt64(data, loop + 1);
					yield return new MethodLine(line, il, i8Value);
					loop += sizeof(Int64);
					break;
				case "ldc.r4":
					Single r4Value = BitConverter.ToSingle(data, loop + 1);
					yield return new MethodLine(line, il, r4Value);
					loop += sizeof(Single);
					break;
				case "ldc.r8":
					Double r8Value = BitConverter.ToDouble(data, loop + 1);
					yield return new MethodLine(line, il, r8Value);
					loop += sizeof(Double);
					break;
				case "br.s":
					yield return new MethodLine(line, il, new Int32[] { loop + 2 + data[loop + 1] });
					loop++;
					break;
				case "blt.s":
				case "bge.s":
				case "beq.s":
				case "bgt.s":
				case "ble.s":
				case "leave.s":
				case "brfalse.s":
				case "brtrue.s":
				case "bgt.un.s":
				case "bge.un.s":
				case "bne.un.s":
				case "ble.un.s":
				case "blt.un.s":
					yield return new MethodLine(line, il, new Int32[] { loop + 2 + (SByte)data[loop + 1] });
					loop++;
					break;
				case "ldarg.s":
				case "starg.s"://Load argument by sequence index
					UInt16 argSequence = data[loop + 1];
					foreach(MemberArgument item in this.Row.ParamList)
						if(item.Sequence == argSequence)
						{
							yield return new MethodLine(line, il, item);
							break;
						}
					loop++;
					break;
				case "ldarga.s"://Load argument by index
					UInt16 argIndex = data[loop + 1];
					UInt16 index = 0;
					foreach(MemberArgument item in this.Row.ParamList)
						if(argIndex == index++)
						{
							yield return new MethodLine(line, il, item);
							break;
						}
					loop++;
					break;
				case "switch":
					UInt32 count = BitConverter.ToUInt32(data, loop + 1);
					Int32 sOffset = loop + sizeof(UInt32);//CHECK: Loop points to instruction here
					Int32[] offsets = new Int32[count];
					for(Int32 o = 0; o < offsets.Length; o++)
					{
						offsets[o] = BitConverter.ToInt32(data, sOffset);
						sOffset += sizeof(Int32);
					}
					yield return new MethodLine(line, il, offsets);
					loop = sOffset;
					break;
				case "ldstr":
					Int32 rawValue1 = BitConverter.ToInt32(data, loop + 1);
					Int32 usOffset = rawValue1 & 0xFFFFFF;
					String str = cell.Table.Root.Parent.USHeap[usOffset];
					yield return new MethodLine(line, il, str);
					loop += 4;
					break;
				case "conv.i":
				case "conv.i1":
				case "conv.i2":
				case "conv.i4":
				case "conv.i8":
				case "conv.u":
				case "conv.u1":
				case "conv.u2":
				case "conv.u4":
				case "conv.u8":
				case "conv.r4":
				case "conv.r8":
				case "conv.ovf.i":
				case "conv.r.un":
				case "ldnull":
				case "ldlen":
				case "ldind.i1":
				case "ldind.i2":
				case "ldind.i4":
				case "ldind.i8":
				case "ldind.u1":
				case "ldind.u2":
				case "ldind.u4":
				case "ldind.r4":
				case "ldind.r8":
				case "ldind.ref":
				case "ldarg.0":
				case "ldarg.1":
				case "ldarg.2":
				case "ldarg.3":
				case "ldc.i4.0":
				case "ldc.i4.1":
				case "ldc.i4.2":
				case "ldc.i4.3":
				case "ldc.i4.4":
				case "ldc.i4.5":
				case "ldc.i4.6":
				case "ldc.i4.7":
				case "ldc.i4.8":
				case "ldc.i4.m1":
				case "ldloc.0":
				case "ldloc.1":
				case "ldloc.2":
				case "ldloc.3":
				case "ldelem.i1":
				case "ldelem.i2":
				case "ldelem.i4":
				case "ldelem.i8":
				case "ldelem.u1":
				case "ldelem.u2":
				case "ldelem.u4":
				case "ldelem.r4":
				case "ldelem.r8":
				case "ldelem.ref":
				case "stloc.0":
				case "stloc.1":
				case "stloc.2":
				case "stloc.3":
				case "stelem.i1":
				case "stelem.i2":
				case "stelem.i4":
				case "stelem.i8":
				case "stelem.r4":
				case "stelem.r8":
				case "stelem.ref":
				case "stind.i1":
				case "stind.i2":
				case "stind.i4":
				case "stind.i8":
				case "stind.r4":
				case "stind.r8":
				case "stind.ref":
				case "add":
				case "add.ovf":
				case "shr":
				case "shr.un":
				case "div":
				case "div.un":
				case "cgt":
				case "cgt.un":
				case "clt":
				case "clt.un":
				case "rem":
				case "rem.un":
				case "mul":
				case "mul.ovf":
				case "ret":
				case "nop":
				case "ceq":
				case "dup":
				case "neg":
				case "xor":
				case "and":
				case "or":
				case "not":
				case "pop":
				case "shl":
				case "volatile.":
				case "sub":
				case "endfinally":
				case "endfilter":
				case "throw":
				case "rethrow":
					//if(il.OpCodeType)
					yield return new MethodLine(line, il);
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