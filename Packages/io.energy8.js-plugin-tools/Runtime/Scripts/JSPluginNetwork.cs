using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Network module for JSPluginTools.
    /// Provides methods for HTTP requests and WebSocket communication via JavaScript.
    /// </summary>
    public static class JSPluginNetwork
    {
        #region Native Methods
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkInitialize();
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkSendRequest(string requestId, string url, string method, string headers, string body, string objectId, string callbackMethod);
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkCancelRequest(string requestId);
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkCreateWebSocket(string socketId, string url, string protocols, string objectId, string messageMethod, string openMethod, string closeMethod, string errorMethod);
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkSendWebSocketMessage(string socketId, string message);
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkCloseWebSocket(string socketId, int code, string reason);
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkShutdown();
        
        [DllImport("__Internal")]
        private static extern int JSPluginNetworkSetRequestTimeout(string requestId, int timeoutMs);
        
        #endregion
        
        #region Initialization
        
        private static bool isInitialized = false;
        
        /// <summary>
        /// Initializes the network module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            if (isInitialized)
                return true;
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            isInitialized = JSPluginNetworkInitialize() == 1;
            #else
            Debug.Log("[JSPluginNetwork] Initialized in stub mode (non-WebGL environment)");
            isInitialized = true;
            #endif
            
            if (isInitialized)
            {
                // Create timer component to manage timeouts
                var timerObject = new GameObject("JSPluginNetworkTimer");
                timerObject.AddComponent<NetworkTimeoutMonitor>();
                GameObject.DontDestroyOnLoad(timerObject);
            }
            
            return isInitialized;
        }
        
        /// <summary>
        /// Shuts down the network module and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            try
            {
                // Close all active WebSockets
                foreach (var socket in new Dictionary<string, WebSocket>(ActiveWebSockets))
                {
                    socket.Value.Close(1001, "Application shutting down");
                }
                
                ActiveWebSockets.Clear();
                
                // Clear all pending requests
                foreach (var requestId in new List<string>(RequestCallbacks.Keys))
                {
                    CancelRequest(requestId);
                }
                
                RequestCallbacks.Clear();
                
                // Remove timeout monitor
                var timerObject = GameObject.Find("JSPluginNetworkTimer");
                if (timerObject != null)
                {
                    GameObject.Destroy(timerObject);
                }
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginNetworkShutdown();
                #endif
                
                isInitialized = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error during shutdown: {ex.Message}");
            }
        }
        
        #endregion
        
        #region HTTP Requests
        
        /// <summary>
        /// HTTP request data
        /// </summary>
        [Serializable]
        public class RequestData
        {
            /// <summary>The request ID</summary>
            public string RequestId;
            
            /// <summary>The URL to request</summary>
            public string Url;
            
            /// <summary>The HTTP method (GET, POST, etc.)</summary>
            public string Method = "GET";
            
            /// <summary>Request headers</summary>
            public Dictionary<string, string> Headers;
            
            /// <summary>Request body (for POST, PUT, etc.)</summary>
            public string Body;
            
            /// <summary>Maximum time in seconds before timeout</summary>
            public float Timeout = 30f;
            
            /// <summary>Whether to include credentials for CORS requests</summary>
            public bool WithCredentials = false;
            
            /// <summary>Response type (text, json, arraybuffer, blob)</summary>
            public string ResponseType = "json";
            
            /// <summary>JSON representation of headers</summary>
            public string HeadersJson
            {
                get
                {
                    if (Headers == null || Headers.Count == 0)
                        return "{}";
                        
                    var jsonObj = new Dictionary<string, string>();
                    foreach (var pair in Headers)
                    {
                        jsonObj[pair.Key] = pair.Value;
                    }
                    
                    return JsonUtility.ToJson(new Wrapper<string> { Values = jsonObj });
                }
            }
        }
        
        /// <summary>
        /// HTTP response data
        /// </summary>
        [Serializable]
        public class ResponseData
        {
            /// <summary>Original request ID</summary>
            public string RequestId;
            
            /// <summary>HTTP status code</summary>
            public int StatusCode;
            
            /// <summary>Response text</summary>
            public string Text;
            
            /// <summary>Response headers</summary>
            public Dictionary<string, string> Headers = new Dictionary<string, string>();
            
            /// <summary>Whether the request was successful</summary>
            public bool IsSuccess;
            
            /// <summary>Error message if the request failed</summary>
            public string Error;
            
            /// <summary>Whether the request timed out</summary>
            public bool IsTimeout;
            
            /// <summary>Whether the request completed</summary>
            public bool IsComplete;
        }
        
        // Wrapper class for serializing dictionaries to JSON
        [Serializable]
        private class Wrapper<T>
        {
            public Dictionary<string, T> Values = new Dictionary<string, T>();
            
            public string ToJson()
            {
                return JsonUtility.ToJson(this);
            }
        }
        
        // Active requests for tracking
        private static readonly Dictionary<string, Action<ResponseData>> RequestCallbacks = new Dictionary<string, Action<ResponseData>>();
        
        // Track request timeouts
        private static readonly Dictionary<string, RequestTimeoutInfo> RequestTimeouts = new Dictionary<string, RequestTimeoutInfo>();
        
        private class RequestTimeoutInfo
        {
            public string RequestId { get; set; }
            public DateTime StartTime { get; set; }
            public int TimeoutMs { get; set; }
            public bool HasTimedOut => TimeoutMs > 0 && (DateTime.Now - StartTime).TotalMilliseconds > TimeoutMs;
        }
        
        /// <summary>
        /// Sets a timeout for a network request
        /// </summary>
        /// <param name="requestId">The ID of the request</param>
        /// <param name="timeoutMs">Timeout duration in milliseconds</param>
        public static void SetRequestTimeout(string requestId, int timeoutMs)
        {
            try
            {
                if (!isInitialized)
                    Initialize();
                    
                if (string.IsNullOrEmpty(requestId))
                {
                    Debug.LogError("[JSPluginNetwork] Request ID cannot be null or empty");
                    return;
                }
                
                if (timeoutMs <= 0)
                {
                    Debug.LogWarning("[JSPluginNetwork] Timeout must be greater than zero");
                    return;
                }
                
                // Add to tracking dictionary
                RequestTimeouts[requestId] = new RequestTimeoutInfo
                {
                    RequestId = requestId,
                    StartTime = DateTime.Now,
                    TimeoutMs = timeoutMs
                };
                
                // Set in JavaScript
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginNetworkSetRequestTimeout(requestId, timeoutMs);
                #else
                Debug.Log($"[JSPluginNetwork] Would set timeout of {timeoutMs}ms for request {requestId}");
                #endif
                
                Debug.Log($"[JSPluginNetwork] Set timeout of {timeoutMs}ms for request {requestId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error setting request timeout: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handles request timeout
        /// </summary>
        /// <param name="requestId">The ID of the request that timed out</param>
        private static void HandleRequestTimeout(string requestId)
        {
            try
            {
                if (RequestCallbacks.TryGetValue(requestId, out var callback))
                {
                    var response = new ResponseData
                    {
                        RequestId = requestId,
                        StatusCode = 0,
                        Text = "Request timed out",
                        IsSuccess = false,
                        IsTimeout = true,
                        IsComplete = true
                    };
                    
                    callback?.Invoke(response);
                    RequestCallbacks.Remove(requestId);
                }
                
                Debug.LogWarning($"[JSPluginNetwork] Request {requestId} timed out");
                
                RequestTimeouts.Remove(requestId);
                CancelRequest(requestId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error handling request timeout: {ex.Message}");
            }
        }
        
        // Component to monitor request timeouts
        private class NetworkTimeoutMonitor : MonoBehaviour
        {
            private readonly List<string> timedOutRequests = new List<string>();
            
            private void Update()
            {
                timedOutRequests.Clear();
                
                // Check for timed-out requests
                foreach (var timeout in RequestTimeouts)
                {
                    if (timeout.Value.HasTimedOut)
                    {
                        timedOutRequests.Add(timeout.Key);
                    }
                }
                
                // Handle timed-out requests
                foreach (var requestId in timedOutRequests)
                {
                    HandleRequestTimeout(requestId);
                }
            }
        }
        
        /// <summary>
        /// Sends an HTTP request via JavaScript
        /// </summary>
        /// <param name="request">Request configuration</param>
        /// <param name="callback">Callback to receive the response</param>
        /// <returns>True if the request was successfully initiated</returns>
        public static bool SendRequest(RequestData request, Action<ResponseData> callback = null)
        {
            if (!isInitialized)
                Initialize();
                
            if (string.IsNullOrEmpty(request.RequestId))
                request.RequestId = Guid.NewGuid().ToString();
                
            if (callback != null)
                RequestCallbacks[request.RequestId] = callback;
                
            // Set timeout if specified
            if (request.Timeout > 0)
            {
                SetRequestTimeout(request.RequestId, (int)(request.Timeout * 1000));
            }
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginNetworkSendRequest(
                request.RequestId,
                request.Url,
                request.Method,
                request.HeadersJson,
                request.Body ?? "",
                "JSPluginNetwork",
                "HandleResponse"
            ) == 1;
            #else
            // Simulate request in non-WebGL environments
            Debug.Log($"[JSPluginNetwork] Would send {request.Method} request to {request.Url}");
            
            // Create a fake response for testing
            var response = new ResponseData
            {
                RequestId = request.RequestId,
                StatusCode = 200,
                Text = "{\"success\":true,\"message\":\"This is a simulated response in non-WebGL mode\"}",
                IsSuccess = true,
                IsComplete = true
            };
            
            // Call the callback on the next frame
            if (callback != null)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => callback(response));
            }
            
            return true;
            #endif
        }
        
        /// <summary>
        /// Cancels an in-progress HTTP request
        /// </summary>
        /// <param name="requestId">The ID of the request to cancel</param>
        /// <returns>True if the request was found and cancelled</returns>
        public static bool CancelRequest(string requestId)
        {
            if (!isInitialized || string.IsNullOrEmpty(requestId))
                return false;
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginNetworkCancelRequest(requestId) == 1;
            #else
            Debug.Log($"[JSPluginNetwork] Would cancel request {requestId}");
            RequestCallbacks.Remove(requestId);
            return true;
            #endif
        }
        
        /// <summary>
        /// Handles an HTTP response from JavaScript
        /// </summary>
        /// <param name="jsonResponse">JSON response data</param>
        public static void HandleResponse(string jsonResponse)
        {
            try
            {
                ResponseData response = JsonUtility.FromJson<ResponseData>(jsonResponse);
                
                // Remove from timeout tracking
                RequestTimeouts.Remove(response.RequestId);
                
                if (RequestCallbacks.TryGetValue(response.RequestId, out var callback))
                {
                    callback?.Invoke(response);
                    
                    // Remove the callback if request is complete
                    if (response.IsComplete)
                    {
                        RequestCallbacks.Remove(response.RequestId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error handling response: {ex.Message}");
            }
        }
        
        #endregion
        
        #region WebSocket
        
        // Dictionary to track active WebSockets
        private static readonly Dictionary<string, WebSocket> ActiveWebSockets = new Dictionary<string, WebSocket>();
        
        /// <summary>
        /// State of a WebSocket connection
        /// </summary>
        public enum WebSocketState
        {
            /// <summary>Connection not yet established</summary>
            Connecting = 0,
            
            /// <summary>Connection established and ready to communicate</summary>
            Open = 1,
            
            /// <summary>Connection is closing</summary>
            Closing = 2,
            
            /// <summary>Connection is closed or could not be opened</summary>
            Closed = 3
        }
        
        /// <summary>
        /// WebSocket connection for real-time communication
        /// </summary>
        public class WebSocket
        {
            public string SocketId { get; }
            public string Url { get; }
            public WebSocketState State { get; private set; } = WebSocketState.Connecting;
            public event Action<string> OnMessage;
            public event Action<byte[]> OnBinaryMessage;
            public event Action OnOpen;
            public event Action<int, string> OnClose;
            public event Action<string> OnError;
            
            private readonly string objectId;
            private bool isValid = true;
            
            /// <summary>
            /// Creates a new WebSocket connection
            /// </summary>
            /// <param name="url">WebSocket server URL</param>
            /// <param name="protocols">Optional WebSocket protocols</param>
            /// <param name="objectId">Object ID for callbacks (defaults to JSPluginNetwork)</param>
            public WebSocket(string url, string protocols = null, string objectId = "JSPluginNetwork")
            {
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentException("WebSocket URL cannot be null or empty", nameof(url));
                
                SocketId = "ws_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                Url = url;
                this.objectId = objectId;
                
                if (!Initialize())
                    Initialize();
                
                // Register with callback system
                ActiveWebSockets[SocketId] = this;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                int result = JSPluginNetworkCreateWebSocket(SocketId, url, protocols, 
                    objectId, "HandleWebSocketMessage", "HandleWebSocketOpen", 
                    "HandleWebSocketClose", "HandleWebSocketError");
                
                if (result != 1)
                {
                    Debug.LogError($"[JSPluginNetwork] Failed to create WebSocket connection to {url}");
                    OnError?.Invoke("Failed to create WebSocket connection");
                    isValid = false;
                }
                #else
                // Simulate connection in non-WebGL environments
                Debug.Log($"[JSPluginNetwork] Would connect to WebSocket at {url}");
                
                // Simulate successful connection after a delay
                UnityMainThreadDispatcher.Instance().StartCoroutine(SimulateConnection());
                #endif
            }
            
            /// <summary>
            /// Sends a text message through the WebSocket
            /// </summary>
            /// <param name="message">Message to send</param>
            /// <returns>True if the message was sent successfully</returns>
            public bool Send(string message)
            {
                if (!isValid || State != WebSocketState.Open)
                    return false;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginNetworkSendWebSocketMessage(SocketId, message) == 1;
                #else
                Debug.Log($"[JSPluginNetwork] Would send WebSocket message: {message}");
                return true;
                #endif
            }
            
            /// <summary>
            /// Sends a binary message through the WebSocket
            /// </summary>
            /// <param name="data">Binary data to send</param>
            /// <returns>True if the message was sent successfully</returns>
            public bool Send(byte[] data)
            {
                if (!isValid || State != WebSocketState.Open || data == null)
                    return false;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                // Convert binary data to base64 string with a special prefix
                string base64 = Convert.ToBase64String(data);
                string message = "__binary__:" + base64;
                return JSPluginNetworkSendWebSocketMessage(SocketId, message) == 1;
                #else
                Debug.Log($"[JSPluginNetwork] Would send WebSocket binary message: {data.Length} bytes");
                return true;
                #endif
            }
            
            /// <summary>
            /// Closes the WebSocket connection
            /// </summary>
            /// <param name="code">Close status code</param>
            /// <param name="reason">Reason for closing</param>
            /// <returns>True if close request was successful</returns>
            public bool Close(int code = 1000, string reason = "")
            {
                if (!isValid)
                    return false;
                
                State = WebSocketState.Closing;
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginNetworkCloseWebSocket(SocketId, code, reason) == 1;
                #else
                Debug.Log($"[JSPluginNetwork] Would close WebSocket with code {code}: {reason}");
                
                // Simulate WebSocket closing
                UnityMainThreadDispatcher.Instance().StartCoroutine(SimulateClosing(code, reason));
                return true;
                #endif
            }
            
            // Update state based on events
            internal void HandleOpen()
            {
                State = WebSocketState.Open;
                OnOpen?.Invoke();
            }
            
            internal void HandleMessage(string message, bool isBinary)
            {
                if (isBinary)
                {
                    try
                    {
                        byte[] binaryData = Convert.FromBase64String(message);
                        OnBinaryMessage?.Invoke(binaryData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[JSPluginNetwork] Error decoding binary message: {ex.Message}");
                        OnError?.Invoke($"Error decoding binary message: {ex.Message}");
                    }
                }
                else
                {
                    OnMessage?.Invoke(message);
                }
            }
            
            internal void HandleClose(int code, string reason)
            {
                State = WebSocketState.Closed;
                isValid = false;
                ActiveWebSockets.Remove(SocketId);
                OnClose?.Invoke(code, reason);
            }
            
            internal void HandleError(string error)
            {
                OnError?.Invoke(error);
            }
            
            #if !UNITY_WEBGL || UNITY_EDITOR
            // Simulate WebSocket connection in non-WebGL environments
            private IEnumerator SimulateConnection()
            {
                yield return new WaitForSeconds(0.2f);
                HandleOpen();
                
                // Simulate a welcome message
                yield return new WaitForSeconds(0.1f);
                HandleMessage("Welcome to simulated WebSocket", false);
            }
            
            private IEnumerator SimulateClosing(int code, string reason)
            {
                yield return new WaitForSeconds(0.2f);
                HandleClose(code, reason);
            }
            #endif
        }
        
        /// <summary>
        /// Handles a WebSocket message event from JavaScript
        /// </summary>
        /// <param name="data">Event data in JSON format</param>
        public static void HandleWebSocketMessage(string data)
        {
            try
            {
                WebSocketMessageData message = JsonUtility.FromJson<WebSocketMessageData>(data);
                
                if (ActiveWebSockets.TryGetValue(message.SocketId, out var socket))
                {
                    socket.HandleMessage(message.Message, message.IsBinary);
                }
                else
                {
                    Debug.LogWarning($"[JSPluginNetwork] Received message for unknown WebSocket: {message.SocketId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error processing WebSocket message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handles a WebSocket open event from JavaScript
        /// </summary>
        /// <param name="socketId">Socket ID</param>
        public static void HandleWebSocketOpen(string socketId)
        {
            if (ActiveWebSockets.TryGetValue(socketId, out var socket))
            {
                socket.HandleOpen();
            }
        }
        
        /// <summary>
        /// Handles a WebSocket close event from JavaScript
        /// </summary>
        /// <param name="data">Event data in JSON format</param>
        public static void HandleWebSocketClose(string data)
        {
            try
            {
                var closeData = JsonUtility.FromJson<WebSocketCloseData>(data);
                
                if (ActiveWebSockets.TryGetValue(closeData.SocketId, out var socket))
                {
                    socket.HandleClose(closeData.Code, closeData.Reason);
                    ActiveWebSockets.Remove(closeData.SocketId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error handling WebSocket close: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handles a WebSocket error event from JavaScript
        /// </summary>
        /// <param name="data">Event data in JSON format</param>
        public static void HandleWebSocketError(string data)
        {
            try
            {
                var errorData = JsonUtility.FromJson<WebSocketErrorData>(data);
                
                if (ActiveWebSockets.TryGetValue(errorData.SocketId, out var socket))
                {
                    socket.HandleError(errorData.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginNetwork] Error handling WebSocket error: {ex.Message}");
            }
        }
        
        // Data classes for WebSocket event handling
        
        [Serializable]
        private class WebSocketMessageData
        {
            public string SocketId;
            public string Message;
            public bool IsBinary;
        }
        
        [Serializable]
        private class WebSocketCloseData
        {
            public string SocketId;
            public int Code;
            public string Reason;
        }
        
        [Serializable]
        private class WebSocketErrorData
        {
            public string SocketId;
            public string Error;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Helper class to ensure callbacks are executed on the Unity main thread
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private readonly object _queueLock = new object();
        
        /// <summary>
        /// Gets the singleton instance, creating it if needed
        /// </summary>
        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance == null)
            {
                var go = new GameObject("UnityMainThreadDispatcher");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
        
        private void Update()
        {
            lock (_queueLock)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }
        
        /// <summary>
        /// Queues an action to be executed on the main thread
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void Enqueue(Action action)
        {
            lock (_queueLock)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}
