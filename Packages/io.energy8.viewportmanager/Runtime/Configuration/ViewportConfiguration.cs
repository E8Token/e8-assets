using System;
using UnityEngine;

namespace Energy8.ViewportManager.Configuration
{
    /// <summary>
    /// Simple viewport configuration that maps to Unity Quality Settings
    /// 
    /// UNITY QUALITY SETTINGS INTEGRATION:
    /// This system uses Unity's built-in Quality Settings (Edit > Project Settings > Quality).
    /// Simply configure the quality levels you need in Unity and this system will switch between them
    /// based on platform + orientation combinations.
    /// </summary>
    [Serializable]
    public class ViewportConfiguration
    {
        [Header("Unity Quality Settings")]
        [Tooltip("Unity Quality Level (0-5). See Edit > Project Settings > Quality")]
        public int unityQualityLevel = 2;
        
        [Header("Custom Overrides (Optional)")]
        [Tooltip("Override target frame rate. Leave 0 to use Unity Quality Settings default")]
        public int customTargetFrameRate = 0;
        
        [Tooltip("Force disable VSync regardless of Quality Settings")]
        public bool forceDisableVSync = false;

        public ViewportConfiguration()
        {
            unityQualityLevel = 2; // Default to Medium
        }

        public ViewportConfiguration(int qualityLevel)
        {
            unityQualityLevel = qualityLevel;
        }

        /// <summary>
        /// Apply this configuration to Unity's graphics settings (TEMPORARILY DISABLED)
        /// </summary>
        public void ApplyToUnity()
        {
            // TEMPORARILY DISABLED - only logging for now
            Debug.Log($"[ViewportConfiguration] Would apply Unity quality level {unityQualityLevel} " +
                     $"(customFPS: {customTargetFrameRate}, forceNoVSync: {forceDisableVSync})");
            
            // TODO: Re-enable when graphics settings are needed
            /*
            // Apply Unity quality level
            QualitySettings.SetQualityLevel(unityQualityLevel, true); // applyExpensiveChanges = true
            
            // Apply custom overrides if specified
            if (customTargetFrameRate > 0)
            {
                Application.targetFrameRate = customTargetFrameRate;
            }
            
            // Override VSync if needed
            if (forceDisableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }
            */
        }

        /// <summary>
        /// Get current Unity quality settings info for debugging
        /// </summary>
        public static string GetCurrentUnityQualityInfo()
        {
            var currentLevel = QualitySettings.GetQualityLevel();
            var levelName = QualitySettings.names[currentLevel];
            
            return $"Unity Quality: Level {currentLevel} ({levelName}), " +
                   $"Shadows: {QualitySettings.shadows}, " +
                   $"AntiAliasing: {QualitySettings.antiAliasing}, " +
                   $"VSync: {QualitySettings.vSyncCount}, " +
                   $"TargetFPS: {Application.targetFrameRate}";
        }

        public override string ToString()
        {
            return $"ViewportConfiguration(Unity Level {unityQualityLevel})";
        }
    }
}
