using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.Core;
using UnityEngine;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Реализация интерфейса INetworkService для работы с сетевыми запросами
    /// </summary>
    public class NetworkService : INetworkService
    {
        private readonly ICommunicationService _communicationService;
        private readonly INetworkManager _networkManager;
        private const string CHANNEL_PREFIX = "network.";

        /// <summary>
        /// Создает новый экземпляр NetworkService
        /// </summary>
        /// <param name="communicationService">Сервис коммуникации для взаимодействия с JavaScript</param>
        public NetworkService(ICommunicationService communicationService)
        {
            _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));

            // NetworkManager создается динамически, так как он нужен только для внутренней работы
            var memoryManager = new Energy8.JSPluginTools.Core.Implementation.MemoryManager();
            var messageBus = new Energy8.JSPluginTools.Core.Implementation.MessageBus(memoryManager);
            var pluginCore = new Energy8.JSPluginTools.Core.Implementation.PluginCore(memoryManager, messageBus);

            _networkManager = new NetworkManager();
            _networkManager.Initialize(pluginCore);
        }

        /// <inheritdoc/>
        public Task<HttpResponse> FetchAsync(HttpRequestOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Convert enum to string
            string methodString = options.Method.ToString();
            string jsonData = options.Body != null ? Newtonsoft.Json.JsonConvert.SerializeObject(options.Body) : null;

            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.SendRequestAsync(
                    methodString,
                    options.Url,
                    jsonData,
                    options.Headers
                ).Result
            ));
        }

        /// <inheritdoc/>
        public Task<HttpResponse> GetAsync(string url, object headers = null)
        {
            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.GetAsync(url, ConvertHeaders(headers)).Result
            ));
        }

        /// <inheritdoc/>
        public Task<HttpResponse> PostAsync(string url, object data, object headers = null)
        {
            string jsonData = data != null ? Newtonsoft.Json.JsonConvert.SerializeObject(data) : null;
            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.PostAsync(url, jsonData, ConvertHeaders(headers)).Result
            ));
        }

        /// <inheritdoc/>
        public Task<HttpResponse> PutAsync(string url, object data, object headers = null)
        {
            string jsonData = data != null ? Newtonsoft.Json.JsonConvert.SerializeObject(data) : null;
            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.PutAsync(url, jsonData, ConvertHeaders(headers)).Result
            ));
        }

        /// <inheritdoc/>
        public Task<HttpResponse> DeleteAsync(string url, object headers = null)
        {
            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.DeleteAsync(url, ConvertHeaders(headers)).Result
            ));
        }

        /// <inheritdoc/>
        public Task<HttpResponse> PatchAsync(string url, object data, object headers = null)
        {
            string jsonData = data != null ? Newtonsoft.Json.JsonConvert.SerializeObject(data) : null;
            return Task.FromResult(ConvertToHttpResponse(
                _networkManager.PatchAsync(url, jsonData, ConvertHeaders(headers)).Result
            ));
        }

        /// <inheritdoc/>
        public async Task<WebSocketConnection> CreateWebSocketAsync(string url, WebSocketOptions options = null)
        {
            // Используем напрямую коммуникационный сервис для создания WebSocket
            var request = new
            {
                url,
                options
            };

            return await _communicationService.SendWithResponseAsync<object, WebSocketConnection>(
                CHANNEL_PREFIX + "createWebSocket",
                request);
        }

        /// <summary>
        /// Преобразует заголовки из объекта в словарь
        /// </summary>
        private Dictionary<string, string> ConvertHeaders(object headers)
        {
            if (headers == null)
                return null;

            var result = new Dictionary<string, string>();

            // Если это уже словарь строк
            if (headers is Dictionary<string, string> strDict)
                return strDict;

            // Если это словарь с другими типами, преобразуем значения в строки
            if (headers is Dictionary<string, object> objDict)
            {
                foreach (var pair in objDict)
                {
                    result[pair.Key] = pair.Value?.ToString();
                }
                return result;
            }

            // Если это анонимный объект или класс, преобразуем его свойства
            var properties = headers.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(headers);
                result[prop.Name] = value?.ToString();
            }

            return result;
        }

        /// <summary>
        /// Преобразует NetworkResponse в HttpResponse
        /// </summary>
        private HttpResponse ConvertToHttpResponse(NetworkResponse response)
        {
            return new HttpResponse
            {
                StatusCode = response.StatusCode,
                Data = response.Data,
                Headers = response.Headers,
                RawResponse = response.Data
            };
        }
    }
}