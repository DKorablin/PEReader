using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Properties within metadata are best viewed as a means to gather together collections of methods defined on a class, give them a name, and not much else</summary>
	/// <remarks>The methods are typically get_ and set_ methods, already defined on the class, and inserted like any other methods into the MethodDef table</remarks>
	public class PropertyRow : BaseMetaRow
	{
		/// <summary>Flags that can be associated with a property</summary>
		public PropertyAttributes Flags => (PropertyAttributes)base.GetValue<UInt16>(0);

		/// <summary>Property name</summary>
		public String Name => base.GetValue<String>(1);

		/// <summary>Signature</summary>
		/// <remarks>
		/// The name of this column is misleading.
		/// It does not index a TypeDef or TypeRef table - instead it indexes the signature in the Blob heap of the Property.
		/// </remarks>
		public Byte[] Type => base.GetValue<Byte[]>(2);

		/// <summary>First byte of signature</summary>
		public CorSignature PropertySig => (CorSignature)this.Type[0];

		/// <summary>Count of input parameters for this property</summary>
		[Obsolete("It's compressed UInt32", true)]
		public Byte ParamCount => this.Type[1];

		/// <summary>Property type</summary>
		public Cor.ELEMENT_TYPE ReturnType { get { return (Cor.ELEMENT_TYPE)this.Type[2]; } }

		/// <summary>Create instance of Property description row</summary>
		public PropertyRow()
			: base(Cor.MetaTableType.Property)
		{
		}

		/// <summary>Name</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return base.ToString(this.Name);
		}
	}
}