using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Toolbar resource info class</summary>
	[DefaultProperty(nameof(Header))]
	public class ResourceToolBar : ResourceBase
	{
		private static readonly UInt32 SizeOfStruct = (UInt32)Marshal.SizeOf(typeof(CommCtrl.TOOLBARDATA));

		/// <summary>Toolbar header</summary>
		public CommCtrl.TOOLBARDATA Header
			=> PinnedBufferReader.BytesToStructure<CommCtrl.TOOLBARDATA>(this.Directory.GetData(), 0);

		/// <summary>TooBar header validation</summary>
		public Boolean IsValid => this.Header.wVersion == 1;

		/// <summary>Create instance of toolbar resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceToolBar(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_TOOLBAR) { }

		/// <summary>Gets the array of toolbar buttons and separators from the current toolbar</summary>
		/// <returns></returns>
		public IEnumerable<CommCtrl.TBBUTTON> GetToolBarTemplate()
		{
			UInt32 iIconIndex = 0;
			UInt32 padding = ResourceToolBar.SizeOfStruct;

			using(PinnedBufferReader reader = this.CreateDataReader())
				for(UInt16 loop = 0; loop < this.Header.wItemsCount; loop++)
				{
					UInt16 toolBarItem = reader.BytesToStructure<UInt16>(ref padding);

					yield return toolBarItem > 0
						? new CommCtrl.TBBUTTON()
						{
							iBitmap = iIconIndex++,
							idCommand = toolBarItem,
							fsState = CommCtrl.TBSTATE.ENABLED,
							fsStyle = CommCtrl.TBSTYLE.BTNS_BUTTON,
							dwData = 0,
							iString = "RT_STRING ID: " + toolBarItem,
						}
						: new CommCtrl.TBBUTTON()
						{
							iBitmap = 0,
							idCommand = 0,
							fsState = CommCtrl.TBSTATE.ENABLED,
							fsStyle = CommCtrl.TBSTYLE.BTNS_SEP,
							dwData = 0,
							iString = null,
						};
				}
		}
	}
}