# ViewportManager Package

Universal viewport management system for Unity that automatically detects device orientation, platform, and screen characteristics.

## Features

- **Platform Detection**: Automatically detects device type, platform, and orientation
- **Event-Driven Architecture**: React to viewport changes through a clean event system
- **Cross-Platform Support**: Works on WebGL, mobile, and desktop platforms
- **Screen Size Monitoring**: Track screen size and aspect ratio changes
- **Minimal Configuration**: Simple setup with automatic detection

## Installation

1. Copy the `io.energy8.viewportmanager` package to your project's `Packages` folder
2. The package will be automatically imported by Unity

## Quick Start

### 1. Add ViewportManager to Your Scene

1. Create an empty GameObject named "ViewportManager"
2. Add the `ViewportManagerBootstrap` component
3. Configure monitoring settings as needed

### 2. Listen to Viewport Changes

**Simple Event Listener:**
```csharp
using Energy8.ViewportManager.Core;

public class MyUIController : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to orientation changes
        ViewportManager.OnOrientationChanged += OnOrientationChanged;
        
        // Subscribe to screen size changes
        ViewportManager.OnScreenSizeChanged += OnScreenSizeChanged;
        
        // Subscribe to full context changes
        ViewportManager.OnContextChanged += OnContextChanged;
    }
    
    private void OnDestroy()
    {
        // Don't forget to unsubscribe!
        ViewportManager.OnOrientationChanged -= OnOrientationChanged;
        ViewportManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        ViewportManager.OnContextChanged -= OnContextChanged;
    }
    
    private void OnOrientationChanged(ScreenOrientation orientation)
    {
        if (orientation == ScreenOrientation.Portrait)
        {
            ShowPortraitLayout();
        }
        else
        {
            ShowLandscapeLayout();
        }
    }
    
    private void OnScreenSizeChanged(int width, int height)
    {
        Debug.Log($"Screen size changed to {width}x{height}");
        UpdateUILayout(width, height);
    }
    
    private void OnContextChanged(ViewportContext context)
    {
        Debug.Log($"Viewport changed: {context}");
    }
}
```

**Using ViewportEventListener Component:**
```csharp
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveUI : ViewportEventListener
{
    [Header("UI Elements")]
    public GameObject portraitLayout;
    public GameObject landscapeLayout;
    
    protected override void OnOrientationChanged(ScreenOrientation orientation)
    {
        base.OnOrientationChanged(orientation);
        
        portraitLayout.SetActive(orientation == ScreenOrientation.Portrait);
        landscapeLayout.SetActive(orientation == ScreenOrientation.Landscape);
    }
    
    protected override void OnViewportContextChanged(ViewportContext context)
    {
        base.OnViewportContextChanged(context);
        
        // Adjust UI based on device type
        if (context.deviceType == DeviceType.Mobile)
        {
            SetMobileUI();
        }
        else
        {
            SetDesktopUI();
        }
    }
}
```

### 3. Manual Detection

```csharp
// Get current viewport information
var context = ViewportManager.CurrentContext;
Debug.Log($"Current: {context.deviceType}, {context.platform}, {context.orientation}");

// Check specific properties
if (ViewportManager.IsPortrait())
{
    Debug.Log("Device is in portrait mode");
}

if (ViewportManager.IsMobile())
{
    Debug.Log("Running on mobile device");
}

// Get screen information
var (width, height) = ViewportManager.GetScreenSize();
float aspectRatio = ViewportManager.GetAspectRatio();
```

## API Reference

### ViewportManager

**Properties:**
- `CurrentContext` - Current viewport context
- `IsInitialized` - Whether the system is initialized
- `LastDetectionTime` - Time of last detection

**Events:**
- `OnContextChanged` - Fired when viewport context changes
- `OnOrientationChanged` - Fired when screen orientation changes
- `OnScreenSizeChanged` - Fired when screen size changes
- `OnInitialized` - Fired when system is initialized

**Methods:**
- `Initialize()` - Initialize the viewport manager
- `RefreshContext()` - Force refresh of viewport context
- `GetOrientation()` - Get current screen orientation
- `GetDeviceType()` - Get current device type
- `GetPlatform()` - Get current platform
- `GetScreenSize()` - Get current screen size
- `GetAspectRatio()` - Get current aspect ratio
- `IsPortrait()` - Check if in portrait mode
- `IsLandscape()` - Check if in landscape mode
- `IsMobile()` - Check if on mobile device
- `IsDesktop()` - Check if on desktop device
- `IsTouchDevice()` - Check if device supports touch

### ViewportContext

```csharp
public struct ViewportContext
{
    public ScreenOrientation orientation;
    public DeviceType deviceType;
    public Platform platform;
    public int screenWidth;
    public int screenHeight;
    public float devicePixelRatio;
}
```

### Enums

**ScreenOrientation:**
- `Landscape`
- `Portrait`

**DeviceType:**
- `Desktop`
- `Mobile`

**Platform:**
- `WebGL`
- `Android`
- `iOS`
- `Windows`
- `macOS`
- `Linux`
- `Mobile`
- `Desktop`
- `Console`

## Platform-Specific Considerations

### WebGL
- Uses JavaScript plugin for enhanced device detection
- Automatically detects mobile browsers vs desktop
- Supports orientation change detection

### Mobile (iOS/Android)
- Optimized for battery life
- Portrait/Landscape orientation handling
- Touch device detection

### Desktop
- Multi-monitor awareness
- High refresh rate support
- Keyboard/mouse input detection

## Components

### ViewportManagerBootstrap
Automatically initializes and monitors the viewport manager.

### ViewportEventListener
Base class for components that need to react to viewport changes.

### BasicViewportEventListener
Simple event listener with Unity Events for visual scripting.

## Best Practices

1. **Always unsubscribe from events** in `OnDestroy()` to prevent memory leaks
2. **Use ViewportEventListener** as base class for responsive components
3. **Enable continuous detection** for dynamic viewport changes
4. **Test on multiple devices** and orientations
5. **Consider performance** when using continuous detection

## Troubleshooting

**Events not firing:**
- Ensure ViewportManagerBootstrap is in the scene
- Check that continuous detection is enabled
- Verify event subscriptions are correct

**Incorrect device detection:**
- Test on actual devices, not just editor
- Check WebGL plugin is working in browser
- Verify screen size thresholds for mobile detection

**Performance issues:**
- Reduce detection interval in ViewportManagerBootstrap
- Disable continuous detection if not needed
- Optimize event handlers