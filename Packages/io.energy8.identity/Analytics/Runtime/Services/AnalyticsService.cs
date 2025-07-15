using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Analytics.Core.Providers;
using Energy8.Identity.Analytics.Core.Services;
using Energy8.Identity.Analytics.Runtime.Factory;
using Energy8.Identity.Configuration.Core;
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
                Debug.Log("Initializing Analytics Service");
                await analyticsProvider.Initialize(ct);
                IsInitialized = true;
                Debug.Log("Analytics Service initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Analytics Service: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IdentityConfiguration.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot log event: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.LogEvent(eventName, parameters);
                
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log($"Analytics event logged: {eventName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error logging analytics event {eventName}: {ex.Message}");
            }
        }

        public void SetUserId(string userId)
        {
            if (!IdentityConfiguration.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot set user ID: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.SetUserId(userId);
                
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log($"Analytics user ID set: {userId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user ID: {ex.Message}");
            }
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            if (!IdentityConfiguration.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot set user properties: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.SetUserProperties(properties);
                
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log("Analytics user properties set");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user properties: {ex.Message}");
            }
        }

        public void ResetAnalyticsData()
        {
            if (!IdentityConfiguration.EnableAnalytics) return;

            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot reset analytics data: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.ResetAnalyticsData();
                
                if (IdentityConfiguration.EnableDebugLogging)
                {
                    Debug.Log("Analytics data reset");
                }
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
