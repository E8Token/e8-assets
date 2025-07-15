using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using UnityEngine;

namespace Energy8.Identity.Http.Runtime.Factory
{
    /// <summary>
    /// Factory for creating HTTP client instances with proper configuration.
    /// Follows the same pattern as AnalyticsProviderFactory and AuthProviderFactory.
    /// Provides centralized creation and configuration of HTTP clients for the Identity system.
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        /// Creates HTTP client with specified base URL
        /// </summary>
        /// <param name="baseUrl">Base URL for HTTP requests</param>
        /// <returns>HTTP client instance</returns>
        public static IHttpClient CreateClient(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                Debug.LogError("BaseUrl is null or empty in HttpClientFactory");
                throw new System.ArgumentException("BaseUrl cannot be null or empty", nameof(baseUrl));
            }
            
            Debug.Log($"Creating HTTP client with base URL: {baseUrl}");
            
            return new UnityHttpClient(baseUrl);
        }
        
        /// <summary>
        /// Creates HTTP client with configuration-based URL
        /// </summary>
        /// <returns>HTTP client instance with default configuration</returns>
        public static IHttpClient CreateDefaultClient()
        {
            const string defaultUrl = "http://localhost";
            
            Debug.Log($"Creating default HTTP client with URL: {defaultUrl}");
            
            return CreateClient(defaultUrl);
        }
        
        /// <summary>
        /// Creates HTTP client for testing with localhost URL
        /// </summary>
        /// <returns>Test HTTP client instance</returns>
        public static IHttpClient CreateTestClient()
        {
            const string testUrl = "http://localhost:3000";
            
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log($"Creating test HTTP client with URL: {testUrl}");
            }
            
            return CreateClient(testUrl);
        }
        
        /// <summary>
        /// Creates HTTP client with authentication token pre-configured
        /// </summary>
        /// <param name="baseUrl">Base URL for HTTP requests</param>
        /// <param name="authToken">Authentication token</param>
        /// <returns>HTTP client instance with auth token</returns>
        public static IHttpClient CreateAuthenticatedClient(string baseUrl, string authToken)
        {
            var client = CreateClient(baseUrl);
            
            if (!string.IsNullOrEmpty(authToken))
            {
                client.SetAuthToken(authToken);
                
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log("HTTP client configured with authentication token");
                }
            }
            
            return client;
        }
    }
}
