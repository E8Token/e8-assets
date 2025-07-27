using System;
using UnityEngine;
using Energy8.ViewportManager.Core;

namespace Energy8.ViewportManager
{
    /// <summary>
    /// Main viewport manager - handles automatic detection and viewport context management
    /// Focused on viewport detection, screen orientation, and device information only
    /// </summary>
    public static class ViewportManager
    {
        #region Properties
        
        /// <summary>
        /// Current viewport context
        /// </summary>
        public static ViewportContext CurrentContext { get; private set; }
        
        /// <summary>
        /// Is the system initialized?
        /// </summary>
        public static bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Last detection time
        /// </summary>
        public static float LastDetectionTime { get; private set; }
        
        #endregion

        #region Events
        
        /// <summary>
        /// Fired when viewport context changes (orientation, device, platform, screen size)
        /// </summary>
        public static event Action<ViewportContext> OnContextChanged;
        
        /// <summary>
        /// Fired when screen orientation changes
        /// </summary>
        public static event Action<Energy8.ViewportManager.Core.ScreenOrientation> OnOrientationChanged;
        
        /// <summary>
        /// Fired when screen size changes
        /// </summary>
        public static event Action<int, int> OnScreenSizeChanged;
        
        /// <summary>
        /// Fired when system is initialized
        /// </summary>
        public static event Action OnInitialized;
        
        #endregion

        #region Initialization
        
        /// <summary>
        /// Initialize viewport manager
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("ViewportManager is already initialized");
                return;
            }

            // Detect initial context
            CurrentContext = ViewportDetector.DetectContext();
            LastDetectionTime = Time.time;
            
            IsInitialized = true;
            
            Debug.Log($"ViewportManager initialized: {CurrentContext}");
            
            OnInitialized?.Invoke();
        }
        
        #endregion

        #region Context Detection & Updates
        
        /// <summary>
        /// Force refresh of viewport context
        /// </summary>
        public static void RefreshContext()
        {
            if (!IsInitialized) 
            {
                Initialize();
                return;
            }
            
            var newContext = ViewportDetector.DetectContext();
            
            if (!CurrentContext.Equals(newContext))
            {
                UpdateContext(newContext);
            }
            
            LastDetectionTime = Time.time;
        }

        /// <summary>
        /// Update to new viewport context
        /// </summary>
        private static void UpdateContext(ViewportContext newContext)
        {
            var previousContext = CurrentContext;
            CurrentContext = newContext;
            
            Debug.Log($"Viewport context changed: {previousContext} -> {CurrentContext}");
            
            // Fire events
            OnContextChanged?.Invoke(CurrentContext);
            
            // Fire specific events if changed
            if (previousContext.orientation != CurrentContext.orientation)
            {
                OnOrientationChanged?.Invoke(CurrentContext.orientation);
            }
            
            if (previousContext.screenWidth != CurrentContext.screenWidth || 
                previousContext.screenHeight != CurrentContext.screenHeight)
            {
                OnScreenSizeChanged?.Invoke(CurrentContext.screenWidth, CurrentContext.screenHeight);
            }
        }
        
        #endregion

        #region Public API
        
        /// <summary>
        /// Get current screen orientation
        /// </summary>
        public static Energy8.ViewportManager.Core.ScreenOrientation GetOrientation()
        {
            return IsInitialized ? CurrentContext.orientation : ViewportDetector.DetectOrientation();
        }
        
        /// <summary>
        /// Get current device type
        /// </summary>
        public static Energy8.ViewportManager.Core.DeviceType GetDeviceType()
        {
            return IsInitialized ? CurrentContext.deviceType : ViewportDetector.DetectDeviceType();
        }
        
        /// <summary>
        /// Get current platform
        /// </summary>
        public static Platform GetPlatform()
        {
            return IsInitialized ? CurrentContext.platform : ViewportDetector.DetectPlatform();
        }
        
        /// <summary>
        /// Get current screen size
        /// </summary>
        public static (int width, int height) GetScreenSize()
        {
            if (IsInitialized)
            {
                return (CurrentContext.screenWidth, CurrentContext.screenHeight);
            }
            return (Screen.width, Screen.height);
        }
        
        /// <summary>
        /// Get current aspect ratio
        /// </summary>
        public static float GetAspectRatio()
        {
            var (width, height) = GetScreenSize();
            return (float)width / height;
        }
        
        /// <summary>
        /// Check if current orientation is portrait
        /// </summary>
        public static bool IsPortrait()
        {
            return GetOrientation() == Energy8.ViewportManager.Core.ScreenOrientation.Portrait;
        }
        
        /// <summary>
        /// Check if current orientation is landscape
        /// </summary>
        public static bool IsLandscape()
        {
            return GetOrientation() == Energy8.ViewportManager.Core.ScreenOrientation.Landscape;
        }
        
        /// <summary>
        /// Check if current device is mobile
        /// </summary>
        public static bool IsMobile()
        {
            return GetDeviceType() == Energy8.ViewportManager.Core.DeviceType.Mobile;
        }
        
        /// <summary>
        /// Check if current device is desktop
        /// </summary>
        public static bool IsDesktop()
        {
            return GetDeviceType() == Energy8.ViewportManager.Core.DeviceType.Desktop;
        }
        
        /// <summary>
        /// Check if device supports touch
        /// </summary>
        public static bool IsTouchDevice()
        {
            return ViewportDetector.IsTouchDevice();
        }
        
        #endregion

        #region Debug & Info
        
        /// <summary>
        /// Get system information for debugging
        /// </summary>
        public static string GetSystemInfo()
        {
            if (!IsInitialized)
            {
                return "ViewportManager not initialized";
            }
            
            return $"ViewportManager System Info:\n" +
                   $"  Context: {CurrentContext}\n" +
                   $"  Aspect Ratio: {GetAspectRatio():F2}\n" +
                   $"  Touch Support: {IsTouchDevice()}\n" +
                   $"  Last Detection: {LastDetectionTime:F2}s";
        }
        
        #endregion
    }
}