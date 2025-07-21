using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Analytics.Core.Providers;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Analytics.WebGL;
#endif

namespace Energy8.Identity.Analytics.Runtime.Providers
{
    public class WebGLAnalyticsProvider : IAnalyticsProvider
    {        
        public bool IsInitialized { get; private set; }

        public event Action<string> OnError;

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                Debug.Log("Initializing WebGL Analytics Provider");

#if UNITY_WEBGL && !UNITY_EDITOR
                await FirebaseWebGLAnalyticsPlugin.Instance.Initialize();
                FirebaseWebGLAnalyticsPlugin.Instance.OnError += HandleError;
#else
                Debug.Log("WebGL Analytics Provider is not supported on this platform");
#endif

                IsInitialized = true;
                Debug.Log("WebGL Analytics Provider initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize WebGL Analytics Provider: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot log event: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.LogEventAsync(eventName, parameters);
                Debug.Log($"Logged event: {eventName}");
#else
                Debug.Log($"Skipped logging event on non-WebGL platform: {eventName}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error logging event {eventName}: {ex.Message}");
            }
        }

        public void SetUserId(string userId)
        {
            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot set user ID: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.SetUserIdAsync(userId);
                Debug.Log($"Set user ID: {userId}");
#else
                Debug.Log($"Skipped setting user ID on non-WebGL platform: {userId}");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user ID: {ex.Message}");
            }
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot set user properties: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.SetUserPropertiesAsync(properties);
                Debug.Log("Set user properties");
#else
                Debug.Log("Skipped setting user properties on non-WebGL platform");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user properties: {ex.Message}");
            }
        }

        public void ResetAnalyticsData()
        {
            try
            {
                if (!IsInitialized)
                {
                    Debug.LogWarning("Cannot reset analytics data: WebGL Analytics Provider is not initialized");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                FirebaseWebGLAnalyticsPlugin.Instance.ResetAsync();
                Debug.Log("Reset analytics data");
#else
                Debug.Log("Skipped resetting analytics data on non-WebGL platform");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error resetting analytics data: {ex.Message}");
            }
        }

        private void HandleError(string error)
        {
            Debug.LogError($"WebGL Analytics error: {error}");
        }
    }
}
