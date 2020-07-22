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
			if(parent == null)
				throw new ArgumentNullException("parent");

			this._parent = parent;
		}

		/// <summary>Получить секцию по наименованию секции</summary>
		/// <param name="section">Наименование требуемой секции</param>
		/// <returns>Найденная секция или null</returns>
		public SectionHeader GetSection(String section)
		{
			foreach(WinNT.IMAGE_SECTION_HEADER header in this.Parent.Header.Sections)
				if(header.Section == section)
					return new SectionHeader(this.Parent, header);

			return null;
		}

		/// <summary>Получить массив секций с возможностью получения актуальных данных из секции</summary>
		/// <returns>Массив секций обрамлённых обёртками</returns>
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