using System;

namespace AlphaOmega.Debug
{
	/// <summary>Интерфейс директории в PE файле</summary>
	public interface IDirectory
	{
		/// <summary>Директория пустая</summary>
		Boolean IsEmpty { get; }
		/// <summary>Описатель директории</summary>
		WinNT.IMAGE_DATA_DIRECTORY Directory { get; }
	}
}