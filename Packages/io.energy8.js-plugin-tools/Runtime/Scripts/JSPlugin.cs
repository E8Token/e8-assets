using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Main entry point for the JSPlugin system.
    /// Provides access to all modules and simplified API for common operations.
    /// </summary>
    public static class JSPlugin
    {
        private static bool isInitialized = false;
        
        /// <summary>
        /// Whether debug mode is enabled
        /// </summary>
        public static bool DebugMode { get; private set; }
        
        /// <summary>
        /// Initializes all JSPlugin modules
        /// </summary>
        /// <param name="debugMode">Whether to enable debug logging</param>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize(bool debugMode = false)
        {
            if (isInitialized)
                return true;
                
            DebugMode = debugMode;
            bool success = true;
            
            try
            {
                // Start with core initialization
                success = JSPluginCore.Initialize(debugMode);
                if (!success)
                {
                    Debug.LogError("[JSPlugin] Core initialization failed");
                    return false;
                }
                
                // Then initialize other modules
                // These won't use &= operator to avoid void return type issues
                if (success) 
                {
                    JSPluginErrorHandling.Initialize();
                    JSPluginCommunication.Initialize();
                    JSPluginStorage.Initialize();
                    JSPluginDOM.Initialize();
                    JSPluginDevice.Initialize();
                    JSPluginNetwork.Initialize();
                }
                
                Debug.Log("[JSPlugin] All modules initialized successfully");
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPlugin] Error during initialization: {ex.Message}");
                success = false;
            }
            
            return success;
        }
        
        /// <summary>
        /// Shuts down all JSPlugin modules and releases resources
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            // Shutdown modules in reverse order of initialization
            JSPluginNetwork.Shutdown();
            JSPluginDevice.Shutdown();
            JSPluginDOM.Shutdown();
            JSPluginStorage.Shutdown();
            JSPluginCommunication.Shutdown();
            JSPluginErrorHandling.Shutdown();
            JSPluginCore.Shutdown();
            
            isInitialized = false;
            Debug.Log("[JSPlugin] All modules shut down");
        }
        
        /// <summary>
        /// Creates and registers a plugin object
        /// </summary>
        /// <param name="objectId">Unique identifier for the object</param>
        /// <param name="gameObject">The Unity GameObject to associate with the plugin</param>
        /// <returns>A new JSPluginObject instance</returns>
        public static JSPluginObject CreatePlugin(string objectId, GameObject gameObject)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginCore.RegisterObject(objectId, gameObject);
        }
        
        /// <summary>
        /// Gets a previously registered plugin object
        /// </summary>
        /// <param name="objectId">The ID used during registration</param>
        /// <returns>The registered plugin object or null if not found</returns>
        public static JSPluginObject GetPlugin(string objectId)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginCore.GetObject(objectId);
        }
        
        #region Storage Shortcuts
        
        /// <summary>
        /// Saves a string value to localStorage
        /// </summary>
        /// <param name="key">The key under which to store the value</param>
        /// <param name="value">The string value to store</param>
        public static void SetString(string key, string value)
        {
            if (!isInitialized)
                Initialize();
                
            JSPluginStorage.LocalStorage.SetString(key, value);
        }
        
        /// <summary>
        /// Retrieves a string value from localStorage
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">The default value to return if the key is not found</param>
        /// <returns>The stored string or defaultValue if the key doesn't exist</returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginStorage.LocalStorage.GetString(key, defaultValue);
        }
        
        /// <summary>
        /// Saves an object to localStorage by serializing it to JSON
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="key">The key under which to store the object</param>
        /// <param name="value">The object to serialize and store</param>
        public static void SetObject<T>(string key, T value)
        {
            if (!isInitialized)
                Initialize();
                
            JSPluginStorage.LocalStorage.SetObject(key, value);
        }
        
        /// <summary>
        /// Retrieves an object from localStorage by deserializing it from JSON
        /// </summary>
        /// <typeparam name="T">The type to deserialize the object to</typeparam>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">The default value to return if the key is not found or deserialization fails</param>
        /// <returns>The deserialized object or defaultValue if the key doesn't exist or deserialization fails</returns>
        public static T GetObject<T>(string key, T defaultValue = default)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginStorage.LocalStorage.GetObject(key, defaultValue);
        }
        
        #endregion
        
        #region Device Shortcuts
        
        /// <summary>
        /// Checks if the current device is a mobile device
        /// </summary>
        /// <returns>True if the device is mobile</returns>
        public static bool IsMobileDevice()
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginDevice.IsMobile();
        }
        
        /// <summary>
        /// Requests fullscreen mode for the application
        /// </summary>
        /// <returns>True if fullscreen request was successful</returns>
        public static bool RequestFullscreen()
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginDevice.RequestFullscreen();
        }
        
        /// <summary>
        /// Makes the device vibrate (only works on mobile devices that support vibration)
        /// </summary>
        /// <param name="milliseconds">Duration of vibration in milliseconds</param>
        /// <returns>True if vibration request was successful</returns>
        public static bool Vibrate(int milliseconds = 200)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginDevice.Vibrate(milliseconds);
        }
        
        #endregion
        
        #region Communication Shortcuts
        
        /// <summary>
        /// Creates a new communication channel
        /// </summary>
        /// <param name="channelId">Unique identifier for the channel</param>
        /// <param name="gameObject">GameObject that will receive messages</param>
        /// <returns>A new communication channel</returns>
        public static JSPluginCommunication.JSChannel CreateChannel(string channelId, GameObject gameObject)
        {
            if (!isInitialized)
                Initialize();
                
            return new JSPluginCommunication.JSChannel(channelId, gameObject);
        }
        
        /// <summary>
        /// Broadcasts a message to all registered JavaScript listeners
        /// </summary>
        /// <param name="eventName">Name of the event to broadcast</param>
        /// <param name="data">Optional data to include</param>
        /// <returns>True if the broadcast was successful</returns>
        public static bool Broadcast(string eventName, string data = null)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginCommunication.Broadcast(eventName, data);
        }
        
        /// <summary>
        /// Broadcasts a structured object by serializing it to JSON
        /// </summary>
        /// <typeparam name="T">Type of object to broadcast</typeparam>
        /// <param name="eventName">Name of the event to broadcast</param>
        /// <param name="data">Data object to serialize</param>
        /// <returns>True if the broadcast was successful</returns>
        public static bool BroadcastJson<T>(string eventName, T data)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginCommunication.BroadcastJson(eventName, data);
        }
        
        #endregion
        
        #region DOM Shortcuts
        
        /// <summary>
        /// Creates a toast notification
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="type">Type of toast (info, success, warning, error)</param>
        /// <returns>The created toast element</returns>
        public static JSPluginDOM.Element ShowToast(string message, float duration = 3f, string type = "info")
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginDOM.Utility.ShowToast(message, duration, type);
        }
        
        /// <summary>
        /// Creates an overlay with the specified content
        /// </summary>
        /// <param name="content">HTML content for the overlay</param>
        /// <param name="id">Optional ID for the overlay</param>
        /// <returns>The created overlay element</returns>
        public static JSPluginDOM.Element ShowOverlay(string content, string id = null)
        {
            if (!isInitialized)
                Initialize();
                
            return JSPluginDOM.Utility.CreateOverlay(content, id);
        }
        
        #endregion
        
        #region Error Handling Shortcuts
        
        /// <summary>
        /// Reports an error through the error handling system
        /// </summary>
        /// <param name="source">Source of the error</param>
        /// <param name="message">Error message</param>
        /// <param name="severity">Error severity</param>
        public static void ReportError(string source, string message, JSPluginErrorHandling.ErrorSeverity severity = JSPluginErrorHandling.ErrorSeverity.Error)
        {
            if (!isInitialized)
                Initialize();
                
            JSPluginErrorHandling.ProcessError(source, "JSPlugin", message, severity);
        }
        
        /// <summary>
        /// Reports an exception through the error handling system
        /// </summary>
        /// <param name="source">Source of the exception</param>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="severity">Error severity</param>
        public static void ReportException(string source, Exception exception, JSPluginErrorHandling.ErrorSeverity severity = JSPluginErrorHandling.ErrorSeverity.Error)
        {
            if (!isInitialized)
                Initialize();
                
            JSPluginErrorHandling.ProcessException(source, "JSPlugin", exception, severity);
        }
        
        #endregion
    }
}
