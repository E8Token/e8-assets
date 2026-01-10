using UnityEngine;
using System.IO;

namespace Energy8.EnvironmentConfig.Core
{
    /// <summary>
    /// Manages current build environment by reading from StreamingAssets
    /// Reads environment.json at runtime to determine current environment
    /// </summary>
    public static class EnvironmentManager
    {
        private const string EnvironmentConfigPath = "E8Config/environment.json";
        private const string DefaultEnvironment = "Development";
        private static string currentEnvironment;
        private static bool isInitialized = false;

        /// <summary>
        /// Get current environment name (read from environment.json)
        /// </summary>
        public static string CurrentEnvironment
        {
            get
            {
                if (!isInitialized)
                {
                    Initialize();
                }
                return currentEnvironment;
            }
        }

        /// <summary>
        /// Initialize environment by reading from StreamingAssets
        /// Called automatically on first access to CurrentEnvironment
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            string configPath = Path.Combine(Application.streamingAssetsPath, EnvironmentConfigPath);

            if (File.Exists(configPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(configPath);
                    var environmentConfig = JsonUtility.FromJson<EnvironmentConfigData>(jsonContent);
                    
                    if (!string.IsNullOrEmpty(environmentConfig.currentEnvironment))
                    {
                        currentEnvironment = environmentConfig.currentEnvironment;
                        Debug.Log($"[EnvironmentManager] Loaded environment from config: {currentEnvironment}");
                    }
                    else
                    {
                        currentEnvironment = DefaultEnvironment;
                        Debug.LogWarning($"[EnvironmentManager] Empty environment in config, using default: {DefaultEnvironment}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EnvironmentManager] Failed to read environment config: {e.Message}");
                    currentEnvironment = DefaultEnvironment;
                }
            }
            else
            {
                currentEnvironment = DefaultEnvironment;
                Debug.LogWarning($"[EnvironmentManager] Environment config not found at {configPath}, using default: {DefaultEnvironment}");
            }

            isInitialized = true;
        }

        /// <summary>
        /// Re-initialize environment (useful for hot-reload in editor)
        /// </summary>
        public static void Reload()
        {
            isInitialized = false;
            Initialize();
            // Note: Each ModuleManager<T> needs to clear its own cache
            Debug.Log("[EnvironmentManager] Environment reloaded. Module caches need to be cleared individually.");
        }

        /// <summary>
        /// Internal data class for JSON deserialization
        /// </summary>
        [System.Serializable]
        private class EnvironmentConfigData
        {
            public string currentEnvironment;
        }
    }
}
