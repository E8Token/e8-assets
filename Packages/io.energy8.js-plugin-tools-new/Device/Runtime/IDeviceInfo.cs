using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;

namespace Energy8.JSPluginTools.Device
{
    /// <summary>
    /// Interface for accessing device information in WebGL builds
    /// </summary>
    public interface IDeviceInfo
    {
        /// <summary>
        /// Gets the user agent string of the browser
        /// </summary>
        /// <returns>The user agent string</returns>
        Task<string> GetUserAgent();

        /// <summary>
        /// Gets information about the browser (name, version)
        /// </summary>
        /// <returns>Browser information object</returns>
        Task<BrowserInfo> GetBrowserInfo();

        /// <summary>
        /// Gets information about the operating system (name, version)
        /// </summary>
        /// <returns>OS information object</returns>
        Task<OSInfo> GetOSInfo();

        /// <summary>
        /// Gets information about the device screen (width, height, pixel ratio)
        /// </summary>
        /// <returns>Screen information object</returns>
        Task<ScreenInfo> GetScreenInfo();

        /// <summary>
        /// Checks if this device is a mobile device
        /// </summary>
        /// <returns>True if the device is mobile, false otherwise</returns>
        Task<bool> IsMobileDevice();

        /// <summary>
        /// Gets the preferred language of the user's browser
        /// </summary>
        /// <returns>The language code (e.g., "en-US")</returns>
        Task<string> GetLanguage();

        /// <summary>
        /// Gets the device's current time zone
        /// </summary>
        /// <returns>Time zone information</returns>
        Task<TimeZoneInfo> GetTimeZone();

        /// <summary>
        /// Gets hardware information about the device
        /// </summary>
        /// <returns>Hardware information object</returns>
        Task<HardwareInfo> GetHardwareInfo();
    }

    /// <summary>
    /// Contains information about the browser
    /// </summary>
    [Serializable]
    public class BrowserInfo
    {
        public string Name;
        public string Version;
        public string Engine;
        public bool CookiesEnabled;
        public bool JavaScriptEnabled;
    }

    /// <summary>
    /// Contains information about the operating system
    /// </summary>
    [Serializable]
    public class OSInfo
    {
        public string Name;
        public string Version;
        public string Architecture;
        public bool IsMobile;
    }

    /// <summary>
    /// Contains information about the device's screen
    /// </summary>
    [Serializable]
    public class ScreenInfo
    {
        public int Width;
        public int Height;
        public float PixelRatio;
        public int ColorDepth;
        public bool TouchScreen;
        public int RefreshRate;
    }

    /// <summary>
    /// Contains information about the device's time zone
    /// </summary>
    [Serializable]
    public class TimeZoneInfo
    {
        public string Id;
        public string DisplayName;
        public int OffsetMinutes;
        public bool DaylightSavingTime;
    }

    /// <summary>
    /// Contains hardware information about the device
    /// </summary>
    [Serializable]
    public class HardwareInfo
    {
        public int ProcessorCores;
        public int DeviceMemory;
        public string GpuRenderer;
        public string GpuVendor;
        public bool HasBattery;
        public float BatteryLevel;
    }
}