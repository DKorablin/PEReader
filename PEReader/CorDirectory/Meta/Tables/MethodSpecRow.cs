using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Generic method instantiation descriptors</summary>
	public class MethodSpecRow : BaseMetaRow
	{
		/// <summary>
		/// An index into the <see cref="Cor.MetaTableType.MethodDef"/> or <see cref="Cor.MetaTableType.MemberRef"/> table, specifying to which generic method this row refers;
		/// that is, which generic method this row is an instantiation of;
		/// more precisely, a MethodDefOrRef (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Method => this.GetValue<MetaCellCodedToken>(0);

		/// <summary>Signature of this instantiation</summary>
		public Byte[] Instantiation => this.GetValue<Byte[]>(1);

		/// <summary>Generic method arguments</summary>
		public IEnumerable<ElementType> GenArgs
		{
			get
			{
				Byte[] signature = this.Instantiation;
				UInt32 offset = 0;
				Cor.IMAGE_CEE_CS signatureFlags = (Cor.IMAGE_CEE_CS)signature[offset++];
				if(signatureFlags != Cor.IMAGE_CEE_CS.GENERICINST)
					throw new InvalidOperationException($"Signature starts from {Cor.IMAGE_CEE_CS.GENERICINST} field. Got: 0x{signature[0]:X}");

				UInt32 genArgCount = NativeMethods.GetPackedValue(signature, offset, out Int32 padding);
				offset += (UInt32)padding;
				for(Int32 loop = 0; loop < genArgCount; loop++)
					yield return new ElementType(this.Row.Cells[0], signature, ref offset);
			}
		}

		/// <summary>Create instance of <see cref="MethodSpecRow"/></summary>
		public MethodSpecRow()
			: base(Cor.MetaTableType.MethodSpec) { }
	}
}