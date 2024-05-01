using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Method definition descriptors</summary>
	[DefaultProperty(nameof(Name))]
	public class MethodDefRow : BaseMetaRow
	{
		private SignatureParser _signatureI;
		private MethodBody _body;

		/// <summary>Method body address</summary>
		public UInt32 RVA => base.GetValue<UInt32>(0);

		/// <summary>Specifies flags for the attributes of a method implementation</summary>
		public MethodImplAttributes ImplFlags => (MethodImplAttributes)base.GetValue<UInt16>(1);

		/// <summary>Specifies flags for method attributes. These flags are defined in the corhdr.h file</summary>
		public MethodAttributes Flags => (MethodAttributes)base.GetValue<UInt16>(2);

		/// <summary>Method name</summary>
		public String Name => base.GetValue<String>(3);

		/// <summary>
		/// As name implies, this signature stores information related to methods defined in current assembly,
		/// such as the calling convention type, the number of generic parameters,
		/// the number of normal method's parameters,
		/// the return type and the type of each parameter supplied to the method.
		/// Is indexed by the MethodDef.Signature column.
		/// </summary>
		/// <remarks>If Signature is GENERIC (0x10), the generic arguments are described in the GenericParam table (§II.22.20)</remarks>
		public Byte[] Signature => base.GetValue<Byte[]>(4);

		/// <summary>Strongly typed method signature with return type, calling convention and arguments types</summary>
		private SignatureParser SignatureI => _signatureI ?? (_signatureI = new SignatureParser(this.Row, Signature));

		/// <summary>Calling convention that are made in managed code for current method</summary>
		public Cor.IMAGE_CEE_CS CorCallingConvention => this.SignatureI.CorCallingConvention;

		/// <summary>Count of method args</summary>
		public Int32 ArgsCount => this.SignatureI.ArgsCount;

		/// <summary>Return method type</summary>
		public ElementType ReturnType => this.SignatureI.ReturnType;

		/// <summary>Arguments types</summary>
		public ElementType[] ArgsType => this.SignatureI.ArgumentsTypes;

		/// <summary>Index into the Param table</summary>
		/// <remarks>
		/// It marks the first of a contiguous run of Parameters owned by this method. The run continues to the smaller of:
		/// The last row of the Param table.
		/// The next run of Parameters, found by inspecting the ParamList of the next row in the MethodDef table
		/// </remarks>
		internal MetaCellPointer ParamListI => base.GetValue<MetaCellPointer>(5);

		/// <summary>Get a list of method arguments</summary>
		public IEnumerable<MemberArgument> ParamList
		{
			get
			{
				if(this.ArgsCount == 0)
					yield break;

				UInt16 argsIndex = 0;
				foreach(MetaRow row in this.ParamListI.GetTargetRowsIt())
				{
					ParamRow param = new ParamRow { Row = row, };
					if(param.Sequence == 0x00)//TODO: This param applied to return object
						continue;

					ElementType argument = this.SignatureI.ArgumentsTypes[argsIndex++];
					yield return new MemberArgument(argument, param) { Flags = param.Flags, };
				}

				if(argsIndex == 0 && this.SignatureI.ArgumentsTypes.Length > 0)//Arguments without reference to MetaTable
					foreach(var arg in this.SignatureI.ArgumentsTypes)
						yield return new MemberArgument(arg, null, argsIndex++);
			}
		}

		/// <summary>Method body descriptor</summary>
		public MethodBody Body => this._body ?? (this._body = new MethodBody(this));

		/// <summary>Create instance of Method definition row</summary>
		public MethodDefRow()
			: base(Cor.MetaTableType.MethodDef) { }

		/// <summary>String representation of the Method definition row</summary>
		/// <returns>String representation of the current row</returns>
		public override String ToString()
			=> base.ToString(this.Name);
	}
}