# Core Module Documentation

## Overview

The Core module is the foundation of the JS Plugin Tools framework. It provides essential functionality for managing communication between Unity and JavaScript in WebGL builds. This module handles memory management, message passing, and the overall lifecycle of the plugin system.

## Features

- **Initialization and Shutdown**: Manages the plugin lifecycle
- **Memory Management**: Handles memory allocation and deallocation for data exchange
- **Message Bus**: Provides a communication channel between Unity and JavaScript
- **GameObject Registration**: Manages Unity GameObject references for JavaScript interop
- **WebGL Detection**: Automatically detects if the application is running in WebGL context

## Classes and Components

### IPluginCore

The main interface that defines core functionality:

```csharp
public interface IPluginCore
{
    void Initialize();
    void Shutdown();
    bool IsWebGLContext { get; }
    bool IsInitialized { get; }
    event Action OnInitialized;
    event Action OnShutdown;
    string RegisterGameObject(string gameObjectName);
    void UnregisterGameObject(string objectId);
}
```

### PluginCore

The implementation of `IPluginCore` that provides:
- Memory management through `IMemoryManager`
- Message passing through `IMessageBus`
- Registration of Unity GameObjects for JavaScript communication

### PluginCoreBehaviour

A MonoBehaviour wrapper for PluginCore that allows finding it with `FindObjectOfType`.

### MemoryManager

Handles memory allocation and data copying between managed and unmanaged code.

### MessageBus

Handles message passing between Unity and JavaScript.

### BaseServiceBehaviour

Base class for all JSPluginTools service behaviour components.

### ServiceLocator

Utility class for finding and initializing JSPluginTools services.

## Usage

### Basic Setup

```csharp
// Create and initialize the plugin core
var memoryManager = new MemoryManager();
var messageBus = new MessageBus(memoryManager);
var pluginCore = new PluginCore(memoryManager, messageBus);
pluginCore.Initialize();

// Create a MonoBehaviour wrapper
var coreBehaviour = PluginCoreBehaviour.CreateInstance(pluginCore);
```

### Registering a GameObject for JavaScript Communication

```csharp
// Register a GameObject to enable JavaScript to send messages to it
string objectId = pluginCore.RegisterGameObject("MyGameObject");

// When you're done with the GameObject
pluginCore.UnregisterGameObject(objectId);
```

### Using the MessageBus

```csharp
// Get the MessageBus from the plugin core
var messageBus = ((PluginCore)pluginCore).MessageBus;

// Register a handler for receiving messages from JavaScript
messageBus.RegisterMessageHandler("myMessageType", (string payload) => {
    Debug.Log($"Received message: {payload}");
});

// Send a message to JavaScript
messageBus.SendMessage("myJSMessageType", "Hello from Unity!");

// Send a message and expect a response
messageBus.SendMessageWithResponse<string, string>(
    "myRequestType", 
    "Request data", 
    response => {
        Debug.Log($"Got response: {response}");
    }
);
```

### Using ServiceLocator to Find or Create Service Behaviours

```csharp
// Find or create a service behaviour and initialize it with a service
var myService = new MyService();
var behaviour = ServiceLocator.FindOrCreateServiceBehaviour<MyServiceBehaviour, MyService>(
    myService, 
    "MyServiceGameObject"
);
```

## JavaScript API

In the JavaScript console, the following Core API is available:

```javascript
// Get plugin version
Energy8JSPluginTools.Core.version;

// Toggle debug mode
Energy8JSPluginTools.Core.toggleDebug();

// Check if debug mode is on
Energy8JSPluginTools.Core.isDebugMode();

// Get all registered Unity GameObjects
Energy8JSPluginTools.Core.getRegisteredObjects();

// Get all registered message handlers
Energy8JSPluginTools.Core.getMessageHandlers();

// Send a message to a Unity GameObject
Energy8JSPluginTools.Core.sendMessageToUnity("GameObjectName", "MethodName", "Message");
```

## Best Practices

1. Always initialize the PluginCore before using any other modules.
2. Use `PluginCoreBehaviour.CreateInstance()` to create a singleton core instance.
3. Don't forget to unregister GameObjects when they are destroyed.
4. Use try-catch blocks when calling JavaScript functions to handle errors gracefully.
5. Use the built-in WebGL detection to provide fallback functionality for non-WebGL platforms.