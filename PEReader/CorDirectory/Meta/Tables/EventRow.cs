using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// Events are treated within metadata much like Properties;
	/// that is, as a way to associate a collection of methods defined on a given class.
	/// There are two required methods (add_ and remove_) plus an optional one (raise_);
	/// additional methods with other names are also permitted (§18).
	/// All of the methods gathered together as an Event shall be defined on the class (§I.8.11.4).
	/// </summary>
	public class EventRow : BaseMetaRow
	{
		/// <summary>Attributes of an event</summary>
		public EventAttributes EventFlags => (EventAttributes)base.GetValue<UInt16>(0);

		/// <summary>Event name</summary>
		public String Name => base.GetValue<String>(1);

		/// <summary>
		/// An index into a TypeDef, a TypeRef, or TypeSpec table;
		/// more precisely, a TypeDefOrRef (§II.24.2.6) coded index.
		/// </summary>
		/// <remarks>This corresponds to the Type of the Event; it is not the Type that owns this event</remarks>
		public MetaCellCodedToken EventType => base.GetValue<MetaCellCodedToken>(2);

		/// <summary>Create instance of Event row</summary>
		public EventRow()
			: base(Cor.MetaTableType.Event) { }

		/// <summary>Name</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> base.ToString(this.Name);
	}
}