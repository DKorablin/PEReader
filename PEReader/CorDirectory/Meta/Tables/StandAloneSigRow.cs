using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Signatures are stored in the metadata Blob heap.
	/// In most cases, they are indexed by a column in some table - Field.Signature, Method.Signature, MemberRef.Signature, etc.
	/// However, there are two cases that require a metadata token for a signature that is not indexed by any metadata table.
	/// The StandAloneSig table fulfils this need.
	/// It has just one column, which points to a Signature in the Blob heap.
	/// </summary>
	public class StandAloneSigRow : BaseMetaRow
	{
		/// <summary>
		/// The signature shall describe either:
		/// <list type="bullet">
		///		<item>
		///			<term>A method</term>
		///			<description>code generators create a row in the StandAloneSig table for each occurrence of a calli CIL instruction. That row indexes the call-site signature for the function pointer operand of the calli instruction</description>
		///		</item>
		///		<item>
		///			<term>Local variables</term>
		///			<description>code generators create one row in the StandAloneSig table for each method, to describe all of its local variables. The .locals directive (§II.15.4.1) in ILAsm generates a row in the StandAloneSig table</description>
		///		</item>
		/// </list>
		/// </summary>
		public Byte[] Signature => base.GetValue<Byte[]>(0);

		/// <summary>Create instance of assembly signature row</summary>
		public StandAloneSigRow()
			: base(Cor.MetaTableType.StandAloneSig) { }
	}
}