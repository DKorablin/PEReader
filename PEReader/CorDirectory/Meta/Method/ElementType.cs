using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Type of method element type</summary>
	/// <remarks>
	/// Parses the Type/Param/RetType/LocalVar grammar from ECMA-335 II.23.2.10-II.23.2.12.
	/// Type constructors (BYREF, PTR, SZARRAY, ARRAY), custom modifiers, PINNED and SENTINEL may be combined and nested in any order.
	/// </remarks>
	[DebuggerDisplay("TypeName={TypeName}")]
	public struct ElementType
	{
		/// <summary>Common language runtime Type (innermost type after all type constructors and modifiers are removed)</summary>
		public Cor.ELEMENT_TYPE Type { get; }

		/// <summary>A single-dimensional (<see cref="Cor.ELEMENT_TYPE.SZARRAY"/>) or general (<see cref="Cor.ELEMENT_TYPE.ARRAY"/>) array type modifier is present</summary>
		public Boolean IsArray => this.MultiArray > 0;

		/// <summary>Count of nested array type modifiers (<see cref="Cor.ELEMENT_TYPE.SZARRAY"/> and <see cref="Cor.ELEMENT_TYPE.ARRAY"/>)</summary>
		public Byte MultiArray { get; }

		/// <summary>Count of nested unmanaged pointer type modifiers (<see cref="Cor.ELEMENT_TYPE.PTR"/>). Ex: 2 for <c>T**</c></summary>
		public Byte PointerDepth { get; }

		/// <summary>A pointer type modifier</summary>
		public Boolean IsPointer => this.PointerDepth > 0;

		/// <summary>A reference type modifier</summary>
		public Boolean IsByRef { get; }

		/// <summary>Local variable that points at a pinned object (<see cref="Cor.ELEMENT_TYPE.PINNED"/>)</summary>
		public Boolean IsPinned { get; }

		/// <summary>This element is the first of the variable arguments in vararg call site signature (<see cref="Cor.ELEMENT_TYPE.SENTINEL"/>)</summary>
		public Boolean IsSentinel { get; }

		/// <summary>If the CustomModifier is tagged <see cref="Cor.ELEMENT_TYPE.CMOD_REQ"/>, any importing compiler shall ‘understand’ the semantic implied by this CustomModifier in order to reference the surrounding Signature.</summary>
		public Boolean IsRequiredModifier { get; }

		/// <summary>All custom modifiers (modreq/modopt) attached to this element or null if there are no modifiers</summary>
		public CustomModifier[] CustomModifiers { get; }

		/// <summary>
		/// Type constructors applied to <see cref="Type"/> in signature order (outermost first):
		/// <see cref="Cor.ELEMENT_TYPE.BYREF"/>, <see cref="Cor.ELEMENT_TYPE.PTR"/>, <see cref="Cor.ELEMENT_TYPE.SZARRAY"/>, <see cref="Cor.ELEMENT_TYPE.ARRAY"/>.
		/// Null if there are no type constructors.
		/// </summary>
		public Cor.ELEMENT_TYPE[] TypeModifiers { get; }

		/// <summary>Shapes of each <see cref="Cor.ELEMENT_TYPE.ARRAY"/> entry in <see cref="TypeModifiers"/> (in the same order) or null</summary>
		public ArrayShape[] ArrayShapes { get; }

		/// <summary>This element contains generic type(s)</summary>
		public ElementType[] GenericArguments { get; }

		/// <summary>Function pointer signature if <see cref="Type"/> is <see cref="Cor.ELEMENT_TYPE.FNPTR"/></summary>
		public FunctionPointerSignature FunctionPointer { get; }

		/// <summary>Raw value referenced to meta table or generic parameter index for <see cref="Cor.ELEMENT_TYPE.VAR"/> and <see cref="Cor.ELEMENT_TYPE.MVAR"/></summary>
		public UInt32? RawPointer { get; }

		/// <summary>An index into a TypeDef, a TypeRef or TypeSpec table; more precisely, a TypeDefOrRef coded index</summary>
		public CellPointerBase TypeDefOrRef { get; }

		internal ElementType(MetaCell cell, Byte[] signature, ref UInt32 offset)
		{
			List<CustomModifier> customModifiers = null;
			List<Cor.ELEMENT_TYPE> typeModifiers = null;
			Boolean isByRef = false;
			Boolean isPinned = false;
			Boolean isSentinel = false;
			Boolean isRequiredModifier = false;
			Boolean isGeneric = false;
			Int32 arraysCount = 0;
			Int32 pointersCount = 0;
			Int32 arrayShapesCount = 0;

			// Prefixes: custom modifiers, PINNED, SENTINEL, type constructors and GENERICINST
			Cor.ELEMENT_TYPE type;
			Boolean isPrefix;
			do
			{
				type = (Cor.ELEMENT_TYPE)signature[offset++];
				isPrefix = true;
				switch(type)
				{
				case Cor.ELEMENT_TYPE.CMOD_REQ:// Required modifier : followed by a TypeDef or TypeRef token
				case Cor.ELEMENT_TYPE.CMOD_OPT:// Optional modifier : followed by a TypeDef or TypeRef token
					Boolean isRequired = type == Cor.ELEMENT_TYPE.CMOD_REQ;
					isRequiredModifier |= isRequired;
					UInt32 modifierToken = NativeMethods.GetPackedValue(signature, ref offset);
					if(customModifiers == null)
						customModifiers = new List<CustomModifier>();
					customModifiers.Add(new CustomModifier(cell, isRequired, modifierToken));
					break;
				case Cor.ELEMENT_TYPE.PINNED:
					isPinned = true;
					break;
				case Cor.ELEMENT_TYPE.SENTINEL:
					isSentinel = true;
					break;
				case Cor.ELEMENT_TYPE.BYREF:
				case Cor.ELEMENT_TYPE.PTR:
				case Cor.ELEMENT_TYPE.SZARRAY:
				case Cor.ELEMENT_TYPE.ARRAY:// ARRAY Type ArrayShape - the shape follows the element type
					if(type == Cor.ELEMENT_TYPE.BYREF)
						isByRef = true;
					else if(type == Cor.ELEMENT_TYPE.PTR)
						pointersCount++;
					else
					{
						arraysCount++;
						if(type == Cor.ELEMENT_TYPE.ARRAY)
							arrayShapesCount++;
					}

					if(typeModifiers == null)
						typeModifiers = new List<Cor.ELEMENT_TYPE>();
					typeModifiers.Add(type);
					break;
				case Cor.ELEMENT_TYPE.GENERICINST:// GENERICINST (CLASS | VALUETYPE) TypeDefOrRefOrSpecEncoded GenArgCount Type*
					isGeneric = true;
					break;
				default:
					isPrefix = false;
					break;
				}
			} while(isPrefix);

			// Element type payload
			UInt32? rawPointer = null;
			CellPointerBase typeDefOrRef = null;
			FunctionPointerSignature functionPointer = null;
			switch(type)
			{
			case Cor.ELEMENT_TYPE.CLASS:// Followed by TypeDef or TypeRef token
			case Cor.ELEMENT_TYPE.VALUETYPE:// Followed by TypeDef or TypeRef token
				UInt32 typeToken = NativeMethods.GetPackedValue(signature, ref offset);
				typeDefOrRef = new CellPointerBase(cell, typeToken);
				rawPointer = typeToken;
				break;
			case Cor.ELEMENT_TYPE.MVAR:
			case Cor.ELEMENT_TYPE.VAR:
				rawPointer = NativeMethods.GetPackedValue(signature, ref offset);
				break;
			case Cor.ELEMENT_TYPE.FNPTR:// Followed by full method signature
				functionPointer = new FunctionPointerSignature(cell, signature, ref offset);
				break;
			}

			ElementType[] genericArguments = null;
			if(isGeneric)
			{
				UInt32 argsCount = NativeMethods.GetPackedValue(signature, ref offset);
				genericArguments = new ElementType[argsCount];
				for(UInt32 loop = 0; loop < argsCount; loop++)
					genericArguments[loop] = new ElementType(cell, signature, ref offset);
			}

			// Array shapes follow the element type, innermost array first
			ArrayShape[] arrayShapes = null;
			if(arrayShapesCount > 0)
			{
				arrayShapes = new ArrayShape[arrayShapesCount];
				for(Int32 loop = arrayShapesCount - 1; loop >= 0; loop--)
					arrayShapes[loop] = new ArrayShape(signature, ref offset);
			}

			this.Type = type;
			this.MultiArray = checked((Byte)arraysCount);
			this.PointerDepth = checked((Byte)pointersCount);
			this.IsByRef = isByRef;
			this.IsPinned = isPinned;
			this.IsSentinel = isSentinel;
			this.IsRequiredModifier = isRequiredModifier;
			this.CustomModifiers = customModifiers?.ToArray();
			this.TypeModifiers = typeModifiers?.ToArray();
			this.ArrayShapes = arrayShapes;
			this.GenericArguments = genericArguments;
			this.FunctionPointer = functionPointer;
			this.RawPointer = rawPointer;
			this.TypeDefOrRef = typeDefOrRef;
		}

		/// <summary>Formats type constructors in reflection notation (innermost first). Ex: <c>Int32*[]&amp;</c></summary>
		private String FormatTypeModifiers()
		{
			if(this.TypeModifiers == null)
				return String.Empty;

			StringBuilder result = new StringBuilder();
			Int32 shapeIndex = this.ArrayShapes == null ? 0 : this.ArrayShapes.Length;
			for(Int32 loop = this.TypeModifiers.Length - 1; loop >= 0; loop--)
				switch(this.TypeModifiers[loop])
				{
				case Cor.ELEMENT_TYPE.PTR:
					result.Append('*');
					break;
				case Cor.ELEMENT_TYPE.BYREF:
					result.Append('&');
					break;
				case Cor.ELEMENT_TYPE.SZARRAY:
					result.Append("[]");
					break;
				case Cor.ELEMENT_TYPE.ARRAY:
					result.Append(this.ArrayShapes[--shapeIndex].ToString());
					break;
				}
			return result.ToString();
		}

		/// <summary>Formats element as it stored in CLI</summary>
		/// <returns>String representation</returns>
		public String ToStringOriginal()
		{
			String fnPtr = this.FunctionPointer == null
				? String.Empty
				: this.FunctionPointer.Format(a => a.ToStringOriginal());
			String generic = this.GenericArguments == null
				? String.Empty
				: "<" + String.Join(", ", Array.ConvertAll(this.GenericArguments, a => a.ToStringOriginal())) + ">";

			return $"{{{this.Type}}}{fnPtr}{generic}{this.FormatTypeModifiers()}";
		}

		/// <summary>Formats element more precisely to .NET code</summary>
		/// <returns>String representation</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public String TypeName
		{
			get
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
				case Cor.ELEMENT_TYPE.FNPTR:
					sType = this.FunctionPointer.Format(a => a.TypeName);
					break;
				case Cor.ELEMENT_TYPE.VAR:
					goto default;
				default:
					sType = this.Type.ToString();
					break;
				}
				String generic = this.GenericArguments == null
					? String.Empty
					: "<" + String.Join(", ", Array.ConvertAll(this.GenericArguments, a => a.TypeName)) + ">";
				return sType + generic + this.FormatTypeModifiers();
			}
		}
	}

	/// <summary>Custom modifier (modreq / modopt) attached to a type in a signature (ECMA-335 II.23.2.7)</summary>
	[DebuggerDisplay("IsRequired={IsRequired} RawPointer={RawPointer}")]
	public class CustomModifier
	{
		/// <summary>Modifier is tagged with <see cref="Cor.ELEMENT_TYPE.CMOD_REQ"/> (otherwise <see cref="Cor.ELEMENT_TYPE.CMOD_OPT"/>)</summary>
		public Boolean IsRequired { get; }

		/// <summary>Raw TypeDefOrRef coded index of the modifier type</summary>
		public UInt32 RawPointer { get; }

		/// <summary>An index into a TypeDef, a TypeRef or TypeSpec table of the modifier type</summary>
		public CellPointerBase TypeDefOrRef { get; }

		internal CustomModifier(MetaCell cell, Boolean isRequired, UInt32 rawPointer)
		{
			this.IsRequired = isRequired;
			this.RawPointer = rawPointer;
			this.TypeDefOrRef = new CellPointerBase(cell, rawPointer);
		}
	}

	/// <summary>Shape of a general (multi-dimensional) array (ECMA-335 II.23.2.13)</summary>
	public class ArrayShape
	{
		/// <summary>Number of dimensions</summary>
		public UInt32 Rank { get; }

		/// <summary>Sizes of the first N dimensions (may be less than <see cref="Rank"/>)</summary>
		public UInt32[] Sizes { get; }

		/// <summary>Lower bounds of the first N dimensions (may be less than <see cref="Rank"/>)</summary>
		public Int32[] LowerBounds { get; }

		internal ArrayShape(Byte[] signature, ref UInt32 offset)
		{
			this.Rank = NativeMethods.GetPackedValue(signature, ref offset);

			UInt32 numSizes = NativeMethods.GetPackedValue(signature, ref offset);
			this.Sizes = new UInt32[numSizes];
			for(UInt32 loop = 0; loop < numSizes; loop++)
				this.Sizes[loop] = NativeMethods.GetPackedValue(signature, ref offset);

			UInt32 numLoBounds = NativeMethods.GetPackedValue(signature, ref offset);
			this.LowerBounds = new Int32[numLoBounds];
			for(UInt32 loop = 0; loop < numLoBounds; loop++)
			{
				this.LowerBounds[loop] = NativeMethods.GetPackedSignedValue(signature, offset, out Int32 padding);
				offset += (UInt32)padding;
			}
		}

		/// <summary>Formats array shape. Ex: <c>[,]</c>, <c>[*]</c>, <c>[1...5,]</c></summary>
		/// <remarks>Zero lower bound without size is omitted (as in reflection), because compilers emit it for each dimension of <c>T[,]</c></remarks>
		/// <returns>String representation</returns>
		public override String ToString()
		{
			if(this.Rank == 1 && this.Sizes.Length == 0 && this.LowerBounds.Length == 0)
				return "[*]";

			String[] dimensions = new String[this.Rank];
			for(UInt32 loop = 0; loop < this.Rank; loop++)
			{
				Boolean hasSize = loop < this.Sizes.Length;
				Boolean hasLowerBound = loop < this.LowerBounds.Length;
				if(hasSize && hasLowerBound)
					dimensions[loop] = this.LowerBounds[loop] + "..." + (this.LowerBounds[loop] + (Int64)this.Sizes[loop] - 1);
				else if(hasLowerBound && this.LowerBounds[loop] != 0)
					dimensions[loop] = this.LowerBounds[loop] + "...";
				else if(hasSize)
					dimensions[loop] = this.Sizes[loop].ToString();
				else
					dimensions[loop] = String.Empty;
			}
			return "[" + String.Join(",", dimensions) + "]";
		}
	}

	/// <summary>Signature of the function pointer (<see cref="Cor.ELEMENT_TYPE.FNPTR"/>) MethodDefSig or MethodRefSig (ECMA-335 II.23.2.1, II.23.2.2)</summary>
	public class FunctionPointerSignature
	{
		/// <summary>Calling convention including <see cref="Cor.IMAGE_CEE_CS.HASTHIS"/>, <see cref="Cor.IMAGE_CEE_CS.EXPLICITTHIS"/> and <see cref="Cor.IMAGE_CEE_CS.GENERIC"/> flags</summary>
		public Cor.IMAGE_CEE_CS CallingConvention { get; }

		/// <summary>Count of generic parameters if <see cref="Cor.IMAGE_CEE_CS.GENERIC"/> flag is set</summary>
		public UInt32 GenericParametersCount { get; }

		/// <summary>Function return type</summary>
		public ElementType ReturnType { get; }

		/// <summary>Function arguments (vararg arguments are marked with <see cref="ElementType.IsSentinel"/> on the first of them)</summary>
		public ElementType[] Arguments { get; }

		/// <summary>Function pointer uses one of unmanaged calling conventions (C, StdCall, ThisCall, FastCall or Unmanaged)</summary>
		public Boolean IsUnmanaged
		{
			get
			{
				Byte callConv = (Byte)(this.CallingConvention & Cor.IMAGE_CEE_CS.MASK);
				return (callConv >= 0x1 && callConv <= 0x4) || callConv == (Byte)Cor.IMAGE_CEE_CS.UNMGD;
			}
		}

		internal FunctionPointerSignature(MetaCell cell, Byte[] signature, ref UInt32 offset)
		{
			this.CallingConvention = (Cor.IMAGE_CEE_CS)signature[offset++];
			if((this.CallingConvention & Cor.IMAGE_CEE_CS.GENERIC) == Cor.IMAGE_CEE_CS.GENERIC)
				this.GenericParametersCount = NativeMethods.GetPackedValue(signature, ref offset);

			UInt32 paramCount = NativeMethods.GetPackedValue(signature, ref offset);
			this.ReturnType = new ElementType(cell, signature, ref offset);
			this.Arguments = new ElementType[paramCount];
			for(UInt32 loop = 0; loop < paramCount; loop++)
				this.Arguments[loop] = new ElementType(cell, signature, ref offset);
		}

		/// <summary>Formats function pointer in C# notation. Ex: <c>delegate* unmanaged&lt;Int32, Void&gt;</c></summary>
		/// <param name="formatter">Element formatter</param>
		/// <returns>String representation</returns>
		internal String Format(Converter<ElementType, String> formatter)
		{
			String[] types = new String[this.Arguments.Length + 1];
			for(Int32 loop = 0; loop < this.Arguments.Length; loop++)
				types[loop] = (this.Arguments[loop].IsSentinel ? "..., " : String.Empty) + formatter(this.Arguments[loop]);
			types[types.Length - 1] = formatter(this.ReturnType);

			return "delegate*" + (this.IsUnmanaged ? " unmanaged" : String.Empty) + "<" + String.Join(", ", types) + ">";
		}
	}
}