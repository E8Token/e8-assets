using System;
using System.Collections.Generic;
using System.Net;

namespace Energy8.Identity.Http.Core.Models
{
    /// <summary>
    /// HTTP request configuration options
    /// </summary>
    [Serializable]
    public class RequestOptions
    {
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public Dictionary<string, string> Headers { get; set; } = new();
        public bool LogRequests { get; set; } = false;
        public bool DetailedLogging { get; set; } = false;
        
        public RequestOptions()
        {
        }
        
        public RequestOptions(int timeoutSeconds, int retryCount = 3, bool logRequests = false)
        {
            TimeoutSeconds = timeoutSeconds;
            RetryCount = retryCount;
            LogRequests = logRequests;
        }
    }

    /// <summary>
    /// HTTP request model for middleware pipeline
    /// </summary>
    [Serializable]
    public class HttpRequest
    {
        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Full URL of the request
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Request data (payload)
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
            TimeoutSeconds = 30;
        }
    }

    /// <summary>
    /// HTTP response model for middleware pipeline
    /// </summary>
    [Serializable]
    public class HttpResponse
    {
        /// <summary>
        /// Deserialized response data
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Response body as string
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message (if any)
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        public HttpResponse()
        {
            StatusCode = HttpStatusCode.OK;
            Success = false;
        }
    }
    
    /// <summary>
    /// HTTP response wrapper with metadata (typed version)
    /// </summary>
    [Serializable]
    public class HttpResponseTyped<T>
    {
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public bool IsSuccess { get; set; }
        public TimeSpan ResponseTime { get; set; }
        
        public HttpResponseTyped()
        {
        }
        
        public HttpResponseTyped(T data, int statusCode, bool isSuccess)
        {
            Data = data;
            StatusCode = statusCode;
            IsSuccess = isSuccess;
        }
    }
    
    /// <summary>
    /// HTTP client statistics and metrics
    /// </summary>
    [Serializable]
    public class HttpClientStats
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public DateTime LastRequestTime { get; set; }
        
        public float SuccessRate => TotalRequests > 0 ? (float)SuccessfulRequests / TotalRequests : 0f;
        
        public HttpClientStats()
        {
        }
    }
    
    /// <summary>
    /// Request error information
    /// </summary>
    [Serializable]
    public class RequestErrorData
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
        
        public RequestErrorData()
        {
        }
        
        public RequestErrorData(string message, string code = null)
        {
            Message = message;
            Code = code;
        }
    }
}
