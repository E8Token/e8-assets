using Energy8.Identity.Analytics.Core.Providers;
using Energy8.Identity.Analytics.Runtime.Providers;
using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Base;
using UnityEngine;

namespace Energy8.Identity.Analytics.Runtime.Factory
{
    /// <summary>
    /// Factory for creating appropriate analytics providers based on platform and configuration
    /// </summary>
    public static class AnalyticsProviderFactory
    {
        /// <summary>
        /// Creates the appropriate analytics provider for the current platform
        /// </summary>
        /// <returns>Platform-specific analytics provider or null if analytics disabled</returns>
        public static IAnalyticsProvider CreateProvider()
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            
            // Check if analytics is enabled in configuration
            if (config == null || !config.EnableAnalytics)
            {
                return new DefaultAnalyticsProvider(); // Return no-op provider
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGLAnalyticsProvider();
#elif !UNITY_WEBGL || UNITY_EDITOR
            // Use native Firebase provider for non-WebGL platforms
            return new FirebaseNativeAnalyticsProvider();
#else
            // Fallback to default provider
            return new DefaultAnalyticsProvider();
#endif
        }

        /// <summary>
        /// Creates a provider for testing purposes
        /// </summary>
        /// <returns>Default analytics provider</returns>
        public static IAnalyticsProvider CreateTestProvider()
        {
            return new DefaultAnalyticsProvider();
        }
    }
}
