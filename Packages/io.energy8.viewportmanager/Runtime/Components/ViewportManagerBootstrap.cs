using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// MonoBehaviour that automatically initializes and monitors the viewport manager
    /// </summary>
    public class ViewportManagerBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        [Header("Monitoring")]
        [SerializeField] private bool enableContinuousDetection = true;
        [SerializeField] private float detectionInterval = 1.0f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private KeyCode debugInfoKey = KeyCode.F12;

        private float lastDetectionTime;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeViewportManager();
            }

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (!initializeOnAwake)
            {
                InitializeViewportManager();
            }
        }

        private void Update()
        {
            // Continuous detection
            if (enableContinuousDetection && Time.time - lastDetectionTime > detectionInterval)
            {
                ViewportManager.RefreshContext();
                lastDetectionTime = Time.time;
            }

            // Debug info shortcut
            if (enableDebugLogging && IsDebugKeyPressed())
            {
                Debug.Log(ViewportManager.GetSystemInfo());
            }
        }

        /// <summary>
        /// Initialize the viewport manager
        /// </summary>
        public void InitializeViewportManager()
        {
            if (ViewportManager.IsInitialized)
            {
                if (enableDebugLogging)
                {
                    Debug.Log("ViewportManager already initialized");
                }
                return;
            }

            // Initialize
            ViewportManager.Initialize();

            if (enableDebugLogging)
            {
                Debug.Log($"ViewportManager initialized by {gameObject.name}");
                Debug.Log(ViewportManager.GetSystemInfo());
            }

            // Subscribe to events for logging
            if (enableDebugLogging)
            {
                ViewportManager.OnContextChanged += (context) => 
                    Debug.Log($"Context changed: {context}");
                    
                ViewportManager.OnOrientationChanged += (orientation) => 
                    Debug.Log($"Orientation changed: {orientation}");
                    
                ViewportManager.OnScreenSizeChanged += (width, height) => 
                    Debug.Log($"Screen size changed: {width}x{height}");
            }
        }

        /// <summary>
        /// Force refresh viewport detection
        /// </summary>
        [ContextMenu("Refresh Viewport")]
        public void RefreshViewport()
        {
            ViewportManager.RefreshContext();
        }

        /// <summary>
        /// Print debug information
        /// </summary>
        [ContextMenu("Print Debug Info")]
        public void PrintDebugInfo()
        {
            Debug.Log(ViewportManager.GetSystemInfo());
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Refresh on focus change (useful for WebGL)
            if (hasFocus && enableContinuousDetection)
            {
                ViewportManager.RefreshContext();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Refresh when unpausing (useful for mobile)
            if (!pauseStatus && enableContinuousDetection)
            {
                ViewportManager.RefreshContext();
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp detection interval
            detectionInterval = Mathf.Max(0.1f, detectionInterval);
        }
        #endif

        /// <summary>
        /// Safely checks if debug key is pressed, compatible with both input systems
        /// </summary>
        private bool IsDebugKeyPressed()
        {
            // Check if we're using the new Input System
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // New Input System is enabled and Legacy Input is disabled
            // For now, skip debug key functionality
            // In future, implement Input System support here
            return false;
#else
            try
            {
                // Try using legacy input system
                return Input.GetKeyDown(debugInfoKey);
            }
            catch (System.InvalidOperationException)
            {
                // Legacy input is disabled, Input System is active
                // For now, skip debug key functionality
                return false;
            }
            catch (System.Exception)
            {
                // Any other input system error, silently fail
                return false;
            }
#endif
        }
    }
}