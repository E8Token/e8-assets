using System;
using UnityEngine;

namespace Energy8.BuildDeploySystem
{
    public static class BuildSystem
    {
        public static event Action<string> OnBuildStarted;
        public static event Action<string, bool> OnBuildCompleted;
        public static event Action<string> OnBuildProgress;
        
        private static BuildConfiguration currentBuildConfig;
        
        public static void StartBuild(BuildConfiguration config, bool clean = false)
        {
            if (config == null)
            {
                Debug.LogError("Build configuration is null");
                return;
            }
            
            currentBuildConfig = config;
            OnBuildStarted?.Invoke(config.configName);
            
#if UNITY_EDITOR
            // В Editor режиме используем рефлексию для вызова Editor кода
            var editorAssembly = System.Reflection.Assembly.Load("Energy8.BuildDeploySystem.Editor");
            var editorType = editorAssembly.GetType("Energy8.BuildDeploySystem.Editor.BuildSystemEditor");
            var method = editorType.GetMethod("BuildWithConfiguration", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, new object[] { config, clean });
#else
            Debug.LogWarning("Build system can only be used in Unity Editor");
#endif
        }
        
        public static BuildConfiguration GetCurrentBuildConfig()
        {
            return currentBuildConfig;
        }
        
        // Public methods for triggering events from Editor code
        public static void TriggerBuildCompleted(string configName, bool success)
        {
            OnBuildCompleted?.Invoke(configName, success);
        }
        
        public static void TriggerBuildProgress(string message)
        {
            OnBuildProgress?.Invoke(message);
        }
    }
}