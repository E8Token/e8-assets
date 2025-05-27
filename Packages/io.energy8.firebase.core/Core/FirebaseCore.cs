using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Core.Api;
using Energy8.Firebase.Core.Configuration;
using Energy8.Firebase.Core.Configuration.Models;
using Energy8.Firebase.Core.Models;
using UnityEngine;

namespace Energy8.Firebase.Core
{
    public static class FirebaseCore
    {
        private static IFirebaseCoreApi coreApi;
        private static bool isInitialized = false;

        public static event Action<FirebaseAppInfo> OnAppInitialized;
        public static event Action<string> OnAppDeleted;
        public static event Action<string, Exception> OnInitializationError;        static FirebaseCore()
        {
            // Проверяем конфигурацию при статической инициализации
            EnsureConfigurationExists();
            InitializeProvider();
        }

        private static void EnsureConfigurationExists()
        {
            var config = FirebaseCoreConfiguration.Instance;
            if (config == null)
            {
                Debug.LogWarning("[FirebaseCore] Configuration not found. Please create one via Project Settings > Firebase > Core");
            }
            else
            {
                Debug.Log("[FirebaseCore] Configuration loaded successfully");
            }
        }private static void InitializeProvider()
        {
            // Provider будет инициализирован через конкретные платформенные assembly
            // Native assembly инициализирует NativeFirebaseCoreProvider
            // WebGL assembly инициализирует WebFirebaseCoreProvider
            
            // Временно создаем заглушку, пока не подключатся платформенные провайдеры
            if (coreApi == null)
            {
                Debug.LogWarning("[FirebaseCore] No platform provider available. Make sure Native or WebGL assemblies are included.");
                return;
            }

            coreApi.OnAppInitialized += (appInfo) => OnAppInitialized?.Invoke(appInfo);
            coreApi.OnAppDeleted += (appName) => OnAppDeleted?.Invoke(appName);
            coreApi.OnInitializationError += (appName, error) => OnInitializationError?.Invoke(appName, error);

            isInitialized = true;

            // Auto-initialize if enabled
            if (FirebaseCoreConfiguration.AutoInitialize)
            {
                _ = AutoInitializeDefaultApp();
            }
        }        private static async Task AutoInitializeDefaultApp()
        {
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                var platform = FirebasePlatform.Web;
#else
                var platform = FirebasePlatform.SDK;
#endif
                var config = FirebaseCoreConfiguration.GetConfigForCurrentEnvironment(platform);
                if (!string.IsNullOrEmpty(config))
                {
                    await InitializeAppAsync(config, FirebaseCoreConfiguration.GetDefaultAppName());
                }
                else
                {
                    Debug.LogWarning($"[FirebaseCore] No configuration found for current environment ({FirebaseCoreConfiguration.DefaultEnvironment}) and platform ({platform})");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Auto-initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize app for specific environment and platform
        /// </summary>
        public static async Task<FirebaseAppInfo> InitializeAppForEnvironmentAsync(FirebaseEnvironment environment, FirebasePlatform platform, string appName = null, CancellationToken ct = default)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("FirebaseCore is not initialized");
            }

            var config = FirebaseCoreConfiguration.GetConfigForEnvironment(environment, platform);
            if (string.IsNullOrEmpty(config))
            {
                throw new InvalidOperationException($"No configuration found for environment {environment} and platform {platform}");
            }            appName ??= FirebaseCoreConfiguration.GetAppNameForEnvironment(environment, platform);
            return await coreApi.InitializeAppAsync(config, appName, ct);
        }

        /// <summary>
        /// Initialize app for current default environment
        /// </summary>
        public static async Task<FirebaseAppInfo> InitializeAppForCurrentEnvironmentAsync(FirebasePlatform platform, string appName = null, CancellationToken ct = default)
        {
            return await InitializeAppForEnvironmentAsync(FirebaseCoreConfiguration.DefaultEnvironment, platform, appName, ct);
        }

        /// <summary>
        /// Check if configuration exists for specific environment and platform
        /// </summary>
        public static bool IsEnvironmentConfigured(FirebaseEnvironment environment, FirebasePlatform platform)
        {
            return FirebaseCoreConfiguration.IsEnvironmentConfigured(environment, platform);
        }

        /// <summary>
        /// Get current platform based on build target
        /// </summary>
        public static FirebasePlatform GetCurrentPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return FirebasePlatform.Web;
#else
            return FirebasePlatform.SDK;
#endif
        }        public static async Task<FirebaseAppInfo> InitializeAppAsync(string config, string appName = null, CancellationToken ct = default)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("FirebaseCore is not initialized");
            }

            appName ??= FirebaseCoreConfiguration.GetDefaultAppName();
            return await coreApi.InitializeAppAsync(config, appName, ct);
        }        public static FirebaseAppInfo GetApp(string appName = null)
        {
            appName ??= FirebaseCoreConfiguration.GetDefaultAppName();
            return coreApi?.GetApp(appName);
        }

        public static IEnumerable<FirebaseAppInfo> GetAllApps()
        {
            return coreApi?.GetAllApps() ?? new List<FirebaseAppInfo>();
        }        public static async Task<bool> DeleteAppAsync(string appName = null, CancellationToken ct = default)
        {
            if (!isInitialized)
            {
                return false;
            }

            appName ??= FirebaseCoreConfiguration.GetDefaultAppName();
            return await coreApi.DeleteAppAsync(appName, ct);
        }        public static bool IsAppInitialized(string appName = null)
        {
            appName ??= FirebaseCoreConfiguration.GetDefaultAppName();
            return coreApi?.IsAppInitialized(appName) ?? false;
        }

        public static FirebaseAppInfo DefaultApp => GetApp();
        public static bool IsProviderSet => coreApi != null;
        public static bool IsInitialized => isInitialized;

        /// <summary>
        /// Register platform provider (called by platform-specific assemblies)
        /// </summary>
        public static void RegisterProvider(IFirebaseCoreApi provider)
        {
            if (coreApi == null)
            {
                coreApi = provider;
                InitializeProvider();
            }
        }

        /// <summary>
        /// Unregister platform provider
        /// </summary>
        internal static void UnregisterProvider()
        {
            coreApi = null;
            isInitialized = false;
        }
    }
}
