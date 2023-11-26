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
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG) { }

		/// <summary>Get dialog template in resource directory</summary>
		/// <exception cref="NotImplementedException">Class type not implemented</exception>
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
				{//Reading new window template
					templateOld = new WinUser.DLGTEMPLATE();
					padding = (UInt32)Marshal.SizeOf(typeof(WinUser.DLGTEMPLATEEX));

					result = new DialogTemplate(template);
				} else
				{//Reading old window template
					templateOld = reader.BytesToStructure<WinUser.DLGTEMPLATE>(ref padding);

					result = new DialogTemplate(templateOld);
				}

				result.Menu = ResourceBase.GetSzOrInt(reader, ref padding);//Resource identifier or menu name
				result.WindowName = ResourceBase.GetSzOrInt(reader, ref padding);//Window identifier or window class name

				if(reader.BytesToStructure<UInt16>(padding) == 0x0000)
					padding += sizeof(UInt16);
				else//Reading window header
					result.Title = reader.BytesToStringUni(ref padding);

				//Reading font information
				if((template.IsValid && template.ContainsFont)
					|| templateOld.ContainsFont)
				{
					DialogFont font = new DialogFont()
					{
						FontSize = reader.BytesToStructure<UInt16>(ref padding),
					};
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

				//Reading control elements following window handle
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

					result.Controls[loop] = DialogItemTemplate.CreateDialogItem(control, controlEx, itemClass, itemText, extraData);

					/*UInt32 align = (padding - startTemplate) % sizeof(UInt32);
					if(align > 0)
						padding += sizeof(UInt32) - align;//Align to 4 bytes. At the same time even if there is no alignment then 4 bytes still need to be added*/
				}
			}
			return result;
		}
	}
}