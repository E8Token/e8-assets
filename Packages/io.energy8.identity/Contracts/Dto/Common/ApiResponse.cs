using Energy8.Contracts.Dto.Errors;

namespace Energy8.Contracts.Dto.Common
{
    /// <summary>
    /// Represents a standard API response wrapper
    /// </summary>
    [System.Serializable]
    public class ApiResponse<T> : DtoBase
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Contains the response data when the request is successful
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Contains error details when the request fails
        /// </summary>
        public ErrorDto Error { get; set; }

        /// <summary>
        /// Trace identifier for request tracking
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// Constructor for creating a response with all parameters
        /// </summary>
        /// <param name="success">Indicates if the request was successful</param>
        /// <param name="data">Response data</param>
        /// <param name="error">Error details</param>
        /// <param name="traceId">Trace identifier</param>
        public ApiResponse(bool success, T data = default(T), ErrorDto error = null, string traceId = null)
        {
            Success = success;
            Data = data;
            Error = error;
            TraceId = traceId;
        }

        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <param name="data">Response data</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>Successful ApiResponse</returns>
        public static ApiResponse<T> Ok(T data, string traceId = null)
        {
            return new ApiResponse<T>(true, data, null, traceId);
        }

        /// <summary>
        /// Creates a failed response with error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="description">Optional error description</param>
        /// <param name="traceId">Optional trace identifier</param>
        /// <returns>Failed ApiResponse</returns>
        public static ApiResponse<T> Fail(string message, string description = null, string traceId = null)
        {
            return new ApiResponse<T>(false, default(T), new ErrorDto(message, description), traceId);
        }
    }
}