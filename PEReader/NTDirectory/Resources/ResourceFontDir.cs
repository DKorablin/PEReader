using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Font directory resource class</summary>
	[DefaultProperty("NumberOfFonts")]
	public class ResourceFontDir : ResourceBase, IEnumerable<ResourceFontDir.FontDirEntry>
	{
		/// <summary>Font dir info</summary>
		public class FontDirEntry : ResourceFont.FontEntry
		{
			/// <summary>A unique ordinal identifier for an individual font in a font resource group.</summary>
			public UInt16 fontOrdinal;
		}

		/// <summary>Получить кол-во шрифтов в директории</summary>
		public UInt16 NumberOfFonts
		{
			get
			{
				return PinnedBufferReader.BytesToStructure<UInt16>(base.Directory.GetData(), 0);
			}
		}

		/// <summary>Create instance of font directory resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceFontDir(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR)
		{
		}

		/// <summary>Get all fonts from directory</summary>
		/// <returns>Fonts from directory</returns>
		public IEnumerator<FontDirEntry> GetEnumerator()
		{
			UInt16 numberOfFonts = this.NumberOfFonts;
			if(numberOfFonts > 0)
			{
				UInt32 padding = sizeof(UInt16);
				using(PinnedBufferReader reader = base.CreateDataReader())
					for(UInt16 loop = 0;loop < numberOfFonts;loop++)
					{
						FontDirEntry entry = new FontDirEntry()
						{
							fontOrdinal = reader.BytesToStructure<UInt16>(ref padding),
						};
						ResourceFont.GetFont(reader, ref entry, ref padding);
						yield return entry;
					}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}