using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Storage module for JSPluginTools.
    /// Provides unified access to browser storage mechanisms in WebGL builds
    /// with fallback to PlayerPrefs in non-WebGL environments.
    /// </summary>
    public static class JSPluginStorage
    {
        #region Native Methods
        
        [DllImport("__Internal")]
        private static extern int JSPluginLocalStorageSetItem(string key, string value);
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginLocalStorageGetItem(string key, string defaultValue);
        
        [DllImport("__Internal")]
        private static extern int JSPluginLocalStorageHasKey(string key);
        
        [DllImport("__Internal")]
        private static extern int JSPluginLocalStorageDeleteItem(string key);
        
        [DllImport("__Internal")]
        private static extern int JSPluginLocalStorageClear();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginLocalStorageGetKeys();
        
        [DllImport("__Internal")]
        private static extern int JSPluginStorageInitialize();
        
        [DllImport("__Internal")]
        private static extern int JSPluginSessionStorageSetItem(string key, string value);
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginSessionStorageGetItem(string key, string defaultValue);
        
        [DllImport("__Internal")]
        private static extern int JSPluginSessionStorageHasKey(string key);
        
        [DllImport("__Internal")]
        private static extern int JSPluginSessionStorageDeleteItem(string key);
        
        [DllImport("__Internal")]
        private static extern int JSPluginSessionStorageClear();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginSessionStorageGetKeys();
        
        #endregion
        
        /// <summary>
        /// Storage type to use for operations
        /// </summary>
        public enum StorageType
        {
            /// <summary>Long-term persistent storage (localStorage)</summary>
            Persistent,
            
            /// <summary>Session-only storage that clears when browser is closed (sessionStorage)</summary>
            Session
        }
        
        /// <summary>
        /// Initializes the storage module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginStorageInitialize() == 1;
            #else
            return true;
            #endif
        }
        
        /// <summary>
        /// Shuts down the storage module
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                // Save any pending changes
                LocalStorage.Save();
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                // Optional: Call any JavaScript cleanup methods if needed
                #endif
                
                Debug.Log("[JSPluginStorage] Storage module shut down");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginStorage] Error during shutdown: {ex.Message}");
            }
        }
        
        private static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            
            return Marshal.PtrToStringUTF8(ptr);
        }
        
        #region LocalStorage Implementation

        /// <summary>
        /// Provides access to browser localStorage with PlayerPrefs fallback.
        /// </summary>
        public static class LocalStorage
        {
            private static bool isInitialized = false;
            
            /// <summary>
            /// Initializes the local storage
            /// </summary>
            /// <returns>True if initialization was successful</returns>
            public static bool Initialize()
            {
                if (isInitialized)
                    return true;
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                isInitialized = JSPluginStorageInitialize() == 1;
                #else
                Debug.Log("[JSPluginStorage] LocalStorage initialized in stub mode (non-WebGL environment)");
                isInitialized = true;
                #endif
                
                return isInitialized;
            }
            
            /// <summary>
            /// Stores a string value in localStorage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The string value to store</param>
            /// <returns>True if successful</returns>
            public static bool SetString(string key, string value)
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                    
                if (value == null)
                    value = string.Empty;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginLocalStorageSetItem(key, value) == 1;
                    #else
                    PlayerPrefs.SetString(key, value);
                    PlayerPrefs.Save();
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error setting value for key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            /// <summary>
            /// Retrieves a string value from localStorage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <returns>The stored string or defaultValue if not found</returns>
            public static string GetString(string key, string defaultValue = "")
            {
                if (string.IsNullOrEmpty(key))
                    return defaultValue;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    IntPtr ptr = JSPluginLocalStorageGetItem(key, defaultValue);
                    string result = Marshal.PtrToStringUTF8(ptr);
                    return string.IsNullOrEmpty(result) ? defaultValue : result;
                    #else
                    return PlayerPrefs.GetString(key, defaultValue);
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error getting value for key '{key}': {ex.Message}");
                    return defaultValue;
                }
            }
            
            /// <summary>
            /// Checks if a key exists in localStorage
            /// </summary>
            /// <param name="key">The key to check</param>
            /// <returns>True if the key exists</returns>
            public static bool HasKey(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                    
                if (!isInitialized)
                    Initialize();
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginLocalStorageHasKey(key) == 1;
                #else
                return PlayerPrefs.HasKey(key);
                #endif
            }
            
            /// <summary>
            /// Deletes a key from localStorage
            /// </summary>
            /// <param name="key">The key to delete</param>
            /// <returns>True if successful</returns>
            public static bool DeleteKey(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginLocalStorageDeleteItem(key) == 1;
                    #else
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error deleting key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            /// <summary>
            /// Clears all data from localStorage
            /// </summary>
            /// <returns>True if successful</returns>
            public static bool DeleteAll()
            {
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginLocalStorageClear() == 1;
                    #else
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error clearing localStorage: {ex.Message}");
                    return false;
                }
            }
            
            /// <summary>
            /// Gets all keys in localStorage
            /// </summary>
            /// <returns>Array of keys</returns>
            public static string[] GetKeys()
            {
                if (!isInitialized)
                    Initialize();
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                IntPtr ptr = JSPluginLocalStorageGetKeys();
                string keysJson = Marshal.PtrToStringUTF8(ptr);
                
                if (string.IsNullOrEmpty(keysJson))
                    return new string[0];
                    
                try
                {
                    return JsonUtility.FromJson<StringArray>(keysJson).Items;
                }
                catch
                {
                    Debug.LogError("[JSPluginStorage] Error parsing localStorage keys");
                    return new string[0];
                }
                #else
                Debug.LogWarning("[JSPluginStorage] GetKeys() is not fully supported in non-WebGL environments");
                return new string[0];
                #endif
            }
            
            /// <summary>
            /// Saves any pending changes (only needed in some environments)
            /// </summary>
            /// <returns>True if successful</returns>
            public static bool Save()
            {
                if (!isInitialized)
                    Initialize();
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                // No explicit save needed for localStorage
                return true;
                #else
                PlayerPrefs.Save();
                return true;
                #endif
            }
            
            #region Type-Specific Methods
            
            /// <summary>
            /// Stores an integer value in localStorage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The integer value to store</param>
            /// <returns>True if successful</returns>
            public static bool SetInt(string key, int value)
            {
                return SetString(key, value.ToString());
            }
            
            /// <summary>
            /// Retrieves an integer value from localStorage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <returns>The stored integer or defaultValue if not found</returns>
            public static int GetInt(string key, int defaultValue = 0)
            {
                string value = GetString(key, defaultValue.ToString());
                if (int.TryParse(value, out int result))
                    return result;
                return defaultValue;
            }
            
            /// <summary>
            /// Stores a float value in localStorage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The float value to store</param>
            /// <returns>True if successful</returns>
            public static bool SetFloat(string key, float value)
            {
                return SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            
            /// <summary>
            /// Retrieves a float value from localStorage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <returns>The stored float or defaultValue if not found</returns>
            public static float GetFloat(string key, float defaultValue = 0f)
            {
                string value = GetString(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (float.TryParse(value, System.Globalization.NumberStyles.Float, 
                                  System.Globalization.CultureInfo.InvariantCulture, out float result))
                    return result;
                return defaultValue;
            }
            
            /// <summary>
            /// Stores a boolean value in localStorage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The boolean value to store</param>
            /// <returns>True if successful</returns>
            public static bool SetBool(string key, bool value)
            {
                return SetInt(key, value ? 1 : 0);
            }
            
            /// <summary>
            /// Retrieves a boolean value from localStorage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <returns>The stored boolean or defaultValue if not found</returns>
            public static bool GetBool(string key, bool defaultValue = false)
            {
                return GetInt(key, defaultValue ? 1 : 0) != 0;
            }
            
            /// <summary>
            /// Stores an object in localStorage by serializing it to JSON
            /// </summary>
            /// <typeparam name="T">Type of object to store</typeparam>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The object to store</param>
            /// <returns>True if successful</returns>
            public static bool SetObject<T>(string key, T value)
            {
                if (value == null)
                {
                    return DeleteKey(key);
                }
                
                try
                {
                    string json = JsonUtility.ToJson(value);
                    return SetString(key, json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error serializing object for key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            /// <summary>
            /// Retrieves an object from localStorage by deserializing it from JSON
            /// </summary>
            /// <typeparam name="T">Type of object to retrieve</typeparam>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist or deserialization fails</param>
            /// <returns>The deserialized object or defaultValue if not found</returns>
            public static T GetObject<T>(string key, T defaultValue = default)
            {
                if (!HasKey(key))
                    return defaultValue;
                    
                string json = GetString(key);
                if (string.IsNullOrEmpty(json))
                    return defaultValue;
                    
                try
                {
                    T result = JsonUtility.FromJson<T>(json);
                    return result != null ? result : defaultValue;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error deserializing object for key '{key}': {ex.Message}");
                    return defaultValue;
                }
            }
            
            #endregion
        }

        #endregion

        #region SessionStorage Implementation

        /// <summary>
        /// Provides access to browser sessionStorage with PlayerPrefs fallback.
        /// Session storage is cleared when the browser tab is closed.
        /// </summary>
        public static class SessionStorage
        {
            private static bool isInitialized = false;
            
            /// <summary>
            /// Initializes the session storage
            /// </summary>
            /// <returns>True if initialization was successful</returns>
            public static bool Initialize()
            {
                if (isInitialized)
                    return true;
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                isInitialized = JSPluginStorageInitialize() == 1;
                #else
                Debug.Log("[JSPluginStorage] SessionStorage initialized in stub mode (non-WebGL environment)");
                isInitialized = true;
                #endif
                
                return isInitialized;
            }
            
            /// <summary>
            /// Stores a string value in sessionStorage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The string value to store</param>
            /// <returns>True if successful</returns>
            public static bool SetString(string key, string value)
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                    
                if (value == null)
                    value = string.Empty;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginSessionStorageSetItem(key, value) == 1;
                    #else
                    // Use PlayerPrefs with a session prefix for the fallback
                    PlayerPrefs.SetString("session_" + key, value);
                    PlayerPrefs.Save();
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error setting session value for key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            /// <summary>
            /// Retrieves a string value from sessionStorage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <returns>The stored string or defaultValue if not found</returns>
            public static string GetString(string key, string defaultValue = "")
            {
                if (string.IsNullOrEmpty(key))
                    return defaultValue;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    IntPtr ptr = JSPluginSessionStorageGetItem(key, defaultValue);
                    string result = Marshal.PtrToStringUTF8(ptr);
                    return string.IsNullOrEmpty(result) ? defaultValue : result;
                    #else
                    return PlayerPrefs.GetString("session_" + key, defaultValue);
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error getting session value for key '{key}': {ex.Message}");
                    return defaultValue;
                }
            }
            
            /// <summary>
            /// Checks if a key exists in sessionStorage
            /// </summary>
            /// <param name="key">The key to check</param>
            /// <returns>True if the key exists</returns>
            public static bool HasKey(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                    
                if (!isInitialized)
                    Initialize();
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginSessionStorageHasKey(key) == 1;
                #else
                return PlayerPrefs.HasKey("session_" + key);
                #endif
            }
            
            /// <summary>
            /// Deletes a key from sessionStorage
            /// </summary>
            /// <param name="key">The key to delete</param>
            /// <returns>True if successful</returns>
            public static bool DeleteKey(string key)
            {
                if (string.IsNullOrEmpty(key))
                    return false;
                    
                if (!isInitialized)
                    Initialize();
                    
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginSessionStorageDeleteItem(key) == 1;
                    #else
                    PlayerPrefs.DeleteKey("session_" + key);
                    PlayerPrefs.Save();
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error deleting session key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            #region Type-Specific Methods
            
            // These methods mirror the functionality of LocalStorage but for sessionStorage
            
            public static bool SetInt(string key, int value) => SetString(key, value.ToString());
            
            public static int GetInt(string key, int defaultValue = 0)
            {
                string value = GetString(key, defaultValue.ToString());
                if (int.TryParse(value, out int result))
                    return result;
                return defaultValue;
            }
            
            public static bool SetFloat(string key, float value) =>
                SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            
            public static float GetFloat(string key, float defaultValue = 0f)
            {
                string value = GetString(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (float.TryParse(value, System.Globalization.NumberStyles.Float, 
                                  System.Globalization.CultureInfo.InvariantCulture, out float result))
                    return result;
                return defaultValue;
            }
            
            public static bool SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);
            
            public static bool GetBool(string key, bool defaultValue = false) => 
                GetInt(key, defaultValue ? 1 : 0) != 0;
            
            public static bool SetObject<T>(string key, T value)
            {
                if (value == null)
                {
                    return DeleteKey(key);
                }
                
                try
                {
                    string json = JsonUtility.ToJson(value);
                    return SetString(key, json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error serializing session object for key '{key}': {ex.Message}");
                    return false;
                }
            }
            
            public static T GetObject<T>(string key, T defaultValue = default)
            {
                if (!HasKey(key))
                    return defaultValue;
                    
                string json = GetString(key);
                if (string.IsNullOrEmpty(json))
                    return defaultValue;
                    
                try
                {
                    T result = JsonUtility.FromJson<T>(json);
                    return result != null ? result : defaultValue;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginStorage] Error deserializing session object for key '{key}': {ex.Message}");
                    return defaultValue;
                }
            }
            
            #endregion
        }

        #endregion

        #region Generic Storage API

        /// <summary>
        /// Provides a unified API for working with different storage mechanisms
        /// </summary>
        public static class Store
        {
            /// <summary>
            /// Gets or sets a string value in the specified storage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The string value to store (set) or null (get)</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The stored string value or empty string if not found</returns>
            public static string String(string key, string value = null, StorageType storageType = StorageType.Persistent)
            {
                // If value is null, this is a get operation
                if (value == null)
                {
                    return GetString(key, string.Empty, storageType);
                }
                
                // Otherwise it's a set operation
                SetString(key, value, storageType);
                return value;
            }
            
            /// <summary>
            /// Sets a string value in the specified storage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The string value to store</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool SetString(string key, string value, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.SetString(key, value)
                    : SessionStorage.SetString(key, value);
            }
            
            /// <summary>
            /// Gets a string value from the specified storage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The stored string or defaultValue if not found</returns>
            public static string GetString(string key, string defaultValue = "", StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.GetString(key, defaultValue)
                    : SessionStorage.GetString(key, defaultValue);
            }
            
            /// <summary>
            /// Checks if a key exists in the specified storage
            /// </summary>
            /// <param name="key">The key to check</param>
            /// <param name="storageType">Type of storage to check</param>
            /// <returns>True if the key exists</returns>
            public static bool HasKey(string key, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.HasKey(key)
                    : SessionStorage.HasKey(key);
            }
            
            /// <summary>
            /// Deletes a key from the specified storage
            /// </summary>
            /// <param name="key">The key to delete</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool DeleteKey(string key, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.DeleteKey(key)
                    : SessionStorage.DeleteKey(key);
            }
            
            /// <summary>
            /// Sets an integer value in the specified storage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The integer value to store</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool SetInt(string key, int value, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.SetInt(key, value)
                    : SessionStorage.SetInt(key, value);
            }
            
            /// <summary>
            /// Gets an integer value from the specified storage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The stored integer or defaultValue if not found</returns>
            public static int GetInt(string key, int defaultValue = 0, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.GetInt(key, defaultValue)
                    : SessionStorage.GetInt(key, defaultValue);
            }
            
            /// <summary>
            /// Sets a float value in the specified storage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The float value to store</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool SetFloat(string key, float value, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.SetFloat(key, value)
                    : SessionStorage.SetFloat(key, value);
            }
            
            /// <summary>
            /// Gets a float value from the specified storage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The stored float or defaultValue if not found</returns>
            public static float GetFloat(string key, float defaultValue = 0f, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.GetFloat(key, defaultValue)
                    : SessionStorage.GetFloat(key, defaultValue);
            }
            
            /// <summary>
            /// Sets a boolean value in the specified storage
            /// </summary>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The boolean value to store</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool SetBool(string key, bool value, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.SetBool(key, value)
                    : SessionStorage.SetBool(key, value);
            }
            
            /// <summary>
            /// Gets a boolean value from the specified storage
            /// </summary>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The stored boolean or defaultValue if not found</returns>
            public static bool GetBool(string key, bool defaultValue = false, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.GetBool(key, defaultValue)
                    : SessionStorage.GetBool(key, defaultValue);
            }
            
            /// <summary>
            /// Sets an object in the specified storage by serializing it to JSON
            /// </summary>
            /// <typeparam name="T">Type of object to store</typeparam>
            /// <param name="key">The key to store under</param>
            /// <param name="value">The object to store</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>True if successful</returns>
            public static bool SetObject<T>(string key, T value, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.SetObject(key, value)
                    : SessionStorage.SetObject(key, value);
            }
            
            /// <summary>
            /// Gets an object from the specified storage by deserializing it from JSON
            /// </summary>
            /// <typeparam name="T">Type of object to retrieve</typeparam>
            /// <param name="key">The key to retrieve</param>
            /// <param name="defaultValue">Default value if key doesn't exist or deserialization fails</param>
            /// <param name="storageType">Type of storage to use</param>
            /// <returns>The deserialized object or defaultValue if not found</returns>
            public static T GetObject<T>(string key, T defaultValue = default, StorageType storageType = StorageType.Persistent)
            {
                return storageType == StorageType.Persistent
                    ? LocalStorage.GetObject(key, defaultValue)
                    : SessionStorage.GetObject(key, defaultValue);
            }
        }

        /// <summary>
        /// Helper class for serializing string arrays
        /// </summary>
        [Serializable]
        internal class StringArray
        {
            public string[] Items;
        }

        #endregion

        /// <summary>
        /// Compatibility class for backward compatibility with older code.
        /// Simply forwards calls to JSPluginStorage.LocalStorage.
        /// </summary>
        public static class JSLocalStorage
        {
            // Forward all method calls to JSPluginStorage.LocalStorage
            public static void SetString(string key, string value) => JSPluginStorage.LocalStorage.SetString(key, value);
            public static string GetString(string key, string defaultValue = "") => JSPluginStorage.LocalStorage.GetString(key, defaultValue);
            public static void SetInt(string key, int value) => JSPluginStorage.LocalStorage.SetInt(key, value);
            public static int GetInt(string key, int defaultValue = 0) => JSPluginStorage.LocalStorage.GetInt(key, defaultValue);
            public static void SetFloat(string key, float value) => JSPluginStorage.LocalStorage.SetFloat(key, value);
            public static float GetFloat(string key, float defaultValue = 0f) => JSPluginStorage.LocalStorage.GetFloat(key, defaultValue);
            public static void SetBool(string key, bool value) => JSPluginStorage.LocalStorage.SetBool(key, value);
            public static bool GetBool(string key, bool defaultValue = false) => JSPluginStorage.LocalStorage.GetBool(key, defaultValue);
            public static void SetObject<T>(string key, T value) => JSPluginStorage.LocalStorage.SetObject(key, value);
            public static T GetObject<T>(string key, T defaultValue = default) => JSPluginStorage.LocalStorage.GetObject(key, defaultValue);
            public static bool HasKey(string key) => JSPluginStorage.LocalStorage.HasKey(key);
            public static void DeleteKey(string key) => JSPluginStorage.LocalStorage.DeleteKey(key);
            public static void DeleteAll() => JSPluginStorage.LocalStorage.DeleteAll();
            public static void Save() => JSPluginStorage.LocalStorage.Save();
            public static string[] GetKeys() => JSPluginStorage.LocalStorage.GetKeys();
        }
    }
}