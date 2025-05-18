/**
 * Unity Web Plugin System - Core JavaScript Bridge
 * This file provides core functionality for Unity-JavaScript communication
 */

var CorePluginLib = {
    // Private state (not directly accessible from Unity)
    // These variables will be available only within the plugin scope
    $CorePlugin: {
        // Core module properties
        modules: {},
        callbacks: {},
        nextCallbackId: 1,
        unityInstance: null,
        isInitialized: false,
        
        /**
         * Initializes the bridge between Unity and JavaScript
         */
        initialize: function() {
            if (this.isInitialized) {
                console.warn('[UnityWebPlugin.Core] Already initialized');
                return;
            }
            
            // Find the Unity instance
            this.findUnityInstance();
            
            console.log('[UnityWebPlugin.Core] Initialized');
            this.isInitialized = true;
            
            // Dispatch initialization event
            this.dispatchEvent('initialized');
        },
        
        /**
         * Attempts to find the Unity instance in the page
         */
        findUnityInstance: function() {
            // Look for Unity canvas
            var canvas = document.querySelector('canvas[data-unity-build]') || 
                         document.querySelector('canvas#unity-canvas') || 
                         document.querySelector('canvas[style*="background: #"]');
            
            if (canvas) {
                // Find the instance attached to the canvas or in the global gameInstance
                this.unityInstance = canvas.unityInstance || 
                                     window.unityInstance || 
                                     window.gameInstance;
                
                if (this.unityInstance) {
                    console.log('[UnityWebPlugin.Core] Unity instance found');
                } else {
                    console.warn('[UnityWebPlugin.Core] Unity canvas found but instance not detected');
                    
                    // Try to find any Unity instance in the global scope
                    for (var key in window) {
                        if (window[key] && typeof window[key].SendMessage === 'function') {
                            this.unityInstance = window[key];
                            console.log('[UnityWebPlugin.Core] Unity instance found in global scope');
                            break;
                        }
                    }
                }
            } else {
                console.warn('[UnityWebPlugin.Core] Unity canvas not found');
                
                // Try to find any Unity instance in the global scope
                for (var key in window) {
                    if (window[key] && typeof window[key].SendMessage === 'function') {
                        this.unityInstance = window[key];
                        console.log('[UnityWebPlugin.Core] Unity instance found in global scope');
                        break;
                    }
                }
            }
            
            if (!this.unityInstance) {
                console.error('[UnityWebPlugin.Core] Unable to find Unity instance. Communication with Unity will not work.');
            }
        },
        
        /**
         * Sends a message to Unity
         * @param {string} gameObjectName - The name of the GameObject to send the message to
         * @param {string} methodName - The name of the method to call
         * @param {string} message - The message to send
         */
        sendMessageToUnity: function(gameObjectName, methodName, message) {
            if (!this.isInitialized) {
                console.error('[UnityWebPlugin.Core] Not initialized');
                return;
            }
            
            if (!this.unityInstance) {
                console.error('[UnityWebPlugin.Core] Unity instance not found');
                return;
            }
            
            try {
                this.unityInstance.SendMessage(gameObjectName, methodName, message);
            } catch (error) {
                console.error('[UnityWebPlugin.Core] Error sending message to Unity:', error);
            }
        },
        
        /**
         * Receives a message from Unity
         * @param {string} jsonMessage - The JSON message from Unity
         */
        receiveMessageFromUnity: function(jsonMessage) {
            if (!jsonMessage) {
                console.error('[UnityWebPlugin.Core] Received empty message from Unity');
                return;
            }
            
            try {
                var message = JSON.parse(jsonMessage);
                
                if (!message.moduleId) {
                    console.error('[UnityWebPlugin.Core] Received message with no moduleId');
                    return;
                }
                
                if (!message.action) {
                    console.error('[UnityWebPlugin.Core] Received message with no action');
                    return;
                }
                
                var module = this.modules[message.moduleId];
                
                if (!module) {
                    console.warn('[UnityWebPlugin.Core] Message received for unknown module:', message.moduleId);
                    return;
                }
                
                if (typeof module.handleUnityMessage === 'function') {
                    module.handleUnityMessage(message.action, message.data, message.callbackId);
                } else {
                    console.warn('[UnityWebPlugin.Core] Module does not implement handleUnityMessage:', message.moduleId);
                }
            } catch (error) {
                console.error('[UnityWebPlugin.Core] Error processing message from Unity:', error);
            }
        },
        
        /**
         * Registers a JavaScript module
         * @param {string} moduleId - The unique identifier for the module
         * @param {object} module - The module object
         */
        registerModule: function(moduleId, module) {
            if (!moduleId) {
                console.error('[UnityWebPlugin.Core] Module ID cannot be null or empty');
                return false;
            }
            
            if (!module) {
                console.error('[UnityWebPlugin.Core] Cannot register null module');
                return false;
            }
            
            if (this.modules[moduleId]) {
                console.error('[UnityWebPlugin.Core] Module with ID already registered:', moduleId);
                return false;
            }
            
            this.modules[moduleId] = module;
            console.log('[UnityWebPlugin.Core] Module registered:', moduleId);
            
            return true;
        },
        
        /**
         * Gets a registered module by its ID
         * @param {string} moduleId - The ID of the module to retrieve
         * @returns {object|null} The module if found, null otherwise
         */
        getModule: function(moduleId) {
            return this.modules[moduleId] || null;
        },
        
        /**
         * Sends a message to a Unity module
         * @param {string} moduleId - The ID of the target module
         * @param {string} action - The action to perform
         * @param {object|string} data - Optional data to send
         * @param {function} callback - Optional callback function
         */
        sendMessageToUnityModule: function(moduleId, action, data, callback) {
            if (!this.isInitialized) {
                console.error('[UnityWebPlugin.Core] Not initialized');
                if (callback) callback({ success: false, errorMessage: 'Not initialized' });
                return;
            }
            
            if (!moduleId) {
                console.error('[UnityWebPlugin.Core] Module ID cannot be null or empty');
                if (callback) callback({ success: false, errorMessage: 'Module ID cannot be null or empty' });
                return;
            }
            
            if (!action) {
                console.error('[UnityWebPlugin.Core] Action cannot be null or empty');
                if (callback) callback({ success: false, errorMessage: 'Action cannot be null or empty' });
                return;
            }
            
            var callbackId = null;
            
            if (callback && typeof callback === 'function') {
                callbackId = 'cb_' + this.nextCallbackId++;
                this.callbacks[callbackId] = callback;
            }
            
            var message = {
                moduleId: moduleId,
                action: action,
                data: (typeof data === 'object') ? JSON.stringify(data) : data,
                callbackId: callbackId
            };
            
            this.sendMessageToUnity('[JSPluginTools]', 'OnMessageFromJS', JSON.stringify(message));
        },
        
        /**
         * Invokes a callback function
         * @param {string} callbackId - The ID of the callback to invoke
         * @param {string} responseJson - The response data as JSON
         */
        invokeCallback: function(callbackId, responseJson) {
            if (!callbackId || !this.callbacks[callbackId]) {
                console.warn('[UnityWebPlugin.Core] Callback not found:', callbackId);
                return;
            }
            
            try {
                var response = JSON.parse(responseJson);
                this.callbacks[callbackId](response);
            } catch (error) {
                console.error('[UnityWebPlugin.Core] Error invoking callback:', error);
                this.callbacks[callbackId]({ success: false, errorMessage: 'Error invoking callback: ' + error.message });
            } finally {
                delete this.callbacks[callbackId];
            }
        },
        
        /**
         * Notifies that a Unity module has been initialized
         * @param {string} moduleId - The ID of the initialized module
         */
        notifyModuleInitialized: function(moduleId) {
            console.log('[UnityWebPlugin.Core] Unity module initialized:', moduleId);
            this.dispatchEvent('moduleInitialized', { moduleId: moduleId });
        },
        
        /**
         * Notifies that a Unity module has been shut down
         * @param {string} moduleId - The ID of the shut down module
         */
        notifyModuleShutdown: function(moduleId) {
            console.log('[UnityWebPlugin.Core] Unity module shut down:', moduleId);
            this.dispatchEvent('moduleShutdown', { moduleId: moduleId });
        },
        
        /**
         * Dispatches a custom event
         * @param {string} eventName - The name of the event to dispatch
         * @param {object} data - Optional data to include with the event
         */
        dispatchEvent: function(eventName, data) {
            var event;
            
            if (typeof CustomEvent === 'function') {
                event = new CustomEvent('UnityWebPlugin.' + eventName, {
                    detail: data || {},
                    bubbles: true,
                    cancelable: true
                });
            } else {
                // For IE compatibility
                event = document.createEvent('CustomEvent');
                event.initCustomEvent('UnityWebPlugin.' + eventName, true, true, data || {});
            }
            
            document.dispatchEvent(event);
        }
    },
    
    // Public API accessible from Unity
    
    /**
     * Initialize the Core Plugin
     */
    InitializePlugin: function() {
        // Create global namespace if it doesn't exist
        if (!window.UnityWebPlugin) {
            window.UnityWebPlugin = {};
        }
        
        // Expose the core API to JavaScript
        window.UnityWebPlugin.Core = {
            initialize: CorePlugin.initialize.bind(CorePlugin),
            registerModule: CorePlugin.registerModule.bind(CorePlugin),
            getModule: CorePlugin.getModule.bind(CorePlugin),
            sendMessageToUnity: CorePlugin.sendMessageToUnity.bind(CorePlugin),
            receiveMessageFromUnity: CorePlugin.receiveMessageFromUnity.bind(CorePlugin),
            sendMessageToUnityModule: CorePlugin.sendMessageToUnityModule.bind(CorePlugin),
            invokeCallback: CorePlugin.invokeCallback.bind(CorePlugin),
            notifyModuleInitialized: CorePlugin.notifyModuleInitialized.bind(CorePlugin),
            notifyModuleShutdown: CorePlugin.notifyModuleShutdown.bind(CorePlugin)
        };
        
        // Initialize the core
        CorePlugin.initialize();
    },
    
    /**
     * Call a JavaScript function from Unity
     * @param {string} functionPathPtr - Pointer to the function path string
     * @param {string} argsJsonPtr - Pointer to the JSON arguments string
     */
    CallJS: function(functionPathPtr, argsJsonPtr) {
        var functionPath = UTF8ToString(functionPathPtr);
        var argsJson = UTF8ToString(argsJsonPtr);
        
        try {
            // Parse the arguments
            var args = JSON.parse(argsJson);
            
            // Find the function to call
            var namespaces = functionPath.split('.');
            var context = window;
            
            for (var i = 0; i < namespaces.length - 1; i++) {
                context = context[namespaces[i]];
                if (!context) {
                    console.error('[UnityJSPluginTools] Function path not found: ' + functionPath);
                    return;
                }
            }
            
            var func = context[namespaces[namespaces.length - 1]];
            
            if (typeof func !== 'function') {
                console.error('[UnityJSPluginTools] Not a function: ' + functionPath);
                return;
            }
            
            // Call the function with the arguments
            func.apply(context, args);
        } catch (error) {
            console.error('[UnityJSPluginTools] Error calling JavaScript function: ' + error.message);
        }
    },
    
    /**
     * Receive message from Unity and route it to the appropriate module
     * @param {string} jsonMessagePtr - Pointer to the JSON message string
     */
    ReceiveMessageFromUnity: function(jsonMessagePtr) {
        var jsonMessage = UTF8ToString(jsonMessagePtr);
        CorePlugin.receiveMessageFromUnity(jsonMessage);
    }
};

// Use the $CorePlugin notation to indicate it should be included in the global scope
autoAddDeps(CorePluginLib, '$CorePlugin');
mergeInto(LibraryManager.library, CorePluginLib);
