# Energy8 JS Plugin Tools Documentation

## Overview

Energy8 JS Plugin Tools is a comprehensive framework for Unity WebGL applications that enables seamless communication between Unity and JavaScript. This framework provides a structured way to access browser features, manipulate the DOM, make network requests, gather device information, and utilize browser storage mechanisms directly from your Unity code.

## Modules

The framework consists of the following modules:

1. **[Core](./Core/Documentation.md)**: The foundation of the framework, providing plugin lifecycle management, memory handling, and message passing.

2. **[Communication](./Communication/Documentation.md)**: Enables structured, channel-based messaging between Unity and JavaScript.

3. **[DOM](./DOM/Documentation.md)**: Allows Unity to manipulate web page elements, add event listeners, and create UI components.

4. **[Device](./Device/Documentation.md)**: Provides information about the user's device, browser, operating system, and hardware capabilities.

5. **[Network](./Network/Documentation.md)**: Enables making HTTP requests from Unity to web servers and APIs.

6. **[Storage](./Storage/Documentation.md)**: Provides access to browser storage mechanisms like LocalStorage and IndexedDB.

## Getting Started

### Installation

1. Import the JSPluginTools package into your Unity project.
2. Ensure you have the required dependencies:
   - Newtonsoft.Json (com.unity.nuget.newtonsoft-json)
   - Unity WebRequest module (com.unity.modules.unitywebrequest)

### Basic Setup

Here's how to initialize the framework and access its modules:

```csharp
using Energy8.JSPluginTools;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.DOM;
using Energy8.JSPluginTools.Device;
using Energy8.JSPluginTools.Network;
using Energy8.JSPluginTools.Storage;
using UnityEngine;

public class JSPluginToolsInitializer : MonoBehaviour
{
    private IPluginCore _pluginCore;
    private ICommunicationManager _communicationManager;
    private IDOMManager _domManager;
    private INetworkManager _networkManager;
    private IStorageManager _storageManager;
    private IDeviceInfo _deviceInfo;

    private void Awake()
    {
        // Initialize the plugin framework
        InitializeJSPluginTools();
    }

    private void InitializeJSPluginTools()
    {
        // Create and initialize the plugin core
        var memoryManager = new MemoryManager();
        var messageBus = new MessageBus(memoryManager);
        _pluginCore = new PluginCore(memoryManager, messageBus);
        _pluginCore.Initialize();

        // Create a MonoBehaviour wrapper
        var coreBehaviour = PluginCoreBehaviour.CreateInstance((PluginCore)_pluginCore);

        // Initialize the Communication module
        _communicationManager = new CommunicationManager();
        _communicationManager.Initialize(_pluginCore);

        // Initialize the DOM module
        _domManager = new DOMManager();
        _domManager.Initialize(_pluginCore);

        // Initialize the Network module
        _networkManager = new NetworkManager();
        _networkManager.Initialize(_pluginCore);

        // Initialize the Storage module
        _storageManager = new StorageManager();
        _storageManager.Initialize(_pluginCore);

        // Set up the Device info service
        var communicationService = new CommunicationService(((PluginCore)_pluginCore).MessageBus);
        _deviceInfo = new DeviceInfo(communicationService);

        Debug.Log("JS Plugin Tools initialized successfully");
    }

    // Example usage of different modules
    private async void UseModules()
    {
        // Check device information
        var browserInfo = await _deviceInfo.GetBrowserInfo();
        Debug.Log($"Browser: {browserInfo.Name} {browserInfo.Version}");

        // Create a DOM element
        string buttonId = await _domManager.CreateElement("button", "testButton", "btn", "margin: 10px;");
        await _domManager.SetContent(buttonId, "Click Me!");
        await _domManager.AddEventListener(buttonId, "click", OnButtonClick);

        // Make a network request
        var response = await _networkManager.Request(new NetworkRequest {
            Url = "https://api.example.com/data",
            Method = "GET"
        });

        // Store data in browser storage
        await _storageManager.SetString("lastVisit", System.DateTime.Now.ToString(), StorageType.Local);
    }

    private void OnButtonClick(DOMEventData eventData)
    {
        Debug.Log("Button was clicked!");
    }
}
```

### Using the JSPluginToolsManager

For simplified initialization, you can use the provided `JSPluginToolsManager`:

```csharp
using Energy8.JSPluginTools;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // The manager automatically initializes all modules
        var manager = JSPluginToolsManager.Instance;
        
        // Access modules through the manager
        var domManager = manager.GetModule<IDOMManager>();
        var networkManager = manager.GetModule<INetworkManager>();
        var storageManager = manager.GetModule<IStorageManager>();
        var deviceInfo = manager.GetModule<IDeviceInfo>();
        
        // Now you can use these modules
    }
}
```

## Module Interaction

The framework is designed for modules to work together seamlessly:

1. The **Core** module initializes the JavaScript environment and provides the messaging infrastructure.

2. The **Communication** module uses the Core's messaging system to enable channel-based communication.

3. Other modules like **DOM**, **Storage**, and **Network** use the Communication module to interact with JavaScript.

4. The **Device** module provides information that can be used to adapt your application's behavior based on the user's environment.

## Examples

### Creating UI Elements with the DOM Module

```csharp
// Get the DOM manager
var domManager = JSPluginToolsManager.Instance.GetModule<IDOMManager>();

// Create a form for user input
async void CreateLoginForm()
{
    // Create a container div
    string formId = await domManager.CreateElement("div", "loginForm", "login-form", "padding: 20px; border: 1px solid #ccc;");
    
    // Create username input
    string usernameId = await domManager.CreateElement("input", "usernameInput", "form-control", null, formId);
    await domManager.SetAttribute(usernameId, "type", "text");
    await domManager.SetAttribute(usernameId, "placeholder", "Username");
    
    // Create password input
    string passwordId = await domManager.CreateElement("input", "passwordInput", "form-control", "margin-top: 10px;", formId);
    await domManager.SetAttribute(passwordId, "type", "password");
    await domManager.SetAttribute(passwordId, "placeholder", "Password");
    
    // Create login button
    string buttonId = await domManager.CreateElement("button", "loginButton", "btn btn-primary", "margin-top: 10px;", formId);
    await domManager.SetContent(buttonId, "Login");
    
    // Add event listener to the button
    await domManager.AddEventListener(buttonId, "click", async (e) => {
        // Get input values
        string username = await domManager.GetAttribute(usernameId, "value");
        string password = await domManager.GetAttribute(passwordId, "value");
        
        // Process login
        ProcessLogin(username, password);
    });
}
```

### Saving and Loading Game Data with Storage

```csharp
// Get the Storage manager
var storageManager = JSPluginToolsManager.Instance.GetModule<IStorageManager>();

// Save game data
async void SaveGame(GameData gameData)
{
    try {
        await storageManager.SetObject("currentGame", gameData, StorageType.Local);
        Debug.Log("Game saved successfully");
    } catch (Exception ex) {
        Debug.LogError($"Failed to save game: {ex.Message}");
    }
}

// Load game data
async Task<GameData> LoadGame()
{
    try {
        if (await storageManager.Exists("currentGame", StorageType.Local)) {
            return await storageManager.GetObject<GameData>("currentGame", StorageType.Local);
        }
    } catch (Exception ex) {
        Debug.LogError($"Failed to load game: {ex.Message}");
    }
    
    // Return a new game if loading fails
    return new GameData();
}
```

### Making API Requests with Network

```csharp
// Get the Network manager
var networkManager = JSPluginToolsManager.Instance.GetModule<INetworkManager>();

// Fetch game leaderboard
async Task<List<LeaderboardEntry>> GetLeaderboard()
{
    try {
        return await networkManager.RequestJson<List<LeaderboardEntry>>(new NetworkRequest {
            Url = "https://api.yourgame.com/leaderboard",
            Method = "GET",
            Headers = new Dictionary<string, string> {
                { "Authorization", $"Bearer {userToken}" }
            }
        });
    } catch (Exception ex) {
        Debug.LogError($"Failed to fetch leaderboard: {ex.Message}");
        return new List<LeaderboardEntry>();
    }
}

// Submit player score
async Task<bool> SubmitScore(int score, string username)
{
    try {
        var response = await networkManager.Request(new NetworkRequest {
            Url = "https://api.yourgame.com/scores",
            Method = "POST",
            JsonBody = new {
                player = username,
                score = score,
                timestamp = DateTime.UtcNow.ToString("o")
            }
        });
        
        return response.IsSuccess;
    } catch (Exception ex) {
        Debug.LogError($"Failed to submit score: {ex.Message}");
        return false;
    }
}
```

### Adapting to Different Devices

```csharp
// Get the Device info
var deviceInfo = JSPluginToolsManager.Instance.GetModule<IDeviceInfo>();

// Configure game based on device
async void ConfigureForDevice()
{
    // Check if it's a mobile device
    bool isMobile = await deviceInfo.IsMobileDevice();
    
    // Get screen information
    var screenInfo = await deviceInfo.GetScreenInfo();
    
    // Adjust UI scale based on device
    float uiScale = isMobile ? Mathf.Max(1.5f, screenInfo.PixelRatio) : 1.0f;
    UIManager.SetGlobalScale(uiScale);
    
    // Configure input method
    InputManager.UseTouchControls = isMobile;
    
    // Adjust quality settings based on device capabilities
    var hardwareInfo = await deviceInfo.GetHardwareInfo();
    if (hardwareInfo.LogicalCores <= 2) {
        QualitySettings.SetQualityLevel(0); // Low quality for weak devices
    } else if (hardwareInfo.LogicalCores >= 8) {
        QualitySettings.SetQualityLevel(5); // Highest quality for powerful devices
    } else {
        QualitySettings.SetQualityLevel(3); // Medium quality for average devices
    }
}
```

## Best Practices

1. **Initialization**: Initialize the Core module first, then other modules.

2. **Error Handling**: Always use try-catch blocks when calling module methods, as WebGL operations can fail for various reasons.

3. **Asynchronous Operations**: Use async/await for all operations that interact with JavaScript to avoid blocking the main thread.

4. **Resource Management**: Clean up DOM elements and event listeners when they are no longer needed.

5. **WebGL Context**: Remember that some features are only available in WebGL builds. Use `IPluginCore.IsWebGLContext` to check the context.

6. **Testing**: Test your WebGL builds in different browsers to ensure compatibility.

7. **Security**: Be aware of browser security restrictions, especially for network requests (CORS).

8. **Module Separation**: Keep your code organized by using each module for its intended purpose.

## Troubleshooting

### Common Issues

1. **JavaScript Console Errors**: Check the browser's console for detailed error messages.

2. **Network Requests Failing**: Ensure your API endpoints support CORS for WebGL builds.

3. **DOM Elements Not Appearing**: Check the DOM hierarchy and element visibility settings.

4. **Storage Operations Failing**: Browser storage might be disabled or full.

### Debugging

1. Enable debug mode in the JavaScript console:
```javascript
Energy8JSPluginTools.Core.toggleDebug();
```

2. Monitor channel communication:
```javascript
Energy8JSPluginTools.Core.getMessageHandlers();
```

3. Check registered GameObjects:
```javascript
Energy8JSPluginTools.Core.getRegisteredObjects();
```

## Further Resources

- For detailed documentation on each module, see the respective module documentation files.
- For API references, check the XML documentation in the code.
- For examples, look at the test files in each module's tests directory.

## Support

For issues, questions, or feature requests, please contact: support@energy8.io