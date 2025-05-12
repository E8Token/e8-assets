# Communication Module Documentation

## Overview

The Communication module provides a standardized way to send and receive messages between Unity and JavaScript in WebGL builds. It offers a channel-based communication system with support for typed objects, async/await patterns, and event subscription.

## Features

- **Channel-based Communication**: Organize messages by channel for better separation of concerns
- **Type-Safe Messaging**: Send and receive strongly-typed objects between Unity and JavaScript
- **Promise-like API**: Use async/await pattern for request-response communication
- **Event Subscription**: Subscribe to specific channels for event-based programming
- **Automatic Serialization**: Convert between C# objects and JavaScript objects seamlessly

## Classes and Components

### ICommunicationService

The main interface for sending and receiving messages:

```csharp
public interface ICommunicationService
{
    IMessageBus MessageBus { get; }
    void Send(string channel, string data);
    void Send<T>(string channel, T data);
    Task<TResponse> SendWithResponseAsync<TResponse>(string channel, string data, int timeout = 5000);
    Task<TResponse> SendWithResponseAsync<TRequest, TResponse>(string channel, TRequest data, int timeout = 5000);
    void Subscribe(string channel, Action<string> handler);
    void Subscribe<T>(string channel, Action<T> handler);
    void Unsubscribe(string channel);
}
```

### CommunicationService

The implementation of `ICommunicationService` that provides:
- Channel-based communication using the MessageBus from the Core module
- Serialization of C# objects to JSON for JavaScript
- Deserialization of JSON from JavaScript to C# objects

### ICommunicationManager

Higher-level interface for managing communication:

```csharp
public interface ICommunicationManager
{
    void Initialize(IPluginCore core);
    void Send<T>(string channel, T data);
    Task<TResponse> SendAsync<TRequest, TResponse>(string channel, TRequest data);
    void RegisterHandler<T>(string channel, Action<T> handler);
    void UnregisterHandler(string channel);
    bool IsInitialized { get; }
    event Action OnInitialized;
}
```

### CommunicationManager

The implementation of `ICommunicationManager` that provides:
- Initialization of the communication module with the plugin core
- Management of channel handlers
- Channel name prefix handling

### CommunicationServiceBehaviour

A MonoBehaviour wrapper for CommunicationService that allows finding it with `FindObjectOfType`.

## Usage

### Setting Up Communication

```csharp
// Get or create the plugin core
var coreBehaviour = PluginCoreBehaviour.CreateInstance();
var pluginCore = coreBehaviour.Core;

// Create communication service and manager
var communicationService = new CommunicationService(pluginCore.MessageBus);
var serviceBehaviour = CommunicationServiceBehaviour.CreateInstance(communicationService);

// Create and initialize the communication manager
var communicationManager = new CommunicationManager();
communicationManager.Initialize(pluginCore);
```

### Sending Messages

```csharp
// Send a simple string message
communicationManager.Send("myChannel", "Hello JavaScript!");

// Send a typed object
var data = new MyData { Id = 1, Name = "Test" };
communicationManager.Send("dataChannel", data);

// Send with response (async/await)
try {
    var response = await communicationManager.SendAsync<MyRequest, MyResponse>(
        "requestChannel", 
        new MyRequest { Query = "some query" }
    );
    Debug.Log($"Got response: {response.Result}");
}
catch (Exception ex) {
    Debug.LogError($"Error: {ex.Message}");
}
```

### Receiving Messages

```csharp
// Subscribe to a channel with a string handler
communicationManager.RegisterHandler<string>("notificationChannel", message => {
    Debug.Log($"Notification received: {message}");
});

// Subscribe to a channel with a typed object handler
communicationManager.RegisterHandler<UserData>("userChannel", userData => {
    Debug.Log($"User connected: {userData.Name}, ID: {userData.Id}");
});

// Unsubscribe from a channel
communicationManager.UnregisterHandler("userChannel");
```

### Using the Communication Service Directly

```csharp
// Get the communication service
var communicationService = serviceBehaviour.Service;

// Send a message directly
communicationService.Send("directChannel", "Direct message");

// Wait for a response
var response = await communicationService.SendWithResponseAsync<ResponseData>(
    "query", 
    "What's the weather?",
    timeout: 10000 // 10 seconds
);
```

## JavaScript API

In JavaScript, the Communication module provides these methods:

```javascript
// Send a message to Unity
Energy8JSPluginTools.Communication.send("myChannel", "Message content");

// Send an object to Unity
Energy8JSPluginTools.Communication.send("dataChannel", { id: 1, name: "Test" });

// Register a handler for a channel
Energy8JSPluginTools.Communication.registerChannelHandler("requestChannel", 
    function(data, sendResponse) {
        console.log("Request received:", data);
        // Process the request
        const result = processRequest(data);
        // Send back a response
        sendResponse({ success: true, result: result });
    }
);

// Get all registered channel handlers
const channels = Energy8JSPluginTools.Communication.getChannels();
```

## Best Practices

1. Organize your channels by feature or domain (e.g., "user.login", "game.score").
2. Use typed objects instead of raw strings for better type safety.
3. Set appropriate timeouts for async operations.
4. Always unsubscribe from channels when they are no longer needed.
5. Handle exceptions in async operations.
6. Use the communication manager for most cases, and only use the service directly for special cases.
7. Register handlers early in your application lifecycle.
8. Keep message payloads reasonably small to minimize serialization overhead.