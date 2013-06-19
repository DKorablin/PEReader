using System;

namespace AlphaOmega.Debug
{
	/// <summary>Загрузчик PE файла</summary>
	public interface IImageLoader : IDisposable
	{
		/// <summary>Модуль загружен в память. Все RVA адреса переписаны в VA.</summary>
		Boolean IsModuleMapped { get; }
		/// <summary>Базовый адрес загруженного модуля в память</summary>
		Int64 BaseAddress { get; }
		/// <summary>Источник получения PE файла</summary>
		String Source { get; }
		/// <summary>Получить структуру с определённого отступа</summary>
		/// <typeparam name="T">Структура</typeparam>
		/// <param name="padding">Отступ от начала файла или RVA</param>
		/// <returns>Прочитанная структура</returns>
		T PtrToStructure<T>(UInt32 padding) where T : struct;
		/// <summary>Получить массив байт с начала отступа</summary>
		/// <param name="padding">Отступ от начала файла или RVA</param>
		/// <param name="length">Читаемый размер</param>
		/// <returns>Получить массив байт с отступа</returns>
		Byte[] ReadBytes(UInt32 padding, UInt32 length);
		/// <summary>Получить строку с отпределённого отступа</summary>
		/// <param name="padding">Отступ от начала файла или RVA</param>
		/// <returns>Прочитанная строка</returns>
		String PtrToStringAnsi(UInt32 padding);
	}
}