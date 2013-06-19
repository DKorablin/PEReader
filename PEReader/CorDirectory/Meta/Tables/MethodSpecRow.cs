using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Generic method instantiation descriptors.</summary>
	public class MethodSpecRow : BaseMetaRow
	{
		/// <summary>
		/// An index into the MethodDef or MemberRef table, specifying to which generic method this row refers;
		/// that is, which generic method this row is an instantiation of;
		/// more precisely, a MethodDefOrRef (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Method { get { return base.GetValue<MetaCellCodedToken>(0); } }
		/// <summary>Signature of this instantiation</summary>
		public Byte[] Instantiation { get { return base.GetValue<Byte[]>(1); } }
	}
}