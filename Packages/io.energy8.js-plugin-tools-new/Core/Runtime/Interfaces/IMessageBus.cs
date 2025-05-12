using System;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Provides methods for sending and receiving messages between Unity and JavaScript
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Sends a message to JavaScript with string payload
        /// </summary>
        /// <param name="messageType">Type of the message</param>
        /// <param name="payload">String payload to send</param>
        void SendMessage(string messageType, string payload);

        /// <summary>
        /// Sends a message to JavaScript with an object payload that will be serialized to JSON
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="messageType">Type of the message</param>
        /// <param name="payload">Object payload to serialize and send</param>
        void SendMessage<T>(string messageType, T payload);

        /// <summary>
        /// Sends a message to JavaScript and expects a response
        /// </summary>
        /// <typeparam name="TRequest">Type of the request object</typeparam>
        /// <typeparam name="TResponse">Type of the expected response object</typeparam>
        /// <param name="messageType">Type of the message</param>
        /// <param name="payload">Request payload</param>
        /// <param name="callback">Callback to handle the response</param>
        void SendMessageWithResponse<TRequest, TResponse>(
            string messageType, 
            TRequest payload, 
            Action<TResponse> callback);

        /// <summary>
        /// Registers a handler for receiving messages from JavaScript
        /// </summary>
        /// <param name="messageType">Type of message to handle</param>
        /// <param name="handler">Handler function receiving string payload</param>
        void RegisterMessageHandler(string messageType, Action<string> handler);

        /// <summary>
        /// Registers a handler for receiving messages from JavaScript with JSON deserialization
        /// </summary>
        /// <typeparam name="T">Type to deserialize the payload into</typeparam>
        /// <param name="messageType">Type of message to handle</param>
        /// <param name="handler">Handler function receiving deserialized object</param>
        void RegisterMessageHandler<T>(string messageType, Action<T> handler);

        /// <summary>
        /// Unregisters a previously registered message handler
        /// </summary>
        /// <param name="messageType">Type of message to unregister handlers for</param>
        void UnregisterMessageHandler(string messageType);
    }
}