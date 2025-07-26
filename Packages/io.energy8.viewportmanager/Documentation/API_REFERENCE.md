# ViewportManager API Reference

Complete API documentation for the ViewportManager package.

## Table of Contents

1. [Core Classes](#core-classes)
2. [Components](#components)
3. [Configuration](#configuration)
4. [Data Structures](#data-structures)
5. [Enums](#enums)
6. [Events](#events)
7. [WebGL Plugins](#webgl-plugins)

## Core Classes

### ViewportManager

**Namespace**: `Energy8.ViewportManager`

**Description**: Main static class that handles automatic detection and configuration switching.

#### Properties

```csharp
public static ViewportContext CurrentContext { get; private set; }
```
Current viewport context (orientation, device type, platform).

```csharp
public static ViewportConfiguration CurrentConfiguration { get; private set; }
```
Current viewport configuration.

```csharp
public static int CurrentQualityLevel { get; }
```
Current Unity quality level (read-only).

```csharp
public static bool IsInitialized { get; private set; }
```
Whether the system is initialized.

```csharp
public static ViewportConfigurationMatrix ConfigMatrix { get; private set; }
```
Configuration matrix being used.

#### Methods

```csharp
public static void Initialize(ViewportConfigurationMatrix configMatrix = null)
```
Initialize viewport manager with configuration matrix. If no matrix provided, attempts to load default from Resources.

**Parameters:**
- `configMatrix`: Optional configuration matrix. If null, loads from Resources or creates fallback.

```csharp
public static void RefreshContext()
```
Force refresh of viewport context. Checks for changes and updates if necessary.

```csharp
public static void SetQualityLevel(int qualityLevel)
```
Manually set quality profile (overrides automatic detection).

**Parameters:**
- `qualityLevel`: Unity quality level (0-5)

```csharp
public static void SetConfigurationMatrix(ViewportConfigurationMatrix matrix)
```
Update configuration matrix at runtime.

**Parameters:**
- `matrix`: New configuration matrix to use

```csharp
public static string GetSystemInfo()
```
Get comprehensive debug information about current system state.

**Returns:** Formatted string with current context, configuration, and system info.

#### Events

```csharp
public static event Action<ViewportContext> OnContextChanged
```
Fired when viewport context changes (orientation, device, platform).

```csharp
public static event Action<ViewportConfiguration> OnConfigurationChanged
```
Fired when viewport configuration changes.

```csharp
public static event Action<int> OnQualityChanged
```
Fired when Unity quality level changes.

```csharp
public static event Action OnInitialized
```
Fired when system is initialized.

---

### ViewportDetector

**Namespace**: `Energy8.ViewportManager.Core`

**Description**: Static utility class for detecting viewport characteristics across different platforms.

#### Methods

```csharp
public static DeviceType DetectDeviceType()
```
Detect the current device type (Desktop/Mobile).

**Returns:** `DeviceType.Mobile` or `DeviceType.Desktop`

**Detection Logic:**
- WebGL: Uses JavaScript plugin for enhanced detection
- Fallback: Checks `Application.isMobilePlatform` and screen size

```csharp
public static Platform DetectPlatform()
```
Detect the current platform.

**Returns:** Platform enum value based on compilation defines

```csharp
public static ScreenOrientation DetectOrientation()
```
Detect the current screen orientation.

**Returns:** `ScreenOrientation.Portrait` or `ScreenOrientation.Landscape`

**Detection Logic:**
- WebGL: Uses JavaScript plugin
- Fallback: Compares screen width vs height

```csharp
public static DeviceInfo GetDeviceInfo()
```
Get detailed device information (enhanced on WebGL).

**Returns:** `DeviceInfo` structure with comprehensive device data

```csharp
public static string GetUserAgent()
```
Get user agent string.

**Returns:** Browser user agent (WebGL) or system info (other platforms)

```csharp
public static bool IsTouchDevice()
```
Check if current device supports touch input.

**Returns:** `true` if touch is supported

```csharp
public static int GetRecommendedQualityLevel()
```
Get Unity Quality Level recommendation based on device characteristics.

**Returns:** Recommended quality level (0-5)

**Recommendation Logic:**
- Mobile + Portrait: Level 0 (optimized for weak devices)
- Mobile + Landscape: Level 2 (better performance but still mobile)
- Desktop + WebGL: Level 3 (good performance in browsers)
- Desktop + Native: Level 4 (best performance)

```csharp
public static ViewportContext DetectContext()
```
Detect current viewport context (combined detection).

**Returns:** Complete `ViewportContext` with orientation, device type, and platform

```csharp
public static bool HasContextChanged(ViewportContext previous)
```
Check if context has changed since previous detection.

**Parameters:**
- `previous`: Previous context to compare against

**Returns:** `true` if context has changed

```csharp
public static ViewportInfo DetectViewport()
```
Detect complete viewport information including screen dimensions.

**Returns:** `ViewportInfo` structure with context and screen data

## Components

### ViewportManagerBootstrap

**Namespace**: `Energy8.ViewportManager.Components`

**Description**: MonoBehaviour that automatically initializes and monitors the viewport manager.

#### Inspector Properties

```csharp
[SerializeField] private ViewportConfigurationMatrix configurationMatrix
```
Configuration matrix to use for initialization.

```csharp
[SerializeField] private bool initializeOnAwake = true
```
Whether to initialize ViewportManager in Awake() or Start().

```csharp
[SerializeField] private bool dontDestroyOnLoad = true
```
Whether to persist this GameObject across scene changes.

```csharp
[SerializeField] private bool enableContinuousDetection = true
```
Whether to continuously monitor for viewport changes.

```csharp
[SerializeField] private float detectionInterval = 1.0f
```
How often to check for changes (in seconds).

```csharp
[SerializeField] private bool enableDebugLogging = false
```
Whether to enable debug logging.

```csharp
[SerializeField] private KeyCode debugInfoKey = KeyCode.F12
```
Key to press for debug information output.

#### Public Methods

```csharp
public void InitializeViewportManager()
```
Manually initialize the viewport manager.

```csharp
public void RefreshViewport()
```
Force refresh viewport detection.

```csharp
public void PrintDebugInfo()
```
Print debug information to console.

```csharp
public void SetConfigurationMatrix(ViewportConfigurationMatrix matrix)
```
Set configuration matrix at runtime.

---

### ViewportEventListener

**Namespace**: `Energy8.ViewportManager.Components`

**Description**: Abstract base class for components that automatically listen to ViewportManager events.

#### Protected Properties

```csharp
protected ViewportContext CurrentContext { get; private set; }
```
Current viewport context.

```csharp
protected ViewportConfiguration CurrentConfiguration { get; private set; }
```
Current viewport configuration.

```csharp
protected bool IsSubscribed { get; private set; }
```
Whether currently subscribed to events.

#### Inspector Properties

```csharp
[SerializeField] private bool enableLogging = false
```
Whether to enable debug logging.

```csharp
[SerializeField] private bool autoSubscribe = true
```
Whether to automatically subscribe to events in Start().

```csharp
[SerializeField] private bool persistAcrossScenes = true
```
Whether to persist across scene loads with DontDestroyOnLoad.

#### Public Methods

```csharp
public void SubscribeToEvents()
```
Manually subscribe to ViewportManager events.

```csharp
public void UnsubscribeFromEvents()
```
Manually unsubscribe from ViewportManager events.

#### Virtual Methods for Override

```csharp
protected virtual void OnInitialSetup(ViewportContext initialContext, ViewportConfiguration initialConfiguration)
```
Called when component is first set up with initial viewport state.

```csharp
protected virtual void OnManagerInitialized()
```
Called when ViewportManager is initialized.

```csharp
protected virtual void OnContextChanged(ViewportContext previousContext, ViewportContext newContext)
```
Called when viewport context changes.

```csharp
protected virtual void OnConfigurationChanged(ViewportConfiguration previousConfiguration, ViewportConfiguration newConfiguration)
```
Called when viewport configuration changes.

```csharp
protected virtual void OnQualityChanged(int qualityLevel)
```
Called when Unity quality level changes.

```csharp
protected virtual void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
```
Called specifically when orientation changes.

```csharp
protected virtual void OnDeviceTypeChanged(DeviceType previousDeviceType, DeviceType newDeviceType)
```
Called specifically when device type changes.

```csharp
protected virtual void OnPlatformChanged(Platform previousPlatform, Platform newPlatform)
```
Called specifically when platform changes.

---

### ViewportOrientationTester

**Namespace**: `Energy8.ViewportManager.Components`

**Description**: Simple component for testing viewport orientation detection without applying graphics settings.

#### Inspector Properties

```csharp
[SerializeField] private bool enableLogging = true
```
Whether to log orientation changes.

```csharp
[SerializeField] private float checkInterval = 1.0f
```
How often to check for changes (in seconds).

#### Public Methods

```csharp
public void LogCurrentInfo()
```
Log current viewport information to console.

```csharp
public void ForceCheck()
```
Force check for viewport changes.

---

### ViewportResponsiveComponent

**Namespace**: `Energy8.ViewportManager.Components`

**Description**: Base class for creating responsive UI components.

#### Virtual Methods

```csharp
protected virtual void OnViewportChanged(ViewportInfo info)
```
Called when viewport information changes.

**Parameters:**
- `info`: New viewport information

## Configuration

### ViewportConfiguration

**Namespace**: `Energy8.ViewportManager.Configuration`

**Description**: Simple viewport configuration that maps to Unity Quality Settings.

#### Properties

```csharp
public int unityQualityLevel = 2
```
Unity Quality Level (0-5). Maps to Edit > Project Settings > Quality.

```csharp
public int customTargetFrameRate = 0
```
Optional override for target frame rate. 0 uses Unity Quality Settings default.

```csharp
public bool forceDisableVSync = false
```
Force disable VSync regardless of Quality Settings.

#### Constructors

```csharp
public ViewportConfiguration()
```
Default constructor. Sets quality level to 2 (Medium).

```csharp
public ViewportConfiguration(int qualityLevel)
```
Constructor with specific quality level.

**Parameters:**
- `qualityLevel`: Unity quality level (0-5)

#### Methods

```csharp
public void ApplyToUnity()
```
Apply this configuration to Unity's graphics settings.

**Note:** Currently disabled - only logs configuration.

```csharp
public static string GetCurrentUnityQualityInfo()
```
Get current Unity quality settings info for debugging.

**Returns:** Formatted string with current quality settings

---

### ViewportConfigurationMatrix

**Namespace**: `Energy8.ViewportManager.Configuration`

**Description**: ScriptableObject that maps viewport contexts to Unity Quality Levels.

#### Properties

```csharp
[SerializeField] private ViewportConfigEntry[] configurations
```
Array of configuration entries.

```csharp
[SerializeField] private int defaultUnityQualityLevel = 2
```
Default fallback quality level.

#### Methods

```csharp
public ViewportConfiguration GetConfiguration(ViewportContext context)
```
Get configuration for the given viewport context.

**Parameters:**
- `context`: Viewport context to find configuration for

**Returns:** Matching `ViewportConfiguration` or default if no match found

```csharp
public ViewportConfiguration GetConfiguration(ViewportInfo info)
```
Get configuration for viewport info (converts to ViewportContext first).

**Parameters:**
- `info`: Viewport info to find configuration for

**Returns:** Matching `ViewportConfiguration`

```csharp
public ViewportContext[] GetAllContexts()
```
Get all available viewport contexts from the matrix.

**Returns:** Array of all configured contexts

```csharp
public string GetDebugInfo()
```
Get debug information about the matrix.

**Returns:** Formatted string with matrix information

#### Nested Classes

##### ViewportConfigEntry

```csharp
[Serializable]
public class ViewportConfigEntry
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
    public int unityQualityLevel;
    public int customTargetFrameRate;
    public bool forceDisableVSync;
}
```

**Methods:**

```csharp
public bool MatchesContext(ViewportContext context)
```
Check if this entry matches the given context.

**Parameters:**
- `context`: Context to match against

**Returns:** `true` if all fields match

## Data Structures

### ViewportContext

**Namespace**: `Energy8.ViewportManager.Core`

**Description**: Complete viewport context structure.

```csharp
[Serializable]
public struct ViewportContext
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
}
```

#### Constructors

```csharp
public ViewportContext(ScreenOrientation orientation, DeviceType deviceType, Platform platform)
```

#### Methods

```csharp
public override string ToString()
```
**Returns:** Format: "Orientation+DeviceType+Platform"

```csharp
public override bool Equals(object obj)
```
Compares all fields for equality.

```csharp
public override int GetHashCode()
```
Generates hash code from all fields.

---

### ViewportInfo

**Namespace**: `Energy8.ViewportManager.Core`

**Description**: Complete viewport information including screen dimensions.

```csharp
[Serializable]
public struct ViewportInfo
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
    public int screenWidth;
    public int screenHeight;
}
```

#### Constructors

```csharp
public ViewportInfo(ScreenOrientation orientation, DeviceType deviceType, Platform platform, int screenWidth, int screenHeight)
```

#### Methods

```csharp
public override string ToString()
```
**Returns:** Format: "DeviceType/Platform/Orientation (WidthxHeight)"

---

### DeviceInfo

**Namespace**: `Energy8.ViewportManager.Plugins`

**Description**: Device information structure (primarily for WebGL).

```csharp
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
```

## Enums

### ScreenOrientation

**Namespace**: `Energy8.ViewportManager.Core`

```csharp
public enum ScreenOrientation
{
    Landscape,
    Portrait
}
```

### DeviceType

**Namespace**: `Energy8.ViewportManager.Core`

```csharp
public enum DeviceType
{
    Desktop,
    Mobile
}
```

### Platform

**Namespace**: `Energy8.ViewportManager.Core`

```csharp
public enum Platform
{
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
```

## Events

All events are static events on the `ViewportManager` class:

### OnContextChanged

```csharp
public static event Action<ViewportContext> OnContextChanged
```

**Triggered when:** Viewport context changes (orientation, device type, or platform)

**Parameters:** New `ViewportContext`

### OnConfigurationChanged

```csharp
public static event Action<ViewportConfiguration> OnConfigurationChanged
```

**Triggered when:** Viewport configuration changes

**Parameters:** New `ViewportConfiguration`

### OnQualityChanged

```csharp
public static event Action<int> OnQualityChanged
```

**Triggered when:** Unity quality level changes

**Parameters:** New quality level (0-5)

### OnInitialized

```csharp
public static event Action OnInitialized
```

**Triggered when:** ViewportManager system is initialized

**Parameters:** None

## WebGL Plugins

### ViewportDetectionPlugin

**Namespace**: `Energy8.ViewportManager.Plugins`

**Description**: WebGL plugin for enhanced device detection using JavaScript.

#### Methods

```csharp
public string GetUserAgent()
```
Get browser's user agent string.

```csharp
public string GetDeviceInfo()
```
Get detailed device information as JSON string.

```csharp
public bool IsMobileDevice()
```
Check if current device is mobile based on user agent and screen characteristics.

```csharp
public string GetScreenOrientation()
```
Get current screen orientation ("landscape" or "portrait").

```csharp
public string GetPlatformType()
```
Get platform type string.

### JavaScript Library Functions

**File**: `ViewportDetection.jslib`

#### ViewportDetectionGetUserAgent

Returns browser's navigator.userAgent string.

#### ViewportDetectionGetScreenInfo

Returns JSON object with comprehensive screen information:

```javascript
{
    width: screen.width,
    height: screen.height,
    availWidth: screen.availWidth,
    availHeight: screen.availHeight,
    devicePixelRatio: window.devicePixelRatio,
    orientationAngle: screen.orientation.angle,
    isTouchDevice: boolean,
    dpi: 96,
    orientation: "Landscape" | "Portrait"
}
```

#### ViewportDetectionIsTouchDevice

Returns 1 if touch is supported, 0 otherwise.

#### ViewportDetectionDetectDeviceType

Returns "Mobile" or "Desktop" based on user agent analysis and screen characteristics.

---

This completes the comprehensive API reference for the ViewportManager package. All classes, methods, properties, and events are documented with their signatures, parameters, return values, and usage notes.