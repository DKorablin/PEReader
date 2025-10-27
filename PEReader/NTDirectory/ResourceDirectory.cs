using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Resource directory</summary>
	[DefaultProperty(nameof(DirectoryEntry))]
	public class ResourceDirectory : IEnumerable<ResourceDirectory>, ISectionData
	{
		#region Fields
		internal static readonly UInt32 DirectorySize = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.IMAGE_RESOURCE_DIRECTORY));
		internal static readonly UInt32 DirectoryEntrySize = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY));

		private WinNT.Resource.IMAGE_RESOURCE_DATA_ENTRY? _dataEntry;
		#endregion Fields
		#region Properties
		/// <summary>Root node of resources</summary>
		public Resource Root { get; }

		/// <summary>Parent directory of the current directory</summary>
		public ResourceDirectory Parent { get; }

		/// <summary>Directory description</summary>
		public WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY DirectoryEntry { get; }

		/// <summary>Directory descriptor</summary>
		public WinNT.Resource.IMAGE_RESOURCE_DIRECTORY? Directory
			=> this.DirectoryEntry.IsDirectory
				? this.Root.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY>(this.Root.Directory.VirtualAddress + this.DirectoryEntry.DirectoryAddress)
				: (WinNT.Resource.IMAGE_RESOURCE_DIRECTORY?)null;

		/// <summary>The name of the directory</summary>
		public String Name
		{
			get
			{
				if(this.DirectoryEntry.IsNameString)
				{
					WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_STRING result = this.Root.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_STRING>(this.Root.Directory.VirtualAddress + this.DirectoryEntry.NameAddress);
					return result.Name;
				} else if(this.Parent == null)
				{//Only the root folder can have a constant name specified.
					switch(this.DirectoryEntry.NameType)
					{
					case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.Undefined:
						return this.DirectoryEntry.NameAddress.ToString();
					default:
						return this.DirectoryEntry.NameType.ToString();
					}
				} else
					return this.DirectoryEntry.NameAddress.ToString();
			}
		}

		/// <summary>Data description</summary>
		public WinNT.Resource.IMAGE_RESOURCE_DATA_ENTRY? DataEntry
		{
			get
			{
				if(!this.DirectoryEntry.IsDirectory && !this.DirectoryEntry.IsNameString)
				{
					if(this._dataEntry == null)
						this._dataEntry = this.Root.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DATA_ENTRY>(this.Root.Directory.VirtualAddress + this.DirectoryEntry.DirectoryAddress);
					return this._dataEntry.Value;
				} else
					return null;
			}
		}

		#endregion Properties
		/// <summary>Create instance of resource root directory class</summary>
		/// <param name="entry">Directory description</param>
		/// <param name="root">PE directory</param>
		public ResourceDirectory(WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY entry, Resource root)
			: this(entry, root, null) { }

		/// <summary>Create instance of resource directory class</summary>
		/// <param name="entry">Directory description</param>
		/// <param name="root">PE directory</param>
		/// <param name="parent">Parent directory</param>
		public ResourceDirectory(WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY entry, Resource root, ResourceDirectory parent)
		{
			this.DirectoryEntry = entry;
			this.Root = root ?? throw new ArgumentNullException(nameof(root));
			this.Parent = parent;
		}

		/// <summary>Get all data in directory</summary>
		public Byte[] GetData()
		{//TODO: Not implemented VS_VERSIONINFO. All information is here: http://msdn.microsoft.com/en-us/library/ms647001%28v=vs.85%29.aspx
		 //TODO: Not implemented RT_ICON. All information is here: http://www.codeproject.com/Articles/30644/Replacing-ICON-resources-in-EXE-and-DLL-files
			WinNT.Resource.IMAGE_RESOURCE_DATA_ENTRY? dataEntry = this.DataEntry;
			return dataEntry == null
				? null
				: this.Root.Parent.Header.ReadBytes(dataEntry.Value.OffsetToData, dataEntry.Value.Size);
		}

		/// <summary>Get a list of all directories in the current directory</summary>
		/// <returns>List of subdirectories</returns>
		public IEnumerator<ResourceDirectory> GetEnumerator()
		{
			var root = this.Directory;
			if(root.HasValue && root.Value.ContainsEntries)
			{
				UInt32 directorySize = ResourceDirectory.DirectorySize;
				UInt32 size = ResourceDirectory.DirectoryEntrySize;

				for(UInt16 loop = 0;loop < root.Value.NumberOfEntries;loop++)
				{
					UInt32 padding = this.Root.Directory.VirtualAddress + directorySize + this.DirectoryEntry.DirectoryAddress + (size * loop);
					WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY result = this.Root.Parent.Header.PtrToStructure<WinNT.Resource.IMAGE_RESOURCE_DIRECTORY_ENTRY>(padding);
					yield return new ResourceDirectory(result, this.Root, this);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>Convert langId to string representation</summary>
		/// <param name="langId">culture identifier</param>
		/// <returns></returns>
		public static String GetLangName(Int32 langId)
		{
			switch(langId)
			{
			case 0x0000:
				return "Language Neutral";
			case 0x0400:
				return "Process or User Default Language";
			case 0x0800:
				return "System Default Language";
			default:
				return CultureInfo.GetCultureInfo(langId).DisplayName;
			}
		}
	}
}