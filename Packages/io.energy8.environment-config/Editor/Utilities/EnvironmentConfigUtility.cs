using UnityEngine;
using System.IO;

namespace Energy8.EnvironmentConfig.Editor.Utilities
{
    /// <summary>
    /// Utility class for managing environment configuration JSON file
    /// </summary>
    public static class EnvironmentConfigUtility
    {
        private const string EnvironmentConfigPath = "Assets/StreamingAssets/E8Config/environment.json";

        /// <summary>
        /// Get current environment from JSON config
        /// </summary>
        public static string GetCurrentEnvironment()
        {
            if (File.Exists(EnvironmentConfigPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(EnvironmentConfigPath);
                    var config = JsonUtility.FromJson<EnvironmentConfigData>(jsonContent);
                    return config.currentEnvironment ?? "Development";
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[EnvironmentConfigUtility] Failed to read environment config: {e.Message}");
                    return "Development";
                }
            }
            return "Development";
        }

        /// <summary>
        /// Set current environment in JSON config
        /// </summary>
        public static void SetCurrentEnvironment(string environmentName)
        {
            string directory = Path.GetDirectoryName(EnvironmentConfigPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new EnvironmentConfigData
            {
                currentEnvironment = environmentName
            };

            string jsonContent = JsonUtility.ToJson(config, true);
            File.WriteAllText(EnvironmentConfigPath, jsonContent);
            
            Debug.Log($"[EnvironmentConfigUtility] Environment set to: {environmentName}");
        }

        /// <summary>
        /// Ensure environment config file exists
        /// </summary>
        public static void EnsureConfigExists()
        {
            if (!File.Exists(EnvironmentConfigPath))
            {
                SetCurrentEnvironment("Development");
            }
        }

        [System.Serializable]
        private class EnvironmentConfigData
        {
            public string currentEnvironment;
        }
    }
}
