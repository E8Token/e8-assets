namespace Energy8.Identity.Http.Core
{
    /// <summary>
    /// Interface for HTTP client factory to support dependency injection and testing
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Creates HTTP client with specified base URL
        /// </summary>
        /// <param name="baseUrl">Base URL for HTTP requests</param>
        /// <returns>HTTP client instance</returns>
        IHttpClient CreateClient(string baseUrl);
        
        /// <summary>
        /// Creates HTTP client with configuration-based URL
        /// </summary>
        /// <returns>HTTP client instance with default configuration</returns>
        IHttpClient CreateDefaultClient();
        
        /// <summary>
        /// Creates HTTP client for testing
        /// </summary>
        /// <returns>Test HTTP client instance</returns>
        IHttpClient CreateTestClient();
        
        /// <summary>
        /// Creates HTTP client with authentication token pre-configured
        /// </summary>
        /// <param name="baseUrl">Base URL for HTTP requests</param>
        /// <param name="authToken">Authentication token</param>
        /// <returns>HTTP client instance with auth token</returns>
        IHttpClient CreateAuthenticatedClient(string baseUrl, string authToken);
    }
}
