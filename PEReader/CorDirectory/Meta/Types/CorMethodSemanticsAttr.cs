using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Contains values that describe the relationship between a method and an associated property or event</summary>
	/// <remarks>http://msdn.microsoft.com/en-us/library/ms232595.aspx</remarks>
	[Flags]
	public enum CorMethodSemanticsAttr : short
	{
		/// <summary>Specifies that the method is a set accessor for a property</summary>
		msSetter = 0x0001,
		/// <summary>Specifies that the method is a get accessor for a property</summary>
		msGetter = 0x0002,
		/// <summary>Specifies that the method has a relationship to a property or an event other than those defined here</summary>
		msOther = 0x0004,
		/// <summary>Specifies that the method adds handler methods for an event</summary>
		msAddOn = 0x0008,
		/// <summary>Specifies that the method removes handler methods for an event</summary>
		msRemoveOn = 0x0010,
		/// <summary>Specifies that the method raises an event</summary>
		msFire = 0x0020,
	}
}