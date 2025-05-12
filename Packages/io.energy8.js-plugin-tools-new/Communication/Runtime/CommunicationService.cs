using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;
using UnityEngine;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// Implementation of the ICommunicationService interface
    /// </summary>
    public class CommunicationService : ICommunicationService
    {
        private readonly Dictionary<string, TaskCompletionSource<object>> _pendingResponses = 
            new Dictionary<string, TaskCompletionSource<object>>();
        private readonly Dictionary<string, CancellationTokenSource> _requestTimeouts = 
            new Dictionary<string, CancellationTokenSource>();
        
        /// <inheritdoc/>
        public IMessageBus MessageBus { get; }

        /// <summary>
        /// Creates a new instance of CommunicationService
        /// </summary>
        /// <param name="messageBus">Message bus to use for communication</param>
        public CommunicationService(IMessageBus messageBus)
        {
            MessageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <inheritdoc/>
        public void Send(string channel, string data)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            MessageBus.SendMessage(channel, data ?? string.Empty);
        }

        /// <inheritdoc/>
        public void Send<T>(string channel, T data)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            MessageBus.SendMessage(channel, data);
        }

        /// <inheritdoc/>
        public Task<TResponse> SendWithResponseAsync<TResponse>(string channel, string data, int timeout = 5000)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout must be greater than zero", nameof(timeout));
            }

            var tcs = new TaskCompletionSource<object>();
            string requestId = Guid.NewGuid().ToString();
            
            var payload = new
            {
                data = data,
                requestId = requestId
            };

            lock (_pendingResponses)
            {
                _pendingResponses[requestId] = tcs;
            }

            // Set up timeout
            var cts = new CancellationTokenSource();
            _requestTimeouts[requestId] = cts;
            
            cts.Token.Register(() =>
            {
                lock (_pendingResponses)
                {
                    if (_pendingResponses.ContainsKey(requestId))
                    {
                        _pendingResponses.Remove(requestId);
                        tcs.TrySetException(new TimeoutException($"Request to channel '{channel}' timed out after {timeout}ms"));
                    }
                }
                
                cts.Dispose();
                _requestTimeouts.Remove(requestId);
            }, useSynchronizationContext: false);

            // Set timeout timer
            cts.CancelAfter(timeout);

            // Subscribe to response
            MessageBus.RegisterMessageHandler<ResponseWrapper>($"{channel}_response", responseWrapper =>
            {
                if (responseWrapper.RequestId == requestId)
                {
                    try
                    {
                        lock (_pendingResponses)
                        {
                            if (_pendingResponses.TryGetValue(requestId, out var pendingTcs))
                            {
                                _pendingResponses.Remove(requestId);
                                
                                if (_requestTimeouts.TryGetValue(requestId, out var tokenSource))
                                {
                                    tokenSource.Cancel();
                                    tokenSource.Dispose();
                                    _requestTimeouts.Remove(requestId);
                                }
                                
                                pendingTcs.TrySetResult(responseWrapper.Response);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"JSPluginTools [CommunicationService]: Error processing response: {ex.Message}");
                        tcs.TrySetException(ex);
                    }
                    
                    // Unregister the one-time handler
                    MessageBus.UnregisterMessageHandler($"{channel}_response");
                }
            });

            // Send the message
            try
            {
                MessageBus.SendMessage(channel, payload);
            }
            catch (Exception ex)
            {
                lock (_pendingResponses)
                {
                    _pendingResponses.Remove(requestId);
                }
                
                if (_requestTimeouts.TryGetValue(requestId, out var tokenSource))
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    _requestTimeouts.Remove(requestId);
                }
                
                MessageBus.UnregisterMessageHandler($"{channel}_response");
                tcs.TrySetException(ex);
            }

            return tcs.Task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception.InnerException;
                }
                
                if (t.Result == null)
                {
                    return default;
                }
                
                try
                {
                    return (TResponse)t.Result;
                }
                catch (InvalidCastException ex)
                {
                    throw new InvalidOperationException($"Cannot convert response to type {typeof(TResponse).Name}", ex);
                }
            });
        }

        /// <inheritdoc/>
        public Task<TResponse> SendWithResponseAsync<TRequest, TResponse>(string channel, TRequest data, int timeout = 5000)
        {
            return SendWithResponseAsync<TResponse>(channel, data == null ? null : JsonUtility.ToJson(data), timeout);
        }

        /// <inheritdoc/>
        public void Subscribe(string channel, Action<string> handler)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            MessageBus.RegisterMessageHandler(channel, handler);
        }

        /// <inheritdoc/>
        public void Subscribe<T>(string channel, Action<T> handler)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            MessageBus.RegisterMessageHandler(channel, handler);
        }

        /// <inheritdoc/>
        public void Unsubscribe(string channel)
        {
            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
            }

            MessageBus.UnregisterMessageHandler(channel);
        }

        /// <summary>
        /// Wrapper for response messages with request ID
        /// </summary>
        private class ResponseWrapper
        {
            public string RequestId { get; set; }
            public object Response { get; set; }
        }
    }
}