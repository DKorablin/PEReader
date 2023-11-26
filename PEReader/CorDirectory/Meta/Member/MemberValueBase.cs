using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Base member constant value reader</summary>
	public class MemberValueBase
	{
		private Object _value;

		/// <summary>Member or argument name</summary>
		public String Name { get; }

		/// <summary>Type of argument</summary>
		public ElementType Type { get; }

		/// <summary>Reflected eum type</summary>
		public String EnumType { get; internal set; }

		/// <summary>Argument has value</summary>
		public Boolean HasValue { get; private set; }

		/// <summary>Argument value</summary>
		public Object Value
		{
			get => _value;
			internal set
			{
				this._value = value;
				this.HasValue = true;
			}
		}

		internal MemberValueBase(ElementType type, String name)
		{
			this.Type = type;
			this.Name = name;
		}
	}
}