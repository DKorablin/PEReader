using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Dialog resource class</summary>
	public class ResourceDialog : ResourceBase
	{
		/// <summary>Шаблон диалога</summary>
		public struct DialogTemplate
		{
			private SzInt? _menu;
			private SzInt? _windowName;

			/// <summary>Template version</summary>
			public UInt16? Version;
			/// <summary>Signature</summary>
			public UInt16? Signature;
			/// <summary>Help ID</summary>
			public UInt32? helpID;
			/// <summary>Dialog styles</summary>
			public WinNT.Resource.WS Styles;
			/// <summary>Dialog EX styles</summary>
			public WinNT.Resource.WS_EX StylesEx;
			/// <summary>X coordinate</summary>
			public Int16 X;
			/// <summary>Y coordinate</summary>
			public Int16 Y;
			/// <summary>Width</summary>
			public Int16 CX;
			/// <summary>height</summary>
			public Int16 CY;
			/// <summary>Dialog menu index</summary>
			public SzInt? Menu
			{
				get { return this._menu; }
				set
				{
					if(value.HasValue && !value.Value.IsEmpty)
						this._menu = value;
				}
			}
			/// <summary>WindowName</summary>
			public SzInt? WindowName
			{
				get { return this._windowName; }
				set
				{
					if(value.HasValue && !value.Value.IsEmpty)
						this._windowName = value;
				}
			}
			/// <summary>Dialog title</summary>
			public String Title;
			/// <summary>Dialog font</summary>
			public DialogFont? Font;
			/// <summary>Controls</summary>
			public DialogItemTemplate[] Controls;
		}
		/// <summary>Шаблон элемента диалога</summary>
		public struct DialogItemTemplate
		{
			/// <summary>Help ID</summary>
			public UInt32 HelpID;
			/// <summary>Control styles</summary>
			public WinNT.Resource.WS Styles;
			/// <summary>Control extended styles</summary>
			public WinNT.Resource.WS_EX StylesEx;
			/// <summary>X location</summary>
			public Int16 X;
			/// <summary>Y location</summary>
			public Int16 Y;
			/// <summary>Width</summary>
			public Int16 CX;
			/// <summary>Height</summary>
			public Int16 CY;
			/// <summary>ID</summary>
			public UInt32 ControlID;
			/// <summary>Control system class</summary>
			public ControlSystemClass ItemSystemClass;
			/// <summary>Control COM class</summary>
			public String ItemClass;
			/// <summary>Item title text</summary>
			public SzInt? ItemText;
			/// <summary>Control item extended data</summary>
			public Byte[] ExtraData;
		}
		/// <summary>Шрифт</summary>
		public struct DialogFont
		{
			/// <summary>Размер шрифта</summary>
			public UInt16 FontSize;
			/// <summary>Толщина шрифта</summary>
			public UInt16 FontWeight;
			/// <summary>Наклонный</summary>
			public Byte? Italic;
			/// <summary>Character Set</summary>
			public Byte? CharSet;
			/// <summary>Наименование шрифта</summary>
			public String TypeFace;
		}
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
		/// <summary>Create instance of dialog resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceDialog(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG)
		{

		}
		/// <summary>Get dialog template in resource directory</summary>
		/// <exception cref="T:NotImplementedException">Class type not implemented</exception>
		/// <returns>Dialog template from directory</returns>
		public DialogTemplate GetDialogTemplate()
		{
			UInt32 padding = 0;

			DialogTemplate result = new DialogTemplate();
			using(PinnedBufferReader reader = base.CreateDataReader())
			{
				WinNT.Resource.DLGTEMPLATEEX template = reader.BytesToStructure<WinNT.Resource.DLGTEMPLATEEX>(0);
				WinNT.Resource.DLGTEMPLATE templateOld;
				if(template.IsValid)
				{//Чтение нового шаблона диалогового окна
					templateOld = new WinNT.Resource.DLGTEMPLATE();
					padding = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.DLGTEMPLATEEX));

					result.Version = template.dlgVer;
					result.Signature = template.signature;
					result.helpID = template.helpID;
					result.Styles = template.style;
					result.StylesEx = template.exStyle;
					result.X = template.x;
					result.Y = template.y;
					result.CX = template.cx;
					result.CY = template.cy;

					result.Controls = new DialogItemTemplate[template.cDlgItems];
				} else
				{//Чтение старого шаблона диалогового окна
					templateOld = reader.BytesToStructure<WinNT.Resource.DLGTEMPLATE>(ref padding);

					result.Styles = templateOld.style;
					result.StylesEx = templateOld.dwExtendedStyle;
					result.X = templateOld.x;
					result.Y = templateOld.y;
					result.CX = templateOld.cx;
					result.CY = templateOld.cy;

					result.Controls = new DialogItemTemplate[templateOld.cdit];
				}

				result.Menu = ResourceBase.GetSzOrInt(reader, ref padding);//Идентификатор ресурса или наименование меню
				result.WindowName = ResourceBase.GetSzOrInt(reader, ref padding);//Идентификатор окна или наименование класса окна

				if(reader.BytesToStructure<UInt16>(padding) == 0x0000)
					padding += sizeof(UInt16);
				else//Чтение заголовка окна
					result.Title = reader.BytesToStringUni(ref padding);

				//Чтение информации о шрифте
				if((template.IsValid && template.ContainsFont)
					|| templateOld.ContainsFont)
				{
					DialogFont font = new DialogFont();
					font.FontSize = reader.BytesToStructure<UInt16>(ref padding);
					if(template.IsValid)
					{//Reaing new dialog font
						font.FontWeight = reader.BytesToStructure<UInt16>(ref padding);

						font.Italic = reader[padding++];
						font.CharSet = reader[padding++];
					}
					font.TypeFace = reader.BytesToStringUni(ref padding);
					result.Font = font;

					//padding += (padding % 4 == 0) ? 4 - (padding % 4) : 0;//Align to DWORD
					//padding += padding % sizeof(UInt32);//Align to DWORD
				}

				//Чтение  элементов управления, следующих после описателя окна
				for(Int32 loop = 0;loop < result.Controls.Length;loop++)
				{
					padding = NativeMethods.AlignToInt(padding);

					UInt32 startTemplate = padding;
					result.Controls[loop] = new DialogItemTemplate();
					if(template.IsValid)
					{
						WinNT.Resource.DLGITEMTEMPLATEEX controlEx = reader.BytesToStructure<WinNT.Resource.DLGITEMTEMPLATEEX>(ref padding);
						result.Controls[loop].HelpID = controlEx.helpID;
						result.Controls[loop].Styles = controlEx.style;
						result.Controls[loop].StylesEx = controlEx.exStyle;
						result.Controls[loop].X = controlEx.x;
						result.Controls[loop].Y = controlEx.y;
						result.Controls[loop].CX = controlEx.cx;
						result.Controls[loop].CY = controlEx.cy;
						result.Controls[loop].ControlID = controlEx.id;
					} else
					{
						WinNT.Resource.DLGITEMTEMPLATE control = reader.BytesToStructure<WinNT.Resource.DLGITEMTEMPLATE>(ref padding);
						result.Controls[loop].Styles = control.style;
						result.Controls[loop].StylesEx = control.dwExtendedStyle;
						result.Controls[loop].X = control.x;
						result.Controls[loop].Y = control.y;
						result.Controls[loop].CX = control.cx;
						result.Controls[loop].CY = control.cy;
						result.Controls[loop].ControlID = control.id;
					}

					SzInt itemClass = ResourceBase.GetSzOrInt(reader, ref padding);

					if(!itemClass.IsEmpty)
						switch(itemClass.Type)
						{
						case SzInt.SzIntResult.Index:
							result.Controls[loop].ItemSystemClass = (ControlSystemClass)itemClass.Index;
							break;
						case SzInt.SzIntResult.Name:
							result.Controls[loop].ItemClass = itemClass.Name;
							break;
						default:
							throw new NotImplementedException();
						}

					SzInt itemText = ResourceBase.GetSzOrInt(reader, ref padding);
					if(!itemText.IsEmpty)
						result.Controls[loop].ItemText = itemText;

					UInt16 extraCount = reader.BytesToStructure<UInt16>(padding);

					if(extraCount > 0)
					{
						result.Controls[loop].ExtraData = reader.GetBytes(padding, extraCount);
						padding += extraCount;//Including size of extraCount
					} else
					{
						result.Controls[loop].ExtraData = new Byte[] { };
						padding += sizeof(UInt16);
					}

					/*UInt32 align = (padding - startTemplate) % sizeof(UInt32);
					if(align > 0)
						padding += sizeof(UInt32) - align;//Выравнивание по 4 байта. При этом, даже если выравнивания нет, то 4 байта всё равно надо прибавить*/
				}
			}
			return result;
		}
	}
}