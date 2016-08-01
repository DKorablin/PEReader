using System;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Dialog resource class</summary>
	public class ResourceDialog : ResourceBase
	{
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

			DialogTemplate result;
			using(PinnedBufferReader reader = base.CreateDataReader())
			{
				WinUser.DLGTEMPLATEEX template = reader.BytesToStructure<WinUser.DLGTEMPLATEEX>(0);
				WinUser.DLGTEMPLATE templateOld;
				if(template.IsValid)
				{//Чтение нового шаблона диалогового окна
					templateOld = new WinUser.DLGTEMPLATE();
					padding = (UInt32)Marshal.SizeOf(typeof(WinUser.DLGTEMPLATEEX));

					result = new DialogTemplate(template);
				} else
				{//Чтение старого шаблона диалогового окна
					templateOld = reader.BytesToStructure<WinUser.DLGTEMPLATE>(ref padding);

					result = new DialogTemplate(templateOld);
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

					WinUser.DLGITEMTEMPLATEEX? controlEx = null;
					WinUser.DLGITEMTEMPLATE? control = null;

					if(template.IsValid)
						controlEx = reader.BytesToStructure<WinUser.DLGITEMTEMPLATEEX>(ref padding);
					else
						control = reader.BytesToStructure<WinUser.DLGITEMTEMPLATE>(ref padding);

					SzInt itemClass = ResourceBase.GetSzOrInt(reader, ref padding);

					SzInt itemText = ResourceBase.GetSzOrInt(reader, ref padding);

					Byte[] extraData;
					UInt16 extraCount = reader.BytesToStructure<UInt16>(padding);
					if(extraCount > 0)
					{//TODO: Missing: +sizeof(UInt16)???
						extraData = reader.GetBytes(padding, extraCount);
						padding += extraCount;//Including size of extraCount
					} else
					{
						extraData = new Byte[] { };
						padding += sizeof(UInt16);
					}

					DialogItemTemplate ctrl = new DialogItemTemplate(control, controlEx, itemClass, itemText, extraData);

					result.Controls[loop] = ctrl;

					/*UInt32 align = (padding - startTemplate) % sizeof(UInt32);
					if(align > 0)
						padding += sizeof(UInt32) - align;//Выравнивание по 4 байта. При этом, даже если выравнивания нет, то 4 байта всё равно надо прибавить*/
				}
			}
			return result;
		}
	}
}