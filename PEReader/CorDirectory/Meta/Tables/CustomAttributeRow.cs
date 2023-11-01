using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

		/// <summary>Fixed argument values</summary>
		public IEnumerable<MethodParamValueRow> FixedArg
		{
			get
			{
				if(this.Type.TableType == Cor.MetaTableType.MethodDef)
				{
					Int32 offset = 2;//Prolog
					MethodDefRow row = this.Type.GetTargetRowTyped<MethodDefRow>();
					foreach(MethodParamRow arg in row.ParamList)
					{
						Object value = null;
						switch(arg.Type.Type)
						{
						case Cor.ELEMENT_TYPE.STRING:
							Int32 packedLength = BlobHeap.GetPackedLength(this.Value, ref offset);
							value = System.Text.Encoding.ASCII.GetString(this.Value, offset, packedLength);
							offset += packedLength;
							/*if(packedLength == 0xff)
								value = null;
							else if(packedLength == 0x00)
								value = String.Empty;*/
							break;
						case Cor.ELEMENT_TYPE.BOOLEAN:
							value = BitConverter.ToBoolean(this.Value, offset);
							offset += Marshal.SizeOf(value);
							break;
						case Cor.ELEMENT_TYPE.I2:
							value = BitConverter.ToInt16(this.Value, offset);
							offset += Marshal.SizeOf(value);
							break;
						case Cor.ELEMENT_TYPE.I4:
							value = BitConverter.ToInt32(this.Value, offset);
							offset += Marshal.SizeOf(value);
							break;
						case Cor.ELEMENT_TYPE.I8:
							value = BitConverter.ToInt64(this.Value, offset);
							offset += Marshal.SizeOf(value);
							break;
						default:
							throw new NotImplementedException();
						}
						yield return new MethodParamValueRow(arg, value);
					}
				}
				yield break;
			}
		}

		/// <summary>Create instance of custom attribute description row</summary>
		public CustomAttributeRow()
			: base(Cor.MetaTableType.CustomAttribute) { }
	}
}