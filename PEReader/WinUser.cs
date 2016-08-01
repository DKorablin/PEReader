using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>USER procedure declarations, constant definitions and macros</summary>
	public struct WinUser
	{
		/// <summary>
		/// A block of text used by an extended dialog box template to describe the extended dialog box.
		/// For a description of the format of an extended dialog box template, see <see cref="DLGTEMPLATEEX"/>.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/ms645389%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct DLGITEMTEMPLATEEX
		{
			/// <summary>The help context identifier for the control. When the system sends a WM_HELP message, it passes the helpID value in the dwContextId member of the HELPINFO structure.</summary>
			public UInt32 helpID;

			/// <summary>
			/// The extended styles for a window.
			/// This member is not used to create controls in dialog boxes, but applications that use dialog box templates can use it to create other types of windows.
			/// </summary>
			/// <remarks>For a list of values, see Extended Window Styles.</remarks>
			public WS_EX exStyle;

			/// <summary>
			/// The style of the control.
			/// This member can be a combination of window style values (such as WS_BORDER) and one or more of the control style values (such as BS_PUSHBUTTON and ES_LEFT).
			/// </summary>
			public WS style;

			/// <summary>
			/// The x-coordinate, in dialog box units, of the upper-left corner of the control.
			/// This coordinate is always relative to the upper-left corner of the dialog box's client area.
			/// </summary>
			public Int16 x;

			/// <summary>
			/// The y-coordinate, in dialog box units, of the upper-left corner of the control.
			/// This coordinate is always relative to the upper-left corner of the dialog box's client area.
			/// </summary>
			public Int16 y;

			/// <summary>The width, in dialog box units, of the control.</summary>
			public Int16 cx;

			/// <summary>The height, in dialog box units, of the control.</summary>
			public Int16 cy;

			/// <summary>The control identifier.</summary>
			public UInt32 id;
		}

		/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu.</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct MENUITEMEX
		{
			/// <summary>The menu item type.</summary>
			/// <remarks>This member can be a combination of the type (beginning with MFT) values listed with the MENUITEMINFO structure.</remarks>
			public MFT dwType;

			/// <summary>The menu item state.</summary>
			/// <remarks>This member can be a combination of the state (beginning with MFS) values listed with the MENUITEMINFO structure.</remarks>
			public MFS dwState;

			/// <summary>
			/// The menu item identifier.
			/// This is an application-defined value that identifies the menu item.
			/// In an extended menu resource, items that open drop-down menus or submenus as well as command items can have identifiers.
			/// </summary>
			public UInt32 menuId;

			/// <summary>
			/// Specifies whether the menu item is the last item in the menu bar, drop-down menu, submenu, or shortcut menu and whether it is an item that opens a drop-down menu or submenu.
			/// This member can be zero or more of these values.
			/// </summary>
			/// <remarks>For 32-bit applications, this member is a word; for 16-bit applications, it is a byte.</remarks>
			public MFe bResInfo;

			/// <summary>
			/// The structure defines a item that opens a drop-down menu or submenu.
			/// Subsequent structures define menu items in the corresponding drop-down menu or submenu.
			/// </summary>
			public Boolean IsPopUp { get { return (this.bResInfo & MFe.POPUP) == MFe.POPUP; } }

			/// <summary>The structure defines the last menu item in the menu bar, drop-down menu, submenu, or shortcut menu.</summary>
			public Boolean IsFinal { get { return (this.bResInfo & MFe.END) == MFe.END; } }
		}

		/// <summary>Defines a menu item in a menu template.</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct MENUITEMPOPUP
		{
			/// <summary>The type of menu item.</summary>
			public MF mtOption;

			/// <summary>Menu item ID</summary>
			public UInt16 mtID;

			/// <summary>
			/// The structure defines a item that opens a drop-down menu or submenu.
			/// Subsequent structures define menu items in the corresponding drop-down menu or submenu.
			/// </summary>
			public Boolean IsPopUp { get { return (this.mtOption & MF.POPUP) == MF.POPUP; } }

			/// <summary>The structure defines the last menu item in the menu bar, drop-down menu, submenu, or shortcut menu.</summary>
			public Boolean IsFinal { get { return (this.mtOption & MF.END) == MF.END; } }
		}

		/// <summary>Contains information about each item in a menu resource that does not open a menu or a submenu.</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms648024%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct MENUITEM
		{
			/// <summary>The type of menu item.</summary>
			public MF mtOption;

			//// <summary>A null-terminated Unicode string that contains the text for this menu item. There is no fixed limit on the size of this string.</summary>
			//public String menuText;
			// <summary>
			// A menu separator is a special type of menu item that is inactive but appears as a dividing bar between two active menu items.
			// Indicate a menu separator by leaving the menuText member empty.
			// </summary>
			//public Boolean IsSeparator { get { return String.IsNullOrEmpty(this.menuText); } }

			/// <summary>
			/// The structure defines a item that opens a drop-down menu or submenu.
			/// Subsequent structures define menu items in the corresponding drop-down menu or submenu.
			/// </summary>
			public Boolean IsPopUp { get { return (this.mtOption & MF.POPUP) == MF.POPUP; } }

			/// <summary>The structure defines the last menu item in the menu bar, drop-down menu, submenu, or shortcut menu.</summary>
			public Boolean IsFinal { get { return (this.mtOption & MF.END) == MF.END; } }
		}

		/// <summary>Contains version information for the menu resource. The structure definition provided here is for explanation only; it is not present in any standard header file.</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms648018%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct MENUHEADER
		{
			/// <summary>The version number of the menu template.</summary>
			/// <remarks>This member must be equal to zero to indicate that this is an RT_MENU created with a standard menu template.</remarks>
			public UInt16 wVersion;

			/// <summary>The size of the menu template header.</summary>
			/// <remarks>This value is zero for menus you create with a standard menu template.</remarks>
			public UInt16 cbHeaderSize;

			/// <summary>Old menu format</summary>
			public Boolean IsOldFormat { get { return this.wVersion == 0; } }

			/// <summary>New menu format</summary>
			public Boolean IsNewFormat { get { return this.wVersion == 1; } }
		}

		/// <summary>
		/// Defines the dimensions and style of a dialog box.
		/// This structure, always the first in a standard template for a dialog box, also specifies the number of controls in the dialog box and therefore specifies the number of subsequent DLGITEMTEMPLATE structures in the template.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/ms645394%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct DLGTEMPLATE
		{
			/// <summary>
			/// The style of the dialog box. This member can be a combination of window style values (such as WS_CAPTION and WS_SYSMENU) and dialog box style values (such as DS_CENTER).
			/// If the style member includes the DS_SETFONT style, the header of the dialog box template contains additional data specifying the font to use for text in the client area and controls of the dialog box. The font data begins on the WORD boundary that follows the title array. The font data specifies a 16-bit point size value and a Unicode font name string. If possible, the system creates a font according to the specified values. Then the system sends a WM_SETFONT message to the dialog box and to each control to provide a handle to the font. If DS_SETFONT is not specified, the dialog box template does not include the font data.
			/// The DS_SHELLFONT style is not supported in the DLGTEMPLATE header.
			/// </summary>
			public WS style;
			/// <summary>The extended styles for a window. This member is not used to create dialog boxes, but applications that use dialog box templates can use it to create other types of windows.</summary>
			public WS_EX dwExtendedStyle;
			/// <summary>The number of items in the dialog box. </summary>
			public UInt16 cdit;
			/// <summary>The x-coordinate, in dialog box units, of the upper-left corner of the dialog box.</summary>
			public Int16 x;
			/// <summary>The y-coordinate, in dialog box units, of the upper-left corner of the dialog box.</summary>
			public Int16 y;
			/// <summary>The width, in dialog box units, of the dialog box.</summary>
			public Int16 cx;
			/// <summary>The height, in dialog box units, of the dialog box.</summary>
			public Int16 cy;
			/// <summary>В структуре присутсвует информация по шрифту</summary>
			public Boolean ContainsFont { get { return (this.style & WS.DS_SETFONT) == WS.DS_SETFONT; } }
		}

		/// <summary>
		/// Defines the dimensions and style of a control in a dialog box.
		/// One or more of these structures are combined with a DLGTEMPLATE structure to form a standard template for a dialog box.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/ms644997%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct DLGITEMTEMPLATE
		{
			/// <summary>The style of the control. This member can be a combination of window style values (such as WS_BORDER) and one or more of the control style values (such as BS_PUSHBUTTON and ES_LEFT).</summary>
			public WS style;
			/// <summary>The extended styles for a window. This member is not used to create controls in dialog boxes, but applications that use dialog box templates can use it to create other types of windows. For a list of values, see Extended Window Styles.</summary>
			public WS_EX dwExtendedStyle;
			/// <summary>The x-coordinate, in dialog box units, of the upper-left corner of the control. This coordinate is always relative to the upper-left corner of the dialog box's client area.</summary>
			public Int16 x;
			/// <summary>The y-coordinate, in dialog box units, of the upper-left corner of the control. This coordinate is always relative to the upper-left corner of the dialog box's client area.</summary>
			public Int16 y;
			/// <summary>The width, in dialog box units, of the control.</summary>
			public Int16 cx;
			/// <summary>The height, in dialog box units, of the control.</summary>
			public Int16 cy;
			/// <summary>The control identifier.</summary>
			public UInt16 id;
		}

		/// <summary>
		/// An extended dialog box template begins with a DLGTEMPLATEEX header that describes the dialog box and specifies the number of controls in the dialog box.
		/// For each control in a dialog box, an extended dialog box template has a block of data that uses the DLGITEMTEMPLATEEX format to describe the control.
		/// The DLGTEMPLATEEX structure is not defined in any standard header file.
		/// The structure definition is provided here to explain the format of an extended template for a dialog box.
		/// </summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/ms645398%28v=vs.85%29.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct DLGTEMPLATEEX
		{
			/// <summary>The version number of the extended dialog box template. This member must be set to 1. </summary>
			public UInt16 dlgVer;
			/// <summary>
			/// Indicates whether a template is an extended dialog box template.
			/// If signature is 0xFFFF, this is an extended dialog box template.
			/// In this case, the dlgVer member specifies the template version number.
			/// If signature is any value other than 0xFFFF, this is a standard dialog box template that uses the DLGTEMPLATE and DLGITEMTEMPLATE structures.
			/// </summary>
			public UInt16 signature;
			/// <summary>The help context identifier for the dialog box window. When the system sends a WM_HELP message, it passes this value in the wContextId member of the HELPINFO structure.</summary>
			public UInt32 helpID;
			/// <summary>
			/// The extended windows styles.
			/// This member is not used when creating dialog boxes, but applications that use dialog box templates can use it to create other types of windows.
			/// For a list of values, see Extended Window Styles.
			/// </summary>
			public WS_EX exStyle;
			/// <summary>
			/// The style of the dialog box. This member can be a combination of window style values and dialog box style values.
			/// </summary>
			/// <remarks>
			/// If style includes the DS_SETFONT or DS_SHELLFONT dialog box style, the DLGTEMPLATEEX header of the extended dialog box template contains four additional members ( pointsize, weight, italic, and typeface) that describe the font to use for the text in the client area and controls of the dialog box. If possible, the system creates a font according to the values specified in these members.
			/// Then the system sends a WM_SETFONT message to the dialog box and to each control to provide a handle to the font.
			/// </remarks>
			public WS style;
			/// <summary>The number of controls in the dialog box.</summary>
			public UInt16 cDlgItems;
			/// <summary>The x-coordinate, in dialog box units, of the upper-left corner of the dialog box.</summary>
			public Int16 x;
			/// <summary>The y-coordinate, in dialog box units, of the upper-left corner of the dialog box.</summary>
			public Int16 y;
			/// <summary>The width, in dialog box units, of the dialog box.</summary>
			public Int16 cx;
			/// <summary>The height, in dialog box units, of the dialog box.</summary>
			public Int16 cy;
			/// <summary>Valid template</summary>
			public Boolean IsValid { get { return this.signature == 0xFFFF; } }
			/// <summary>В структуре присутсвует информация по шрифту</summary>
			public Boolean ContainsFont { get { return (this.style & WinUser.WS.DS_SETFONT) == WinUser.WS.DS_SETFONT || (this.style & WinUser.WS.DS_SHELLFONT) == WinUser.WS.DS_SHELLFONT; } }
		}

		/// <summary>Describes the data in an individual accelerator table resource. The structure definition provided here is for explanation only; it is not present in any standard header file.</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct ACCELTABLEENTRY
		{
			/// <summary>Describes keyboard accelerator characteristics. This member can have one or more of the following values from Winuser.h. </summary>
			public AccelFlags fFlags;
			/// <summary>An ANSI character value or a virtual-key code that identifies the accelerator key. </summary>
			public UInt16 wAnsi;
			/// <summary>An identifier for the keyboard accelerator. This is the value passed to the window procedure when the user presses the specified key. </summary>
			public UInt16 wId;
			/// <summary>The number of bytes inserted to ensure that the structure is aligned on a DWORD boundary. </summary>
			public UInt16 padding;
			/// <summary>Получить строковое представление wAnsi параметра</summary>
			/// <remarks>Для получения строкового представления используются Win32 вызовы</remarks>
			/// <exception cref="T:Win32Exception">Can't convert Key code to text key name</exception>
			/// <returns>Строковое представление параметра</returns>
			public String StringKey
			{
				get
				{
					StringBuilder result = new StringBuilder(10);
					UInt32 key = NativeMethods.MapVirtualKey((UInt32)this.wAnsi, NativeMethods.MAPVK.VK_TO_VSC);
					if(key == 0 || NativeMethods.GetKeyNameText(key << 16, result, 10) == 0)
						throw new Win32Exception();
					return result.ToString();
				}
			}
		}

		/// <summary>Describes keyboard accelerator characteristics.</summary>
		[Flags]
		public enum AccelFlags : short
		{
			/// <summary>The accelerator key is a virtual-key code. If this flag is not specified, the accelerator key is assumed to specify an ASCII character code. </summary>
			FVIRTKEY = 1,
			/// <summary>A menu item on the menu bar is not highlighted when an accelerator is used. This attribute is obsolete and retained only for backward compatibility with resource files designed for 16-bit Windows.</summary>
			FNOINVERT = 0x02,
			/// <summary>The accelerator is activated only if the user presses the SHIFT key. This flag applies only to virtual keys. </summary>
			FSHIFT = 0x04,
			/// <summary>The accelerator is activated only if the user presses the CTRL key. This flag applies only to virtual keys. </summary>
			FCONTROL = 0x08,
			/// <summary>The accelerator is activated only if the user presses the ALT key. This flag applies only to virtual keys. </summary>
			FALT = 0x10,
			/// <summary>The entry is last in an accelerator table. </summary>
			Last = 0x80,
		}

		/// <summary>Specifies a combination of button styles. If you create a button using the BUTTON class with the CreateWindow or CreateWindowEx function, you can specify any of the button styles listed below.</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/bb775951(v=vs.85).aspx</remarks>
		[Flags]
		public enum BS : uint
		{
			/// <summary>Creates a push button that posts a WM_COMMAND message to the owner window when the user selects the button.</summary>
			PUSHBUTTON = 0x00000000,
			/// <summary>Creates a push button that behaves like a BS_PUSHBUTTON style button, but has a distinct appearance. If the button is in a dialog box, the user can select the button by pressing the ENTER key, even when the button does not have the input focus. This style is useful for enabling the user to quickly select the most likely (default) option.</summary>
			DEFPUSHBUTTON = 0x00000001,
			/// <summary>Creates a small, empty check box with text. By default, the text is displayed to the right of the check box. To display the text to the left of the check box, combine this flag with the BS_LEFTTEXT style (or with the equivalent BS_RIGHTBUTTON style).</summary>
			CHECKBOX = 0x00000002,
			/// <summary>Creates a button that is the same as a check box, except that the check state automatically toggles between checked and cleared each time the user selects the check box.</summary>
			AUTOCHECKBOX = 0x00000003,
			/// <summary>Creates a small circle with text. By default, the text is displayed to the right of the circle. To display the text to the left of the circle, combine this flag with the BS_LEFTTEXT style (or with the equivalent BS_RIGHTBUTTON style). Use radio buttons for groups of related, but mutually exclusive choices.</summary>
			RADIOBUTTON = 0x00000004,
			/// <summary>Creates a button that is the same as a check box, except that the box can be grayed as well as checked or cleared. Use the grayed state to show that the state of the check box is not determined.</summary>
			_3STATE = 0x00000005,
			/// <summary>Creates a button that is the same as a three-state check box, except that the box changes its state when the user selects it. The state cycles through checked, indeterminate, and cleared.</summary>
			AUTO3STATE = 0x00000006,
			/// <summary>Creates a rectangle in which other controls can be grouped. Any text associated with this style is displayed in the rectangle's upper left corner.</summary>
			GROUPBOX = 0x00000007,
			/// <summary>Obsolete, but provided for compatibility with 16-bit versions of Windows. Applications should use BS_OWNERDRAW instead.</summary>
			USERBUTTON = 0x00000008,
			/// <summary>Creates a button that is the same as a radio button, except that when the user selects it, the system automatically sets the button's check state to checked and automatically sets the check state for all other buttons in the same group to cleared.</summary>
			AUTORADIOBUTTON = 0x00000009,
			//PUSHBOX=0x0000000A,
			/// <summary>Creates an owner-drawn button. The owner window receives a WM_DRAWITEM message when a visual aspect of the button has changed. Do not combine the BS_OWNERDRAW style with any other button styles.</summary>
			OWNERDRAW = 0x0000000B,
			/// <summary>Creates a split button. A split button has a drop down arrow.</summary>
			SPLITBUTTON = 0x0000000C,
			/// <summary>Creates a split button that behaves like a BS_PUSHBUTTON style button, but also has a distinctive appearance. If the split button is in a dialog box, the user can select the split button by pressing the ENTER key, even when the split button does not have the input focus. This style is useful for enabling the user to quickly select the most likely (default) option.</summary>
			DEFSPLITBUTTON = 0x0000000D,
			/// <summary>Creates a command link button that behaves like a BS_PUSHBUTTON style button, but the command link button has a green arrow on the left pointing to the button text. A caption for the button text can be set by sending the BCM_SETNOTE message to the button.</summary>
			COMMANDLINK = 0x0000000E,
			/// <summary>
			/// Do not use this style.
			/// A composite style bit that results from using the OR operator on BS_* style bits.
			/// It can be used to mask out valid BS_* bits from a given bitmask.
			/// Note that this is out of date and does not correctly include all valid styles.
			/// Thus, you should not use this style.
			/// </summary>
			TYPEMASK = 0x0000000F,
			/// <summary>Specifies that the button displays text.</summary>
			TEXT = 0x00000000,
			/// <summary>Places text on the left side of the radio button or check box when combined with a radio button or check box style. Same as the BS_RIGHTBUTTON style.</summary>
			LEFTTEXT = 0x00000020,
			/// <summary>Specifies that the button displays an icon. See the Remarks section for its interaction with BS_BITMAP.</summary>
			ICON = 0x00000040,
			/// <summary>Specifies that the button displays a bitmap. See the Remarks section for its interaction with BS_ICON.</summary>
			BITMAP = 0x00000080,
			//CONTENTMASK = 0x000000F0,
			/// <summary>Left-justifies the text in the button rectangle. However, if the button is a check box or radio button that does not have the BS_RIGHTBUTTON style, the text is left justified on the right side of the check box or radio button.</summary>
			LEFT = 0x00000100,
			/// <summary>Right-justifies text in the button rectangle. However, if the button is a check box or radio button that does not have the BS_RIGHTBUTTON style, the text is right justified on the right side of the check box or radio button.</summary>
			RIGHT = 0x00000200,
			/// <summary>Centers text horizontally in the button rectangle.</summary>
			CENTER = 0x00000300,
			/// <summary>Places text at the top of the button rectangle.</summary>
			TOP = 0x00000400,
			/// <summary>Places text at the bottom of the button rectangle.</summary>
			BOTTOM = 0x00000800,
			/// <summary>Places text in the middle (vertically) of the button rectangle.</summary>
			VCENTER = 0x00000C00,
			//REALSIZEIMAGE = 0x00000F00,
			//ALIGNMASK = 0x00000F00,
			/// <summary>Makes a button (such as a check box, three-state check box, or radio button) look and act like a push button. The button looks raised when it isn't pushed or checked, and sunken when it is pushed or checked.</summary>
			PUSHLIKE = 0x00001000,
			/// <summary>Wraps the button text to multiple lines if the text string is too long to fit on a single line in the button rectangle.</summary>
			MULTLINE = 0x00002000,
			/// <summary>
			/// Enables a button to send BN_KILLFOCUS and BN_SETFOCUS notification codes to its parent window.
			/// Note that buttons send the BN_CLICKED notification code regardless of whether it has this style. To get BN_DBLCLK notification codes, the button must have the BS_RADIOBUTTON or BS_OWNERDRAW style.
			/// </summary>
			NOTIFY = 0x00004000,
			//CHECKED = 0x00004000,
			/// <summary>Specifies that the button is two-dimensional; it does not use the default shading to create a 3-D image. </summary>
			FLAT = 0x00008000,
			//NOBORDER = 0x00010000,
			//Creates a command link button that behaves like a BS_PUSHBUTTON style button. If the button is in a dialog box, the user can select the command link button by pressing the ENTER key, even when the command link button does not have the input focus. This style is useful for enabling the user to quickly select the most likely (default) option.
			//DEFCOMMANDLINK,
			//Positions a radio button's circle or a check box's square on the right side of the button rectangle. Same as the BS_LEFTTEXT style.
			//RIGHTBUTTON,
		}

		/// <summary>To create an edit control using the CreateWindow or CreateWindowEx function, specify the EDIT class, appropriate window style constants, and a combination of the following edit control styles. After the control has been created, these styles cannot be modified, except as noted.</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/bb775464(v=vs.85).aspx</remarks>
		[Flags]
		public enum ES
		{
			/// <summary>Aligns text with the left margin.</summary>
			LEFT = 0x00000000,
			/// <summary>Centers text in a single-line or multiline edit control.</summary>
			CENTER = 0x00000001,
			/// <summary>Right-aligns text in a single-line or multiline edit control.</summary>
			RIGHT = 0x00000002,
			/// <summary>
			/// Designates a multiline edit control. The default is single-line edit control.
			/// When the multiline edit control is in a dialog box, the default response to pressing the ENTER key is to activate the default button. To use the ENTER key as a carriage return, use the ES_WANTRETURN style.
			/// When the multiline edit control is not in a dialog box and the ES_AUTOVSCROLL style is specified, the edit control shows as many lines as possible and scrolls vertically when the user presses the ENTER key. If you do not specify ES_AUTOVSCROLL, the edit control shows as many lines as possible and beeps if the user presses the ENTER key when no more lines can be displayed.
			/// If you specify the ES_AUTOHSCROLL style, the multiline edit control automatically scrolls horizontally when the caret goes past the right edge of the control. To start a new line, the user must press the ENTER key. If you do not specify ES_AUTOHSCROLL, the control automatically wraps words to the beginning of the next line when necessary. A new line is also started if the user presses the ENTER key. The window size determines the position of the Wordwrap. If the window size changes, the Wordwrapping position changes and the text is redisplayed.
			/// Multiline edit controls can have scroll bars. An edit control with scroll bars processes its own scroll bar messages. Note that edit controls without scroll bars scroll as described in the previous paragraphs and process any scroll messages sent by the parent window.
			/// </summary>
			MULTILINE = 0x00000004,
			/// <summary>Converts all characters to uppercase as they are typed into the edit control.</summary>
			/// <remarks>To change this style after the control has been created, use SetWindowLong.</remarks>
			UPPERCASE = 0x00000008,
			/// <summary>Converts all characters to lowercase as they are typed into the edit control.</summary>
			/// <remarks>To change this style after the control has been created, use SetWindowLong.</remarks>
			LOWERCASE = 0x00000010,
			/// <summary>
			/// Displays an asterisk (*) for each character typed into the edit control. This style is valid only for single-line edit controls.
			/// </summary>
			/// <remarks>
			/// To change the characters that is displayed, or set or clear this style, use the EM_SETPASSWORDCHAR message.
			/// Note  To use Comctl32.dll version 6, specify it in a manifest. For more information on manifests, see Enabling Visual Styles.
			/// </remarks>
			PASSWORD = 0x00000020,
			/// <summary>Automatically scrolls text up one page when the user presses the ENTER key on the last line.</summary>
			AUTOVSCROLL = 0x00000040,
			/// <summary>
			/// Automatically scrolls text to the right by 10 characters when the user types a character at the end of the line.
			/// When the user presses the ENTER key, the control scrolls all text back to position zero.
			/// </summary>
			AUTOHSCROLL = 0x00000080,
			/// <summary>
			/// Negates the default behavior for an edit control.
			/// The default behavior hides the selection when the control loses the input focus and inverts the selection when the control receives the input focus.
			/// If you specify ES_NOHIDESEL, the selected text is inverted, even if the control does not have the focus.
			/// </summary>
			NOHIDESEL = 0x00000100,
			/// <summary>
			/// Converts text entered in the edit control.
			/// The text is converted from the Windows character set to the OEM character set and then back to the Windows character set.
			/// This ensures proper character conversion when the application calls the CharToOem function to convert a Windows string in the edit control to OEM characters.
			/// This style is most useful for edit controls that contain file names that will be used on file systems that do not support Unicode.
			/// </summary>
			/// <remarks>To change this style after the control has been created, use SetWindowLong.</remarks>
			OEMCONVERT = 0x00000400,
			/// <summary>Prevents the user from typing or editing text in the edit control.</summary>
			/// <remarks>To change this style after the control has been created, use the EM_SETREADONLY message.</remarks>
			READONLY = 0x00000800,
			/// <summary>
			/// Specifies that a carriage return be inserted when the user presses the ENTER key while entering text into a multiline edit control in a dialog box.
			/// If you do not specify this style, pressing the ENTER key has the same effect as pressing the dialog box's default push button.
			/// This style has no effect on a single-line edit control.
			/// </summary>
			/// <remarks>To change this style after the control has been created, use SetWindowLong.</remarks>
			WANTRETURN = 0x00001000,
			//AUTOWRAP = 0x00002000,
			//TITLE = 0x00004000,
			//TIP = 0x00008000,
		}

		/// <summary>Describes the menu item.</summary>
		[Flags]
		public enum MF : ushort
		{
			/// <summary>Indicates that the menu item is initially inactive and drawn with a gray effect.</summary>
			GRAYED = 0x00000001,
			/// <summary>Indicates that the menu item has a check mark next to it.</summary>
			CHECKED = 0x00000008,
			/// <summary>The menu item opens a menu or a submenu; the flag is used internally by the system.</summary>
			POPUP = 0x00000010,
			/// <summary>
			/// Indicates that the menu item is placed in a new column.
			/// The old and new columns are separated by a bar.
			/// </summary>
			MENUBARBREAK = 0x00000020,
			/// <summary>Indicates that the menu item is placed in a new column.</summary>
			MENUBREAK = 0x00000040,
			/// <summary>The menu item is the last on the menu; the flag is used internally by the system.</summary>
			END = 0x00000080,
			/// <summary>
			/// Indicates that the owner window of the menu is responsible for drawing all visual aspects of the menu item, including highlighted, selected, and inactive states.
			/// This option is not valid for an item in a menu bar.
			/// </summary>
			OWNERDRAW = 0x00000100,
			/// <summary>Indicates that the menu item has a vertical separator to its left.</summary>
			HELP = 0x00004000,
		}

		/// <summary>The menu item state.</summary>
		/// <remarks>http://msdn.microsoft.com/en-us/library/windows/desktop/ms647578%28v=vs.85%29.aspx</remarks>
		[Flags]
		public enum MFS : int
		{
			/// <summary>Checks the menu item.</summary>
			CHECKED = 0x00000008,
			/// <summary>Specifies that the menu item is the default.</summary>
			/// <remarks>A menu can contain only one default menu item, which is displayed in bold.</remarks>
			DEFAULT = 0x00001000,
			/// <summary>Disables the menu item and grays it so that it cannot be selected.</summary>
			/// <remarks>This is equivalent to MFS_GRAYED.</remarks>
			DISABLED = 0x00000003,
			// <summary>Disables the menu item and grays it so that it cannot be selected.</summary>
			// <remarks>This is equivalent to MFS_DISABLED.</remarks>
			//GRAYED = 0x00000003,
			/// <summary>Enables the menu item so that it can be selected. This is the default state.</summary>
			ENABLED = 0x00000000,
			// <summary>Unchecks the menu item.</summary>
			//UNCHECKED=0x00000000,
			// <summary>Removes the highlight from the menu item. This is the default state.</summary>
			//UNHILITE=0x00000000,
			/// <summary>Highlights the menu item.</summary>
			HILITE = 0x00000080,
		}

		/// <summary>Menu item types.</summary>
		[Flags]
		public enum MFT : int
		{
			/// <summary>
			/// Displays the menu item using a text string.
			/// The dwTypeData member is the pointer to a null-terminated string, and the cch member is the length of the string.</summary>
			/// <remarks>MFT_STRING is replaced by MIIM_STRING.</remarks>
			STRING = 0x00000000,
			/// <summary>
			/// Displays the menu item using a bitmap.
			/// The low-order word of the dwTypeData member is the bitmap handle, and the cch member is ignored.
			/// </summary>
			/// <remarks>MFT_BITMAP is replaced by MIIM_BITMAP and hbmpItem. </remarks>
			BITMAP = 0x00000004,
			/// <summary>
			/// Places the menu item on a new line (for a menu bar) or in a new column (for a drop-down menu, submenu, or shortcut menu).
			/// For a drop-down menu, submenu, or shortcut menu, a vertical line separates the new column from the old.
			/// </summary>
			MENUBARBREAK = 0x00000020,
			/// <summary>
			/// Places the menu item on a new line (for a menu bar) or in a new column (for a drop-down menu, submenu, or shortcut menu).
			/// For a drop-down menu, submenu, or shortcut menu, the columns are not separated by a vertical line.
			/// </summary>
			MENUBREAK = 0x00000040,
			/// <summary>
			/// Assigns responsibility for drawing the menu item to the window that owns the menu.
			/// The window receives a WM_MEASUREITEM message before the menu is displayed for the first time, and a WM_DRAWITEM message whenever the appearance of the menu item must be updated.
			/// If this value is specified, the dwTypeData member contains an application-defined value.
			/// </summary>
			OWNERDRAW = 0x00000100,
			/// <summary>Displays selected menu items using a radio-button mark instead of a check mark if the hbmpChecked member is NULL.</summary>
			RADIOCHECK = 0x00000200,
			/// <summary>
			/// Specifies that the menu item is a separator.
			/// A menu item separator appears as a horizontal dividing line.
			/// The dwTypeData and cch members are ignored.
			/// </summary>
			/// <remarks>This value is valid only in a drop-down menu, submenu, or shortcut menu.</remarks>
			SEPARATOR = 0x00000800,
			/// <summary>
			/// Specifies that menus cascade right-to-left (the default is left-to-right).
			/// This is used to support right-to-left languages, such as Arabic and Hebrew.
			/// </summary>
			RIGHTORDER = 0x00002000,
			/// <summary>
			/// Right-justifies the menu item and any subsequent items.
			/// This value is valid only if the menu item is in a menu bar.
			/// </summary>
			RIGHTJUSTIFY = 0x00004000,
		}

		/// <summary>The following are the window styles. After the window has been created, these styles cannot be modified, except as noted.</summary>
		[Flags]
		public enum WS : uint
		{
			/// <summary>The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.</summary>
			WS_OVERLAPPED = 0x00000000,
			/// <summary>The window has a thin-line border.</summary>
			WS_BORDER = 0x00800000,
			/// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
			WS_CAPTION = 0x00C00000,
			/// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
			WS_CHILD = 0x40000000,
			// <summary>Same as the WS_CHILD style.</summary>
			//WS_CHILDWINDOW = 0x40000000,
			/// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
			WS_CLIPCHILDREN = 0x02000000,
			/// <summary>Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.</summary>
			WS_CLIPSIBLINGS = 0x04000000,
			/// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
			WS_DISABLED = 0x08000000,
			/// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
			WS_DLGFRAME = 0x00400000,
			/// <summary>
			/// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style. The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
			/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
			/// </summary>
			WS_GROUP = 0x00020000,
			/// <summary>The window has a horizontal scroll bar.</summary>
			WS_HSCROLL = 0x00100000,
			// <summary>The window is initially minimized. Same as the WS_MINIMIZE style.</summary>
			//WS_ICONIC = 0x20000000,
			/// <summary>The window is initially maximized.</summary>
			WS_MAXIMIZE = 0x01000000,
			/// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
			WS_MAXIMIZEBOX = 0x00010000,
			/// <summary>The window is initially minimized. Same as the WS_ICONIC style.</summary>
			WS_MINIMIZE = 0x20000000,
			/// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
			WS_MINIMIZEBOX = 0x00020000,
			/// <summary>The window is an overlapped window. Same as the WS_TILEDWINDOW style.</summary>
			WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
			/// <summary>The windows is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
			WS_POPUP = 0x80000000,
			/// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
			WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
			// <summary>The window has a sizing border. Same as the WS_THICKFRAME style.</summary>
			//WS_SIZEBOX = 0x00040000,
			/// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
			WS_SYSMENU = 0x00080000,
			/// <summary>
			/// The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
			/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
			/// </summary>
			WS_TABSTOP = 0x00010000,
			/// <summary>The window has a sizing border. Same as the WS_SIZEBOX style.</summary>
			WS_THICKFRAME = 0x00040000,
			// <summary>The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.</summary>
			//WS_TILED = 0x00000000,
			/// <summary>The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.</summary>
			WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
			/// <summary>
			/// The window is initially visible.
			/// This style can be turned on and off by using the ShowWindow or SetWindowPos function.
			/// </summary>
			WS_VISIBLE = 0x10000000,
			/// <summary>The window has a vertical scroll bar.</summary>
			WS_VSCROLL = 0x00200000,
			/// <summary>Indicates that the coordinates of the dialog box are screen coordinates. If this style is not specified, the coordinates are client coordinates.</summary>
			DS_ABSALIGN = 0x01,
			/// <summary>
			/// This style is obsolete and is included for compatibility with 16-bit versions of Windows.
			/// If you specify this style, the system creates the dialog box with the WS_EX_TOPMOST style.
			/// This style does not prevent the user from accessing other windows on the desktop.
			/// </summary>
			/// <remarks>Do not combine this style with the DS_CONTROL style.</remarks>
			DS_SYSMODAL = 0x02,
			/// <summary>
			/// Applies to 16-bit applications only.
			/// This style directs edit controls in the dialog box to allocate memory from the application's data segment.
			/// Otherwise, edit controls allocate storage from a global memory object.
			/// </summary>
			DS_LOCALEDIT = 0x20,
			/// <summary>
			/// Indicates that the header of the dialog box template (either standard or extended) contains additional data specifying the font to use for text in the client area and controls of the dialog box.
			/// If possible, the system selects a font according to the specified font data.
			/// The system passes a handle to the font to the dialog box and to each control by sending them the WM_SETFONT message.
			/// For descriptions of the format of this font data, see DLGTEMPLATE and DLGTEMPLATEEX.
			/// </summary>
			/// <remarks>If neither DS_SETFONT nor DS_SHELLFONT is specified, the dialog box template does not include the font data.</remarks>
			DS_SETFONT = 0x40,
			/// <summary>Creates a dialog box with a modal dialog-box frame that can be combined with a title bar and window menu by specifying the WS_CAPTION and WS_SYSMENU styles.</summary>
			DS_MODALFRAME = 0x80,
			/// <summary>Suppresses WM_ENTERIDLE messages that the system would otherwise send to the owner of the dialog box while the dialog box is displayed.</summary>
			DS_NOIDLEMSG = 0x100,
			/// <summary>
			/// Causes the system to use the SetForegroundWindow function to bring the dialog box to the foreground.
			/// This style is useful for modal dialog boxes that require immediate attention from the user regardless of whether the owner window is the foreground window.
			/// </summary>
			/// <remarks>
			/// The system restricts which processes can set the foreground window.
			/// For more information, see Foreground and Background Windows.
			/// </remarks>
			DS_SETFOREGROUND = 0x200,
			/// <summary>Obsolete. The system automatically applies the three-dimensional look to dialog boxes created by applications.</summary>
			DS_3DLOOK = 0x0004,
			/// <summary>
			/// Causes the dialog box to use the SYSTEM_FIXED_FONT instead of the default SYSTEM_FONT.
			/// This is a monospace font compatible with the System font in 16-bit versions of Windows earlier than 3.0.
			/// </summary>
			DS_FIXEDSYS = 0x0008,
			/// <summary>Creates the dialog box even if errors occur — for example, if a child window cannot be created or if the system cannot create a special data segment for an edit control.</summary>
			DS_NOFAILCREATE = 0x0010,
			/// <summary>
			/// Creates a dialog box that works well as a child window of another dialog box, much like a page in a property sheet.
			/// This style allows the user to tab among the control windows of a child dialog box, use its accelerator keys, and so on.
			/// </summary>
			DS_CONTROL = 0x0400,
			/// <summary>
			/// Centers the dialog box in the working area of the monitor that contains the owner window.
			/// If no owner window is specified, the dialog box is centered in the working area of a monitor determined by the system.
			/// The working area is the area not obscured by the taskbar or any appbars.
			/// </summary>
			DS_CENTER = 0x0800,
			/// <summary>Centers the dialog box on the mouse cursor.</summary>
			DS_CENTERMOUSE = 0x1000,
			/// <summary>
			/// Includes a question mark in the title bar of the dialog box.
			/// When the user clicks the question mark, the cursor changes to a question mark with a pointer.
			/// If the user then clicks a control in the dialog box, the control receives a WM_HELP message.
			/// The control should pass the message to the dialog box procedure, which should call the function using the HELP_WM_HELP command.
			/// The help application displays a pop-up window that typically contains help for the control.
			/// </summary>
			/// <remarks>
			/// Note that DS_CONTEXTHELP is only a placeholder.
			/// When the dialog box is created, the system checks for DS_CONTEXTHELP and, if it is there, adds WS_EX_CONTEXTHELP to the extended style of the dialog box.
			/// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
			/// </remarks>
			DS_CONTEXTHELP = 0x2000,
			/// <summary>
			/// Indicates that the dialog box should use the system font.
			/// The typeface member of the extended dialog box template must be set to MS Shell Dlg. Otherwise, this style has no effect.
			/// It is also recommended that you use the DIALOGEX Resource, rather than the DIALOG Resource.
			/// For more information, see Dialog Box Fonts.
			/// </summary>
			/// <remarks>
			/// The system selects a font using the font data specified in the pointsize, weight, and italic members.
			/// The system passes a handle to the font to the dialog box and to each control by sending them the WM_SETFONT message.
			/// For descriptions of the format of this font data, see DLGTEMPLATEEX.
			/// If neither DS_SHELLFONT nor DS_SETFONT is specified, the extended dialog box template does not include the font data.
			/// </remarks>
			DS_SHELLFONT = DS_SETFONT | DS_FIXEDSYS,
			/// <summary>Specifies use of pixels, not dialog coordinates.</summary>
			DS_USEPIXELS = 0x8000,
		}

		/// <summary>The following are the extended window styles.</summary>
		[Flags]
		public enum WS_EX : uint
		{
			/// <summary>The window accepts drag-drop files.</summary>
			ACCEPTFILES = 0x00000010,
			/// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
			APPWINDOW = 0x00040000,
			/// <summary>The window has a border with a sunken edge.</summary>
			CLIENTEDGE = 0x00000200,
			/// <summary>
			/// Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks.
			/// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
			/// </summary>
			/// <remarks>Windows 2000:  This style is not supported.</remarks>
			COMPOSITED = 0x02000000,
			/// <summary>
			/// The title bar of the window includes a question mark.
			/// When the user clicks the question mark, the cursor changes to a question mark with a pointer.
			/// If the user then clicks a child window, the child receives a WM_HELP message.
			/// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
			/// The Help application displays a pop-up window that typically contains help for the child window.
			/// </summary>
			/// <remarks>WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.</remarks>
			CONTEXTHELP = 0x00000400,
			/// <summary>
			/// The window itself contains child windows that should take part in dialog box navigation.
			/// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
			/// </summary>
			CONTROLPARENT = 0x00010000,
			/// <summary>The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.</summary>
			DLGMODALFRAME = 0x00000001,
			/// <summary>The window is a layered window. This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.</summary>
			/// <remarks>Windows 8:  The WS_EX_LAYERED style is supported for top-level windows and child windows. Previous Windows versions support WS_EX_LAYERED only for top-level windows.</remarks>
			LAYERED = 0x00080000,
			/// <summary>If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.</summary>
			LAYOUTRTL = 0x00400000,
			/// <summary>The window has generic left-aligned properties. This is the default.</summary>
			LEFT = 0x00000000,
			/// <summary>If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area.</summary>
			/// <remarks>For other languages, the style is ignored.</remarks>
			LEFTSCROLLBAR = 0x00004000,
			/// <summary>The window text is displayed using left-to-right reading-order properties. This is the default.</summary>
			LTRREADING = 0x00000000,
			/// <summary>The window is a MDI child window.</summary>
			MDICHILD = 0x00000040,
			/// <summary>
			/// A top-level window created with this style does not become the foreground window when the user clicks it.
			/// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
			/// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
			/// </summary>
			/// <remarks>The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.</remarks>
			NOACTIVATE = 0x08000000,
			/// <summary>The window does not pass its window layout to its child windows.</summary>
			NOINHERITLAYOUT = 0x00100000,
			/// <summary>The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.</summary>
			NOPARENTNOTIFY = 0x00000004,
			/// <summary>The window is an overlapped window.</summary>
			OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,
			/// <summary>The window is palette window, which is a modeless dialog box that presents an array of commands.</summary>
			PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,
			/// <summary>
			/// The window has generic "right-aligned" properties. This depends on the window class.
			/// This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
			/// </summary>
			/// <remarks>
			/// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively.
			/// Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
			/// </remarks>
			RIGHT = 0x00001000,
			/// <summary>The vertical scroll bar (if present) is to the right of the client area. This is the default.</summary>
			RIGHTSCROLLBAR = 0x00000000,
			/// <summary>
			/// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties.
			/// For other languages, the style is ignored.
			/// </summary>
			RTLREADING = 0x00002000,
			/// <summary>The window has a three-dimensional border style intended to be used for items that do not accept user input.</summary>
			STATICEDGE = 0x00020000,
			/// <summary>
			/// The window is intended to be used as a floating toolbar.
			/// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
			/// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
			/// If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
			/// </summary>
			TOOLWINDOW = 0x00000080,
			/// <summary>The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.</summary>
			TOPMOST = 0x00000008,
			/// <summary>
			/// The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
			/// The window appears transparent because the bits of underlying sibling windows have already been painted.
			/// </summary>
			/// <remarks>To achieve transparency without these restrictions, use the SetWindowRgn function.</remarks>
			TRANSPARENT = 0x00000020,
			/// <summary>The window has a border with a raised edge.</summary>
			WINDOWEDGE = 0x00000100,
		}

		/// <summary>Menu item type in PE file.</summary>
		[Flags]
		public enum MFe : ushort
		{
			/// <summary>
			/// The structure defines a item that opens a drop-down menu or submenu.
			/// Subsequent structures define menu items in the corresponding drop-down menu or submenu.
			/// </summary>
			POPUP = 0x01,
			/// <summary>The structure defines the last menu item in the menu bar, drop-down menu, submenu, or shortcut menu.</summary>
			END = 0x80,
		}
	}
}