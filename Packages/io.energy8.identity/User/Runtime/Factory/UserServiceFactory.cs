using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using Energy8.Identity.User.Core.Services;
using UnityEngine;

namespace Energy8.Identity.User.Runtime.Factory
{
    /// <summary>
    /// Factory for creating user service instances with proper dependency injection
    /// </summary>
    public static class UserServiceFactory
    {
        /// <summary>
        /// Creates the appropriate user service with provided dependencies
        /// </summary>
        /// <param name="httpClient">HTTP client for API calls</param>
        /// <param name="authProvider">Authentication provider</param>
        /// <returns>User service instance</returns>
        public static IUserService CreateService(IHttpClient httpClient, IAuthProvider authProvider)
        {
            if (httpClient == null)
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.LogError("HttpClient is null in UserServiceFactory");
                }
                throw new System.ArgumentNullException(nameof(httpClient));
            }
            
            if (authProvider == null)
            {
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.LogError("AuthProvider is null in UserServiceFactory");
                }
                throw new System.ArgumentNullException(nameof(authProvider));
            }
            
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log("Creating UserService instance with dependencies");
            }
            
            return new Runtime.Services.UserService(httpClient, authProvider);
        }
        
        /// <summary>
        /// Creates user service with automatic dependency resolution
        /// </summary>
        /// <returns>User service with default dependencies</returns>
        public static IUserService CreateDefaultService()
        {
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log("Creating UserService with default dependencies");
            }
            
            // TODO: Use HttpClientFactory.CreateDefaultClient() when Runtime namespace is available
            var httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
            var authProvider = Auth.Runtime.Factory.AuthProviderFactory.CreateProvider(httpClient);
            
            return CreateService(httpClient, authProvider);
        }
        
        /// <summary>
        /// Creates a user service for testing with mock dependencies
        /// </summary>
        /// <returns>Test user service instance</returns>
        public static IUserService CreateTestService()
        {
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log("Creating test UserService instance");
            }
            
            // Create mock dependencies for testing
            // TODO: Use HttpClientFactory.CreateTestClient() when Runtime namespace is available
            var mockHttpClient = new UnityHttpClient("http://localhost:3000");
            var mockAuthProvider = Auth.Runtime.Factory.AuthProviderFactory.CreateTestProvider();
            
            return new Runtime.Services.UserService(mockHttpClient, mockAuthProvider);
        }
    }
}
