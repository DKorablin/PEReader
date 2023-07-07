using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>RT_DLGINIT resource reader class</summary>
	public class ResourceDialogInit : ResourceBase, IEnumerable<ResourceDialogInit.ControlInitData>
	{
		/// <summary>Microsoft Foundation Class RT_DIALOG init data</summary>
		/// <remarks>https://msdn.microsoft.com/en-us/library/xkd95027.aspx</remarks>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct MfcCtrlData
		{
			/// <summary>Dialog ControlID</summary>
			public UInt16 ControlID;

			/// <summary>WM to control</summary>
			public UInt16 MessageID;

			/// <summary>Length of data</summary>
			public UInt32 DataLength;
		}

		/// <summary>Microsoft Foundation Class RT_DIALOG init data</summary>
		public struct ControlInitData
		{
			/// <summary>Dialog ControlID</summary>
			public UInt16 ControlID;

			/// <summary>WM to control</summary>
			public UInt16 MessageID;

			/// <summary>Length of data</summary>
			public UInt32 DataLength;

			/// <summary>Payload</summary>
			public Byte[] Data;

			/// <summary>Create instance of init control data with header and payload</summary>
			/// <param name="header">Header</param>
			/// <param name="data">Payload</param>
			public ControlInitData(MfcCtrlData header, Byte[] data)
			{
				this.ControlID = header.ControlID;
				this.MessageID = header.MessageID;
				this.DataLength = header.DataLength;
				this.Data = data ?? throw new ArgumentNullException(nameof(data));
			}
		}

		/// <summary>Create instance of RT_DLGINIT resource reader class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceDialogInit(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DLGINIT)
		{
		}

		/// <summary>Get array of MFC dialog init data</summary>
		/// <returns>MFC dialog init data</returns>
		public IEnumerator<ResourceDialogInit.ControlInitData> GetEnumerator()
		{
			UInt32 padding = 0;
			UInt32 headerDataSize = (UInt32)Marshal.SizeOf(typeof(MfcCtrlData));
			using(PinnedBufferReader reader = base.CreateDataReader())
				while(padding < reader.Length)
				{
					MfcCtrlData header = reader.BytesToStructure<MfcCtrlData>(ref padding);
					Byte[] data = reader.GetBytes(padding, (UInt32)header.DataLength);
					padding += (UInt32)header.DataLength;

					yield return new ControlInitData(header, data);

					if(padding + headerDataSize > reader.Length)
						yield break;//TODO: В заключении, обычно, идут 2 байта. Но проверял пока только на DBaseTool
				}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}