using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Communication module for JSPluginTools.
    /// Handles messaging between Unity and JavaScript environments.
    /// </summary>
    public static class JSPluginCommunication
    {
        #region Native Methods

        [DllImport("__Internal")]
        private static extern bool JSPluginSendMessage(string objectId, string methodName, string parameter);

        [DllImport("__Internal")]
        private static extern bool JSPluginBroadcastMessage(string eventName, string parameter);

        [DllImport("__Internal")]
        private static extern int JSPluginRegisterMessageHandler(string handlerId, string methodName);

        [DllImport("__Internal")]
        private static extern int JSPluginInitializeCommunication();
        
        [DllImport("__Internal")]
        private static extern int JSPluginShutdownCommunication();

        #endregion

        private static bool isInitialized = false;
        
        /// <summary>
        /// Initializes the communication module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            if (isInitialized)
                return true;
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            isInitialized = JSPluginInitializeCommunication() == 1;
            #else
            JSPluginErrorHandling.LogEvent("JSPluginCommunication", "Initialized in stub mode (non-WebGL environment)", JSPluginErrorHandling.ErrorSeverity.Info);
            isInitialized = true;
            #endif
            
            if (isInitialized)
            {
                JSPluginErrorHandling.LogEvent("JSPluginCommunication", "Communication module initialized successfully", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            
            return isInitialized;
        }
        
        /// <summary>
        /// Shuts down the communication module and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            try
            {
                // Clear all broadcast listeners
                BroadcastListeners.Clear();
                
                // Clear the message queue
                MessageQueue.Clear();
                IsConnected = false;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginShutdownCommunication();
                #endif
                
                isInitialized = false;
                JSPluginErrorHandling.LogEvent("JSPluginCommunication", "Communication module shut down", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginCommunication", "Shutdown", ex, JSPluginErrorHandling.ErrorSeverity.Error);
            }
        }

        #region Message Handling

        /// <summary>
        /// Represents a two-way communication channel with JavaScript
        /// </summary>
        public class JSChannel
        {
            /// <summary>Unique identifier for this channel</summary>
            public string ChannelId { get; }
            
            /// <summary>Associated game object for Unity callbacks</summary>
            public GameObject GameObject { get; }
            
            private readonly Dictionary<string, string> registeredHandlers = new();
            
            /// <summary>
            /// Creates a new communication channel
            /// </summary>
            /// <param name="channelId">Unique identifier for this channel</param>
            /// <param name="gameObject">GameObject that will receive messages</param>
            public JSChannel(string channelId, GameObject gameObject)
            {
                ChannelId = channelId;
                GameObject = gameObject;
                
                // Register with core system
                JSPluginCore.RegisterObject(channelId, gameObject);
            }
            
            /// <summary>
            /// Sends a message to JavaScript
            /// </summary>
            /// <param name="methodName">Name of the JavaScript method to call</param>
            /// <param name="parameter">Optional parameter to pass</param>
            /// <returns>True if the message was sent successfully</returns>
            public bool Send(string methodName, string parameter = null)
            {
                try
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    return JSPluginSendMessage(ChannelId, methodName, parameter);
                    #else
                    Debug.Log($"[JSPluginCommunication] Would send message to {ChannelId}.{methodName}: {parameter}");
                    return true;
                    #endif
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException(
                        "JSPluginCommunication", 
                        $"Sending message to {methodName}",
                        ex,
                        JSPluginErrorHandling.ErrorSeverity.Error,
                        ChannelId);
                    return false;
                }
            }
            
            /// <summary>
            /// Sends a structured object to JavaScript by serializing it to JSON
            /// </summary>
            /// <typeparam name="T">Type of object to serialize</typeparam>
            /// <param name="methodName">Name of the JavaScript method to call</param>
            /// <param name="data">Object to serialize</param>
            /// <returns>True if the message was sent successfully</returns>
            public bool SendJson<T>(string methodName, T data)
            {
                try
                {
                    string json = JsonUtility.ToJson(data);
                    return Send(methodName, json);
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException(
                        "JSPluginCommunication", 
                        $"Serializing data for {methodName}",
                        ex,
                        JSPluginErrorHandling.ErrorSeverity.Error,
                        ChannelId);
                    return false;
                }
            }
            
            /// <summary>
            /// Registers a message handler for receiving messages from JavaScript
            /// </summary>
            /// <param name="messageType">Type of message to handle</param>
            /// <param name="methodName">Name of the method on the GameObject to call</param>
            /// <returns>This channel for method chaining</returns>
            public JSChannel RegisterHandler(string messageType, string methodName)
            {
                try
                {
                    registeredHandlers[messageType] = methodName;
                    
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    JSPluginRegisterMessageHandler(ChannelId + ":" + messageType, methodName);
                    #endif
                    
                    return this;
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException(
                        "JSPluginCommunication", 
                        $"Registering handler for {messageType}",
                        ex,
                        JSPluginErrorHandling.ErrorSeverity.Error,
                        ChannelId);
                    return this;
                }
            }
        }

        #endregion

        #region Broadcast System

        private static readonly Dictionary<string, List<Action<string>>> BroadcastListeners = new();
        
        /// <summary>
        /// Sends a broadcast message to all registered JavaScript listeners
        /// </summary>
        /// <param name="eventName">Name of the event to broadcast</param>
        /// <param name="data">Optional data to include</param>
        /// <returns>True if the broadcast was successful</returns>
        public static bool Broadcast(string eventName, string data = null)
        {
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginBroadcastMessage(eventName, data);
                #else
                Debug.Log($"[JSPluginCommunication] Would broadcast event '{eventName}' with data: {data}");
                
                // Notify local listeners even in non-WebGL environments
                if (BroadcastListeners.TryGetValue(eventName, out var listeners))
                {
                    foreach (var listener in listeners)
                    {
                        try
                        {
                            listener?.Invoke(data);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in broadcast listener: {ex.Message}");
                        }
                    }
                }
                
                return true;
                #endif
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException(
                    "JSPluginCommunication", 
                    $"Broadcasting event {eventName}",
                    ex,
                    JSPluginErrorHandling.ErrorSeverity.Error);
                return false;
            }
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
            try
            {
                string json = JsonUtility.ToJson(data);
                return Broadcast(eventName, json);
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException(
                    "JSPluginCommunication", 
                    $"Serializing broadcast data for {eventName}",
                    ex,
                    JSPluginErrorHandling.ErrorSeverity.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Subscribes to broadcasts of a specific event
        /// </summary>
        /// <param name="eventName">Name of the event to listen for</param>
        /// <param name="listener">Callback to invoke when the event is broadcast</param>
        /// <returns>Action that can be called to unsubscribe</returns>
        public static Action SubscribeToBroadcast(string eventName, Action<string> listener)
        {
            if (!BroadcastListeners.TryGetValue(eventName, out var listeners))
            {
                listeners = new List<Action<string>>();
                BroadcastListeners[eventName] = listeners;
            }
            
            listeners.Add(listener);
            
            // Return an unsubscribe function
            return () => {
                if (BroadcastListeners.TryGetValue(eventName, out var l))
                {
                    l.Remove(listener);
                    if (l.Count == 0)
                    {
                        BroadcastListeners.Remove(eventName);
                    }
                }
            };
        }

        #endregion

        #region Message Queue

        // Queue for messages that need to be delivered when the connection is established
        private static readonly Queue<PendingMessage> MessageQueue = new();
        private static bool IsConnected = false;
        
        private class PendingMessage
        {
            public string ObjectId { get; set; }
            public string MethodName { get; set; }
            public string Parameter { get; set; }
            public bool IsBroadcast { get; set; }
        }
        
        /// <summary>
        /// Sets the connection status and processes any queued messages
        /// </summary>
        /// <param name="connected">Whether the connection to JavaScript is established</param>
        public static void SetConnectionStatus(bool connected)
        {
            IsConnected = connected;
            
            if (connected)
            {
                ProcessMessageQueue();
            }
        }
        
        /// <summary>
        /// Queues a message to be sent when the connection is established
        /// </summary>
        /// <param name="objectId">Target object ID</param>
        /// <param name="methodName">Method to call</param>
        /// <param name="parameter">Parameter data</param>
        /// <param name="isBroadcast">Whether this is a broadcast message</param>
        public static void QueueMessage(string objectId, string methodName, string parameter, bool isBroadcast = false)
        {
            MessageQueue.Enqueue(new PendingMessage {
                ObjectId = objectId,
                MethodName = methodName,
                Parameter = parameter,
                IsBroadcast = isBroadcast
            });
            
            if (IsConnected)
            {
                ProcessMessageQueue();
            }
        }
        
        private static void ProcessMessageQueue()
        {
            while (MessageQueue.Count > 0 && IsConnected)
            {
                var message = MessageQueue.Dequeue();
                
                try
                {
                    if (message.IsBroadcast)
                    {
                        Broadcast(message.MethodName, message.Parameter);
                    }
                    else
                    {
                        JSPluginSendMessage(message.ObjectId, message.MethodName, message.Parameter);
                    }
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException(
                        "JSPluginCommunication", 
                        "Processing queued message",
                        ex,
                        JSPluginErrorHandling.ErrorSeverity.Warning);
                }
            }
        }

        #endregion
    }
}
