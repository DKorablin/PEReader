using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Method definition descriptors.</summary>
	[DefaultProperty("Name")]
	public class MethodDefRow : BaseMetaRow
	{
		#region Fields
		private MethodBody _body;
		#endregion Fields

		/// <summary>Method body address</summary>
		public UInt32 RVA { get { return base.GetValue<UInt32>(0); } }

		/// <summary>Specifies flags for the attributes of a method implementation.</summary>
		public MethodImplAttributes ImplFlags { get { return (MethodImplAttributes)base.GetValue<UInt16>(1); } }

		/// <summary>Specifies flags for method attributes. These flags are defined in the corhdr.h file.</summary>
		public MethodAttributes Flags { get { return (MethodAttributes)base.GetValue<UInt16>(2); } }

		/// <summary>Method name</summary>
		public String Name { get { return base.GetValue<String>(3); } }

		/// <summary>
		/// As name implies, this signature stores information related to methods defined in current assembly,
		/// such as the calling convention type, the number of generic parameters,
		/// the number of normal method's parameters,
		/// the return type and the type of each parameter supplied to the method.
		/// Is indexed by the MethodDef.Signature column.
		/// </summary>
		/// <remarks>If Signature is GENERIC (0x10), the generic arguments are described in the GenericParam table (§II.22.20). </remarks>
		public Byte[] Signature { get { return base.GetValue<Byte[]>(4); } }

		/// <summary>Index into the Param table.</summary>
		/// <remarks>
		/// It marks the first of a contiguous run of Parameters owned by this method. The run continues to the smaller of:
		/// The last row of the Param table.
		/// The next run of Parameters, found by inspecting the ParamList of the next row in the MethodDef table.
		/// </remarks>
		internal MetaCellPointer ParamListI { get { return base.GetValue<MetaCellPointer>(5); } }

		/// <summary>Получить список параметров для метода.</summary>
		public IEnumerable<ParamRow> ParamList
		{
			get
			{
				foreach(MetaRow row in this.ParamListI.GetTargetRowsIt())
					yield return new ParamRow() { Row = row, };
			}
		}

		/// <summary>Method body descriptor</summary>
		public MethodBody Body
		{
			get
			{
				return this._body == null
					? this._body = new MethodBody(this)
					: this._body;
			}
		}

		/// <summary>String representation of the Method definition row</summary>
		/// <returns>String representation of the current row</returns>
		public override String ToString()
		{
			return base.ToString(this.Name);
		}
	}
}