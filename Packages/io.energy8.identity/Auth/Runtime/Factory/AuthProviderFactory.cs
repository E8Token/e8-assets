using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Auth.Runtime.Providers;
using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Base;
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
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");

#if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGLAuthProvider();
#else
            if (httpClient == null)
            {
                httpClient = new UnityHttpClient(config?.AuthServerUrl ?? "http://localhost");
            }

            return new NativeAuthProvider(httpClient);
#endif
        }
    }
}
