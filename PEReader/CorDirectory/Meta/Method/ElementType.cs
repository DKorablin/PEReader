using System;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Type of method element type</summary>
	public struct ElementType
	{
		/// <summary>Common language runtime Type</summary>
		public Cor.ELEMENT_TYPE Type { get; internal set; }

		/// <summary>A single-dimensional, zero lower-bound array type modifier</summary>
		public Boolean IsArray { get; internal set; }

		/// <summary>As ingle-dimentional or multi-dimentional array type modifier</summary>
		public Byte MultiArray { get; internal set; }

		/// <summary>A pointer type modifier</summary>
		public Boolean IsPointer { get; internal set; }

		/// <summary>A reference type modifier</summary>
		public Boolean IsByRef { get;internal set; }

		/// <summary>If the CustomModifier is tagged <see cref="Cor.ELEMENT_TYPE.CMOD_REQ"/>, any importing compiler shall ‘understand’ the semantic implied by this CustomModifier in order to reference the surrounding Signature.</summary>
		public Boolean IsRequiredModifier { get; internal set; }

		/// <summary>This element contains generic type(s)</summary>
		public ElementType[] GenericArguments { get; }

		/// <summary>Raw value referenced to meta table</summary>
		public UInt32? RawPointer { get; private set; }

		/// <summary>An index into a TypeDef, a TypeRef or TypeSpec table; more precisely, a TypeDefOrRef coded index</summary>
		public CellPointerBase TypeDefOrRef { get; private set; }

		internal ElementType(MetaCell cell, Byte[] signature, ref UInt32 offset)
		{
			this.TypeDefOrRef = null;
			this.RawPointer = null;

			this.Type = (Cor.ELEMENT_TYPE)signature[offset++];

			this.IsRequiredModifier = this.Type == Cor.ELEMENT_TYPE.CMOD_REQ;
			if(this.IsRequiredModifier)
			{
				UInt32 rawValue1 = (UInt32)NativeMethods.GetPackedValue(signature, offset, out Int32 padding);
				this.TypeDefOrRef = new CellPointerBase(cell, rawValue1);
				this.RawPointer = rawValue1;
				offset += (UInt32)padding;

				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];
			}

			this.IsByRef = this.Type == Cor.ELEMENT_TYPE.BYREF;
			if(this.IsByRef)
				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];

			this.MultiArray = 0;
			this.IsArray = this.Type == Cor.ELEMENT_TYPE.SZARRAY;
			if(this.IsArray)
				do
				{
					this.MultiArray++;
					this.Type = (Cor.ELEMENT_TYPE)signature[offset++];
				} while(this.Type == Cor.ELEMENT_TYPE.SZARRAY);

			Boolean isGeneric = this.Type == Cor.ELEMENT_TYPE.GENERICINST;
			if(isGeneric)
				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];

			this.IsPointer = this.Type == Cor.ELEMENT_TYPE.PTR;
			if(this.IsPointer)
				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];

			switch(this.Type)
			{
			case Cor.ELEMENT_TYPE.CLASS:// Followed by TypeDef or TypeRef token
			case Cor.ELEMENT_TYPE.VALUETYPE:// Followed by TypeDef or TypeRef token
			case Cor.ELEMENT_TYPE.CMOD_REQ:// Required modifier : followed by a TypeDef or TypeRef token
				UInt32 rawValue1 = (UInt32)NativeMethods.GetPackedValue(signature, offset, out Int32 padding);
				this.TypeDefOrRef = new CellPointerBase(cell, rawValue1);
				this.RawPointer = rawValue1;
				offset += (UInt32)padding;
				break;
			case Cor.ELEMENT_TYPE.MVAR:
			case Cor.ELEMENT_TYPE.VAR:
				UInt32 genericArgumentIndex = (UInt32)NativeMethods.GetPackedValue(signature, offset, out Int32 padding2);
				this.TypeDefOrRef = null;
				this.RawPointer = genericArgumentIndex;
				offset += (UInt32)padding2;
				break;
			}

			if(isGeneric)
			{
				Byte argsCount = signature[offset++];
				this.GenericArguments = new ElementType[argsCount];
				for(Byte loop = 0; loop < argsCount; loop++)
					this.GenericArguments[loop] = new ElementType(cell, signature, ref offset);
			} else
				this.GenericArguments = null;
		}

		/// <summary>Format element type as string representation</summary>
		/// <returns></returns>
		public override String ToString()
			=> this.ToStringTyped();

		/// <summary>Formats element as it storead in CLI</summary>
		/// <returns>String representation</returns>
		public String ToStringOriginal()
		{
			String arr = (this.IsArray ? String.Join(String.Empty, Array.ConvertAll(new Object[this.MultiArray], o => { return "[]"; })) : String.Empty);
			String ptr = (this.IsPointer ? "*" : String.Empty);
			String generic = this.GenericArguments == null
				? String.Empty
				: "<" + String.Join(", ", Array.ConvertAll(this.GenericArguments, a => a.ToStringOriginal())) + ">";

			return $"{{{this.Type}}}{generic}{arr}{ptr}";
		}

		/// <summary>Formats element more precisely to .NET code</summary>
		/// <returns>String representation</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public String ToStringTyped()
		{
			String sType;
			switch(this.Type)
			{
			case Cor.ELEMENT_TYPE.OBJECT:
				sType = typeof(Object).Name;
				break;
			case Cor.ELEMENT_TYPE.BOOLEAN:
				sType = typeof(Boolean).Name;
				break;
			case Cor.ELEMENT_TYPE.STRING:
				sType = typeof(String).Name;
				break;
			case Cor.ELEMENT_TYPE.I:
				sType = typeof(IntPtr).Name;
				break;
			case Cor.ELEMENT_TYPE.I2:
				sType = typeof(Int16).Name;
				break;
			case Cor.ELEMENT_TYPE.I4:
				sType = typeof(Int32).Name;
				break;
			case Cor.ELEMENT_TYPE.I8:
				sType = typeof(Int64).Name;
				break;
			case Cor.ELEMENT_TYPE.U:
				sType = typeof(UIntPtr).Name;
				break;
			case Cor.ELEMENT_TYPE.U1:
				sType = typeof(Byte).Name;
				break;
			case Cor.ELEMENT_TYPE.U2:
				sType = typeof(UInt16).Name;
				break;
			case Cor.ELEMENT_TYPE.U4:
				sType = typeof(UInt32).Name;
				break;
			case Cor.ELEMENT_TYPE.U8:
				sType = typeof(UInt64).Name;
				break;
			case Cor.ELEMENT_TYPE.VOID:
				sType = typeof(void).Name;
				break;
			case Cor.ELEMENT_TYPE.CLASS:
			case Cor.ELEMENT_TYPE.VALUETYPE:
				switch(this.TypeDefOrRef.TableType)
				{
				case Cor.MetaTableType.TypeDef:
					TypeDefRow typeDef = this.TypeDefOrRef.GetTargetRowTyped<TypeDefRow>();
					sType = typeDef.TypeNamespace == String.Empty
						? typeDef.TypeName
						: String.Join(".", new String[] { typeDef.TypeNamespace, typeDef.TypeName });
					break;
				case Cor.MetaTableType.TypeRef:
					TypeRefRow typeRef = this.TypeDefOrRef.GetTargetRowTyped<TypeRefRow>();
					sType = typeRef.TypeNamespace == String.Empty
						? typeRef.TypeName
						: String.Join(".", new String[] { typeRef.TypeNamespace, typeRef.TypeName });
					break;
				case Cor.MetaTableType.TypeSpec:
					TypeSpecRow typeSpec = this.TypeDefOrRef.GetTargetRowTyped<TypeSpecRow>();
					sType = this.Type.ToString() + ".???.TYPESPEC.???";//TODO: Need to validate it
					break;
				default:
					throw new InvalidOperationException();
				}
				break;
			case Cor.ELEMENT_TYPE.MVAR:
				sType = "!" + this.RawPointer;
				break;
			case Cor.ELEMENT_TYPE.VAR:
				goto default;
			default:
				sType = this.Type.ToString();
				break;
			}
			String generic = this.GenericArguments == null
				? String.Empty
				: "<" + String.Join(", ", Array.ConvertAll(this.GenericArguments, a => a.ToStringTyped())) + ">";
			return sType
			+ generic
				+ (this.IsArray ? String.Join(String.Empty, Array.ConvertAll(new Object[this.MultiArray], o => { return "[]"; })) : String.Empty)
				+ (this.IsByRef ? "&" : String.Empty)
				+ (this.IsPointer ? "*" : String.Empty);
		}
	}
}