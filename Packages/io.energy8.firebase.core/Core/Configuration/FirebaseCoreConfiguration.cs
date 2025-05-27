using System.Collections.Generic;
using System.Linq;
using Energy8.Firebase.Core.Configuration.Models;
using UnityEngine;
using System;

namespace Energy8.Firebase.Core.Configuration
{
    [CreateAssetMenu(menuName = "Firebase/Core Configuration")]
    public class FirebaseCoreConfiguration : ScriptableObject
    {
        // Environment-based configuration system with separate SDK and Web lists
        [SerializeField] private List<FirebaseEnvironmentConfig> sdkEnvironmentConfigs = new();
        [SerializeField] private List<FirebaseEnvironmentConfig> webEnvironmentConfigs = new();
        
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private FirebaseEnvironment defaultEnvironment = FirebaseEnvironment.Local;

        private const string ConfigPath = "Firebase/Core/FirebaseCoreConfiguration";

        private static FirebaseCoreConfiguration instance;
        public static FirebaseCoreConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<FirebaseCoreConfiguration>(ConfigPath);
                    
#if UNITY_EDITOR
                    // Автоматически создаем файл настроек если его нет
                    if (instance == null)
                    {
                        instance = CreateDefaultConfiguration();
                    }
#endif
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        private static FirebaseCoreConfiguration CreateDefaultConfiguration()
        {
            var config = CreateInstance<FirebaseCoreConfiguration>();
            config.autoInitialize = true;
            config.defaultEnvironment = FirebaseEnvironment.Local;
            
            // Initialize default SDK environment configurations
            config.sdkEnvironmentConfigs = new List<FirebaseEnvironmentConfig>
            {
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Local, FirebasePlatform.SDK),
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Debug, FirebasePlatform.SDK),
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Production, FirebasePlatform.SDK)
            };
            
            // Initialize default Web environment configurations
            config.webEnvironmentConfigs = new List<FirebaseEnvironmentConfig>
            {
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Local, FirebasePlatform.Web),
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Debug, FirebasePlatform.Web),
                new FirebaseEnvironmentConfig(FirebaseEnvironment.Production, FirebasePlatform.Web)
            };
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var corePath = "Assets/Resources/Firebase/Core";
            
            if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(firebasePath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(corePath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Core");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Core/FirebaseCoreConfiguration.asset";
            UnityEditor.AssetDatabase.CreateAsset(config, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[FirebaseCore] Created default configuration at: {assetPath}");
            return config;
        }
#endif

        // Properties
        public static FirebaseEnvironment DefaultEnvironment
        {
            get => Instance.defaultEnvironment;
            set => Instance.defaultEnvironment = value;
        }

        public static List<FirebaseEnvironmentConfig> SDKEnvironmentConfigs => Instance.sdkEnvironmentConfigs;
        public static List<FirebaseEnvironmentConfig> WebEnvironmentConfigs => Instance.webEnvironmentConfigs;

        public static bool AutoInitialize
        {
            get => Instance.autoInitialize;
            set => Instance.autoInitialize = value;
        }

        // Generate app name based on environment and platform
        public static string GetAppNameForEnvironment(FirebaseEnvironment environment, FirebasePlatform platform)
        {
            return $"Firebase_{environment}_{platform}";
        }

        // Get default app name for current environment
        public static string GetDefaultAppName()
        {
            return GetAppNameForEnvironment(DefaultEnvironment, GetCurrentPlatform());
        }

        // Get current platform based on build target
        public static FirebasePlatform GetCurrentPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return FirebasePlatform.Web;
#else
            return FirebasePlatform.SDK;
#endif
        }

        // Main methods for getting configurations
        public static string GetConfigForEnvironment(FirebaseEnvironment environment, FirebasePlatform platform)
        {
            var config = GetEnvironmentConfig(environment, platform);
            return config?.ConfigText;
        }

        public static FirebaseEnvironmentConfig GetEnvironmentConfig(FirebaseEnvironment environment, FirebasePlatform platform)
        {
            var configs = platform == FirebasePlatform.SDK ? Instance.sdkEnvironmentConfigs : Instance.webEnvironmentConfigs;
            return configs.FirstOrDefault(c => 
                c.Environment == environment && 
                c.Platform == platform && 
                c.IsEnabled);
        }

        public static string GetConfigForCurrentEnvironment(FirebasePlatform platform)
        {
            return GetConfigForEnvironment(DefaultEnvironment, platform);
        }

        // Helper methods for environment management
        public static void SetConfigForEnvironment(FirebaseEnvironment environment, FirebasePlatform platform, TextAsset configAsset)
        {
            var config = GetEnvironmentConfig(environment, platform);
            if (config != null)
            {
                config.Config = configAsset;
            }
            else
            {
                // Create new configuration
                var newConfig = new FirebaseEnvironmentConfig(environment, platform, configAsset);
                var configs = platform == FirebasePlatform.SDK ? Instance.sdkEnvironmentConfigs : Instance.webEnvironmentConfigs;
                configs.Add(newConfig);
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(Instance);
#endif
        }

        public static void SetEnvironmentEnabled(FirebaseEnvironment environment, FirebasePlatform platform, bool enabled)
        {
            var config = GetEnvironmentConfig(environment, platform);
            if (config != null)
            {
                config.IsEnabled = enabled;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public static bool IsEnvironmentConfigured(FirebaseEnvironment environment, FirebasePlatform platform)
        {
            var config = GetEnvironmentConfig(environment, platform);
            return config != null && config.IsEnabled && !string.IsNullOrEmpty(config.ConfigText);
        }

        public static IEnumerable<FirebaseEnvironmentConfig> GetAllConfigs()
        {
            return Instance.sdkEnvironmentConfigs.Concat(Instance.webEnvironmentConfigs);
        }

        public static IEnumerable<FirebaseEnvironmentConfig> GetConfigsForPlatform(FirebasePlatform platform)
        {
            return platform == FirebasePlatform.SDK ? Instance.sdkEnvironmentConfigs : Instance.webEnvironmentConfigs;
        }

        // Utility method to get available environments for a platform
        public static IEnumerable<FirebaseEnvironment> GetAvailableEnvironments(FirebasePlatform platform)
        {
            var configs = GetConfigsForPlatform(platform);
            return configs.Where(c => c.IsEnabled && !string.IsNullOrEmpty(c.ConfigText))
                         .Select(c => c.Environment)
                         .Distinct();
        }
    }
}
