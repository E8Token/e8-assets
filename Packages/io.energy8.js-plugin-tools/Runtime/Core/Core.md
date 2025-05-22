# Core Module

## Purpose

The Core module provides the fundamental architecture and APIs for the JavaScript plugin integration in Unity WebGL. It establishes a robust communication framework that allows bidirectional messaging between JavaScript and C# in a structured, type-safe manner.

## Integration Instructions

### Installation

1. Add the package to your Unity project via Package Manager.
2. Ensure your project is set to target WebGL platform.

### Basic Setup

```csharp
// This is just a reference - no actual code samples in documentation as per spec
```

### Registering a Plugin

To register a plugin, you need to:

1. Create a class that inherits from `JSPluginBase`
2. Implement the required abstract methods
3. Register the plugin with `JSPluginManager.Instance.RegisterPlugin(typeof(YourPlugin))`

### Communication (C# → JavaScript)

Use the extension method `SendMessageToJS` to communicate with JavaScript:

```csharp
// This is just a reference - no actual code samples in documentation as per spec
```

### Communication (JavaScript → C#)

JavaScript code can call C# methods by using the messaging system. Override the `OnMessage` method in your plugin class to handle incoming messages.

## Public API (JS ↔ C#)

### C# API

#### `JSPluginBase`

Abstract base class for all plugins.

- `string PluginName` (abstract property): Gets the unique plugin identifier.
- `void Initialize()` (abstract method): Initializes the plugin instance.
- `void OnMessage(string method, string payload)` (virtual method): Handles messages from JavaScript.

#### `JSPluginModule`

Abstract base class for all plugin modules.

- `string ModuleName` (abstract property): Gets the name of the module.
- `void InitializeModule()` (virtual method): Called once during module registration.

#### `JSPluginManager`

Core manager for JavaScript plugins.

- `static JSPluginManager Instance` (property): Gets the singleton instance.
- `bool DebugMode` (property): Enables or disables debug mode.
- `void RegisterModule(JSPluginModule module)` (method): Registers a plugin module.
- `JSPluginBase RegisterPlugin(Type pluginType)` (method): Registers a plugin.
- `JSPluginBase GetPlugin(string pluginName)` (method): Gets a registered plugin by name.
- `void DispatchMessage(string pluginName, string method, string payload)` (method): Dispatches a message from JavaScript.
- `void SendMessage(string pluginName, string method, string payload)` (method): Sends a message to JavaScript.

#### `JSPluginExtensions`

Extension methods for JavaScript plugin communication.

- `void SendMessageToJS(this JSPluginBase plugin, string method, string payload)` (extension method): Sends a message from C# to JavaScript.

### JavaScript API

The JavaScript API is not exposed globally and is accessed through the Unity `.jslib` system.

- `RegisterPluginWithJS(pluginName)`: Registers a plugin with JavaScript.
- `SendMessageToJS(pluginName, method, payload)`: Sends a message from C# to JavaScript.
- `SetDebugMode(enabled)`: Enables or disables debug mode.

In debug mode, a Symbol-based debug hook is available through:
`Object[Symbol.for('Energy8_JSPluginTools_Debug')]()`
