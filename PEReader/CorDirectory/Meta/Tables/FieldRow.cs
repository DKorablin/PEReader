using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Each row in the Field table results from a top-level .field directive (§II.5.10),
	/// or a .field directive inside a Type (§II.10.2). (For an example, see §II.14.5.).
	/// </summary>
	/// <remarks>
	/// Conceptually, each row in the Field table is owned by one, and only one, row in the TypeDef table.
	/// However, the owner of any row in the Field table is not stored anywhere in the Field table itself.
	/// </remarks>
	public class FieldRow : BaseMetaRow
	{
		/// <summary>A 4-byte bit mask of type TypeAttributes</summary>
		public FieldAttributes Flags => (FieldAttributes)base.GetValue<UInt16>(0);

		/// <summary>The FieldAccessMask subfield of Flags shall contain precisely one of CompilerControlled, Private, FamANDAssem, Assembly, Family, FamORAssem, or Public</summary>
		public FieldAttributes FieldAccessFlags => this.Flags & FieldAttributes.FieldAccessMask;

		/// <summary>Field Name</summary>
		public String Name => base.GetValue<String>(1);

		/// <summary>Banana</summary>
		public Byte[] Signature => base.GetValue<Byte[]>(2);

		/// <summary>First byte of signature</summary>
		public CorSignature FieldSig => (CorSignature)this.Signature[0];

		/// <summary>Field type</summary>
		public ElementType ReturnType
		{
			get
			{
				UInt32 offset = 1;
				return new ElementType(this.Row.Cells[0], this.Signature, ref offset);
			}
		}

		/// <summary>Create instance of Field row</summary>
		public FieldRow()
			: base(Cor.MetaTableType.Field) { }
	}
}