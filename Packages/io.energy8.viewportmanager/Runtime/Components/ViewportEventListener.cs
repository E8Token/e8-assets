using UnityEngine;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;
using VMScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;
using VMDeviceType = Energy8.ViewportManager.Core.DeviceType;
using VMPlatform = Energy8.ViewportManager.Core.Platform;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Base class for components that automatically listen to ViewportManager events
    /// and persist across scene loads with DontDestroyOnLoad.
    /// 
    /// Inherit from this class and override the virtual methods to implement
    /// custom behavior for viewport changes.
    /// </summary>
    public abstract class ViewportEventListener : MonoBehaviour
    {
        [Header("Viewport Event Listener")]
        [SerializeField] private bool enableLogging = false;
        [SerializeField] private bool autoSubscribe = true;
        [SerializeField] private bool persistAcrossScenes = true;

        protected ViewportContext CurrentContext { get; private set; }
        protected ViewportConfiguration CurrentConfiguration { get; private set; }
        protected bool IsSubscribed { get; private set; }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            // Make this component persistent across scene loads
            if (persistAcrossScenes)
            {
                // Check if there's already an instance of this type
                var existingInstance = FindExistingInstance();
                if (existingInstance != null && existingInstance != this)
                {
                    if (enableLogging)
                        Debug.Log($"[{GetType().Name}] Instance already exists, destroying duplicate", this);
                    
                    Destroy(gameObject);
                    return;
                }
                
                DontDestroyOnLoad(gameObject);
                
                if (enableLogging)
                    Debug.Log($"[{GetType().Name}] Made persistent across scenes", this);
            }
        }

        protected virtual void Start()
        {
            if (autoSubscribe)
            {
                SubscribeToEvents();
            }

            // Get initial state if ViewportManager is already initialized
            if (ViewportManager.IsInitialized)
            {
                CurrentContext = ViewportManager.CurrentContext;
                CurrentConfiguration = ViewportManager.CurrentConfiguration;
                
                // Call initial setup
                OnInitialSetup(CurrentContext, CurrentConfiguration);
            }
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// Subscribe to ViewportManager events
        /// </summary>
        public void SubscribeToEvents()
        {
            if (IsSubscribed) return;

            ViewportManager.OnContextChanged += OnViewportContextChanged;
            ViewportManager.OnConfigurationChanged += OnViewportConfigurationChanged;
            ViewportManager.OnQualityChanged += OnViewportQualityChanged;
            ViewportManager.OnInitialized += OnViewportManagerInitialized;

            IsSubscribed = true;

            if (enableLogging)
                Debug.Log($"[{GetType().Name}] Subscribed to ViewportManager events", this);
        }

        /// <summary>
        /// Unsubscribe from ViewportManager events
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            if (!IsSubscribed) return;

            ViewportManager.OnContextChanged -= OnViewportContextChanged;
            ViewportManager.OnConfigurationChanged -= OnViewportConfigurationChanged;
            ViewportManager.OnQualityChanged -= OnViewportQualityChanged;
            ViewportManager.OnInitialized -= OnViewportManagerInitialized;

            IsSubscribed = false;

            if (enableLogging)
                Debug.Log($"[{GetType().Name}] Unsubscribed from ViewportManager events", this);
        }

        #endregion

        #region Event Handlers

        private void OnViewportContextChanged(ViewportContext context)
        {
            var previousContext = CurrentContext;
            CurrentContext = context;

            if (enableLogging)
                Debug.Log($"[{GetType().Name}] Context changed: {previousContext} → {context}", this);

            // Call virtual method for custom implementation
            OnContextChanged(previousContext, context);

            // Check for specific changes
            if (previousContext.orientation != context.orientation)
            {
                OnOrientationChanged(previousContext.orientation, context.orientation);
            }

            if (previousContext.deviceType != context.deviceType)
            {
                OnDeviceTypeChanged(previousContext.deviceType, context.deviceType);
            }

            if (previousContext.platform != context.platform)
            {
                OnPlatformChanged(previousContext.platform, context.platform);
            }
        }

        private void OnViewportConfigurationChanged(ViewportConfiguration configuration)
        {
            var previousConfiguration = CurrentConfiguration;
            CurrentConfiguration = configuration;

            if (enableLogging)
                Debug.Log($"[{GetType().Name}] Configuration changed: {previousConfiguration} → {configuration}", this);

            // Call virtual method for custom implementation
            OnConfigurationChanged(previousConfiguration, configuration);
        }

        private void OnViewportQualityChanged(int qualityLevel)
        {
            if (enableLogging)
                Debug.Log($"[{GetType().Name}] Quality changed to level: {qualityLevel}", this);

            // Call virtual method for custom implementation
            OnQualityChanged(qualityLevel);
        }

        private void OnViewportManagerInitialized()
        {
            CurrentContext = ViewportManager.CurrentContext;
            CurrentConfiguration = ViewportManager.CurrentConfiguration;

            if (enableLogging)
                Debug.Log($"[{GetType().Name}] ViewportManager initialized", this);

            // Call virtual method for custom implementation
            OnManagerInitialized();
            
            // Also call initial setup
            OnInitialSetup(CurrentContext, CurrentConfiguration);
        }

        #endregion

        #region Virtual Methods for Override

        /// <summary>
        /// Called when the component is first set up with initial viewport state
        /// </summary>
        /// <param name="initialContext">Initial viewport context</param>
        /// <param name="initialConfiguration">Initial viewport configuration</param>
        protected virtual void OnInitialSetup(ViewportContext initialContext, ViewportConfiguration initialConfiguration)
        {
            // Override in derived classes for initial setup logic
        }

        /// <summary>
        /// Called when ViewportManager is initialized
        /// </summary>
        protected virtual void OnManagerInitialized()
        {
            // Override in derived classes for initialization logic
        }

        /// <summary>
        /// Called when viewport context changes
        /// </summary>
        /// <param name="previousContext">Previous context</param>
        /// <param name="newContext">New context</param>
        protected virtual void OnContextChanged(ViewportContext previousContext, ViewportContext newContext)
        {
            // Override in derived classes for context change logic
        }

        /// <summary>
        /// Called when viewport configuration changes
        /// </summary>
        /// <param name="previousConfiguration">Previous configuration</param>
        /// <param name="newConfiguration">New configuration</param>
        protected virtual void OnConfigurationChanged(ViewportConfiguration previousConfiguration, ViewportConfiguration newConfiguration)
        {
            // Override in derived classes for configuration change logic
        }

        /// <summary>
        /// Called when quality level changes
        /// </summary>
        /// <param name="newQualityLevel">New quality level</param>
        protected virtual void OnQualityChanged(int newQualityLevel)
        {
            // Override in derived classes for quality change logic
        }

        /// <summary>
        /// Called when orientation changes
        /// </summary>
        /// <param name="fromOrientation">Previous orientation</param>
        /// <param name="toOrientation">New orientation</param>
        protected virtual void OnOrientationChanged(VMScreenOrientation fromOrientation, VMScreenOrientation toOrientation)
        {
            // Override in derived classes for orientation change logic
        }

        /// <summary>
        /// Called when device type changes
        /// </summary>
        /// <param name="fromDeviceType">Previous device type</param>
        /// <param name="toDeviceType">New device type</param>
        protected virtual void OnDeviceTypeChanged(VMDeviceType fromDeviceType, VMDeviceType toDeviceType)
        {
            // Override in derived classes for device type change logic
        }

        /// <summary>
        /// Called when platform changes
        /// </summary>
        /// <param name="fromPlatform">Previous platform</param>
        /// <param name="toPlatform">New platform</param>
        protected virtual void OnPlatformChanged(VMPlatform fromPlatform, VMPlatform toPlatform)
        {
            // Override in derived classes for platform change logic
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if there's already an instance of this component type in the scene
        /// </summary>
        protected virtual ViewportEventListener FindExistingInstance()
        {
            var instances = FindObjectsByType(GetType(), FindObjectsSortMode.None);
            foreach (var instance in instances)
            {
                if (instance != this && instance is ViewportEventListener listener)
                {
                    return listener;
                }
            }
            return null;
        }

        /// <summary>
        /// Get readable name for logging
        /// </summary>
        protected virtual string GetLogName()
        {
            return $"{GetType().Name}({gameObject.name})";
        }

        /// <summary>
        /// Helper method to check if current orientation is portrait
        /// </summary>
        protected bool IsPortraitOrientation()
        {
            return CurrentContext.orientation == VMScreenOrientation.Portrait;
        }

        /// <summary>
        /// Helper method to check if current orientation is landscape
        /// </summary>
        protected bool IsLandscapeOrientation()
        {
            return CurrentContext.orientation == VMScreenOrientation.Landscape ||
                   CurrentContext.orientation == VMScreenOrientation.LandscapeLeft ||
                   CurrentContext.orientation == VMScreenOrientation.LandscapeRight;
        }

        /// <summary>
        /// Helper method to check if current device is mobile
        /// </summary>
        protected bool IsMobileDevice()
        {
            return CurrentContext.deviceType == VMDeviceType.Mobile;
        }

        /// <summary>
        /// Helper method to check if running on WebGL
        /// </summary>
        protected bool IsWebGLPlatform()
        {
            return CurrentContext.platform == VMPlatform.WebGL;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Force refresh the current viewport state
        /// </summary>
        [ContextMenu("Refresh Viewport State")]
        public void RefreshViewportState()
        {
            if (ViewportManager.IsInitialized)
            {
                CurrentContext = ViewportManager.CurrentContext;
                CurrentConfiguration = ViewportManager.CurrentConfiguration;
                
                OnInitialSetup(CurrentContext, CurrentConfiguration);
                
                if (enableLogging)
                    Debug.Log($"[{GetType().Name}] Viewport state refreshed", this);
            }
        }

        /// <summary>
        /// Toggle logging for this component
        /// </summary>
        [ContextMenu("Toggle Logging")]
        public void ToggleLogging()
        {
            enableLogging = !enableLogging;
            Debug.Log($"[{GetType().Name}] Logging {(enableLogging ? "enabled" : "disabled")}", this);
        }

        #endregion
    }
}
