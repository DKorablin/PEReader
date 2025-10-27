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

			/// <summary>Index in resource file</summary>
			public UInt16 Index
			{
				get => this._index;
				set
				{
					this._index = value;
					this.Type = SzInt.SzIntResult.Index;
				}
			}

			/// <summary>String</summary>
			public String Name
			{
				get => this._name;
				set
				{
					this._name = value;
					this.Type = SzInt.SzIntResult.Name;
				}
			}

			/// <summary>Value is empty</summary>
			public Boolean IsEmpty => this.Type == SzIntResult.None;

			/// <summary>Type of value stored in the structure</summary>
			public SzIntResult Type { get; private set; }

			/// <summary>Convert stored value to string</summary>
			/// <exception cref="NotImplementedException">Unknown pointer type specified</exception>
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

		/// <summary>The directory from which we obtain resources</summary>
		public ResourceDirectory Directory { get; }

		/// <summary>Directory type</summary>
		public WinNT.Resource.RESOURCE_DIRECTORY_TYPE Type { get; }

		/// <summary>Create instance of resource directory class</summary>
		/// <param name="directory">Parent PE directory</param>
		/// <param name="type">Resource directory type</param>
		/// <exception cref="ArgumentNullException">directory is null</exception>
		/// <exception cref="InvalidOperationException">directory type must be equals to type</exception>
		public ResourceBase(ResourceDirectory directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE type)
		{
			_ = directory ?? throw new ArgumentNullException(nameof(directory));
			
			if(directory.DataEntry == null || directory.Parent == null || directory.Parent.Parent == null
				|| directory.Parent.Parent.DirectoryEntry.NameType != type)
				throw new InvalidOperationException("Expecting " + type.ToString());
			else
			{
				this.Type = type;
				this.Directory = directory;
			}
		}
		/// <summary>Read integer or string from mapped object</summary>
		/// <param name="reader">Resource allocated bytes array</param>
		/// <param name="padding">Padding from the beginning</param>
		/// <returns></returns>
		protected internal static SzInt GetSzOrInt(PinnedBufferReader reader, ref UInt32 padding)
		{
			SzInt result = new SzInt();
			UInt16 flag = reader.BytesToStructure<UInt16>(padding);

			switch(flag)
			{
			case 0x0000://No value
				padding += sizeof(UInt16);
				return result;
			case 0xFFFF://There is another element that refers to the index in resources
				padding += sizeof(UInt16);//Offset from the first element
				result.Index = reader.BytesToStructure<UInt16>(ref padding);
				return result;
			default:
				result.Name = reader.BytesToStringUni(ref padding);
				return result;
			}
		}
		/// <summary>Create data reader for data in directory</summary>
		/// <returns>Memory pinned data reader</returns>
		public PinnedBufferReader CreateDataReader()
			=> new PinnedBufferReader(this.Directory.GetData());
	}
}