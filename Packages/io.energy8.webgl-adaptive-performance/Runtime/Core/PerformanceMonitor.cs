using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Energy8.WebGL.AdaptivePerformance.Core
{
    /// <summary>
    /// Monitors real-time performance metrics and provides performance data
    /// </summary>
    public static class PerformanceMonitor
    {
        private static readonly Queue<float> frameTimeHistory = new Queue<float>();
        private static readonly Queue<float> fpsHistory = new Queue<float>();
        private static float lastFrameTime;
        private static float accumulatedFrameTime;
        private static int frameCount;
        private static float lastUpdateTime;
        private static PerformanceMetrics lastMetrics;
        
        // Configuration
        private const int MaxHistorySize = 60; // Keep 60 frames of history
        private const float UpdateInterval = 0.5f; // Update metrics every 0.5 seconds
        
        // Events
        public static event Action<PerformanceMetrics> OnMetricsUpdated;
        public static event Action<float> OnFPSChanged;
        public static event Action<ThermalState> OnThermalStateChanged;
        
        // Properties
        public static bool IsInitialized { get; private set; }
        public static PerformanceMetrics CurrentMetrics => lastMetrics;
        public static float AverageFPS { get; private set; }
        public static float AverageFrameTime { get; private set; }
        public static float CurrentFPS { get; private set; }
        public static float CurrentFrameTime { get; private set; }
        
        /// <summary>
        /// Initialize the performance monitor
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized)
                return;
                
            frameTimeHistory.Clear();
            fpsHistory.Clear();
            lastFrameTime = Time.unscaledDeltaTime;
            accumulatedFrameTime = 0f;
            frameCount = 0;
            lastUpdateTime = Time.unscaledTime;
            
            IsInitialized = true;
            
            Debug.Log("[PerformanceMonitor] Initialized");
        }
        
        /// <summary>
        /// Update performance metrics (should be called every frame)
        /// </summary>
        public static void Update()
        {
            if (!IsInitialized)
                return;
                
            float currentTime = Time.unscaledTime;
            float deltaTime = Time.unscaledDeltaTime;
            
            // Update frame timing
            CurrentFrameTime = deltaTime * 1000f; // Convert to milliseconds
            CurrentFPS = deltaTime > 0 ? 1f / deltaTime : 0f;
            
            // Add to history
            frameTimeHistory.Enqueue(CurrentFrameTime);
            fpsHistory.Enqueue(CurrentFPS);
            
            // Maintain history size
            while (frameTimeHistory.Count > MaxHistorySize)
                frameTimeHistory.Dequeue();
            while (fpsHistory.Count > MaxHistorySize)
                fpsHistory.Dequeue();
                
            // Calculate averages
            CalculateAverages();
            
            // Accumulate for periodic updates
            accumulatedFrameTime += deltaTime;
            frameCount++;
            
            // Periodic metrics update
            if (currentTime - lastUpdateTime >= UpdateInterval)
            {
                UpdateMetrics();
                lastUpdateTime = currentTime;
            }
        }
        
        /// <summary>
        /// Calculate average values from history
        /// </summary>
        private static void CalculateAverages()
        {
            if (frameTimeHistory.Count == 0)
                return;
                
            float totalFrameTime = 0f;
            float totalFPS = 0f;
            
            foreach (float frameTime in frameTimeHistory)
                totalFrameTime += frameTime;
            foreach (float fps in fpsHistory)
                totalFPS += fps;
                
            AverageFrameTime = totalFrameTime / frameTimeHistory.Count;
            AverageFPS = totalFPS / fpsHistory.Count;
        }
        
        /// <summary>
        /// Update comprehensive performance metrics
        /// </summary>
        private static void UpdateMetrics()
        {
            var newMetrics = new PerformanceMetrics(
                CurrentFPS,
                CurrentFrameTime,
                GetCPUUsage(),
                GetGPUUsage()
            );
            
            // Update average FPS in metrics
            newMetrics.averageFPS = AverageFPS;
            
            // Check for significant FPS changes
            if (Math.Abs(newMetrics.currentFPS - lastMetrics.currentFPS) > 5f)
            {
                OnFPSChanged?.Invoke(newMetrics.currentFPS);
            }
            
            // Check for thermal state changes
            if (newMetrics.thermalState != lastMetrics.thermalState)
            {
                OnThermalStateChanged?.Invoke(newMetrics.thermalState);
            }
            
            lastMetrics = newMetrics;
            OnMetricsUpdated?.Invoke(newMetrics);
        }
        
        /// <summary>
        /// Get estimated CPU usage (simplified)
        /// </summary>
        private static float GetCPUUsage()
        {
            // This is a simplified estimation based on frame time
            // In a real implementation, you might use platform-specific APIs
            float targetFrameTime = 1000f / Application.targetFrameRate;
            float usage = (CurrentFrameTime / targetFrameTime) * 100f;
            return Mathf.Clamp(usage, 0f, 100f);
        }
        
        /// <summary>
        /// Get estimated GPU usage (simplified)
        /// </summary>
        private static float GetGPUUsage()
        {
            // This is a placeholder - actual GPU usage requires platform-specific implementation
            // For WebGL, this information is typically not available
            return 0f;
        }
        
        /// <summary>
        /// Get performance level recommendation based on current metrics
        /// </summary>
        public static PerformanceLevel GetRecommendedPerformanceLevel()
        {
            if (!IsInitialized)
                return PerformanceLevel.Medium;
                
            float avgFPS = AverageFPS;
            float targetFPS = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
            
            // Calculate performance ratio
            float performanceRatio = avgFPS / targetFPS;
            
            // Recommend performance level based on current performance
            if (performanceRatio >= 1.2f)
                return PerformanceLevel.VeryHigh; // Running well above target
            else if (performanceRatio >= 1.0f)
                return PerformanceLevel.High; // Meeting target
            else if (performanceRatio >= 0.8f)
                return PerformanceLevel.Medium; // Slightly below target
            else if (performanceRatio >= 0.6f)
                return PerformanceLevel.Low; // Significantly below target
            else
                return PerformanceLevel.VeryLow; // Poor performance
        }
        
        /// <summary>
        /// Check if performance is stable
        /// </summary>
        public static bool IsPerformanceStable(float stabilityThreshold = 5f)
        {
            if (fpsHistory.Count < MaxHistorySize / 2)
                return false;
                
            float minFPS = float.MaxValue;
            float maxFPS = float.MinValue;
            
            foreach (float fps in fpsHistory)
            {
                minFPS = Mathf.Min(minFPS, fps);
                maxFPS = Mathf.Max(maxFPS, fps);
            }
            
            return (maxFPS - minFPS) <= stabilityThreshold;
        }
        
        /// <summary>
        /// Get debug information about performance monitoring
        /// </summary>
        public static string GetDebugInfo()
        {
            if (!IsInitialized)
                return "PerformanceMonitor: Not initialized";
                
            return $"PerformanceMonitor: FPS={CurrentFPS:F1} (avg={AverageFPS:F1}), FrameTime={CurrentFrameTime:F2}ms (avg={AverageFrameTime:F2}ms), Stable={IsPerformanceStable()}";
        }
        
        /// <summary>
        /// Reset all performance history
        /// </summary>
        public static void Reset()
        {
            frameTimeHistory.Clear();
            fpsHistory.Clear();
            accumulatedFrameTime = 0f;
            frameCount = 0;
            lastUpdateTime = Time.unscaledTime;
            
            Debug.Log("[PerformanceMonitor] Reset");
        }
        
        /// <summary>
        /// Shutdown the performance monitor
        /// </summary>
        public static void Shutdown()
        {
            if (!IsInitialized)
                return;
                
            Reset();
            IsInitialized = false;
            
            Debug.Log("[PerformanceMonitor] Shutdown");
        }
    }
}