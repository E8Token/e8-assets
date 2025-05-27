using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Analytics.Api;
using Energy8.Firebase.Analytics.Configuration;
using Energy8.Firebase.Analytics.Models;
using Energy8.Firebase.Core;
using UnityEngine;
using Firebase;

namespace Energy8.Firebase.Analytics
{
    public static class FirebaseAnalytics
    {
        private static IFirebaseAnalyticsApi analyticsApi;
        private static bool isInitialized = false;        public static event Action<string> OnEventLogged;
        public static event Action<string, Exception> OnEventLogError;
        public static event Action OnInitialized;

        static FirebaseAnalytics()
        {
            // Проверяем конфигурацию при статической инициализации
            EnsureConfigurationExists();
            InitializeProvider();
        }

        private static void EnsureConfigurationExists()
        {
            var config = FirebaseAnalyticsConfiguration.Instance;
            if (config == null)
            {
                Debug.LogWarning("[FirebaseAnalytics] Configuration not found. Please create one via Project Settings > Firebase > Analytics");
            }
            else
            {
                Debug.Log("[FirebaseAnalytics] Configuration loaded successfully");
            }
        }

        private static void InitializeProvider()
        {
            // Provider будет инициализирован через конкретные платформенные assembly
            // Native assembly инициализирует NativeFirebaseAnalyticsProvider
            // WebGL assembly инициализирует WebFirebaseAnalyticsProvider
            
            // Временно создаем заглушку, пока не подключатся платформенные провайдеры
            if (analyticsApi == null)
            {
                Debug.LogWarning("[FirebaseAnalytics] No platform provider available. Make sure Native or WebGL assemblies are included.");
                return;
            }

            analyticsApi.OnEventLogged += (eventName) => OnEventLogged?.Invoke(eventName);
            analyticsApi.OnEventLogError += (eventName, error) => OnEventLogError?.Invoke(eventName, error);            isInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Register platform-specific provider (called by platform assemblies)
        /// </summary>
        public static void RegisterProvider(IFirebaseAnalyticsApi provider)
        {
            if (provider == null)
            {
                Debug.LogError("[FirebaseAnalytics] Cannot register null provider");
                return;
            }

            analyticsApi = provider;
            
            if (!isInitialized)
            {
                InitializeProvider();
            }
            
            Debug.Log($"[FirebaseAnalytics] Provider registered: {provider.GetType().Name}");
        }        /// <summary>
        /// Log an analytics event
        /// </summary>
        public static async Task<bool> LogEventAsync(AnalyticsEvent eventData, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return false;
            
            try
            {
                await analyticsApi.LogEventAsync(eventData.Name, eventData.Parameters, ct);
                OnEventLogged?.Invoke(eventData.Name);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to log event '{eventData.Name}': {ex.Message}");
                OnEventLogError?.Invoke(eventData.Name, ex);
                return false;
            }
        }

        /// <summary>
        /// Initialize Firebase Analytics
        /// </summary>
        public static async Task<bool> InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                if (isInitialized && analyticsApi != null)
                {
                    return true;
                }

                // Wait for Firebase Core to be initialized
                while (!FirebaseCore.IsInitialized && !ct.IsCancellationRequested)
                {
                    await Task.Delay(100, ct);
                }

                if (ct.IsCancellationRequested)
                {
                    return false;
                }

                InitializeProvider();
                return IsInitialized;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to initialize: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Log an event with the given name
        /// </summary>        /// <summary>
        /// Log an event with the given name
        /// </summary>
        public static async Task<bool> LogEventAsync(string eventName, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return false;
            
            try
            {
                await analyticsApi.LogEventAsync(eventName, ct);
                OnEventLogged?.Invoke(eventName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to log event '{eventName}': {ex.Message}");
                OnEventLogError?.Invoke(eventName, ex);
                return false;
            }
        }

        /// <summary>
        /// Log an event with the given name and parameters
        /// </summary>
        public static async Task<bool> LogEventAsync(string eventName, Dictionary<string, object> parameters, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return false;
            
            try
            {
                await analyticsApi.LogEventAsync(eventName, parameters, ct);
                OnEventLogged?.Invoke(eventName);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to log event '{eventName}': {ex.Message}");
                OnEventLogError?.Invoke(eventName, ex);
                return false;
            }
        }        /// <summary>
        /// Set user ID for analytics
        /// </summary>
        public static async Task<bool> SetUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return false;
            
            try
            {
                await analyticsApi.SetUserIdAsync(userId, ct);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to set user ID: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set user property
        /// </summary>
        public static async Task<bool> SetUserPropertyAsync(string name, string value, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return false;
            
            try
            {
                await analyticsApi.SetUserPropertyAsync(name, value, ct);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to set user property '{name}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set analytics collection enabled/disabled
        /// </summary>
        public static async Task SetAnalyticsCollectionEnabledAsync(bool enabled, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return;
            
            try
            {
                await analyticsApi.SetAnalyticsCollectionEnabledAsync(enabled, ct);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to set analytics collection enabled: {ex.Message}");
            }
        }

        /// <summary>
        /// Set session timeout duration in milliseconds
        /// </summary>
        public static async Task SetSessionTimeoutDurationAsync(long milliseconds, CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return;
            
            try
            {
                await analyticsApi.SetSessionTimeoutDurationAsync(milliseconds, ct);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to set session timeout duration: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset analytics data
        /// </summary>
        public static async Task ResetAnalyticsDataAsync(CancellationToken ct = default)
        {
            if (!EnsureInitialized()) return;
            
            try
            {
                await analyticsApi.ResetAnalyticsDataAsync(ct);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalytics] Failed to reset analytics data: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if Firebase Analytics is initialized
        /// </summary>
        public static bool IsInitialized => isInitialized && analyticsApi != null;

        private static bool EnsureInitialized()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[FirebaseAnalytics] Firebase Analytics is not initialized. Make sure Firebase Core is initialized first.");
                return false;
            }

            if (!FirebaseCore.IsInitialized)
            {
                Debug.LogError("[FirebaseAnalytics] Firebase Core is not initialized. Analytics requires Core to be initialized first.");
                return false;
            }            return true;
        }

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        /// <summary>
        /// Reset Firebase Analytics state for testing purposes
        /// </summary>
        public static void ResetForTesting()
        {
            analyticsApi = null;
            isInitialized = false;
            OnEventLogged = null;
            OnEventLogError = null;
            OnInitialized = null;
        }
#endif
    }
}
