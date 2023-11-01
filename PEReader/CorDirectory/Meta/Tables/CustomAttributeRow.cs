using System;

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
		/// <summary>An index into a metadata table that has an associated HasCustomAttribute (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Parent { get { return base.GetValue<MetaCellCodedToken>(0); } }

		/// <summary>An index into the MethodDef or MemberRef table; more precisely, a CustomAttributeType (§II.24.2.6) coded index</summary>
		public MetaCellCodedToken Type { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>Attribute value</summary>
		/// <remarks>All binary values are stored in little-endian format (except for PackedLen items, which are used only as a count for the number of bytes to follow in a UTF8 string)</remarks>
		public Byte[] Value { get { return base.GetValue<Byte[]>(2); } }
	}
}