using System;
using UnityEngine;

namespace Energy8.ViewportManager.Plugins
{
    /// <summary>
    /// WebGL Plugin for detecting viewport information including device type, user agent, and screen properties
    /// Simplified version without external dependencies
    /// </summary>
    public class ViewportDetectionPlugin : MonoBehaviour
    {
        private static ViewportDetectionPlugin instance;
        public static ViewportDetectionPlugin Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[ViewportDetectionPlugin] Initialized successfully");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Get the browser's user agent string
        /// </summary>
        public string GetUserAgent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetUserAgentJS();
#else
            return "Unity Editor - " + SystemInfo.operatingSystem;
#endif
        }

        /// <summary>
        /// Get detailed device information including screen size, pixel ratio, etc.
        /// Returns JSON string with device information
        /// </summary>
        public string GetDeviceInfo()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetDeviceInfoJS();
#else
            var deviceInfo = new DeviceInfo
            {
                userAgent = "Unity Editor",
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                devicePixelRatio = 1.0f,
                isMobile = false,
                isTablet = false,
                platform = "Editor",
                touchSupport = Input.touchSupported
            };
            return JsonUtility.ToJson(deviceInfo);
#endif
        }

        /// <summary>
        /// Check if the current device is mobile based on user agent and screen characteristics
        /// </summary>
        public bool IsMobileDevice()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsMobileDeviceJS();
#else
            return Application.isMobilePlatform;
#endif
        }

        /// <summary>
        /// Get current screen orientation (landscape/portrait)
        /// </summary>
        public string GetScreenOrientation()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GetScreenOrientationJS();
#else
            return Screen.width > Screen.height ? "landscape" : "portrait";
#endif
        }

        /// <summary>
        /// Get platform type (webgl, mobile, desktop, etc.)
        /// </summary>
        public string GetPlatformType()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return "webgl";
#elif UNITY_ANDROID
            return "android";
#elif UNITY_IOS  
            return "ios";
#elif UNITY_STANDALONE_WIN
            return "windows";
#elif UNITY_STANDALONE_OSX
            return "macos";
#elif UNITY_STANDALONE_LINUX
            return "linux";
#else
            return "unknown";
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetUserAgentJS();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetDeviceInfoJS();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool IsMobileDeviceJS();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetScreenOrientationJS();
#endif
    }

    /// <summary>
    /// Device information structure returned by GetDeviceInfo
    /// </summary>
    [Serializable]
    public class DeviceInfo
    {
        public string userAgent = "";
        public int screenWidth = 0;
        public int screenHeight = 0;
        public float devicePixelRatio = 1.0f;
        public bool isMobile = false;
        public bool isTablet = false;
        public string platform = "";
        public bool touchSupport = false;
        public int availableWidth = 0;
        public int availableHeight = 0;
    }
}