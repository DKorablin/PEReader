using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Win32 API dialog button resource control template</summary>
	public class DialogButtonTemplate : DialogItemTemplate
	{
		/// <summary>Creates a rectangle in which other controls can be grouped. Any text associated with this style is displayed in the rectangle's upper left corner.</summary>
		public Boolean IsGroupBox
		{
			get
			{
				return (((UInt32)base.Styles) & (UInt32)WinUser.BS.GROUPBOX) == (UInt32)WinUser.BS.GROUPBOX;
			}
		}

		/// <summary>Creates a small, empty check box with text. By default, the text is displayed to the right of the check box. To display the text to the left of the check box, combine this flag with the BS_LEFTTEXT style (or with the equivalent BS_RIGHTBUTTON style).</summary>
		public Boolean IsCheckBox
		{
			get
			{
				return (((UInt32)base.Styles) & (UInt32)WinUser.BS.CHECKBOX) == (UInt32)AlphaOmega.Debug.WinUser.BS.CHECKBOX;
			}
		}

		/// <summary>Creates a small circle with text. By default, the text is displayed to the right of the circle. To display the text to the left of the circle, combine this flag with the BS_LEFTTEXT style (or with the equivalent BS_RIGHTBUTTON style). Use radio buttons for groups of related, but mutually exclusive choices.</summary>
		public Boolean IsRadioButton
		{
			get
			{
				if(this.IsGroupBox)
					return false;

				return (((UInt32)base.Styles) & (UInt32)WinUser.BS.RADIOBUTTON) == (UInt32)WinUser.BS.RADIOBUTTON
						|| (((UInt32)base.Styles) & (UInt32)WinUser.BS.AUTORADIOBUTTON) == (UInt32)WinUser.BS.AUTORADIOBUTTON;
			}
		}

		/// <summary>Creates a push button that posts a WM_COMMAND message to the owner window when the user selects the button.</summary>
		public Boolean IsButton
		{
			get { return !this.IsGroupBox && !this.IsCheckBox && !this.IsRadioButton; }
		}

		/// <summary>Specifies that the button is two-dimensional; it does not use the default shading to create a 3-D image. </summary>
		public Boolean IsFlat
		{
			get { return (((UInt32)base.Styles) & (UInt32)WinUser.BS.FLAT) == (UInt32)WinUser.BS.FLAT; }
		}

		/// <summary>Create instance of dialog button resource control template</summary>
		public DialogButtonTemplate(WinUser.DLGITEMTEMPLATE? control, WinUser.DLGITEMTEMPLATEEX? controlEx, ResourceBase.SzInt itemClass, ResourceBase.SzInt itemText, Byte[] extraData)
			: base(control, controlEx, itemClass, itemText, extraData)
		{
		}
	}
}