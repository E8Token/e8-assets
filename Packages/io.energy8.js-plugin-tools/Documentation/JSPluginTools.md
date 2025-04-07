# JSPluginTools Documentation

## Overview

JSPluginTools is a modular framework designed to streamline communication between Unity and JavaScript in WebGL builds. It provides a comprehensive set of utilities for accessing browser APIs, DOM manipulation, storage, networking, and device capabilities directly from C# code.

## Architecture

JSPluginTools follows a modular architecture with each module having specific responsibilities:

```
JSPluginTools
├── Core Module (JSPluginCore)
├── Communication Module (JSPluginCommunication)
├── Storage Module (JSPluginStorage)
├── Events Module (JSPluginEvents)
├── DOM Module (JSPluginDOM)
├── Device Module (JSPluginDevice)
├── Network Module (JSPluginNetwork)
└── Error Handling Module (JSPluginErrorHandling)
```

Each module can be used independently or together through the centralized `JSPlugin` class.

## Getting Started

### Basic Initialization

```csharp
// Initialize all modules at once
JSPlugin.Initialize(debugMode: true);

// Or initialize modules individually
JSPluginCore.Initialize(true);
JSPluginStorage.Initialize();
JSPluginDevice.Initialize();
```

### Creating a Plugin Object

```csharp
// Create a GameObject to receive callbacks
GameObject jsHandler = new GameObject("JSHandler");

// Register it with the plugin system
JSPluginObject plugin = JSPlugin.CreatePlugin("myPlugin", jsHandler);

// Configure event forwarding
plugin.ForwardUnityEvents(EventForwardingOptions.CommonEvents);
```

## Module Documentation

### Core Module (JSPluginCore)

The core module provides the foundation for Unity-JavaScript communication.

```csharp
// Register an object for JS communication
JSPluginObject plugin = JSPluginCore.RegisterObject("myObject", gameObject);

// Send a message to JavaScript
plugin.SendMessage("jsMethod", "Hello from Unity!");

// Register a callback
plugin.RegisterCallback("dataReceived", "OnDataReceived");
```

### Communication Module (JSPluginCommunication)

Handles structured communication between Unity and JavaScript.

```csharp
// Create a communication channel
var channel = JSPlugin.CreateChannel("dataChannel", gameObject);

// Send a message through the channel
channel.Send("updateScore", "100");

// Send structured data as JSON
var playerData = new PlayerData { Id = 1, Name = "Player1", Score = 500 };
channel.SendJson("updatePlayer", playerData);

// Broadcast a message to all listeners
JSPlugin.Broadcast("gameEvent", "levelCompleted");
```

### Storage Module (JSPluginStorage)

Provides access to browser storage mechanisms.

```csharp
// Local storage (persists between sessions)
JSPluginStorage.LocalStorage.SetString("highScore", "1000");
string highScore = JSPluginStorage.LocalStorage.GetString("highScore");

// Session storage (cleared when browser is closed)
JSPluginStorage.SessionStorage.SetObject("playerState", playerData);
var savedState = JSPluginStorage.SessionStorage.GetObject<PlayerData>("playerState");

// Unified API
JSPluginStorage.Store.SetString("key", "value", JSPluginStorage.StorageType.Session);
```

### Events Module (JSPluginEvents)

Manages event forwarding and handling between Unity and JavaScript.

```csharp
// Forward Unity lifecycle events to JavaScript
plugin.ForwardUnityEvents(EventForwardingOptions.AllEvents);

// Register a custom event handler
var events = plugin.ConfigureEvents();
events.RegisterCustomEvent("onScoreChanged", (data) => {
    Debug.Log($"Score changed: {data}");
}, priority: 10);

// Trigger an event
events.TriggerEvent("customEvent", "eventData");
```

### DOM Module (JSPluginDOM)

Provides control over browser DOM elements.

```csharp
// Create a DOM element
var button = JSPluginDOM.CreateElement("button", "myButton", "btn-primary");
button.SetContent("Click Me");
button.SetStyle("color", "white");

// Get an existing element
var header = JSPluginDOM.GetElement("#header");

// Add an event listener
button.AddEventListener("click", "myPlugin", "OnButtonClicked");

// Create a toast notification
JSPlugin.ShowToast("Item Saved!", duration: 3f, type: "success");

// Create an overlay
JSPlugin.ShowOverlay("<h2>Welcome!</h2><p>Click anywhere to continue</p>");
```

### Device Module (JSPluginDevice)

Provides information about the user's device and browser.

```csharp
// Get browser information
var browserInfo = JSPluginDevice.GetBrowserInfo();
Debug.Log($"Browser: {browserInfo.Name} {browserInfo.Version}");

// Check device type
if (JSPlugin.IsMobileDevice()) {
    // Handle mobile-specific logic
}

// Control fullscreen mode
JSPlugin.RequestFullscreen();

// Use device vibration (mobile only)
JSPlugin.Vibrate(200); // 200ms vibration
```

### Network Module (JSPluginNetwork)

Enables HTTP requests and WebSocket communication.

```csharp
// Send HTTP request
var request = new JSPluginNetwork.RequestData {
    Url = "https://api.example.com/data",
    Method = "GET"
};

JSPluginNetwork.SendRequest(request, (response) => {
    if (response.IsSuccess) {
        Debug.Log($"Received: {response.Text}");
    }
});

// Create WebSocket connection
var socket = new JSPluginNetwork.WebSocket("wss://echo.websocket.org");

socket.OnOpen += () => Debug.Log("Connected!");
socket.OnMessage += (msg) => Debug.Log($"Received: {msg}");
socket.OnClose += (code, reason) => Debug.Log($"Closed: {reason}");

socket.Send("Hello WebSocket!");

// Binary data support
socket.OnBinaryMessage += (bytes) => {
    Debug.Log($"Received binary data: {bytes.Length} bytes");
    // Process binary data
};

// Send binary data
byte[] binaryData = new byte[] { 1, 2, 3, 4, 5 };
socket.Send(binaryData);
```

### Error Handling Module (JSPluginErrorHandling)

Provides standardized error handling across modules.

```csharp
// Report an error
JSPlugin.ReportError("MyComponent", "Failed to load resource", 
    JSPluginErrorHandling.ErrorSeverity.Warning);

// Handle an exception
try {
    // Some code that might throw
} catch (Exception ex) {
    JSPlugin.ReportException("MyComponent", ex);
}

// Add a global error handler
JSPluginErrorHandling.AddGlobalErrorHandler("myHandler", (errorInfo) => {
    Debug.LogError($"Error in {errorInfo.Source}: {errorInfo.Message}");
});
```

## Advanced Usage

### Mixing JavaScript and C#

You can write JavaScript functions that interact with your Unity game:

```javascript
// In a JavaScript file loaded in your WebGL page
window.UnityJSTools.onLog(function(logData) {
    console.log("Unity log:", logData.raw);
});

function sendScoreToUnity(score) {
    var channel = window.UnityJSTools.communication.createChannel("gameChannel");
    channel.send("updateScore", { score: score, timestamp: Date.now() });
}
```

### Creating a Custom Plugin

```csharp
// Define a MonoBehaviour to handle the plugin
public class MyJSPlugin : MonoBehaviour
{
    private JSPluginObject plugin;
    
    void Awake()
    {
        JSPlugin.Initialize();
        plugin = JSPlugin.CreatePlugin("myCustomPlugin", gameObject);
        plugin.ForwardUnityEvents();
        plugin.RegisterCallback("dataReceived", "OnDataReceived");
    }
    
    // Called by JavaScript
    public void OnDataReceived(string data)
    {
        Debug.Log($"Received data: {data}");
    }
    
    // Send data to JavaScript
    public void SendToJS(string data)
    {
        plugin.SendMessage("processData", data);
    }
}
```

## Troubleshooting

### WebGL Build Issues

- Ensure your JavaScript files are properly included in your WebGL template
- Check browser console for errors
- Verify JSPlugin initialization is called early in your application lifecycle

### JavaScript Communication Problems

- Confirm that method names match exactly between C# and JavaScript
- Make sure GameObject references are valid and not destroyed
- Check for cross-origin issues if loading content from different domains

### Performance Considerations

- **Memory Management**: The plugin now includes automatic memory management for JavaScript strings to prevent leaks. For very large applications, you may want to monitor memory usage with `JSPluginCore.GetMemoryStats()`.

- **WebSocket Binary Data**: When working with binary WebSocket data, the data is Base64 encoded/decoded when crossing the JavaScript/C# boundary. For very large binary messages, this can impact performance. Consider chunking large binary transfers.

- **Storage Limits**: Browser localStorage typically has a limit of 5-10MB depending on the browser. For larger data needs, consider using the browser's IndexedDB API (requires custom implementation).

- **Event Throttling**: For high-frequency events like orientation changes or window resizing, consider implementing throttling or debouncing in your event handlers.

```csharp
// Example of a simple throttle implementation
private float lastUpdateTime = 0f;
private const float throttleInterval = 0.1f; // 100ms

void Update() 
{
    float now = Time.time;
    if (now - lastUpdateTime < throttleInterval)
        return;
        
    // Perform update logic
    lastUpdateTime = now;
}
```

## Platform Compatibility

| Feature | WebGL | Editor | Standalone | Mobile |
|---------|-------|--------|------------|--------|
| Core    | ✓     | ✓*     | ✓*         | ✓*     |
| Storage | ✓     | ✓*     | ✓*         | ✓*     |
| DOM     | ✓     | -      | -          | -      |
| Device  | ✓     | -      | -          | -      |
| Network | ✓     | -      | -          | -      |

✓* = Limited functionality through fallback implementations

## API Reference

For complete API documentation, see the [API Reference](./API.md).

## License

This package is licensed under the MIT License. See LICENSE.md for details.
