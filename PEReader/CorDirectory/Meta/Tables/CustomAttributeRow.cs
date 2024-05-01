using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The CustomAttribute table stores data that can be used to instantiate a Custom Attribute
	/// (more precisely, an object of the specified Custom Attribute class) at runtime.
	/// The column called Type is slightly misleading -
	/// it actually indexes a constructor method - the owner of that constructor method
	/// is the Type of the Custom Attribute
	/// </summary>
	public class CustomAttributeRow : BaseMetaRow
	{//TODO: Need additional checkup. Maybe Type and Parent misplaced.
		private MemberArgument[] _fixedArgs;
		private MemberElement[] _namedArgs;

		/// <summary>An index into a metadata table that has an associated HasCustomAttribute (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Parent => base.GetValue<MetaCellCodedToken>(0);

		/// <summary>An index into the MethodDef or MemberRef table; more precisely, a CustomAttributeType (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Type => base.GetValue<MetaCellCodedToken>(1);

		/// <summary>Attribute value</summary>
		/// <remarks>All binary values are stored in little-endian format (except for PackedLen items, which are used only as a count for the number of bytes to follow in a UTF8 string)</remarks>
		public Byte[] Value => base.GetValue<Byte[]>(2);

		/// <summary>Description of the fixed arguments for the constructor method</summary>
		/// <remarks>
		/// Their number and type is found by examining that constructor‘s row in the MethodDef table;
		/// this information is not repeated in the 192 Partition II CustomAttrib itself.
		/// </remarks>
		public IEnumerable<MemberArgument> FixedArgs
		{
			get
			{
				if(this._fixedArgs == null)
					this.ParseSignature(out this._fixedArgs, out this._namedArgs);
				return this._fixedArgs;
			}
		}

		/// <summary>Description of the optional "named" fields and properties</summary>
		public IEnumerable<MemberElement> NamedArgs
		{
			get
			{
				if(this._namedArgs == null)
					this.ParseSignature(out this._fixedArgs, out this._namedArgs);
				return this._namedArgs;
			}
		}

		private void ParseSignature(out MemberArgument[] args, out MemberElement[] members)
		{
			List<MemberArgument> lArgs = new List<MemberArgument>();
			List<MemberElement> lMembers = new List<MemberElement>();

			Byte[] signature = this.Value;
			UInt32 offset = 0;
			UInt16 prolog = BitConverter.ToUInt16(signature, (Int32)offset);
			if(prolog != 0x0001)
				throw new InvalidOperationException($"CustomAttrib starts with a Prolog – an unsigned int16, with value 0x0001. Real value: {prolog}");
			offset += sizeof(UInt16);

			switch(this.Type.TableType)
			{
			case Cor.MetaTableType.MethodDef:
				MethodDefRow row1 = this.Type.GetTargetRowTyped<MethodDefRow>();
				foreach(MemberArgument arg in row1.ParamList)
				{
					if(arg.ParamRow == null)//ParamRow can be null here
						lArgs.Add(new MemberArgument(arg.Type, arg.Name, arg.Sequence) { Flags = arg.Flags, });
					else
						lArgs.Add(new MemberArgument(arg.Type, arg.ParamRow) { Flags = arg.Flags, });
				}
				break;
			case Cor.MetaTableType.MemberRef:
				MemberRefRow row2 = this.Type.GetTargetRowTyped<MemberRefRow>();
				UInt16 sequence = 0;
				foreach(ElementType element in row2.ArgsType)
					lArgs.Add(new MemberArgument(element, null, sequence++));
				break;
			default:
				throw new NotSupportedException($"Their number and type is found by examining that constructor‘s row in the MethodDef or MethodDef tables. Real value: {this.Type.TableType}");
			}

			foreach(MemberArgument arg in lArgs)
			{
				Object value = this.ParseTypedValue(arg.Type, signature,out String enumType, ref offset);
				arg.EnumType = enumType;
				arg.Value = value;
			}

			UInt16 numNamed = BitConverter.ToUInt16(signature, (Int32)offset);
			offset += sizeof(UInt16);
			for(UInt16 loop = 0; loop < numNamed; loop++)
			{
				Cor.ELEMENT_TYPE type = (Cor.ELEMENT_TYPE)signature[offset++];
				switch(type)
				{
				case Cor.ELEMENT_TYPE.PROPERTY:
				case Cor.ELEMENT_TYPE.FIELD:
					ElementType element = new ElementType(this.Row.Cells[0], signature, ref offset);

					String enumTypeName;
					String elementName;
					Object elementValue;
					switch(element.Type)
					{
					case Cor.ELEMENT_TYPE.ENUM:
						enumTypeName = (String)ParseTypedValue(Cor.ELEMENT_TYPE.STRING, signature, ref offset);
						elementName = (String)ParseTypedValue(Cor.ELEMENT_TYPE.STRING, signature, ref offset);
						elementValue = ParseTypedValue(this.FindEnumBaseType(enumTypeName), signature, ref offset);
						break;
					default:
						enumTypeName = null;
						elementName = (String)ParseTypedValue(Cor.ELEMENT_TYPE.STRING, signature, ref offset);
						elementValue = this.ParseTypedValue(element, signature, out enumTypeName, ref offset);
						break;
					}
					
					lMembers.Add(new MemberElement(type, element, elementName) { EnumType = enumTypeName, Value = elementValue});
					break;
				default:
					throw new InvalidOperationException($"Attribute value can contain only {Cor.ELEMENT_TYPE.PROPERTY} or {Cor.ELEMENT_TYPE.FIELD}. Got: {type}");
				}
			}
			if(offset != signature.Length)
			{
				Exception exc = new CustomAttributeFormatException($"Binary format of the specified custom attribute was invalid. Signature length: {signature.Length:N0} End position:{offset:N0}");
				exc.Data.Add("Signature", signature);
				throw exc;
			}

			args = lArgs.ToArray();
			members = lMembers.ToArray();
		}

		private Object ParseTypedValue(ElementType type, Byte[] value, out String enumTypeName, ref UInt32 offset)
		{
			enumTypeName = null;

			if(type.IsArray)
			{//TODO: We will nedd to parse subsequent bytes with respect of types described int the ELSE block lower
				UInt32 NumElem = BitConverter.ToUInt32(value, (Int32)offset);
				offset += sizeof(UInt32);
				Object[] values = new Object[NumElem];
				for(UInt32 loop = 0; loop < NumElem; loop++)
					values[loop] = ParseTypedValue(type.Type, value, ref offset);
				return values;
			} else
			{
				switch(type.Type)
				{
				case Cor.ELEMENT_TYPE.OBJECT:
				case Cor.ELEMENT_TYPE.BOXED://Yup. Both OBJECT and BOXED can contains ENUM type
					ElementType objType = new ElementType(this.Row.Cells[0], value, ref offset);
					return this.ParseTypedValue(objType, value, out enumTypeName, ref offset);
				case Cor.ELEMENT_TYPE.ENUM:
					enumTypeName = (String)ParseTypedValue(Cor.ELEMENT_TYPE.STRING, value, ref offset);
					return ParseTypedValue(this.FindEnumBaseType(enumTypeName), value, ref offset);
				default:
					return ParseTypedValue(type.Type, value, ref offset);
				}
			}
		}

		private Cor.ELEMENT_TYPE FindEnumBaseType(String enumTypeName)
		{
			if(enumTypeName.IndexOf(',') > -1)
			{
				//Enum stored as: {.NET Type} + {Property/Field name} + {EnumValue (Int32)} - We can get size of enum only from TypeDef otherwise we have to load external assembly...
				//BUG: We can't read size of VALUETYPE if ENUM defined in external assembly
				return Cor.ELEMENT_TYPE.I4;
			} else
			{
				foreach(TypeDefRow typeDef in this.Row.Table.Root.TypeDef)
					if(new TypeReader(typeDef).FullName == enumTypeName)
					{
						foreach(FieldRow field in typeDef.FieldList)
							if(field.Name == "value__" && field.ReturnType.Type != Cor.ELEMENT_TYPE.VALUETYPE)
								return field.ReturnType.Type;//TODO: We need to check field type related to the official docs
					}
				throw new NotSupportedException($"Valuetype for enum {enumTypeName} is required to read value for attribute argument value but it's not found");
			}
		}

		private static Object ParseTypedValue(Cor.ELEMENT_TYPE type, Byte[] value, ref UInt32 offset)
		{
			Object result;
			switch(type)
			{
			case Cor.ELEMENT_TYPE.STRING:
				if(value[offset] == 0xff)
				{
					result = null;
					offset += 1;
				} else
					goto case Cor.ELEMENT_TYPE.CLASS;
				break;
			case Cor.ELEMENT_TYPE.CLASS:
			case Cor.ELEMENT_TYPE.TYPE:
			case Cor.ELEMENT_TYPE.BOXED:
				UInt32 packedLength1 = NativeMethods.GetPackedValue(value, ref offset);
				result = Encoding.UTF8.GetString(value, (Int32)offset, (Int32)packedLength1);
				offset += packedLength1;
				break;
			case Cor.ELEMENT_TYPE.BOOLEAN:
				result = BitConverter.ToBoolean(value, (Int32)offset);
				offset += sizeof(Boolean);
				break;
			case Cor.ELEMENT_TYPE.U1:
				result = value[offset];
				offset += sizeof(Byte);
				break;
			case Cor.ELEMENT_TYPE.I2:
				result = BitConverter.ToInt16(value, (Int32)offset);
				offset += sizeof(Int16);
				break;
			case Cor.ELEMENT_TYPE.I4:
				result = BitConverter.ToInt32(value, (Int32)offset);
				offset += sizeof(Int32);
				break;
			case Cor.ELEMENT_TYPE.I8:
				result = BitConverter.ToInt64(value, (Int32)offset);
				offset += sizeof(Int64);
				break;
			case Cor.ELEMENT_TYPE.R4:
				result = BitConverter.ToSingle(value, (Int32)offset);
				offset += sizeof(Single);
				break;
				case Cor.ELEMENT_TYPE.R8:
				result = BitConverter.ToDouble(value, (Int32)offset);
				offset += sizeof(Double);
				break;
			case Cor.ELEMENT_TYPE.VALUETYPE://TODO: Not all values are stored as Int32. We need to find different approach to get enum value length
				result = BitConverter.ToInt32(value, (Int32)offset);
				offset += sizeof(Int32);
				break;
			case Cor.ELEMENT_TYPE.OBJECT://Firstly we need to get real object type. Parsed in the different method
				goto default;
			case Cor.ELEMENT_TYPE.ENUM://Here lies not only value but type of object. Parsed in the different method
				goto default;
			default:
				throw new NotImplementedException($"Parse for {type} is not implemented");
			}
			return result;
		}

		/// <summary>Create instance of custom attribute description row</summary>
		public CustomAttributeRow()
			: base(Cor.MetaTableType.CustomAttribute)
		{
		}
	}
}