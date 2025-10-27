using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Create instance if class method builder based on strongly typed metadata <see cref="Cor.MetaTableType.MethodDef"/> row</summary>
	[DebuggerDisplay(nameof(Name) + " = {" + nameof(Name) + "}")]
	public class MethodReader
	{
		/// <summary>Method definition description</summary>
		public MethodDefRow MethodDef { get; }

		/// <summary>Method name</summary>
		public String Name
		{
			get
			{
				String result = this.MethodDef.Name;
				MethodAttributes flags = this.MethodDef.Flags;
				if((flags & MethodAttributes.SpecialName) == MethodAttributes.SpecialName)
				{
					switch(result)
					{
					case ".ctor":
					case ".cctor":
						result = this.DeclaringType.TypeDef.TypeName;
						break;
					default:
						if(result.StartsWith("get_") || result.StartsWith("set_"))
							result = result.Substring(4);
						break;
					}
				}
				return result;
			}
		}

		/// <summary>This method is defined as property</summary>
		public virtual Boolean IsProperty
		{
			get
			{
				MethodAttributes flags = this.MethodDef.Flags;
				if((flags & MethodAttributes.SpecialName) == MethodAttributes.SpecialName)
				{
					String name = this.MethodDef.Name;
					return name.StartsWith("get_") || name.StartsWith("set_");
				}
				return false;
			}
		}

		/// <summary>Method return type</summary>
		public virtual ElementType Return => this.MethodDef.ReturnType;

		/// <summary>MetaData from current assembly</summary>
		protected internal StreamTables MetaData => this.MethodDef.Row.Table.Root;

		/// <summary>Gets the type that declares the current nested type or generic type parameter.</summary>
		public TypeReader DeclaringType
		{
			get
			{
				foreach(TypeDefRow row in this.MetaData.TypeDef)
					foreach(MethodDefRow method in row.MethodList)
						if(method == this.MethodDef)
							return new TypeReader(row);

				throw new InvalidOperationException($"Declaring type for method {this.Name} not found");
			}
		}

		/// <summary>Create instance of <see cref="MethodReader"/></summary>
		/// <param name="methodDefRow">Strongly typed metadata <see cref="Cor.MetaTableType.MethodDef"/> row</param>
		/// <exception cref="ArgumentNullException"><paramref name="methodDefRow"/> is required</exception>
		public MethodReader(MethodDefRow methodDefRow)
			=> this.MethodDef = methodDefRow ?? throw new ArgumentNullException(nameof(methodDefRow));

		/// <summary>Gets list of all arguments for current method</summary>
		/// <returns>List of <see cref="MemberArgument"/> instances describing which arguments required for current method to invoke</returns>
		public IEnumerable<MemberArgument> GetArguments()
		{
			foreach(MemberArgument arg in this.MethodDef.ParamList)
				yield return arg;
		}

		/// <summary>Gets list of generic arguments related to current method</summary>
		/// <returns>List of <see cref="GenericParamRow"/> describing all method generic parameters</returns>
		public IEnumerable<GenericParamRow> GetGenericArguments()
		{
			foreach(GenericParamRow row in this.MetaData.GenericParam)
				if(row.Owner.TargetRow == this.MethodDef)
					yield return row;
		}

		/// <summary>Gets list of attributes applied to current method</summary>
		/// <returns>List of <see cref="AttributeReader"/> instances related to current method</returns>
		public virtual IEnumerable<AttributeReader> GetCustomAttributes()
		{
			foreach(CustomAttributeRow row in this.MetaData.CustomAttribute)
				if(row.Parent.TableType == Cor.MetaTableType.MethodDef
					&& row.Parent.TargetRow == this.MethodDef)
						yield return new AttributeReader(row);
		}
	}
}