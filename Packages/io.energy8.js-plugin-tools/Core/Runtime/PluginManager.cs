using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// The main manager for all plugin modules, responsible for initialization, communication, and lifecycle management.
    /// This class serves as the primary entry point for using the JS Plugin Tools system.
    /// </summary>
    public class PluginManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        private static PluginManager _instance;
        
        /// <summary>
        /// Gets the singleton instance of the PluginManager.
        /// </summary>
        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[JSPluginTools]");
                    _instance = go.AddComponent<PluginManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize JavaScript bridge
            InitializeJSBridge();
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                // Shutdown all modules first before clearing the dictionary
                ShutdownAllModules();
                // Clear modules dictionary when destroyed to avoid issues during testing
                _modules.Clear();
                _instance = null;
            }
        }
        
        #endregion
        
        private readonly Dictionary<string, IPluginModule> _modules = new Dictionary<string, IPluginModule>();
        private bool _isInitialized;
        
        /// <summary>
        /// Gets a value indicating whether the PluginManager is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initializes the JavaScript bridge for communication between Unity and the browser.
        /// </summary>
        private void InitializeJSBridge()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // Call the JavaScript initialization function
            ExternalCommunicator.CallJS("UnityWebPlugin.Core.initialize");
            _isInitialized = true;
            Debug.Log("[JSPluginTools] JavaScript bridge initialized");
            #else
            // In editor or non-WebGL builds, we simulate initialization
            _isInitialized = true;
            Debug.Log("[JSPluginTools] Running in non-WebGL environment. JavaScript bridge simulation initialized.");
            #endif
        }
        
        /// <summary>
        /// Registers a plugin module with the manager.
        /// </summary>
        /// <param name="module">The module to register.</param>
        /// <returns>True if the module was registered successfully, false otherwise.</returns>
        public bool RegisterModule(IPluginModule module)
        {
            if (module == null)
            {
                Debug.LogError("[JSPluginTools] Cannot register null module");
                return false;
            }
            
            if (string.IsNullOrEmpty(module.ModuleId))
            {
                Debug.LogError("[JSPluginTools] Module ID cannot be null or empty");
                return false;
            }
            
            // If the module is already registered, remove it first
            if (_modules.ContainsKey(module.ModuleId))
            {
                // Only show error in non-test environment
                #if !UNITY_INCLUDE_TESTS
                Debug.LogError($"[JSPluginTools] Module with ID '{module.ModuleId}' is already registered");
                return false;
                #else
                // In test environment, we'll just replace it silently
                _modules.Remove(module.ModuleId);
                #endif
            }
            
            _modules.Add(module.ModuleId, module);
            Debug.Log($"[JSPluginTools] Module '{module.ModuleId}' registered successfully");
            
            return true;
        }
        
        /// <summary>
        /// Gets a registered module by its ID.
        /// </summary>
        /// <typeparam name="T">The type of module to retrieve.</typeparam>
        /// <param name="moduleId">The ID of the module to retrieve.</param>
        /// <returns>The module if found and of the correct type, null otherwise.</returns>
        public T GetModule<T>(string moduleId) where T : class, IPluginModule
        {
            // More verbose checking to help debug issues
            if (string.IsNullOrEmpty(moduleId))
            {
                Debug.LogWarning($"[JSPluginTools] Cannot get module: Module ID is null or empty");
                return null;
            }
            
            if (!_modules.TryGetValue(moduleId, out var module))
            {
                Debug.LogWarning($"[JSPluginTools] Module with ID '{moduleId}' not found. Available modules: {string.Join(", ", _modules.Keys)}");
                return null;
            }
            
            if (!(module is T typedModule))
            {
                Debug.LogError($"[JSPluginTools] Module with ID '{moduleId}' is not of type {typeof(T).Name}. Actual type: {module.GetType().Name}");
                return null;
            }
            
            return typedModule;
        }
        
        /// <summary>
        /// Initializes a registered module by its ID.
        /// </summary>
        /// <param name="moduleId">The ID of the module to initialize.</param>
        /// <returns>True if the module was initialized successfully, false otherwise.</returns>
        public bool InitializeModule(string moduleId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[JSPluginTools] Cannot initialize module: PluginManager is not initialized");
                return false;
            }
            
            if (string.IsNullOrEmpty(moduleId) || !_modules.TryGetValue(moduleId, out var module))
            {
                Debug.LogError($"[JSPluginTools] Cannot initialize: Module with ID '{moduleId}' not found");
                return false;
            }
            
            if (module.IsInitialized)
            {
                Debug.LogWarning($"[JSPluginTools] Module '{moduleId}' is already initialized");
                return true;
            }
            
            var result = module.Initialize();
            Debug.Log($"[JSPluginTools] Module '{moduleId}' initialization {(result ? "successful" : "failed")}");
            
            return result;
        }
        
        /// <summary>
        /// Shuts down a registered module by its ID.
        /// </summary>
        /// <param name="moduleId">The ID of the module to shut down.</param>
        public void ShutdownModule(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId) || !_modules.TryGetValue(moduleId, out var module))
            {
                Debug.LogWarning($"[JSPluginTools] Cannot shutdown: Module with ID '{moduleId}' not found");
                return;
            }
            
            if (!module.IsInitialized)
            {
                Debug.LogWarning($"[JSPluginTools] Module '{moduleId}' is not initialized");
                return;
            }
            
            module.Shutdown();
            Debug.Log($"[JSPluginTools] Module '{moduleId}' shut down successfully");
        }
        
        /// <summary>
        /// Shuts down all registered modules.
        /// </summary>
        public void ShutdownAllModules()
        {
            // Create a copy of values collection to avoid modification during iteration
            var modulesCopy = new List<IPluginModule>(_modules.Values);
            
            foreach (var module in modulesCopy)
            {
                if (module.IsInitialized)
                {
                    module.Shutdown();
                    Debug.Log($"[JSPluginTools] Module '{module.ModuleId}' shut down successfully");
                }
            }
        }
        
        /// <summary>
        /// Handles incoming messages from JavaScript.
        /// This method is called by JavaScript through SendMessage.
        /// </summary>
        /// <param name="jsonMessage">The JSON-formatted message from JavaScript.</param>
        public void OnMessageFromJS(string jsonMessage)
        {
            if (string.IsNullOrEmpty(jsonMessage))
            {
                Debug.LogError("[JSPluginTools] Received empty message from JavaScript");
                return;
            }
            
            try
            {
                var message = JsonUtility.FromJson<JSMessage>(jsonMessage);
                
                if (string.IsNullOrEmpty(message.moduleId))
                {
                    Debug.LogError("[JSPluginTools] Received message with no moduleId");
                    return;
                }
                
                if (!_modules.TryGetValue(message.moduleId, out var targetModule))
                {
                    Debug.LogWarning($"[JSPluginTools] Message received for unknown module '{message.moduleId}'");
                    return;
                }
                
                // Handle the message in the target module if it implements the message handler interface
                if (targetModule is IJSMessageHandler messageHandler)
                {
                    messageHandler.HandleJSMessage(message);
                }
                else
                {
                    Debug.LogWarning($"[JSPluginTools] Module '{message.moduleId}' does not implement IJSMessageHandler");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginTools] Error processing message from JavaScript: {ex.Message}");
            }
        }
    }
}
