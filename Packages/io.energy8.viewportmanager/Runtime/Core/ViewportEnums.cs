using System;

namespace Energy8.ViewportManager.Core
{
    /// <summary>
    /// Screen orientation detection
    /// </summary>
    public enum ScreenOrientation
    {
        Any,
        Landscape,
        Portrait
    }

    /// <summary>
    /// Device type classification
    /// </summary>
    public enum DeviceType
    {
        Any,
        Desktop,
        Mobile,
        Tablet
    }

    /// <summary>
    /// Platform detection
    /// </summary>
    public enum Platform
    {
        Any,
        WebGL,
        Mobile,
        Desktop,
        Console,
        Android,
        iOS,
        Windows,
        macOS,
        Linux
    }

    /// <summary>
    /// Complete viewport context
    /// </summary>
    [Serializable]
    public struct ViewportContext
    {
        public ScreenOrientation orientation;
        public DeviceType deviceType;
        public Platform platform;
        public int screenWidth;
        public int screenHeight;
        public float devicePixelRatio;

        public ViewportContext(ScreenOrientation orientation, DeviceType deviceType, Platform platform, int screenWidth, int screenHeight, float devicePixelRatio = 1.0f)
        {
            this.orientation = orientation;
            this.deviceType = deviceType;
            this.platform = platform;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.devicePixelRatio = devicePixelRatio;
        }

        public override string ToString()
        {
            return $"{orientation}+{deviceType}+{platform} ({screenWidth}x{screenHeight})";
        }

        public override bool Equals(object obj)
        {
            if (obj is ViewportContext other)
            {
                return orientation == other.orientation && 
                       deviceType == other.deviceType && 
                       platform == other.platform &&
                       screenWidth == other.screenWidth &&
                       screenHeight == other.screenHeight;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(orientation, deviceType, platform, screenWidth, screenHeight);
        }
    }

    /// <summary>
    /// Device information from WebGL
    /// </summary>
    [Serializable]
    public struct DeviceInfo
    {
        public string userAgent;
        public int screenWidth;
        public int screenHeight;
        public float devicePixelRatio;
        public bool isMobile;
        public string platform;
        public bool touchSupport;
    }
}