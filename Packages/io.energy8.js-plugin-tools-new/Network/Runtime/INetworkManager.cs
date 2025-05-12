using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Интерфейс для работы с сетевыми запросами через JavaScript
    /// </summary>
    public interface INetworkManager
    {
        /// <summary>
        /// Инициализирует модуль Network с указанным ядром плагина
        /// </summary>
        /// <param name="core">Экземпляр ядра плагина</param>
        void Initialize(IPluginCore core);
        
        /// <summary>
        /// Проверяет, инициализирован ли модуль
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Событие, возникающее при инициализации модуля
        /// </summary>
        event Action OnInitialized;
        
        /// <summary>
        /// Выполняет GET-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> GetAsync(string url, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет POST-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> PostAsync(string url, string data, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет PUT-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> PutAsync(string url, string data, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет DELETE-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> DeleteAsync(string url, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет HEAD-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> HeadAsync(string url, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет PATCH-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> PatchAsync(string url, string data, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет OPTIONS-запрос
        /// </summary>
        /// <param name="url">URL для запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> OptionsAsync(string url, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Выполняет произвольный HTTP-запрос
        /// </summary>
        /// <param name="method">HTTP-метод (GET, POST, PUT и т.д.)</param>
        /// <param name="url">URL для запроса</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> SendRequestAsync(string method, string url, string data = null, Dictionary<string, string> headers = null);
        
        /// <summary>
        /// Загружает файл по URL
        /// </summary>
        /// <param name="url">URL файла</param>
        /// <param name="onProgress">Функция обратного вызова для отслеживания прогресса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> DownloadFileAsync(string url, Action<float> onProgress = null);
        
        /// <summary>
        /// Отправляет файл на сервер
        /// </summary>
        /// <param name="url">URL для отправки</param>
        /// <param name="fileData">Двоичные данные файла</param>
        /// <param name="fileName">Имя файла</param>
        /// <param name="fileField">Имя поля формы для файла</param>
        /// <param name="formData">Дополнительные данные формы</param>
        /// <param name="onProgress">Функция обратного вызова для отслеживания прогресса</param>
        /// <returns>Результат запроса</returns>
        Task<NetworkResponse> UploadFileAsync(string url, byte[] fileData, string fileName, string fileField = "file", Dictionary<string, string> formData = null, Action<float> onProgress = null);
        
        /// <summary>
        /// Проверяет доступность сети
        /// </summary>
        /// <returns>True, если сеть доступна</returns>
        Task<bool> IsOnlineAsync();
        
        /// <summary>
        /// Включает или выключает кэширование запросов
        /// </summary>
        /// <param name="enabled">True для включения кэширования</param>
        void SetCaching(bool enabled);
        
        /// <summary>
        /// Устанавливает тайм-аут для запросов
        /// </summary>
        /// <param name="timeoutInSeconds">Тайм-аут в секундах</param>
        void SetTimeout(int timeoutInSeconds);
        
        /// <summary>
        /// Очищает кэш запросов
        /// </summary>
        Task<bool> ClearCacheAsync();
    }
    
    /// <summary>
    /// Ответ сетевого запроса
    /// </summary>
    [Serializable]
    public class NetworkResponse
    {
        /// <summary>
        /// Успешность выполнения запроса
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// HTTP-статус
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Текст статуса
        /// </summary>
        public string StatusText { get; set; }
        
        /// <summary>
        /// Данные ответа
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// Заголовки ответа
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        
        /// <summary>
        /// Двоичные данные (для загрузки файлов)
        /// </summary>
        public byte[] BinaryData { get; set; }
        
        /// <summary>
        /// Сообщение об ошибке (если запрос не успешен)
        /// </summary>
        public string Error { get; set; }
    }
}