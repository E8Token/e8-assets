using System;
using UnityEngine;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;

namespace Energy8.ViewportManager
{
    /// <summary>
    /// Main viewport manager - handles automatic detection and configuration switching
    /// </summary>
    public static class ViewportManager
    {
        #region Properties
        
        /// <summary>
        /// Current viewport context
        /// </summary>
        public static ViewportContext CurrentContext { get; private set; }
        
        /// <summary>
        /// Current viewport configuration
        /// </summary>
        public static ViewportConfiguration CurrentConfiguration { get; private set; }
        
        /// <summary>
        /// Current Unity quality level (read-only for now)
        /// </summary>
        public static int CurrentQualityLevel => QualitySettings.GetQualityLevel();
        
        /// <summary>
        /// Is the system initialized?
        /// </summary>
        public static bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Configuration matrix being used
        /// </summary>
        public static ViewportConfigurationMatrix ConfigMatrix { get; private set; }
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when viewport context changes (orientation, device, platform)
        /// </summary>
        public static event Action<ViewportContext> OnContextChanged;
        
        /// <summary>
        /// Fired when viewport configuration changes
        /// </summary>
        public static event Action<ViewportConfiguration> OnConfigurationChanged;
        
        /// <summary>
        /// Fired when Unity quality level changes
        /// </summary>
        public static event Action<int> OnQualityChanged;
        
        /// <summary>
        /// Fired when system is initialized
        /// </summary>
        public static event Action OnInitialized;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize viewport manager with configuration matrix
        /// </summary>
        public static void Initialize(ViewportConfigurationMatrix configMatrix = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("ViewportManager is already initialized");
                return;
            }

            // Use provided matrix or try to load default
            ConfigMatrix = configMatrix ?? LoadDefaultConfigMatrix();
            
            if (ConfigMatrix == null)
            {
                Debug.LogError("ViewportManager: No configuration matrix provided and no default found. Creating fallback.");
                ConfigMatrix = CreateFallbackMatrix();
            }

            // Detect initial context
            CurrentContext = ViewportDetector.DetectContext();
            
            // Get initial configuration
            CurrentConfiguration = ConfigMatrix.GetConfiguration(CurrentContext);
            
            // Apply configuration
            ApplyCurrentConfiguration();
            
            IsInitialized = true;
            
            Debug.Log($"ViewportManager initialized: {CurrentContext} -> Unity Quality Level {CurrentConfiguration.unityQualityLevel}");
            
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Load default configuration matrix from Resources
        /// </summary>
        private static ViewportConfigurationMatrix LoadDefaultConfigMatrix()
        {
            return Resources.Load<ViewportConfigurationMatrix>("ViewportConfigMatrix");
        }

        /// <summary>
        /// Create fallback configuration matrix
        /// </summary>
        private static ViewportConfigurationMatrix CreateFallbackMatrix()
        {
            var matrix = ScriptableObject.CreateInstance<ViewportConfigurationMatrix>();
            // Default configurations will be initialized automatically in OnEnable
            return matrix;
        }
        
        #endregion

        #region Context Detection & Updates
        
        /// <summary>
        /// Force refresh of viewport context
        /// </summary>
        public static void RefreshContext()
        {
            if (!IsInitialized) return;
            
            var newContext = ViewportDetector.DetectContext();
            
            if (!CurrentContext.Equals(newContext))
            {
                UpdateContext(newContext);
            }
        }

        /// <summary>
        /// Update to new viewport context
        /// </summary>
        private static void UpdateContext(ViewportContext newContext)
        {
            var previousContext = CurrentContext;
            var previousQuality = CurrentQualityLevel;
            
            CurrentContext = newContext;
            CurrentConfiguration = ConfigMatrix.GetConfiguration(CurrentContext);
            
            Debug.Log($"Viewport context changed: {previousContext} -> {CurrentContext}");
            
            // Apply new configuration
            ApplyCurrentConfiguration();
            
            // Fire events
            OnContextChanged?.Invoke(CurrentContext);
            OnConfigurationChanged?.Invoke(CurrentConfiguration);
            
            if (previousQuality != CurrentQualityLevel)
            {
                OnQualityChanged?.Invoke(CurrentQualityLevel);
            }
        }

        /// <summary>
        /// Apply current configuration to Unity systems
        /// </summary>
        private static void ApplyCurrentConfiguration()
        {
            if (CurrentConfiguration == null) return;
            
            // TEMPORARILY DISABLED - only orientation detection and events
            Debug.Log($"[ViewportManager] Configuration detected: {CurrentConfiguration} (graphics settings disabled)");
            
            // TODO: Re-enable when graphics settings are needed
            /*
            try
            {
                CurrentConfiguration.ApplyToUnity();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to apply viewport configuration: {e.Message}");
            }
            */
        }
        
        #endregion

        #region Manual Control
        
        /// <summary>
        /// Manually set quality profile (overrides automatic detection)
        /// </summary>
        public static void SetQualityLevel(int qualityLevel)
        {
            if (!IsInitialized) return;
            
            var newConfig = new ViewportConfiguration(qualityLevel);
            var previousQuality = CurrentQualityLevel;
            
            CurrentConfiguration = newConfig;
            ApplyCurrentConfiguration();
            
            Debug.Log($"Quality level manually set to: {qualityLevel}");
            
            OnConfigurationChanged?.Invoke(CurrentConfiguration);
            
            if (previousQuality != CurrentQualityLevel)
            {
                OnQualityChanged?.Invoke(CurrentQualityLevel);
            }
        }

        /// <summary>
        /// Manually set configuration matrix
        /// </summary>
        public static void SetConfigurationMatrix(ViewportConfigurationMatrix matrix)
        {
            if (matrix == null)
            {
                Debug.LogError("Cannot set null configuration matrix");
                return;
            }
            
            ConfigMatrix = matrix;
            
            if (IsInitialized)
            {
                RefreshContext();
            }
            
            Debug.Log("Configuration matrix updated");
        }
        
        #endregion

        #region Utility
        
        /// <summary>
        /// Get detailed system information
        /// </summary>
        public static string GetSystemInfo()
        {
            return $"ViewportManager Status:\n" +
                   $"- Initialized: {IsInitialized}\n" +
                   $"- Current Context: {CurrentContext}\n" +
                   $"- Current Quality Level: {CurrentQualityLevel}\n" +
                   $"- Config Matrix: {(ConfigMatrix != null ? "Loaded" : "None")}\n\n" +
                   ViewportDetector.GetDeviceInfo();
        }

        /// <summary>
        /// Reset to automatic detection mode
        /// </summary>
        public static void ResetToAutomatic()
        {
            if (!IsInitialized) return;
            
            RefreshContext();
            Debug.Log("Reset to automatic viewport detection");
        }
        
        #endregion
    }
}
