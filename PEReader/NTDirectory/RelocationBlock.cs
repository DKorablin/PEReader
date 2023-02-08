using System;
using System.Collections.Generic;
using System.Collections;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Relocation block info</summary>
	public struct RelocationBlock : IEnumerable<RelocationSection>
	{
		private readonly WinNT.IMAGE_BASE_RELOCATION _block;
		private readonly RelocationSection[] _sections;

		/// <summary>Relocation block</summary>
		public WinNT.IMAGE_BASE_RELOCATION Block { get { return this._block; } }

		/// <summary>Get relocation section by index</summary>
		/// <param name="index">Relocation section index</param>
		/// <returns>Section</returns>
		public RelocationSection this[Int32 index] { get { return this._sections[index]; } }

		/// <summary>Count of sections in the relocation block</summary>
		public Int32 Count { get { return this._sections.Length; } }

		/// <summary>Create instance of relocation block info</summary>
		/// <param name="block">Relocation block</param>
		/// <param name="sections">Array of sections</param>
		public RelocationBlock(WinNT.IMAGE_BASE_RELOCATION block, RelocationSection[] sections)
		{
			this._block = block;
			this._sections = sections;
		}

		/// <summary>Sections in current relocation block</summary>
		/// <returns>Relocation sections</returns>
		public IEnumerator<RelocationSection> GetEnumerator()
		{
			foreach(RelocationSection section in this._sections)
				yield return section;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}