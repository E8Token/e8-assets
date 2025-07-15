using System;
using System.Collections.Generic;

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
    /// HTTP response wrapper with metadata
    /// </summary>
    [Serializable]
    public class HttpResponse<T>
    {
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public bool IsSuccess { get; set; }
        public TimeSpan ResponseTime { get; set; }
        
        public HttpResponse()
        {
        }
        
        public HttpResponse(T data, int statusCode, bool isSuccess)
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
}
