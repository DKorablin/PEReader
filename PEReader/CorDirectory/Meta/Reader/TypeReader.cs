using System;
using System.Collections.Generic;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Class reader based on strongly typed metadata <see cref="Cor.MetaTableType.TypeDef"/> table</summary>
	public class TypeReader
	{
		/// <summary>Class in current assembly</summary>
		public TypeDefRow TypeDef { get; }

		/// <summary>Gets the fully qualified name of the <see cref="System.Type"/>, including the namespace of the <see cref="System.Type"/> but not the assembly.</summary>
		public String FullName
			=> this.TypeDef.TypeNamespace == String.Empty
				? this.TypeDef.TypeName
				: this.TypeDef.TypeNamespace + "." + this.TypeDef.TypeName;

		/// <summary>Gets the type from which the current System.Type directly inherits</summary>
		public String BaseType
		{
			get
			{
				switch(this.TypeDef.Extends.TableType)
				{
				case Cor.MetaTableType.TypeRef:
					TypeRefRow typeRef = this.TypeDef.Extends.GetTargetRowTyped<TypeRefRow>();
					return typeRef.TypeNamespace + "." + typeRef.TypeName;
				case Cor.MetaTableType.TypeDef:
					TypeDefRow typeDef = this.TypeDef.Extends.GetTargetRowTyped<TypeDefRow>();
					return typeDef.TypeNamespace + "." + typeDef.TypeName;
				default:
					throw new NotImplementedException();
				}
			}
		}

		/// <summary>Create instance of <see cref="TypeReader"/></summary>
		/// <param name="typeDef">Strongly typed metadata <see cref="Cor.MetaTableType.TypeDef"/> row</param>
		/// <exception cref="ArgumentNullException"><c>typeDef</c> is required</exception>
		public TypeReader(TypeDefRow typeDef)
			=> this.TypeDef = typeDef ?? throw new ArgumentNullException(nameof(typeDef));

		/// <summary>Gets list of all members in the current type (Including properties)</summary>
		/// <returns>List of <see cref="MethodReader"/> instances related to current type</returns>
		public IEnumerable<MethodReader> GetMembers()
		{
			MethodReader getProperty = null;
			foreach(MethodDefRow row in this.TypeDef.MethodList)
			{
				MethodReader result = new MethodReader(row);

				if(getProperty != null)
				{
					if(result.IsProperty && getProperty.Name == result.Name)
					{
						yield return new PropertyReader(getProperty.MethodDef, row);
						result = null;
					} else
						yield return new PropertyReader(getProperty.MethodDef, null);
					getProperty = null;
				}

				if(result == null) continue;

				if(result.IsProperty && row.Name.StartsWith("get_"))
					getProperty = result;//Trying to find set_ property. If I will not found it I will send it only set property
				else
					yield return result;
			}
		}

		/// <summary>Gets list of all properties in the current type</summary>
		/// <returns>List of <see cref="PropertyReader"/> instances related to current type</returns>
		public IEnumerable<PropertyReader> GetProperties()
		{
			foreach(MethodReader row in this.GetMembers())
				if(row is PropertyReader result)
					yield return result;
		}

		/// <summary>Gets list of all fields in current type</summary>
		/// <returns>List of all fields related to current type</returns>
		public IEnumerable<FieldRow> GetFields()
		{
			foreach(FieldRow row in this.TypeDef.FieldList)
				yield return row;
		}

		/// <summary>Gets list of attributes applied to current class</summary>
		/// <returns>List of <see cref="AttributeReader"/> instances related to curent class</returns>
		public IEnumerable<AttributeReader> GetAttributes()
		{
			foreach(CustomAttributeRow row in this.TypeDef.Row.Table.Root.CustomAttribute)
				if(row.Parent.TableType == Cor.MetaTableType.TypeDef)
					if(row.Parent.TargetRow == this.TypeDef)
						yield return new AttributeReader(row);
		}

		/// <summary>Gets all the interfaces implemented or inherited by the current type</summary>
		/// <returns>A list of <see cref="InterfaceImplRow"/> that describres all inherited interfaces for current type</returns>
		public IEnumerable<InterfaceImplRow> GetInterfaces()
		{
			foreach(InterfaceImplRow row in this.TypeDef.Row.Table.Root.InterfaceImpl)
				if(row.Class == this.TypeDef)
					yield return row;
		}
	}
}