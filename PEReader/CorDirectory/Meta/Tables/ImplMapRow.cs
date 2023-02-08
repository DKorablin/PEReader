using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The ImplMap table holds information about unmanaged methods that can be reached from managed code, using PInvoke dispatch</summary>
	public class ImplMapRow : BaseMetaRow
	{
		/// <summary>a 2-byte bitmask of type PInvokeAttributes, §23.1.8</summary>
		public CorPinvokeMap MappingFlags { get { return (CorPinvokeMap)base.GetValue<UInt16>(0); } }

		/// <summary>
		/// an index into the Field or MethodDef table; more precisely,
		/// a MemberForwarded (§II.24.2.6) coded index).
		/// However, it only ever indexes the MethodDef table, since Field export is not supported
		/// </summary>
		public MetaCellCodedToken MemberForwarded { get { return base.GetValue<MetaCellCodedToken>(1); } }

		/// <summary>Unmanaged method name</summary>
		public String ImportName { get { return base.GetValue<String>(2); } }

		/// <summary>an index into the ModuleRef table</summary>
		internal MetaCellPointer ImportScopeI { get { return base.GetValue<MetaCellPointer>(3); } }

		/// <summary>Umanaged library name</summary>
		public ModuleRefRow ImportScope
		{
			get { return new ModuleRefRow() { Row = this.ImportScopeI.TargetRow, }; }
		}
	}
}