using System;
using System.IO;

namespace AlphaOmega.Debug
{
	/// <summary>Test loader</summary>
	public class LookupLoader : StreamLoader, IDisposable
	{
		private readonly String _filePath;
		private Byte[] _map;
		private static readonly String[] _arr = new String[]
		{
			" ", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A",
			"B", "C", "D", "E", "F", "G", "H", "I", "J", "K",
			"L", "M", "N", "O", "P", "Q", "R", "S", "T", "Y", "V", "W", "X", "Y", "Z",
		};

		/// <summary>Create instance of test loader</summary>
		/// <param name="filePath"></param>
		public LookupLoader(String filePath)
			: base(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			this._filePath = filePath;
			this._map = new Byte[new FileInfo(filePath).Length];
		}
		
		/// <summary>Get structure from specific padding from the beginning of the image</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>Readed structure from image</returns>
		public override T PtrToStructure<T>(UInt32 padding)
			=> base.PtrToStructure<T>(padding);

		/// <summary>Get bytes from specific padding and specific length</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <param name="length">Length of bytes to read</param>
		/// <returns>Readed bytes</returns>
		public override Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			Byte[] result = base.ReadBytes(padding, length);

			for(UInt32 loop = padding; loop < padding + length; loop++)
				_map[loop]++;

			return result;
		}

		/// <summary>Get ACSII string from specific padding from the beginning of the image</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>String from pointer</returns>
		public override String PtrToStringAnsi(UInt32 padding)
		{
			String result = base.PtrToStringAnsi(padding);

			for(UInt32 loop = padding; loop < padding + result.Length; loop++)
				_map[loop]++;

			return result;
		}

		/// <summary>Save to text file all data that was readed</summary>
		public new void Dispose()
		{
			File.WriteAllText(
				LookupLoader.GetFileUniqueName(this._filePath, ".log", 0),
				String.Join(String.Empty, Array.ConvertAll(_map, delegate(Byte b) { return _arr[b]; })));

			base.Dispose();
		}

		/// <summary>Получить уникальное наименование файла</summary>
		/// <param name="path">Путь с наименованием файла</param>
		/// /// <param name="extension">Расширение, которое добавляется к файлу</param>
		/// <param name="index">Индекс наименования, если файл с таким наименованием уже существует</param>
		/// <returns>Уникальное наимеование файла</returns>
		private static String GetFileUniqueName(String path, String extension, UInt32 index)
		{
			String filePath = index > 0
				? String.Format("{0}[{1}]{2}", path, index, extension)
				: path + extension;

			return File.Exists(filePath)
				? LookupLoader.GetFileUniqueName(path, extension, checked(index + 1))
				: filePath;
		}
	}
}