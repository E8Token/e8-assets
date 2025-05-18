/**
 * Unity Web Plugin System - Core JavaScript
 * Version: 1.0.0
 * 
 * This file provides the browser-side functionality for the Unity WebGL plugin system.
 * It should be included in the final WebGL build to enable JS ↔ Unity communication.
 */
(function(global) {
    'use strict';
    
    // Ensure the namespace exists
    global.UnityWebPlugin = global.UnityWebPlugin || {};
    
    /**
     * Core module that provides the foundation for the plugin system
     */
    var Core = {
        /**
         * Stores registered modules
         * @private
         */
        _modules: {},
        
        /**
         * Stores callback functions
         * @private
         */
        _callbacks: {},
        
        /**
         * Next callback ID
         * @private
         */
        _nextCallbackId: 1,
        
        /**
         * The Unity instance (assigned when Unity calls initialize)
         * @private
         */
        _unityInstance: null,
        
        /**
         * Whether the bridge is initialized
         * @private
         */
        _isInitialized: false,
        
        /**
         * Initializes the bridge between Unity and JavaScript
         * This function is called by Unity when the plugin is initialized
         */
        initialize: function() {
            if (this._isInitialized) {
                console.warn('[UnityWebPlugin.Core] Already initialized');
                return;
            }
            
            // Find the Unity instance
            this._findUnityInstance();
            
            console.log('[UnityWebPlugin.Core] Initialized');
            this._isInitialized = true;
            
            // Dispatch initialization event
            this._dispatchEvent('initialized');
        },
        
        /**
         * Attempts to find the Unity instance in the page
         * @private
         */
        _findUnityInstance: function() {
            // Look for Unity canvas
            var canvas = document.querySelector('canvas[data-unity-build]') || 
                         document.querySelector('canvas#unity-canvas') || 
                         document.querySelector('canvas[style*="background: #"]');
            
            if (canvas) {
                // Find the instance attached to the canvas or in the global gameInstance
                this._unityInstance = canvas.unityInstance || 
                                      global.unityInstance || 
                                      global.gameInstance;
                
                if (this._unityInstance) {
                    console.log('[UnityWebPlugin.Core] Unity instance found');
                } else {
                    console.warn('[UnityWebPlugin.Core] Unity canvas found but instance not detected');
                    
                    // Try to find any Unity instance in the global scope
                    for (var key in global) {
                        if (global[key] && typeof global[key].SendMessage === 'function') {
                            this._unityInstance = global[key];
                            console.log('[UnityWebPlugin.Core] Unity instance found in global scope');
                            break;
                        }
                    }
                }
            } else {
                console.warn('[UnityWebPlugin.Core] Unity canvas not found');
                
                // Try to find any Unity instance in the global scope
                for (var key in global) {
                    if (global[key] && typeof global[key].SendMessage === 'function') {
                        this._unityInstance = global[key];
                        console.log('[UnityWebPlugin.Core] Unity instance found in global scope');
                        break;
                    }
                }
            }
            
            if (!this._unityInstance) {
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
            if (!this._isInitialized) {
                console.error('[UnityWebPlugin.Core] Not initialized');
                return;
            }
            
            if (!this._unityInstance) {
                console.error('[UnityWebPlugin.Core] Unity instance not found');
                return;
            }
            
            try {
                this._unityInstance.SendMessage(gameObjectName, methodName, message);
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
                
                var module = this._modules[message.moduleId];
                
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
         * @returns {boolean} True if registration was successful, false otherwise
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
            
            if (this._modules[moduleId]) {
                console.error('[UnityWebPlugin.Core] Module with ID already registered:', moduleId);
                return false;
            }
            
            this._modules[moduleId] = module;
            console.log('[UnityWebPlugin.Core] Module registered:', moduleId);
            
            return true;
        },
        
        /**
         * Gets a registered module by its ID
         * @param {string} moduleId - The ID of the module to retrieve
         * @returns {object|null} The module if found, null otherwise
         */
        getModule: function(moduleId) {
            return this._modules[moduleId] || null;
        },
        
        /**
         * Sends a message to a Unity module
         * @param {string} moduleId - The ID of the target module
         * @param {string} action - The action to perform
         * @param {object|string} [data=null] - Optional data to send
         * @param {function} [callback=null] - Optional callback function
         */
        sendMessageToUnityModule: function(moduleId, action, data, callback) {
            if (!this._isInitialized) {
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
                callbackId = 'cb_' + this._nextCallbackId++;
                this._callbacks[callbackId] = callback;
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
            if (!callbackId || !this._callbacks[callbackId]) {
                console.warn('[UnityWebPlugin.Core] Callback not found:', callbackId);
                return;
            }
            
            try {
                var response = JSON.parse(responseJson);
                this._callbacks[callbackId](response);
            } catch (error) {
                console.error('[UnityWebPlugin.Core] Error invoking callback:', error);
                this._callbacks[callbackId]({ success: false, errorMessage: 'Error invoking callback: ' + error.message });
            } finally {
                delete this._callbacks[callbackId];
            }
        },
        
        /**
         * Notifies that a Unity module has been initialized
         * @param {string} moduleId - The ID of the initialized module
         */
        notifyModuleInitialized: function(moduleId) {
            console.log('[UnityWebPlugin.Core] Unity module initialized:', moduleId);
            this._dispatchEvent('moduleInitialized', { moduleId: moduleId });
        },
        
        /**
         * Notifies that a Unity module has been shut down
         * @param {string} moduleId - The ID of the shut down module
         */
        notifyModuleShutdown: function(moduleId) {
            console.log('[UnityWebPlugin.Core] Unity module shut down:', moduleId);
            this._dispatchEvent('moduleShutdown', { moduleId: moduleId });
        },
        
        /**
         * Dispatches a custom event
         * @param {string} eventName - The name of the event to dispatch
         * @param {object} [data=null] - Optional data to include with the event
         * @private
         */
        _dispatchEvent: function(eventName, data) {
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
        },
        
        /**
         * Gets the version of the plugin system
         * @returns {string} The version string
         */
        getVersion: function() {
            return '1.0.0';
        }
    };
    
    // Export the Core module
    global.UnityWebPlugin.Core = Core;
    
    // Create a base module class for JavaScript modules
    global.UnityWebPlugin.BaseModule = {
        /**
         * Creates a new module instance
         * @param {string} moduleId - The unique identifier for the module
         * @returns {object} A new module instance
         */
        create: function(moduleId) {
            var module = {
                /**
                 * The module ID
                 */
                moduleId: moduleId,
                
                /**
                 * Handles a message from Unity
                 * @param {string} action - The action to perform
                 * @param {string} data - The data associated with the action
                 * @param {string} callbackId - Optional callback ID for responding
                 */
                handleUnityMessage: function(action, data, callbackId) {
                    console.log('[UnityWebPlugin.' + moduleId + '] Received message:', action);
                    
                    var handler = this._actionHandlers[action];
                    
                    if (handler && typeof handler === 'function') {
                        try {
                            var parsedData = data ? JSON.parse(data) : null;
                            var result = handler.call(this, parsedData);
                            
                            if (callbackId) {
                                this.sendCallbackResponse(callbackId, true, null, result);
                            }
                        } catch (error) {
                            console.error('[UnityWebPlugin.' + moduleId + '] Error handling action:', error);
                            
                            if (callbackId) {
                                this.sendCallbackResponse(callbackId, false, error.message);
                            }
                        }
                    } else {
                        console.warn('[UnityWebPlugin.' + moduleId + '] No handler for action:', action);
                        
                        if (callbackId) {
                            this.sendCallbackResponse(callbackId, false, 'No handler for action: ' + action);
                        }
                    }
                },
                
                /**
                 * Sends a message to Unity
                 * @param {string} action - The action to perform
                 * @param {object} data - The data to send
                 * @param {function} callback - Optional callback function
                 */
                sendMessageToUnity: function(action, data, callback) {
                    global.UnityWebPlugin.Core.sendMessageToUnityModule(moduleId, action, data, callback);
                },
                
                /**
                 * Sends a callback response to Unity
                 * @param {string} callbackId - The ID of the callback to respond to
                 * @param {boolean} success - Whether the operation was successful
                 * @param {string} errorMessage - Optional error message
                 * @param {object} data - Optional data to send
                 */
                sendCallbackResponse: function(callbackId, success, errorMessage, data) {
                    var response = {
                        success: !!success,
                        errorMessage: errorMessage || null,
                        data: data ? JSON.stringify(data) : null
                    };
                    
                    global.UnityWebPlugin.Core.invokeCallback(callbackId, JSON.stringify(response));
                },
                
                /**
                 * Action handlers map
                 * @private
                 */
                _actionHandlers: {},
                
                /**
                 * Registers a handler for a specific action
                 * @param {string} action - The action to handle
                 * @param {function} handler - The handler function
                 */
                registerActionHandler: function(action, handler) {
                    if (!action) {
                        console.error('[UnityWebPlugin.' + moduleId + '] Cannot register handler for empty action');
                        return;
                    }
                    
                    if (!handler || typeof handler !== 'function') {
                        console.error('[UnityWebPlugin.' + moduleId + '] Handler must be a function');
                        return;
                    }
                    
                    this._actionHandlers[action] = handler;
                }
            };
            
            return module;
        }
    };
    
})(typeof window !== 'undefined' ? window : self);
