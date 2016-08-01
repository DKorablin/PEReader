using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Win32 API dialog generic resource control template</summary>
	public class DialogItemTemplate
	{
		/// <summary>Predefined system class</summary>
		public enum ControlSystemClass : ushort
		{
			/// <summary>Button</summary>
			Button = 0x0080,

			/// <summary>TextBox</summary>
			Edit = 0x0081,

			/// <summary>Label</summary>
			Static = 0x0082,

			/// <summary>ListBox</summary>
			ListBox = 0x0083,

			/// <summary>ScrollBar</summary>
			ScrollBar = 0x0084,

			/// <summary>ComboBox</summary>
			ComboBox = 0x0085,
		}

		/// <summary>Help ID</summary>
		public readonly UInt32 HelpID;

		/// <summary>Control styles</summary>
		public readonly WinUser.WS Styles;

		/// <summary>Control extended styles</summary>
		public readonly WinUser.WS_EX StylesEx;

		/// <summary>X location</summary>
		public readonly Int16 X;

		/// <summary>Y location</summary>
		public readonly Int16 Y;

		/// <summary>Width</summary>
		public readonly Int16 CX;

		/// <summary>Height</summary>
		public readonly Int16 CY;

		/// <summary>ID</summary>
		public readonly UInt32 ControlID;

		/// <summary>Control system class</summary>
		public readonly ControlSystemClass ItemSystemClass;

		/// <summary>Control COM class</summary>
		public readonly String ItemClass;

		/// <summary>Item title text</summary>
		public readonly ResourceBase.SzInt? ItemText;

		/// <summary>Control item extended data</summary>
		public readonly Byte[] ExtraData;

		/// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
		public Boolean IsDisabled
		{
			get { return (this.Styles & WinUser.WS.WS_DISABLED) == WinUser.WS.WS_DISABLED; }
		}

		/// <summary>Win32 API dialog generic resource control template</summary>
		/// <param name="control">Defines the dimensions and style of a control in a dialog box.</param>
		/// <param name="controlEx">A block of text used by an extended dialog box template to describe the extended dialog box.</param>
		/// <param name="itemClass">Type of dialog item template (predefined control or COM control)</param>
		/// <param name="itemText">Item title text</param>
		/// <param name="extraData">Control item extended data</param>
		public DialogItemTemplate(WinUser.DLGITEMTEMPLATE? control, WinUser.DLGITEMTEMPLATEEX? controlEx, ResourceBase.SzInt itemClass, ResourceBase.SzInt itemText, Byte[] extraData)
		{
			if(control == null && controlEx == null)
				throw new ArgumentNullException("control or controlEx must be not null");

			if(control != null)
			{
				this.Styles = control.Value.style;
				this.StylesEx = control.Value.dwExtendedStyle;
				this.X = control.Value.x;
				this.Y = control.Value.y;
				this.CX = control.Value.cx;
				this.CY = control.Value.cy;
				this.ControlID = control.Value.id;
			}

			if(controlEx != null)
			{
				this.HelpID = controlEx.Value.helpID;
				this.Styles = controlEx.Value.style;
				this.StylesEx = controlEx.Value.exStyle;
				this.X = controlEx.Value.x;
				this.Y = controlEx.Value.y;
				this.CX = controlEx.Value.cx;
				this.CY = controlEx.Value.cy;
				this.ControlID = controlEx.Value.id;
			}

			if(!itemClass.IsEmpty)
				switch(itemClass.Type)
				{
				case ResourceBase.SzInt.SzIntResult.Index:
					this.ItemSystemClass = (DialogItemTemplate.ControlSystemClass)itemClass.Index;
					break;
				case ResourceBase.SzInt.SzIntResult.Name:
					this.ItemClass = itemClass.Name;
					break;
				default:
					throw new NotSupportedException();
				}

			if(!itemText.IsEmpty)
				this.ItemText = itemText;
			this.ExtraData = extraData;
		}

		/// <summary>Create instance of extended Win32 API tempate control described by itemClass member</summary>
		/// <param name="control">Defines the dimensions and style of a control in a dialog box.</param>
		/// <param name="controlEx">A block of text used by an extended dialog box template to describe the extended dialog box.</param>
		/// <param name="itemClass">Type of dialog item template (predefined control or COM control)</param>
		/// <param name="itemText">Item title text</param>
		/// <param name="extraData">Control item extended data</param>
		/// <returns>Win32 API dialog item template</returns>
		public static DialogItemTemplate CreateDialogItem(WinUser.DLGITEMTEMPLATE? control, WinUser.DLGITEMTEMPLATEEX? controlEx, ResourceBase.SzInt itemClass, ResourceBase.SzInt itemText, Byte[] extraData)
		{
			if(itemClass.IsEmpty || itemClass.Type == ResourceBase.SzInt.SzIntResult.Name)
				return new DialogItemTemplate(control, controlEx, itemClass, itemText, extraData);
			else
				switch((DialogItemTemplate.ControlSystemClass)itemClass.Index)
				{
				case ControlSystemClass.Button:
					return new DialogButtonTemplate(control, controlEx, itemClass, itemText, extraData);
				case ControlSystemClass.Edit:
					return new DialogEditTemplate(control, controlEx, itemClass, itemText, extraData);
				default:
					return new DialogItemTemplate(control, controlEx, itemClass, itemText, extraData);
				}
		}
	}
}