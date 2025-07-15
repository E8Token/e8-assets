using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Energy8.Identity.Analytics.Core.Providers;


#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Analytics;
using Firebase;
#endif

namespace Energy8.Identity.Analytics.Runtime.Providers
{
    public class FirebaseNativeAnalyticsProvider : IAnalyticsProvider
    {
        public bool IsInitialized { get; private set; }

        public event Action<string> OnError;

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                Debug.Log("Initializing Firebase Native Analytics Provider");

#if !UNITY_WEBGL || UNITY_EDITOR
                // Ensure Firebase is initialized
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    IsInitialized = true;
                    Debug.Log("Firebase Native Analytics Provider initialized successfully");
                }
                else
                {
                    Debug.LogError($"Firebase dependencies not available: {dependencyStatus}");
                    throw new Exception($"Firebase initialization failed: {dependencyStatus}");
                }
#else
                Debug.LogWarning("Firebase Native Analytics not supported on WebGL");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Firebase Native Analytics: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Cannot log event: Firebase Native Analytics not initialized");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                if (parameters == null || parameters.Count == 0)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                }
                else
                {
                    var firebaseParams = new Parameter[parameters.Count];
                    int index = 0;
                    foreach (var param in parameters)
                    {
                        firebaseParams[index] = new Parameter(param.Key, param.Value?.ToString() ?? "");
                        index++;
                    }
                    FirebaseAnalytics.LogEvent(eventName, firebaseParams);
                }
                Debug.Log($"Logged event: {eventName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error logging event {eventName}: {ex.Message}");
            }
#endif
        }

        public void SetUserId(string userId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Cannot set user ID: Firebase Native Analytics not initialized");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                FirebaseAnalytics.SetUserId(userId);
                Debug.Log($"Set user ID: {userId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user ID: {ex.Message}");
            }
#endif
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Cannot set user properties: Firebase Native Analytics not initialized");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        FirebaseAnalytics.SetUserProperty(property.Key, property.Value?.ToString() ?? "");
                    }
                }
                Debug.Log("Set user properties");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting user properties: {ex.Message}");
            }
#endif
        }

        public void ResetAnalyticsData()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("Cannot reset analytics: Firebase Native Analytics not initialized");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                FirebaseAnalytics.ResetAnalyticsData();
                Debug.Log("Reset analytics data");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error resetting analytics data: {ex.Message}");
            }
#endif
        }
    }
}
