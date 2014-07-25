using System;

namespace AlphaOmega.Debug
{
	/// <summary>Получение данных из секции</summary>
	public interface ISectionData
	{
		/// <summary>Получить все данные в виде массива байт из секции PE файла</summary>
		/// <returns>Массив байт в секции</returns>
		Byte[] GetData();
	}
}