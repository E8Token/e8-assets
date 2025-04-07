var JSPluginCore = {
    /**
     * Core state object shared across all plugin modules
     * @private
     */
    $JSPluginState: {
        callbacks: {},
        errorHandlers: {},
        objects: {},
        unityInstance: null,
        initialized: false,
        debugEnabled: false,
        eventHandlers: {},
        traceEvents: false,
        memoryManager: {
            allocatedStrings: {},
            nextAllocationId: 1,
            totalAllocations: 0,
            totalDeallocations: 0,
            // Track memory usage statistics
            stats: {
                currentAllocationCount: 0,
                peakAllocationCount: 0,
                totalBytesAllocated: 0
            }
        }
    },
    
    /**
     * Helper functions for plugin operations
     * @private
     */
    $JSPluginHelper: {
        /**
         * Attempts to initialize the plugin system
         * @param {Object} instance - Optional Unity instance object
         * @return {boolean} True if initialization was successful
         */
        tryInitialize: function(instance) {
            if (JSPluginState.initialized) return true;
            
            try {
                // Поиск Unity instance
                if (instance) {
                    JSPluginState.unityInstance = instance;
                } else if (typeof window !== 'undefined') {
                    JSPluginState.unityInstance = window.unityInstance || 
                                              window.gameInstance || 
                                              (window.Module ? window.Module.unityInstance : null);
                }
                
                JSPluginState.initialized = !!JSPluginState.unityInstance;
                
                if (JSPluginState.initialized) {
                    this.log("Initialized successfully");
                } else {
                    this.logError("Failed to initialize: Unity instance not found");
                }
                
                return JSPluginState.initialized;
            } catch (error) {
                this.logError("Error during initialization: " + error);
                return false;
            }
        },
        
        /**
         * Logs an informational message
         * @param {string} message - The message to log
         */
        log: function(message) {
            console.log("[UnityJSTools] " + message);
        },
        
        /**
         * Logs an error message
         * @param {string} message - The error message to log
         */
        logError: function(message) {
            console.error("[UnityJSTools] " + message);
        },
        
        /**
         * Logs a debug message (only when debug mode is enabled)
         * @param {string} message - The debug message to log
         */
        logDebug: function(message) {
            if (JSPluginState.debugEnabled) {
                console.log("[UnityJSTools Debug] " + message);
            }
        },
        
        /**
         * Ensures the plugin system is initialized
         * @return {boolean} True if initialized successfully
         */
        ensureInitialized: function() {
            if (!JSPluginState.initialized) {
                this.tryInitialize();
                if (!JSPluginState.initialized) {
                    this.logError("Not initialized. Call InitializeJSTools first");
                    return false;
                }
            }
            return true;
        },
        
        /**
         * Sends a message to a Unity GameObject
         * @param {string} objectName - The name of the GameObject
         * @param {string} methodName - The method to call on the GameObject
         * @param {string} parameter - Optional parameter to pass
         * @return {boolean} True if the message was sent successfully
         */
        sendUnityMessage: function(objectName, methodName, parameter) {
            if (!this.ensureInitialized()) return false;
            
            try {
                if (parameter !== undefined && parameter !== null) {
                    JSPluginState.unityInstance.SendMessage(objectName, methodName, parameter);
                } else {
                    JSPluginState.unityInstance.SendMessage(objectName, methodName);
                }
                return true;
            } catch (error) {
                this.logError("Error sending message to " + objectName + "." + methodName + ": " + error);
                return false;
            }
        },
        
        /**
         * Allocates a string in the Emscripten heap and returns a pointer to it
         * @param {string} str - The string to allocate
         * @param {number} [timeout=100] - Optional timeout in ms before automatic deallocation
         * @return {number} Pointer to the allocated string
         */
        allocateString: function(str, timeout) {
            if (str === null || str === undefined) {
                str = "";
            }
            
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(str, buffer, bufferSize);
            
            // Store in our memory manager
            var id = JSPluginState.memoryManager.nextAllocationId++;
            JSPluginState.memoryManager.allocatedStrings[id] = {
                pointer: buffer,
                size: bufferSize,
                timestamp: Date.now()
            };
            
            // Update memory stats
            JSPluginState.memoryManager.stats.currentAllocationCount++;
            JSPluginState.memoryManager.stats.totalBytesAllocated += bufferSize;
            JSPluginState.memoryManager.totalAllocations++;
            
            if (JSPluginState.memoryManager.stats.currentAllocationCount > 
                JSPluginState.memoryManager.stats.peakAllocationCount) {
                JSPluginState.memoryManager.stats.peakAllocationCount = 
                    JSPluginState.memoryManager.stats.currentAllocationCount;
            }
            
            // Schedule cleanup if requested
            if (timeout !== undefined && timeout > 0) {
                setTimeout(function() {
                    this.freeString(id);
                }.bind(this), timeout);
            }
            
            return buffer;
        },
        
        /**
         * Frees a previously allocated string
         * @param {number} id - The allocation ID to free
         * @return {boolean} True if the string was freed, false if not found
         */
        freeString: function(id) {
            var allocation = JSPluginState.memoryManager.allocatedStrings[id];
            if (!allocation) return false;
            
            _free(allocation.pointer);
            delete JSPluginState.memoryManager.allocatedStrings[id];
            
            // Update memory stats
            JSPluginState.memoryManager.stats.currentAllocationCount--;
            JSPluginState.memoryManager.totalDeallocations++;
            
            return true;
        },
        
        /**
         * Logs memory statistics
         */
        logMemoryStats: function() {
            if (!JSPluginState.debugEnabled) return;
            
            var stats = JSPluginState.memoryManager.stats;
            this.log("Memory Stats: Current=" + stats.currentAllocationCount + 
                     ", Peak=" + stats.peakAllocationCount +
                     ", Total Bytes=" + stats.totalBytesAllocated +
                     ", Allocations=" + JSPluginState.memoryManager.totalAllocations +
                     ", Deallocations=" + JSPluginState.memoryManager.totalDeallocations);
        },
        
        /**
         * Runs periodic memory cleanup
         */
        runMemoryCleanup: function() {
            // Check for old allocations that might have been missed
            var now = Date.now();
            var threshold = 5000; // 5 seconds
            
            for (var id in JSPluginState.memoryManager.allocatedStrings) {
                var allocation = JSPluginState.memoryManager.allocatedStrings[id];
                if (now - allocation.timestamp > threshold) {
                    this.freeString(id);
                    if (JSPluginState.debugEnabled) {
                        this.log("Auto-freed stale memory allocation: " + id);
                    }
                }
            }
            
            // Log memory stats in debug mode
            if (JSPluginState.debugEnabled) {
                this.logMemoryStats();
            }
        }
    },
    
    /**
     * Initializes the JS Tools system
     * @param {boolean} debugMode - Whether to enable debug logging
     */
    JSPluginInitializeTools: function(debugMode) {
        if (JSPluginState.initialized) return;
        
        JSPluginState.debugEnabled = !!debugMode;
        JSPluginHelper.log("JS Tools initializing with debug mode: " + JSPluginState.debugEnabled);
        
        try {
            // Find Unity instance
            if (typeof window !== 'undefined') {
                JSPluginState.unityInstance = window.unityInstance || 
                                          window.gameInstance || 
                                          (window.Module ? window.Module.unityInstance : null);
            }
            
            // Set up periodic memory cleanup
            if (typeof window !== 'undefined') {
                window.setInterval(function() {
                    JSPluginHelper.runMemoryCleanup();
                }, 30000); // Run every 30 seconds
            }
            
            // Initialize logging module if available
            if (typeof JSPluginLogging !== 'undefined' && 
                typeof JSPluginLogging.JSPluginInitializeLogging === 'function') {
                JSPluginLogging.JSPluginInitializeLogging();
                JSPluginHelper.logDebug("Logging module initialized");
            }
            
            // Initialize storage module if available
            if (typeof JSPluginStorage !== 'undefined' && 
                typeof JSPluginStorage.JSPluginInitializeStorage === 'function') {
                JSPluginStorage.JSPluginInitializeStorage();
                JSPluginHelper.logDebug("Storage module initialized");
            }
            
            JSPluginState.initialized = true;
            JSPluginHelper.log("JS Tools initialized successfully");
        } catch (error) {
            JSPluginHelper.logError("Error during initialization: " + error);
        }
    },
    
    /**
     * Exports plugin functionality to the global window object
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginExportToWindow: function() {
        if (typeof window !== 'undefined') {
            try {
                window.UnityJSTools = {
                    /**
                     * Registers a Unity GameObject for JavaScript interaction
                     * @param {string} id - Unique identifier for the object
                     * @param {string} objectName - Name of the Unity GameObject
                     */
                    registerObject: function(id, objectName) {
                        JSPluginState.objects[id] = objectName;
                        JSPluginHelper.logDebug("Registered Unity object '" + objectName + "' with ID '" + id + "'");
                    },
                    
                    /**
                     * Sends a message to a registered Unity GameObject
                     * @param {string} objectId - The object identifier
                     * @param {string} methodName - The method name to call
                     * @param {string} parameter - Optional parameter to pass
                     * @return {boolean} True if successful
                     */
                    sendMessage: function(objectId, methodName, parameter) {
                        var objectName = JSPluginState.objects[objectId];
                        if (!objectName) {
                            JSPluginHelper.logError("Object with ID '" + objectId + "' not registered");
                            return false;
                        }
                        
                        return JSPluginHelper.sendUnityMessage(objectName, methodName, parameter);
                    },
                    
                    /**
                     * Registers a callback function for a specific object and type
                     * @param {string} objectId - The object identifier
                     * @param {string} callbackType - The type of callback
                     * @param {string} methodName - The method name on the GameObject
                     */
                    registerCallback: function(objectId, callbackType, methodName) {
                        if (!JSPluginState.callbacks[objectId]) {
                            JSPluginState.callbacks[objectId] = {};
                        }
                        
                        JSPluginState.callbacks[objectId][callbackType] = methodName;
                        JSPluginHelper.logDebug("Registered callback '" + callbackType + "' for object '" + objectId + "' with method '" + methodName + "'");
                    },
                    
                    /**
                     * Registers an error handler for a specific object
                     * @param {string} objectId - The object identifier
                     * @param {string} methodName - The method name on the GameObject
                     */
                    registerErrorHandler: function(objectId, methodName) {
                        JSPluginState.errorHandlers[objectId] = methodName;
                        JSPluginHelper.logDebug("Registered error handler for object '" + objectId + "' with method '" + methodName + "'");
                    },
                    
                    /**
                     * Triggers a callback on a registered Unity GameObject
                     * @param {string} objectId - The object identifier
                     * @param {string} callbackType - The type of callback
                     * @param {string} parameter - Optional parameter to pass
                     * @return {boolean} True if successful
                     */
                    triggerCallback: function(objectId, callbackType, parameter) {
                        if (!JSPluginState.callbacks[objectId] || !JSPluginState.callbacks[objectId][callbackType]) {
                            JSPluginHelper.logError("No callback registered for '" + callbackType + "' on object '" + objectId + "'");
                            return false;
                        }
                        
                        var methodName = JSPluginState.callbacks[objectId][callbackType];
                        var objectName = JSPluginState.objects[objectId];
                        
                        return JSPluginHelper.sendUnityMessage(objectName, methodName, parameter);
                    },
                    
                    /**
                     * Reports an error to a registered error handler
                     * @param {string} objectId - The object identifier
                     * @param {string} context - Context where the error occurred
                     * @param {string|Object} error - The error object or message
                     * @return {boolean} True if successful
                     */
                    reportError: function(objectId, context, error) {
                        JSPluginHelper.logError(context + ": " + error);
                        
                        if (!JSPluginState.errorHandlers[objectId]) {
                            JSPluginHelper.logError("No error handler registered for object '" + objectId + "'");
                            return false;
                        }
                        
                        var errorMessage = error;
                        if (typeof error === 'object') {
                            try {
                                errorMessage = JSON.stringify(error);
                            } catch (e) {
                                errorMessage = error.toString();
                            }
                        }
                        
                        var methodName = JSPluginState.errorHandlers[objectId];
                        var objectName = JSPluginState.objects[objectId];
                        
                        return JSPluginHelper.sendUnityMessage(objectName, methodName, errorMessage);
                    },
                    
                    /**
                     * Registers an event handler for Unity lifecycle events
                     * @param {string} objectId - The object identifier
                     * @param {string} eventName - Name of the event (Awake, Start, etc)
                     * @param {Function} callback - The JavaScript function to call
                     * @return {Function} Function to unregister the handler
                     */
                    on: function(objectId, eventName, callback) {
                        var handlerId = objectId + "_" + eventName;
                        JSPluginState.eventHandlers[handlerId] = callback;
                        
                        // Регистрируем колбэк на стороне Unity
                        this.registerCallback(objectId, "event_" + eventName, "HandleEvent");
                        JSPluginHelper.logDebug("Registered event handler for '" + eventName + "' on object '" + objectId + "'");
                        
                        return function() {
                            // Функция для удаления обработчика
                            delete JSPluginState.eventHandlers[handlerId];
                            JSPluginHelper.logDebug("Removed event handler for '" + eventName + "' on object '" + objectId + "'");
                        };
                    },
                    
                    /**
                     * Enables or disables event tracing
                     * @param {boolean} enabled - Whether to enable tracing
                     */
                    traceEvents: function(enabled) {
                        JSPluginState.traceEvents = !!enabled;
                    },
                    
                    /**
                     * Manually triggers an event
                     * @param {string} objectId - The object identifier
                     * @param {string} eventName - Name of the event
                     * @param {*} data - Optional data to pass to the event handler
                     * @return {boolean} True if successful
                     */
                    triggerEvent: function(objectId, eventName, data) {
                        var handlerId = objectId + "_" + eventName;
                        if (JSPluginState.eventHandlers[handlerId]) {
                            if (JSPluginState.traceEvents) {
                                JSPluginHelper.log("Event '" + eventName + "' triggered on '" + objectId + "'");
                            }
                            
                            try {
                                JSPluginState.eventHandlers[handlerId](data);
                                return true;
                            } catch (error) {
                                JSPluginHelper.logError("Error in event handler for '" + eventName + "': " + error);
                                return false;
                            }
                        }
                        return false;
                    }
                };
                
                // Other modules will extend this object
                
                JSPluginHelper.log("Successfully exported to window object");
                return 1;
            } catch (error) {
                JSPluginHelper.logError("Error exporting to window: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Registers a Unity instance for communication
     * @param {number} instance - Pointer to Unity instance
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginRegisterUnityInstance: function(instance) {
        return JSPluginHelper.tryInitialize(instance) ? 1 : 0;
    },
    
    /**
     * Registers a Unity GameObject for JavaScript interaction
     * @param {string} objectId - Pointer to object ID string
     * @param {string} objectName - Pointer to GameObject name string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginRegisterUnityObject: function(objectId, objectName) {
        try {
            var id = UTF8ToString(objectId);
            var name = UTF8ToString(objectName);
            
            JSPluginState.objects[id] = name;
            JSPluginHelper.logDebug("Registered Unity object '" + name + "' with ID '" + id + "'");
            return 1;
        } catch (error) {
            JSPluginHelper.logError("Error in RegisterUnityObject: " + error);
            return 0;
        }
    },
    
    /**
     * Registers a callback for a specific object and type
     * @param {string} objectId - Pointer to object ID string
     * @param {string} callbackType - Pointer to callback type string
     * @param {string} methodName - Pointer to method name string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginRegisterCallback: function(objectId, callbackType, methodName) {
        try {
            var id = UTF8ToString(objectId);
            var type = UTF8ToString(callbackType);
            var method = UTF8ToString(methodName);
            
            if (!JSPluginState.callbacks[id]) {
                JSPluginState.callbacks[id] = {};
            }
            
            JSPluginState.callbacks[id][type] = method;
            JSPluginHelper.logDebug("Registered callback '" + type + "' for object '" + id + "' with method '" + method + "'");
            return 1;
        } catch (error) {
            JSPluginHelper.logError("Error in RegisterCallback: " + error);
            return 0;
        }
    },
    
    /**
     * Registers an error handler for a specific object
     * @param {string} objectId - Pointer to object ID string
     * @param {string} methodName - Pointer to method name string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginRegisterErrorHandler: function(objectId, methodName) {
        try {
            var id = UTF8ToString(objectId);
            var method = UTF8ToString(methodName);
            
            JSPluginState.errorHandlers[id] = method;
            JSPluginHelper.logDebug("Registered error handler for object '" + id + "' with method '" + method + "'");
            return 1;
        } catch (error) {
            JSPluginHelper.logError("Error in RegisterErrorHandler: " + error);
            return 0;
        }
    },
    
    /**
     * Sends a message from Unity to JavaScript
     * @param {string} objectId - Pointer to object ID string
     * @param {string} methodName - Pointer to method name string
     * @param {string} parameter - Pointer to parameter string or null
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginSendMessage: function(objectId, methodName, parameter) {
        try {
            var id = UTF8ToString(objectId);
            var method = UTF8ToString(methodName);
            var param = parameter ? UTF8ToString(parameter) : null;
            
            var objectName = JSPluginState.objects[id];
            if (!objectName) {
                JSPluginHelper.logError("Object with ID '" + id + "' not registered");
                return 0;
            }
            
            return JSPluginHelper.sendUnityMessage(objectName, method, param) ? 1 : 0;
        } catch (error) {
            JSPluginHelper.logError("Error in SendMessage: " + error);
            return 0;
        }
    },
    
    /**
     * Triggers a callback from Unity to JavaScript
     * @param {string} objectId - Pointer to object ID string
     * @param {string} callbackType - Pointer to callback type string
     * @param {string} parameter - Pointer to parameter string or null
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginTriggerCallback: function(objectId, callbackType, parameter) {
        try {
            var id = UTF8ToString(objectId);
            var type = UTF8ToString(callbackType);
            var param = parameter ? UTF8ToString(parameter) : null;
            
            if (!JSPluginState.callbacks[id] || !JSPluginState.callbacks[id][type]) {
                JSPluginHelper.logError("No callback registered for '" + type + "' on object '" + id + "'");
                return 0;
            }
            
            var methodName = JSPluginState.callbacks[id][type];
            var objectName = JSPluginState.objects[id];
            
            return JSPluginHelper.sendUnityMessage(objectName, methodName, param) ? 1 : 0;
        } catch (error) {
            JSPluginHelper.logError("Error in TriggerCallback: " + error);
            return 0;
        }
    },
    
    /**
     * Reports an error from Unity to JavaScript
     * @param {string} objectId - Pointer to object ID string
     * @param {string} context - Pointer to context string
     * @param {string} error - Pointer to error message string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginReportError: function(objectId, context, error) {
        try {
            var id = UTF8ToString(objectId);
            var ctx = UTF8ToString(context);
            var err = UTF8ToString(error);
            
            JSPluginHelper.logError(ctx + ": " + err);
            
            if (!JSPluginState.errorHandlers[id]) {
                JSPluginHelper.logError("No error handler registered for object '" + id + "'");
                return 0;
            }
            
            var methodName = JSPluginState.errorHandlers[id];
            var objectName = JSPluginState.objects[id];
            
            return JSPluginHelper.sendUnityMessage(objectName, methodName, err) ? 1 : 0;
        } catch (error) {
            JSPluginHelper.logError("Error in ReportError: " + error);
            return 0;
        }
    }
};

// Register internal variables
autoAddDeps(JSPluginCore, '$JSPluginState');
autoAddDeps(JSPluginCore, '$JSPluginHelper');
mergeInto(LibraryManager.library, JSPluginCore);