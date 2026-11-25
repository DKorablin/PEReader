using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Menu resource class</summary>
	[DefaultProperty(nameof(Header))]
	public class ResourceMenu : ResourceBase
	{
		/// <summary>Menu item on any level of the menu</summary>
		[DefaultProperty(nameof(Title))]
		public class MenuItem
		{
			/// <summary>Help ID</summary>
			/// <remarks>Present only in an extended menu template for an item that opens a submenu</remarks>
			public UInt32 dwHelpId;

			/// <summary>Defines a menu item in a standard menu template</summary>
			public WinUser.MENUITEMTEMPLATE? Item;

			/// <summary>Defines a menu item in an extended menu template</summary>
			public WinUser.MENUITEMEX? ItemEx;

			/// <summary>Menu item title</summary>
			public String Title;

			/// <summary>Items of the submenu opened by this item</summary>
			/// <remarks><c>null</c> if the item does not open a submenu</remarks>
			public MenuPopupItem[] SubItems;

			/// <summary>Extended template</summary>
			public Boolean IsExMenu => this.ItemEx.HasValue;

			/// <summary>Menu item identifier</summary>
			/// <remarks>In a standard menu template an item that opens a submenu has no identifier and 0 is returned</remarks>
			public UInt32 MenuId
				=> this.IsExMenu
					? this.ItemEx.Value.menuId
					: this.Item?.mtID ?? 0;

			/// <summary>The item opens a drop-down menu or submenu</summary>
			public Boolean IsPopUp
				=> this.IsExMenu
					? this.ItemEx.Value.IsPopUp
					: this.Item?.IsPopUp ?? false;

			/// <summary>Specifies that the menu item is a separator</summary>
			/// <remarks>
			/// Same as Windows: an item flagged as separator or a text item with an empty title that does not open a submenu.
			/// Unlike Windows, an item with an empty title that opens a submenu (POPUP "" in shortcut menus) is not treated as a separator, so its submenu stays reachable.
			/// </remarks>
			public Boolean IsSeparator
			{
				get
				{
					Boolean isSeparator, isText;
					if(this.IsExMenu)
					{
						WinUser.MFT type = this.ItemEx.Value.dwType;
						isSeparator = (type & WinUser.MFT.SEPARATOR) == WinUser.MFT.SEPARATOR;
						isText = (type & (WinUser.MFT.BITMAP | WinUser.MFT.OWNERDRAW)) == 0;
					} else if(this.Item.HasValue)
					{
						WinUser.MF option = this.Item.Value.mtOption;
						isSeparator = (option & WinUser.MF.SEPARATOR) == WinUser.MF.SEPARATOR;
						isText = (option & (WinUser.MF.BITMAP | WinUser.MF.OWNERDRAW)) == 0;
					} else
						return false;

					return isSeparator
						|| (isText && !this.IsPopUp && String.IsNullOrEmpty(this.Title));//RC writes "MENUITEM SEPARATOR" as an item with no ID and no text
				}
			}
		}

		/// <summary>Menu item inside a drop-down menu or submenu</summary>
		/// <remarks>Has the same layout as <see cref="MenuItem"/> and is kept as a separate type for compatibility</remarks>
		[DefaultProperty(nameof(Title))]
		public class MenuPopupItem : MenuItem { }

		/// <summary>Maximum supported submenu nesting level</summary>
		/// <remarks>Protects the recursive reader from a stack overflow on malformed resources</remarks>
		private const Int32 MaxMenuDepth = 64;

		private static readonly UInt32 SizeOfHeader = (UInt32)Marshal.SizeOf(typeof(WinUser.MENUHEADER));
		private static readonly UInt32 SizeOfItemEx = (UInt32)Marshal.SizeOf(typeof(WinUser.MENUITEMEX));

		/// <summary>Resource menu header</summary>
		public WinUser.MENUHEADER Header
			=> PinnedBufferReader.BytesToStructure<WinUser.MENUHEADER>(base.Directory.GetData(), 0);

		/// <summary>Help identifier of the whole menu</summary>
		/// <remarks>Present only in an extended menu template (MENUEX_TEMPLATE_HEADER.dwHelpId)</remarks>
		public UInt32? HelpId
		{
			get
			{
				using(PinnedBufferReader reader = base.CreateDataReader())
				{
					WinUser.MENUHEADER header = reader.BytesToStructure<WinUser.MENUHEADER>(0);
					return header.IsOldFormat || header.cbHeaderSize < sizeof(UInt32)
						? (UInt32?)null
						: reader.BytesToStructure<UInt32>(SizeOfHeader);
				}
			}
		}

		/// <summary>Create instance of menu resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceMenu(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU) { }

		/// <summary>Get menu from resources</summary>
		/// <remarks>
		/// Reading stops at the item marked as the last one (MF_END).
		/// If the marker is missing (Ex: wordpad.exe), the items are read until the end of the resource data.
		/// Menu template formats: https://learn.microsoft.com/en-us/windows/win32/menurc/resource-file-formats
		/// </remarks>
		/// <returns>Top level menu items</returns>
		public IEnumerable<MenuItem> GetMenuTemplate()
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
			{
				WinUser.MENUHEADER header = reader.BytesToStructure<WinUser.MENUHEADER>(0);

				//Standard template: cbHeaderSize is 0.
				//Extended template: cbHeaderSize is wOffset, the offset to the first item relative to the end of this member.
				UInt32 padding = SizeOfHeader + header.cbHeaderSize;
				return ResourceMenu.ReadItems<MenuItem>(reader, ref padding, !header.IsOldFormat, 0);
			}
		}

		/// <summary>Read the items of one menu level and, recursively, all of their submenus</summary>
		/// <typeparam name="T">Type of the created menu items</typeparam>
		/// <param name="reader">Menu resource data</param>
		/// <param name="padding">Offset of the first item; on return the offset after the last read item</param>
		/// <param name="isExtended">Extended (MENUEX) template</param>
		/// <param name="depth">Current submenu nesting level</param>
		/// <returns>Menu items of the current level</returns>
		private static T[] ReadItems<T>(PinnedBufferReader reader, ref UInt32 padding, Boolean isExtended, Int32 depth) where T : MenuItem, new()
		{
			List<T> result = new List<T>();
			Boolean isFinal = false;
			while(!isFinal)
			{
				T menu = new T();
				Boolean isRead = isExtended
					? ResourceMenu.ReadItemEx(reader, ref padding, menu, out isFinal)
					: ResourceMenu.ReadItem(reader, ref padding, menu, out isFinal);
				if(!isRead)
					break;//End of data without MF_END marker

				result.Add(menu);

				if(menu.IsPopUp && depth < MaxMenuDepth)
					menu.SubItems = ResourceMenu.ReadItems<MenuPopupItem>(reader, ref padding, isExtended, depth + 1);
			}
			return result.ToArray();
		}

		/// <summary>Read one item of a standard menu template</summary>
		/// <param name="reader">Menu resource data</param>
		/// <param name="padding">Offset of the item; on return the offset after the item</param>
		/// <param name="menu">Menu item to fill</param>
		/// <param name="isFinal">The item is the last one in the current menu level</param>
		/// <returns><c>false</c> if there is no data left for the item</returns>
		private static Boolean ReadItem(PinnedBufferReader reader, ref UInt32 padding, MenuItem menu, out Boolean isFinal)
		{
			isFinal = true;
			if(padding + sizeof(UInt16) > reader.Length)
				return false;

			WinUser.MENUITEMTEMPLATE item = new WinUser.MENUITEMTEMPLATE()
			{
				mtOption = (WinUser.MF)reader.BytesToStructure<UInt16>(ref padding),
			};

			if(!item.IsPopUp)
			{//mtID is present only for an item that does not open a submenu
				if(padding + sizeof(UInt16) > reader.Length)
					return false;
				item.mtID = reader.BytesToStructure<UInt16>(ref padding);
			}

			menu.Item = item;
			menu.Title = ResourceMenu.ReadTitle(reader, ref padding);
			isFinal = item.IsFinal;
			return true;
		}

		/// <summary>Read one item of an extended menu template</summary>
		/// <param name="reader">Menu resource data</param>
		/// <param name="padding">Offset of the item; on return the offset after the item</param>
		/// <param name="menu">Menu item to fill</param>
		/// <param name="isFinal">The item is the last one in the current menu level</param>
		/// <returns><c>false</c> if there is no data left for the item</returns>
		private static Boolean ReadItemEx(PinnedBufferReader reader, ref UInt32 padding, MenuItem menu, out Boolean isFinal)
		{
			isFinal = true;
			padding = NativeMethods.AlignToInt(padding);//Each MENUEX_TEMPLATE_ITEM starts on a DWORD boundary
			if(padding + SizeOfItemEx > reader.Length)
				return false;

			WinUser.MENUITEMEX item = reader.BytesToStructure<WinUser.MENUITEMEX>(ref padding);
			menu.ItemEx = item;
			menu.Title = ResourceMenu.ReadTitle(reader, ref padding);

			if(item.IsPopUp)
			{//dwHelpId is present only for an item that opens a submenu, and it starts on a DWORD boundary after the text
				padding = NativeMethods.AlignToInt(padding);
				if(padding + sizeof(UInt32) <= reader.Length)
					menu.dwHelpId = reader.BytesToStructure<UInt32>(ref padding);
			}

			isFinal = item.IsFinal;
			return true;
		}

		/// <summary>Read the null-terminated Unicode item text</summary>
		/// <param name="reader">Menu resource data</param>
		/// <param name="padding">Offset of the text; on return the offset after the null terminator</param>
		/// <returns>Item text or an empty string if there is no data left</returns>
		private static String ReadTitle(PinnedBufferReader reader, ref UInt32 padding)
			=> padding < reader.Length
				? reader.BytesToStringUni(ref padding)
				: String.Empty;
	}
}