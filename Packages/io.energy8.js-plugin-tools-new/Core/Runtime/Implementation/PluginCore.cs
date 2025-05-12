using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools.Core.Implementation
{
    /// <summary>
    /// Main implementation of the IPluginCore interface
    /// </summary>
    public class PluginCore : IPluginCore
    {
        private bool _isInitialized = false;
        private Dictionary<string, string> _registeredObjects = new Dictionary<string, string>();
        private static int _objectIdCounter = 0;

        /// <summary>
        /// Instance of the memory manager used by this plugin core
        /// </summary>
        public IMemoryManager MemoryManager { get; private set; }

        /// <summary>
        /// Instance of the message bus used by this plugin core
        /// </summary>
        public IMessageBus MessageBus { get; private set; }

        /// <summary>
        /// Creates a new instance of the PluginCore
        /// </summary>
        /// <param name="memoryManager">The memory manager implementation to use</param>
        /// <param name="messageBus">The message bus implementation to use</param>
        public PluginCore(IMemoryManager memoryManager, IMessageBus messageBus)
        {
            MemoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
            MessageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <inheritdoc/>
        public event Action OnInitialized;

        /// <inheritdoc/>
        public event Action OnShutdown;

        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc/>
        public bool IsWebGLContext
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("JSPluginTools: Plugin is already initialized");
                return;
            }

            try
            {
                if (IsWebGLContext)
                {
                    // Call JavaScript initialization method
                    JSPluginInitialize();
                }

                _isInitialized = true;
                Debug.Log("JSPluginTools: Plugin initialized successfully");
                OnInitialized?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools: Failed to initialize plugin: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            try
            {
                OnShutdown?.Invoke();

                if (IsWebGLContext)
                {
                    // Call JavaScript cleanup method
                    JSPluginShutdown();
                }

                // Clear resources
                _registeredObjects.Clear();
                MemoryManager.ReleaseAll();
                
                _isInitialized = false;
                Debug.Log("JSPluginTools: Plugin shutdown successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools: Error during plugin shutdown: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public string RegisterGameObject(string gameObjectName)
        {
            if (string.IsNullOrEmpty(gameObjectName))
            {
                throw new ArgumentException("Game object name cannot be null or empty", nameof(gameObjectName));
            }

            string objectId = GenerateObjectId();
            _registeredObjects[objectId] = gameObjectName;

            if (IsWebGLContext)
            {
                JSRegisterGameObject(gameObjectName, objectId);
            }

            Debug.Log($"JSPluginTools: Registered GameObject '{gameObjectName}' with ID '{objectId}'");
            return objectId;
        }

        /// <inheritdoc/>
        public void UnregisterGameObject(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                throw new ArgumentException("Object ID cannot be null or empty", nameof(objectId));
            }

            if (!_registeredObjects.TryGetValue(objectId, out string gameObjectName))
            {
                Debug.LogWarning($"JSPluginTools: Attempted to unregister unknown object ID: {objectId}");
                return;
            }

            if (IsWebGLContext)
            {
                JSUnregisterGameObject(objectId);
            }

            _registeredObjects.Remove(objectId);
            Debug.Log($"JSPluginTools: Unregistered GameObject '{gameObjectName}' with ID '{objectId}'");
        }

        private string GenerateObjectId()
        {
            return $"jsplugintoolsobj_{_objectIdCounter++}";
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JSPluginInitialize();

        [DllImport("__Internal")]
        private static extern void JSPluginShutdown();

        [DllImport("__Internal")]
        private static extern void JSRegisterGameObject(string gameObjectName, string objectId);

        [DllImport("__Internal")]
        private static extern void JSUnregisterGameObject(string objectId);
#else
        // Stub implementations for non-WebGL platforms
        private static void JSPluginInitialize() { }
        private static void JSPluginShutdown() { }
        private static void JSRegisterGameObject(string gameObjectName, string objectId) { }
        private static void JSUnregisterGameObject(string objectId) { }
#endif
    }
}