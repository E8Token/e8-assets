using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Component that captures Unity lifecycle events and forwards them to JavaScript.
    /// Part of the Event Module in the JSPlugin modular architecture.
    /// </summary>
    /// <remarks>
    /// This component acts as a bridge between Unity's lifecycle events and JavaScript,
    /// allowing JavaScript code to respond to Unity events like Awake, Start, Update, etc.
    /// </remarks>
    public class JSPluginEvents : MonoBehaviour
    {
        private JSPluginObject pluginObject;
        private EventForwardingOptions options;
        private bool initialized = false;
        
        // Storage for pending events
        private readonly List<string> pendingEvents = new();

        // Dictionary to store custom event handlers
        private readonly Dictionary<string, System.Action<string>> customEventHandlers = new();
        
        // Dictionary to store event priorities
        private readonly Dictionary<string, int> eventPriorities = new();
        
        /// <summary>
        /// Initializes the event system for the specified plugin object.
        /// </summary>
        /// <param name="pluginObject">The JS plugin object to forward events to</param>
        /// <param name="options">Options specifying which events to forward</param>
        /// <returns>This component for method chaining</returns>
        public JSPluginEvents Initialize(JSPluginObject pluginObject, EventForwardingOptions options)
        {
            this.pluginObject = pluginObject;
            this.options = options;
            initialized = true;
            
            // Process any events that were triggered before initialization
            foreach (var eventName in pendingEvents)
            {
                TriggerEvent(eventName);
            }
            pendingEvents.Clear();
            
            return this;
        }
        
        /// <summary>
        /// Registers a custom event handler for events not covered by the standard Unity lifecycle.
        /// </summary>
        /// <param name="eventName">The name of the custom event</param>
        /// <param name="handler">The action to execute when the event is triggered</param>
        /// <param name="priority">Priority of the event (higher numbers execute first)</param>
        /// <returns>This component for chaining</returns>
        public JSPluginEvents RegisterCustomEvent(string eventName, System.Action<string> handler, int priority = 0)
        {
            customEventHandlers[eventName] = handler;
            eventPriorities[eventName] = priority;
            return this;
        }
        
        /// <summary>
        /// Unregisters a custom event handler.
        /// </summary>
        /// <param name="eventName">The name of the custom event to unregister</param>
        /// <returns>This component for chaining</returns>
        public JSPluginEvents UnregisterCustomEvent(string eventName)
        {
            if (customEventHandlers.ContainsKey(eventName))
            {
                customEventHandlers.Remove(eventName);
                eventPriorities.Remove(eventName);
            }
            return this;
        }
        
        /// <summary>
        /// Triggers a named event to be sent to JavaScript.
        /// </summary>
        /// <param name="eventName">The name of the event to trigger</param>
        /// <param name="data">Optional data to pass with the event</param>
        public void TriggerEvent(string eventName, string data = null)
        {
            if (!initialized)
            {
                pendingEvents.Add(eventName);
                return;
            }
            
            // First execute any registered custom handlers
            if (customEventHandlers.TryGetValue(eventName, out var handler))
            {
                handler(data);
            }
            
            // Then forward to JavaScript
            pluginObject?.TriggerEvent(eventName, data);
        }
        
        /// <summary>
        /// Determines if a specific event should be forwarded based on the options.
        /// </summary>
        /// <param name="eventName">The name of the event to check</param>
        /// <returns>True if the event should be forwarded</returns>
        private bool ShouldForwardEvent(string eventName)
        {
            if (!initialized) return false;

            return eventName switch
            {
                "Awake" => options.HasFlag(EventForwardingOptions.Awake),
                "Start" => options.HasFlag(EventForwardingOptions.Start),
                "Update" => options.HasFlag(EventForwardingOptions.Update),
                "FixedUpdate" => options.HasFlag(EventForwardingOptions.FixedUpdate),
                "LateUpdate" => options.HasFlag(EventForwardingOptions.LateUpdate),
                "OnEnable" => options.HasFlag(EventForwardingOptions.OnEnable),
                "OnDisable" => options.HasFlag(EventForwardingOptions.OnDisable),
                "OnDestroy" => options.HasFlag(EventForwardingOptions.OnDestroy),
                _ => customEventHandlers.ContainsKey(eventName), // Allow custom events
            };
        }
        
        /// <summary>
        /// Sets the priority of a specific event.
        /// Higher priority events are processed first.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="priority">Priority value (higher = processed first)</param>
        /// <returns>This component for chaining</returns>
        public JSPluginEvents SetEventPriority(string eventName, int priority)
        {
            eventPriorities[eventName] = priority;
            return this;
        }
        
        /// <summary>
        /// Gets the priority of a specific event.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <returns>Priority value or 0 if not set</returns>
        public int GetEventPriority(string eventName)
        {
            return eventPriorities.TryGetValue(eventName, out int priority) ? priority : 0;
        }
        
        // Unity event implementations
        private void Awake()
        {
            if (ShouldForwardEvent("Awake"))
                TriggerEvent("Awake");
        }
        
        private void Start()
        {
            if (ShouldForwardEvent("Start"))
                TriggerEvent("Start");
        }
        
        private void Update()
        {
            if (ShouldForwardEvent("Update"))
                TriggerEvent("Update");
        }
        
        private void FixedUpdate()
        {
            if (ShouldForwardEvent("FixedUpdate"))
                TriggerEvent("FixedUpdate");
        }
        
        private void LateUpdate()
        {
            if (ShouldForwardEvent("LateUpdate"))
                TriggerEvent("LateUpdate");
        }
        
        private void OnEnable()
        {
            if (ShouldForwardEvent("OnEnable"))
                TriggerEvent("OnEnable");
        }
        
        private void OnDisable()
        {
            if (ShouldForwardEvent("OnDisable"))
                TriggerEvent("OnDisable");
        }
        
        private void OnDestroy()
        {
            if (ShouldForwardEvent("OnDestroy"))
                TriggerEvent("OnDestroy");
        }
    }
    
    /// <summary>
    /// Flags for selecting which Unity events should be forwarded to JavaScript.
    /// </summary>
    [System.Flags]
    public enum EventForwardingOptions
    {
        /// <summary>No events will be forwarded</summary>
        None = 0,
        
        /// <summary>Forward the Awake event</summary>
        Awake = 1 << 0,
        
        /// <summary>Forward the Start event</summary>
        Start = 1 << 1,
        
        /// <summary>Forward the Update event</summary>
        Update = 1 << 2,
        
        /// <summary>Forward the FixedUpdate event</summary>
        FixedUpdate = 1 << 3,
        
        /// <summary>Forward the LateUpdate event</summary>
        LateUpdate = 1 << 4,
        
        /// <summary>Forward the OnEnable event</summary>
        OnEnable = 1 << 5,
        
        /// <summary>Forward the OnDisable event</summary>
        OnDisable = 1 << 6,
        
        /// <summary>Forward the OnDestroy event</summary>
        OnDestroy = 1 << 7,
        
        // Preset combinations
        /// <summary>Forward only lifecycle events (Awake, Start, OnDestroy)</summary>
        LifecycleOnly = Awake | Start | OnDestroy,
        
        /// <summary>Forward common events (Awake, Start, OnEnable, OnDisable, OnDestroy)</summary>
        CommonEvents = Awake | Start | OnEnable | OnDisable | OnDestroy,
        
        /// <summary>Forward all supported events</summary>
        AllEvents = ~0,
        
        /// <summary>Forward all supported events including custom events</summary>
        AllEventsIncludingCustom = ~0
    }
    
    /// <summary>
    /// Extension for JSPluginObject to work with the new events system
    /// </summary>
    public static class JSPluginObjectEventsExtension
    {
        /// <summary>
        /// Configures forwarding of Unity events to JavaScript with the new event system.
        /// </summary>
        /// <param name="pluginObject">The JS plugin object</param>
        /// <param name="options">Which events should be forwarded</param>
        /// <returns>The JSPluginEvents component for further configuration</returns>
        public static JSPluginEvents ConfigureEvents(this JSPluginObject pluginObject, EventForwardingOptions options = EventForwardingOptions.CommonEvents)
        {
            var gameObject = pluginObject.GameObject;
            
            if (!gameObject.TryGetComponent<JSPluginEvents>(out var events))
            {
                events = gameObject.AddComponent<JSPluginEvents>();
            }

            events.Initialize(pluginObject, options);
            return events;
        }
    }
}