using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Base component for listening to viewport changes
    /// </summary>
    public abstract class ViewportEventListener : MonoBehaviour
    {
        [Header("Event Settings")]
        [SerializeField] protected bool autoSubscribe = true;
        [SerializeField] protected bool enableDebugLogging = false;

        protected virtual void Start()
        {
            if (autoSubscribe)
            {
                SubscribeToEvents();
            }
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Subscribe to viewport events
        /// </summary>
        public virtual void SubscribeToEvents()
        {
            ViewportManager.OnContextChanged += OnViewportContextChanged;
            ViewportManager.OnOrientationChanged += OnOrientationChanged;
            ViewportManager.OnScreenSizeChanged += OnScreenSizeChanged;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[{gameObject.name}] Subscribed to viewport events");
            }
        }

        /// <summary>
        /// Unsubscribe from viewport events
        /// </summary>
        public virtual void UnsubscribeFromEvents()
        {
            ViewportManager.OnContextChanged -= OnViewportContextChanged;
            ViewportManager.OnOrientationChanged -= OnOrientationChanged;
            ViewportManager.OnScreenSizeChanged -= OnScreenSizeChanged;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[{gameObject.name}] Unsubscribed from viewport events");
            }
        }

        /// <summary>
        /// Called when viewport context changes
        /// </summary>
        protected virtual void OnViewportContextChanged(ViewportContext context)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[{gameObject.name}] Viewport context changed: {context}");
            }
        }

        /// <summary>
        /// Called when screen orientation changes
        /// </summary>
        protected virtual void OnOrientationChanged(Energy8.ViewportManager.Core.ScreenOrientation orientation)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[{gameObject.name}] Orientation changed: {orientation}");
            }
        }

        /// <summary>
        /// Called when screen size changes
        /// </summary>
        protected virtual void OnScreenSizeChanged(int width, int height)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[{gameObject.name}] Screen size changed: {width}x{height}");
            }
        }
    }

    /// <summary>
    /// Simple viewport event listener with Unity Events
    /// </summary>
    public class BasicViewportEventListener : ViewportEventListener
    {
        [Header("Unity Events")]
        public UnityEngine.Events.UnityEvent<ViewportContext> OnContextChangedEvent;
        public UnityEngine.Events.UnityEvent<Energy8.ViewportManager.Core.ScreenOrientation> OnOrientationChangedEvent;
        public UnityEngine.Events.UnityEvent<int, int> OnScreenSizeChangedEvent;
        public UnityEngine.Events.UnityEvent OnPortraitModeEvent;
        public UnityEngine.Events.UnityEvent OnLandscapeModeEvent;
        public UnityEngine.Events.UnityEvent OnMobileDeviceEvent;
        public UnityEngine.Events.UnityEvent OnDesktopDeviceEvent;

        protected override void OnViewportContextChanged(ViewportContext context)
        {
            base.OnViewportContextChanged(context);
            OnContextChangedEvent?.Invoke(context);
            
            // Fire device type events
            if (context.deviceType == Energy8.ViewportManager.Core.DeviceType.Mobile)
            {
                OnMobileDeviceEvent?.Invoke();
            }
            else if (context.deviceType == Energy8.ViewportManager.Core.DeviceType.Desktop)
            {
                OnDesktopDeviceEvent?.Invoke();
            }
        }

        protected override void OnOrientationChanged(Energy8.ViewportManager.Core.ScreenOrientation orientation)
        {
            base.OnOrientationChanged(orientation);
            OnOrientationChangedEvent?.Invoke(orientation);
            
            // Fire orientation events
            if (orientation == Energy8.ViewportManager.Core.ScreenOrientation.Portrait)
            {
                OnPortraitModeEvent?.Invoke();
            }
            else if (orientation == Energy8.ViewportManager.Core.ScreenOrientation.Landscape)
            {
                OnLandscapeModeEvent?.Invoke();
            }
        }

        protected override void OnScreenSizeChanged(int width, int height)
        {
            base.OnScreenSizeChanged(width, height);
            OnScreenSizeChangedEvent?.Invoke(width, height);
        }
    }
}