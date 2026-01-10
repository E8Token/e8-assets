using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Analytics.Core.Providers;
using Energy8.Identity.Analytics.Core.Services;
using Energy8.Identity.Analytics.Runtime.Factory;
using Energy8.Identity.Configuration.Core;
using Energy8.EnvironmentConfig.Base;
using UnityEngine;

namespace Energy8.Identity.Analytics.Runtime.Services
{
    /// <summary>
    /// Implementation of analytics service that coordinates with analytics providers
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsProvider analyticsProvider;

        public bool IsInitialized { get; private set; }

        public AnalyticsService(IAnalyticsProvider analyticsProvider)
        {
            this.analyticsProvider = analyticsProvider ?? throw new ArgumentNullException(nameof(analyticsProvider));
        }

        /// <summary>
        /// Creates AnalyticsService with appropriate provider for current platform
        /// </summary>
        /// <returns>Configured AnalyticsService instance</returns>
        public static AnalyticsService CreateDefault()
        {
            var provider = AnalyticsProviderFactory.CreateProvider();
            return new AnalyticsService(provider);
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                await analyticsProvider.Initialize(ct);
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Analytics Service: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            if (config == null || !config.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                analyticsProvider.LogEvent(eventName, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error logging analytics event {eventName}: {ex.Message}");
            }
        }

        public void SetUserId(string userId)
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            if (config == null || !config.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                analyticsProvider.SetUserId(userId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user ID: {ex.Message}");
            }
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            if (config == null || !config.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                analyticsProvider.SetUserProperties(properties);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user properties: {ex.Message}");
            }
        }

        public void ResetAnalyticsData()
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            if (config == null || !config.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    return;
                }

                analyticsProvider.ResetAnalyticsData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error resetting analytics data: {ex.Message}");
            }
        }

        // Common events implementation
        public void LogSignIn(string method)
        {
            var parameters = new Dictionary<string, object>
            {
                { "method", method }
            };
            LogEvent("login", parameters);
        }

        public void LogSignOut()
        {
            LogEvent("logout");
        }

        public void LogScreenView(string screenName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "screen_name", screenName }
            };
            LogEvent("screen_view", parameters);
        }

        public void LogUserAction(string action, Dictionary<string, object> parameters = null)
        {
            var eventParams = new Dictionary<string, object>
            {
                { "action", action }
            };
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    eventParams[param.Key] = param.Value;
                }
            }
            
            LogEvent("user_action", eventParams);
        }
    }
}
