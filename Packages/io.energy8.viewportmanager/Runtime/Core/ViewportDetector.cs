using UnityEngine;
using Energy8.ViewportManager.Plugins;

namespace Energy8.ViewportManager.Core
{
    /// <summary>
    /// Utility class for detecting viewport characteristics across different platforms
    /// </summary>
    public static class ViewportDetector
    {
        /// <summary>
        /// Detect the current device type (Desktop/Mobile)
        /// </summary>
        public static DeviceType DetectDeviceType()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Use WebGL plugin for enhanced detection
            if (ViewportDetectionPlugin.Instance != null)
            {
                return ViewportDetectionPlugin.Instance.IsMobileDevice() ? DeviceType.Mobile : DeviceType.Desktop;
            }
#endif
            
            // Fallback detection
            if (Application.isMobilePlatform)
                return DeviceType.Mobile;
                
            // Check screen size as additional indicator
            if (Screen.width < 768 || Screen.height < 768)
                return DeviceType.Mobile;
                
            return DeviceType.Desktop;
        }

        /// <summary>
        /// Detect the current platform
        /// </summary>
        public static Platform DetectPlatform()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Platform.WebGL;
#elif UNITY_ANDROID
            return Platform.Android;
#elif UNITY_IOS
            return Platform.iOS;
#elif UNITY_STANDALONE_WIN
            return Platform.Windows;
#elif UNITY_STANDALONE_OSX
            return Platform.macOS;
#elif UNITY_STANDALONE_LINUX
            return Platform.Linux;
#else
            return Platform.Desktop;
#endif
        }

        /// <summary>
        /// Detect the current screen orientation
        /// </summary>
        public static ScreenOrientation DetectOrientation()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Use WebGL plugin for enhanced detection
            if (ViewportDetectionPlugin.Instance != null)
            {
                string orientation = ViewportDetectionPlugin.Instance.GetScreenOrientation();
                return orientation.ToLower() == "portrait" ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
            }
#endif
            
            // Fallback to screen dimensions and Unity's orientation
            bool isPortrait = Screen.width < Screen.height;
            if (isPortrait)
            {
                return ScreenOrientation.Portrait;
            }
            
            // For landscape, try to get more specific orientation from Unity
#if !UNITY_WEBGL || UNITY_EDITOR
            var unityOrientation = Screen.orientation;
            if (unityOrientation == UnityEngine.ScreenOrientation.LandscapeLeft)
                return ScreenOrientation.Landscape;
            else if (unityOrientation == UnityEngine.ScreenOrientation.LandscapeRight)
                return ScreenOrientation.Landscape;
#endif
            
            // Default landscape fallback
            return ScreenOrientation.Landscape;
        }

        /// <summary>
        /// Get detailed device information (WebGL only)
        /// </summary>
        public static DeviceInfo GetDeviceInfo()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (ViewportDetectionPlugin.Instance != null)
            {
                string jsonInfo = ViewportDetectionPlugin.Instance.GetDeviceInfo();
                try
                {
                    return JsonUtility.FromJson<DeviceInfo>(jsonInfo);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse device info: {e.Message}");
                }
            }
#endif
            
            // Fallback device info
            return new DeviceInfo
            {
                userAgent = GetUserAgent(),
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                devicePixelRatio = 1.0f,
                isMobile = DetectDeviceType() == DeviceType.Mobile,
                platform = DetectPlatform().ToString(),
                touchSupport = Input.touchSupported
            };
        }

        /// <summary>
        /// Get user agent string
        /// </summary>
        public static string GetUserAgent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (ViewportDetectionPlugin.Instance != null)
            {
                return ViewportDetectionPlugin.Instance.GetUserAgent();
            }
#endif
            
            return $"Unity {Application.unityVersion} - {SystemInfo.operatingSystem}";
        }

        /// <summary>
        /// Check if current device supports touch input
        /// </summary>
        public static bool IsTouchDevice()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var deviceInfo = GetDeviceInfo();
            return deviceInfo.touchSupport;
#else
            return Input.touchSupported || Application.isMobilePlatform;
#endif
        }

        /// <summary>
        /// Get a Unity Quality Level recommendation based on device characteristics
        /// </summary>
        public static int GetRecommendedQualityLevel()
        {
            var deviceType = DetectDeviceType();
            var orientation = DetectOrientation();
            var platform = DetectPlatform();
            
            // Mobile + Portrait = Level 0 (optimized for weak devices)
            if (deviceType == DeviceType.Mobile && orientation == ScreenOrientation.Portrait)
                return 0;
                
            // Mobile + Landscape (any) = Level 2 (better performance but still mobile)
            if (deviceType == DeviceType.Mobile && 
                (orientation == ScreenOrientation.Landscape || 
                 orientation == ScreenOrientation.Landscape || 
                 orientation == ScreenOrientation.Landscape))
                return 2;
                
            // Desktop + WebGL = Level 3 (good performance in browsers)
            if (deviceType == DeviceType.Desktop && platform == Platform.WebGL)
                return 3;
                
            // Desktop + Native = Level 4 (best performance)
            if (deviceType == DeviceType.Desktop && platform != Platform.WebGL)
                return 4;
                
            // Default fallback
            return 2;
        }

        /// <summary>
        /// Detect current viewport context (combined detection)
        /// </summary>
        public static ViewportContext DetectContext()
        {
            var deviceType = DetectDeviceType();
            var orientation = DetectOrientation();
            var platform = DetectPlatform();

            return new ViewportContext(orientation, deviceType, platform);
        }

        /// <summary>
        /// Check if context has changed since previous detection
        /// </summary>
        public static bool HasContextChanged(ViewportContext previous)
        {
            var current = DetectContext();
            return !current.Equals(previous);
        }

        /// <summary>
        /// Detect current viewport info (full detection)
        /// </summary>
        public static ViewportInfo DetectViewport()
        {
            return new ViewportInfo
            {
                deviceType = DetectDeviceType(),
                platform = DetectPlatform(),
                orientation = DetectOrientation(),
                screenWidth = Screen.width,
                screenHeight = Screen.height
            };
        }
    }
}
