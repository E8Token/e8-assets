using System.Collections.Generic;

namespace Energy8.JSPluginTools.Network
{
    /// <summary>
    /// Represents an HTTP response
    /// </summary>
    public class HttpResponse<T>
    {
        /// <summary>
        /// The response data converted to the specified type
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Whether the request was successful (status code 2xx)
        /// </summary>
        public bool IsSuccessful => StatusCode >= 200 && StatusCode < 300;

        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Raw response body as string
        /// </summary>
        public string RawResponse { get; set; }
    }

    /// <summary>
    /// Non-generic version of HttpResponse for cases when type is unknown
    /// </summary>
    public class HttpResponse : HttpResponse<object>
    {
    }
}