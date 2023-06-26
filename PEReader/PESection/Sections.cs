using System;
using System.Collections;
using System.Collections.Generic;

namespace AlphaOmega.Debug.PESection
{
	/// <summary>Manager PE sections wrapper</summary>
	public class Sections : IEnumerable<SectionHeader>
	{
		private readonly PEFile _parent;

		/// <summary>PE directory</summary>
		internal PEFile Parent { get { return this._parent; } }

		/// <summary>Create instance of section header reader class</summary>
		/// <param name="parent">PE directory</param>
		public Sections(PEFile parent)
		{
			this._parent = parent ?? throw new ArgumentNullException(nameof(parent));
		}

		/// <summary>Get section by name</summary>
		/// <param name="section">Name of required section</param>
		/// <returns>Found section header or null of section not found</returns>
		public SectionHeader GetSection(String section)
		{
			foreach(WinNT.IMAGE_SECTION_HEADER header in this.Parent.Header.Sections)
				if(header.Section == section)
					return new SectionHeader(this.Parent, header);

			return null;
		}

		/// <summary>Get all sections with opportunity to read data from each section</summary>
		/// <returns>Sections array with wrappers</returns>
		public IEnumerator<SectionHeader> GetEnumerator()
		{
			foreach(WinNT.IMAGE_SECTION_HEADER header in this.Parent.Header.Sections)
				yield return new SectionHeader(this.Parent, header);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}