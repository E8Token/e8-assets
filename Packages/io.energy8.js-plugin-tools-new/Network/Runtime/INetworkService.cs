using System;
using System.Threading.Tasks;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Интерфейс для работы с сетевыми запросами
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        /// Выполняет HTTP-запрос с использованием указанных параметров
        /// </summary>
        /// <param name="options">Параметры запроса</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> FetchAsync(HttpRequestOptions options);
        
        /// <summary>
        /// Выполняет HTTP GET-запрос по указанному URL
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса (необязательно)</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> GetAsync(string url, object headers = null);
        
        /// <summary>
        /// Выполняет HTTP POST-запрос по указанному URL с указанными данными
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса (необязательно)</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> PostAsync(string url, object data, object headers = null);
        
        /// <summary>
        /// Выполняет HTTP PUT-запрос по указанному URL с указанными данными
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса (необязательно)</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> PutAsync(string url, object data, object headers = null);
        
        /// <summary>
        /// Выполняет HTTP DELETE-запрос по указанному URL
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса (необязательно)</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> DeleteAsync(string url, object headers = null);
        
        /// <summary>
        /// Выполняет HTTP PATCH-запрос по указанному URL с указанными данными
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса (необязательно)</param>
        /// <returns>Результат запроса в виде объекта HttpResponse</returns>
        Task<HttpResponse> PatchAsync(string url, object data, object headers = null);
        
        /// <summary>
        /// Создает WebSocket-соединение по указанному URL
        /// </summary>
        /// <param name="url">URL для WebSocket-соединения</param>
        /// <param name="options">Параметры соединения (необязательно)</param>
        /// <returns>Объект WebSocketConnection для управления соединением</returns>
        Task<WebSocketConnection> CreateWebSocketAsync(string url, WebSocketOptions options = null);
    }
}