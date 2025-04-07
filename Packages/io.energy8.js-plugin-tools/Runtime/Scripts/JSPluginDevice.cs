using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Device module for JSPluginTools.
    /// Provides information about the current device and browser environment.
    /// </summary>
    public static class JSPluginDevice
    {
        #region Native Methods
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceInitialize();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDeviceGetUserAgent();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDeviceGetBrowserInfo();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDeviceGetOSInfo();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceIsMobile();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceIsTablet();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceIsDesktop();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceAddOrientationChangeListener(string objectId, string methodName);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceRemoveOrientationChangeListener();
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDeviceGetScreenInfo();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceRequestFullscreen();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceExitFullscreen();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceIsFullscreen();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceVibrate(int milliseconds);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceShutdown();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDeviceIsVibrationSupported();
        
        #endregion
        
        private static bool isInitialized = false;
        
        /// <summary>
        /// Initializes the device module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            if (isInitialized)
                return true;
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            isInitialized = JSPluginDeviceInitialize() == 1;
            #else
            JSPluginErrorHandling.LogEvent("JSPluginDevice", "Initialized in stub mode (non-WebGL environment)", JSPluginErrorHandling.ErrorSeverity.Info);
            isInitialized = true;
            #endif
            
            if (isInitialized)
            {
                JSPluginErrorHandling.LogEvent("JSPluginDevice", "Device module initialized successfully", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            
            return isInitialized;
        }
        
        /// <summary>
        /// Shuts down the device module
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            try
            {
                // Remove any listeners
                RemoveOrientationChangeListener();
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDeviceShutdown();
                #endif
                
                isInitialized = false;
                JSPluginErrorHandling.LogEvent("JSPluginDevice", "Device module shut down", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginDevice", "Shutdown", ex, JSPluginErrorHandling.ErrorSeverity.Error);
            }
        }
        
        private static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            
            return Marshal.PtrToStringUTF8(ptr);
        }
        
        #region Device Information
        
        /// <summary>
        /// Information about the browser
        /// </summary>
        [Serializable]
        public class BrowserInfo
        {
            /// <summary>Browser name (Chrome, Firefox, etc.)</summary>
            public string Name;
            
            /// <summary>Browser version</summary>
            public string Version;
            
            /// <summary>Browser engine (Gecko, WebKit, etc.)</summary>
            public string Engine;
            
            /// <summary>Browser language</summary>
            public string Language;
            
            /// <summary>Whether cookies are enabled</summary>
            public bool CookiesEnabled;
            
            /// <summary>Whether local storage is available</summary>
            public bool LocalStorageAvailable;
        }
        
        /// <summary>
        /// Information about the operating system
        /// </summary>
        [Serializable]
        public class OSInfo
        {
            /// <summary>OS name (Windows, macOS, iOS, Android, etc.)</summary>
            public string Name;
            
            /// <summary>OS version</summary>
            public string Version;
            
            /// <summary>OS architecture (32-bit, 64-bit)</summary>
            public string Architecture;
            
            /// <summary>Device platform type</summary>
            public string Platform;
        }
        
        /// <summary>
        /// Information about the device screen
        /// </summary>
        [Serializable]
        public class ScreenInfo
        {
            /// <summary>Screen width in pixels</summary>
            public int Width;
            
            /// <summary>Screen height in pixels</summary>
            public int Height;
            
            /// <summary>Color depth in bits</summary>
            public int ColorDepth;
            
            /// <summary>Pixel ratio (for high DPI screens)</summary>
            public float PixelRatio;
            
            /// <summary>Current orientation (portrait, landscape)</summary>
            public string Orientation;
            
            /// <summary>Screen touch capabilities</summary>
            public string TouchSupport;
        }
        
        /// <summary>
        /// Gets detailed information about the current browser
        /// </summary>
        /// <returns>Browser information or null if not available</returns>
        public static BrowserInfo GetBrowserInfo()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = JSPluginDeviceGetBrowserInfo();
            string json = PtrToString(ptr);
            
            if (string.IsNullOrEmpty(json))
                return new BrowserInfo();
                
            return JsonUtility.FromJson<BrowserInfo>(json);
            #else
            return new BrowserInfo
            {
                Name = "Unity Editor",
                Version = Application.unityVersion,
                Engine = "Unity",
                Language = System.Globalization.CultureInfo.CurrentCulture.Name,
                CookiesEnabled = true,
                LocalStorageAvailable = true
            };
            #endif
        }
        
        /// <summary>
        /// Gets detailed information about the current operating system
        /// </summary>
        /// <returns>OS information or null if not available</returns>
        public static OSInfo GetOSInfo()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = JSPluginDeviceGetOSInfo();
            string json = PtrToString(ptr);
            
            if (string.IsNullOrEmpty(json))
                return new OSInfo();
                
            return JsonUtility.FromJson<OSInfo>(json);
            #else
            return new OSInfo
            {
                Name = SystemInfo.operatingSystem,
                Version = SystemInfo.operatingSystem,
                Architecture = SystemInfo.processorType,
                Platform = Application.platform.ToString()
            };
            #endif
        }
        
        /// <summary>
        /// Gets the user agent string from the browser
        /// </summary>
        /// <returns>User agent string or null if not available</returns>
        public static string GetUserAgent()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = JSPluginDeviceGetUserAgent();
            return PtrToString(ptr);
            #else
            return "Mozilla/5.0 (Unity Editor)";
            #endif
        }
        
        /// <summary>
        /// Checks if the current device is a mobile device
        /// </summary>
        /// <returns>True if the device is mobile</returns>
        public static bool IsMobile()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceIsMobile() == 1;
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Checks if the current device is a tablet
        /// </summary>
        /// <returns>True if the device is a tablet</returns>
        public static bool IsTablet()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceIsTablet() == 1;
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Checks if the current device is a desktop computer
        /// </summary>
        /// <returns>True if the device is a desktop</returns>
        public static bool IsDesktop()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceIsDesktop() == 1;
            #else
            return true;
            #endif
        }
        
        /// <summary>
        /// Gets information about the device screen
        /// </summary>
        /// <returns>Screen information object</returns>
        public static ScreenInfo GetScreenInfo()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = JSPluginDeviceGetScreenInfo();
            string json = PtrToString(ptr);
            
            if (string.IsNullOrEmpty(json))
                return new ScreenInfo();
                
            return JsonUtility.FromJson<ScreenInfo>(json);
            #else
            return new ScreenInfo
            {
                Width = Screen.width,
                Height = Screen.height,
                ColorDepth = 32,
                PixelRatio = 1.0f,
                Orientation = Screen.width > Screen.height ? "landscape" : "portrait",
                TouchSupport = "not supported"
            };
            #endif
        }
        
        #endregion
        
        #region Device Capabilities
        
        private static string orientationListenerObjectId;
        private static string orientationListenerMethodName;
        
        /// <summary>
        /// Registers a listener for orientation change events
        /// </summary>
        /// <param name="objectId">GameObject ID to receive callbacks</param>
        /// <param name="methodName">Method name to call on orientation change</param>
        /// <returns>True if the listener was successfully registered</returns>
        public static bool AddOrientationChangeListener(string objectId, string methodName)
        {
            if (!isInitialized)
                Initialize();
                
            orientationListenerObjectId = objectId;
            orientationListenerMethodName = methodName;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceAddOrientationChangeListener(objectId, methodName) == 1;
            #else
            Debug.Log($"[JSPluginDevice] Would add orientation change listener with callback to {objectId}.{methodName}");
            return true;
            #endif
        }
        
        /// <summary>
        /// Removes the orientation change listener
        /// </summary>
        /// <returns>True if the listener was successfully removed</returns>
        public static bool RemoveOrientationChangeListener()
        {
            if (!isInitialized)
                Initialize();
                
            orientationListenerObjectId = null;
            orientationListenerMethodName = null;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceRemoveOrientationChangeListener() == 1;
            #else
            Debug.Log("[JSPluginDevice] Would remove orientation change listener");
            return true;
            #endif
        }
        
        /// <summary>
        /// Requests fullscreen mode for the application
        /// </summary>
        /// <returns>True if fullscreen request was successful</returns>
        public static bool RequestFullscreen()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceRequestFullscreen() == 1;
            #else
            Debug.Log("[JSPluginDevice] Would request fullscreen mode");
            return true;
            #endif
        }
        
        /// <summary>
        /// Exits fullscreen mode
        /// </summary>
        /// <returns>True if exiting fullscreen was successful</returns>
        public static bool ExitFullscreen()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceExitFullscreen() == 1;
            #else
            Debug.Log("[JSPluginDevice] Would exit fullscreen mode");
            return true;
            #endif
        }
        
        /// <summary>
        /// Checks if the application is currently in fullscreen mode
        /// </summary>
        /// <returns>True if in fullscreen mode</returns>
        public static bool IsFullscreen()
        {
            if (!isInitialized)
                Initialize();
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginDeviceIsFullscreen() == 1;
            #else
            return Screen.fullScreen;
            #endif
        }
        
        /// <summary>
        /// Checks if vibration is supported by the device.
        /// </summary>
        /// <returns>
        /// <c>true</c> if vibration is supported; 
        /// <c>false</c> otherwise.
        /// In non-WebGL environments, always returns <c>false</c>.
        /// </returns>
        public static bool IsVibrationSupported()
        {
            try
            {
                if (!isInitialized)
                    Initialize();
                    
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginDeviceIsVibrationSupported() == 1;
                #else
                JSPluginErrorHandling.LogEvent("JSPluginDevice", "Vibration is not supported in non-WebGL environments", JSPluginErrorHandling.ErrorSeverity.Info);
                return false;
                #endif
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginDevice", "IsVibrationSupported", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Makes the device vibrate (only works on mobile devices that support vibration)
        /// </summary>
        /// <param name="milliseconds">Duration of vibration in milliseconds</param>
        /// <returns>
        /// True if vibration request was successful.
        /// In non-WebGL environments, always returns false.
        /// </returns>
        public static bool Vibrate(int milliseconds)
        {
            try
            {
                if (!isInitialized)
                    Initialize();
                    
                if (milliseconds <= 0)
                {
                    JSPluginErrorHandling.LogEvent("JSPluginDevice", "Vibration duration must be greater than zero", JSPluginErrorHandling.ErrorSeverity.Warning);
                    return false;
                }
                
                if (!IsVibrationSupported())
                {
                    JSPluginErrorHandling.LogEvent("JSPluginDevice", "Vibration is not supported on this device", JSPluginErrorHandling.ErrorSeverity.Info);
                    return false;
                }
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginDeviceVibrate(milliseconds) == 1;
                #else
                JSPluginErrorHandling.LogEvent("JSPluginDevice", $"Would vibrate device for {milliseconds}ms", JSPluginErrorHandling.ErrorSeverity.Info);
                return false; // Return false to be consistent with IsVibrationSupported
                #endif
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginDevice", "Vibrate", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Called by JavaScript when the device orientation changes
        /// </summary>
        /// <param name="orientationJson">JSON containing orientation information</param>
        public static void OnOrientationChanged(string orientationJson)
        {
            try
            {
                if (!string.IsNullOrEmpty(orientationListenerObjectId) && 
                    !string.IsNullOrEmpty(orientationListenerMethodName))
                {
                    var handler = JSPluginCore.GetObject(orientationListenerObjectId);
                    if (handler != null)
                    {
                        handler.SendMessage(orientationListenerMethodName, orientationJson);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginDevice] Error in orientation change handler: {ex.Message}");
            }
        }
        
        #endregion
    }
}
