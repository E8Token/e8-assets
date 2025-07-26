# ViewportManager Documentation

Welcome to the ViewportManager package documentation! This Unity package provides automatic viewport detection and responsive UI capabilities for cross-platform Unity applications.

## 📚 Documentation Overview

This documentation is organized into several guides to help you get started quickly and master the advanced features:

### 🚀 [Quick Start Guide](QUICK_START.md)
**Start here!** Get ViewportManager working in your project in 5 minutes.
- Basic setup and installation
- Simple orientation detection
- Common use cases with code examples
- Testing and debugging tips

### 📖 [Complete Usage Guide](USAGE_GUIDE.md)
**Comprehensive guide** covering all features and advanced usage.
- Detailed component explanations
- Configuration matrices and quality settings
- Event system deep dive
- Platform-specific considerations
- Performance optimization

### 🔧 [API Reference](API_REFERENCE.md)
**Technical reference** for all classes, methods, and properties.
- Complete API documentation
- Method signatures and parameters
- Event definitions
- Enum values and structures

## 🎯 What is ViewportManager?

ViewportManager is a Unity package that automatically detects and responds to viewport changes across different platforms and devices. It provides:

- **Automatic Detection**: Screen orientation, device type, and platform detection
- **Event System**: Clean event-driven architecture for responsive UI
- **Cross-Platform**: Works on Mobile, Desktop, and WebGL with platform-specific optimizations
- **Performance Optimized**: Efficient detection with configurable intervals
- **Quality Integration**: Automatic Unity Quality Settings management based on device capabilities

## 🏃‍♂️ Quick Example

```csharp
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveUI : ViewportEventListener
{
    protected override void OnOrientationChanged(ScreenOrientation previous, ScreenOrientation current)
    {
        if (current == ScreenOrientation.Portrait)
        {
            // Switch to portrait layout
            ShowPortraitUI();
        }
        else
        {
            // Switch to landscape layout
            ShowLandscapeUI();
        }
    }
}
```

## 🎮 Key Features

### 🔄 Automatic Viewport Detection
- Screen orientation changes (Portrait/Landscape)
- Device type detection (Mobile/Desktop)
- Platform identification (iOS, Android, WebGL, Windows, etc.)
- Real-time monitoring with configurable intervals

### 📱 Responsive UI Support
- Event-driven architecture for clean code
- Base classes for easy implementation
- Automatic event subscription management
- Support for multiple listeners

### 🌐 Cross-Platform Compatibility
- **Mobile**: Native orientation detection
- **Desktop**: Window resize and aspect ratio detection
- **WebGL**: Enhanced JavaScript-based detection with browser APIs
- **Editor**: Simulation support for testing

### ⚙️ Quality Settings Integration
- Automatic quality level adjustment based on device capabilities
- Configurable quality matrices
- Custom frame rate and VSync settings
- Performance optimization for different device types

### 🛠️ Developer-Friendly
- Comprehensive debugging tools
- Runtime system information
- Test components for easy validation
- Extensive logging and monitoring

## 📋 System Requirements

- **Unity Version**: 2021.3 LTS or newer
- **Platforms**: iOS, Android, WebGL, Windows, macOS, Linux
- **Dependencies**: None (self-contained package)

## 🚀 Getting Started

1. **[Start with Quick Start Guide](QUICK_START.md)** - Get up and running in 5 minutes
2. **Add Bootstrap Component** - Drop `ViewportManagerBootstrap` into your scene
3. **Create Event Listeners** - Inherit from `ViewportEventListener` or subscribe to events
4. **Test and Debug** - Use built-in testing components and debug tools

## 📖 Documentation Structure

```
Documentation/
├── README.md           # This overview (start here)
├── QUICK_START.md      # 5-minute setup guide
├── USAGE_GUIDE.md      # Complete feature guide
└── API_REFERENCE.md    # Technical API reference
```

## 🎯 Common Use Cases

### 📱 Mobile App Orientation
```csharp
// Automatically switch UI layouts when device rotates
protected override void OnOrientationChanged(ScreenOrientation previous, ScreenOrientation current)
{
    portraitPanel.SetActive(current == ScreenOrientation.Portrait);
    landscapePanel.SetActive(current != ScreenOrientation.Portrait);
}
```

### 🖥️ Responsive Web Game
```csharp
// Adapt UI for different screen sizes in WebGL
protected override void OnDeviceTypeChanged(DeviceType previous, DeviceType current)
{
    mobileControls.SetActive(current == DeviceType.Mobile);
    desktopControls.SetActive(current == DeviceType.Desktop);
}
```

### ⚙️ Quality Optimization
```csharp
// Automatically adjust quality based on device capabilities
protected override void OnQualityChanged(int previousQuality, int newQuality)
{
    Debug.Log($"Quality changed from {previousQuality} to {newQuality}");
    // Custom quality-specific adjustments
}
```

## 🔧 Advanced Features

- **Configuration Matrices**: Define custom quality settings for different device/platform combinations
- **Event Filtering**: Subscribe to specific types of viewport changes
- **Performance Monitoring**: Built-in performance tracking and optimization
- **Custom Detection**: Extend the system with custom viewport detection logic
- **Persistence**: Maintain viewport state across scene transitions

## 🐛 Troubleshooting

### Common Issues

**Events not firing?**
- Ensure `ViewportManagerBootstrap` is in your scene
- Check that "Enable Continuous Detection" is enabled
- Verify event subscription timing

**Wrong orientation detected?**
- Test on actual devices, not Unity Editor
- Check WebGL browser console for JavaScript errors
- Verify platform-specific settings

**Performance issues?**
- Increase detection interval
- Disable continuous detection if not needed
- Optimize event listener count

See the [Usage Guide](USAGE_GUIDE.md) for detailed troubleshooting steps.

## 📞 Support

For questions, issues, or feature requests:

1. Check the documentation guides above
2. Review the API reference for technical details
3. Use the built-in debugging tools
4. Test with the provided example components

## 🎉 Ready to Start?

**👉 [Begin with the Quick Start Guide](QUICK_START.md)** to get ViewportManager working in your project in just 5 minutes!

Or jump directly to:
- **[Usage Guide](USAGE_GUIDE.md)** for comprehensive feature coverage
- **[API Reference](API_REFERENCE.md)** for technical documentation

---

**Happy developing! 🚀**

*ViewportManager - Making Unity apps responsive across all platforms*