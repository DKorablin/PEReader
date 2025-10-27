using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AlphaOmega.Debug.NTDirectory.Resources;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Native resource directory class</summary>
	[DefaultProperty(nameof(Header))]
	public class Resource : PEDirectoryBase, IEnumerable<ResourceDirectory>
	{
		/// <summary>Root resource directory</summary>
		public WinNT.Resource.IMAGE_RESOURCE_DIRECTORY? Header
			=> base.Directory.IsEmpty
				? (WinNT.Resource.IMAGE_RESOURCE_DIRECTORY?)null
				: base.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY>(base.Directory.VirtualAddress);

		/// <summary>Create instance of resource directory class</summary>
		/// <param name="parent">Data directory</param>
		public Resource(PEFile parent)
			: base(parent, WinNT.IMAGE_DIRECTORY_ENTRY.RESOURCE) { }

		/// <summary>Get PE file version information from resources</summary>
		/// <returns>PE file version information</returns>
		public ResourceVersion GetVersion()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION))
				return new ResourceVersion(directory);
			return null;
		}

		/// <summary>Get manifest from PE file resources</summary>
		/// <returns>manifest</returns>
		public ResourceManifest GetManifest()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST))
				return new ResourceManifest(directory);
			return null;
		}

		/// <summary>Get all rows from resources</summary>
		/// <returns>Lines from PE file</returns>
		public IEnumerable<ResourceString> GetStrings()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING))
				yield return new ResourceString(directory);
		}

		/// <summary>Get all accelerators from resources</summary>
		/// <returns>Accelerators</returns>
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

		/// <summary>Get a description of windows in an application</summary>
		/// <returns>List of dialogs in the application</returns>
		public IEnumerable<ResourceDialog> GetDialogs()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG))
				yield return new ResourceDialog(directory);
		}

		/// <summary>Get initialization information for windows in MFC applications</summary>
		/// <returns>List of resources with dialog initialization data in MFC applications</returns>
		public IEnumerable<ResourceDialogInit> GetDialogInint()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DLGINIT))
				yield return new ResourceDialogInit(directory);
		}

		/// <summary>Get menu description in app</summary>
		/// <returns>Menu list in app</returns>
		public IEnumerable<ResourceMenu> GetMenus()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU))
				yield return new ResourceMenu(directory);
		}

		/// <summary>Get descriptions of all toolbars in the application</summary>
		/// <returns>List of toolbars in the application</returns>
		public IEnumerable<ResourceToolBar> GetToolBars()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_TOOLBAR))
				yield return new ResourceToolBar(directory);
		}

		/// <summary>Get description of all pictures in resources</summary>
		/// <returns>List of all images in resources</returns>
		public IEnumerable<ResourceBitmap> GetBitmaps()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP))
				yield return new ResourceBitmap(directory);
		}

		/// <summary>Get description of all icons in resources</summary>
		/// <returns>List of all icons in resources</returns>
		public IEnumerable<ResourceIcon> GetIcons()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON))
				yield return new ResourceIcon(directory);
		}

		/// <summary>Get description of all resource directories</summary>
		/// <returns>List of all resource directories</returns>
		public IEnumerable<ResourceFontDir> GetFontDirs()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR))
				yield return new ResourceFontDir(directory);
		}

		/// <summary>Get fonts from resources</summary>
		/// <returns>List of all fonts from resources</returns>
		public IEnumerable<ResourceFont> GetFonts()
		{
			foreach(ResourceDirectory directory in this.GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT))
				yield return new ResourceFont(directory);
		}

		/// <summary>Get data directories</summary>
		/// <param name="directoryType">Type of directory resources from which to get</param>
		/// <returns>Directory corresponding to a specific directory</returns>
		public IEnumerable<ResourceDirectory> GetResources(WinNT.Resource.RESOURCE_DIRECTORY_TYPE directoryType)
		{
			foreach(var directory in this)
				if(directory.DirectoryEntry.NameType == directoryType)
					foreach(var subDir1 in directory)//Identifiers
						foreach(var subDir2 in subDir1)
							yield return subDir2;
		}

		/// <summary>Get the directory by address</summary>
		/// <remarks>Only works on data directories</remarks>
		/// <param name="directoryAddress">Data directory address</param>
		/// <returns>Directory with data</returns>
		public ResourceDirectory GetResource(UInt32 directoryAddress)
		{
			foreach(var directory in this)
				foreach(var subDir1 in directory)//Identifiers
					foreach(var subDir2 in subDir1)
						if(subDir2.DirectoryEntry.DirectoryAddress == directoryAddress)
							return subDir2;

			return null;
		}

		/// <summary>Get resource directories from image directory</summary>
		/// <returns>Resource directories</returns>
		public IEnumerator<ResourceDirectory> GetEnumerator()
		{
			var root = this.Header;
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

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}