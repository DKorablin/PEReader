using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Version resource class</summary>
	[DefaultProperty("VersionInfo")]
	public class ResourceVersion : ResourceBase
	{
		/// <summary>Тип таблицы</summary>
		public enum VersionTableType
		{
			/// <summary>Таблица со строковыми данными</summary>
			StringFileInfo,
			/// <summary>Таблица с бинарными данными</summary>
			VarFileInfo,
		}

		/// <summary>PE file base language</summary>
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct Translation
		{
			/// <summary>Language ID</summary>
			public UInt16 langID;

			/// <summary>CodePage</summary>
			public UInt16 charsetID;
			
			/// <summary>Convert language to string</summary>
			/// <exception cref="T:NotImplementedException">Unknown langID</exception>
			/// <returns>String</returns>
			public override String ToString()
			{
				String lang = ResourceDirectory.GetLangName(this.langID);
				String charset = Encoding.GetEncoding(this.charsetID).EncodingName;

				return String.Format("Lang: {0} Charset: {1}", lang, charset);
			}
		}

		/// <summary>Tables group</summary>
		public class VersionData
		{
			/// <summary>PE structure</summary>
			public WinNT.Resource.VarFileInfo Info;
			/// <summary>Type of group (Text or Bin)</summary>
			public VersionTableType szKey;
			/// <summary>Tables</summary>
			public VersionTable[] Items;
			/// <summary>Type of table</summary>
			/// <returns>String</returns>
			public override String ToString()
			{
				return szKey.ToString();
			}
		}

		/// <summary>Table of values</summary>
		public class VersionTable
		{
			/// <summary>PE structure</summary>
			public WinNT.Resource.StringTable Table;
			/// <summary>Name of the table</summary>
			public String szKey;
			/// <summary>Values</summary>
			public VersionItem[] Items;
			/// <summary>Table name</summary>
			/// <returns>String</returns>
			public override String ToString() { return this.szKey; }
		}

		/// <summary>Value info</summary>
		public class VersionItem
		{
			/// <summary>PE structure</summary>
			public WinNT.Resource.V_STRING? Item;
			/// <summary>Name of the item</summary>
			public String szKey;
			/// <summary>Value</summary>
			public Object Value;
			/// <summary>Convert value to string</summary>
			/// <exception cref="T:NotImplementedException">Don't now how to convert version item to string</exception>
			/// <returns>String</returns>
			public override String ToString()
			{
				if(this.Value is String)
					return this.Value.ToString();
				else if(this.Value is Byte[])
					return BitConverter.ToString((Byte[])this.Value);
				else if(this.Value != null)
					return this.Value.ToString();
				else throw new NotImplementedException();
			}
		}

		/// <summary>Default version info</summary>
		public WinNT.Resource.VS_VERSIONINFO VersionInfo
		{
			get
			{
				return PinnedBufferReader.BytesToStructure<WinNT.Resource.VS_VERSIONINFO>(base.Directory.GetData(), 0);
			}
		}

		/// <summary>Extended version info</summary>
		public WinVer.VS_FIXEDFILEINFO? FileInfo
		{
			get
			{
				if(this.VersionInfo.wValueLength == 0)
					return null;
				else
				{
					UInt32 padding = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.VS_VERSIONINFO));
					padding = NativeMethods.AlignToInt(padding);
					return PinnedBufferReader.BytesToStructure<WinVer.VS_FIXEDFILEINFO>(base.Directory.GetData(), padding);
				}
			}
		}

		private UInt32 HeaderPadding
		{
			get
			{
				UInt32 padding = (UInt32)Marshal.SizeOf(typeof(WinNT.Resource.VS_VERSIONINFO)) + this.VersionInfo.wValueLength;
				return padding = NativeMethods.AlignToInt(padding);
			}
		}
		
		/// <summary>Create instance of version resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceVersion(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION)
		{
		}

		/// <summary>Get version tables</summary>
		/// <returns>Extended file description</returns>
		public VersionData[] GetFileInfo()
		{
			UInt32 padding = this.HeaderPadding;
			using(PinnedBufferReader reader = base.CreateDataReader())
			{
				List<VersionData> result = new List<VersionData>();

				while(padding < reader.Length && result.Count < 2)
					result.Add(this.GetVersionData(reader, ref padding));
				return result.ToArray();
			}
		}

		private VersionData GetVersionData(PinnedBufferReader reader, ref UInt32 padding)
		{
			VersionData result = new VersionData();

			padding = NativeMethods.AlignToInt(padding);

			UInt32 length = padding;

			result.Info = reader.BytesToStructure<WinNT.Resource.VarFileInfo>(ref padding);

			String szKey = reader.BytesToStringUni(ref padding);
			result.szKey = (VersionTableType)Enum.Parse(typeof(VersionTableType), szKey);

			List<VersionTable> items = new List<VersionTable>();
			while(padding - length < result.Info.wLength)
			{
				padding = NativeMethods.AlignToInt(padding);
				items.Add(this.GetVersionTable(reader, result.szKey, ref padding));
				padding = NativeMethods.AlignToInt(padding);
			}
			result.Items = items.ToArray();

			return result;
		}

		/// <summary>Gets version table from PE/PE+ resources</summary>
		/// <param name="reader">Mapped bytes</param>
		/// <param name="type">Type of version table</param>
		/// <param name="padding">Base padding</param>
		/// <exception cref="T:NotImplementedException">Unknown table row type</exception>
		/// <returns>Readed version table</returns>
		private VersionTable GetVersionTable(PinnedBufferReader reader, VersionTableType type, ref UInt32 padding)
		{
			VersionTable result = new VersionTable();

			UInt32 length = padding;

			result.Table = reader.BytesToStructure<WinNT.Resource.StringTable>(ref padding);
			result.szKey = reader.BytesToStringUni(ref padding);

			List<VersionItem> items = new List<VersionItem>();
			while(padding - length < result.Table.wLength)
			{
				padding = NativeMethods.AlignToInt(padding);
				switch(type)
				{
				case VersionTableType.StringFileInfo:
					items.Add(this.GetStringVersionItem(reader, ref padding));
					break;
				case VersionTableType.VarFileInfo:
					items.Add(this.GetBinaryVersionItem(reader, result, ref padding));
					break;
				default: throw new NotImplementedException();
				}
				padding = NativeMethods.AlignToInt(padding);
			}
			result.Items = items.ToArray();
			return result;
		}

		/// <summary>Получить элемент версии в сохранённый в бинарном виде</summary>
		/// <param name="reader">Allocated bytes in memory</param>
		/// <param name="item">Version table</param>
		/// <param name="padding">Отступ от начала массива</param>
		/// <returns>Элемент версии</returns>
		private VersionItem GetBinaryVersionItem(PinnedBufferReader reader, VersionTable item, ref UInt32 padding)
		{
			VersionItem result = new VersionItem();

			if(item.szKey == "Translation")
			{
				result.Value = reader.BytesToStructure<Translation>(padding);
				String text = result.Value.ToString();
			} else
				result.Value = reader.GetBytes(padding, item.Table.wValueLength);
			padding += item.Table.wValueLength;

			return result;
		}

		private VersionItem GetStringVersionItem(PinnedBufferReader reader, ref UInt32 padding)
		{
			VersionItem result = new VersionItem();

			result.Item = reader.BytesToStructure<WinNT.Resource.V_STRING>(ref padding);
			result.szKey = reader.BytesToStringUni(ref padding);

			padding = NativeMethods.AlignToInt(padding);

			result.Value = reader.BytesToStringUni(padding);
			padding += result.Item.Value.ValueLength;

			return result;
		}
	}
}