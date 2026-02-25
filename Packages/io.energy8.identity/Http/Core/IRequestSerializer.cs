using System.Collections.Generic;

namespace Energy8.Identity.Http.Core
{
    /// <summary>
    /// Интерфейс для сериализации HTTP запросов
    /// Позволяет использовать различные форматы данных (WWWForm, JSON, XML и т.д.)
    /// </summary>
    public interface IRequestSerializer
    {
        /// <summary>
        /// Сериализует данные в массив байтов для отправки
        /// </summary>
        /// <param name="data">Данные для сериализации</param>
        /// <returns>Сериализованные данные в виде массива байтов</returns>
        byte[] Serialize(object data);

        /// <summary>
        /// Возвращает HTTP заголовки, необходимые для формата данных
        /// </summary>
        /// <returns>Словарь HTTP заголовков</returns>
        Dictionary<string, string> GetHeaders();
    }
}
