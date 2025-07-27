using System;

namespace Energy8.WebGL.AdaptivePerformance.Core
{
    /// <summary>
    /// Performance level classification
    /// </summary>
    public enum PerformanceLevel
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4,
        Ultra = 5
    }

    /// <summary>
    /// Performance metrics categories
    /// </summary>
    public enum PerformanceMetric
    {
        FrameRate,
        FrameTime,
        GPUMemory,
        CPUUsage,
        BatteryLevel,
        ThermalState
    }

    /// <summary>
    /// Thermal state of the device
    /// </summary>
    public enum ThermalState
    {
        Nominal = 0,
        Fair = 1,
        Serious = 2,
        Critical = 3
    }

    /// <summary>
    /// Performance adjustment strategy
    /// </summary>
    public enum AdjustmentStrategy
    {
        Conservative,  // Slow, safe adjustments
        Balanced,      // Moderate adjustments
        Aggressive     // Fast, responsive adjustments
    }

    /// <summary>
    /// Performance configuration profile
    /// </summary>
    [Serializable]
    public struct PerformanceProfile
    {
        public PerformanceLevel level;
        public int unityQualityLevel;
        public int targetFrameRate;
        public bool enableVSync;
        public int shadowQuality;
        public int antiAliasing;
        public float renderScale;
        public bool enablePostProcessing;
        public int textureQuality;
        public int particleRaycastBudget;
        public int lodBias;

        public PerformanceProfile(PerformanceLevel level, int unityQualityLevel)
        {
            this.level = level;
            this.unityQualityLevel = unityQualityLevel;
            this.targetFrameRate = GetDefaultFrameRate(level);
            this.enableVSync = level >= PerformanceLevel.High;
            this.shadowQuality = (int)level;
            this.antiAliasing = GetDefaultAntiAliasing(level);
            this.renderScale = GetDefaultRenderScale(level);
            this.enablePostProcessing = level >= PerformanceLevel.Medium;
            this.textureQuality = (int)level;
            this.particleRaycastBudget = GetDefaultParticleBudget(level);
            this.lodBias = GetDefaultLODBias(level);
        }

        private static int GetDefaultFrameRate(PerformanceLevel level)
        {
            return level switch
            {
                PerformanceLevel.VeryLow => 30,
                PerformanceLevel.Low => 30,
                PerformanceLevel.Medium => 60,
                PerformanceLevel.High => 60,
                PerformanceLevel.VeryHigh => 120,
                PerformanceLevel.Ultra => 144,
                _ => 60
            };
        }

        private static int GetDefaultAntiAliasing(PerformanceLevel level)
        {
            return level switch
            {
                PerformanceLevel.VeryLow => 0,
                PerformanceLevel.Low => 0,
                PerformanceLevel.Medium => 2,
                PerformanceLevel.High => 4,
                PerformanceLevel.VeryHigh => 8,
                PerformanceLevel.Ultra => 8,
                _ => 0
            };
        }

        private static float GetDefaultRenderScale(PerformanceLevel level)
        {
            return level switch
            {
                PerformanceLevel.VeryLow => 0.5f,
                PerformanceLevel.Low => 0.75f,
                PerformanceLevel.Medium => 1.0f,
                PerformanceLevel.High => 1.0f,
                PerformanceLevel.VeryHigh => 1.25f,
                PerformanceLevel.Ultra => 1.5f,
                _ => 1.0f
            };
        }

        private static int GetDefaultParticleBudget(PerformanceLevel level)
        {
            return level switch
            {
                PerformanceLevel.VeryLow => 16,
                PerformanceLevel.Low => 32,
                PerformanceLevel.Medium => 64,
                PerformanceLevel.High => 128,
                PerformanceLevel.VeryHigh => 256,
                PerformanceLevel.Ultra => 512,
                _ => 64
            };
        }

        private static int GetDefaultLODBias(PerformanceLevel level)
        {
            return level switch
            {
                PerformanceLevel.VeryLow => 2,
                PerformanceLevel.Low => 1,
                PerformanceLevel.Medium => 1,
                PerformanceLevel.High => 0,
                PerformanceLevel.VeryHigh => 0,
                PerformanceLevel.Ultra => 0,
                _ => 1
            };
        }

        public override string ToString()
        {
            return $"PerformanceProfile({level}, Unity:{unityQualityLevel}, FPS:{targetFrameRate}, Scale:{renderScale})";
        }
    }

    /// <summary>
    /// Real-time performance metrics
    /// </summary>
    [Serializable]
    public struct PerformanceMetrics
    {
        public float currentFPS;
        public float averageFPS;
        public float frameTime;
        public float cpuUsage;
        public float gpuUsage;
        public long usedMemory;
        public long totalMemory;
        public float batteryLevel;
        public ThermalState thermalState;
        public float timestamp;

        public PerformanceMetrics(float fps, float frameTime, float cpuUsage = 0f, float gpuUsage = 0f)
        {
            this.currentFPS = fps;
            this.averageFPS = fps;
            this.frameTime = frameTime;
            this.cpuUsage = cpuUsage;
            this.gpuUsage = gpuUsage;
            this.usedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory();
            this.totalMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemory();
            this.batteryLevel = UnityEngine.SystemInfo.batteryLevel;
            this.thermalState = ThermalState.Nominal;
            this.timestamp = UnityEngine.Time.time;
        }

        public override string ToString()
        {
            return $"Metrics(FPS:{currentFPS:F1}, FrameTime:{frameTime:F2}ms, CPU:{cpuUsage:F1}%, Memory:{usedMemory / 1024 / 1024}MB)";
        }
    }
}