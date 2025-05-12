using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.Storage
{
    /// <summary>
    /// Implementation of IStorageService interface for accessing browser storage mechanisms
    /// </summary>
    public class StorageService : IStorageService
    {
        private const string CHANNEL_PREFIX = "storage.";
        private readonly ICommunicationService _communicationService;
        private bool _indexedDBInitialized = false;
        private string _currentDatabase = null;

        /// <summary>
        /// Creates a new StorageService instance
        /// </summary>
        /// <param name="communicationService">Communication service for interacting with JavaScript</param>
        public StorageService(ICommunicationService communicationService)
        {
            _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        }

        /// <inheritdoc/>
        public async Task<bool> IsAvailable(StorageType storageType)
        {
            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, bool>(
                    CHANNEL_PREFIX + "isAvailable",
                    new StorageRequest
                    {
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error checking if storage is available: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetItem(string key, StorageType storageType = StorageType.Local)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, string>(
                    CHANNEL_PREFIX + "getItem",
                    new StorageRequest
                    {
                        Key = key,
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting item with key '{key}': {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetItem<T>(string key, StorageType storageType = StorageType.Local)
        {
            string json = await GetItem(key, storageType);
            
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }
            
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error deserializing item with key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetItem(string key, string value, StorageType storageType = StorageType.Local)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, bool>(
                    CHANNEL_PREFIX + "setItem",
                    new StorageRequest
                    {
                        Key = key,
                        Value = value,
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error setting item with key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetItem<T>(string key, T value, StorageType storageType = StorageType.Local)
        {
            string json;
            
            try
            {
                json = JsonConvert.SerializeObject(value);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error serializing item with key '{key}': {ex.Message}");
                return false;
            }
            
            return await SetItem(key, json, storageType);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveItem(string key, StorageType storageType = StorageType.Local)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, bool>(
                    CHANNEL_PREFIX + "removeItem",
                    new StorageRequest
                    {
                        Key = key,
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error removing item with key '{key}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasKey(string key, StorageType storageType = StorageType.Local)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, bool>(
                    CHANNEL_PREFIX + "hasKey",
                    new StorageRequest
                    {
                        Key = key,
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error checking if key '{key}' exists: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetAllKeys(StorageType storageType = StorageType.Local)
        {
            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, List<string>>(
                    CHANNEL_PREFIX + "getAllKeys",
                    new StorageRequest
                    {
                        StorageType = storageType.ToString()
                    }
                );
                
                return response ?? new List<string>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting all keys: {ex.Message}");
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> Clear(StorageType storageType = StorageType.Local)
        {
            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, bool>(
                    CHANNEL_PREFIX + "clear",
                    new StorageRequest
                    {
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error clearing storage: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageSize(StorageType storageType = StorageType.Local)
        {
            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, long>(
                    CHANNEL_PREFIX + "getStorageSize",
                    new StorageRequest
                    {
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting storage size: {ex.Message}");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetRemainingSpace(StorageType storageType = StorageType.Local)
        {
            try
            {
                var response = await _communicationService.SendWithResponseAsync<StorageRequest, long>(
                    CHANNEL_PREFIX + "getRemainingSpace",
                    new StorageRequest
                    {
                        StorageType = storageType.ToString()
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting remaining space: {ex.Message}");
                return -1;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> OpenDatabase(string databaseName, int version = 1)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<IndexedDBRequest, bool>(
                    CHANNEL_PREFIX + "openDatabase",
                    new IndexedDBRequest
                    {
                        DatabaseName = databaseName,
                        Version = version
                    }
                );
                
                if (response)
                {
                    _indexedDBInitialized = true;
                    _currentDatabase = databaseName;
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error opening database '{databaseName}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateObjectStore(string storeName, string keyPath = "id")
        {
            if (!_indexedDBInitialized)
            {
                Debug.LogError("JSPluginTools [Storage]: Cannot create object store - database not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(storeName))
            {
                throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<IndexedDBRequest, bool>(
                    CHANNEL_PREFIX + "createObjectStore",
                    new IndexedDBRequest
                    {
                        StoreName = storeName,
                        KeyPath = keyPath
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error creating object store '{storeName}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AddToStore<T>(string storeName, T data, string key = null)
        {
            if (!_indexedDBInitialized)
            {
                Debug.LogError("JSPluginTools [Storage]: Cannot add to store - database not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(storeName))
            {
                throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));
            }

            try
            {
                string jsonData = JsonConvert.SerializeObject(data);
                
                var response = await _communicationService.SendWithResponseAsync<IndexedDBRequest, bool>(
                    CHANNEL_PREFIX + "addToStore",
                    new IndexedDBRequest
                    {
                        StoreName = storeName,
                        Key = key,
                        Data = jsonData
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error adding to store '{storeName}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetFromStore<T>(string storeName, string key)
        {
            if (!_indexedDBInitialized)
            {
                Debug.LogError("JSPluginTools [Storage]: Cannot get from store - database not initialized");
                return default;
            }

            if (string.IsNullOrEmpty(storeName))
            {
                throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<IndexedDBRequest, string>(
                    CHANNEL_PREFIX + "getFromStore",
                    new IndexedDBRequest
                    {
                        StoreName = storeName,
                        Key = key
                    }
                );
                
                if (string.IsNullOrEmpty(response))
                {
                    return default;
                }
                
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error getting from store '{storeName}': {ex.Message}");
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveFromStore(string storeName, string key)
        {
            if (!_indexedDBInitialized)
            {
                Debug.LogError("JSPluginTools [Storage]: Cannot remove from store - database not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(storeName))
            {
                throw new ArgumentException("Store name cannot be null or empty", nameof(storeName));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            try
            {
                var response = await _communicationService.SendWithResponseAsync<IndexedDBRequest, bool>(
                    CHANNEL_PREFIX + "removeFromStore",
                    new IndexedDBRequest
                    {
                        StoreName = storeName,
                        Key = key
                    }
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [Storage]: Error removing from store '{storeName}': {ex.Message}");
                return false;
            }
        }
        
        // Internal classes for request/response
        
        [Serializable]
        private class StorageRequest
        {
            public string StorageType;
            public string Key;
            public string Value;
        }
        
        [Serializable]
        private class IndexedDBRequest
        {
            public string DatabaseName;
            public int Version;
            public string StoreName;
            public string KeyPath;
            public string Key;
            public string Data;
        }
    }
}