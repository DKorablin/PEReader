using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Native resource directory class</summary>
	[DefaultProperty("RootResource")]
	public class Resource : NTDirectoryBase, IEnumerable<ResourceDirectory>
	{
		/// <summary>Root resource directory</summary>
		public WinNT.Resource.IMAGE_RESOURCE_DIRECTORY? RootResource
		{
			get
			{
				if(base.Directory.IsEmpty)
					return null;
				else
					return base.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY>(base.Directory.VirtualAddress);
			}
		}
		/// <summary>Получить информацию о версии PE файла из ресурсов</summary>
		/// <returns>Информация о версии PE файла</returns>
		public ResourceVersion GetVersion()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION))
				return new ResourceVersion(directory);
			return null;
		}
		/// <summary>Получить манифест из ресурсов PE файла</summary>
		/// <returns>Манифест</returns>
		public ResourceManifest GetManifest()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST))
				return new ResourceManifest(directory);
			return null;
		}
		/// <summary>Получить все строки из ресурсов</summary>
		/// <returns>Строки из PE файла</returns>
		public IEnumerable<ResourceString> GetStrings()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING))
				yield return new ResourceString(directory);
		}
		/// <summary>Получить все акселераторы из ресурсов</summary>
		/// <returns>Акселераторы</returns>
		public IEnumerable<ResourceAccelerator> GetAccelerators()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR))
				yield return new ResourceAccelerator(directory);
		}
		/// <summary>Get message tables resource classes</summary>
		/// <returns>Message Table classes</returns>
		public IEnumerable<ResourceMessageTable> GetMessageTables()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MESSAGETABLE))
				yield return new ResourceMessageTable(directory);
		}
		/// <summary>Получить описание ококн в приложении</summary>
		/// <returns>Список диалогов в приложении</returns>
		public IEnumerable<ResourceDialog> GetDialogs()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG))
				yield return new ResourceDialog(directory);
		}
		/// <summary>Получить описание меню в приложении</summary>
		/// <returns>Список меню в приложении</returns>
		public IEnumerable<ResourceMenu> GetMenus()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU))
				yield return new ResourceMenu(directory);
		}
		/// <summary>Получить описание всех картинок в ресурсах</summary>
		/// <returns>Список всех картинок в ресурсах</returns>
		public IEnumerable<ResourceBitmap> GetBitmaps()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP))
				yield return new ResourceBitmap(directory);
		}
		/// <summary>Получить описание всех директорий ресурсов</summary>
		/// <returns>Список всех директорий с ресурсами</returns>
		public IEnumerable<ResourceFontDir> GetFontDirs()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR))
				yield return new ResourceFontDir(directory);
		}
		/// <summary>Получить шрифты из ресурсов</summary>
		/// <returns>Список всех шрифтов из ресурсов</returns>
		public IEnumerable<ResourceFont> GetFonts()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT))
				yield return new ResourceFont(directory);
		}
		/// <summary>Получить директории с данными</summary>
		/// <param name="directoryType">Тип директроии ресурсы из которой получить</param>
		/// <returns>Директории соответствующий определённой директории</returns>
		public IEnumerable<ResourceDirectory> GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE directoryType)
		{
			foreach(var directory in this)
				if(directory.DirectoryEntry.NameType == directoryType)
					foreach(var subDir1 in directory)//Идентификаторы
						foreach(var subDir2 in subDir1)
							yield return subDir2;
		}
		/// <summary>Create instance of resource directory class</summary>
		/// <param name="parent">Data directory</param>
		public Resource(PEDirectory parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.RESOURCE)
		{
		}
		/// <summary>Get recource directories from image direstory</summary>
		/// <returns>Resourec directories</returns>
		public IEnumerator<ResourceDirectory> GetEnumerator()
		{
			var root = this.RootResource;
			if(root.HasValue && root.Value.ContainsEntries)
			{
				UInt32 sizeOfStruct = ResourceDirectory.DirectoryEntrySize;

				UInt32 padding = base.Directory.VirtualAddress + ResourceDirectory.DirectorySize;
				for(UInt16 loop = 0; loop < root.Value.NumberOfEntries; loop++)
				{
					WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY entry = base.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY>(padding);
					padding += sizeOfStruct;
					yield return new ResourceDirectory(entry, this);
				}
			}
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}