using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Class in the current assembly.</summary>
	[DefaultProperty("TypeName")]
	public class TypeDefRow : BaseMetaRow
	{
		/// <summary>A 4-byte bit mask of type TypeAttributes (#II.23.1.15) .</summary>
		public TypeAttributes Flags { get { return (TypeAttributes)base.GetValue<UInt32>(0); } }
		/// <summary>Type name</summary>
		public String TypeName { get { return base.GetValue<String>(1); } }
		/// <summary>Type namespace</summary>
		public String TypeNamespace { get { return base.GetValue<String>(2); } }
		/// <summary>an index into the TypeDef, TypeRef, or TypeSpec table; more precisely, a TypeDefOrRef (#II.24.2.6) coded index</summary>
		public MetaCellCodedToken Extends { get { return base.GetValue<MetaCellCodedToken>(3); } }
		/// <summary>An index into the Field table; it marks the first of a contiguous run of Fields owned by this Type.</summary>
		/// <remarks>
		/// The run continues to the smaller of:
		/// The last row of the Field table.
		/// The next run of Fields, found by inspecting the FieldList of the next row in this TypeDef table.
		/// </remarks>
		internal MetaCellPointer FieldListI { get { return base.GetValue<MetaCellPointer>(4); } }
		/// <summary>An index into the MethodDef table; it marks the first of a continguous run of Methods owned by this Type.</summary>
		/// <remarks>
		/// The run continues to the smaller of:
		/// The last row of the MethodDef table.
		/// the next run of Methods, found by inspecting the MethodList of the next row in this TypeDef table.
		/// </remarks>
		internal MetaCellPointer MethodListI { get { return base.GetValue<MetaCellPointer>(5); } }

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
		/// <summary>TypeNamespace+"."+TypeName</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			String result;
			if(String.IsNullOrEmpty(this.TypeNamespace))
				result = this.TypeName;
			else
				result = this.TypeNamespace + "." + this.TypeName;

			return base.ToString(result);
		}
	}
}