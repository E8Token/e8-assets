using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Base class for plugin modules. Implements core functionality required by the IPluginModule interface.
    /// </summary>
    public abstract class PluginModuleBase : IPluginModule
    {
        private bool _isInitialized;
        
        /// <summary>
        /// Gets the unique identifier for this plugin module.
        /// </summary>
        public abstract string ModuleId { get; }
        
        /// <summary>
        /// Gets a value indicating whether this module is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initializes this module.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        public bool Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"[JSPluginTools] Module '{ModuleId}' is already initialized");
                return true;
            }
            
            try
            {
                bool success = OnInitialize();
                _isInitialized = success;
                
                if (success)
                {
                    Debug.Log($"[JSPluginTools] Module '{ModuleId}' initialized successfully");
                    
                    // Notify JavaScript that the module is initialized
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    ExternalCommunicator.CallJS($"UnityWebPlugin.Core.notifyModuleInitialized", ModuleId);
                    #endif
                }
                else
                {
                    Debug.LogError($"[JSPluginTools] Failed to initialize module '{ModuleId}'");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginTools] Error initializing module '{ModuleId}': {ex.Message}");
                _isInitialized = false;
                return false;
            }
        }
        
        /// <summary>
        /// Shuts down this module, releasing any resources it holds.
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[JSPluginTools] Module '{ModuleId}' is not initialized");
                return;
            }
            
            try
            {
                OnShutdown();
                _isInitialized = false;
                Debug.Log($"[JSPluginTools] Module '{ModuleId}' shut down successfully");
                
                // Notify JavaScript that the module is shut down
                #if UNITY_WEBGL && !UNITY_EDITOR
                ExternalCommunicator.CallJS($"UnityWebPlugin.Core.notifyModuleShutdown", ModuleId);
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginTools] Error shutting down module '{ModuleId}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Called when the module is being initialized.
        /// Override this method to provide module-specific initialization logic.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        protected abstract bool OnInitialize();
        
        /// <summary>
        /// Called when the module is being shut down.
        /// Override this method to provide module-specific shutdown logic.
        /// </summary>
        protected abstract void OnShutdown();
        
        /// <summary>
        /// Sends a message to JavaScript.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="data">Optional data payload as a JSON string.</param>
        /// <param name="callbackId">Optional callback ID for asynchronous operations.</param>
        protected void SendMessageToJS(string action, string data = null, string callbackId = null)
        {
            ExternalCommunicator.SendMessageToJS(ModuleId, action, data, callbackId);
        }
    }
    
    /// <summary>
    /// Base class for plugin modules that can handle messages from JavaScript.
    /// </summary>
    public abstract class JSMessageHandlerModuleBase : PluginModuleBase, IJSMessageHandler
    {
        private readonly Dictionary<string, Action<string>> _actionHandlers = new Dictionary<string, Action<string>>();
        
        /// <summary>
        /// Handles a message received from JavaScript.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        public void HandleJSMessage(JSMessage message)
        {
            if (string.IsNullOrEmpty(message.action))
            {
                Debug.LogError($"[JSPluginTools] Received message with no action for module '{ModuleId}'");
                return;
            }
            
            if (_actionHandlers.TryGetValue(message.action, out var handler))
            {
                try
                {
                    handler.Invoke(message.data);
                    
                    // If this message expects a callback, send a success response
                    if (!string.IsNullOrEmpty(message.callbackId))
                    {
                        SendCallbackResponse(message.callbackId, true, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginTools] Error handling action '{message.action}' for module '{ModuleId}': {ex.Message}");
                    
                    // If this message expects a callback, send an error response
                    if (!string.IsNullOrEmpty(message.callbackId))
                    {
                        SendCallbackResponse(message.callbackId, false, ex.Message);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[JSPluginTools] Unhandled action '{message.action}' for module '{ModuleId}'");
                
                // If this message expects a callback, send an error response
                if (!string.IsNullOrEmpty(message.callbackId))
                {
                    SendCallbackResponse(message.callbackId, false, $"Unhandled action: {message.action}");
                }
            }
        }
        
        /// <summary>
        /// Registers a handler for a specific action message.
        /// </summary>
        /// <param name="action">The action to handle.</param>
        /// <param name="handler">The handler to call when the action is received.</param>
        protected void RegisterActionHandler(string action, Action<string> handler)
        {
            if (string.IsNullOrEmpty(action))
            {
                Debug.LogError($"[JSPluginTools] Cannot register handler for empty action in module '{ModuleId}'");
                return;
            }
            
            if (handler == null)
            {
                Debug.LogError($"[JSPluginTools] Cannot register null handler for action '{action}' in module '{ModuleId}'");
                return;
            }
            
            _actionHandlers[action] = handler;
        }
        
        /// <summary>
        /// Sends a callback response to JavaScript.
        /// </summary>
        /// <param name="callbackId">The ID of the callback to respond to.</param>
        /// <param name="success">Whether the operation was successful.</param>
        /// <param name="errorMessage">An optional error message if the operation failed.</param>
        /// <param name="data">Optional data to send with the response.</param>
        protected void SendCallbackResponse(string callbackId, bool success, string errorMessage = null, string data = null)
        {
            var response = new JSCallbackResponse
            {
                success = success,
                errorMessage = errorMessage,
                data = data
            };
            
            string responseJson = JsonUtility.ToJson(response);
            ExternalCommunicator.CallJS("UnityWebPlugin.Core.invokeCallback", callbackId, responseJson);
        }
        
        /// <summary>
        /// Represents a response to a JavaScript callback.
        /// </summary>
        [Serializable]
        private class JSCallbackResponse
        {
            /// <summary>
            /// Gets or sets a value indicating whether the operation was successful.
            /// </summary>
            public bool success;
            
            /// <summary>
            /// Gets or sets an error message if the operation failed.
            /// </summary>
            public string errorMessage;
            
            /// <summary>
            /// Gets or sets optional data to send with the response.
            /// </summary>
            public string data;
        }
    }
}
