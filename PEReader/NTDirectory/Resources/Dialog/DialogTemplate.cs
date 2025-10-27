using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Win32 API dialog template descriptor</summary>
	public class DialogTemplate
	{
		private ResourceBase.SzInt? _menu;
		private ResourceBase.SzInt? _windowName;

		/// <summary>Template version</summary>
		public UInt16? Version;

		/// <summary>Signature</summary>
		public UInt16? Signature;

		/// <summary>Help ID</summary>
		public UInt32? helpID;

		/// <summary>Dialog styles</summary>
		public WinUser.WS Styles;

		/// <summary>Dialog EX styles</summary>
		public WinUser.WS_EX StylesEx;

		/// <summary>X coordinate</summary>
		public Int16 X;

		/// <summary>Y coordinate</summary>
		public Int16 Y;

		/// <summary>Width</summary>
		public Int16 CX;

		/// <summary>height</summary>
		public Int16 CY;

		/// <summary>Dialog menu index</summary>
		public ResourceBase.SzInt? Menu
		{
			get => this._menu;
			set
			{
				if(value != null && !value.Value.IsEmpty)
					this._menu = value;
			}
		}

		/// <summary>WindowName</summary>
		public ResourceBase.SzInt? WindowName
		{
			get => this._windowName;
			set
			{
				if(value != null && !value.Value.IsEmpty)
					this._windowName = value;
			}
		}
		/// <summary>Dialog title</summary>
		public String Title;

		/// <summary>Dialog font</summary>
		public DialogFont? Font;

		/// <summary>Controls</summary>
		public DialogItemTemplate[] Controls;

		/// <summary>Create instance of the Win32 API dialog template</summary>
		/// <param name="template">An extended dialog box template begins with a DLGTEMPLATEEX header that describes the dialog box and specifies the number of controls in the dialog box.</param>
		public DialogTemplate(WinUser.DLGTEMPLATEEX template)
		{
			this.Version = template.dlgVer;
			this.Signature = template.signature;
			this.helpID = template.helpID;
			this.Styles = template.style;
			this.StylesEx = template.exStyle;
			this.X = template.x;
			this.Y = template.y;
			this.CX = template.cx;
			this.CY = template.cy;

			this.Controls = new DialogItemTemplate[template.cDlgItems];
		}

		/// <summary>Create instance of the Win32 API dialog template</summary>
		/// <param name="templateOld">Defines the dimensions and style of a dialog box.</param>
		public DialogTemplate(WinUser.DLGTEMPLATE templateOld)
		{
			this.Styles = templateOld.style;
			this.StylesEx = templateOld.dwExtendedStyle;
			this.X = templateOld.x;
			this.Y = templateOld.y;
			this.CX = templateOld.cx;
			this.CY = templateOld.cy;

			this.Controls = new DialogItemTemplate[templateOld.cdit];
		}
	}
}