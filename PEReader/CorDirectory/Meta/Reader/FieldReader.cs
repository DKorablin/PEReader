using System;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary></summary>
	public class FieldReader
	{
		/// <summary>Metadata field row</summary>
		public FieldRow Field { get; }

		/// <summary>Gets field name</summary>
		public String Name => Field.Name;

		/// <summary>Gets field type</summary>
		public ElementType ReturnType => Field.ReturnType;

		/// <summary>Create instance if <see cref="FieldReader"/></summary>
		/// <param name="field">With strongly typed metadata <see cref="FieldRow"/></param>
		/// <exception cref="ArgumentNullException"><c>field</c> is required</exception>
		public FieldReader(FieldRow field)
			=> this.Field = field ?? throw new ArgumentNullException(nameof(field));
	}
}