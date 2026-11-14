using System;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Dialog resource class</summary>
	public class ResourceDialog : ResourceBase
	{
		/// <summary>Point size value that tells the system to use the message box font (NONCLIENTMETRICS.lfMessageFont) instead of the font described in the template</summary>
		/// <remarks>When this value is set, the weight, italic, charset and typeface members are not present in the template</remarks>
		private const UInt16 MessageBoxFontPointSize = 0x7FFF;

		/// <summary>Create instance of dialog resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceDialog(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG) { }

		/// <summary>Get dialog template in resource directory</summary>
		/// <returns>Dialog template from directory</returns>
		public DialogTemplate GetDialogTemplate()
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
			{
				UInt32 padding = 0;

				//DLGTEMPLATEEX starts with WORD dlgVer (1) followed by WORD signature (0xFFFF).
				//In DLGTEMPLATE the same 4 bytes are the style DWORD. Checking only these 2 WORDs prevents reading
				//the bigger DLGTEMPLATEEX header past the end of a small DLGTEMPLATE.
				Boolean isExtended = reader.BytesToStructure<UInt16>(0) == 1
					&& reader.BytesToStructure<UInt16>(sizeof(UInt16)) == 0xFFFF;

				DialogTemplate result;
				Boolean containsFont;
				if(isExtended)
				{//Reading extended dialog template
					WinUser.DLGTEMPLATEEX template = reader.BytesToStructure<WinUser.DLGTEMPLATEEX>(ref padding);
					containsFont = template.ContainsFont;
					result = new DialogTemplate(template);
				} else
				{//Reading standard dialog template
					WinUser.DLGTEMPLATE template = reader.BytesToStructure<WinUser.DLGTEMPLATE>(ref padding);
					containsFont = template.ContainsFont;
					result = new DialogTemplate(template);
				}

				result.Menu = ResourceBase.GetSzOrInt(reader, ref padding);//Resource identifier or menu name
				result.WindowClass = ResourceBase.GetSzOrInt(reader, ref padding);//Window class ordinal or window class name

				if(reader.BytesToStructure<UInt16>(padding) == 0x0000)
					padding += sizeof(UInt16);
				else//Reading window title
					result.Title = reader.BytesToStringUni(ref padding);

				//Reading font information (begins on the WORD boundary following the title, which Unicode strings always end on)
				if(containsFont)
				{
					DialogFont font = new DialogFont()
					{
						FontSize = reader.BytesToStructure<UInt16>(ref padding),
					};

					if(font.FontSize != MessageBoxFontPointSize)
					{
						if(isExtended)
						{//Reading extended dialog font
							font.FontWeight = reader.BytesToStructure<UInt16>(ref padding);

							font.Italic = reader[padding++];
							font.CharSet = reader[padding++];
						}
						font.TypeFace = reader.BytesToStringUni(ref padding);
					}
					result.Font = font;
				}

				//Reading control elements following the dialog header
				for(Int32 loop = 0; loop < result.Controls.Length; loop++)
				{
					padding = NativeMethods.AlignToInt(padding);//Each DLGITEMTEMPLATE(EX) starts on a DWORD boundary

					WinUser.DLGITEMTEMPLATEEX? controlEx = null;
					WinUser.DLGITEMTEMPLATE? control = null;

					if(isExtended)
						controlEx = reader.BytesToStructure<WinUser.DLGITEMTEMPLATEEX>(ref padding);
					else
						control = reader.BytesToStructure<WinUser.DLGITEMTEMPLATE>(ref padding);

					SzInt itemClass = ResourceBase.GetSzOrInt(reader, ref padding);
					SzInt itemText = ResourceBase.GetSzOrInt(reader, ref padding);

					//Creation data: WORD size followed by that many bytes of data. The size does not include the size WORD itself
					UInt16 extraCount = reader.BytesToStructure<UInt16>(ref padding);
					Byte[] extraData = extraCount > 0
						? reader.GetBytes(padding, extraCount)
						: new Byte[] { };
					padding += extraCount;

					result.Controls[loop] = DialogItemTemplate.CreateDialogItem(control, controlEx, itemClass, itemText, extraData);
				}

				return result;
			}
		}
	}
}