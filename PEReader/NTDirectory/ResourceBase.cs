using System;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Base class of resource directory</summary>
	[DefaultProperty("Type")]
	public class ResourceBase
	{
		/// <summary>String or integer structure</summary>
		public struct SzInt
		{
			/// <summary>Type of data in structure</summary>
			public enum SzIntResult
			{
				/// <summary>Empty index</summary>
				None = 0,
				/// <summary>Index in resource file</summary>
				Index = 1,
				/// <summary>String</summary>
				Name = 2,
			}
			private UInt16 _index;
			private String _name;
			private SzIntResult _type;
			/// <summary>Index in resource file</summary>
			public UInt16 Index
			{
				get { return this._index; }
				set
				{
					this._index = value;
					this.Type = SzInt.SzIntResult.Index;
				}
			}
			/// <summary>String</summary>
			public String Name
			{
				get { return this._name; }
				set
				{
					this._name = value;
					this.Type = SzInt.SzIntResult.Name;
				}
			}
			/// <summary>Value is empty</summary>
			public Boolean IsEmpty { get { return this.Type == SzIntResult.None; } }
			/// <summary>Type of value stored in the structure</summary>
			public SzIntResult Type
			{
				get { return this._type; }
				private set { this._type = value; }
			}
			/// <summary>Convert stored value to string</summary>
			/// <exception cref="T:NotImplementedException">Unknown pointer type specified</exception>
			/// <returns>String</returns>
			public override String ToString()
			{
				switch(this.Type)
				{
				case SzIntResult.Index:
					return this.Index.ToString();
				case SzIntResult.Name:
					return this.Name;
				case SzIntResult.None:
					return this.Type.ToString();
				default:
					throw new NotImplementedException();
				}
			}
		}
		private readonly ResourceDirectory _directory;
		private readonly WinNT.Resource.RESOURCE_DIRECTORY_TYPE _type;
		/// <summary>Сама директория из которой получаем ресурсы</summary>
		public ResourceDirectory Directory { get { return this._directory; } }
		/// <summary>Тип директории</summary>
		public WinNT.Resource.RESOURCE_DIRECTORY_TYPE Type { get { return this._type; } }

		/// <summary>Create instance of resource directory class</summary>
		/// <param name="directory">Parent PE directory</param>
		/// <param name="type">Resource directory type</param>
		/// <exception cref="T:ArgumentNullException">directory is null</exception>
		/// <exception cref="T:InvalidOperationException">directory type must be equals to type</exception>
		public ResourceBase(ResourceDirectory directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE type)
		{
			if(directory == null) throw new ArgumentNullException("directory");
			else if(directory.DataEntry == null || directory.Parent == null || directory.Parent.Parent == null
				|| directory.Parent.Parent.DirectoryEntry.NameType != type)
				throw new InvalidOperationException("Expecting " + type.ToString());
			else
			{
				this._type = type;
				this._directory = directory;
			}
		}
		/// <summary>Read integer or string from mapped object</summary>
		/// <param name="reader">Resource allocated bytes array</param>
		/// <param name="padding">Padding from the beginning</param>
		/// <returns></returns>
		protected static SzInt GetSzOrInt(BytesReader reader, ref UInt32 padding)
		{
			SzInt result = new SzInt();
			UInt16 flag = reader.BytesToStructure<UInt16>(padding);

			switch(flag)
			{
			case 0x0000://Нет значения
				padding += sizeof(UInt16);
				return result;
			case 0xFFFF://Есть ещё один элемент ссылающийся на индекс в ресурсах
				padding += sizeof(UInt16);//Сдвиг от первого элемента
				result.Index = reader.BytesToStructure<UInt16>(ref padding);
				return result;
			default:
				result.Name = reader.BytesToStringUni(ref padding);
				return result;
			}
		}
		/// <summary>Create data reader for data in directory</summary>
		/// <returns>Memory pinned data reader</returns>
		public BytesReader CreateDataReader()
		{
			return new BytesReader(this.Directory.GetData());
		}
	}
}