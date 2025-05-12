using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Реализация менеджера сетевых запросов через JavaScript
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        private IPluginCore _core;
        private IMessageBus _messageBus;
        private bool _isInitialized = false;
        
        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;
        
        /// <inheritdoc/>
        public event Action OnInitialized;
        
        private const string MESSAGE_PREFIX = "network.";
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void __JS_Network_Init();
#endif
        
        /// <inheritdoc/>
        public void Initialize(IPluginCore core)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("JSPluginTools [Network]: Already initialized");
                return;
            }
            
            _core = core ?? throw new ArgumentNullException(nameof(core));
            
            if (_core is PluginCore pluginCore)
            {
                _messageBus = pluginCore.MessageBus;
            }
            else
            {
                throw new InvalidOperationException("JSPluginTools [Network]: Core implementation does not provide access to MessageBus");
            }
            
#if UNITY_WEBGL && !UNITY_EDITOR
            __JS_Network_Init();
#endif
            
            _isInitialized = true;
            Debug.Log("JSPluginTools [Network]: Module initialized");
            OnInitialized?.Invoke();
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> GetAsync(string url, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("GET", url, null, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> PostAsync(string url, string data, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("POST", url, data, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> PutAsync(string url, string data, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("PUT", url, data, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> DeleteAsync(string url, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("DELETE", url, null, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> HeadAsync(string url, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("HEAD", url, null, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> PatchAsync(string url, string data, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("PATCH", url, data, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> OptionsAsync(string url, Dictionary<string, string> headers = null)
        {
            return await SendRequestAsync("OPTIONS", url, null, headers);
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> SendRequestAsync(string method, string url, string data = null, Dictionary<string, string> headers = null)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }
            
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("Method cannot be null or empty", nameof(method));
            }
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}sendRequest";
                
                var parameters = new
                {
                    method,
                    url,
                    data,
                    headers
                };
                
                var tcs = new TaskCompletionSource<NetworkResponse>();
                
                _messageBus.SendMessageWithResponse<object, NetworkResponseDto>(
                    messageType,
                    parameters,
                    response =>
                    {
                        var networkResponse = new NetworkResponse
                        {
                            Success = response.Success,
                            StatusCode = response.StatusCode,
                            StatusText = response.StatusText,
                            Data = response.Data,
                            Headers = response.Headers,
                            Error = response.Error
                        };
                        
                        tcs.SetResult(networkResponse);
                    });
                
                Debug.Log($"JSPluginTools [Network]: Sent {method} request to {url}");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error sending {method} request to {url}: {ex.Message}");
                
                return new NetworkResponse
                {
                    Success = false,
                    StatusCode = 0,
                    StatusText = "Error",
                    Error = ex.Message
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> DownloadFileAsync(string url, Action<float> onProgress = null)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}downloadFile";
                string progressType = $"{MESSAGE_PREFIX}downloadProgress";
                
                var parameters = new
                {
                    url
                };
                
                var tcs = new TaskCompletionSource<NetworkResponse>();
                
                // Регистрируем обработчик прогресса, если он указан
                if (onProgress != null)
                {
                    _messageBus.RegisterMessageHandler<float>(
                        progressType,
                        progress => onProgress(progress));
                }
                
                _messageBus.SendMessageWithResponse<object, NetworkResponseWithBinaryDto>(
                    messageType,
                    parameters,
                    response =>
                    {
                        var networkResponse = new NetworkResponse
                        {
                            Success = response.Success,
                            StatusCode = response.StatusCode,
                            StatusText = response.StatusText,
                            Data = response.Data,
                            Headers = response.Headers,
                            Error = response.Error
                        };
                        
                        // Декодируем Base64 в бинарные данные, если они есть
                        if (!string.IsNullOrEmpty(response.BinaryDataBase64))
                        {
                            networkResponse.BinaryData = Convert.FromBase64String(response.BinaryDataBase64);
                        }
                        
                        tcs.SetResult(networkResponse);
                        
                        // Отписываемся от обработчика прогресса
                        if (onProgress != null)
                        {
                            _messageBus.UnregisterMessageHandler(progressType);
                        }
                    });
                
                Debug.Log($"JSPluginTools [Network]: Started downloading file from {url}");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error downloading file from {url}: {ex.Message}");
                
                return new NetworkResponse
                {
                    Success = false,
                    StatusCode = 0,
                    StatusText = "Error",
                    Error = ex.Message
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<NetworkResponse> UploadFileAsync(string url, byte[] fileData, string fileName, string fileField = "file", Dictionary<string, string> formData = null, Action<float> onProgress = null)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }
            
            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentException("File data cannot be null or empty", nameof(fileData));
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}uploadFile";
                string progressType = $"{MESSAGE_PREFIX}uploadProgress";
                
                // Преобразуем бинарные данные в Base64
                string fileDataBase64 = Convert.ToBase64String(fileData);
                
                var parameters = new
                {
                    url,
                    fileDataBase64,
                    fileName,
                    fileField,
                    formData
                };
                
                var tcs = new TaskCompletionSource<NetworkResponse>();
                
                // Регистрируем обработчик прогресса, если он указан
                if (onProgress != null)
                {
                    _messageBus.RegisterMessageHandler<float>(
                        progressType,
                        progress => onProgress(progress));
                }
                
                _messageBus.SendMessageWithResponse<object, NetworkResponseDto>(
                    messageType,
                    parameters,
                    response =>
                    {
                        var networkResponse = new NetworkResponse
                        {
                            Success = response.Success,
                            StatusCode = response.StatusCode,
                            StatusText = response.StatusText,
                            Data = response.Data,
                            Headers = response.Headers,
                            Error = response.Error
                        };
                        
                        tcs.SetResult(networkResponse);
                        
                        // Отписываемся от обработчика прогресса
                        if (onProgress != null)
                        {
                            _messageBus.UnregisterMessageHandler(progressType);
                        }
                    });
                
                Debug.Log($"JSPluginTools [Network]: Started uploading file {fileName} to {url}");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error uploading file {fileName} to {url}: {ex.Message}");
                
                return new NetworkResponse
                {
                    Success = false,
                    StatusCode = 0,
                    StatusText = "Error",
                    Error = ex.Message
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> IsOnlineAsync()
        {
            CheckInitialized();
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}isOnline";
                
                var tcs = new TaskCompletionSource<bool>();
                
                _messageBus.SendMessageWithResponse<object, bool>(
                    messageType,
                    null,
                    response => tcs.SetResult(response));
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error checking online status: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public void SetCaching(bool enabled)
        {
            CheckInitialized();
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}setCaching";
                
                var parameters = new
                {
                    enabled
                };
                
                _messageBus.SendMessage(messageType, parameters);
                
                Debug.Log($"JSPluginTools [Network]: Caching {(enabled ? "enabled" : "disabled")}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error setting caching: {ex.Message}");
            }
        }
        
        /// <inheritdoc/>
        public void SetTimeout(int timeoutInSeconds)
        {
            CheckInitialized();
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}setTimeout";
                
                var parameters = new
                {
                    timeoutInSeconds
                };
                
                _messageBus.SendMessage(messageType, parameters);
                
                Debug.Log($"JSPluginTools [Network]: Timeout set to {timeoutInSeconds} seconds");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error setting timeout: {ex.Message}");
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> ClearCacheAsync()
        {
            CheckInitialized();
            
            try
            {
                string messageType = $"{MESSAGE_PREFIX}clearCache";
                
                var tcs = new TaskCompletionSource<bool>();
                
                _messageBus.SendMessageWithResponse<object, bool>(
                    messageType,
                    null,
                    response => tcs.SetResult(response));
                
                bool result = await tcs.Task;
                
                Debug.Log($"JSPluginTools [Network]: Cache cleared: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Network]: Error clearing cache: {ex.Message}");
                return false;
            }
        }
        
        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("JSPluginTools [Network]: Module not initialized. Call Initialize() first");
            }
        }
        
        /// <summary>
        /// DTO для получения ответа от JavaScript
        /// </summary>
        [Serializable]
        private class NetworkResponseDto
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            
            [JsonProperty("statusCode")]
            public int StatusCode { get; set; }
            
            [JsonProperty("statusText")]
            public string StatusText { get; set; }
            
            [JsonProperty("data")]
            public string Data { get; set; }
            
            [JsonProperty("headers")]
            public Dictionary<string, string> Headers { get; set; }
            
            [JsonProperty("error")]
            public string Error { get; set; }
        }
        
        /// <summary>
        /// DTO для получения бинарных данных от JavaScript
        /// </summary>
        [Serializable]
        private class NetworkResponseWithBinaryDto : NetworkResponseDto
        {
            [JsonProperty("binaryDataBase64")]
            public string BinaryDataBase64 { get; set; }
        }
    }
}