using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Win32 API dialog TextBox resource control template</summary>
	public class DialogEditTemplate : DialogItemTemplate
	{
		/// <summary>Prevents the user from typing or editing text in the edit control.</summary>
		public Boolean IsReadOnly
		{
			get { return (((UInt32)base.Styles) & (UInt32)WinUser.ES.READONLY) == (UInt32)WinUser.ES.READONLY; }
		}

		/// <summary>Converts all characters to uppercase as they are typed into the edit control.</summary>
		public Boolean IsUpperCase
		{
			get { return (((UInt32)base.Styles) & (UInt32)WinUser.ES.UPPERCASE) == (UInt32)WinUser.ES.UPPERCASE; }
		}

		/// <summary>Converts all characters to lowercase as they are typed into the edit control.</summary>
		public Boolean IsLowerCase
		{
			get { return (((UInt32)base.Styles) & (UInt32)WinUser.ES.LOWERCASE) == (UInt32)WinUser.ES.LOWERCASE; }
		}

		/// <summary>Create instance of dialog TextBox resource control template</summary>
		/// <param name="control"></param>
		/// <param name="controlEx"></param>
		/// <param name="itemClass"></param>
		/// <param name="itemText"></param>
		/// <param name="extraData"></param>
		public DialogEditTemplate(WinUser.DLGITEMTEMPLATE? control, WinUser.DLGITEMTEMPLATEEX? controlEx, ResourceBase.SzInt itemClass, ResourceBase.SzInt itemText, Byte[] extraData)
			: base(control, controlEx, itemClass, itemText, extraData)
		{
		}
	}
}
