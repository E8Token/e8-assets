# JS Plugin Tools - Core Module

## Purpose

The Core module serves as the foundation of the JS Plugin Tools system, providing the fundamental interfaces, classes, and communication mechanisms required for Unity-JavaScript interaction in WebGL builds. It establishes the architecture that all other plugin modules build upon, ensuring seamless communication between Unity C# code and browser JavaScript.

## Features

- **Modular Architecture**: A flexible plugin system that allows modules to be registered, initialized, and managed independently.
- **Bidirectional Communication**: Enables communication between Unity C# and browser JavaScript using `Application.ExternalCall` and `SendMessage`.
- **Message Passing System**: Structured message passing with support for callbacks and error handling.
- **Module Lifecycle Management**: Standardized initialization and shutdown processes for all modules.
- **Namespaced Code**: Clear namespace conventions for both C# (`Energy8.JSPluginTools`) and JavaScript (`window.UnityWebPlugin`).

## Public API

### C# API

#### IPluginModule Interface

```csharp
public interface IPluginModule
{
    string ModuleId { get; }
    bool IsInitialized { get; }
    bool Initialize();
    void Shutdown();
}
```

#### PluginManager Class

```csharp
public class PluginManager : MonoBehaviour
{
    public static PluginManager Instance { get; }
    public bool IsInitialized { get; }
    
    public bool RegisterModule(IPluginModule module);
    public T GetModule<T>(string moduleId) where T : class, IPluginModule;
    public bool InitializeModule(string moduleId);
    public void ShutdownModule(string moduleId);
    public void ShutdownAllModules();
    public void OnMessageFromJS(string jsonMessage);
}
```

#### PluginModuleBase Class

```csharp
public abstract class PluginModuleBase : IPluginModule
{
    public abstract string ModuleId { get; }
    public bool IsInitialized { get; }
    
    public bool Initialize();
    public void Shutdown();
    
    protected abstract bool OnInitialize();
    protected abstract void OnShutdown();
    protected void SendMessageToJS(string action, string data = null, string callbackId = null);
}
```

#### JSMessageHandlerModuleBase Class

```csharp
public abstract class JSMessageHandlerModuleBase : PluginModuleBase, IJSMessageHandler
{
    public void HandleJSMessage(JSMessage message);
    
    protected void RegisterActionHandler(string action, Action<string> handler);
    protected void SendCallbackResponse(string callbackId, bool success, string errorMessage = null, string data = null);
}
```

#### ExternalCommunicator Class

```csharp
public static class ExternalCommunicator
{
    public static void CallJS(string functionPath, params object[] args);
    public static void SendMessageToJS(string moduleId, string action, string data = null, string callbackId = null);
}
```

### JavaScript API

#### UnityWebPlugin.Core

```javascript
window.UnityWebPlugin.Core = {
    // Module Registration
    registerModule: function(moduleId, module),
    getModule: function(moduleId),
    
    // Communication
    sendMessageToUnity: function(gameObjectName, methodName, message),
    receiveMessageFromUnity: function(jsonMessage),
    sendMessageToUnityModule: function(moduleId, action, data, callback),
    invokeCallback: function(callbackId, responseJson),
    
    // Lifecycle
    initialize: function(),
    notifyModuleInitialized: function(moduleId),
    notifyModuleShutdown: function(moduleId)
}
```

## Initialization and Usage

### In Unity C#

1. **Initial Setup**:
   ```csharp
   // Access the PluginManager singleton
   var pluginManager = PluginManager.Instance;
   ```

2. **Creating a Custom Module**:
   ```csharp
   public class MyCustomModule : JSMessageHandlerModuleBase
   {
       public override string ModuleId => "MyCustomModule";
       
       protected override bool OnInitialize()
       {
           // Register action handlers
           RegisterActionHandler("doSomething", HandleDoSomething);
           return true;
       }
       
       protected override void OnShutdown()
       {
           // Clean up resources
       }
       
       private void HandleDoSomething(string data)
       {
           // Handle the action
           Debug.Log($"Doing something with: {data}");
           
           // Optionally send a message back to JavaScript
           SendMessageToJS("somethingDone", "{ \"result\": \"success\" }");
       }
   }
   ```

3. **Registering and Initializing a Module**:
   ```csharp
   // Create and register the module
   var myModule = new MyCustomModule();
   pluginManager.RegisterModule(myModule);
   
   // Initialize the module
   pluginManager.InitializeModule(myModule.ModuleId);
   ```

### In JavaScript

1. **Creating a Custom Module**:
   ```javascript
   // Define the module
   var myJSModule = {
       handleUnityMessage: function(action, data, callbackId) {
           if (action === "somethingDone") {
               console.log("Unity did something:", JSON.parse(data));
           }
       }
   };
   
   // Register the module
   UnityWebPlugin.Core.registerModule("MyJSModule", myJSModule);
   ```

2. **Sending Messages to Unity**:
   ```javascript
   UnityWebPlugin.Core.sendMessageToUnityModule(
       "MyCustomModule",
       "doSomething",
       { param1: "value1", param2: "value2" },
       function(response) {
           if (response.success) {
               console.log("Success:", response.data);
           } else {
               console.error("Error:", response.errorMessage);
           }
       }
   );
   ```

## JS ↔ C# Interaction Points

The Core module facilitates communication between Unity C# and browser JavaScript through the following interaction points:

1. **Unity to JavaScript**:
   - `ExternalCommunicator.CallJS`: Calls a JavaScript function directly.
   - `ExternalCommunicator.SendMessageToJS`: Sends a structured message to JavaScript.
   - Messages flow through `Application.ExternalCall` to `UnityWebPlugin.Core.receiveMessageFromUnity`.

2. **JavaScript to Unity**:
   - `UnityWebPlugin.Core.sendMessageToUnity`: Sends a message to a Unity GameObject.
   - `UnityWebPlugin.Core.sendMessageToUnityModule`: Sends a structured message to a specific Unity module.
   - Messages flow through `SendMessage` to `PluginManager.OnMessageFromJS` and then to the appropriate module's `HandleJSMessage` method.

3. **Callbacks**:
   - JavaScript can receive asynchronous responses from Unity via callbacks.
   - Callbacks are managed through the `callbackId` system.
   - Unity invokes callbacks using `SendCallbackResponse`, which calls `UnityWebPlugin.Core.invokeCallback`.

## Best Practices

1. **Module Design**:
   - Create small, focused modules that handle specific functionality.
   - Extend `PluginModuleBase` or `JSMessageHandlerModuleBase` for new modules.
   - Use clear, descriptive module IDs that match between C# and JavaScript.

2. **Communication**:
   - Use structured messages with clearly defined actions.
   - Handle errors gracefully and provide meaningful error messages.
   - Use callbacks for asynchronous operations.

3. **Error Handling**:
   - Always check for module initialization before performing operations.
   - Validate inputs and handle edge cases.
   - Provide useful debug information through Unity logs and JavaScript console.

4. **Testing**:
   - Test modules in both the Unity Editor and WebGL builds.
   - Create unit tests for both C# and JavaScript components.
   - Test edge cases like initialization failure and error handling.

## Limitations

- The Core module requires Unity WebGL builds to function as intended.
- Communication between C# and JavaScript is serialized as JSON, which may have performance implications for large data transfers.
- Deep object hierarchies may not serialize correctly; prefer flat structures.
- Function references cannot be passed between C# and JavaScript.

## Extending the Core

When extending the Core module or creating new plugin modules:

1. Follow the established namespace conventions:
   - C#: `Energy8.JSPluginTools.[ModuleName]`
   - JavaScript: `window.UnityWebPlugin.[ModuleName]`

2. Implement the required interfaces:
   - `IPluginModule` for basic modules
   - `IJSMessageHandler` for modules that handle JavaScript messages

3. Document new modules following the same format as the Core module documentation.

4. Create comprehensive tests for new functionality.

5. Register modules with the `PluginManager` to ensure proper lifecycle management.
