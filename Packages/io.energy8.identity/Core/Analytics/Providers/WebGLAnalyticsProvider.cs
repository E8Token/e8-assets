using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Plugins.WebGL.Analytics;
#endif

namespace Energy8.Identity.Core.Analytics.Providers
{
    public class WebGLAnalyticsProvider : IAnalyticsProvider
    {
        private readonly ILogger<WebGLAnalyticsProvider> logger = new Logger<WebGLAnalyticsProvider>();
        
        public bool IsInitialized { get; private set; }

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Initializing WebGL Analytics Provider");

#if UNITY_WEBGL && !UNITY_EDITOR
                await FirebaseWebGLAnalyticsPlugin.Instance.Initialize();
                FirebaseWebGLAnalyticsPlugin.Instance.OnError += HandleError;
#else
                logger.LogInfo("WebGL Analytics Provider is not supported on this platform");
#endif

                IsInitialized = true;
                logger.LogInfo("WebGL Analytics Provider initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to initialize WebGL Analytics Provider: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (!IsInitialized)
                {
                    logger.LogWarning("Cannot log event: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.LogEventAsync(eventName, parameters);
                logger.LogInfo($"Logged event: {eventName}");
#else
                logger.LogInfo($"Skipped logging event on non-WebGL platform: {eventName}");
#endif
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
                    logger.LogWarning("Cannot set user ID: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.SetUserIdAsync(userId);
                logger.LogInfo($"Set user ID: {userId}");
#else
                logger.LogInfo($"Skipped setting user ID on non-WebGL platform: {userId}");
#endif
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
                    logger.LogWarning("Cannot set user properties: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.SetUserPropertiesAsync(properties);
                logger.LogInfo("Set user properties");
#else
                logger.LogInfo("Skipped setting user properties on non-WebGL platform");
#endif
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
                    logger.LogWarning("Cannot reset analytics data: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.ResetAsync();
                logger.LogInfo("Reset analytics data");
#else
                logger.LogInfo("Skipped resetting analytics data on non-WebGL platform");
#endif
            }
            catch (Exception ex)
            {
                logger.LogError($"Error resetting analytics data: {ex.Message}");
            }
        }

        private void HandleError(string error)
        {
            logger.LogError($"WebGL Analytics error: {error}");
        }
    }
}
