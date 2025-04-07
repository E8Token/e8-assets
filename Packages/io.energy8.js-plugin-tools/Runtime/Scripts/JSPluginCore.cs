using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Core utility class for working with JavaScript plugins in Unity WebGL.
    /// Provides methods for communication between Unity and JavaScript code.
    /// </summary>
    public static class JSPluginCore
    {
        #region Native Methods

        [DllImport("__Internal")]
        private static extern void JSPluginInitializeTools(bool debugMode);

        [DllImport("__Internal")]
        private static extern bool JSPluginRegisterUnityInstance(IntPtr instance);

        [DllImport("__Internal")]
        private static extern int JSPluginRegisterUnityObject(string objectId, string objectName);

        [DllImport("__Internal")]
        private static extern int JSPluginRegisterCallback(string objectId, string callbackType, string methodName);

        [DllImport("__Internal")]
        private static extern int JSPluginRegisterErrorHandler(string objectId, string methodName);

        [DllImport("__Internal")]
        private static extern bool JSPluginSendMessage(string objectId, string methodName, string parameter);

        [DllImport("__Internal")]
        private static extern bool JSPluginTriggerCallback(string objectId, string callbackType, string parameter);

        [DllImport("__Internal")]
        private static extern bool JSPluginReportError(string objectId, string context, string error);

        [DllImport("__Internal")]
        private static extern int JSPluginExportToWindow();

        [DllImport("__Internal")]
        private static extern void JSPluginShutdownTools();

        #endregion

        private static readonly Dictionary<string, JSPluginObject> registeredObjects = new();

        private static bool isInitialized = false;

        /// <summary>
        /// Gets a value indicating whether the JSPluginCore system has been initialized.
        /// </summary>
        public static bool IsInitialized => isInitialized;

        /// <summary>
        /// Initializes the JS plugin system.
        /// Must be called before using any other methods.
        /// </summary>
        /// <param name="debugMode">Enable detailed debug logging</param>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize(bool debugMode = false)
        {
            if (isInitialized)
                return true;

            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Initialize the core JS plugin system
                JSPluginInitializeTools(debugMode);
                
                // Export our tools to the global window object
                int exportResult = JSPluginExportToWindow();
                
                isInitialized = true;
                Debug.Log("JSPluginCore initialized in WebGL build");
                
                return true;
#else
                Debug.Log("JSPluginCore initialized in editor/non-WebGL (stub mode)");
                isInitialized = true;
                return true;
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginCore] Error during initialization: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Shuts down the JS plugin system and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            try
            {
                // Clear all registered objects
                registeredObjects.Clear();
                
#if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginShutdownTools();
#endif
                
                isInitialized = false;
                Debug.Log("[JSPluginCore] Shut down");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSPluginCore] Error during shutdown: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers a Unity GameObject to be used with JS callbacks.
        /// </summary>
        /// <param name="objectId">Unique identifier for the object</param>
        /// <param name="gameObject">Unity GameObject that will receive messages</param>
        /// <returns>A JSPluginObject that can be used to manage callbacks</returns>
        /// <exception cref="ArgumentException">Thrown when objectId is null or empty</exception>
        /// <exception cref="ArgumentNullException">Thrown when gameObject is null</exception>
        public static JSPluginObject RegisterObject(string objectId, GameObject gameObject)
        {
            if (string.IsNullOrEmpty(objectId))
                throw new ArgumentException("Object ID cannot be null or empty", nameof(objectId));

            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject), "GameObject cannot be null");

            // Ensure we're initialized
            if (!isInitialized)
                Initialize();

            // Check if already registered
            if (registeredObjects.TryGetValue(objectId, out JSPluginObject existingObject))
                return existingObject;

            // Register in JS
#if UNITY_WEBGL && !UNITY_EDITOR
            JSPluginRegisterUnityObject(objectId, gameObject.name);
#endif

            // Create and store the wrapper object
            var pluginObject = new JSPluginObject(objectId, gameObject);
            registeredObjects[objectId] = pluginObject;

            return pluginObject;
        }

        /// <summary>
        /// Gets a previously registered JS plugin object.
        /// </summary>
        /// <param name="objectId">The ID used during registration</param>
        /// <returns>The registered object or null if not found</returns>
        public static JSPluginObject GetObject(string objectId)
        {
            if (registeredObjects.TryGetValue(objectId, out JSPluginObject obj))
                return obj;

            return null;
        }

        #region Internal Methods

        /// <summary>
        /// Registers a callback method for a specific object and callback type.
        /// </summary>
        /// <param name="objectId">The object identifier</param>
        /// <param name="callbackType">The type of callback</param>
        /// <param name="methodName">The method name to call</param>
        internal static void RegisterCallback(string objectId, string callbackType, string methodName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            JSPluginRegisterCallback(objectId, callbackType, methodName);
#endif
        }

        /// <summary>
        /// Registers an error handler method for a specific object.
        /// </summary>
        /// <param name="objectId">The object identifier</param>
        /// <param name="methodName">The method name to call on error</param>
        internal static void RegisterErrorHandler(string objectId, string methodName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            JSPluginRegisterErrorHandler(objectId, methodName);
#endif
        }

        /// <summary>
        /// Sends a message to JavaScript.
        /// </summary>
        /// <param name="objectId">The object identifier</param>
        /// <param name="methodName">The JavaScript method to call</param>
        /// <param name="parameter">Optional parameter to pass</param>
        /// <returns>True if successful</returns>
        internal static bool SendMessageToJS(string objectId, string methodName, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginSendMessage(objectId, methodName, parameter);
#else
            Debug.Log($"[JSPluginUtils] Would send to {objectId}.{methodName}: {parameter}");
            return true;
#endif
        }

        /// <summary>
        /// Triggers a callback from JavaScript to Unity.
        /// </summary>
        /// <param name="objectId">The object identifier</param>
        /// <param name="callbackType">The type of callback to trigger</param>
        /// <param name="parameter">Optional parameter to pass</param>
        /// <returns>True if successful</returns>
        internal static bool TriggerCallback(string objectId, string callbackType, string parameter)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginTriggerCallback(objectId, callbackType, parameter);
#else
            Debug.Log($"[JSPluginUtils] Would trigger callback {callbackType} on {objectId} with: {parameter}");
            return true;
#endif
        }

        /// <summary>
        /// Reports an error to JavaScript.
        /// </summary>
        /// <param name="objectId">The object identifier</param>
        /// <param name="context">Context where the error occurred</param>
        /// <param name="error">The error message</param>
        /// <returns>True if successful</returns>
        internal static bool ReportError(string objectId, string context, string error)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return JSPluginReportError(objectId, context, error);
#else
            Debug.LogError($"[JSPluginUtils] Error in {context} for {objectId}: {error}");
            return true;
#endif
        }

        #endregion
    }

    /// <summary>
    /// Represents a Unity object registered for JavaScript interactions.
    /// Provides methods for communication between Unity and JavaScript.
    /// </summary>
    public class JSPluginObject
    {
        /// <summary>
        /// Gets the unique identifier for this object.
        /// </summary>
        public string ObjectId { get; }

        /// <summary>
        /// Gets the Unity GameObject this object is attached to.
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// Initializes a new instance of the JSPluginObject class.
        /// </summary>
        /// <param name="objectId">The unique identifier for this object</param>
        /// <param name="gameObject">The Unity GameObject this object is attached to</param>
        internal JSPluginObject(string objectId, GameObject gameObject)
        {
            ObjectId = objectId;
            GameObject = gameObject;
        }

        /// <summary>
        /// Registers a callback method on this object.
        /// </summary>
        /// <param name="callbackType">Type/name of the callback</param>
        /// <param name="methodName">Name of the method to call on the GameObject</param>
        public void RegisterCallback(string callbackType, string methodName)
        {
            JSPluginCore.RegisterCallback(ObjectId, callbackType, methodName);
        }

        /// <summary>
        /// Registers an error handler method for this object.
        /// </summary>
        /// <param name="methodName">Name of the method to call for errors</param>
        public void RegisterErrorHandler(string methodName)
        {
            JSPluginCore.RegisterErrorHandler(ObjectId, methodName);
        }

        /// <summary>
        /// Sends a message to JavaScript.
        /// </summary>
        /// <param name="methodName">Name of the JavaScript method to call</param>
        /// <param name="parameter">Optional parameter to pass</param>
        /// <returns>True if successful</returns>
        public bool SendMessage(string methodName, string parameter = null)
        {
            return JSPluginCore.SendMessageToJS(ObjectId, methodName, parameter);
        }

        /// <summary>
        /// Sends a message to JavaScript with a JSON serialized object.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="methodName">Name of the JavaScript method to call</param>
        /// <param name="data">Object to serialize to JSON</param>
        /// <returns>True if successful</returns>
        public bool SendMessageJson<T>(string methodName, T data)
        {
            string json = JsonUtility.ToJson(data);
            return SendMessage(methodName, json);
        }

        /// <summary>
        /// Triggers a callback on this object.
        /// </summary>
        /// <param name="callbackType">Type of callback to trigger</param>
        /// <param name="parameter">Optional parameter to pass</param>
        /// <returns>True if successful</returns>
        public bool TriggerCallback(string callbackType, string parameter = null)
        {
            return JSPluginCore.TriggerCallback(ObjectId, callbackType, parameter);
        }

        /// <summary>
        /// Reports an error through the registered error handler.
        /// </summary>
        /// <param name="context">Context where the error occurred</param>
        /// <param name="error">The error message</param>
        /// <returns>True if successful</returns>
        public bool ReportError(string context, string error)
        {
            return JSPluginCore.ReportError(ObjectId, context, error);
        }

        /// <summary>
        /// Reports an exception through the registered error handler.
        /// </summary>
        /// <param name="context">Context where the exception occurred</param>
        /// <param name="exception">The exception that occurred</param>
        /// <returns>True if successful</returns>
        public bool ReportException(string context, Exception exception)
        {
            return ReportError(context, exception.ToString());
        }

        /// <summary>
        /// Configures forwarding of standard Unity events to JavaScript.
        /// </summary>
        /// <param name="options">Which events should be forwarded</param>
        /// <returns>The current plugin object for method chaining</returns>
        public JSPluginObject ForwardUnityEvents(EventForwardingOptions options = EventForwardingOptions.CommonEvents)
        {
            this.ConfigureEvents(options);
            return this;
        }

        /// <summary>
        /// Registers a Unity event handler in JavaScript.
        /// </summary>
        /// <param name="eventName">Name of the event (Awake, Start, Update, etc.)</param>
        /// <param name="jsFunction">Name of the JavaScript function to be called</param>
        /// <returns>The current plugin object for method chaining</returns>
        public JSPluginObject RegisterEventHandler(string eventName, string jsFunction)
        {
            RegisterCallback("event_" + eventName, jsFunction);
            return this;
        }

        /// <summary>
        /// Triggers an event in JavaScript.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="parameter">Optional parameter</param>
        /// <returns>True if successful</returns>
        public bool TriggerEvent(string eventName, string parameter = null)
        {
            return JSPluginCore.TriggerCallback(ObjectId, "event_" + eventName, parameter);
        }

        /// <summary>
        /// Registers handlers for all standard Unity events.
        /// </summary>
        /// <param name="handlerMethodName">Base name of the handler method. The event name will be appended to it.</param>
        /// <returns>The current plugin object for method chaining</returns>
        public JSPluginObject RegisterCommonEventHandlers(string handlerMethodName = "Handle")
        {
            RegisterEventHandler("Awake", handlerMethodName + "AwakeEvent");
            RegisterEventHandler("OnEnable", handlerMethodName + "EnableEvent");
            RegisterEventHandler("Start", handlerMethodName + "StartEvent");
            RegisterEventHandler("Update", handlerMethodName + "UpdateEvent");
            RegisterEventHandler("FixedUpdate", handlerMethodName + "FixedUpdateEvent");
            RegisterEventHandler("LateUpdate", handlerMethodName + "LateUpdateEvent");
            RegisterEventHandler("OnDisable", handlerMethodName + "DisableEvent");
            RegisterEventHandler("OnDestroy", handlerMethodName + "DestroyEvent");
            return this;
        }
    }
}