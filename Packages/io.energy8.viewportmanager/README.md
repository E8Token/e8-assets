# ViewportManager Package

Universal viewport management system for Unity that automatically detects device orientation and platform.

**⚠️ CURRENT STATUS: Graphics settings temporarily disabled - only orientation detection and events are active.**

## Features

- **Platform Detection**: Automatically detects device type, platform, and orientation
- **Event-Driven Architecture**: React to viewport changes through a clean event system
- **Cross-Platform Support**: Works on WebGL, mobile, and desktop platforms
- **Unity Quality Settings Integration**: Ready for Unity's built-in Quality Settings (temporarily disabled)
- **Minimal Configuration**: Simple platform + orientation detection

## Installation

1. Copy the `io.energy8.viewportmanager` package to your project's `Packages` folder
2. The package will be automatically imported by Unity

## Quick Start

### 1. Configure Unity Quality Settings

Set up your Unity Quality Settings first:

1. Go to **Edit > Project Settings > Quality**
2. Configure the quality levels you want to use (0-5):
   - **Level 0**: Mobile portrait (low power)
   - **Level 2**: Mobile landscape (balanced)
   - **Level 3**: Desktop WebGL (good quality)
   - **Level 4**: Desktop native (high quality)

### 2. Add ViewportManager to Your Scene

1. Create an empty GameObject named "ViewportManager"
2. Add the `ViewportManagerBootstrap` component
3. Configure the `Configuration Matrix` with your platform mappings

### 3. Configure Platform Mappings

The system maps Platform + Orientation → Unity Quality Level:

```csharp
// Example mappings in ViewportConfigurationMatrix
Mobile + Portrait + Android → Unity Quality Level 0
Mobile + Landscape + Android → Unity Quality Level 2
Desktop + Landscape + WebGL → Unity Quality Level 3
Desktop + Landscape + Windows → Unity Quality Level 4
```

### 4. Test Orientation Detection

For testing orientation changes without graphics settings:

```csharp
// Add ViewportOrientationTester component to a GameObject
// It will log orientation changes in real-time

// Manual testing:
var context = ViewportDetector.DetectContext();
Debug.Log($"Current: {context.deviceType}, {context.platform}, {context.orientation}");
```

### 5. Listen to Changes (when graphics settings are re-enabled)

**Простой пример:**
```csharp
using Energy8.ViewportManager.Core;

public class MyUIController : MonoBehaviour
{
    private void Start()
    {
        // Подписываемся на изменения ориентации
        ViewportManager.OnContextChanged += OnViewportChanged;
    }
    
    private void OnDestroy()
    {
        // Не забываем отписаться!
        ViewportManager.OnContextChanged -= OnViewportChanged;
    }
    
    private void OnViewportChanged(ViewportContext context)
    {
        if (context.orientation == ScreenOrientation.Portrait)
        {
            // Переключились в портретный режим
            ShowPortraitLayout();
        }
        else
        {
            // Переключились в ландшафтный режим  
            ShowLandscapeLayout();
        }
    }
}
```

**📖 Подробную документацию по событиям см. в [EVENTS_DOCUMENTATION.md](EVENTS_DOCUMENTATION.md)**

## Architecture Overview

### Core Components

- **ViewportManager**: Detects viewport changes and applies Unity Quality Settings
- **ViewportDetector**: Handles device and platform detection
- **ViewportConfiguration**: Simple wrapper for Unity Quality Level + optional overrides
- **ViewportConfigurationMatrix**: Maps platform combinations to quality levels

### Platform Mapping

The system maps these combinations to Unity Quality Levels:

| Device Type | Platform | Orientation | Default Quality Level |
|-------------|----------|-------------|----------------------|
| Mobile      | Android  | Portrait    | 0 (Low)              |
| Mobile      | Android  | Landscape   | 2 (Medium)           |
| Mobile      | iOS      | Portrait    | 0 (Low)              |
| Mobile      | iOS      | Landscape   | 2 (Medium)           |
| Desktop     | WebGL    | Landscape   | 3 (High)             |
| Desktop     | Windows  | Landscape   | 4 (Very High)        |

### Device Detection

The system automatically detects:
- **Device Type**: Mobile, Desktop, Console
- **Platform**: WebGL, iOS, Android, Windows, macOS, Linux
- **Orientation**: Portrait, Landscape
- **Screen Resolution**: Width and height

## Advanced Usage

### Custom Overrides

You can add optional overrides to any configuration:

```csharp
var config = new ViewportConfiguration(3) // Unity Quality Level 3
{
    customTargetFrameRate = 120,  // Override FPS
    forceDisableVSync = true      // Force disable VSync
};

config.ApplyToUnity();
```

### Responsive Components

Use `ViewportResponsiveComponent` to automatically adjust UI elements based on viewport changes:

```csharp
public class ResponsiveUI : ViewportResponsiveComponent
{
    protected override void OnViewportChanged(ViewportInfo info)
    {
        if (info.orientation == ScreenOrientation.Portrait)
        {
            // Adjust UI for portrait mode
        }
        else
        {
            // Adjust UI for landscape mode
        }
    }
}
```

### Debug Information

Get current quality settings info for debugging:

```csharp
var qualityInfo = ViewportConfiguration.GetCurrentUnityQualityInfo();
Debug.Log(qualityInfo);
// Output: "Unity Quality: Level 3 (High), Shadows: All, AntiAliasing: 4, VSync: 1, TargetFPS: 60"
```

## Platform-Specific Considerations

### WebGL
- Uses JavaScript plugin for enhanced device detection
- Automatically detects mobile browsers vs desktop
- Supports orientation change detection

### Mobile (iOS/Android)
- Optimized for battery life and thermal management
- Automatic quality adjustment based on device capabilities
- Portrait/Landscape orientation handling

### Desktop
- Full quality settings available
- High refresh rate support
- Multi-monitor awareness

## API Reference

### ViewportManager

```csharp
public class ViewportManager : MonoBehaviour
{
    // Events
    public event System.Action<ViewportInfo> OnViewportChanged;
    public event System.Action<ViewportConfiguration> OnConfigurationApplied;
    
    // Methods
    public ViewportInfo GetCurrentViewportInfo();
    public ViewportConfiguration GetCurrentConfiguration();
    public void RefreshViewport();
    public void ApplyConfiguration(ViewportConfiguration config);
}
```

### ViewportConfiguration

```csharp
public class ViewportConfiguration
{
    public int unityQualityLevel;        // Unity Quality Level (0-5)
    public int customTargetFrameRate;    // Optional FPS override
    public bool forceDisableVSync;       // Optional VSync override
    
    public void ApplyToUnity();
    public static string GetCurrentUnityQualityInfo();
}
```

### ViewportInfo

```csharp
public struct ViewportInfo
{
    public DeviceType deviceType;
    public Platform platform;
    public ScreenOrientation orientation;
    public int screenWidth;
    public int screenHeight;
}
```

## License

This package is part of the Energy8 ecosystem and follows the same licensing terms as the main project.

## Additional Documentation

- **[Events Documentation](EVENTS_DOCUMENTATION.md)** - Подробное руководство по работе с событиями ViewportManager
- **[API Reference](#api-reference)** - Справочник по API
