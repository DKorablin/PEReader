using System;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>The metadata <see cref="Cor.MetaTableType.MethodDef"/> reader class where row is property</summary>
	public class PropertyReader : MethodReader
	{
		private readonly MethodDefRow _getMethodDef;

		/// <summary>The property return type</summary>
		public override ElementType Return => this._getMethodDef.ReturnType;

		/// <summary>This methid is defined as property</summary>
		public override Boolean IsProperty => true;

		/// <summary>Gets a value indicating whether the property can be read</summary>
		public Boolean CanRead => this._getMethodDef.Name.StartsWith("get_");

		/// <summary>Gets a value indicating whether the property can be written to</summary>
		public Boolean CanWrite => base.MethodDef.Name.StartsWith("set_");

		internal PropertyReader(MethodDefRow getMethodDef, MethodDefRow setMethodDef)
			: base(setMethodDef ?? getMethodDef)
			=> this._getMethodDef = getMethodDef ?? setMethodDef;

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
	}
}