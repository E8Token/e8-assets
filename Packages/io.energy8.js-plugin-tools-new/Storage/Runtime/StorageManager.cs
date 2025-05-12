using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.Storage
{
    /// <summary>
    /// Реализация интерфейса IStorageManager для работы с хранилищем браузера
    /// </summary>
    public class StorageManager : IStorageManager
    {
        private IPluginCore _core;
        private IMessageBus _messageBus;
        private bool _isInitialized = false;
        
        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;
        
        /// <inheritdoc/>
        public event Action OnInitialized;
        
        private const string MESSAGE_PREFIX = "storage.";
        
        /// <inheritdoc/>
        public void Initialize(IPluginCore core)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("JSPluginTools [Storage]: Already initialized");
                return;
            }

            _core = core ?? throw new ArgumentNullException(nameof(core));
            
            if (_core is PluginCore pluginCore)
            {
                _messageBus = pluginCore.MessageBus;
            }
            else
            {
                throw new InvalidOperationException("JSPluginTools [Storage]: Core implementation does not provide access to MessageBus");
            }

            _isInitialized = true;
            Debug.Log("JSPluginTools [Storage]: Module initialized");
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public async Task<bool> SetItemAsync(string key, string value, StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            try
            {
                string method = "setItem";
                var parameters = new
                {
                    key,
                    value,
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error setting item with key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetJsonItemAsync<T>(string key, T value, StorageType storageType = StorageType.Local)
        {
            try
            {
                string jsonValue = JsonConvert.SerializeObject(value);
                return await SetItemAsync(key, jsonValue, storageType);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error serializing object to JSON for key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetItemAsync(string key, StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            try
            {
                string method = "getItem";
                var parameters = new
                {
                    key,
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<string>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting item with key '{key}': {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetJsonItemAsync<T>(string key, StorageType storageType = StorageType.Local)
        {
            try
            {
                string jsonValue = await GetItemAsync(key, storageType);
                
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return default;
                }
                
                return JsonConvert.DeserializeObject<T>(jsonValue);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error deserializing JSON for key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveItemAsync(string key, StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            try
            {
                string method = "removeItem";
                var parameters = new
                {
                    key,
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error removing item with key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasKeyAsync(string key, StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            try
            {
                string method = "hasKey";
                var parameters = new
                {
                    key,
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error checking if key '{key}' exists: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ClearAsync(StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            try
            {
                string method = "clear";
                var parameters = new
                {
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error clearing storage: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string[]> GetKeysAsync(StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            try
            {
                string method = "getKeys";
                var parameters = new
                {
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<string[]>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting keys: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetSizeAsync(StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            try
            {
                string method = "getSize";
                var parameters = new
                {
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<long>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting storage size: {ex.Message}");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsAvailableAsync(StorageType storageType = StorageType.Local)
        {
            CheckInitialized();
            
            try
            {
                string method = "isAvailable";
                var parameters = new
                {
                    storageType = (int)storageType
                };
                
                return await SendStorageRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error checking storage availability: {ex.Message}");
                return false;
            }
        }
        
        private async Task<T> SendStorageRequestAsync<T>(string method, object parameters)
        {
            string messageType = GetMessageType(method);
            
            var tcs = new TaskCompletionSource<T>();
            
            try
            {
                _messageBus.SendMessageWithResponse<object, T>(
                    messageType,
                    parameters,
                    response => tcs.SetResult(response));
                    
                Debug.Log($"JSPluginTools [Storage]: Sent request '{method}'");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error sending storage request '{method}': {ex.Message}");
                tcs.SetException(ex);
                throw;
            }
        }
        
        private string GetMessageType(string method)
        {
            return $"{MESSAGE_PREFIX}{method}";
        }
        
        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("JSPluginTools [Storage]: Module not initialized. Call Initialize() first");
            }
        }
    }
}