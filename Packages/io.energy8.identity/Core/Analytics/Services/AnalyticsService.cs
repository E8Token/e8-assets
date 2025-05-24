using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Analytics.Providers;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Core.Analytics.Services
{
    public interface IAnalyticsService
    {
        bool IsInitialized { get; }
        UniTask Initialize(CancellationToken ct);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserId(string userId);
        void SetUserProperties(Dictionary<string, object> properties);
        void ResetAnalyticsData();
        
        // Common events
        void LogSignIn(string method);
        void LogSignOut();
        void LogScreenView(string screenName);
        void LogUserAction(string action, Dictionary<string, object> parameters = null);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly ILogger<AnalyticsService> logger = new Logger<AnalyticsService>();
        private readonly IAnalyticsProvider analyticsProvider;

        public bool IsInitialized { get; private set; }

        public AnalyticsService(IAnalyticsProvider analyticsProvider)
        {
            this.analyticsProvider = analyticsProvider;
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Initializing Analytics Service");
                await analyticsProvider.Initialize(ct);
                IsInitialized = true;
                logger.LogInfo("Analytics Service initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to initialize Analytics Service: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (!IsInitialized)
                {
                    logger.LogWarning("Cannot log event: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.LogEvent(eventName, parameters);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error logging event {eventName}: {ex.Message}");
            }
        }

        public void SetUserId(string userId)
        {
            try
            {
                if (!IsInitialized)
                {
                    logger.LogWarning("Cannot set user ID: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.SetUserId(userId);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error setting user ID: {ex.Message}");
            }
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            try
            {
                if (!IsInitialized)
                {
                    logger.LogWarning("Cannot set user properties: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.SetUserProperties(properties);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error setting user properties: {ex.Message}");
            }
        }

        public void ResetAnalyticsData()
        {
            try
            {
                if (!IsInitialized)
                {
                    logger.LogWarning("Cannot reset analytics data: Analytics Service is not initialized");
                    return;
                }

                analyticsProvider.ResetAnalyticsData();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error resetting analytics data: {ex.Message}");
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
