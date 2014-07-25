using System;
using System.ComponentModel;
using System.IO;

namespace AlphaOmega.Debug
{
	[DefaultProperty("Source")]
	internal class MemoryLoader : IImageLoader
	{
		private readonly Byte[] _image;
		public Boolean IsModuleMapped { get { return false; } }
		public Int64 BaseAddress { get { return 0; } }
		public String Source
		{
			get;
			private set;
		}
		/// <summary>Create instance of PE/PE+ loader from memory</summary>
		/// <param name="filePath">Path to the file to load</param>
		/// <exception cref="T:ArgumentNullException">filePath is empty</exception>
		/// <exception cref="T:FileNotFoundException">File filePath not found</exception>
		public MemoryLoader(String filePath)
		{
			if(String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");
			if(!File.Exists(filePath))
				throw new FileNotFoundException(String.Format("File {0} not found", filePath));

			this.Source = filePath;
			this._image = File.ReadAllBytes(this.Source);
		}
		public MemoryLoader(Byte[] image)
		{
			this._image = image;
		}
		/// <summary>Read array of bytes from memory</summary>
		/// <param name="padding">Padding from starting value</param>
		/// <param name="length">Length to read from padding</param>
		/// <exception cref="T:NotImplementedException">ReadBytes method is not implemented</exception>
		/// <returns></returns>
		public Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			throw new NotImplementedException();
		}
		/// <summary>Get structure from specific padding from the beginning of the image</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <exception cref="T:NotImplementedException">PtrToStructure method is not implemented</exception>
		/// <returns>Readed structure from image</returns>
		public T PtrToStructure<T>(UInt32 padding) where T : struct
		{
			throw new NotImplementedException();
			/*unsafe
			{
				fixed(Byte* ptr = &this._image[padding])
				{
					return (T)Marshal.PtrToStructure(new IntPtr(ptr), typeof(T));
				}
			}*/
		}
		/// <summary>Get ACSII string from specific padding from the beginning of the image</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <exception cref="T:ArgumentOutOfRangeException">padding more than size of image</exception>
		/// <exception cref="T:NotImplementedException">PtrToStringAnsi method is not implemented</exception>
		/// <returns>String from pointer</returns>
		public String PtrToStringAnsi(UInt32 padding)
		{
			throw new NotImplementedException();
			/*unsafe
			{
				fixed(Byte* ptr = &this._image[padding])
				{
					return Marshal.PtrToStringAnsi(new IntPtr(ptr));
				}
			}*/
		}
		public void Dispose()
		{

		}
	}
}