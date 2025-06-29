using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Simple component for testing viewport orientation detection
    /// Logs orientation changes without applying graphics settings
    /// </summary>
    public class ViewportOrientationTester : MonoBehaviour
    {
        [Header("Testing")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private float checkInterval = 1.0f;
        
        private ViewportContext lastContext;
        private float lastCheckTime;

        private void Start()
        {
            // Get initial context
            lastContext = ViewportDetector.DetectContext();
            
            if (enableLogging)
            {
                Debug.Log($"[OrientationTester] Initial context: {lastContext}");
                LogDetailedInfo();
            }
        }

        private void Update()
        {
            // Check for changes periodically
            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckForChanges();
                lastCheckTime = Time.time;
            }
        }

        private void CheckForChanges()
        {
            var currentContext = ViewportDetector.DetectContext();
            
            if (!currentContext.Equals(lastContext))
            {
                if (enableLogging)
                {
                    Debug.Log($"[OrientationTester] Context changed!");
                    Debug.Log($"  FROM: {lastContext}");
                    Debug.Log($"  TO:   {currentContext}");
                    
                    // Check what specifically changed
                    if (lastContext.orientation != currentContext.orientation)
                        Debug.Log($"  → Orientation: {lastContext.orientation} → {currentContext.orientation}");
                    if (lastContext.deviceType != currentContext.deviceType)
                        Debug.Log($"  → Device: {lastContext.deviceType} → {currentContext.deviceType}");
                    if (lastContext.platform != currentContext.platform)
                        Debug.Log($"  → Platform: {lastContext.platform} → {currentContext.platform}");
                }
                
                lastContext = currentContext;
            }
        }

        private void LogDetailedInfo()
        {
            var info = ViewportDetector.DetectViewport();
            var recommendedQuality = ViewportDetector.GetRecommendedQualityLevel();
            
            Debug.Log($"[OrientationTester] Detailed info:");
            Debug.Log($"  Device: {info.deviceType}");
            Debug.Log($"  Platform: {info.platform}");
            Debug.Log($"  Orientation: {info.orientation}");
            Debug.Log($"  Screen: {info.screenWidth}x{info.screenHeight}");
            Debug.Log($"  Recommended Quality: {recommendedQuality}");
            Debug.Log($"  Unity Screen: {Screen.width}x{Screen.height}");
            Debug.Log($"  Unity Orientation: {Screen.orientation}");
        }

        [ContextMenu("Log Current Info")]
        public void LogCurrentInfo()
        {
            LogDetailedInfo();
        }

        [ContextMenu("Force Check")]
        public void ForceCheck()
        {
            CheckForChanges();
        }
    }
}
