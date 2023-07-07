using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Menu resource class</summary>
	[DefaultProperty("Header")]
	public class ResourceMenu : ResourceBase
	{
		/// <summary>Root menu item</summary>
		[DefaultProperty("Title")]
		public class MenuItem
		{
			/// <summary>Help ID</summary>
			public UInt32 dwHelpId;

			/// <summary>Defines a menu item in a menu template</summary>
			public WinUser.MENUITEM? Item;

			/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu</summary>
			public WinUser.MENUITEMEX? ItemEx;

			/// <summary>Menu item title</summary>
			public String Title;

			/// <summary>Popup items</summary>
			public MenuPopupItem[] SubItems;

			/// <summary>Extended template</summary>
			public Boolean IsExMenu { get { return this.ItemEx.HasValue; } }

			/// <summary>Specifies that the menu item is a separator</summary>
			public Boolean IsSeparator
			{
				get
				{
					return this.IsExMenu
						? (this.ItemEx.Value.dwType & WinUser.MFT.SEPARATOR) == WinUser.MFT.SEPARATOR
						: String.IsNullOrEmpty(this.Title);
				}
			}
		}
		/// <summary>Popup menu item</summary>
		[DefaultProperty("Title")]
		public class MenuPopupItem
		{
			/// <summary>Help ID</summary>
			public UInt32 dwHelpId;

			/// <summary>Defines a menu item in a menu template</summary>
			public WinUser.MENUITEMPOPUP? Item;

			/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu</summary>
			public WinUser.MENUITEMEX? ItemEx;

			/// <summary>Menu item title</summary>
			public String Title;

			/// <summary>Popup items</summary>
			public MenuPopupItem[] SubItems;

			/// <summary>Extended template</summary>
			public Boolean IsExMenu { get { return this.ItemEx.HasValue; } }

			/// <summary>Specifies that the menu item is a separator</summary>
			public Boolean IsSepearator
			{
				get
				{
					return this.IsExMenu
						? (this.ItemEx.Value.dwType & WinUser.MFT.SEPARATOR) == WinUser.MFT.SEPARATOR
						: this.Item.Value.mtID == 0 && String.IsNullOrEmpty(this.Title);
				}
			}
		}

		/// <summary>Resource menu header</summary>
		public WinUser.MENUHEADER Header
		{
			get { return PinnedBufferReader.BytesToStructure<WinUser.MENUHEADER>(base.Directory.GetData(), 0); }
		}

		/// <summary>Create instance of menu resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceMenu(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU)
		{
		}

		/// <summary>Get menu from resources</summary>
		/// <remarks>
		/// Coice:
		/// Menu description: http://msdn.microsoft.com/en-us/library/windows/desktop/ms647558%28v=vs.85%29.aspx
		/// Description of all resources incuding menu: http://msdn.microsoft.com/en-us/library/windows/desktop/ms648007%28v=vs.85%29.aspx
		/// </remarks>
		/// <returns>Menu</returns>
		public IEnumerable<MenuItem> GetMenuTemplate()
		{
			WinUser.MENUHEADER header = this.Header;
			UInt32 padding = (UInt32)Marshal.SizeOf(typeof(WinUser.MENUHEADER)) + header.cbHeaderSize;
			if(header.IsOldFormat)
				return this.GetMenuOld(padding);
			else
			{
				return this.GetMenu(padding);
				/*WinNT.MENUITEMEX menu = NativeMethods.BytesToStructure<WinNT.MENUITEMEX>(bytes, padding);
				padding += (UInt32)Marshal.SizeOf(typeof(WinNT.MENUITEMEX));
				String menuTitle = NativeMethods.BytesToString(bytes, ref padding);
				padding = ResourceBase.AlignToInt(padding);*/
			}
		}

		/// <summary>Gets menu items from padding</summary>
		/// <param name="padding">Starting menu padding</param>
		/// <exception cref="InvalidOperationException">Can't find end of menu marker</exception>
		/// <returns>Menu items</returns>
		private IEnumerable<MenuItem> GetMenu(UInt32 padding)
		{
			//List<MenuItem> result = new List<MenuItem>();
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					padding = NativeMethods.AlignToInt(padding);

					WinUser.MENUITEMEX item = reader.BytesToStructure<WinUser.MENUITEMEX>(ref padding);
					MenuItem menu = new MenuItem() { ItemEx = item, };
					//result.Add(menu);

					menu.Title = reader.BytesToStringUni(ref padding);

					//TODO: bResInfo в 16 битных приложениях занимает не WORD, а BYTE. Если найдётся реальный пример, придётся полностью делать динамичесую структуру.
					if(item.IsPopUp)
					{
						menu.dwHelpId = reader.BytesToStructure<UInt32>(ref padding);
						menu.SubItems = this.GetPopupMenu(reader, ref padding);
					}

					yield return menu;
					if(item.IsFinal)
						yield break;
						//return result.ToArray();
				}
			throw new InvalidOperationException();
		}

		/// <summary>Gets popup menu items from padding</summary>
		/// <param name="reader">Menu bytes</param>
		/// <param name="padding">Starting popup menu padding</param>
		/// <exception cref="InvalidOperationException">Can't find end of menu marker</exception>
		/// <returns>Readed popup menu items</returns>
		private MenuPopupItem[] GetPopupMenu(PinnedBufferReader reader, ref UInt32 padding)
		{
			List<MenuPopupItem> result = new List<MenuPopupItem>();
			while(padding < reader.Length)
			{
				padding = NativeMethods.AlignToInt(padding);

				WinUser.MENUITEMEX item = reader.BytesToStructure<WinUser.MENUITEMEX>(ref padding);
				MenuPopupItem menu = new MenuPopupItem() { ItemEx = item, };
				result.Add(menu);

				menu.Title = reader.BytesToStringUni(ref padding);

				if(item.IsPopUp)
				{
					menu.dwHelpId = reader.BytesToStructure<UInt32>(ref padding);
					menu.SubItems = this.GetPopupMenu(reader, ref padding);
				}

				if(item.IsFinal)
					return result.ToArray();
			}
			throw new InvalidOperationException();
		}

		/// <summary>Get the old style menu</summary>
		/// <param name="padding">Indent from the beginning of the menu</param>
		/// <returns>Menu elements</returns>
		private IEnumerable<MenuItem> GetMenuOld(UInt32 padding)
		{
			//List<MenuItem> result = new List<MenuItem>();
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					WinUser.MENUITEM item = reader.BytesToStructure<WinUser.MENUITEM>(ref padding);
					MenuItem menu = new MenuItem() { Item = item, };
					//result.Add(menu);

					menu.Title = reader.BytesToStringUni(ref padding);

					if(item.IsPopUp)
						menu.SubItems = this.GetPopupMenuOld(reader, ref padding);

					yield return menu;
					if(item.IsFinal)
						yield break;
						//return result.ToArray();
				}
			//throw new InvalidOperationException();C:\Program Files\Windows NT\Accessories\wordpad.exe - Нет MF_END
			//return result.ToArray();
		}

		/// <summary>Get the popup menu</summary>
		/// <param name="reader">Allocated bytes in memory</param>
		/// <param name="padding">Padding for beginning</param>
		/// <exception cref="InvalidOperationException">Cand find end of menu marker</exception>
		/// <returns>Popup menu</returns>
		private MenuPopupItem[] GetPopupMenuOld(PinnedBufferReader reader, ref UInt32 padding)
		{
			List<MenuPopupItem> result = new List<MenuPopupItem>();
			while(padding < reader.Length)
			{
				WinUser.MENUITEMPOPUP item = reader.BytesToStructure<WinUser.MENUITEMPOPUP>(padding);
				if(item.IsPopUp)
				{
					item.mtID = 0;
					padding += (UInt32)Marshal.SizeOf(typeof(WinUser.MENUITEM));
				} else//Если это попап, то читать надо только 1 WORD.
					padding += (UInt32)Marshal.SizeOf(typeof(WinUser.MENUITEMPOPUP));

				MenuPopupItem menu = new MenuPopupItem() { Item = item, };
				result.Add(menu);

				menu.Title = reader.BytesToStringUni(ref padding);

				if(item.IsPopUp)
					menu.SubItems = this.GetPopupMenuOld(reader, ref padding);
				if(item.IsFinal)
					return result.ToArray();
			}
			throw new InvalidOperationException();
		}
	}
}