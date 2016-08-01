using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Toolbar resource info class</summary>
	[DefaultProperty("Header")]
	public class ResourceToolBar : ResourceBase
	{
		private static UInt32 SizeOfStruct = (UInt32)Marshal.SizeOf(typeof(CommCtrl.TOOLBARDATA));

		/// <summary>Toolbar header</summary>
		public CommCtrl.TOOLBARDATA Header
		{
			get { return PinnedBufferReader.BytesToStructure<CommCtrl.TOOLBARDATA>(base.Directory.GetData(), 0); }
		}

		/// <summary>TooBar header validation</summary>
		public Boolean IsValid { get { return this.Header.wVersion == 1; } }

		/// <summary>Create instance of toolbar resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceToolBar(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_TOOLBAR)
		{
		}

		/// <summary>Gets the array of toolbar buttons and separators from the current toolbar</summary>
		/// <returns></returns>
		public IEnumerable<CommCtrl.TBBUTTON> GetToolBarTemplate()
		{
			UInt32 iIconIndex = 0;
			UInt32 padding = ResourceToolBar.SizeOfStruct;

			using(PinnedBufferReader reader = base.CreateDataReader())
				for(UInt16 loop = 0; loop < this.Header.wItemsCount; loop++)
				{
					UInt16 toolBarItem = reader.BytesToStructure<UInt16>(ref padding);

					if(toolBarItem <= 0)
					{
						CommCtrl.TBBUTTON separator = new CommCtrl.TBBUTTON();
						separator.iBitmap = 0;
						separator.idCommand = 0;
						separator.fsState = CommCtrl.TBSTATE.ENABLED;
						separator.fsStyle = CommCtrl.TBSTYLE.BTNS_SEP;
						separator.dwData = 0;
						separator.iString = null;
						yield return separator;
					} else
					{
						String title = "RT_STRING ID: " + toolBarItem;
						CommCtrl.TBBUTTON button = new CommCtrl.TBBUTTON();
						button.iBitmap = iIconIndex++;
						button.idCommand = toolBarItem;
						button.fsState = CommCtrl.TBSTATE.ENABLED;
						button.fsStyle = CommCtrl.TBSTYLE.BTNS_BUTTON;
						button.dwData = 0;
						button.iString = title;
						yield return button;
					}
				}
		}
	}
}