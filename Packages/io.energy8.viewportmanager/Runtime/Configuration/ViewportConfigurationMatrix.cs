using System;
using System.Linq;
using UnityEngine;
using Energy8.ViewportManager.Core;
using VmScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;
using VmDeviceType = Energy8.ViewportManager.Core.DeviceType;

namespace Energy8.ViewportManager.Configuration
{
    /// <summary>
    /// Simple configuration matrix that maps viewport contexts to Unity Quality Levels
    /// Platform + Orientation → Unity Quality Level
    /// </summary>
    [CreateAssetMenu(fileName = "ViewportConfigMatrix", menuName = "Energy8/Viewport Config Matrix")]
    public class ViewportConfigurationMatrix : ScriptableObject
    {
        [Header("Configuration Entries")]
        [SerializeField] private ViewportConfigEntry[] configurations = new ViewportConfigEntry[0];
        
        [Header("Default Fallback")]
        [SerializeField] private int defaultUnityQualityLevel = 2;

        [Serializable]
        public class ViewportConfigEntry
        {
            [Header("Context")]
            public VmScreenOrientation orientation;
            public VmDeviceType deviceType;
            public Platform platform;
            
            [Header("Unity Quality Level")]
            [Range(0, 5)]
            [Tooltip("Unity Quality Level (0-5). See Edit > Project Settings > Quality")]
            public int unityQualityLevel = 2;
            
            [Header("Custom Overrides (Optional)")]
            public int customTargetFrameRate = 0;
            public bool forceDisableVSync = false;

            public ViewportConfigEntry()
            {
                orientation = VmScreenOrientation.Landscape;
                deviceType = VmDeviceType.Desktop;
                platform = Platform.WebGL;
                unityQualityLevel = 2;
            }

            public ViewportConfigEntry(VmScreenOrientation orientation, VmDeviceType deviceType, Platform platform, int qualityLevel)
            {
                this.orientation = orientation;
                this.deviceType = deviceType;
                this.platform = platform;
                this.unityQualityLevel = qualityLevel;
            }

            public bool MatchesContext(ViewportContext context)
            {
                return orientation == context.orientation && 
                       deviceType == context.deviceType && 
                       platform == context.platform;
            }

            public override string ToString()
            {
                return $"{orientation}+{deviceType}+{platform}=Level{unityQualityLevel}";
            }
        }

        private void OnEnable()
        {
            if (configurations == null || configurations.Length == 0)
            {
                InitializeWithDefaults();
            }
        }

        /// <summary>
        /// Get configuration for the given viewport context
        /// </summary>
        public ViewportConfiguration GetConfiguration(ViewportContext context)
        {
            // Find exact match
            var entry = configurations?.FirstOrDefault(c => c.MatchesContext(context));
            
            if (entry != null)
            {
                return new ViewportConfiguration(entry.unityQualityLevel)
                {
                    customTargetFrameRate = entry.customTargetFrameRate,
                    forceDisableVSync = entry.forceDisableVSync
                };
            }
            
            // Fallback to default
            Debug.LogWarning($"No configuration found for {context}, using default quality level: {defaultUnityQualityLevel}");
            return new ViewportConfiguration(defaultUnityQualityLevel);
        }

        /// <summary>
        /// Get configuration for viewport info (converts to ViewportContext first)
        /// </summary>
        public ViewportConfiguration GetConfiguration(ViewportInfo info)
        {
            var context = new ViewportContext(
                info.orientation, 
                info.deviceType, 
                info.platform
            );
            
            return GetConfiguration(context);
        }

        /// <summary>
        /// Initialize with sensible defaults
        /// </summary>
        private void InitializeWithDefaults()
        {
            configurations = new ViewportConfigEntry[]
            {
                // Mobile Portrait - Low quality for battery life
                new ViewportConfigEntry(VmScreenOrientation.Portrait, VmDeviceType.Mobile, Platform.Android, 0),
                new ViewportConfigEntry(VmScreenOrientation.Portrait, VmDeviceType.Mobile, Platform.iOS, 0),
                new ViewportConfigEntry(VmScreenOrientation.Portrait, VmDeviceType.Mobile, Platform.WebGL, 0),
                
                // Mobile Landscape - Medium quality
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Mobile, Platform.Android, 2),
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Mobile, Platform.iOS, 2),
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Mobile, Platform.WebGL, 2),
                
                // Desktop - High quality
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Desktop, Platform.WebGL, 3),
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Desktop, Platform.Windows, 4),
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Desktop, Platform.macOS, 4),
                new ViewportConfigEntry(VmScreenOrientation.Landscape, VmDeviceType.Desktop, Platform.Linux, 3)
            };
        }

        /// <summary>
        /// Get all available viewport contexts from the matrix
        /// </summary>
        public ViewportContext[] GetAllContexts()
        {
            return configurations.Select(c => new ViewportContext(
                c.orientation,
                c.deviceType, 
                c.platform
            )).ToArray();
        }

        /// <summary>
        /// Get debug information about the matrix
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $"ViewportConfigurationMatrix: {configurations?.Length ?? 0} entries\n";
            info += $"Default Quality Level: {defaultUnityQualityLevel}\n";
            
            if (configurations != null)
            {
                foreach (var config in configurations)
                {
                    info += $"  {config}\n";
                }
            }
            
            return info;
        }
    }
}
