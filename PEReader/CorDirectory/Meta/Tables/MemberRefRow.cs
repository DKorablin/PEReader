using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The MemberRef table combines two sorts of references, to Methods and to Fields of a class, known as "MethodRef" and "FieldRef", respectively.
	/// </summary>
	/// <remarks>
	/// An entry is made into the MemberRef table whenever a reference is made in the CIL code to a method or field which is defined in another module or assembly.
	/// (Also, an entry is made for a call to a method with a VARARG signature, even when it is defined in the same module as the call site.)
	/// </remarks>
	public class MemberRefRow : BaseMetaRow
	{
		private SignatureParser _signatureI;

		/// <summary>
		/// An index into the MethodDef, ModuleRef, TypeDef, TypeRef, or TypeSpec tables;
		/// more precisely, a MemberRefParent (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Class { get { return base.GetValue<MetaCellCodedToken>(0); } }

		/// <summary>Memeber name</summary>
		public String Name { get { return base.GetValue<String>(1); } }

		/// <summary>Member arguments and calling convention description</summary>
		public Byte[] Signature { get { return base.GetValue<Byte[]>(2); } }

		/// <summary>Strongly typed method signature with return type, calling convention and arguments types</summary>
		private SignatureParser SignatureI
		{
			get { return this._signatureI ?? (this._signatureI = new SignatureParser(this.Signature)); }
		}

		/// <summary>Calling convention that are made in managed code for current method</summary>
		public Cor.IMAGE_CEE_CS CorCallingConvention
		{
			get { return this._signatureI.CorCallingConvention; }
		}

		/// <summary>Count of method args</summary>
		public Int32 ArgsCount { get { return this.SignatureI.ArgsCount; } }

		/// <summary>Return method type</summary>
		public ElementType ReturnType { get { return this.SignatureI.ReturnType; } }

		/// <summary>Arguments types</summary>
		public ElementType[] ArgsType
		{
			get
			{
				return this.SignatureI.ArgumentsTypes;
			}
		}

		/// <summary>Create instance of Memeber reference row</summary>
		public MemberRefRow()
			: base(Cor.MetaTableType.MemberRef) { }
	}
}