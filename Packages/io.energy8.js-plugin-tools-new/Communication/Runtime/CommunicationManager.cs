using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using UnityEngine;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// Реализация менеджера коммуникаций для типизированного обмена данными между Unity и JavaScript
    /// </summary>
    public class CommunicationManager : ICommunicationManager
    {
        private IPluginCore _core;
        private IMessageBus _messageBus;
        private bool _isInitialized = false;
        private readonly Dictionary<string, List<Delegate>> _handlers = new Dictionary<string, List<Delegate>>();
        private readonly object _syncRoot = new object();

        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc/>
        public event Action OnInitialized;

        private const string COMM_MESSAGE_PREFIX = "comm.";

        /// <inheritdoc/>
        public void Initialize(IPluginCore core)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("JSPluginTools [Communication]: Already initialized");
                return;
            }

            _core = core ?? throw new ArgumentNullException(nameof(core));
            
            if (_core is PluginCore pluginCore)
            {
                _messageBus = pluginCore.MessageBus;
            }
            else
            {
                throw new InvalidOperationException("JSPluginTools [Communication]: Core implementation does not provide access to MessageBus");
            }

            _isInitialized = true;
            Debug.Log("JSPluginTools [Communication]: Module initialized");
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public void Send<T>(string channel, T data)
        {
            CheckInitialized();

            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            string messageType = GetMessageType(channel);
            _messageBus.SendMessage(messageType, data);
            
            Debug.Log($"JSPluginTools [Communication]: Sent message on channel '{channel}'");
        }

        /// <inheritdoc/>
        public async Task<TResponse> SendAsync<TRequest, TResponse>(string channel, TRequest data)
        {
            CheckInitialized();

            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            string messageType = GetMessageType(channel);
            
            var tcs = new TaskCompletionSource<TResponse>();
            
            try
            {
                _messageBus.SendMessageWithResponse<TRequest, TResponse>(
                    messageType,
                    data,
                    response => tcs.SetResult(response));
                    
                Debug.Log($"JSPluginTools [Communication]: Sent async message on channel '{channel}'");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public void RegisterHandler<T>(string channel, Action<T> handler)
        {
            CheckInitialized();

            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            string messageType = GetMessageType(channel);
            
            lock (_syncRoot)
            {
                if (!_handlers.TryGetValue(channel, out List<Delegate> channelHandlers))
                {
                    channelHandlers = new List<Delegate>();
                    _handlers[channel] = channelHandlers;
                }
                
                channelHandlers.Add(handler);
            }
            
            _messageBus.RegisterMessageHandler<T>(messageType, data =>
            {
                Debug.Log($"JSPluginTools [Communication]: Received message on channel '{channel}'");
                handler(data);
            });
            
            Debug.Log($"JSPluginTools [Communication]: Registered handler for channel '{channel}'");
        }

        /// <inheritdoc/>
        public void UnregisterHandler(string channel)
        {
            CheckInitialized();

            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel cannot be null or empty", nameof(channel));
            }

            string messageType = GetMessageType(channel);
            
            lock (_syncRoot)
            {
                if (_handlers.ContainsKey(channel))
                {
                    _handlers.Remove(channel);
                }
            }
            
            _messageBus.UnregisterMessageHandler(messageType);
            
            Debug.Log($"JSPluginTools [Communication]: Unregistered handlers for channel '{channel}'");
        }

        private string GetMessageType(string channel)
        {
            return $"{COMM_MESSAGE_PREFIX}{channel}";
        }

        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("JSPluginTools [Communication]: Module not initialized. Call Initialize() first");
            }
        }
    }
}