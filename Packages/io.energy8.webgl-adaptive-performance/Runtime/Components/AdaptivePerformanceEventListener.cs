using UnityEngine;
using UnityEngine.Events;
using Energy8.WebGL.AdaptivePerformance.Core;
using Energy8.ViewportManager.Core;

namespace Energy8.WebGL.AdaptivePerformance.Components
{
    /// <summary>
    /// Abstract base class for listening to Adaptive Performance events
    /// </summary>
    public abstract class AdaptivePerformanceEventListener : MonoBehaviour
    {
        [Header("Event Subscription")]
        [SerializeField] protected bool subscribeOnStart = true;
        [SerializeField] protected bool unsubscribeOnDestroy = true;
        
        protected virtual void Start()
        {
            if (subscribeOnStart)
            {
                SubscribeToEvents();
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (unsubscribeOnDestroy)
            {
                UnsubscribeFromEvents();
            }
        }
        
        /// <summary>
        /// Subscribe to Adaptive Performance events
        /// </summary>
        public virtual void SubscribeToEvents()
        {
            AdaptivePerformanceManager.OnProfileChanged += OnProfileChanged;
            AdaptivePerformanceManager.OnPerformanceLevelChanged += OnPerformanceLevelChanged;
            AdaptivePerformanceManager.OnConfigurationApplied += OnConfigurationApplied;
            AdaptivePerformanceManager.OnInitialized += OnAdaptivePerformanceInitialized;
            
            PerformanceMonitor.OnMetricsUpdated += OnPerformanceMetricsUpdated;
            PerformanceMonitor.OnFPSChanged += OnFPSChanged;
            PerformanceMonitor.OnThermalStateChanged += OnThermalStateChanged;
        }
        
        /// <summary>
        /// Unsubscribe from Adaptive Performance events
        /// </summary>
        public virtual void UnsubscribeFromEvents()
        {
            AdaptivePerformanceManager.OnProfileChanged -= OnProfileChanged;
            AdaptivePerformanceManager.OnPerformanceLevelChanged -= OnPerformanceLevelChanged;
            AdaptivePerformanceManager.OnConfigurationApplied -= OnConfigurationApplied;
            AdaptivePerformanceManager.OnInitialized -= OnAdaptivePerformanceInitialized;
            
            PerformanceMonitor.OnMetricsUpdated -= OnPerformanceMetricsUpdated;
            PerformanceMonitor.OnFPSChanged -= OnFPSChanged;
            PerformanceMonitor.OnThermalStateChanged -= OnThermalStateChanged;
        }
        
        // Abstract event handlers
        protected abstract void OnProfileChanged(PerformanceProfile profile);
        protected abstract void OnPerformanceLevelChanged(PerformanceLevel level);
        protected abstract void OnConfigurationApplied(ViewportContext context, PerformanceProfile profile);
        protected abstract void OnAdaptivePerformanceInitialized();
        protected abstract void OnPerformanceMetricsUpdated(PerformanceMetrics metrics);
        protected abstract void OnFPSChanged(float fps);
        protected abstract void OnThermalStateChanged(ThermalState thermalState);
    }
    
    /// <summary>
    /// Basic implementation that exposes Adaptive Performance events as Unity Events
    /// </summary>
    public class BasicAdaptivePerformanceEventListener : AdaptivePerformanceEventListener
    {
        [Header("Unity Events")]
        [SerializeField] private UnityEvent onAdaptivePerformanceInitialized;
        [SerializeField] private UnityEvent<PerformanceLevel> onPerformanceLevelChanged;
        [SerializeField] private UnityEvent<float> onFPSChanged;
        [SerializeField] private UnityEvent<ThermalState> onThermalStateChanged;
        
        [Header("Performance Level Events")]
        [SerializeField] private UnityEvent onVeryLowPerformance;
        [SerializeField] private UnityEvent onLowPerformance;
        [SerializeField] private UnityEvent onMediumPerformance;
        [SerializeField] private UnityEvent onHighPerformance;
        [SerializeField] private UnityEvent onVeryHighPerformance;
        [SerializeField] private UnityEvent onUltraPerformance;
        
        [Header("FPS Threshold Events")]
        [SerializeField] private float lowFPSThreshold = 30f;
        [SerializeField] private float highFPSThreshold = 60f;
        [SerializeField] private UnityEvent onLowFPS;
        [SerializeField] private UnityEvent onGoodFPS;
        [SerializeField] private UnityEvent onHighFPS;
        
        [Header("Thermal Events")]
        [SerializeField] private UnityEvent onThermalNominal;
        [SerializeField] private UnityEvent onThermalFair;
        [SerializeField] private UnityEvent onThermalSerious;
        [SerializeField] private UnityEvent onThermalCritical;
        
        protected override void OnProfileChanged(PerformanceProfile profile)
        {
            // Profile changed - could be used for custom logic
        }
        
        protected override void OnPerformanceLevelChanged(PerformanceLevel level)
        {
            onPerformanceLevelChanged?.Invoke(level);
            
            // Invoke specific performance level events
            switch (level)
            {
                case PerformanceLevel.VeryLow:
                    onVeryLowPerformance?.Invoke();
                    break;
                case PerformanceLevel.Low:
                    onLowPerformance?.Invoke();
                    break;
                case PerformanceLevel.Medium:
                    onMediumPerformance?.Invoke();
                    break;
                case PerformanceLevel.High:
                    onHighPerformance?.Invoke();
                    break;
                case PerformanceLevel.VeryHigh:
                    onVeryHighPerformance?.Invoke();
                    break;
                case PerformanceLevel.Ultra:
                    onUltraPerformance?.Invoke();
                    break;
            }
        }
        
        protected override void OnConfigurationApplied(ViewportContext context, PerformanceProfile profile)
        {
            // Configuration applied - could be used for custom logic
        }
        
        protected override void OnAdaptivePerformanceInitialized()
        {
            onAdaptivePerformanceInitialized?.Invoke();
        }
        
        protected override void OnPerformanceMetricsUpdated(PerformanceMetrics metrics)
        {
            // Metrics updated - could be used for custom logic
        }
        
        protected override void OnFPSChanged(float fps)
        {
            onFPSChanged?.Invoke(fps);
            
            // Invoke FPS threshold events
            if (fps < lowFPSThreshold)
            {
                onLowFPS?.Invoke();
            }
            else if (fps >= highFPSThreshold)
            {
                onHighFPS?.Invoke();
            }
            else
            {
                onGoodFPS?.Invoke();
            }
        }
        
        protected override void OnThermalStateChanged(ThermalState thermalState)
        {
            onThermalStateChanged?.Invoke(thermalState);
            
            // Invoke specific thermal state events
            switch (thermalState)
            {
                case ThermalState.Nominal:
                    onThermalNominal?.Invoke();
                    break;
                case ThermalState.Fair:
                    onThermalFair?.Invoke();
                    break;
                case ThermalState.Serious:
                    onThermalSerious?.Invoke();
                    break;
                case ThermalState.Critical:
                    onThermalCritical?.Invoke();
                    break;
            }
        }
    }
    
    /// <summary>
    /// Advanced event listener with performance analysis capabilities
    /// </summary>
    public class AdvancedAdaptivePerformanceEventListener : AdaptivePerformanceEventListener
    {
        [Header("Performance Analysis")]
        [SerializeField] private bool enablePerformanceLogging = true;
        [SerializeField] private float performanceLogInterval = 5f;
        [SerializeField] private bool trackPerformanceHistory = true;
        [SerializeField] private int maxHistoryEntries = 100;
        
        [Header("Performance Alerts")]
        [SerializeField] private float criticalFPSThreshold = 20f;
        [SerializeField] private float excellentFPSThreshold = 90f;
        [SerializeField] private UnityEvent onCriticalPerformance;
        [SerializeField] private UnityEvent onExcellentPerformance;
        
        private float lastLogTime;
        private System.Collections.Generic.List<PerformanceMetrics> performanceHistory;
        
        protected override void Start()
        {
            base.Start();
            
            if (trackPerformanceHistory)
            {
                performanceHistory = new System.Collections.Generic.List<PerformanceMetrics>();
            }
            
            lastLogTime = Time.time;
        }
        
        protected override void OnProfileChanged(PerformanceProfile profile)
        {
            if (enablePerformanceLogging)
            {
                Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Performance profile changed: {profile}");
            }
        }
        
        protected override void OnPerformanceLevelChanged(PerformanceLevel level)
        {
            if (enablePerformanceLogging)
            {
                Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Performance level changed: {level}");
            }
        }
        
        protected override void OnConfigurationApplied(ViewportContext context, PerformanceProfile profile)
        {
            if (enablePerformanceLogging)
            {
                Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Configuration applied for {context}: {profile}");
            }
        }
        
        protected override void OnAdaptivePerformanceInitialized()
        {
            if (enablePerformanceLogging)
            {
                Debug.Log("[AdvancedAdaptivePerformanceEventListener] Adaptive Performance system initialized");
            }
        }
        
        protected override void OnPerformanceMetricsUpdated(PerformanceMetrics metrics)
        {
            // Track performance history
            if (trackPerformanceHistory)
            {
                performanceHistory.Add(metrics);
                
                // Maintain history size
                while (performanceHistory.Count > maxHistoryEntries)
                {
                    performanceHistory.RemoveAt(0);
                }
            }
            
            // Periodic performance logging
            if (enablePerformanceLogging && Time.time - lastLogTime >= performanceLogInterval)
            {
                LogPerformanceAnalysis(metrics);
                lastLogTime = Time.time;
            }
        }
        
        protected override void OnFPSChanged(float fps)
        {
            // Check for critical or excellent performance
            if (fps <= criticalFPSThreshold)
            {
                onCriticalPerformance?.Invoke();
                if (enablePerformanceLogging)
                {
                    Debug.LogWarning($"[AdvancedAdaptivePerformanceEventListener] Critical FPS detected: {fps:F1}");
                }
            }
            else if (fps >= excellentFPSThreshold)
            {
                onExcellentPerformance?.Invoke();
                if (enablePerformanceLogging)
                {
                    Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Excellent FPS detected: {fps:F1}");
                }
            }
        }
        
        protected override void OnThermalStateChanged(ThermalState thermalState)
        {
            if (enablePerformanceLogging)
            {
                Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Thermal state changed: {thermalState}");
            }
        }
        
        /// <summary>
        /// Log detailed performance analysis
        /// </summary>
        private void LogPerformanceAnalysis(PerformanceMetrics metrics)
        {
            var analysis = GetPerformanceAnalysis();
            Debug.Log($"[AdvancedAdaptivePerformanceEventListener] Performance Analysis:\n{analysis}");
        }
        
        /// <summary>
        /// Get comprehensive performance analysis
        /// </summary>
        public string GetPerformanceAnalysis()
        {
            if (!trackPerformanceHistory || performanceHistory.Count == 0)
                return "No performance history available";
                
            var latest = performanceHistory[performanceHistory.Count - 1];
            
            // Calculate averages
            float avgFPS = 0f;
            float avgFrameTime = 0f;
            float minFPS = float.MaxValue;
            float maxFPS = float.MinValue;
            
            foreach (var metrics in performanceHistory)
            {
                avgFPS += metrics.currentFPS;
                avgFrameTime += metrics.frameTime;
                minFPS = Mathf.Min(minFPS, metrics.currentFPS);
                maxFPS = Mathf.Max(maxFPS, metrics.currentFPS);
            }
            
            avgFPS /= performanceHistory.Count;
            avgFrameTime /= performanceHistory.Count;
            
            return $"Current: {latest}\n" +
                   $"Averages: FPS={avgFPS:F1}, FrameTime={avgFrameTime:F2}ms\n" +
                   $"Range: FPS {minFPS:F1}-{maxFPS:F1}\n" +
                   $"History: {performanceHistory.Count} entries\n" +
                   $"Stability: {(maxFPS - minFPS <= 10f ? "Stable" : "Unstable")}";
        }
        
        /// <summary>
        /// Clear performance history
        /// </summary>
        public void ClearPerformanceHistory()
        {
            if (performanceHistory != null)
            {
                performanceHistory.Clear();
            }
        }
    }
}