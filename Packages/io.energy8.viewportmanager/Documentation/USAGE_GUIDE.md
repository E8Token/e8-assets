# ViewportManager Usage Guide

The **ViewportManager** package provides automatic viewport detection and graphics optimization for Unity applications across different platforms and orientations. This guide covers installation, setup, and usage patterns.

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Core Components](#core-components)
5. [Configuration](#configuration)
6. [Event System](#event-system)
7. [Platform Support](#platform-support)
8. [API Reference](#api-reference)
9. [Examples](#examples)
10. [Troubleshooting](#troubleshooting)

## Overview

### Features

- **Automatic Platform Detection**: Detects device type (Mobile/Desktop), platform (WebGL, iOS, Android, Windows, etc.), and screen orientation
- **Event-Driven Architecture**: React to viewport changes through a clean event system
- **Unity Quality Settings Integration**: Maps platform combinations to Unity Quality Levels
- **Cross-Platform Support**: Works on WebGL, mobile, and desktop platforms
- **WebGL Enhanced Detection**: Uses JavaScript plugins for accurate device detection in browsers
- **Responsive Components**: Base classes for creating responsive UI elements

### Current Status

⚠️ **Note**: Graphics settings application is currently disabled. The system focuses on orientation detection and event broadcasting.

## Installation

1. The package is already included in your project at `Packages/io.energy8.viewportmanager`
2. Unity will automatically import the package
3. No additional dependencies required

## Quick Start

### 1. Basic Setup

1. Create an empty GameObject in your scene
2. Add the `ViewportManagerBootstrap` component
3. Configure the settings in the inspector

```csharp
// The bootstrap component will automatically initialize the ViewportManager
// No additional code required for basic functionality
```

### 2. Listen to Orientation Changes

```csharp
using UnityEngine;
using Energy8.ViewportManager.Core;

public class OrientationHandler : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to viewport context changes
        ViewportManager.OnContextChanged += OnViewportChanged;
    }
    
    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        ViewportManager.OnContextChanged -= OnViewportChanged;
    }
    
    private void OnViewportChanged(ViewportContext context)
    {
        Debug.Log($"Viewport changed: {context.deviceType}, {context.platform}, {context.orientation}");
        
        if (context.orientation == ScreenOrientation.Portrait)
        {
            // Handle portrait mode
            SetPortraitLayout();
        }
        else
        {
            // Handle landscape mode
            SetLandscapeLayout();
        }
    }
    
    private void SetPortraitLayout() { /* Your portrait UI logic */ }
    private void SetLandscapeLayout() { /* Your landscape UI logic */ }
}
```

### 3. Manual Detection

```csharp
// Get current viewport context
var context = ViewportDetector.DetectContext();
Debug.Log($"Current: {context.deviceType}, {context.platform}, {context.orientation}");

// Get recommended quality level
int recommendedQuality = ViewportDetector.GetRecommendedQualityLevel();
Debug.Log($"Recommended Unity Quality Level: {recommendedQuality}");
```

## Core Components

### ViewportManager (Static Class)

The main entry point for the viewport management system.

**Key Properties:**
- `CurrentContext`: Current viewport context (orientation, device type, platform)
- `CurrentConfiguration`: Current viewport configuration
- `IsInitialized`: Whether the system is initialized

**Key Methods:**
- `Initialize(configMatrix)`: Initialize the system with a configuration matrix
- `RefreshContext()`: Force refresh of viewport detection

**Events:**
- `OnContextChanged`: Fired when viewport context changes
- `OnConfigurationChanged`: Fired when configuration changes
- `OnQualityChanged`: Fired when Unity quality level changes
- `OnInitialized`: Fired when system is initialized

### ViewportDetector (Static Class)

Utility class for detecting viewport characteristics.

**Key Methods:**
- `DetectDeviceType()`: Returns `DeviceType.Mobile` or `DeviceType.Desktop`
- `DetectPlatform()`: Returns platform enum (WebGL, Android, iOS, Windows, etc.)
- `DetectOrientation()`: Returns `ScreenOrientation.Portrait` or `ScreenOrientation.Landscape`
- `DetectContext()`: Returns complete `ViewportContext`
- `GetRecommendedQualityLevel()`: Returns recommended Unity quality level (0-5)

### ViewportManagerBootstrap (MonoBehaviour)

Component that automatically initializes and monitors the viewport manager.

**Inspector Settings:**
- `Configuration Matrix`: ScriptableObject with platform mappings
- `Initialize On Awake`: Whether to initialize immediately
- `Don't Destroy On Load`: Persist across scene changes
- `Enable Continuous Detection`: Monitor for changes
- `Detection Interval`: How often to check for changes (seconds)

## Configuration

### ViewportConfigurationMatrix

A ScriptableObject that maps platform combinations to Unity Quality Levels.

**Creating a Configuration Matrix:**

1. Right-click in Project window
2. Create → Energy8 → Viewport Config Matrix
3. Configure the entries in the inspector

**Default Mappings:**

| Device Type | Platform | Orientation | Quality Level |
|-------------|----------|-------------|---------------|
| Mobile      | Android  | Portrait    | 0 (Low)       |
| Mobile      | Android  | Landscape   | 2 (Medium)    |
| Mobile      | iOS      | Portrait    | 0 (Low)       |
| Mobile      | iOS      | Landscape   | 2 (Medium)    |
| Desktop     | WebGL    | Landscape   | 3 (High)      |
| Desktop     | Windows  | Landscape   | 4 (Very High) |

**Configuration Entry Structure:**
```csharp
public class ViewportConfigEntry
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
    public int unityQualityLevel; // 0-5
    public int customTargetFrameRate; // Optional override
    public bool forceDisableVSync; // Optional override
}
```

## Event System

### Using ViewportEventListener

Base class for components that automatically listen to ViewportManager events:

```csharp
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveUI : ViewportEventListener
{
    protected override void OnInitialSetup(ViewportContext initialContext, ViewportConfiguration initialConfiguration)
    {
        // Called when component first gets viewport state
        SetupUI(initialContext);
    }
    
    protected override void OnContextChanged(ViewportContext previousContext, ViewportContext newContext)
    {
        // Called when viewport context changes
        UpdateUI(newContext);
    }
    
    protected override void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
    {
        // Called specifically when orientation changes
        if (newOrientation == ScreenOrientation.Portrait)
        {
            ShowPortraitLayout();
        }
        else
        {
            ShowLandscapeLayout();
        }
    }
    
    private void SetupUI(ViewportContext context) { /* Initial setup */ }
    private void UpdateUI(ViewportContext context) { /* Update logic */ }
    private void ShowPortraitLayout() { /* Portrait UI */ }
    private void ShowLandscapeLayout() { /* Landscape UI */ }
}
```

### Manual Event Subscription

```csharp
public class CustomHandler : MonoBehaviour
{
    private void Start()
    {
        ViewportManager.OnContextChanged += HandleContextChange;
        ViewportManager.OnQualityChanged += HandleQualityChange;
    }
    
    private void OnDestroy()
    {
        ViewportManager.OnContextChanged -= HandleContextChange;
        ViewportManager.OnQualityChanged -= HandleQualityChange;
    }
    
    private void HandleContextChange(ViewportContext context)
    {
        // Handle context changes
    }
    
    private void HandleQualityChange(int qualityLevel)
    {
        // Handle quality level changes
    }
}
```

## Platform Support

### WebGL

- Enhanced device detection using JavaScript plugins
- Automatic mobile browser vs desktop detection
- Orientation change detection
- User agent analysis

**WebGL-Specific Features:**
```csharp
// Get detailed device info (WebGL only)
var deviceInfo = ViewportDetector.GetDeviceInfo();
Debug.Log($"User Agent: {deviceInfo.userAgent}");
Debug.Log($"Touch Support: {deviceInfo.touchSupport}");
Debug.Log($"Device Pixel Ratio: {deviceInfo.devicePixelRatio}");
```

### Mobile (iOS/Android)

- Optimized for battery life and thermal management
- Portrait/Landscape orientation handling
- Touch device detection

### Desktop (Windows/macOS/Linux)

- Full quality settings available
- High refresh rate support
- Multi-monitor awareness

## API Reference

### Data Structures

#### ViewportContext
```csharp
public struct ViewportContext
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
}
```

#### ViewportInfo
```csharp
public struct ViewportInfo
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
    public int screenWidth;
    public int screenHeight;
}
```

#### DeviceInfo (WebGL)
```csharp
public class DeviceInfo
{
    public string userAgent;
    public int screenWidth;
    public int screenHeight;
    public float devicePixelRatio;
    public bool isMobile;
    public bool touchSupport;
    public string platform;
}
```

### Enums

#### ScreenOrientation
```csharp
public enum ScreenOrientation
{
    Landscape,
    Portrait
}
```

#### DeviceType
```csharp
public enum DeviceType
{
    Desktop,
    Mobile
}
```

#### Platform
```csharp
public enum Platform
{
    WebGL,
    Android,
    iOS,
    Windows,
    macOS,
    Linux,
    Mobile,
    Desktop,
    Console
}
```

## Examples

### Example 1: Responsive Canvas Scaler

```csharp
using UnityEngine;
using UnityEngine.UI;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveCanvasScaler : ViewportEventListener
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080, 1920);
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920, 1080);
    
    protected override void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
    {
        if (canvasScaler == null) return;
        
        if (newOrientation == ScreenOrientation.Portrait)
        {
            canvasScaler.referenceResolution = portraitReferenceResolution;
        }
        else
        {
            canvasScaler.referenceResolution = landscapeReferenceResolution;
        }
    }
}
```

### Example 2: Platform-Specific UI

```csharp
using UnityEngine;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class PlatformSpecificUI : ViewportEventListener
{
    [SerializeField] private GameObject mobileUI;
    [SerializeField] private GameObject desktopUI;
    [SerializeField] private GameObject touchControls;
    
    protected override void OnInitialSetup(ViewportContext initialContext, ViewportConfiguration initialConfiguration)
    {
        UpdateUIForPlatform(initialContext);
    }
    
    protected override void OnDeviceTypeChanged(DeviceType previousDeviceType, DeviceType newDeviceType)
    {
        UpdateUIForPlatform(ViewportManager.CurrentContext);
    }
    
    private void UpdateUIForPlatform(ViewportContext context)
    {
        bool isMobile = context.deviceType == DeviceType.Mobile;
        
        mobileUI.SetActive(isMobile);
        desktopUI.SetActive(!isMobile);
        touchControls.SetActive(isMobile || ViewportDetector.IsTouchDevice());
    }
}
```

### Example 3: Testing Orientation Changes

```csharp
using UnityEngine;
using Energy8.ViewportManager.Components;

public class OrientationTester : MonoBehaviour
{
    private void Start()
    {
        // Add ViewportOrientationTester component for automatic logging
        var tester = gameObject.AddComponent<ViewportOrientationTester>();
        
        // Or manually test
        InvokeRepeating(nameof(LogCurrentState), 1f, 2f);
    }
    
    private void LogCurrentState()
    {
        var context = ViewportDetector.DetectContext();
        Debug.Log($"Current state: {context}");
        Debug.Log($"Screen size: {Screen.width}x{Screen.height}");
        Debug.Log($"Recommended quality: {ViewportDetector.GetRecommendedQualityLevel()}");
    }
}
```

## Troubleshooting

### Common Issues

#### 1. ViewportManager Not Initialized

**Problem**: Events not firing, null reference exceptions

**Solution**: 
- Ensure `ViewportManagerBootstrap` component is in your scene
- Check that `Initialize On Awake` is enabled
- Manually call `ViewportManager.Initialize()` if needed

#### 2. Orientation Not Detected Correctly

**Problem**: Wrong orientation reported

**Solution**:
- Test on actual devices, not just Unity Editor
- For WebGL, ensure JavaScript plugins are working
- Check browser console for JavaScript errors

#### 3. Events Not Firing

**Problem**: Subscribed to events but not receiving callbacks

**Solution**:
- Verify subscription happens after ViewportManager initialization
- Check that you're not unsubscribing accidentally
- Ensure continuous detection is enabled in bootstrap

#### 4. WebGL Detection Issues

**Problem**: Incorrect device type detection in WebGL

**Solution**:
- Check browser console for JavaScript errors
- Verify that `.jslib` files are included in build
- Test in different browsers

### Debug Tools

#### Enable Debug Logging

```csharp
// In ViewportManagerBootstrap inspector
// Enable "Enable Debug Logging"
// Set "Debug Info Key" (default F12)

// Or manually:
Debug.Log(ViewportManager.GetSystemInfo());
```

#### Manual Context Detection

```csharp
// Force refresh detection
ViewportManager.RefreshContext();

// Get current state
var context = ViewportManager.CurrentContext;
var config = ViewportManager.CurrentConfiguration;
Debug.Log($"Context: {context}, Config: {config}");
```

#### Test Component

Add `ViewportOrientationTester` component to any GameObject for automatic logging of orientation changes.

### Performance Considerations

1. **Detection Interval**: Set appropriate detection interval in bootstrap (default 1 second)
2. **Event Subscriptions**: Always unsubscribe in `OnDestroy()` to prevent memory leaks
3. **Continuous Detection**: Disable if not needed to save performance
4. **WebGL**: JavaScript detection has minimal overhead

### Best Practices

1. **Initialization**: Use `ViewportManagerBootstrap` for automatic setup
2. **Event Handling**: Inherit from `ViewportEventListener` for automatic event management
3. **Configuration**: Create custom configuration matrices for your specific needs
4. **Testing**: Test on actual devices, especially for mobile orientation detection
5. **Persistence**: Use `DontDestroyOnLoad` for components that need to persist across scenes

This completes the comprehensive usage guide for the ViewportManager package. The system provides a robust foundation for responsive Unity applications across multiple platforms.