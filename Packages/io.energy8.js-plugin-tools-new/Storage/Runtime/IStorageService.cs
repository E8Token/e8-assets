using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Energy8.JSPluginTools.Storage
{
    /// <summary>
    /// Interface for accessing browser storage mechanisms in WebGL builds
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Checks if storage is available in the browser
        /// </summary>
        /// <param name="storageType">Type of storage to check</param>
        /// <returns>True if the storage is available, false otherwise</returns>
        Task<bool> IsAvailable(StorageType storageType);

        /// <summary>
        /// Gets a value from storage
        /// </summary>
        /// <param name="key">The key to retrieve</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>The value if found, null otherwise</returns>
        Task<string> GetItem(string key, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Gets a value from storage and deserializes it to the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="key">The key to retrieve</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>The deserialized object if found, default(T) otherwise</returns>
        Task<T> GetItem<T>(string key, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Sets a value in storage
        /// </summary>
        /// <param name="key">The key to set</param>
        /// <param name="value">The value to store</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        Task<bool> SetItem(string key, string value, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Sets an object in storage by serializing it to JSON
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="key">The key to set</param>
        /// <param name="value">The object to serialize and store</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        Task<bool> SetItem<T>(string key, T value, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Removes an item from storage
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        Task<bool> RemoveItem(string key, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Checks if a key exists in storage
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>True if the key exists, false otherwise</returns>
        Task<bool> HasKey(string key, StorageType storageType = StorageType.Local);

        /// <summary>
        /// Gets all keys in storage
        /// </summary>
        /// <param name="storageType">Type of storage to use</param>
        /// <returns>List of all keys in storage</returns>
        Task<List<string>> GetAllKeys(StorageType storageType = StorageType.Local);

        /// <summary>
        /// Clears all items from storage
        /// </summary>
        /// <param name="storageType">Type of storage to clear</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        Task<bool> Clear(StorageType storageType = StorageType.Local);

        /// <summary>
        /// Gets the size of the data stored in bytes
        /// </summary>
        /// <param name="storageType">Type of storage to check</param>
        /// <returns>Size in bytes</returns>
        Task<long> GetStorageSize(StorageType storageType = StorageType.Local);

        /// <summary>
        /// Gets the remaining storage quota in bytes
        /// </summary>
        /// <param name="storageType">Type of storage to check</param>
        /// <returns>Remaining space in bytes, or -1 if not available</returns>
        Task<long> GetRemainingSpace(StorageType storageType = StorageType.Local);
        
        /// <summary>
        /// For IndexedDB operations only: Opens a specific database
        /// </summary>
        /// <param name="databaseName">Name of the database</param>
        /// <param name="version">Version of the database</param>
        /// <returns>True if open was successful</returns>
        Task<bool> OpenDatabase(string databaseName, int version = 1);
        
        /// <summary>
        /// For IndexedDB operations only: Creates an object store in the database
        /// </summary>
        /// <param name="storeName">Name of the object store to create</param>
        /// <param name="keyPath">Key path for the object store</param>
        /// <returns>True if creation was successful</returns>
        Task<bool> CreateObjectStore(string storeName, string keyPath = "id");
        
        /// <summary>
        /// For IndexedDB operations only: Adds an object to a store
        /// </summary>
        /// <typeparam name="T">Type of the object to add</typeparam>
        /// <param name="storeName">Name of the object store</param>
        /// <param name="data">Object to add</param>
        /// <param name="key">Optional key if not using keyPath</param>
        /// <returns>True if the operation was successful</returns>
        Task<bool> AddToStore<T>(string storeName, T data, string key = null);
        
        /// <summary>
        /// For IndexedDB operations only: Gets an object from a store
        /// </summary>
        /// <typeparam name="T">Type of the object to retrieve</typeparam>
        /// <param name="storeName">Name of the object store</param>
        /// <param name="key">Key of the object to retrieve</param>
        /// <returns>The retrieved object</returns>
        Task<T> GetFromStore<T>(string storeName, string key);
        
        /// <summary>
        /// For IndexedDB operations only: Removes an object from a store
        /// </summary>
        /// <param name="storeName">Name of the object store</param>
        /// <param name="key">Key of the object to remove</param>
        /// <returns>True if removal was successful</returns>
        Task<bool> RemoveFromStore(string storeName, string key);
    }

    /// <summary>
    /// Types of browser storage mechanisms
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// LocalStorage - persistent storage
        /// </summary>
        Local,
        
        /// <summary>
        /// SessionStorage - clears when the session ends
        /// </summary>
        Session,
        
        /// <summary>
        /// IndexedDB - for larger structured data
        /// </summary>
        IndexedDB
    }
}