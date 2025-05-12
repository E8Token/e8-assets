# Device Module Documentation

## Overview

The Device module provides access to information about the user's device, browser, and operating system in WebGL builds. It allows Unity applications to adapt their behavior based on device capabilities, browser features, and system information.

## Features

- **Browser Detection**: Identify browser name, version, and engine
- **OS Information**: Access operating system details
- **Screen Properties**: Get screen dimensions and pixel ratios
- **Device Type Detection**: Determine if running on a mobile device
- **Language Settings**: Access user's preferred language
- **Timezone Information**: Get local timezone details
- **Hardware Capabilities**: Access information about device hardware

## Classes and Components

### IDeviceInfo

The main interface for accessing device information:

```csharp
public interface IDeviceInfo
{
    Task<string> GetUserAgent();
    Task<BrowserInfo> GetBrowserInfo();
    Task<OSInfo> GetOSInfo();
    Task<ScreenInfo> GetScreenInfo();
    Task<bool> IsMobileDevice();
    Task<string> GetLanguage();
    Task<TimeZoneInfo> GetTimeZone();
    Task<HardwareInfo> GetHardwareInfo();
}
```

### DeviceInfo

The implementation of `IDeviceInfo` that provides access to device information through the Communication module.

### DeviceInfoBehaviour

A MonoBehaviour wrapper for DeviceInfo that allows finding it with `FindObjectOfType`.

### Data Models

#### BrowserInfo

```csharp
public class BrowserInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Engine { get; set; }
    public bool CookiesEnabled { get; set; }
    public bool JavaScriptEnabled { get; set; }
}
```

#### OSInfo

```csharp
public class OSInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Architecture { get; set; }
}
```

#### ScreenInfo

```csharp
public class ScreenInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int AvailWidth { get; set; }
    public int AvailHeight { get; set; }
    public float PixelRatio { get; set; }
    public int ColorDepth { get; set; }
    public string Orientation { get; set; }
}
```

#### TimeZoneInfo

```csharp
public class TimeZoneInfo
{
    public string Name { get; set; }
    public int Offset { get; set; }
    public bool IsDST { get; set; }
}
```

#### HardwareInfo

```csharp
public class HardwareInfo
{
    public int LogicalCores { get; set; }
    public string DeviceMemory { get; set; }
    public bool TouchSupported { get; set; }
    public int MaxTouchPoints { get; set; }
    public bool HardwareConcurrency { get; set; }
}
```

## Usage

### Basic Device Information

```csharp
// Create device info with the communication service
var communicationService = new CommunicationService(pluginCore.MessageBus);
var deviceInfo = new DeviceInfo(communicationService);

// Get browser information
try {
    var browserInfo = await deviceInfo.GetBrowserInfo();
    Debug.Log($"Browser: {browserInfo.Name} {browserInfo.Version}");
    Debug.Log($"Engine: {browserInfo.Engine}");
    Debug.Log($"Cookies enabled: {browserInfo.CookiesEnabled}");
} 
catch (Exception ex) {
    Debug.LogError($"Failed to get browser info: {ex.Message}");
}

// Get OS information
var osInfo = await deviceInfo.GetOSInfo();
Debug.Log($"OS: {osInfo.Name} {osInfo.Version} ({osInfo.Architecture})");
```

### Responsive Game Design

```csharp
// Adjust game settings based on device type and screen
async void ConfigureGameSettings()
{
    // Check if running on a mobile device
    bool isMobile = await deviceInfo.IsMobileDevice();
    
    // Get screen information
    var screenInfo = await deviceInfo.GetScreenInfo();
    
    // Apply settings based on device type and screen
    if (isMobile) {
        // Mobile-specific settings
        gameSettings.graphicsQuality = GraphicsQuality.Medium;
        gameSettings.uiScale = screenInfo.PixelRatio;
        gameSettings.enableTouchControls = true;
    } else {
        // Desktop-specific settings
        gameSettings.graphicsQuality = GraphicsQuality.High;
        gameSettings.uiScale = 1.0f;
        gameSettings.enableTouchControls = false;
    }
    
    // Adjust rendering resolution based on screen size
    if (screenInfo.Width < 1280) {
        gameSettings.renderScale = 0.8f;
    } else if (screenInfo.Width >= 2560) {
        gameSettings.renderScale = 1.2f;
    } else {
        gameSettings.renderScale = 1.0f;
    }
    
    // Apply the settings
    ApplyGameSettings(gameSettings);
}
```

### Localization

```csharp
// Set up localization based on user's browser language
async void SetupLocalization()
{
    string language = await deviceInfo.GetLanguage();
    
    // Extract primary language code (e.g., "en-US" -> "en")
    string primaryLanguage = language.Split('-')[0].ToLower();
    
    // Set up localization based on language
    switch (primaryLanguage) {
        case "en":
            localizationManager.CurrentLanguage = Language.English;
            break;
        case "es":
            localizationManager.CurrentLanguage = Language.Spanish;
            break;
        case "fr":
            localizationManager.CurrentLanguage = Language.French;
            break;
        // Add more languages as needed
        default:
            localizationManager.CurrentLanguage = Language.English; // Default to English
            break;
    }
    
    // Apply localization
    localizationManager.ApplyLocalization();
}
```

### Time-Based Features

```csharp
// Adjust game features based on local time
async void ConfigureTimeBasedFeatures()
{
    // Get timezone information
    var timeZoneInfo = await deviceInfo.GetTimeZone();
    
    // Get local time based on UTC and timezone offset
    DateTime utcNow = DateTime.UtcNow;
    DateTime localTime = utcNow.AddMinutes(timeZoneInfo.Offset);
    
    // Configure day/night cycle based on local time
    int hour = localTime.Hour;
    if (hour >= 6 && hour < 18) {
        // Day time settings
        environmentController.SetDayTime();
    } else {
        // Night time settings
        environmentController.SetNightTime();
    }
    
    // Handle DST-specific features if needed
    if (timeZoneInfo.IsDST) {
        // Special seasonal content
        contentManager.EnableSeasonalContent("summer");
    }
}
```

### Hardware-Based Performance Settings

```csharp
// Configure performance settings based on hardware capabilities
async void ConfigurePerformanceSettings()
{
    var hardwareInfo = await deviceInfo.GetHardwareInfo();
    
    // Adjust settings based on processor cores
    int targetFPS = 60;
    int particleMultiplier = 1;
    
    if (hardwareInfo.LogicalCores <= 2) {
        // Low-end device
        targetFPS = 30;
        particleMultiplier = 0;
        QualitySettings.SetQualityLevel(0); // Lowest quality
    } else if (hardwareInfo.LogicalCores <= 4) {
        // Mid-range device
        targetFPS = 60;
        particleMultiplier = 1;
        QualitySettings.SetQualityLevel(2); // Medium quality
    } else {
        // High-end device
        targetFPS = 60;
        particleMultiplier = 2;
        QualitySettings.SetQualityLevel(5); // Highest quality
    }
    
    // Apply settings
    Application.targetFrameRate = targetFPS;
    particleSystem.emissionRate *= particleMultiplier;
}
```

## JavaScript API

In JavaScript, the Device module provides these methods:

```javascript
// Get user agent string
const userAgent = Energy8JSPluginTools.Device.getUserAgent();

// Get browser information
const browserInfo = Energy8JSPluginTools.Device.getBrowserInfo();
console.log("Browser:", browserInfo.Name, browserInfo.Version);

// Check if device is mobile
const isMobile = Energy8JSPluginTools.Device.isMobileDevice();

// Get screen information
const screenInfo = Energy8JSPluginTools.Device.getScreenInfo();
console.log("Screen:", screenInfo.Width, "x", screenInfo.Height);

// Get language and timezone
const language = Energy8JSPluginTools.Device.getLanguage();
const timeZone = Energy8JSPluginTools.Device.getTimeZone();
```

## Best Practices

1. Cache device information that doesn't change during gameplay to reduce API calls.
2. Handle API failures gracefully with fallback values when device information can't be retrieved.
3. Use the device type (mobile/desktop) to adjust control schemes and UI layouts.
4. Consider offering a manual override for auto-detected settings to accommodate user preferences.
5. For performance-critical features, test across different device capabilities with varying hardware specs.
6. Be careful with time zone data when implementing time-based features, as it may not be 100% accurate.
7. Remember that some browser features might be restricted or disabled in certain browsers or configurations.
8. Use screen DPI information to scale UI elements appropriately across different devices.
9. Implement progressive feature reduction for low-end devices rather than a binary high/low setting.