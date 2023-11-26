using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Member property or field with value stored in metadata</summary>
	public class MemberElement : MemberValueBase
	{
		/// <summary>Member elemnt type</summary>
		/// <remarks><see cref="Cor.ELEMENT_TYPE.PROPERTY"/> or <see cref="Cor.ELEMENT_TYPE.FIELD"/></remarks>
		public Cor.ELEMENT_TYPE ElementType { get; }

		internal MemberElement(Cor.ELEMENT_TYPE elementType, ElementType type, String name)
			: base(type, name)
			=> this.ElementType = elementType;

		/// <summary>Formats member element (property or field) as string representation</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> $"{GetType().Name}: {{{this.Type}}} {this.Name} = {Value ?? "null"}";
	}
}