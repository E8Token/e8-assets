using UnityEngine;
using Energy8.WebGL.AdaptivePerformance.Core;

namespace Energy8.WebGL.AdaptivePerformance.Components
{
    /// <summary>
    /// Bootstrap component for initializing and managing the Adaptive Performance system
    /// </summary>
    public class AdaptivePerformanceBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AdaptivePerformanceMatrix configurationMatrix;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        [Header("Performance Settings")]
        [SerializeField] private AdjustmentStrategy adjustmentStrategy = AdjustmentStrategy.Balanced;
        [SerializeField] private bool enableAutoAdjustment = true;
        [SerializeField] private float adjustmentCooldown = 2f;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private KeyCode debugInfoKey = KeyCode.F2;
        [SerializeField] private bool showDebugUI = false;
        
        // Debug UI
        private bool showDebugPanel = false;
        private Vector2 debugScrollPosition;
        private GUIStyle debugStyle;
        
        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeAdaptivePerformance();
            }
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        private void Start()
        {
            // Subscribe to events for debugging
            if (enableDebugLogging)
            {
                AdaptivePerformanceManager.OnProfileChanged += OnProfileChanged;
                AdaptivePerformanceManager.OnPerformanceLevelChanged += OnPerformanceLevelChanged;
                AdaptivePerformanceManager.OnConfigurationApplied += OnConfigurationApplied;
                AdaptivePerformanceManager.OnInitialized += OnAdaptivePerformanceInitialized;
            }
        }
        
        private void Update()
        {
            // Update the adaptive performance system
            AdaptivePerformanceManager.Update();
            
            // Handle debug input
            HandleDebugInput();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && AdaptivePerformanceManager.IsInitialized)
            {
                // Refresh configuration when app regains focus
                AdaptivePerformanceManager.RefreshConfiguration();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && AdaptivePerformanceManager.IsInitialized)
            {
                // Refresh configuration when app is unpaused
                AdaptivePerformanceManager.RefreshConfiguration();
            }
        }
        
        /// <summary>
        /// Initialize the adaptive performance system
        /// </summary>
        public void InitializeAdaptivePerformance()
        {
            if (AdaptivePerformanceManager.IsInitialized)
            {
                Debug.LogWarning("[AdaptivePerformanceBootstrap] Adaptive Performance already initialized");
                return;
            }
            
            // Set strategy before initialization
            AdaptivePerformanceManager.SetAdjustmentStrategy(adjustmentStrategy);
            AdaptivePerformanceManager.SetAutoAdjustment(enableAutoAdjustment);
            
            // Initialize with configuration matrix
            AdaptivePerformanceManager.Initialize(configurationMatrix);
            
            if (enableDebugLogging)
            {
                Debug.Log($"[AdaptivePerformanceBootstrap] Initialized with strategy: {adjustmentStrategy}, AutoAdjust: {enableAutoAdjustment}");
            }
        }
        
        /// <summary>
        /// Refresh the adaptive performance configuration
        /// </summary>
        public void RefreshConfiguration()
        {
            AdaptivePerformanceManager.RefreshConfiguration();
            
            if (enableDebugLogging)
            {
                Debug.Log("[AdaptivePerformanceBootstrap] Configuration refreshed");
            }
        }
        
        /// <summary>
        /// Set the configuration matrix at runtime
        /// </summary>
        public void SetConfigurationMatrix(AdaptivePerformanceMatrix matrix)
        {
            configurationMatrix = matrix;
            AdaptivePerformanceManager.SetConfigurationMatrix(matrix);
            
            if (enableDebugLogging)
            {
                Debug.Log("[AdaptivePerformanceBootstrap] Configuration matrix updated");
            }
        }
        
        /// <summary>
        /// Set adjustment strategy at runtime
        /// </summary>
        public void SetAdjustmentStrategy(AdjustmentStrategy strategy)
        {
            adjustmentStrategy = strategy;
            AdaptivePerformanceManager.SetAdjustmentStrategy(strategy);
        }
        
        /// <summary>
        /// Enable or disable auto adjustment at runtime
        /// </summary>
        public void SetAutoAdjustment(bool enabled)
        {
            enableAutoAdjustment = enabled;
            AdaptivePerformanceManager.SetAutoAdjustment(enabled);
        }
        
        /// <summary>
        /// Print debug information to console
        /// </summary>
        public void PrintDebugInfo()
        {
            if (AdaptivePerformanceManager.IsInitialized)
            {
                Debug.Log($"=== Adaptive Performance Debug Info ===\n{AdaptivePerformanceManager.GetDebugInfo()}");
            }
            else
            {
                Debug.Log("Adaptive Performance not initialized");
            }
        }
        
        /// <summary>
        /// Handle debug input
        /// </summary>
        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(debugInfoKey))
            {
                if (showDebugUI)
                {
                    showDebugPanel = !showDebugPanel;
                }
                else
                {
                    PrintDebugInfo();
                }
            }
            
            // Platform-specific debug input
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.F3))
            {
                RefreshConfiguration();
            }
#endif
        }
        
        /// <summary>
        /// Debug GUI for runtime information
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugUI || !showDebugPanel)
                return;
                
            InitializeDebugStyle();
            
            // Debug panel
            GUILayout.BeginArea(new Rect(10, 10, 400, 300), GUI.skin.box);
            GUILayout.Label("Adaptive Performance Debug", debugStyle);
            
            debugScrollPosition = GUILayout.BeginScrollView(debugScrollPosition);
            
            if (AdaptivePerformanceManager.IsInitialized)
            {
                var debugInfo = AdaptivePerformanceManager.GetDebugInfo();
                GUILayout.Label(debugInfo, debugStyle);
                
                GUILayout.Space(10);
                
                // Control buttons
                if (GUILayout.Button("Refresh Configuration"))
                {
                    RefreshConfiguration();
                }
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Conservative"))
                    SetAdjustmentStrategy(AdjustmentStrategy.Conservative);
                if (GUILayout.Button("Balanced"))
                    SetAdjustmentStrategy(AdjustmentStrategy.Balanced);
                if (GUILayout.Button("Aggressive"))
                    SetAdjustmentStrategy(AdjustmentStrategy.Aggressive);
                GUILayout.EndHorizontal();
                
                bool autoAdjust = AdaptivePerformanceManager.AutoAdjustmentEnabled;
                bool newAutoAdjust = GUILayout.Toggle(autoAdjust, "Auto Adjustment");
                if (newAutoAdjust != autoAdjust)
                {
                    SetAutoAdjustment(newAutoAdjust);
                }
            }
            else
            {
                GUILayout.Label("Not Initialized", debugStyle);
                if (GUILayout.Button("Initialize"))
                {
                    InitializeAdaptivePerformance();
                }
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Initialize debug GUI style
        /// </summary>
        private void InitializeDebugStyle()
        {
            if (debugStyle == null)
            {
                debugStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    wordWrap = true
                };
            }
        }
        
        // Event handlers for debug logging
        private void OnAdaptivePerformanceInitialized()
        {
            if (enableDebugLogging)
                Debug.Log("[AdaptivePerformanceBootstrap] Adaptive Performance initialized");
        }
        
        private void OnProfileChanged(PerformanceProfile profile)
        {
            if (enableDebugLogging)
                Debug.Log($"[AdaptivePerformanceBootstrap] Performance profile changed: {profile}");
        }
        
        private void OnPerformanceLevelChanged(PerformanceLevel level)
        {
            if (enableDebugLogging)
                Debug.Log($"[AdaptivePerformanceBootstrap] Performance level changed: {level}");
        }
        
        private void OnConfigurationApplied(Energy8.ViewportManager.Core.ViewportContext context, PerformanceProfile profile)
        {
            if (enableDebugLogging)
                Debug.Log($"[AdaptivePerformanceBootstrap] Configuration applied for {context}: {profile}");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (enableDebugLogging)
            {
                AdaptivePerformanceManager.OnProfileChanged -= OnProfileChanged;
                AdaptivePerformanceManager.OnPerformanceLevelChanged -= OnPerformanceLevelChanged;
                AdaptivePerformanceManager.OnConfigurationApplied -= OnConfigurationApplied;
                AdaptivePerformanceManager.OnInitialized -= OnAdaptivePerformanceInitialized;
            }
        }
    }
}