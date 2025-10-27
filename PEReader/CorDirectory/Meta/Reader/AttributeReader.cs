using System;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Attribute reader based on strongly typed metadata <see cref="Cor.MetaTableType.CustomAttribute"/> table</summary>
	public class AttributeReader
	{
		/// <summary>Metadata <see cref="Cor.MetaTableType.CustomAttribute"/> strongly typed row</summary>
		public CustomAttributeRow Attribute { get; }

		/// <summary>Gets the fully qualified name of the System.Type, including the namespace of the System.Type but not the assembly.</summary>
		public String FullName
		{
			get
			{
				switch(this.Attribute.Type.TableType)
				{
				case Cor.MetaTableType.MemberRef:
					MemberRefRow memberRef = this.Attribute.Type.GetTargetRowTyped<MemberRefRow>();
					TypeRefRow typeRef = memberRef.Class.GetTargetRowTyped<TypeRefRow>();
					return typeRef.TypeNamespace + "." + typeRef.TypeName;
				case Cor.MetaTableType.MethodDef:
					MethodDefRow methodDef = this.Attribute.Type.GetTargetRowTyped<MethodDefRow>();
					foreach(TypeDefRow typeDef in this.Attribute.Row.Table.Root.TypeDef)
						foreach(MethodDefRow method in typeDef.MethodList)
							if(method == methodDef)
								return typeDef.TypeNamespace + "." + typeDef.TypeName;
					throw new NotImplementedException();
				default:
					throw new NotImplementedException();
				}
			}
		}

		/// <summary>Create instance of <see cref="AttributeReader"/></summary>
		/// <param name="customAttribute">Strongly typed metadata <see cref="Cor.MetaTableType.CustomAttribute"/> row</param>
		/// <exception cref="NotImplementedException"><paramref name="customAttribute"/> is required</exception>
		public AttributeReader(CustomAttributeRow customAttribute)
			=> this.Attribute = customAttribute ?? throw new NotImplementedException(nameof(customAttribute));
	}
}