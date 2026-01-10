using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using Energy8.Identity.User.Core.Services;
using Energy8.EnvironmentConfig.Base;
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
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            
            if (httpClient == null)
            {
                throw new System.ArgumentNullException(nameof(httpClient));
            }

            if (authProvider == null)
            {
                throw new System.ArgumentNullException(nameof(authProvider));
            }
            
            return new Runtime.Services.UserService(httpClient, authProvider);
        }
        
        /// <summary>
        /// Creates user service with automatic dependency resolution
        /// </summary>
        /// <returns>User service with default dependencies</returns>
        public static IUserService CreateDefaultService()
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");

            // TODO: Use HttpClientFactory.CreateDefaultClient() when Runtime namespace is available
            var httpClient = new UnityHttpClient(config?.AuthServerUrl ?? "http://localhost");
            var authProvider = Auth.Runtime.Factory.AuthProviderFactory.CreateProvider(httpClient);

            return CreateService(httpClient, authProvider);
        }
    }
}
