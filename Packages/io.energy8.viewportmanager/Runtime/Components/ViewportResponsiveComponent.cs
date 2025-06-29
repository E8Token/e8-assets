using UnityEngine;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;
using VmDeviceType = Energy8.ViewportManager.Core.DeviceType;
using VmScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Base class for components that need to respond to viewport changes
    /// </summary>
    public abstract class ViewportResponsiveComponent : MonoBehaviour
    {
        [Header("Viewport Responsive")]
        [SerializeField] protected bool autoSubscribe = true;
        [SerializeField] protected bool logChanges = false;

        protected virtual void Start()
        {
            if (autoSubscribe)
            {
                SubscribeToViewportChanges();
            }
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromViewportChanges();
        }

        /// <summary>
        /// Subscribe to viewport manager events
        /// </summary>
        protected virtual void SubscribeToViewportChanges()
        {
            ViewportManager.OnConfigurationChanged += OnViewportConfigurationChanged;
            ViewportManager.OnContextChanged += OnViewportContextChanged;
            ViewportManager.OnQualityChanged += OnViewportQualityChanged;

            // Apply current configuration if available
            if (ViewportManager.IsInitialized)
            {
                OnViewportConfigurationChanged(ViewportManager.CurrentConfiguration);
            }
        }

        /// <summary>
        /// Unsubscribe from viewport manager events
        /// </summary>
        protected virtual void UnsubscribeFromViewportChanges()
        {
            ViewportManager.OnConfigurationChanged -= OnViewportConfigurationChanged;
            ViewportManager.OnContextChanged -= OnViewportContextChanged;
            ViewportManager.OnQualityChanged -= OnViewportQualityChanged;
        }

        /// <summary>
        /// Called when viewport configuration changes
        /// </summary>
        protected virtual void OnViewportConfigurationChanged(ViewportConfiguration config)
        {
            if (logChanges)
            {
                Debug.Log($"{gameObject.name}: Viewport configuration changed to Unity Quality Level {config.unityQualityLevel}", this);
            }
        }

        /// <summary>
        /// Called when viewport context changes (orientation, device, platform)
        /// </summary>
        protected virtual void OnViewportContextChanged(ViewportContext context)
        {
            if (logChanges)
            {
                Debug.Log($"{gameObject.name}: Viewport context changed to {context}", this);
            }
        }

        /// <summary>
        /// Called when Unity quality level changes
        /// </summary>
        protected virtual void OnViewportQualityChanged(int qualityLevel)
        {
            if (logChanges)
            {
                Debug.Log($"{gameObject.name}: Quality changed to level {qualityLevel}", this);
            }
        }

        /// <summary>
        /// Get current viewport context
        /// </summary>
        protected ViewportContext GetCurrentContext()
        {
            return ViewportManager.CurrentContext;
        }

        /// <summary>
        /// Get current viewport configuration
        /// </summary>
        protected ViewportConfiguration GetCurrentConfiguration()
        {
            return ViewportManager.CurrentConfiguration;
        }

        /// <summary>
        /// Get current Unity quality level
        /// </summary>
        protected int GetCurrentQualityLevel()
        {
            return QualitySettings.GetQualityLevel();
        }

        /// <summary>
        /// Check if current quality is lite mode (level 0)
        /// </summary>
        protected bool IsLiteMode()
        {
            return GetCurrentQualityLevel() == 0;
        }

        /// <summary>
        /// Check if current device is mobile
        /// </summary>
        protected bool IsMobile()
        {
            return GetCurrentContext().deviceType == VmDeviceType.Mobile;
        }

        /// <summary>
        /// Check if current orientation is portrait
        /// </summary>
        protected bool IsPortrait()
        {
            return GetCurrentContext().orientation == VmScreenOrientation.Portrait;
        }

        /// <summary>
        /// Force refresh viewport detection
        /// </summary>
        protected void RefreshViewport()
        {
            ViewportManager.RefreshContext();
        }
    }
}
