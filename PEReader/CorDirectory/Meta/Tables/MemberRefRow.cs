using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The MemberRef table combines two sorts of references, to Methods and to Fields of a class,
	/// known as "MethodRef" and "FieldRef", respectively.
	/// </summary>
	/// <remarks>
	/// An entry is made into the MemberRef table whenever a reference
	/// is made in the CIL code to a method or field which is defined in another module or assembly.
	/// (Also, an entry is made for a call to a method with a VARARG signature,
	/// even when it is defined in the same module as the call site.)
	/// </remarks>
	public class MemberRefRow : BaseMetaRow
	{
		/// <summary>
		/// An index into the MethodDef, ModuleRef, TypeDef, TypeRef, or TypeSpec tables;
		/// more precisely, a MemberRefParent (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Class { get { return base.GetValue<MetaCellCodedToken>(0); } }

		/// <summary>Memeber name</summary>
		public String Name { get { return base.GetValue<String>(1); } }

		/// <summary>Banana</summary>
		public Byte[] Signature { get { return base.GetValue<Byte[]>(2); } }
	}
}