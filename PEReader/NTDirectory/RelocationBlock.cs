using System;
using System.Collections.Generic;
using System.Collections;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Relocation block info</summary>
	public struct RelocationBlock : IEnumerable<RelocationSection>
	{
		private RelocationSection[] Sections { get; }

		/// <summary>Relocation block</summary>
		public WinNT.IMAGE_BASE_RELOCATION Block { get; }

		/// <summary>Get relocation section by index</summary>
		/// <param name="index">Relocation section index</param>
		/// <returns>Section</returns>
		public RelocationSection this[Int32 index] => this.Sections[index];

		/// <summary>Count of sections in the relocation block</summary>
		public Int32 Count => this.Sections.Length;

		/// <summary>Create instance of relocation block info</summary>
		/// <param name="block">Relocation block</param>
		/// <param name="sections">Array of sections</param>
		public RelocationBlock(WinNT.IMAGE_BASE_RELOCATION block, RelocationSection[] sections)
		{
			this.Block = block;
			this.Sections = sections ?? throw new ArgumentNullException(nameof(sections));
		}

		/// <summary>Sections in current relocation block</summary>
		/// <returns>Relocation sections</returns>
		public IEnumerator<RelocationSection> GetEnumerator()
		{
			foreach(RelocationSection section in this.Sections)
				yield return section;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}