using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.ImageLoader
{
	/// <summary>Test loader</summary>
	public class LookupLoader : IImageLoader
	{
		Byte[] _map;
		private StreamLoader _baseLoader;

		/// <summary>Modeul Mapped to memory</summary>
		public Boolean IsModuleMapped { get { return _baseLoader.IsModuleMapped; } }

		/// <summary>Base PE file address</summary>
		public Int64 BaseAddress { get { return _baseLoader.BaseAddress; } }

		/// <summary>File source</summary>
		public String Source { get { return _baseLoader.Source; } }

		/// <summary>Create instance of test loader</summary>
		/// <param name="filePath"></param>
		public LookupLoader(String filePath)
		{
			this._baseLoader = StreamLoader.FromFile(filePath);
			this._map = new Byte[new FileInfo(filePath).Length];
		}
		
		/// <summary>Get structure from specific padding from the beginning of the image</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>Readed structure from image</returns>
		public T PtrToStructure<T>(UInt32 padding) where T : struct
		{
			T result = _baseLoader.PtrToStructure<T>(padding);

			for(UInt32 loop = padding; loop < padding + (UInt32)Marshal.SizeOf(typeof(T)); loop++)
				_map[loop]++;
			return result;
		}

		/// <summary>Get bytes from specific padding and specific length</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <param name="length">Length of bytes to read</param>
		/// <returns>Readed bytes</returns>
		public Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			Byte[] result = _baseLoader.ReadBytes(padding, length);

			for(UInt32 loop = padding; loop < padding + length; loop++)
				_map[loop]++;

			return result;
		}

		/// <summary>Get ACSII string from specific padding from the beginning of the image</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>String from pointer</returns>
		public String PtrToStringAnsi(UInt32 padding)
		{
			String result = _baseLoader.PtrToStringAnsi(padding);

			for(UInt32 loop = padding; loop < padding + result.Length; loop++)
				_map[loop]++;

			return result;
		}

		/// <summary>Save to text file all data that was readed</summary>
		public void Dispose()
		{
			File.WriteAllText(this.Source + ".log", String.Join("", Array.ConvertAll(_map, delegate(Byte b) { return _arr[b]; })));
			this._baseLoader.Dispose();
		}
		private static String[] _arr = new String[] { " ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "_10_", "_11_", "_12_", "_13_", "_14_", "_15_", "_16_", "_17_", "_18_", "_19_", "_20_", "_21_", "_22_", "_23_", "_24_", "_25_", "_26_", "_27_", };
	}
}