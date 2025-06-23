# Energy8 WebGL Templates

This directory contains three specialized WebGL templates for different deployment scenarios:

## Template Types

### 1. Standard Template (`Data-Templates/Standard/`)
**Best for: Desktop and tablet users**

**Features:**
- Enhanced loading screen with detailed progress
- Smooth progress bars with animation effects
- Performance monitor (can be toggled with Ctrl+P)
- Orientation warnings for mobile devices
- Full analytics integration
- All platform handlers enabled

**Use case:** Default template for most deployments targeting desktop/tablet users.

### 2. Lite Template (`Data-Templates/Lite/`)
**Best for: Mobile devices and low-bandwidth connections**

**Features:**
- Simplified loading screen with minimal animations
- Optimized for portrait orientation
- Touch interaction hints
- Reduced script footprint
- Minimal analytics
- Optimized for performance on mobile devices

**Use case:** Mobile-optimized builds, especially for slower devices or limited bandwidth.

### 3. Debug Template (`Data-Templates/Debug/`)
**Best for: Development and testing**

**Features:**
- Debug header with build information
- Advanced performance monitoring
- Interactive debug console
- Debug panel with Unity controls
- Enhanced loading information with technical details
- All debugging tools enabled (Eruda, console commands)
- Development-specific analytics

**Use case:** Development builds, testing, and debugging scenarios.

## File Structure

```
Data-Templates/
├── Standard/
│   ├── index.html
│   └── TemplateData/
│       └── standard-theme.css
├── Lite/
│   ├── index.html
│   └── TemplateData/
│       └── lite-theme.css
└── Debug/
    ├── index.html
    └── TemplateData/
        └── debug-theme.css
```

## Shared Resources

These JavaScript files are shared between templates and provide enhanced functionality:

- **`progressManager.js`** - Enhanced progress tracking with speed calculations and ETA
- **`performanceMonitor.js`** - Real-time performance monitoring (FPS, memory, etc.)
- **`touchHelper.js`** - Touch interaction hints for mobile devices
- **`orientationHandler.js`** - Improved orientation detection and warnings

## Usage in Unity Build System

To use these templates in your Energy8 Build Deploy System:

1. **Configure Build Settings:**
   ```csharp
   // In your BuildConfiguration
   config.WebGLTemplate = "Standard"; // or "Lite" or "Debug"
   ```

2. **Template Selection Logic:**
   ```csharp
   public string GetWebGLTemplate(BuildTarget target, bool isDebug, bool isMobile)
   {
       if (isDebug) return "Debug";
       if (isMobile) return "Lite";
       return "Standard";
   }
   ```

## Template Features Comparison

| Feature | Standard | Lite | Debug |
|---------|----------|------|-------|
| Enhanced Progress Bar | ✅ | ❌ | ✅ |
| Performance Monitor | ✅ | ❌ | ✅ |
| Touch Hints | ❌ | ✅ | ❌ |
| Debug Console | ❌ | ❌ | ✅ |
| Orientation Warnings | ✅ | ✅* | ✅ |
| Analytics | Full | Minimal | Enhanced |
| Mobile Optimized | ❌ | ✅ | ❌ |
| File Size | Medium | Small | Large |

*Lite version shows hints instead of full warnings

## JavaScript API

All templates expose these functions for Unity integration:

### Progress Management
```javascript
UpdateLoadingProgress(progress, stage, currentFile)
SetLoadingStage(stage, title)
ShowProgressBar()
HideProgressBar()
```

### Performance Monitoring
```javascript
ShowPerformanceMonitor()
HidePerformanceMonitor()
TogglePerformanceMonitor()
UpdateUnityPerformance(drawCalls, triangles, gpuMemory, fps)
GetPerformanceMetrics()
```

### Touch Helpers (Lite version)
```javascript
ShowTouchHint(message, duration)
HideTouchHint()
UpdateTouchHint(message)
SetTouchHintPosition(position)
```

### Orientation Handling
```javascript
ShowOrientationWarning()
HideOrientationWarning()
GetCurrentOrientation()
IsCorrectOrientation()
```

## Customization

### Adding Custom Styles
Each template has its own CSS theme file that can be customized:
- `standard-theme.css` - Desktop/tablet optimizations
- `lite-theme.css` - Mobile optimizations
- `debug-theme.css` - Development tools styling

### Template Variables
All templates support Unity's template variables:
- `{{{ PRODUCT_NAME }}}` - Product name
- `{{{ COMPANY_NAME }}}` - Company name
- `{{{ VERSION }}}` - Build version
- `{{{ BUILD_TARGET }}}` - Build target platform

## Performance Considerations

### Standard Template
- Moderate resource usage
- Smooth animations may impact low-end devices
- Full feature set

### Lite Template
- Minimal resource usage
- Reduced animations for better performance
- Optimized for mobile networks

### Debug Template
- Higher resource usage due to monitoring tools
- Should not be used in production
- Includes all development tools

## Browser Compatibility

All templates are tested and compatible with:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile browsers on iOS 14+ and Android 8+

## Best Practices

1. **Use Standard template** for desktop/tablet deployment
2. **Use Lite template** for mobile deployment or bandwidth-constrained environments
3. **Use Debug template** only during development
4. **Test on target devices** before production deployment
5. **Monitor performance metrics** to choose the appropriate template

## Unity Integration Example

```csharp
// In your Unity C# code
public void UpdateLoadingProgress(float progress, string stage, string file)
{
    Application.ExternalEval($"UpdateLoadingProgress({progress}, '{stage}', '{file}')");
}

public void ShowPerformanceStats(int drawCalls, int triangles, long gpuMemory, float fps)
{
    Application.ExternalEval($"UpdateUnityPerformance({drawCalls}, {triangles}, {gpuMemory}, {fps})");
}
```
