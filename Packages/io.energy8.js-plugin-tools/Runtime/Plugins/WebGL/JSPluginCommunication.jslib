var JSPluginCommunication = {
    // Use the shared state objects
    $JSPluginState: null,
    $JSPluginHelper: null,
    
    /**
     * Communication-specific state
     * @private
     */
    $CommunicationState: {
        // Broadcast listeners registered from JavaScript
        broadcastListeners: {},
        
        // Message handlers registered by channels
        messageHandlers: {},
        
        // Connection status
        isConnected: false
    },
    
    /**
     * Helper functions for communication module
     * @private
     */
    $CommunicationHelper: {
        /**
         * Log an informational message
         * @param {string} message - The message to log
         */
        log: function(message) {
            console.log("[UnityJSTools/Communication] " + message);
        },
        
        /**
         * Log an error message
         * @param {string} message - The error message to log
         */
        logError: function(message) {
            console.error("[UnityJSTools/Communication] " + message);
        },
        
        /**
         * Dispatches a broadcast event to all registered listeners
         * @param {string} eventName - Name of the event to broadcast
         * @param {string} data - Data to pass to listeners
         */
        dispatchBroadcast: function(eventName, data) {
            if (CommunicationState.broadcastListeners[eventName]) {
                CommunicationState.broadcastListeners[eventName].forEach(function(listener) {
                    try {
                        listener(data);
                    } catch (e) {
                        this.logError("Error in broadcast listener: " + e);
                    }
                });
            }
        }
    },
    
    /**
     * Initializes the communication module and extends the UnityJSTools global object
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginInitializeCommunication: function() {
        if (typeof window !== 'undefined' && window.UnityJSTools) {
            try {
                // Add communication methods
                window.UnityJSTools.communication = {
                    /**
                     * Sends a message to Unity
                     * @param {string} channelId - The channel identifier
                     * @param {string} messageType - Type of message being sent
                     * @param {string|object} data - Data to send (will be stringified if an object)
                     * @return {boolean} True if successful
                     */
                    send: function(channelId, messageType, data) {
                        try {
                            var payload = (typeof data === 'object') ? JSON.stringify(data) : data;
                            var objectName = JSPluginState.objects[channelId];
                            
                            if (!objectName) {
                                CommunicationHelper.logError("Channel with ID '" + channelId + "' not registered");
                                return false;
                            }
                            
                            var handlerId = channelId + ":" + messageType;
                            var methodName = CommunicationState.messageHandlers[handlerId];
                            
                            if (!methodName) {
                                CommunicationHelper.logError("No handler registered for message type '" + messageType + "' on channel '" + channelId + "'");
                                return false;
                            }
                            
                            return JSPluginHelper.sendUnityMessage(objectName, methodName, payload);
                        } catch (e) {
                            CommunicationHelper.logError("Error sending message: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Subscribes to a broadcast event
                     * @param {string} eventName - Name of the event to listen for
                     * @param {Function} listener - Function to call when event is broadcast
                     * @return {Function} Function to call to unsubscribe
                     */
                    subscribe: function(eventName, listener) {
                        if (typeof listener !== 'function') {
                            CommunicationHelper.logError("Listener must be a function");
                            return function() {};
                        }
                        
                        if (!CommunicationState.broadcastListeners[eventName]) {
                            CommunicationState.broadcastListeners[eventName] = [];
                        }
                        
                        CommunicationState.broadcastListeners[eventName].push(listener);
                        
                        return function() {
                            var listeners = CommunicationState.broadcastListeners[eventName];
                            if (listeners) {
                                var index = listeners.indexOf(listener);
                                if (index !== -1) {
                                    listeners.splice(index, 1);
                                }
                                
                                if (listeners.length === 0) {
                                    delete CommunicationState.broadcastListeners[eventName];
                                }
                            }
                        };
                    },
                    
                    /**
                     * Creates a new channel for two-way communication
                     * @param {string} channelId - Unique identifier for the channel
                     * @return {Object} Channel object with methods for communication
                     */
                    createChannel: function(channelId) {
                        return {
                            id: channelId,
                            
                            /**
                             * Sends a message through this channel
                             * @param {string} messageType - Type of message to send
                             * @param {*} data - Data to send with the message
                             * @return {boolean} True if successful
                             */
                            send: function(messageType, data) {
                                return window.UnityJSTools.communication.send(channelId, messageType, data);
                            },
                            
                            /**
                             * Registers a handler for messages on this channel
                             * @param {string} messageType - Type of messages to handle
                             * @param {Function} handler - Function to call when messages arrive
                             * @return {Object} This channel for method chaining
                             */
                            onMessage: function(messageType, handler) {
                                // Store the handler and link it to a Unity method name
                                CommunicationState.messageHandlers[channelId + ":" + messageType] = handler;
                                return this;
                            }
                        };
                    },
                    
                    /**
                     * Sets the connection status and notifies Unity
                     * @param {boolean} connected - Whether the connection is established
                     */
                    setConnectionStatus: function(connected) {
                        CommunicationState.isConnected = !!connected;
                        
                        // Notify Unity of the connection status change
                        JSPluginHelper.sendUnityMessage("JSPluginCommunication", "SetConnectionStatus", 
                            CommunicationState.isConnected ? "1" : "0");
                    },
                    
                    /**
                     * Checks if the communication channel is connected
                     * @return {boolean} True if connected
                     */
                    isConnected: function() {
                        return CommunicationState.isConnected;
                    }
                };
                
                return 1;
            } catch (error) {
                console.error("[UnityJSTools] Error initializing communication: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Broadcasts a message from Unity to all JavaScript listeners
     * @param {string} eventName - Pointer to event name string
     * @param {string} data - Pointer to data string or null
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginBroadcastMessage: function(eventName, data) {
        try {
            var event = UTF8ToString(eventName);
            var payload = data ? UTF8ToString(data) : null;
            
            CommunicationHelper.dispatchBroadcast(event, payload);
            return 1;
        } catch (error) {
            CommunicationHelper.logError("Error in BroadcastMessage: " + error);
            return 0;
        }
    },
    
    /**
     * Registers a message handler for a specific channel and message type
     * @param {string} handlerId - Pointer to handler ID string (channelId:messageType)
     * @param {string} methodName - Pointer to Unity method name string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginRegisterMessageHandler: function(handlerId, methodName) {
        try {
            var id = UTF8ToString(handlerId);
            var method = UTF8ToString(methodName);
            
            CommunicationState.messageHandlers[id] = method;
            return 1;
        } catch (error) {
            CommunicationHelper.logError("Error in RegisterMessageHandler: " + error);
            return 0;
        }
    }
};

// Proper dependency registration
autoAddDeps(JSPluginCommunication, '$CommunicationState');
autoAddDeps(JSPluginCommunication, '$CommunicationHelper');
mergeInto(LibraryManager.library, JSPluginCommunication);
