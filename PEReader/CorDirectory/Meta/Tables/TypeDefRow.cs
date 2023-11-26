using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Class in the current assembly</summary>
	[DefaultProperty(nameof(TypeName))]
	public class TypeDefRow : BaseMetaRow
	{
		/// <summary>A 4-byte bit mask of type TypeAttributes (§II.23.1.15)</summary>
		public TypeAttributes Flags => (TypeAttributes)base.GetValue<UInt32>(0);

		/// <summary>Specifies type visibility information</summary>
		public TypeAttributes VisibilityMask => this.Flags & TypeAttributes.VisibilityMask;

		/// <summary>Specifies class layout information</summary>
		public TypeAttributes LayoutMask => this.Flags & TypeAttributes.LayoutMask;

		/// <summary>Specifies class semantics information; the current class is contextful (else agile)</summary>
		public TypeAttributes ClassSemanticsMask => this.Flags & TypeAttributes.ClassSemanticsMask;

		/// <summary>Used to retrieve string information for native interoperability</summary>
		public TypeAttributes StringFormatMask => this.Flags & TypeAttributes.StringFormatMask;

		/// <summary>Attributes reserved for runtime use</summary>
		public TypeAttributes ReservedMask => this.Flags & TypeAttributes.ReservedMask;

		/// <summary>Used to retrieve non-standard encoding information for native interop</summary>
		/// <remarks>
		/// The meaning of the values of these 2 bits is unspecified.
		/// Not used in the Microsoft implementation of the .NET Framework.
		/// </remarks>
		public TypeAttributes CustomFormatMask => this.Flags & TypeAttributes.CustomFormatMask;

		/// <summary>Type name</summary>
		public String TypeName => base.GetValue<String>(1);

		/// <summary>Type namespace</summary>
		public String TypeNamespace => base.GetValue<String>(2);

		/// <summary>an index into the TypeDef, TypeRef, or TypeSpec table; more precisely, a TypeDefOrRef (#II.24.2.6) coded index</summary>
		public MetaCellCodedToken Extends => base.GetValue<MetaCellCodedToken>(3);

		/// <summary>An index into the Field table; it marks the first of a contiguous run of Fields owned by this Type</summary>
		/// <remarks>
		/// The run continues to the smaller of:
		/// The last row of the Field table.
		/// The next run of Fields, found by inspecting the FieldList of the next row in this TypeDef table
		/// </remarks>
		internal MetaCellPointer FieldListI => base.GetValue<MetaCellPointer>(4);

		/// <summary>An index into the MethodDef table; it marks the first of a continguous run of Methods owned by this Type</summary>
		/// <remarks>
		/// The run continues to the smaller of:
		/// The last row of the MethodDef table.
		/// the next run of Methods, found by inspecting the MethodList of the next row in this TypeDef table
		/// </remarks>
		internal MetaCellPointer MethodListI => base.GetValue<MetaCellPointer>(5);

		/// <summary>Fields rows</summary>
		public IEnumerable<FieldRow> FieldList
		{
			get
			{
				foreach(MetaRow row in this.FieldListI.GetTargetRowsIt())
					yield return new FieldRow() { Row = row, };
			}
		}

		/// <summary>Methods rows</summary>
		public IEnumerable<MethodDefRow> MethodList
		{
			get
			{
				foreach(MetaRow row in this.MethodListI.GetTargetRowsIt())
					yield return new MethodDefRow() { Row = row, };
			}
		}

		/// <summary>Create isntance of class in the current assembly</summary>
		public TypeDefRow()
			: base(Cor.MetaTableType.TypeDef) { }

		/// <summary>TypeNamespace+"."+TypeName</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			String result = String.IsNullOrEmpty(this.TypeNamespace)
				? this.TypeName
				: this.TypeNamespace + "." + this.TypeName;

			return base.ToString(result);
		}
	}
}