using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
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
			/// <summary>Defines a menu item in a menu template.</summary>
			public WinNT.Resource.MENUITEM? Item;
			/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu.</summary>
			public WinNT.Resource.MENUITEMEX? ItemEx;
			/// <summary>Menu item title</summary>
			public String Title;
			/// <summary>Popup items</summary>
			public MenuPopupItem[] SubItems;
			/// <summary>Extended template</summary>
			public Boolean IsExMenu { get { return this.ItemEx.HasValue; } }
			/// <summary>Specifies that the menu item is a separator.</summary>
			public Boolean IsSeparator
			{
				get
				{
					if(this.IsExMenu)
						return (this.ItemEx.Value.dwType & WinNT.Resource.MFT.SEPARATOR) == WinNT.Resource.MFT.SEPARATOR;
					else return String.IsNullOrEmpty(this.Title);
				}
			}
		}
		/// <summary>Popup menu item</summary>
		[DefaultProperty("Title")]
		public class MenuPopupItem
		{
			/// <summary>Help ID</summary>
			public UInt32 dwHelpId;
			/// <summary>Defines a menu item in a menu template.</summary>
			public WinNT.Resource.MENUITEMPOPUP? Item;
			/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu.</summary>
			public WinNT.Resource.MENUITEMEX? ItemEx;
			/// <summary>Menu item title</summary>
			public String Title;
			/// <summary>Popup items</summary>
			public MenuPopupItem[] SubItems;
			/// <summary>Extended template</summary>
			public Boolean IsExMenu { get { return this.ItemEx.HasValue; } }
			/// <summary>Specifies that the menu item is a separator.</summary>
			public Boolean IsSepearator
			{
				get
				{
					if(this.IsExMenu)
						return (this.ItemEx.Value.dwType & WinNT.Resource.MFT.SEPARATOR) == WinNT.Resource.MFT.SEPARATOR;
					else
						return this.Item.Value.mtID == 0 && String.IsNullOrEmpty(this.Title);
				}
			}
		}
		/// <summary>Resource menu header</summary>
		public WinNT.Resource.MENUHEADER Header
		{
			get { return PinnedBufferReader.BytesToStructure<WinNT.Resource.MENUHEADER>(base.Directory.GetData(), 0); }
		}
		/// <summary>Create instance of menu resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceMenu(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU)
		{

		}
		/// <summary>Get menu from resources</summary>
		/// <remarks>
		/// На выбор:
		/// Описание меню: http://msdn.microsoft.com/en-us/library/windows/desktop/ms647558%28v=vs.85%29.aspx
		/// Описание всех ресурсов включая меню: http://msdn.microsoft.com/en-us/library/windows/desktop/ms648007%28v=vs.85%29.aspx
		/// </remarks>
		/// <returns>Menu</returns>
		public MenuItem[] GetMenuTemplate()
		{
			var header = this.Header;
			UInt32 padding = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.MENUHEADER)) + header.cbHeaderSize;
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
		/// <exception cref="T:InvalidOperationException">Can't find end of menu marker</exception>
		/// <returns>Menu items</returns>
		private MenuItem[] GetMenu(UInt32 padding)
		{
			List<MenuItem> result = new List<MenuItem>();
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					padding = NativeMethods.AlignToInt(padding);

					WinNT.Resource.MENUITEMEX item = reader.BytesToStructure<WinNT.Resource.MENUITEMEX>(ref padding);
					MenuItem menu = new MenuItem()
					{
						ItemEx = item,
					};
					result.Add(menu);

					menu.Title = reader.BytesToStringUni(ref padding);

					//TODO: bResInfo в 16 битных приложениях занимает не WORD, а BYTE. Если найдётся реальный пример, придётся полностью делать динамичесую структуру.
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
		/// <summary>Gets popup menu items from padding</summary>
		/// <param name="reader">Menu bytes</param>
		/// <param name="padding">Starting popup menu padding</param>
		/// <exception cref="T:InvalidOperationException">Can't find end of menu marker</exception>
		/// <returns>Readed popup menu items</returns>
		private MenuPopupItem[] GetPopupMenu(PinnedBufferReader reader, ref UInt32 padding)
		{
			List<MenuPopupItem> result = new List<MenuPopupItem>();
			while(padding < reader.Length)
			{
				padding = NativeMethods.AlignToInt(padding);

				WinNT.Resource.MENUITEMEX item = reader.BytesToStructure<WinNT.Resource.MENUITEMEX>(ref padding);
				MenuPopupItem menu = new MenuPopupItem()
				{
					ItemEx = item,
				};
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
		/// <summary>Получить меню старого образца</summary>
		/// <param name="padding">Отступ от начала меню</param>
		/// <returns>Элементы меню</returns>
		private MenuItem[] GetMenuOld(UInt32 padding)
		{
			List<MenuItem> result = new List<MenuItem>();
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					WinNT.Resource.MENUITEM item = reader.BytesToStructure<WinNT.Resource.MENUITEM>(ref padding);
					MenuItem menu = new MenuItem()
					{
						Item = item,
					};
					result.Add(menu);

					menu.Title = reader.BytesToStringUni(ref padding);

					if(item.IsPopUp)
						menu.SubItems = this.GetPopupMenuOld(reader, ref padding);
					if(item.IsFinal)
						return result.ToArray();
				}
			//throw new InvalidOperationException();C:\Program Files\Windows NT\Accessories\wordpad.exe - Нет MF_END
			return result.ToArray();
		}
		/// <summary>Получить всплывающее меню</summary>
		/// <param name="reader">Allocated bytes in memory</param>
		/// <param name="padding">Отступ от начала</param>
		/// <exception cref="T:InvalidOperationException">Cand find end of menu marker</exception>
		/// <returns>Всплывающее меню</returns>
		private MenuPopupItem[] GetPopupMenuOld(PinnedBufferReader reader, ref UInt32 padding)
		{
			List<MenuPopupItem> result = new List<MenuPopupItem>();
			while(padding < reader.Length)
			{
				WinNT.Resource.MENUITEMPOPUP item = reader.BytesToStructure<WinNT.Resource.MENUITEMPOPUP>(padding);
				if(item.IsPopUp)
				{
					item.mtID = 0;
					padding += (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.MENUITEM));
				} else//Если это попап, то читать надо только 1 WORD.
					padding += (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.MENUITEMPOPUP));

				MenuPopupItem menu = new MenuPopupItem()
				{
					Item = item,
				};
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