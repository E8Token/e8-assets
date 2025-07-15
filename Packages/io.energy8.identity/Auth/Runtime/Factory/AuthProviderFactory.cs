using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Auth.Runtime.Providers;
using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using UnityEngine;

namespace Energy8.Identity.Auth.Runtime.Factory
{
    /// <summary>
    /// Factory for creating appropriate auth providers based on platform and configuration
    /// </summary>
    public static class AuthProviderFactory
    {
        /// <summary>
        /// Creates the appropriate auth provider for the current platform
        /// </summary>
        /// <param name="httpClient">HTTP client for native provider</param>
        /// <returns>Platform-specific auth provider</returns>
        public static IAuthProvider CreateProvider(IHttpClient httpClient = null)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log("Creating WebGL Auth Provider");
            }
            return new WebGLAuthProvider();
#else
            if (IdentityConfiguration.EnableDebugLogging)
            {
                Debug.Log("Creating Native Auth Provider");
            }
            
            if (httpClient == null)
            {
                Debug.LogWarning("HttpClient is null for NativeAuthProvider, creating default");
                httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
            }
            
            return new NativeAuthProvider(httpClient);
#endif
        }

        /// <summary>
        /// Creates a provider for testing purposes
        /// </summary>
        /// <param name="httpClient">HTTP client for testing</param>
        /// <returns>Test auth provider</returns>
        public static IAuthProvider CreateTestProvider(IHttpClient httpClient = null)
        {
            httpClient ??= new UnityHttpClient("localhost");
            return new NativeAuthProvider(httpClient);
        }
    }
}
