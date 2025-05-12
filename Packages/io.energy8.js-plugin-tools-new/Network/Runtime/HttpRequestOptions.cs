using System.Collections.Generic;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Представляет метод HTTP-запроса
    /// </summary>
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH,
        HEAD,
        OPTIONS
    }

    /// <summary>
    /// Тип содержимого HTTP-запроса
    /// </summary>
    public enum ContentType
    {
        JSON,
        FormData,
        Text,
        Binary,
        URLEncoded
    }

    /// <summary>
    /// Параметры для выполнения HTTP-запроса
    /// </summary>
    public class HttpRequestOptions
    {
        /// <summary>
        /// URL запроса
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Метод HTTP-запроса
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.GET;

        /// <summary>
        /// Тип содержимого запроса
        /// </summary>
        public ContentType ContentType { get; set; } = ContentType.JSON;

        /// <summary>
        /// Заголовки запроса
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Тело запроса (для методов POST, PUT, PATCH)
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        /// Таймаут запроса в миллисекундах
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// Флаг, указывающий, нужно ли отправлять куки с запросом
        /// </summary>
        public bool WithCredentials { get; set; }

        /// <summary>
        /// Указывает, как обрабатывать ошибки HTTP (4xx, 5xx)
        /// Если true, то ошибка будет генерироваться как исключение
        /// Если false, то ошибка будет возвращена как обычный ответ
        /// </summary>
        public bool ThrowOnHttpError { get; set; } = true;
    }
}