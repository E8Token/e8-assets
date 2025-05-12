using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Newtonsoft.Json;

namespace Energy8.JSPluginTools.Core.Implementation
{
    /// <summary>
    /// Implementation of the IMessageBus interface for handling messages between Unity and JavaScript
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly IMemoryManager _memoryManager;
        private readonly Dictionary<string, List<object>> _messageHandlers = new Dictionary<string, List<object>>();
        private readonly Dictionary<string, object> _responseCallbacks = new Dictionary<string, object>();
        private int _callbackIdCounter = 0;
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Creates a new MessageBus instance
        /// </summary>
        /// <param name="memoryManager">Memory manager for handling string passing to JavaScript</param>
        public MessageBus(IMemoryManager memoryManager)
        {
            _memoryManager = memoryManager ?? throw new ArgumentNullException(nameof(memoryManager));
        }

        /// <inheritdoc/>
        public void SendMessage(string messageType, string payload)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                throw new ArgumentException("Message type cannot be null or empty", nameof(messageType));
            }

            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                SendMessageToJS(messageType, payload ?? string.Empty);
#else
                Debug.Log($"JSPluginTools [MessageBus]: Would send message '{messageType}' with payload: {payload}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [MessageBus]: Error sending message '{messageType}': {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void SendMessage<T>(string messageType, T payload)
        {
            string jsonPayload = JsonConvert.SerializeObject(payload);
            SendMessage(messageType, jsonPayload);
        }

        /// <inheritdoc/>
        public void SendMessageWithResponse<TRequest, TResponse>(
            string messageType, 
            TRequest payload, 
            Action<TResponse> callback)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                throw new ArgumentException("Message type cannot be null or empty", nameof(messageType));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            string callbackId = GenerateCallbackId();
            lock (_syncRoot)
            {
                _responseCallbacks[callbackId] = callback;
            }

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(new
                {
                    data = payload,
                    callbackId = callbackId
                });

#if UNITY_WEBGL && !UNITY_EDITOR
                SendMessageWithResponseToJS(messageType, jsonPayload, callbackId);
#else
                Debug.Log($"JSPluginTools [MessageBus]: Would send message with response '{messageType}' with callback ID: {callbackId}");
                // For testing in non-WebGL context, simulate response
                TResponse mockResponse = default;
                callback(mockResponse);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [MessageBus]: Error sending message with response '{messageType}': {ex.Message}");
                
                lock (_syncRoot)
                {
                    _responseCallbacks.Remove(callbackId);
                }
                
                throw;
            }
        }

        /// <inheritdoc/>
        public void RegisterMessageHandler(string messageType, Action<string> handler)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                throw new ArgumentException("Message type cannot be null or empty", nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            lock (_syncRoot)
            {
                if (!_messageHandlers.ContainsKey(messageType))
                {
                    _messageHandlers[messageType] = new List<object>();
                }
                
                _messageHandlers[messageType].Add(handler);
            }

            Debug.Log($"JSPluginTools [MessageBus]: Registered handler for message type '{messageType}'");
        }

        /// <inheritdoc/>
        public void RegisterMessageHandler<T>(string messageType, Action<T> handler)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                throw new ArgumentException("Message type cannot be null or empty", nameof(messageType));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Action<string> jsonHandler = jsonPayload =>
            {
                try
                {
                    T typedPayload = JsonConvert.DeserializeObject<T>(jsonPayload);
                    handler(typedPayload);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSPluginTools [MessageBus]: Error deserializing payload for message '{messageType}': {ex.Message}");
                }
            };

            RegisterMessageHandler(messageType, jsonHandler);
        }

        /// <inheritdoc/>
        public void UnregisterMessageHandler(string messageType)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                throw new ArgumentException("Message type cannot be null or empty", nameof(messageType));
            }

            lock (_syncRoot)
            {
                if (_messageHandlers.ContainsKey(messageType))
                {
                    _messageHandlers.Remove(messageType);
                    Debug.Log($"JSPluginTools [MessageBus]: Unregistered handlers for message type '{messageType}'");
                }
            }
        }

        /// <summary>
        /// Handles messages received from JavaScript
        /// </summary>
        /// <param name="messageType">Type of message received</param>
        /// <param name="jsonPayload">JSON payload received</param>
        public void HandleMessageFromJS(string messageType, string jsonPayload)
        {
            if (string.IsNullOrEmpty(messageType))
            {
                Debug.LogWarning("JSPluginTools [MessageBus]: Received message with empty message type");
                return;
            }

            List<object> handlers = null;

            lock (_syncRoot)
            {
                if (_messageHandlers.TryGetValue(messageType, out var messageHandlers))
                {
                    handlers = new List<object>(messageHandlers);
                }
            }

            if (handlers != null && handlers.Count > 0)
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        if (handler is Action<string> stringHandler)
                        {
                            stringHandler(jsonPayload);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSPluginTools [MessageBus]: Error in message handler for '{messageType}': {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.Log($"JSPluginTools [MessageBus]: No handlers registered for message type '{messageType}'");
            }
        }

        /// <summary>
        /// Handles responses from JavaScript for messages sent with callbacks
        /// </summary>
        /// <param name="callbackId">The callback ID to identify the original request</param>
        /// <param name="jsonResponse">JSON response payload</param>
        public void HandleResponseFromJS(string callbackId, string jsonResponse)
        {
            if (string.IsNullOrEmpty(callbackId))
            {
                Debug.LogWarning("JSPluginTools [MessageBus]: Received response with empty callback ID");
                return;
            }

            object callback = null;

            lock (_syncRoot)
            {
                if (_responseCallbacks.TryGetValue(callbackId, out callback))
                {
                    _responseCallbacks.Remove(callbackId);
                }
            }

            if (callback != null)
            {
                try
                {
                    Type callbackType = callback.GetType();
                    Type responseType = callbackType.GetGenericArguments()[0];
                    
                    object response = JsonConvert.DeserializeObject(jsonResponse, responseType);
                    
                    callbackType.GetMethod("Invoke").Invoke(callback, new[] { response });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSPluginTools [MessageBus]: Error processing response for callback ID '{callbackId}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"JSPluginTools [MessageBus]: No callback registered for callback ID '{callbackId}'");
            }
        }
        
        private string GenerateCallbackId()
        {
            return $"callback_{++_callbackIdCounter}_{DateTime.UtcNow.Ticks}";
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SendMessageToJS(string messageType, string payload);

        [DllImport("__Internal")]
        private static extern void SendMessageWithResponseToJS(string messageType, string payload, string callbackId);
#else
        // Stub implementations for non-WebGL platforms
        private static void SendMessageToJS(string messageType, string payload) { }
        private static void SendMessageWithResponseToJS(string messageType, string payload, string callbackId) { }
#endif
    }
}