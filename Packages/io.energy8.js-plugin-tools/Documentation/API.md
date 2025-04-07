# JSPluginTools API Reference

This document provides a detailed API reference for all modules in the JSPluginTools framework.

## JSPlugin

The main entry point that provides access to all modules.

### Methods

```csharp
// Initialize the framework
public static bool Initialize(bool debugMode = false)

// Create a plugin object
public static JSPluginObject CreatePlugin(string objectId, GameObject gameObject)

// Get a previously registered plugin object
public static JSPluginObject GetPlugin(string objectId)

// Storage shortcuts
public static void SetString(string key, string value)
public static string GetString(string key, string defaultValue = "")
public static void SetObject<T>(string key, T value)
public static T GetObject<T>(string key, T defaultValue = default)

// Device shortcuts
public static bool IsMobileDevice()
public static bool RequestFullscreen()
public static bool Vibrate(int milliseconds = 200)

// Communication shortcuts
public static JSPluginCommunication.JSChannel CreateChannel(string channelId, GameObject gameObject)
public static bool Broadcast(string eventName, string data = null)
public static bool BroadcastJson<T>(string eventName, T data)

// DOM shortcuts
public static JSPluginDOM.Element ShowToast(string message, float duration = 3f, string type = "info")
public static JSPluginDOM.Element ShowOverlay(string content, string id = null)

// Error reporting shortcuts
public static void ReportError(string source, string message, JSPluginErrorHandling.ErrorSeverity severity = JSPluginErrorHandling.ErrorSeverity.Error)
public static void ReportException(string source, Exception exception, JSPluginErrorHandling.ErrorSeverity severity = JSPluginErrorHandling.ErrorSeverity.Error)
```

## JSPluginCore

Core functionality for Unity-JavaScript communication.

### Methods

```csharp
// Initialize the core system
public static void Initialize(bool debugMode = false)

// Register a GameObject for JS communication
public static JSPluginObject RegisterObject(string objectId, GameObject gameObject)

// Get a previously registered object
public static JSPluginObject GetObject(string objectId)
```

### JSPluginObject Class

```csharp
// Properties
public string ObjectId { get; }
public GameObject GameObject { get; }

// Methods
public void RegisterCallback(string callbackType, string methodName)
public void RegisterErrorHandler(string methodName)
public bool SendMessage(string methodName, string parameter = null)
public bool SendMessageJson<T>(string methodName, T data)
public bool TriggerCallback(string callbackType, string parameter = null)
public bool ReportError(string context, string error)
public bool ReportException(string context, Exception exception)
public JSPluginObject ForwardUnityEvents(EventForwardingOptions options = EventForwardingOptions.CommonEvents)
public JSPluginObject RegisterEventHandler(string eventName, string jsFunction)
public bool TriggerEvent(string eventName, string parameter = null)
public JSPluginObject RegisterCommonEventHandlers(string handlerMethodName = "Handle")
```

## JSPluginStorage

Provides access to browser storage mechanisms.

### LocalStorage Class

```csharp
// Basic storage operations
public static void SetString(string key, string value)
public static string GetString(string key, string defaultValue = "")
public static void SetInt(string key, int value)
public static int GetInt(string key, int defaultValue = 0)
public static void SetFloat(string key, float value)
public static float GetFloat(string key, float defaultValue = 0f)
public static void SetBool(string key, bool value)
public static bool GetBool(string key, bool defaultValue = false)

// Object operations
public static void SetObject<T>(string key, T value)
public static T GetObject<T>(string key, T defaultValue = default)

// Management operations
public static bool HasKey(string key)
public static void DeleteKey(string key)
public static void DeleteAll()
public static void Save()
public static string[] GetKeys()
```

### SessionStorage Class

```csharp
// Same API as LocalStorage
// ...
```

### Store Class

```csharp
// Unified storage API that works with both storage types
public static void SetString(string key, string value, StorageType storageType = StorageType.Persistent)
public static string GetString(string key, string defaultValue = "", StorageType storageType = StorageType.Persistent)
// ...and other methods
```

## JSPluginEvents

Manages event forwarding and handling.

```csharp
// Initialize the event system
public void Initialize(JSPluginObject pluginObject, EventForwardingOptions options)

// Register a custom event handler
public JSPluginEvents RegisterCustomEvent(string eventName, System.Action<string> handler, int priority = 0)

// Unregister a custom event handler
public JSPluginEvents UnregisterCustomEvent(string eventName)

// Trigger an event
public void TriggerEvent(string eventName, string data = null)

// Set event priority
public JSPluginEvents SetEventPriority(string eventName, int priority)

// Get event priority
public int GetEventPriority(string eventName)
```

## JSPluginDOM

Provides DOM manipulation capabilities.

```csharp
// Create a DOM element
public static Element CreateElement(string tagName, string id = null, string className = null)

// Get an existing element
public static Element GetElement(string selector)
```

### Element Class

```csharp
// Properties
public string Id { get; }

// Methods
public Element SetProperty(string property, string value)
public string GetProperty(string property)
public Element SetStyle(string property, string value)
public Element SetStyles(Dictionary<string, string> styles)
public Element SetContent(string content)
public Element AppendChild(Element child)
public void Remove()
public Element AddEventListener(string eventType, string callbackObjectId, string callbackMethod)
public Element RemoveEventListener(string eventType)
public bool IsVisible()
public Element SetVisible(bool visible)
public Element Show()
public Element Hide()
```

## JSPluginDevice

Provides information about the device and browser.

```csharp
// Get information
public static BrowserInfo GetBrowserInfo()
public static OSInfo GetOSInfo()
public static string GetUserAgent()
public static ScreenInfo GetScreenInfo()

// Check device type
public static bool IsMobile()
public static bool IsTablet()
public static bool IsDesktop()

// Device capabilities
public static bool AddOrientationChangeListener(string objectId, string methodName)
public static bool RemoveOrientationChangeListener()
public static bool RequestFullscreen()
public static bool ExitFullscreen()
public static bool IsFullscreen()
public static bool Vibrate(int milliseconds)
```

## JSPluginNetwork

Enables HTTP requests and WebSocket communication.

```csharp
// HTTP requests
public static bool SendRequest(RequestData request, Action<ResponseData> callback = null)
public static bool CancelRequest(string requestId)

// WebSocket
public class WebSocket
{
    // Constructor
    public WebSocket(string url, string protocols = null, string objectId = "JSPluginNetwork")
    
    // Properties
    public string SocketId { get; }
    public string Url { get; }
    public WebSocketState State { get; }
    
    // Events
    public event Action<string> OnMessage;
    public event Action OnOpen;
    public event Action<int, string> OnClose;
    public event Action<string> OnError;
    
    // Methods
    public bool Send(string message)
    public bool Close(int code = 1000, string reason = "")
}
```

## JSPluginErrorHandling

Provides standardized error handling.

```csharp
// Configuration
public static ErrorHandlingConfig Config { get; }

// Error handling
public static void ProcessError(string source, string context, string message, ErrorSeverity severity = ErrorSeverity.Error, string stackTrace = null, string objectId = null)
public static void ProcessException(string source, string context, Exception exception, ErrorSeverity severity = ErrorSeverity.Error, string objectId = null)

// Error handler management
public static void AddGlobalErrorHandler(string handlerId, Action<ErrorInfo> handler)
public static void RemoveGlobalErrorHandler(string handlerId)
```

## JSPluginCommunication

Facilitates structured communication.

```csharp
// Broadcasting
public static bool Broadcast(string eventName, string data = null)
public static bool BroadcastJson<T>(string eventName, T data)
public static Action SubscribeToBroadcast(string eventName, Action<string> listener)

// Connection management
public static void SetConnectionStatus(bool connected)
public static void QueueMessage(string objectId, string methodName, string parameter, bool isBroadcast = false)

// Channel-based communication
public class JSChannel
{
    // Constructor
    public JSChannel(string channelId, GameObject gameObject)
    
    // Properties
    public string ChannelId { get; }
    public GameObject GameObject { get; }
    
    // Methods
    public bool Send(string methodName, string parameter = null)
    public bool SendJson<T>(string methodName, T data)
    public JSChannel RegisterHandler(string messageType, string methodName)
}
```

## Event Types

### EventForwardingOptions

```csharp
[Flags]
public enum EventForwardingOptions
{
    None = 0,
    Awake = 1 << 0,
    Start = 1 << 1,
    Update = 1 << 2,
    FixedUpdate = 1 << 3,
    LateUpdate = 1 << 4,
    OnEnable = 1 << 5,
    OnDisable = 1 << 6,
    OnDestroy = 1 << 7,
    
    // Preset combinations
    LifecycleOnly = Awake | Start | OnDestroy,
    CommonEvents = Awake | Start | OnEnable | OnDisable | OnDestroy,
    AllEvents = ~0,
    AllEventsIncludingCustom = ~0
}
```

## Error Types

### ErrorSeverity

```csharp
public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical,
    Fatal
}
```
