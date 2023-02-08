using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Class reference descriptors</summary>
	public class TypeRefRow : BaseMetaRow
	{
		/// <summary>
		/// an index into a Module, ModuleRef, AssemblyRef or TypeRef table, or null;
		/// more precisely, a ResolutionScope (§II.24.2.6) coded index
		/// </summary>
		public MetaCellCodedToken ResolutionScope { get { return base.GetValue<MetaCellCodedToken>(0); } }

		/// <summary>Type name</summary>
		public String TypeName { get { return base.GetValue<String>(1); } }

		/// <summary>Type namespace</summary>
		public String TypeNamespace { get { return base.GetValue<String>(2); } }
	}
}