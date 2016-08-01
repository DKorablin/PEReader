using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Interface for the Windows Common Controls</summary>
	public struct CommCtrl
	{
		/// <summary>structure of RT_TOOLBAR resource</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct TOOLBARDATA
		{
			/// <summary>version # should be 1</summary>
			public UInt16 wVersion;
			/// <summary>width of one bitmap</summary>
			public UInt16 wWidth;
			/// <summary>height of one bitmap</summary>
			public UInt16 wHeight;
			/// <summary>buttons count</summary>
			public UInt16 wItemsCount;
			// array of command IDs, actual size is wItemCount
			//public UInt16[] items;
		}

		/// <summary>Contains information about a button in a toolbar.</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/bb760476(v=vs.85).aspx</remarks>
		public struct TBBUTTON
		{
			/// <summary>
			/// Zero-based index of the button image. Set this member to I_IMAGECALLBACK, and the toolbar will send the TBN_GETDISPINFO notification code to retrieve the image index when it is needed.
			/// </summary>
			/// <remarks>
			/// Version 5.81. Set this member to I_IMAGENONE to indicate that the button does not have an image. The button layout will not include any space for a bitmap, only text.
			/// If the button is a separator, that is, if fsStyle is set to BTNS_SEP, iBitmap determines the width of the separator, in pixels. For information on selecting button images from image lists, see TB_SETIMAGELIST message.
			/// </remarks>
			public UInt32 iBitmap;
			/// <summary>Command identifier associated with the button. This identifier is used in a WM_COMMAND message when the button is chosen. </summary>
			public UInt32 idCommand;
			/// <summary>Button state flags. This member can be a combination of the values listed in Toolbar Button States. </summary>
			public TBSTATE fsState;
			/// <summary>Button style. This member can be a combination of the button style values listed in Toolbar Control and Button Styles. </summary>
			public TBSTYLE fsStyle;
			/// <summary>Application-defined value.</summary>
			public UInt32 dwData;
			/// <summary>Zero-based index of the button string, or a pointer to a string buffer that contains text for the button. </summary>
			public String iString;
		}

		/// <summary>This section lists the states a toolbar button can have</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/bb760437(v=vs.85).aspx</remarks>
		[Flags]
		public enum TBSTATE : ushort
		{
			/// <summary>The button has the TBSTYLE_CHECK style and is being clicked</summary>
			CHECKED = 1,
			/// <summary>Version 4.70. The button's text is cut off and an ellipsis is displayed.</summary>
			ELLIPSES = 0x40,
			/// <summary>The button accepts user input. A button that does not have this state is grayed.</summary>
			ENABLED = 4,
			/// <summary>The button is not visible and cannot receive user input.</summary>
			HIDDEN = 8,
			/// <summary>The button is grayed.</summary>
			INDETERMINATE = 16,
			/// <summary>Version 4.71. The button is marked. The interpretation of a marked item is dependent upon the application.</summary>
			MARKED = 0x0080,
			/// <summary>The button is being clicked.</summary>
			PRESSED = 2,
			/// <summary>The button is followed by a line break. The button must also have the TBSTATE_ENABLED state.</summary>
			WRAP = 32,
		}

		/// <summary>The following window styles are specific to toolbars. They are combined with other window styles when the toolbar is created.</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/bb760439(v=vs.85).aspx</remarks>
		[Flags]
		public enum TBSTYLE : ushort
		{
			/// <summary>
			/// Allows users to change a toolbar button's position by dragging it while holding down the ALT key.
			/// If this style is not specified, the user must hold down the SHIFT key while dragging a button.
			/// Note that the CCS_ADJUSTABLE style must be specified to enable toolbar buttons to be dragged.
			/// </summary>
			ALTDRAG = 1024,
			/// <summary>Version 4.70. Generates NM_CUSTOMDRAW notification codes when the toolbar processes WM_ERASEBKGND messages.</summary>
			CUSTOMERASE = 8192,
			/// <summary>
			/// Version 4.70. Creates a flat toolbar.
			/// In a flat toolbar, both the toolbar and the buttons are transparent and hot-tracking is enabled.
			/// Button text appears under button bitmaps.
			/// To prevent repainting problems, this style should be set before the toolbar control becomes visible.
			/// </summary>
			FLAT = 2048,
			/// <summary>
			/// Version 4.70. Creates a flat toolbar with button text to the right of the bitmap.
			/// Otherwise, this style is identical to TBSTYLE_FLAT. To prevent repainting problems, this style should be set before the toolbar control becomes visible.
			/// </summary>
			LIST = 4096,
			/// <summary>Version 4.71. Generates TBN_GETOBJECT notification codes to request drop target objects when the cursor passes over toolbar buttons.</summary>
			REGISTERDROP = 0x4000,
			/// <summary>Creates a tooltip control that an application can use to display descriptive text for the buttons in the toolbar.</summary>
			TOOLTIPS = 256,
			/// <summary>
			/// Version 4.71. Creates a transparent toolbar.
			/// In a transparent toolbar, the toolbar is transparent but the buttons are not.
			/// Button text appears under button bitmaps.
			/// To prevent repainting problems, this style should be set before the toolbar control becomes visible.
			/// </summary>
			TRANSPARENT = 0x8000,
			/// <summary>
			/// Creates a toolbar that can have multiple lines of buttons.
			/// Toolbar buttons can "wrap" to the next line when the toolbar becomes too narrow to include all buttons on the same line.
			/// When the toolbar is wrapped, the break will occur on either the rightmost separator or the rightmost button if there are no separators on the bar.
			/// This style must be set to display a vertical toolbar control when the toolbar is part of a vertical rebar control. This style cannot be combined with CCS_VERT.
			/// </summary>
			WRAPABLE = 512,

			/// <summary>
			/// Version 5.80.
			/// Specifies that the toolbar control should not assign the standard width to the button.
			/// Instead, the button's width will be calculated based on the width of the text plus the image of the button.
			/// Use the equivalent style flag, TBSTYLE_AUTOSIZE, for version 4.72 and earlier.
			/// </summary>
			BTNS_AUTOSIZE = TBSTYLE.AUTOSIZE,
			/// <summary>
			/// Version 5.80.
			/// Creates a standard button.
			/// Use the equivalent style flag, TBSTYLE_BUTTON, for version 4.72 and earlier.
			/// This flag is defined as 0, and should be used to signify that no other flags are set.
			/// </summary>
			BTNS_BUTTON = TBSTYLE.BUTTON,
			/// <summary>
			/// Version 5.80.
			/// Creates a dual-state push button that toggles between the pressed and nonpressed states each time the user clicks it.
			/// The button has a different background color when it is in the pressed state.
			/// Use the equivalent style flag, TBSTYLE_CHECK, for version 4.72 and earlier.
			/// </summary>
			BTNS_CHECK = TBSTYLE.CHECK,
			/// <summary>
			/// Version 5.80.
			/// Creates a button that stays pressed until another button in the group is pressed, similar to option buttons (also known as radio buttons).
			/// It is equivalent to combining BTNS_CHECK and BTNS_GROUP.
			/// Use the equivalent style flag, TBSTYLE_CHECKGROUP, for version 4.72 and earlier.
			/// </summary>
			BTNS_CHECKGROUP = TBSTYLE.CHECKGROUP,
			/// <summary>
			/// Version 5.80.
			/// Creates a drop-down style button that can display a list when the button is clicked.
			/// Instead of the WM_COMMAND message used for normal buttons, drop-down buttons send a TBN_DROPDOWN notification code.
			/// An application can then have the notification handler display a list of options.
			/// Use the equivalent style flag, TBSTYLE_DROPDOWN, for version 4.72 and earlier. 
			/// </summary>
			/// <remarks>
			/// If the toolbar has the TBSTYLE_EX_DRAWDDARROWS extended style, drop-down buttons will have a drop-down arrow displayed in a separate section to their right.
			/// If the arrow is clicked, a TBN_DROPDOWN notification code will be sent.
			/// If the associated button is clicked, a WM_COMMAND message will be sent.
			/// </remarks>
			BTNS_DROPDOWN = TBSTYLE.DROPDOWN,
			/// <summary>
			/// Version 5.80.
			/// When combined with BTNS_CHECK, creates a button that stays pressed until another button in the group is pressed.
			/// Use the equivalent style flag, TBSTYLE_GROUP, for version 4.72 and earlier.
			/// </summary>
			BTNS_GROUP = TBSTYLE.GROUP,
			/// <summary>
			/// Version 5.80.
			/// Specifies that the button text will not have an accelerator prefix associated with it.
			/// Use the equivalent style flag, TBSTYLE_NOPREFIX, for version 4.72 and earlier.
			/// </summary>
			BTNS_NOPREFIX = TBSTYLE.NOPREFIX,
			/// <summary>
			/// Version 5.80.
			/// Creates a separator, providing a small gap between button groups.
			/// A button that has this style does not receive user input.
			/// Use the equivalent style flag, TBSTYLE_SEP, for version 4.72 and earlier.
			/// </summary>
			BTNS_SEP = TBSTYLE.SEP,
			/// <summary>
			/// Version 5.81.
			/// Specifies that button text should be displayed.
			/// All buttons can have text, but only those buttons with the BTNS_SHOWTEXT button style will display it.
			/// This button style must be used with the TBSTYLE_LIST style and the TBSTYLE_EX_MIXEDBUTTONS extended style.
			/// If you set text for buttons that do not have the BTNS_SHOWTEXT style, the toolbar control will automatically display it as a tooltip when the cursor hovers over the button.
			/// This feature allows your application to avoid handling the TBN_GETINFOTIP or TTN_GETDISPINFO notification code for the toolbar.
			/// </summary>
			BTNS_SHOWTEXT = 0x0040,
			/// <summary>
			/// Version 5.80.
			/// Specifies that the button will have a drop-down arrow, but not as a separate section.
			/// Buttons with this style behave the same, regardless of whether the TBSTYLE_EX_DRAWDDARROWS extended style is set.
			/// </summary>
			BTNS_WHOLEDROPDOWN = 0x0080,
			/// <summary>
			/// Version 5.80.
			/// Specifies that the toolbar control should not assign the standard width to the button.
			/// Instead, the button's width will be calculated based on the width of the text plus the image of the button.
			/// Use the equivalent style flag, TBSTYLE_AUTOSIZE, for version 4.72 and earlier.
			/// </summary>
			AUTOSIZE = 16,
			/// <summary>
			/// Version 5.80.
			/// Creates a standard button.
			/// Use the equivalent style flag, TBSTYLE_BUTTON, for version 4.72 and earlier.
			/// This flag is defined as 0, and should be used to signify that no other flags are set.
			/// </summary>
			BUTTON = 0,
			/// <summary>
			/// Version 5.80.
			/// Creates a dual-state push button that toggles between the pressed and nonpressed states each time the user clicks it.
			/// The button has a different background color when it is in the pressed state.
			/// Use the equivalent style flag, TBSTYLE_CHECK, for version 4.72 and earlier.
			/// </summary>
			CHECK = 2,
			/// <summary>
			/// Version 5.80.
			/// Creates a button that stays pressed until another button in the group is pressed, similar to option buttons (also known as radio buttons).
			/// It is equivalent to combining BTNS_CHECK and BTNS_GROUP.
			/// Use the equivalent style flag, TBSTYLE_CHECKGROUP, for version 4.72 and earlier.
			/// </summary>
			CHECKGROUP = TBSTYLE.GROUP | TBSTYLE.CHECK,
			/// <summary>
			/// Version 5.80.
			/// Creates a drop-down style button that can display a list when the button is clicked.
			/// Instead of the WM_COMMAND message used for normal buttons, drop-down buttons send a TBN_DROPDOWN notification code.
			/// An application can then have the notification handler display a list of options.
			/// Use the equivalent style flag, TBSTYLE_DROPDOWN, for version 4.72 and earlier. 
			/// </summary>
			/// <remarks>
			/// If the toolbar has the TBSTYLE_EX_DRAWDDARROWS extended style, drop-down buttons will have a drop-down arrow displayed in a separate section to their right.
			/// If the arrow is clicked, a TBN_DROPDOWN notification code will be sent.
			/// If the associated button is clicked, a WM_COMMAND message will be sent.
			/// </remarks>
			DROPDOWN = 8,
			/// <summary>
			/// Version 5.80.
			/// When combined with BTNS_CHECK, creates a button that stays pressed until another button in the group is pressed.
			/// Use the equivalent style flag, TBSTYLE_GROUP, for version 4.72 and earlier.
			/// </summary>
			GROUP = 4,
			/// <summary>
			/// Version 5.80.
			/// Specifies that the button text will not have an accelerator prefix associated with it.
			/// Use the equivalent style flag, TBSTYLE_NOPREFIX, for version 4.72 and earlier.
			/// </summary>
			NOPREFIX = 32,
			/// <summary>
			/// Version 5.80.
			/// Creates a separator, providing a small gap between button groups.
			/// A button that has this style does not receive user input.
			/// Use the equivalent style flag, TBSTYLE_SEP, for version 4.72 and earlier.
			/// </summary>
			SEP = 1,
		}
	}
}