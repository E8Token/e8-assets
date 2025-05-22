using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Core manager for JavaScript plugins.
    /// Handles plugin registration and communication between JavaScript and C#.
    /// </summary>
    public class JSPluginManager : MonoBehaviour
    {
        #region Singleton

        private static JSPluginManager _instance;

        /// <summary>
        /// Gets the singleton instance of the plugin manager.
        /// </summary>
        public static JSPluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Energy8.JSPluginTools.Manager");
                    _instance = go.AddComponent<JSPluginManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        #endregion

        private readonly Dictionary<string, JSPluginBase> _plugins = new Dictionary<string, JSPluginBase>();
        private readonly Dictionary<string, JSPluginModule> _modules = new Dictionary<string, JSPluginModule>();
        private bool _isDebugMode = false;

        /// <summary>
        /// Enables or disables debug mode.
        /// </summary>
        public bool DebugMode
        {
            get => _isDebugMode;
            set
            {
                _isDebugMode = value;
                SetDebugMode(_isDebugMode);
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
        }

        /// <summary>
        /// Registers a plugin module.
        /// </summary>
        /// <param name="module">The module to register.</param>
        public void RegisterModule(JSPluginModule module)
        {
            if (module == null)
            {
                Debug.LogError("Cannot register null module");
                return;
            }

            string moduleName = module.ModuleName;
            if (string.IsNullOrEmpty(moduleName))
            {
                Debug.LogError("Module name cannot be null or empty");
                return;
            }

            if (_modules.ContainsKey(moduleName))
            {
                Debug.LogWarning($"Module {moduleName} is already registered");
                return;
            }

            _modules[moduleName] = module;
            module.InitializeModule();
            
            if (_isDebugMode)
            {
                Debug.Log($"Registered module: {moduleName}");
            }
        }

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginType">The type of the plugin to register.</param>
        /// <returns>The plugin instance or null if registration failed.</returns>
        public JSPluginBase RegisterPlugin(Type pluginType)
        {
            if (pluginType == null || !typeof(JSPluginBase).IsAssignableFrom(pluginType))
            {
                Debug.LogError($"Invalid plugin type: {pluginType}");
                return null;
            }

            // Create a new GameObject for the plugin
            var pluginObject = new GameObject($"Energy8.JSPlugin.{pluginType.Name}");
            pluginObject.transform.SetParent(transform);
            
            // Add the plugin component
            var plugin = (JSPluginBase)pluginObject.AddComponent(pluginType);
            
            if (string.IsNullOrEmpty(plugin.PluginName))
            {
                Debug.LogError($"Plugin {pluginType.Name} has null or empty PluginName");
                Destroy(pluginObject);
                return null;
            }
            
            if (_plugins.ContainsKey(plugin.PluginName))
            {
                Debug.LogWarning($"Plugin {plugin.PluginName} is already registered");
                Destroy(pluginObject);
                return null;
            }
            
            _plugins[plugin.PluginName] = plugin;
            
            // Initialize the plugin
            plugin.Initialize();
            
            // Register the plugin with JavaScript
            RegisterPluginWithJS(plugin.PluginName);
            
            if (_isDebugMode)
            {
                Debug.Log($"Registered plugin: {plugin.PluginName}");
            }
            
            return plugin;
        }

        /// <summary>
        /// Gets a registered plugin by name.
        /// </summary>
        /// <param name="pluginName">The name of the plugin to get.</param>
        /// <returns>The plugin instance or null if the plugin is not registered.</returns>
        public JSPluginBase GetPlugin(string pluginName)
        {
            if (_plugins.TryGetValue(pluginName, out var plugin))
            {
                return plugin;
            }
            
            return null;
        }

        /// <summary>
        /// Dispatches a message from JavaScript to the appropriate plugin.
        /// </summary>
        /// <param name="pluginName">The name of the target plugin.</param>
        /// <param name="method">The method to call.</param>
        /// <param name="payload">The JSON payload.</param>
        public void DispatchMessage(string pluginName, string method, string payload)
        {
            if (_isDebugMode)
            {
                Debug.Log($"Dispatching message to {pluginName}.{method}: {payload}");
            }
            
            if (!_plugins.TryGetValue(pluginName, out var plugin))
            {
                Debug.LogWarning($"Plugin not found: {pluginName}");
                return;
            }
            
            try
            {
                plugin.OnMessage(method, payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in plugin {pluginName}.{method}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a message from C# to JavaScript.
        /// </summary>
        /// <param name="pluginName">The name of the plugin sending the message.</param>
        /// <param name="method">The method to call.</param>
        /// <param name="payload">The JSON payload.</param>
        public void SendMessage(string pluginName, string method, string payload)
        {
            if (_isDebugMode)
            {
                Debug.Log($"Sending message from {pluginName}.{method}: {payload}");
            }
            
            SendMessageToJS(pluginName, method, payload);
        }

        #region JavaScript Interop Functions

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void RegisterPluginWithJS(string pluginName);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SendMessageToJS(string pluginName, string method, string payload);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SetDebugMode(bool enabled);

        #endregion
    }
}
