using System;
using System.Collections.Generic;
using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.WebGL.AdaptivePerformance.Core
{
    /// <summary>
    /// Configuration entry that maps viewport context to performance profile
    /// </summary>
    [Serializable]
    public struct AdaptivePerformanceEntry
    {
        public Energy8.ViewportManager.Core.ScreenOrientation orientation;
        public Energy8.ViewportManager.Core.DeviceType deviceType;
        public Energy8.ViewportManager.Core.Platform platform;
        public PerformanceProfile profile;

        public AdaptivePerformanceEntry(Energy8.ViewportManager.Core.ScreenOrientation orientation, Energy8.ViewportManager.Core.DeviceType deviceType, Energy8.ViewportManager.Core.Platform platform, PerformanceProfile profile)
        {
            this.orientation = orientation;
            this.deviceType = deviceType;
            this.platform = platform;
            this.profile = profile;
        }

        public bool Matches(ViewportContext context)
        {
            return (orientation == Energy8.ViewportManager.Core.ScreenOrientation.Any || orientation == context.orientation) &&
                   (deviceType == Energy8.ViewportManager.Core.DeviceType.Any || deviceType == context.deviceType) &&
                   (platform == Energy8.ViewportManager.Core.Platform.Any || platform == context.platform);
        }

        public override string ToString()
        {
            return $"Entry({orientation}, {deviceType}, {platform}) -> {profile}";
        }
    }

    /// <summary>
    /// ScriptableObject that defines performance profiles for different viewport contexts
    /// </summary>
    [CreateAssetMenu(fileName = "AdaptivePerformanceMatrix", menuName = "Energy8/Adaptive Performance Matrix")]
    public class AdaptivePerformanceMatrix : ScriptableObject
    {
        [Header("Performance Configuration")]
        [SerializeField] private List<AdaptivePerformanceEntry> entries = new List<AdaptivePerformanceEntry>();
        
        [Header("Fallback Settings")]
        [SerializeField] private PerformanceProfile fallbackProfile = new PerformanceProfile(PerformanceLevel.Medium, 3);
        
        [Header("Debug Information")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private string lastMatchedEntry = "None";
        [SerializeField] private string currentProfile = "None";

        /// <summary>
        /// Get performance profile for the given viewport context
        /// </summary>
        public PerformanceProfile GetProfile(ViewportContext context)
        {
            // Find the most specific match
            AdaptivePerformanceEntry? bestMatch = null;
            int bestSpecificity = -1;

            foreach (var entry in entries)
            {
                if (entry.Matches(context))
                {
                    int specificity = CalculateSpecificity(entry);
                    if (specificity > bestSpecificity)
                    {
                        bestMatch = entry;
                        bestSpecificity = specificity;
                    }
                }
            }

            if (bestMatch.HasValue)
            {
                var match = bestMatch.Value;
                if (enableDebugLogging)
                {
                    lastMatchedEntry = match.ToString();
                    currentProfile = match.profile.ToString();
                    Debug.Log($"[AdaptivePerformanceMatrix] Matched: {match}");
                }
                return match.profile;
            }

            // No match found, use fallback
            if (enableDebugLogging)
            {
                lastMatchedEntry = "Fallback";
                currentProfile = fallbackProfile.ToString();
                Debug.Log($"[AdaptivePerformanceMatrix] No match found for {context}, using fallback: {fallbackProfile}");
            }
            
            return fallbackProfile;
        }

        /// <summary>
        /// Calculate specificity score for an entry (higher = more specific)
        /// </summary>
        private int CalculateSpecificity(AdaptivePerformanceEntry entry)
        {
            int score = 0;
            if (entry.orientation != Energy8.ViewportManager.Core.ScreenOrientation.Any) score += 4;
            if (entry.deviceType != Energy8.ViewportManager.Core.DeviceType.Any) score += 2;
            if (entry.platform != Energy8.ViewportManager.Core.Platform.Any) score += 1;
            return score;
        }

        /// <summary>
        /// Add a new performance entry
        /// </summary>
        public void AddEntry(Energy8.ViewportManager.Core.ScreenOrientation orientation, Energy8.ViewportManager.Core.DeviceType deviceType, Energy8.ViewportManager.Core.Platform platform, PerformanceProfile profile)
        {
            var entry = new AdaptivePerformanceEntry(orientation, deviceType, platform, profile);
            entries.Add(entry);
        }

        /// <summary>
        /// Remove all entries
        /// </summary>
        public void ClearEntries()
        {
            entries.Clear();
        }

        /// <summary>
        /// Get all entries (read-only)
        /// </summary>
        public IReadOnlyList<AdaptivePerformanceEntry> GetEntries()
        {
            return entries.AsReadOnly();
        }

        /// <summary>
        /// Initialize with default performance profiles
        /// </summary>
        private void OnEnable()
        {
            if (entries.Count == 0)
            {
                InitializeDefaultProfiles();
            }
        }

        /// <summary>
        /// Set up default performance profiles for common scenarios
        /// </summary>
        private void InitializeDefaultProfiles()
        {
            // Mobile profiles - more conservative
            var mobileLowProfile = new PerformanceProfile(PerformanceLevel.Low, 1);
            var mobileMediumProfile = new PerformanceProfile(PerformanceLevel.Medium, 2);
            
            // Desktop profiles - more aggressive
            var desktopMediumProfile = new PerformanceProfile(PerformanceLevel.Medium, 3);
            var desktopHighProfile = new PerformanceProfile(PerformanceLevel.High, 4);
            
            // WebGL profiles - balanced
            var webglProfile = new PerformanceProfile(PerformanceLevel.Medium, 2);

            // Mobile configurations
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Portrait, Energy8.ViewportManager.Core.DeviceType.Mobile, Energy8.ViewportManager.Core.Platform.Any, mobileLowProfile);
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Landscape, Energy8.ViewportManager.Core.DeviceType.Mobile, Energy8.ViewportManager.Core.Platform.Any, mobileMediumProfile);
            
            // Desktop configurations
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Any, Energy8.ViewportManager.Core.DeviceType.Desktop, Energy8.ViewportManager.Core.Platform.Windows, desktopHighProfile);
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Any, Energy8.ViewportManager.Core.DeviceType.Desktop, Energy8.ViewportManager.Core.Platform.macOS, desktopHighProfile);
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Any, Energy8.ViewportManager.Core.DeviceType.Desktop, Energy8.ViewportManager.Core.Platform.Linux, desktopMediumProfile);
            
            // WebGL configurations
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Any, Energy8.ViewportManager.Core.DeviceType.Any, Energy8.ViewportManager.Core.Platform.WebGL, webglProfile);
            
            // Tablet configurations
            AddEntry(Energy8.ViewportManager.Core.ScreenOrientation.Any, Energy8.ViewportManager.Core.DeviceType.Tablet, Energy8.ViewportManager.Core.Platform.Any, mobileMediumProfile);

            if (enableDebugLogging)
            {
                Debug.Log($"[AdaptivePerformanceMatrix] Initialized with {entries.Count} default profiles");
            }
        }

        /// <summary>
        /// Get debug information about current configuration
        /// </summary>
        public string GetDebugInfo()
        {
            return $"AdaptivePerformanceMatrix: {entries.Count} entries, Last Match: {lastMatchedEntry}, Current: {currentProfile}";
        }
    }
}