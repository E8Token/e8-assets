using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// Provides high-level methods for communicating between Unity and JavaScript
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Gets the underlying message bus used by this service
        /// </summary>
        IMessageBus MessageBus { get; }

        /// <summary>
        /// Sends raw string data to JavaScript
        /// </summary>
        /// <param name="channel">Communication channel name</param>
        /// <param name="data">String data to send</param>
        void Send(string channel, string data);

        /// <summary>
        /// Sends an object to JavaScript as JSON
        /// </summary>
        /// <typeparam name="T">Type of object to send</typeparam>
        /// <param name="channel">Communication channel name</param>
        /// <param name="data">Object to serialize and send</param>
        void Send<T>(string channel, T data);

        /// <summary>
        /// Sends data and waits for a response
        /// </summary>
        /// <typeparam name="TResponse">Type of response expected</typeparam>
        /// <param name="channel">Communication channel name</param>
        /// <param name="data">String data to send</param>
        /// <param name="timeout">Optional timeout in milliseconds</param>
        /// <returns>Task that completes when response is received</returns>
        Task<TResponse> SendWithResponseAsync<TResponse>(string channel, string data, int timeout = 5000);

        /// <summary>
        /// Sends an object and waits for a response
        /// </summary>
        /// <typeparam name="TRequest">Type of request to send</typeparam>
        /// <typeparam name="TResponse">Type of response expected</typeparam>
        /// <param name="channel">Communication channel name</param>
        /// <param name="data">Object to serialize and send</param>
        /// <param name="timeout">Optional timeout in milliseconds</param>
        /// <returns>Task that completes when response is received</returns>
        Task<TResponse> SendWithResponseAsync<TRequest, TResponse>(string channel, TRequest data, int timeout = 5000);

        /// <summary>
        /// Subscribes to a channel to receive string data
        /// </summary>
        /// <param name="channel">Channel name to subscribe to</param>
        /// <param name="handler">Handler function for received data</param>
        void Subscribe(string channel, Action<string> handler);

        /// <summary>
        /// Subscribes to a channel to receive typed objects
        /// </summary>
        /// <typeparam name="T">Type to deserialize received data to</typeparam>
        /// <param name="channel">Channel name to subscribe to</param>
        /// <param name="handler">Handler function for deserialized objects</param>
        void Subscribe<T>(string channel, Action<T> handler);

        /// <summary>
        /// Unsubscribes from a channel
        /// </summary>
        /// <param name="channel">Channel name to unsubscribe from</param>
        void Unsubscribe(string channel);
    }
}