# Energy8 Firebase Analytics

Cross-platform Firebase Analytics API for Unity that provides a unified interface for both native platforms and WebGL.

## Features

- **Cross-platform**: Supports both native platforms (iOS, Android, Desktop) and WebGL
- **Unified API**: Single API that works across all platforms
- **Configuration-based**: Easy configuration through Unity Project Settings
- **Event validation**: Built-in validation for event names and parameters
- **Debug logging**: Optional debug logging for development
- **Predefined constants**: Includes Firebase Analytics event and parameter name constants

## Architecture

The package follows a modular architecture similar to Firebase Core:

- **Core**: Main API and configuration
- **Editor**: Unity Editor integration and settings
- **Native**: Native platform implementation
- **WebGL**: WebGL platform implementation with JavaScript interop

## Installation

1. Add the package to your Unity project
2. Ensure Firebase Core is properly configured
3. Configure Firebase Analytics through Project Settings > Firebase > Analytics

## Usage

### Basic Event Logging

```csharp
using Energy8.Firebase.Analytics;
using Energy8.Firebase.Analytics.Models;

// Log simple event
await FirebaseAnalytics.LogEventAsync(AnalyticsEventNames.APP_OPEN);

// Log event with parameters
var parameters = new Dictionary<string, object>
{
    { AnalyticsParameterNames.LEVEL, 5 },
    { AnalyticsParameterNames.CHARACTER, "warrior" }
};
await FirebaseAnalytics.LogEventAsync(AnalyticsEventNames.LEVEL_UP, parameters);
```

### User Properties

```csharp
// Set user ID
await FirebaseAnalytics.SetUserIdAsync("user123");

// Set user property
await FirebaseAnalytics.SetUserPropertyAsync("player_type", "premium");
```

### Configuration

```csharp
// Enable/disable analytics collection
await FirebaseAnalytics.SetAnalyticsCollectionEnabledAsync(true);

// Set session timeout (30 minutes)
await FirebaseAnalytics.SetSessionTimeoutDurationAsync(1800000);
```

## Configuration

Configure Firebase Analytics through Unity's Project Settings:

1. Go to **Edit > Project Settings**
2. Navigate to **Firebase > Analytics**
3. Configure your analytics settings:
   - Enable/disable analytics collection
   - Set debug logging
   - Configure session timeout
   - Set automatic screen reporting

## Platform-Specific Implementations

### Native Platforms
Uses Firebase Unity SDK for iOS, Android, and desktop platforms.

### WebGL
Uses Google Analytics 4 (gtag) for web analytics integration.

## Dependencies

- Energy8 Firebase Core
- Energy8 WebGL Plugin Platform (for WebGL support)

## Events

The package provides event callbacks for monitoring analytics operations:

```csharp
FirebaseAnalytics.OnEventLogged += (eventName) => 
{
    Debug.Log($"Event logged: {eventName}");
};

FirebaseAnalytics.OnEventLogError += (eventName, error) => 
{
    Debug.LogError($"Failed to log event {eventName}: {error.Message}");
};
```
