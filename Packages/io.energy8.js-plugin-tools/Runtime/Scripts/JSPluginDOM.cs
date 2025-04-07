using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// DOM manipulation module for JSPluginTools.
    /// Provides methods to interact with browser DOM elements from Unity.
    /// </summary>
    public static class JSPluginDOM
    {
        #region Native Methods
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMInitialize();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMCreateElement(string tagName, string id, string className);
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDOMGetElement(string selector);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMSetElementProperty(string elementId, string property, string value);
        
        [DllImport("__Internal")]
        private static extern IntPtr JSPluginDOMGetElementProperty(string elementId, string property);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMSetElementStyle(string elementId, string property, string value);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMSetElementContent(string elementId, string content);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMAppendToElement(string parentId, string childId);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMRemoveElement(string elementId);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMAddEventListener(string elementId, string eventType, string callbackObjectId, string callbackMethod);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMRemoveEventListener(string elementId, string eventType);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMIsElementVisible(string elementId);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMToggleVisibility(string elementId, int visible);
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMShutdown();
        
        [DllImport("__Internal")]
        private static extern int JSPluginDOMCleanupAllElements();
        
        #endregion
        
        private static bool isInitialized = false;
        private static Dictionary<string, Element> activeElements = new Dictionary<string, Element>();
        
        private static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            
            return Marshal.PtrToStringUTF8(ptr);
        }
        
        /// <summary>
        /// Initializes the DOM module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            if (isInitialized)
                return true;
                
            #if UNITY_WEBGL && !UNITY_EDITOR
            isInitialized = JSPluginDOMInitialize() == 1;
            #else
            Debug.Log("[JSPluginDOM] Initialized in stub mode (non-WebGL environment)");
            isInitialized = true;
            #endif
            
            if (isInitialized)
            {
                JSPluginErrorHandling.LogEvent("JSPluginDOM", "DOM module initialized successfully", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            
            return isInitialized;
        }
        
        /// <summary>
        /// Shuts down the DOM module and cleans up resources
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            try
            {
                // Clean up all active elements
                foreach (var element in new Dictionary<string, Element>(activeElements))
                {
                    element.Value.RemoveAllEventListeners();
                }
                
                activeElements.Clear();
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMCleanupAllElements();
                JSPluginDOMShutdown();
                #endif
                
                isInitialized = false;
                JSPluginErrorHandling.LogEvent("JSPluginDOM", "DOM module shut down", JSPluginErrorHandling.ErrorSeverity.Info);
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginDOM", "Shutdown", ex, JSPluginErrorHandling.ErrorSeverity.Error);
            }
        }
        
        /// <summary>
        /// Represents a DOM element in the browser
        /// </summary>
        public class Element
        {
            /// <summary>The DOM element's ID</summary>
            public string Id { get; }
            
            // Track registered event listeners for cleanup
            private readonly HashSet<string> registeredEventTypes = new HashSet<string>();
            
            /// <summary>Creates a new Element instance</summary>
            /// <param name="id">DOM element ID</param>
            public Element(string id)
            {
                Id = id;
            }
            
            /// <summary>Sets a property on the element</summary>
            /// <param name="property">Property name</param>
            /// <param name="value">Property value</param>
            /// <returns>This Element for method chaining</returns>
            public Element SetProperty(string property, string value)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMSetElementProperty(Id, property, value);
                #else
                Debug.Log($"[JSPluginDOM] Would set property {property}={value} on element {Id}");
                #endif
                return this;
            }
            
            /// <summary>Gets a property value from the element</summary>
            /// <param name="property">Property name</param>
            /// <returns>Property value or null if not found</returns>
            public string GetProperty(string property)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                IntPtr ptr = JSPluginDOMGetElementProperty(Id, property);
                return PtrToString(ptr);
                #else
                Debug.Log($"[JSPluginDOM] Would get property {property} from element {Id}");
                return "stub-value-" + property;
                #endif
            }
            
            /// <summary>Sets a style property on the element</summary>
            /// <param name="property">Style property name</param>
            /// <param name="value">Style value</param>
            /// <returns>This Element for method chaining</returns>
            public Element SetStyle(string property, string value)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMSetElementStyle(Id, property, value);
                #else
                Debug.Log($"[JSPluginDOM] Would set style {property}={value} on element {Id}");
                #endif
                return this;
            }
            
            /// <summary>Sets multiple style properties at once</summary>
            /// <param name="styles">Dictionary of style properties and values</param>
            /// <returns>This Element for method chaining</returns>
            public Element SetStyles(Dictionary<string, string> styles)
            {
                foreach (var style in styles)
                {
                    SetStyle(style.Key, style.Value);
                }
                return this;
            }
            
            /// <summary>Sets the element's inner HTML content</summary>
            /// <param name="content">HTML content to set</param>
            /// <returns>This Element for method chaining</returns>
            public Element SetContent(string content)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMSetElementContent(Id, content);
                #else
                Debug.Log($"[JSPluginDOM] Would set content on element {Id}: {content}");
                #endif
                return this;
            }
            
            /// <summary>Appends a child element to this element</summary>
            /// <param name="child">Child element to append</param>
            /// <returns>This Element for method chaining</returns>
            public Element AppendChild(Element child)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMAppendToElement(Id, child.Id);
                #else
                Debug.Log($"[JSPluginDOM] Would append child {child.Id} to element {Id}");
                #endif
                return this;
            }
            
            /// <summary>Removes this element from the DOM and cleans up resources</summary>
            public void Remove()
            {
                try
                {
                    // First remove all event listeners
                    RemoveAllEventListeners();
                    
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    JSPluginDOMRemoveElement(Id);
                    #else
                    JSPluginErrorHandling.LogEvent("JSPluginDOM", $"Would remove element {Id}", JSPluginErrorHandling.ErrorSeverity.Info);
                    #endif
                    
                    // Remove from active elements list
                    activeElements.Remove(Id);
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException("JSPluginDOM", "Remove", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                }
            }
            
            /// <summary>Adds an event listener to this element</summary>
            /// <param name="eventType">Type of event (click, mouseover, etc.)</param>
            /// <param name="callbackObjectId">Object ID for callback</param>
            /// <param name="callbackMethod">Method name to call</param>
            /// <returns>This Element for method chaining</returns>
            public Element AddEventListener(string eventType, string callbackObjectId, string callbackMethod)
            {
                try 
                {
                    if (string.IsNullOrEmpty(eventType))
                    {
                        JSPluginErrorHandling.LogEvent("JSPluginDOM", "Event type cannot be null or empty", JSPluginErrorHandling.ErrorSeverity.Warning);
                        return this;
                    }
                    
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    int result = JSPluginDOMAddEventListener(Id, eventType, callbackObjectId, callbackMethod);
                    if (result == 1)
                    {
                        registeredEventTypes.Add(eventType);
                    }
                    #else
                    JSPluginErrorHandling.LogEvent("JSPluginDOM", $"Would add {eventType} event listener to {Id} with callback {callbackObjectId}.{callbackMethod}", JSPluginErrorHandling.ErrorSeverity.Info);
                    registeredEventTypes.Add(eventType);
                    #endif
                    
                    return this;
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException("JSPluginDOM", $"Adding event listener {eventType}", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                    return this;
                }
            }
            
            /// <summary>Removes an event listener from this element</summary>
            /// <param name="eventType">Type of event to remove</param>
            /// <returns>This Element for method chaining</returns>
            public Element RemoveEventListener(string eventType)
            {
                try
                {
                    if (string.IsNullOrEmpty(eventType))
                    {
                        JSPluginErrorHandling.LogEvent("JSPluginDOM", "Event type cannot be null or empty", JSPluginErrorHandling.ErrorSeverity.Warning);
                        return this;
                    }
                    
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    int result = JSPluginDOMRemoveEventListener(Id, eventType);
                    if (result == 1)
                    {
                        registeredEventTypes.Remove(eventType);
                    }
                    #else
                    JSPluginErrorHandling.LogEvent("JSPluginDOM", $"Would remove {eventType} event listener from {Id}", JSPluginErrorHandling.ErrorSeverity.Info);
                    registeredEventTypes.Remove(eventType);
                    #endif
                    
                    return this;
                }
                catch (Exception ex)
                {
                    JSPluginErrorHandling.ProcessException("JSPluginDOM", $"Removing event listener {eventType}", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                    return this;
                }
            }
            
            /// <summary>
            /// Removes all event listeners registered on this element
            /// </summary>
            /// <returns>This Element for method chaining</returns>
            public Element RemoveAllEventListeners()
            {
                foreach (var eventType in new HashSet<string>(registeredEventTypes))
                {
                    RemoveEventListener(eventType);
                }
                return this;
            }
            
            /// <summary>Checks if this element is visible</summary>
            /// <returns>True if visible, false otherwise</returns>
            public bool IsVisible()
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                return JSPluginDOMIsElementVisible(Id) == 1;
                #else
                Debug.Log($"[JSPluginDOM] Would check visibility of element {Id}");
                return true;
                #endif
            }
            
            /// <summary>Shows or hides this element</summary>
            /// <param name="visible">True to show, false to hide</param>
            /// <returns>This Element for method chaining</returns>
            public Element SetVisible(bool visible)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                JSPluginDOMToggleVisibility(Id, visible ? 1 : 0);
                #else
                Debug.Log($"[JSPluginDOM] Would set element {Id} visibility to {visible}");
                #endif
                return this;
            }
            
            /// <summary>Shows this element</summary>
            /// <returns>This Element for method chaining</returns>
            public Element Show()
            {
                return SetVisible(true);
            }
            
            /// <summary>Hides this element</summary>
            /// <returns>This Element for method chaining</returns>
            public Element Hide()
            {
                return SetVisible(false);
            }
        }
        
        /// <summary>
        /// Creates a new DOM element
        /// </summary>
        /// <param name="tagName">HTML tag name (div, span, etc.)</param>
        /// <param name="id">Element ID (optional)</param>
        /// <param name="className">CSS class name (optional)</param>
        /// <returns>A new Element instance or null if creation failed</returns>
        public static Element CreateElement(string tagName, string id = null, string className = null)
        {
            try
            {
                if (!isInitialized)
                    Initialize();
                    
                if (string.IsNullOrEmpty(tagName))
                {
                    JSPluginErrorHandling.LogEvent("JSPluginDOM", "Tag name cannot be null or empty", JSPluginErrorHandling.ErrorSeverity.Error);
                    return null;
                }
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                id = id ?? "unity-" + Guid.NewGuid().ToString().Substring(0, 8);
                if (JSPluginDOMCreateElement(tagName, id, className) != 1)
                {
                    JSPluginErrorHandling.LogEvent("JSPluginDOM", $"Failed to create {tagName} element", JSPluginErrorHandling.ErrorSeverity.Error);
                    return null;
                }
                
                var element = new Element(id);
                activeElements[id] = element;
                return element;
                #else
                id = id ?? "unity-stub-" + UnityEngine.Random.Range(1000, 9999);
                JSPluginErrorHandling.LogEvent("JSPluginDOM", $"Would create {tagName} element with id={id}, class={className}", JSPluginErrorHandling.ErrorSeverity.Info);
                var element = new Element(id);
                activeElements[id] = element;
                return element;
                #endif
            }
            catch (Exception ex)
            {
                JSPluginErrorHandling.ProcessException("JSPluginDOM", "CreateElement", ex, JSPluginErrorHandling.ErrorSeverity.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Gets an existing DOM element by ID or CSS selector
        /// </summary>
        /// <param name="selector">Element ID or CSS selector</param>
        /// <returns>Element instance or null if not found</returns>
        public static Element GetElement(string selector)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = JSPluginDOMGetElement(selector);
            string result = PtrToString(ptr);
            if (string.IsNullOrEmpty(result))
                return null;
            return new Element(result);
            #else
            Debug.Log($"[JSPluginDOM] Would get element with selector {selector}");
            return new Element("stub-" + selector);
            #endif
        }
        
        /// <summary>
        /// Utility methods for common DOM operations
        /// </summary>
        public static class Utility
        {
            /// <summary>
            /// Creates an overlay with the specified content
            /// </summary>
            /// <param name="content">HTML content for the overlay</param>
            /// <param name="id">Optional ID for the overlay</param>
            /// <returns>The created overlay element</returns>
            public static Element CreateOverlay(string content, string id = null)
            {
                id = id ?? "unity-overlay-" + Guid.NewGuid().ToString().Substring(0, 8);
                
                var overlay = CreateElement("div", id);
                overlay.SetStyles(new Dictionary<string, string>
                {
                    { "position", "fixed" },
                    { "top", "0" },
                    { "left", "0" },
                    { "width", "100%" },
                    { "height", "100%" },
                    { "background-color", "rgba(0, 0, 0, 0.7)" },
                    { "display", "flex" },
                    { "justify-content", "center" },
                    { "align-items", "center" },
                    { "z-index", "1000" }
                });
                
                var contentDiv = CreateElement("div", id + "-content");
                contentDiv.SetStyles(new Dictionary<string, string>
                {
                    { "background-color", "white" },
                    { "padding", "20px" },
                    { "border-radius", "5px" },
                    { "max-width", "80%" },
                    { "max-height", "80%" },
                    { "overflow", "auto" }
                });
                
                contentDiv.SetContent(content);
                overlay.AppendChild(contentDiv);
                
                var closeButton = CreateElement("button", id + "-close", "close-button");
                closeButton.SetContent("×");
                closeButton.SetStyles(new Dictionary<string, string>
                {
                    { "position", "absolute" },
                    { "top", "10px" },
                    { "right", "10px" },
                    { "background", "none" },
                    { "border", "none" },
                    { "font-size", "24px" },
                    { "cursor", "pointer" },
                    { "color", "white" }
                });
                
                closeButton.AddEventListener("click", "JSPluginDOM", "CloseOverlay");
                overlay.AppendChild(closeButton);
                
                // Add to body
                var body = GetElement("body");
                body.AppendChild(overlay);
                
                return overlay;
            }
            
            /// <summary>
            /// Closes an overlay by ID
            /// </summary>
            /// <param name="id">Overlay ID</param>
            public static void CloseOverlay(string id)
            {
                var overlay = GetElement(id);
                if (overlay != null)
                {
                    overlay.Remove();
                }
            }
            
            /// <summary>
            /// Creates a toast notification
            /// </summary>
            /// <param name="message">Message to display</param>
            /// <param name="duration">Duration in seconds</param>
            /// <param name="type">Type of toast (info, success, warning, error)</param>
            /// <returns>The created toast element</returns>
            public static Element ShowToast(string message, float duration = 3f, string type = "info")
            {
                string id = "unity-toast-" + Guid.NewGuid().ToString().Substring(0, 8);
                
                var toast = CreateElement("div", id, "unity-toast unity-toast-" + type);
                toast.SetContent(message);
                
                var bgColor = "rgba(50, 50, 50, 0.8)";
                switch (type)
                {
                    case "success": bgColor = "rgba(40, 167, 69, 0.9)"; break;
                    case "warning": bgColor = "rgba(255, 193, 7, 0.9)"; break;
                    case "error": bgColor = "rgba(220, 53, 69, 0.9)"; break;
                    case "info": default: bgColor = "rgba(23, 162, 184, 0.9)"; break;
                }
                
                toast.SetStyles(new Dictionary<string, string>
                {
                    { "position", "fixed" },
                    { "bottom", "20px" },
                    { "left", "50%" },
                    { "transform", "translateX(-50%)" },
                    { "background-color", bgColor },
                    { "color", "white" },
                    { "padding", "10px 20px" },
                    { "border-radius", "4px" },
                    { "opacity", "0" },
                    { "transition", "opacity 0.3s ease-in-out" },
                    { "z-index", "1001" }
                });
                
                var body = GetElement("body");
                body.AppendChild(toast);
                
                // Animate in
                JSPluginCommunication.QueueMessage("JSPluginDOM", "AnimateToast", 
                    JsonUtility.ToJson(new { id, visible = true, duration }));
                
                // Set timeout to remove
                JSPluginCommunication.QueueMessage("JSPluginDOM", "RemoveToastAfterDelay", 
                    JsonUtility.ToJson(new { id, delay = duration }));
                
                return toast;
            }
        }
    }
}
