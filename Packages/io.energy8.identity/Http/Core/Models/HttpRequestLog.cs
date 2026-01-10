using System;
using System.Collections.Generic;

namespace Energy8.Identity.Http.Core.Models
{
    /// <summary>
    /// Represents a complete HTTP request log including request data, response, and metadata
    /// </summary>
    public class HttpRequestLog
    {
        /// <summary>
        /// Timestamp when the request was made (ISO 8601 format)
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Full URL of the request
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Request data sent to the server (JSON format)
        /// </summary>
        public object RequestData { get; set; }

        /// <summary>
        /// Request headers (Authorization header is masked if token logging is disabled)
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// HTTP response status code
        /// </summary>
        public long ResponseCode { get; set; }

        /// <summary>
        /// Response body from the server
        /// </summary>
        public string ResponseBody { get; set; }

        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the request failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Request duration in milliseconds
        /// </summary>
        public long Duration { get; set; }

        public HttpRequestLog()
        {
            Timestamp = DateTime.UtcNow.ToString("o");
            Headers = new Dictionary<string, string>();
        }
    }
}