using System;
using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.WebGL.AdaptivePerformance.Core
{
    /// <summary>
    /// Main manager for adaptive performance system that combines viewport data with performance monitoring
    /// to automatically adjust graphics settings for optimal performance
    /// </summary>
    public static class AdaptivePerformanceManager
    {
        // Configuration
        private static AdaptivePerformanceMatrix configurationMatrix;
        private static AdjustmentStrategy adjustmentStrategy = AdjustmentStrategy.Balanced;
        private static bool autoAdjustmentEnabled = true;
        private static float adjustmentCooldown = 2f; // Minimum time between adjustments
        private static float lastAdjustmentTime;
        
        // State
        private static bool isInitialized;
        private static PerformanceProfile currentProfile;
        private static PerformanceLevel targetPerformanceLevel;
        private static ViewportContext lastViewportContext;
        
        // Events
        public static event Action<PerformanceProfile> OnProfileChanged;
        public static event Action<PerformanceLevel> OnPerformanceLevelChanged;
        public static event Action<ViewportContext, PerformanceProfile> OnConfigurationApplied;
        public static event Action OnInitialized;
        
        // Properties
        public static bool IsInitialized => isInitialized;
        public static PerformanceProfile CurrentProfile => currentProfile;
        public static PerformanceLevel CurrentPerformanceLevel => currentProfile.level;
        public static AdjustmentStrategy Strategy => adjustmentStrategy;
        public static bool AutoAdjustmentEnabled => autoAdjustmentEnabled;
        public static AdaptivePerformanceMatrix ConfigurationMatrix => configurationMatrix;
        
        /// <summary>
        /// Initialize the adaptive performance system
        /// </summary>
        public static void Initialize(AdaptivePerformanceMatrix matrix = null)
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AdaptivePerformanceManager] Already initialized");
                return;
            }
            
            // Initialize performance monitoring
            PerformanceMonitor.Initialize();
            
            // Set configuration matrix
            configurationMatrix = matrix;
            if (configurationMatrix == null)
            {
                configurationMatrix = CreateDefaultMatrix();
                Debug.Log("[AdaptivePerformanceManager] Using default configuration matrix");
            }
            
            // Subscribe to viewport manager events
            if (Energy8.ViewportManager.ViewportManager.IsInitialized)
            {
                Energy8.ViewportManager.ViewportManager.OnContextChanged += OnViewportContextChanged;
                
                // Apply initial configuration
                var context = Energy8.ViewportManager.ViewportManager.CurrentContext;
                ApplyConfiguration(context);
            }
            else
            {
                Debug.LogWarning("[AdaptivePerformanceManager] ViewportManager not initialized. Will use fallback configuration.");
                ApplyFallbackConfiguration();
            }
            
            // Subscribe to performance events
            PerformanceMonitor.OnMetricsUpdated += OnPerformanceMetricsUpdated;
            PerformanceMonitor.OnFPSChanged += OnFPSChanged;
            
            isInitialized = true;
            lastAdjustmentTime = Time.unscaledTime;
            
            Debug.Log($"[AdaptivePerformanceManager] Initialized with strategy: {adjustmentStrategy}");
            OnInitialized?.Invoke();
        }
        
        /// <summary>
        /// Update the adaptive performance system (should be called every frame)
        /// </summary>
        public static void Update()
        {
            if (!isInitialized)
                return;
                
            // Update performance monitoring
            PerformanceMonitor.Update();
            
            // Check for automatic adjustments
            if (autoAdjustmentEnabled && CanAdjust())
            {
                CheckForPerformanceAdjustment();
            }
        }
        
        /// <summary>
        /// Apply configuration for the given viewport context
        /// </summary>
        public static void ApplyConfiguration(ViewportContext context)
        {
            if (configurationMatrix == null)
            {
                Debug.LogError("[AdaptivePerformanceManager] No configuration matrix available");
                return;
            }
            
            var profile = configurationMatrix.GetProfile(context);
            ApplyProfile(profile);
            
            lastViewportContext = context;
            OnConfigurationApplied?.Invoke(context, profile);
            
            Debug.Log($"[AdaptivePerformanceManager] Applied configuration for {context}: {profile}");
        }
        
        /// <summary>
        /// Apply a specific performance profile
        /// </summary>
        public static void ApplyProfile(PerformanceProfile profile)
        {
            var previousProfile = currentProfile;
            currentProfile = profile;
            
            // Apply Unity quality settings
            ApplyUnitySettings(profile);
            
            // Fire events if profile changed
            if (!previousProfile.Equals(profile))
            {
                OnProfileChanged?.Invoke(profile);
                
                if (previousProfile.level != profile.level)
                {
                    OnPerformanceLevelChanged?.Invoke(profile.level);
                }
            }
        }
        
        /// <summary>
        /// Apply Unity graphics settings from performance profile
        /// </summary>
        private static void ApplyUnitySettings(PerformanceProfile profile)
        {
            try
            {
                // Apply quality level
                if (profile.unityQualityLevel >= 0 && profile.unityQualityLevel < QualitySettings.names.Length)
                {
                    QualitySettings.SetQualityLevel(profile.unityQualityLevel, true);
                }
                
                // Apply target frame rate
                Application.targetFrameRate = profile.targetFrameRate;
                
                // Apply VSync
                QualitySettings.vSyncCount = profile.enableVSync ? 1 : 0;
                
                // Apply shadow quality
                QualitySettings.shadowResolution = (ShadowResolution)Mathf.Clamp(profile.shadowQuality, 0, 3);
                
                // Apply anti-aliasing
                QualitySettings.antiAliasing = profile.antiAliasing;
                
                // Apply texture quality
                QualitySettings.globalTextureMipmapLimit = Mathf.Max(0, 3 - profile.textureQuality);
                
                // Apply particle raycast budget
                QualitySettings.particleRaycastBudget = profile.particleRaycastBudget;
                
                // Apply LOD bias
                QualitySettings.lodBias = profile.lodBias;
                
                Debug.Log($"[AdaptivePerformanceManager] Applied Unity settings: Quality={profile.unityQualityLevel}, FPS={profile.targetFrameRate}, VSync={profile.enableVSync}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdaptivePerformanceManager] Failed to apply Unity settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Handle viewport context changes
        /// </summary>
        private static void OnViewportContextChanged(ViewportContext newContext)
        {
            Debug.Log($"[AdaptivePerformanceManager] Viewport context changed: {newContext}");
            ApplyConfiguration(newContext);
        }
        
        /// <summary>
        /// Handle performance metrics updates
        /// </summary>
        private static void OnPerformanceMetricsUpdated(PerformanceMetrics metrics)
        {
            // This can be used for more sophisticated performance analysis
            // Currently handled by CheckForPerformanceAdjustment
        }
        
        /// <summary>
        /// Handle FPS changes
        /// </summary>
        private static void OnFPSChanged(float newFPS)
        {
            // Trigger immediate adjustment check on significant FPS changes
            if (autoAdjustmentEnabled && CanAdjust())
            {
                CheckForPerformanceAdjustment();
            }
        }
        
        /// <summary>
        /// Check if performance adjustment is needed and apply it
        /// </summary>
        private static void CheckForPerformanceAdjustment()
        {
            var recommendedLevel = PerformanceMonitor.GetRecommendedPerformanceLevel();
            
            // Only adjust if recommendation differs significantly from current level
            if (ShouldAdjustPerformance(recommendedLevel))
            {
                AdjustPerformanceLevel(recommendedLevel);
                lastAdjustmentTime = Time.unscaledTime;
            }
        }
        
        /// <summary>
        /// Determine if performance adjustment is needed
        /// </summary>
        private static bool ShouldAdjustPerformance(PerformanceLevel recommendedLevel)
        {
            var currentLevel = currentProfile.level;
            
            // Apply strategy-based adjustment logic
            return adjustmentStrategy switch
            {
                AdjustmentStrategy.Conservative => Math.Abs((int)recommendedLevel - (int)currentLevel) >= 2,
                AdjustmentStrategy.Balanced => Math.Abs((int)recommendedLevel - (int)currentLevel) >= 1,
                AdjustmentStrategy.Aggressive => recommendedLevel != currentLevel,
                _ => false
            };
        }
        
        /// <summary>
        /// Adjust performance level while maintaining viewport-specific settings
        /// </summary>
        private static void AdjustPerformanceLevel(PerformanceLevel newLevel)
        {
            // Create adjusted profile based on current profile but with new performance level
            var adjustedProfile = new PerformanceProfile(newLevel, (int)newLevel);
            
            // Preserve some viewport-specific settings if needed
            // This could be enhanced to be more sophisticated
            
            ApplyProfile(adjustedProfile);
            
            Debug.Log($"[AdaptivePerformanceManager] Performance adjusted from {currentProfile.level} to {newLevel}");
        }
        
        /// <summary>
        /// Check if adjustment is allowed (cooldown period)
        /// </summary>
        private static bool CanAdjust()
        {
            return Time.unscaledTime - lastAdjustmentTime >= adjustmentCooldown;
        }
        
        /// <summary>
        /// Apply fallback configuration when ViewportManager is not available
        /// </summary>
        private static void ApplyFallbackConfiguration()
        {
            var fallbackProfile = new PerformanceProfile(PerformanceLevel.Medium, 3);
            ApplyProfile(fallbackProfile);
            Debug.Log("[AdaptivePerformanceManager] Applied fallback configuration");
        }
        
        /// <summary>
        /// Create default configuration matrix
        /// </summary>
        private static AdaptivePerformanceMatrix CreateDefaultMatrix()
        {
            var matrix = ScriptableObject.CreateInstance<AdaptivePerformanceMatrix>();
            // The matrix will initialize itself with default values
            return matrix;
        }
        
        /// <summary>
        /// Set configuration matrix
        /// </summary>
        public static void SetConfigurationMatrix(AdaptivePerformanceMatrix matrix)
        {
            configurationMatrix = matrix;
            
            if (isInitialized && Energy8.ViewportManager.ViewportManager.IsInitialized)
            {
                ApplyConfiguration(Energy8.ViewportManager.ViewportManager.CurrentContext);
            }
            
            Debug.Log("[AdaptivePerformanceManager] Configuration matrix updated");
        }
        
        /// <summary>
        /// Set adjustment strategy
        /// </summary>
        public static void SetAdjustmentStrategy(AdjustmentStrategy strategy)
        {
            adjustmentStrategy = strategy;
            Debug.Log($"[AdaptivePerformanceManager] Adjustment strategy set to: {strategy}");
        }
        
        /// <summary>
        /// Enable or disable automatic performance adjustment
        /// </summary>
        public static void SetAutoAdjustment(bool enabled)
        {
            autoAdjustmentEnabled = enabled;
            Debug.Log($"[AdaptivePerformanceManager] Auto adjustment {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Force refresh of current configuration
        /// </summary>
        public static void RefreshConfiguration()
        {
            if (!isInitialized)
                return;
                
            if (Energy8.ViewportManager.ViewportManager.IsInitialized)
            {
                ApplyConfiguration(Energy8.ViewportManager.ViewportManager.CurrentContext);
            }
            else
            {
                ApplyFallbackConfiguration();
            }
        }
        
        /// <summary>
        /// Get debug information
        /// </summary>
        public static string GetDebugInfo()
        {
            if (!isInitialized)
                return "AdaptivePerformanceManager: Not initialized";
                
            var perfInfo = PerformanceMonitor.GetDebugInfo();
            return $"AdaptivePerformanceManager: {currentProfile}, Strategy={adjustmentStrategy}, AutoAdjust={autoAdjustmentEnabled}\n{perfInfo}";
        }
        
        /// <summary>
        /// Shutdown the adaptive performance system
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            // Unsubscribe from events
            if (Energy8.ViewportManager.ViewportManager.IsInitialized)
            {
                Energy8.ViewportManager.ViewportManager.OnContextChanged -= OnViewportContextChanged;
            }
            
            PerformanceMonitor.OnMetricsUpdated -= OnPerformanceMetricsUpdated;
            PerformanceMonitor.OnFPSChanged -= OnFPSChanged;
            
            // Shutdown performance monitoring
            PerformanceMonitor.Shutdown();
            
            isInitialized = false;
            
            Debug.Log("[AdaptivePerformanceManager] Shutdown");
        }
    }
}