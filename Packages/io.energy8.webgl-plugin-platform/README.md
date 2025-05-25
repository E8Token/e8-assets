# WebGL Plugin Platform

Platform for developing WebGL plugins that improve communication between Unity and browser.

## Package Structure

```
io.energy8.webgl-plugin-platform/
├── Runtime/                              // Core runtime components
│   ├── Core/                            // Main classes
│   │   ├── PluginManager.cs             // Central manager singleton
│   │   ├── BasePlugin.cs                // Abstract base plugin class
│   │   └── IPluginSettings.cs           // Settings interface
│   ├── Attributes/                      // Attributes
│   │   └── JSCallableAttribute.cs       // Method marking attribute
│   └── JavaScript/                      // JavaScript bridge
│       ├── WebGLPluginPlatform.jslib    // Library functions
│       └── WebGLPluginPlatform.jspre    // Namespace creation
├── Editor/                              // Editor integration
│   ├── Settings/                        // Settings management
│   │   └── WebGLPluginPlatformSettings.cs
│   └── GUI/                            // Project Settings UI
│       └── WebGLPluginPlatformSettingsProvider.cs
└── Samples~/                           // Sample implementations
    └── SamplePlugin/                    // Complete plugin example
        ├── Scripts/                     // C# classes
        ├── JavaScript/                  // JS bridge files
        └── Documentation/               // Usage examples
```

## Features

- **PluginManager** - Central manager for plugin control
- **Base Classes** - `BasePlugin`, `IPluginSettings`, `JSCallableAttribute`
- **Project Settings GUI** - Plugin management through Unity Project Settings
- **JSON Communication** - Standardized communication protocol
- **Automatic Scanning** - Automatic plugin discovery and registration
- **Load Priorities** - Control initialization order (0-100)
- **Async Calls** - Callback support for asynchronous operations

## Quick Start

1. Add package to your Unity project
2. Create plugin class inheriting from `BasePlugin`
3. Mark methods with `[JSCallable]` attribute
4. Configure plugin in Project Settings > WebGL Plugin Manager

## Plugin Example

```csharp
public class MyPlugin : BasePlugin
{
    public override IPluginSettings Settings => mySettings;
    
    [JSCallable]
    public string GetData(string key)
    {
        return PlayerPrefs.GetString(key);
    }
    
    public override void Initialize() { /* Initialization */ }
    public override void Enable() { /* Enable */ }
    public override void Disable() { /* Disable */ }
    public override void Destroy() { /* Destroy */ }
}
```

## JavaScript API

The platform creates global namespaces for easy plugin access:

```javascript
// Global WebGLPluginPlatform namespace (always available)
var result = WebGLPluginPlatform.call('MyPlugin', 'GetData', {key: 'test'});

// Individual plugin namespaces (created by .jspre files)
var message = SamplePlugin.getMessage('World');
SamplePlugin.getAsyncMessage('Async World', function(response) {
    console.log('Async result:', response);
});

// Direct library calls (from .jslib files)
var info = SamplePlugin.getPluginInfo();
```

## Namespace System

Each plugin can create its own JavaScript namespace:

1. **Core Platform**: `WebGLPluginPlatform` - unified API for all plugins
2. **Plugin Namespaces**: `YourPlugin` - convenient wrapper functions
3. **Library Functions**: `YourPluginLib` - raw Unity/JS bridge functions

Example namespace structure:
```javascript
// Created by WebGLPluginPlatform.jspre
WebGLPluginPlatform.call('PluginName', 'MethodName', data);

// Created by YourPlugin.jspre  
var YourPlugin = YourPlugin || {};

// Created by YourPlugin.jslib
YourPlugin.easyMethod = function(param) {
    return WebGLPluginPlatform.call('YourPlugin', 'Method', {param: param});
};
```

## Plugin Structure

```
MyPlugin/
├── Scripts/
│   ├── MyPlugin.cs                 // Main plugin class
│   └── MyPluginSettings.cs         // Plugin settings
├── JavaScript/
│   ├── MyPlugin.jslib             // JavaScript library
│   └── MyPlugin.jspre             // Pre-file for namespaces
└── Documentation/
    └── examples.html              // Usage examples
```

## Plugin Management

Open **Edit > Project Settings > WebGL Plugin Manager** for:
- View all plugins
- Enable/disable plugins
- Configure priorities
- Edit plugin settings

## Testing and Validation

The package includes a comprehensive test suite with:

### Test Categories
- **Runtime Tests**: Core functionality, plugin lifecycle, JSCallable attributes
- **Editor Tests**: Settings management, GUI integration, Project Settings
- **WebGL Tests**: JavaScript interop, build compilation, integration testing

### Running Tests
Use Unity Editor menu: **Energy8 > WebGL Plugin Platform**
- **Run All Tests**: Execute complete test suite
- **Run Runtime Tests**: Test core plugin functionality
- **Run Editor Tests**: Test editor integration
- **Run WebGL Tests**: Test WebGL-specific features
- **Generate Test Report**: Create detailed test analysis
- **Validate Package**: Check package integrity

### Test Features
- **Automated Build Testing**: WebGL compilation validation
- **JavaScript Integration**: Unity-JS communication testing
- **Error Handling**: Comprehensive failure scenario testing
- **Performance Validation**: Plugin initialization benchmarks
- **Package Integrity**: File structure and dependency validation

See `Tests/README.md` for detailed test documentation.
