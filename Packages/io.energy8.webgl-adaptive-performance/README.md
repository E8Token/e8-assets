# E8 WebGL Adaptive Performance

> **⚠️ STATUS: UNDER DEVELOPMENT**
>
> This package is currently in active development and is not yet used in production. APIs and features are subject to change.

Adaptive graphics performance system that automatically adjusts Unity quality settings based on viewport data and device capabilities for optimal WebGL performance.

## Features

- **Viewport Integration**: Seamlessly integrates with E8 ViewportManager to get device and platform information
- **Real-time Performance Monitoring**: Tracks FPS, frame time, memory usage, and other performance metrics
- **Automatic Quality Adjustment**: Dynamically adjusts graphics settings based on performance feedback
- **Configurable Strategies**: Choose between Conservative, Balanced, or Aggressive adjustment strategies
- **Cross-platform Support**: Optimized for WebGL with fallbacks for other platforms
- **Event-driven Architecture**: React to performance changes with Unity Events
- **Thermal Management**: Responds to device thermal state changes
- **Debug Tools**: Built-in debugging and performance analysis tools

## Installation

### Via Package Manager

1. Open Unity Package Manager
2. Click "+" and select "Add package from git URL"
3. Enter: `https://github.com/E8Token/e8-assets.git?path=Packages/io.energy8.webgl-adaptive-performance`

### Via manifest.json

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "io.energy8.webgl-adaptive-performance": "https://github.com/E8Token/e8-assets.git?path=Packages/io.energy8.webgl-adaptive-performance",
    "io.energy8.viewportmanager": "https://github.com/E8Token/e8-assets.git?path=Packages/io.energy8.viewportmanager"
  }
}
```

## Quick Start

### 1. Basic Setup

```csharp
using Energy8.WebGL.AdaptivePerformance.Core;
using Energy8.ViewportManager.Core;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Initialize ViewportManager first
        ViewportManager.Initialize();
        
        // Initialize Adaptive Performance
        AdaptivePerformanceManager.Initialize();
    }
    
    void Update()
    {
        // Update the adaptive performance system
        AdaptivePerformanceManager.Update();
    }
}
```

### 2. Using Bootstrap Component

Add the `AdaptivePerformanceBootstrap` component to a GameObject in your scene:

```csharp
// The bootstrap will automatically initialize and manage the system
// Configure settings in the inspector
```

### 3. Listening to Performance Events

```csharp
using Energy8.WebGL.AdaptivePerformance.Core;

public class PerformanceHandler : MonoBehaviour
{
    void Start()
    {
        // Subscribe to performance events
        AdaptivePerformanceManager.OnPerformanceLevelChanged += OnPerformanceLevelChanged;
        PerformanceMonitor.OnFPSChanged += OnFPSChanged;
    }
    
    void OnPerformanceLevelChanged(PerformanceLevel level)
    {
        Debug.Log($"Performance level changed to: {level}");
        
        switch (level)
        {
            case PerformanceLevel.VeryLow:
                // Disable expensive effects
                break;
            case PerformanceLevel.High:
                // Enable premium effects
                break;
        }
    }
    
    void OnFPSChanged(float fps)
    {
        if (fps < 30f)
        {
            Debug.LogWarning("Low FPS detected!");
        }
    }
}
```

### 4. Custom Performance Configuration

```csharp
// Create custom performance matrix
var matrix = ScriptableObject.CreateInstance<AdaptivePerformanceMatrix>();

// Add custom profiles
var mobileProfile = new PerformanceProfile(PerformanceLevel.Low, 1);
matrix.AddEntry(ScreenOrientation.Any, DeviceType.Mobile, Platform.WebGL, mobileProfile);

// Apply the matrix
AdaptivePerformanceManager.SetConfigurationMatrix(matrix);
```

## API Reference

### AdaptivePerformanceManager

Main static class for managing adaptive performance.

#### Properties

- `IsInitialized`: Whether the system is initialized
- `CurrentProfile`: Current performance profile being used
- `CurrentPerformanceLevel`: Current performance level
- `Strategy`: Current adjustment strategy
- `AutoAdjustmentEnabled`: Whether automatic adjustments are enabled

#### Methods

- `Initialize(AdaptivePerformanceMatrix matrix = null)`: Initialize the system
- `Update()`: Update the system (call every frame)
- `SetAdjustmentStrategy(AdjustmentStrategy strategy)`: Set adjustment strategy
- `SetAutoAdjustment(bool enabled)`: Enable/disable auto adjustment
- `RefreshConfiguration()`: Force refresh current configuration
- `GetDebugInfo()`: Get debug information string

#### Events

- `OnProfileChanged`: Fired when performance profile changes
- `OnPerformanceLevelChanged`: Fired when performance level changes
- `OnConfigurationApplied`: Fired when configuration is applied
- `OnInitialized`: Fired when system is initialized

### PerformanceMonitor

Static class for monitoring real-time performance metrics.

#### Properties

- `CurrentMetrics`: Current performance metrics
- `AverageFPS`: Average FPS over recent history
- `CurrentFPS`: Current FPS
- `IsPerformanceStable()`: Check if performance is stable

#### Events

- `OnMetricsUpdated`: Fired when metrics are updated
- `OnFPSChanged`: Fired when FPS changes significantly
- `OnThermalStateChanged`: Fired when thermal state changes

### Performance Enums

#### PerformanceLevel

- `VeryLow`: Minimal quality for very low-end devices
- `Low`: Low quality for low-end devices
- `Medium`: Balanced quality for mid-range devices
- `High`: High quality for high-end devices
- `VeryHigh`: Very high quality for premium devices
- `Ultra`: Maximum quality for top-tier devices

#### AdjustmentStrategy

- `Conservative`: Slow, safe adjustments
- `Balanced`: Moderate adjustments (default)
- `Aggressive`: Fast, responsive adjustments

#### ThermalState

- `Nominal`: Normal operating temperature
- `Fair`: Slightly elevated temperature
- `Serious`: High temperature, performance may be affected
- `Critical`: Very high temperature, immediate action needed

## Configuration

### AdaptivePerformanceMatrix

Create performance configuration matrices using the ScriptableObject:

1. Right-click in Project window
2. Select "Create > Energy8 > Adaptive Performance Matrix"
3. Configure entries for different device/platform combinations
4. Assign to AdaptivePerformanceBootstrap or set via code

### Performance Profiles

Each profile contains:

- Unity Quality Level
- Target Frame Rate
- VSync settings
- Shadow Quality
- Anti-aliasing
- Render Scale
- Post-processing
- Texture Quality
- Particle Budget
- LOD Bias

## Platform Considerations

### WebGL

- Optimized for browser performance
- Limited access to system metrics
- Focus on frame rate and memory usage
- Automatic quality scaling based on performance

### Mobile

- Thermal management support
- Battery level monitoring
- Conservative default settings
- Orientation-aware configurations

### Desktop

- Higher performance profiles available
- More aggressive quality settings
- Better performance monitoring

## Components

### AdaptivePerformanceBootstrap

Main component for initializing and managing the adaptive performance system.

**Features:**
- Automatic initialization
- Configuration matrix assignment
- Debug tools and UI
- Event handling

### BasicAdaptivePerformanceEventListener

Component that exposes adaptive performance events as Unity Events.

**Events:**
- Performance level changes
- FPS threshold events
- Thermal state changes
- Initialization events

### AdvancedAdaptivePerformanceEventListener

Advanced event listener with performance analysis capabilities.

**Features:**
- Performance history tracking
- Detailed logging
- Performance analysis
- Critical performance alerts

## Best Practices

1. **Initialize Early**: Initialize the system in your game's startup sequence
2. **Monitor Performance**: Use event listeners to react to performance changes
3. **Test on Target Devices**: Test your performance configurations on actual target devices
4. **Use Appropriate Strategy**: Choose the right adjustment strategy for your game type
5. **Custom Profiles**: Create custom performance profiles for your specific needs
6. **Debug Tools**: Use the built-in debug tools during development

## Troubleshooting

### Common Issues

**System not initializing:**
- Ensure ViewportManager is initialized first
- Check for error messages in console
- Verify dependencies are properly installed

**Performance not adjusting:**
- Check if auto-adjustment is enabled
- Verify adjustment strategy is appropriate
- Check cooldown period settings

**Poor performance detection:**
- Ensure Update() is called every frame
- Check if performance monitoring is working
- Verify target frame rate settings

### Debug Tools

- Press F2 (configurable) to show debug information
- Use `AdaptivePerformanceManager.GetDebugInfo()` for detailed status
- Enable debug logging in AdaptivePerformanceBootstrap
- Use AdvancedAdaptivePerformanceEventListener for detailed analysis

## Dependencies

- Unity 6000.0 or later
- E8 ViewportManager package

## License

Copyright © 2024 Energy8. All rights reserved.