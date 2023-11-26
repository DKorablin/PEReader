using System;

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
			=> ToString(true);

		private String ToString(Boolean withType)
		{
			String arr = (IsArray ? String.Join(String.Empty, Array.ConvertAll(new Object[this.MultiArray], o => { return "[]"; })) : String.Empty);
			String ptr = (IsPointer ? "*" : String.Empty);
			String generic = this.GenericArguments == null
				? String.Empty
				: "<" + String.Join(", ", Array.ConvertAll(this.GenericArguments, a => a.ToString(false))) + ">";

			return withType
				? $"{GetType().Name}: {{{this.Type}}}{generic}{arr}{ptr}"
				: $"{{{this.Type}}}{generic}{arr}{ptr}";
		}
	}
}