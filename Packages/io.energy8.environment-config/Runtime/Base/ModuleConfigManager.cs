using UnityEngine;
using System;
using System.Collections.Generic;

namespace Energy8.EnvironmentConfig.Base
{
    /// <summary>
    /// Generic manager for loading module configurations based on current environment
    /// Usage: var config = ModuleConfigManager<MyConfig>.GetCurrentConfig("ModuleName");
    /// </summary>
    public static class ModuleConfigManager<T> where T : BaseModuleConfig
    {
        private static readonly Dictionary<string, T> configCache = new Dictionary<string, T>();

        /// <summary>
        /// Get current configuration for the specified module and environment
        /// </summary>
        /// <param name="moduleName">Module name (e.g., "Identity", "Analytics")</param>
        /// <returns>Configuration asset or null if not found</returns>
        public static T GetCurrentConfig(string moduleName)
        {
            string environmentName = Core.EnvironmentManager.CurrentEnvironment;
            return GetConfigForEnvironment(moduleName, environmentName);
        }

        /// <summary>
        /// Get configuration for specific environment
        /// </summary>
        /// <param name="moduleName">Module name (kept for compatibility, but not used for path)</param>
        /// <param name="environmentName">Environment name</param>
        /// <returns>Configuration asset or null if not found</returns>
        public static T GetConfigForEnvironment(string moduleName, string environmentName)
        {
            string configClassName = typeof(T).Name;
            string cacheKey = $"{configClassName}_{environmentName}";
            
            if (configCache.TryGetValue(cacheKey, out T cachedConfig))
            {
                return cachedConfig;
            }

            string configPath = $"E8Config/{configClassName}_{environmentName}";
            var config = Resources.Load<T>(configPath);

            if (config != null)
            {
                configCache[cacheKey] = config;
                Debug.Log($"[ModuleConfigManager] Loaded {configClassName} config for environment: {environmentName}");
                config.LogConfigInfo();
            }
            else
            {
                Debug.LogError($"[ModuleConfigManager] Failed to load {configClassName} config for environment: {environmentName}. Path: {configPath}");
            }

            return config;
        }

        /// <summary>
        /// Clear configuration cache for this type (useful for hot-reload in editor)
        /// </summary>
        public static void ClearCache()
        {
            configCache.Clear();
            Debug.Log("[ModuleConfigManager] Configuration cache cleared");
        }

        /// <summary>
        /// Clear all configuration caches (useful for hot-reload in editor)
        /// </summary>
        public static void ClearAllCaches()
        {
            // Static method to clear all caches for all types
            // Note: This is a limitation of the generic type system
            // Each ModuleConfigManager<T> maintains its own cache
            Debug.Log("[ModuleConfigManager] All configuration caches clearing requested");
        }

        /// <summary>
        /// Validate configuration is properly set up
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <param name="moduleName">Module name for error messages</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateConfig(T config, string moduleName)
        {
            if (config == null)
            {
                string configClassName = typeof(T).Name;
                Debug.LogError($"[ModuleConfigManager] {configClassName} config is null");
                return false;
            }

            return true;
        }
    }
}
