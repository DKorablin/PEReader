using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The ImplMap table holds information about unmanaged methods that can be reached from managed code, using PInvoke dispatch</summary>
	public class ImplMapRow : BaseMetaRow
	{
		/// <summary>a 2-byte bitmask of type PInvokeAttributes, §23.1.8</summary>
		public CorPinvokeMap MappingFlags => (CorPinvokeMap)this.GetValue<UInt16>(0);

		/// <summary>An index into the Field or MethodDef table; more precisely, a MemberForwarded (§II.24.2.6) coded index)</summary>
		/// <remarks>However, it only ever indexes the MethodDef table, since Field export is not supported</remarks>
		public MetaCellCodedToken MemberForwarded => this.GetValue<MetaCellCodedToken>(1);

		/// <summary>Unmanaged method name</summary>
		public String ImportName => this.GetValue<String>(2);

		/// <summary>an index into the ModuleRef table</summary>
		internal MetaCellPointer ImportScopeI => this.GetValue<MetaCellPointer>(3);

		/// <summary>Unmanaged library name</summary>
		public ModuleRefRow ImportScope => new ModuleRefRow() { Row = this.ImportScopeI.TargetRow, };

		/// <summary>Create instance of <see cref="ImplMapRow"/></summary>
		public ImplMapRow()
			: base(Cor.MetaTableType.ImplMap) { }
	}
}