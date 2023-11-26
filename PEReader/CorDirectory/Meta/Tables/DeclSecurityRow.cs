using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Security attributes, which derive from System.Security.Permissions.SecurityAttribute (see Partition IV), can be attached to a TypeDef, a Method, or an Assembly.
	/// All constructors of this class shall take a System.Security.Permissions.SecurityAction value as their first parameter, describing what should be done with the permission on the type, method or assembly to which it is attached.
	/// Code access security attributes, which derive from System.Security.Permissions.
	/// CodeAccessSecurityAttribute, can have any of the security actions.
	/// </summary>
	public class DeclSecurityRow : BaseMetaRow
	{
		/// <summary>
		/// Action is a 2-byte representation of Security Actions (see System.Security.SecurityAction in Partition IV).
		/// </summary>
		/// <remarks>
		/// The values 0 - 0xFF are reserved for future standards use.
		/// Values 0x20 - 0x7F and 0x100 - 0x07FF are for uses where the action can be ignored if it is not understood or supported.
		/// Values 0x80 - 0xFF and 0x0800 - 0xFFFF are for uses where the action shall be implemented for secure operation;
		/// in implementations where the action is not available, no access to the assembly, type, or method shall be permitted.
		/// </remarks>
		public UInt16 Action => base.GetValue<UInt16>(0);

		/// <summary>
		/// an index into the TypeDef, MethodDef, or Assembly table;
		/// more precisely, a HasDeclSecurity (§II.24.2.6) coded index.
		/// </summary>
		public MetaCellCodedToken Parent => base.GetValue<MetaCellCodedToken>(1);

		/// <summary>
		/// The permission set contains the permissions that were requested with an
		/// Action on a specific Method, Type, or Assembly (see Parent).
		/// In other words, the blob will contain an encoding of all the attributes on the Parent
		/// with that particular Action.
		/// </summary>
		/// <remarks>
		/// PermissionSet is a 'blob' having the following format:
		/// A byte containing a period (.).
		/// A compressed unsigned integer containing the number of attributes encoded in the blob.
		/// An array of attributes each containing the following:
		/// A String, which is the fully-qualified type name of the attribute. (Strings are encoded as a compressed unsigned integer to indicate the size followed by an array of UTF8 characters.)
		/// A set of properties, encoded as the named arguments to a custom attribute would be (as in §II.23.3, beginning with NumNamed).
		/// </remarks>
		public Byte[] PermissionSet => base.GetValue<Byte[]>(2);

		/// <summary>Create instance of security attribute row</summary>
		public DeclSecurityRow()
			: base(Cor.MetaTableType.DeclSecurity) { }
	}
}