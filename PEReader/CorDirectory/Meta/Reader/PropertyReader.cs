using System;
using System.Collections.Generic;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>The metadata <see cref="Cor.MetaTableType.MethodDef"/> reader class where row is property</summary>
	public class PropertyReader : MethodReader
	{
		private readonly TypeDefRow _typeDef;
		private readonly MethodDefRow _getMethodDef;
		private PropertyRow _property;

		/// <summary>The property return type</summary>
		public override ElementType Return { get => this._getMethodDef.ReturnType; }

		/// <summary>This method is defined as property</summary>
		public override Boolean IsProperty { get => true; }

		/// <summary>Gets a value indicating whether the property can be read</summary>
		public Boolean CanRead { get => this._getMethodDef.Name.StartsWith("get_"); }

		/// <summary>Gets a value indicating whether the property can be written to</summary>
		public Boolean CanWrite { get => base.MethodDef.Name.StartsWith("set_"); }

		/// <summary>Property description information</summary>
		/// <exception cref="InvalidOperationException">Failed to find this property in the list of all properties</exception>
		public PropertyRow Property
		{
			get
			{
				if(this._property == null)
					foreach(PropertyMapRow mapRow in this._typeDef.Row.Table.Root.PropertyMap)
						if(mapRow.Parent == this._typeDef)
						{
							foreach(PropertyRow row in mapRow.PropertyList)
								if(row.Name == this.Name)
								{
									this._property = row;
									break;
								}
							break;
						}

				return this._property
					?? throw new InvalidOperationException($"Property {this.Name} not found in the list of all properties");
			}
		}

		internal PropertyReader(TypeDefRow typeDef, MethodDefRow getMethodDef, MethodDefRow setMethodDef)
			: base(setMethodDef ?? getMethodDef)
		{
			this._typeDef = typeDef ?? throw new ArgumentNullException(nameof(typeDef));
			this._getMethodDef = getMethodDef ?? setMethodDef;
		}

		/// <summary>Returns the public get accessor for this property</summary>
		/// <returns>The public get accessor for this property, if the get accessor exists; otherwise, null</returns>
		public MethodReader GetGetMethod()
			=> this.CanRead
				? new MethodReader(this._getMethodDef)
				: null;

		/// <summary>Returns the public set accessor for this property</summary>
		/// <returns>The Set method for this property, if the set accessor exists; otherwise, null</returns>
		public MethodReader GetSetMethod()
			=> this.CanWrite
				? new MethodReader(base.MethodDef)
				: null;

		/// <inheritdoc/>
		public override IEnumerable<AttributeReader> GetCustomAttributes()
		{
			foreach(CustomAttributeRow row in this.MetaData.CustomAttribute)
				if(row.Parent.TableType == Cor.MetaTableType.Property
					&& row.Parent.TargetRow == this.Property)
						yield return new AttributeReader(row);
		}
	}
}