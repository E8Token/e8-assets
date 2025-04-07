var JSPluginNetwork = {
    // Use the shared core module state objects
    $JSPluginState: null,
    $JSPluginHelper: null,
    
    /**
     * Network-specific state object
     * @private
     */
    $NetworkState: {
        // Active HTTP requests
        requests: {},
        
        // Active WebSocket connections
        webSockets: {},
        
        // Allocated strings
        allocatedStrings: {},
        
        // Counter for generating unique IDs
        nextId: 1
    },
    
    /**
     * Helper functions for network module
     * @private
     */
    $NetworkHelper: {
        /**
         * Log an error message
         * @param {string} message - Error message
         */
        logError: function(message) {
            console.error("[UnityJSTools/Network] " + message);
        },
        
        /**
         * Log a debug message
         * @param {string} message - Debug message
         */
        logDebug: function(message) {
            if (JSPluginState.debugEnabled) {
                console.log("[UnityJSTools/Network] " + message);
            }
        },
        
        /**
         * Allocates a string in the Emscripten heap and returns a pointer to it
         * @param {string} str - The string to allocate
         * @return {number} Pointer to the allocated string
         */
        allocateString: function(str) {
            if (str === null || str === undefined) {
                str = "";
            }
            
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(str, buffer, bufferSize);
            
            // Store in our cache to prevent memory leaks
            var id = NetworkState.nextId++;
            NetworkState.allocatedStrings[id] = buffer;
            
            // Schedule cleanup on next frame
            setTimeout(function() {
                if (NetworkState.allocatedStrings[id]) {
                    _free(NetworkState.allocatedStrings[id]);
                    delete NetworkState.allocatedStrings[id];
                }
            }, 0);
            
            return buffer;
        },
        
        /**
         * Sends a response back to Unity
         * @param {Object} response - Response data
         */
        sendResponseToUnity: function(response) {
            try {
                var objectName = JSPluginState.objects[response.objectId] || "JSPluginNetwork";
                var jsonData = JSON.stringify(response);
                JSPluginHelper.sendUnityMessage(objectName, response.callbackMethod, jsonData);
            } catch (error) {
                this.logError("Error sending response to Unity: " + error);
            }
        }
    },
    
    /**
     * Initializes the network module and extends the UnityJSTools global object
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginNetworkInitialize: function() {
        if (typeof window !== 'undefined' && window.UnityJSTools) {
            try {
                // Add network methods
                window.UnityJSTools.network = {
                    /**
                     * Sends an HTTP request
                     * @param {Object} config - Request configuration
                     * @return {string} Request ID
                     */
                    request: function(config) {
                        var requestId = config.requestId || ("req_" + NetworkState.nextId++);
                        
                        try {
                            // Create the XHR object
                            var xhr = new XMLHttpRequest();
                            
                            // Store the request
                            NetworkState.requests[requestId] = {
                                xhr: xhr,
                                config: config,
                                requestId: requestId,
                                timestamp: Date.now()
                            };
                            
                            // Setup timeout
                            var timeoutMs = (config.timeout || 30) * 1000;
                            xhr.timeout = timeoutMs;
                            
                            // Setup callbacks
                            xhr.onload = function() {
                                var request = NetworkState.requests[requestId];
                                if (!request) return;
                                
                                // Parse response headers
                                var headers = {};
                                var headerString = xhr.getAllResponseHeaders();
                                var headerPairs = headerString.split('\r\n');
                                
                                for (var i = 0; i < headerPairs.length; i++) {
                                    var headerPair = headerPairs[i];
                                    var index = headerPair.indexOf(': ');
                                    if (index > 0) {
                                        var key = headerPair.substring(0, index);
                                        var val = headerPair.substring(index + 2);
                                        headers[key] = val;
                                    }
                                }
                                
                                var response = {
                                    requestId: requestId,
                                    objectId: request.config.objectId,
                                    callbackMethod: request.config.callbackMethod,
                                    statusCode: xhr.status,
                                    text: xhr.responseText,
                                    headers: headers,
                                    isSuccess: xhr.status >= 200 && xhr.status < 300,
                                    error: xhr.status >= 400 ? "HTTP Error " + xhr.status : null,
                                    isTimeout: false,
                                    isComplete: true
                                };
                                
                                NetworkHelper.sendResponseToUnity(response);
                                delete NetworkState.requests[requestId];
                            };
                            
                            xhr.onerror = function() {
                                var request = NetworkState.requests[requestId];
                                if (!request) return;
                                
                                var response = {
                                    requestId: requestId,
                                    objectId: request.config.objectId,
                                    callbackMethod: request.config.callbackMethod,
                                    statusCode: 0,
                                    text: "",
                                    headers: {},
                                    isSuccess: false,
                                    error: "Network Error",
                                    isTimeout: false,
                                    isComplete: true
                                };
                                
                                NetworkHelper.sendResponseToUnity(response);
                                delete NetworkState.requests[requestId];
                            };
                            
                            xhr.ontimeout = function() {
                                var request = NetworkState.requests[requestId];
                                if (!request) return;
                                
                                var response = {
                                    requestId: requestId,
                                    objectId: request.config.objectId,
                                    callbackMethod: request.config.callbackMethod,
                                    statusCode: 0,
                                    text: "",
                                    headers: {},
                                    isSuccess: false,
                                    error: "Request Timeout",
                                    isTimeout: true,
                                    isComplete: true
                                };
                                
                                NetworkHelper.sendResponseToUnity(response);
                                delete NetworkState.requests[requestId];
                            };
                            
                            xhr.open(config.method || 'GET', config.url, true);
                            
                            // Set headers
                            if (config.headers) {
                                for (var header in config.headers) {
                                    if (config.headers.hasOwnProperty(header)) {
                                        xhr.setRequestHeader(header, config.headers[header]);
                                    }
                                }
                            }
                            
                            // Set credentials mode
                            xhr.withCredentials = !!config.withCredentials;
                            
                            // Set response type
                            if (config.responseType) {
                                xhr.responseType = config.responseType;
                            }
                            
                            // Send the request
                            xhr.send(config.body || null);
                            
                            return requestId;
                        } catch (error) {
                            NetworkHelper.logError("Error sending request: " + error);
                            
                            var response = {
                                requestId: requestId,
                                objectId: config.objectId,
                                callbackMethod: config.callbackMethod,
                                statusCode: 0,
                                text: "",
                                headers: {},
                                isSuccess: false,
                                error: "Error: " + error.message,
                                isTimeout: false,
                                isComplete: true
                            };
                            
                            NetworkHelper.sendResponseToUnity(response);
                            return requestId;
                        }
                    },
                    
                    /**
                     * Cancels an HTTP request
                     * @param {string} requestId - Request ID to cancel
                     * @return {boolean} True if request was found and cancelled
                     */
                    cancelRequest: function(requestId) {
                        var request = NetworkState.requests[requestId];
                        if (!request) return false;
                        
                        try {
                            request.xhr.abort();
                            delete NetworkState.requests[requestId];
                            return true;
                        } catch (error) {
                            NetworkHelper.logError("Error cancelling request: " + error);
                            return false;
                        }
                    },
                    
                    /**
                     * Creates a WebSocket connection
                     * @param {Object} config - WebSocket configuration
                     * @return {Object} WebSocket instance
                     */
                    createWebSocket: function(config) {
                        try {
                            var protocols = config.protocols ? config.protocols.split(',') : undefined;
                            var ws = protocols ? new WebSocket(config.url, protocols) : new WebSocket(config.url);
                            
                            var socketInfo = {
                                id: config.socketId,
                                socket: ws,
                                objectId: config.objectId,
                                messageMethod: config.messageMethod,
                                openMethod: config.openMethod,
                                closeMethod: config.closeMethod,
                                errorMethod: config.errorMethod
                            };
                            
                            NetworkState.webSockets[config.socketId] = socketInfo;
                            
                            ws.onopen = function(evt) {
                                try {
                                    var objectName = JSPluginState.objects[socketInfo.objectId] || "JSPluginNetwork";
                                    JSPluginHelper.sendUnityMessage(objectName, socketInfo.openMethod, socketInfo.id);
                                } catch (error) {
                                    NetworkHelper.logError("Error in WebSocket onopen: " + error);
                                }
                            };
                            
                            ws.onmessage = function(evt) {
                                try {
                                    var objectName = JSPluginState.objects[socketInfo.objectId] || "JSPluginNetwork";
                                    var messageData;
                                    
                                    // Handle different types of data
                                    if (typeof evt.data === 'string') {
                                        messageData = {
                                            SocketId: socketInfo.id,
                                            Message: evt.data,
                                            IsBinary: false
                                        };
                                    } else if (evt.data instanceof ArrayBuffer) {
                                        // Handle binary data
                                        var array = new Uint8Array(evt.data);
                                        var base64 = "";
                                        var len = array.byteLength;
                                        for (var i = 0; i < len; i++) {
                                            base64 += String.fromCharCode(array[i]);
                                        }
                                        
                                        messageData = {
                                            SocketId: socketInfo.id,
                                            Message: btoa(base64), // Base64 encode the binary data
                                            IsBinary: true
                                        };
                                    } else if (evt.data instanceof Blob) {
                                        // Handle Blob data - convert to base64
                                        var reader = new FileReader();
                                        reader.onload = function() {
                                            var arrayBuffer = reader.result;
                                            var array = new Uint8Array(arrayBuffer);
                                            var base64 = "";
                                            var len = array.byteLength;
                                            for (var i = 0; i < len; i++) {
                                                base64 += String.fromCharCode(array[i]);
                                            }
                                            
                                            var blobData = {
                                                SocketId: socketInfo.id,
                                                Message: btoa(base64), // Base64 encode the binary data
                                                IsBinary: true
                                            };
                                            
                                            var jsonBlobData = JSON.stringify(blobData);
                                            JSPluginHelper.sendUnityMessage(objectName, socketInfo.messageMethod, jsonBlobData);
                                        };
                                        reader.readAsArrayBuffer(evt.data);
                                        return; // Exit early as we'll send the message in the onload callback
                                    } else {
                                        // Fallback for other data types
                                        messageData = {
                                            SocketId: socketInfo.id,
                                            Message: String(evt.data),
                                            IsBinary: false
                                        };
                                    }
                                    
                                    var jsonData = JSON.stringify(messageData);
                                    JSPluginHelper.sendUnityMessage(objectName, socketInfo.messageMethod, jsonData);
                                } catch (error) {
                                    NetworkHelper.logError("Error in WebSocket onmessage: " + error);
                                }
                            };
                            
                            ws.onclose = function(evt) {
                                try {
                                    var objectName = JSPluginState.objects[socketInfo.objectId] || "JSPluginNetwork";
                                    var closeData = {
                                        SocketId: socketInfo.id,
                                        Code: evt.code,
                                        Reason: evt.reason || ""
                                    };
                                    
                                    var jsonData = JSON.stringify(closeData);
                                    JSPluginHelper.sendUnityMessage(objectName, socketInfo.closeMethod, jsonData);
                                    
                                    delete NetworkState.webSockets[socketInfo.id];
                                } catch (error) {
                                    NetworkHelper.logError("Error in WebSocket onclose: " + error);
                                }
                            };
                            
                            ws.onerror = function(evt) {
                                try {
                                    var objectName = JSPluginState.objects[socketInfo.objectId] || "JSPluginNetwork";
                                    var errorData = {
                                        SocketId: socketInfo.id,
                                        Error: "WebSocket error"
                                    };
                                    
                                    var jsonData = JSON.stringify(errorData);
                                    JSPluginHelper.sendUnityMessage(objectName, socketInfo.errorMethod, jsonData);
                                } catch (error) {
                                    NetworkHelper.logError("Error in WebSocket onerror: " + error);
                                }
                            };
                            
                            return {
                                id: socketInfo.id,
                                
                                send: function(message) {
                                    try {
                                        // Check if message is a base64 encoded binary message
                                        if (typeof message === 'string' && message.startsWith('__binary__:')) {
                                            // Extract the base64 string
                                            var base64 = message.substring(11);
                                            
                                            // Decode base64 to binary
                                            var binaryString = atob(base64);
                                            var len = binaryString.length;
                                            var bytes = new Uint8Array(len);
                                            
                                            // Convert to byte array
                                            for (var i = 0; i < len; i++) {
                                                bytes[i] = binaryString.charCodeAt(i);
                                            }
                                            
                                            // Send as binary data
                                            ws.send(bytes.buffer);
                                        } else {
                                            // Send as regular string
                                            ws.send(message);
                                        }
                                        return true;
                                    } catch (error) {
                                        NetworkHelper.logError("Error sending WebSocket message: " + error);
                                        return false;
                                    }
                                },
                                
                                close: function(code, reason) {
                                    try {
                                        ws.close(code || 1000, reason || "");
                                        return true;
                                    } catch (error) {
                                        NetworkHelper.logError("Error closing WebSocket: " + error);
                                        return false;
                                    }
                                }
                            };
                        } catch (error) {
                            NetworkHelper.logError("Error creating WebSocket: " + error);
                            
                            // Report error back to Unity
                            try {
                                var objectName = JSPluginState.objects[config.objectId] || "JSPluginNetwork";
                                var errorData = {
                                    SocketId: config.socketId,
                                    Error: "Failed to create WebSocket: " + error.message
                                };
                                
                                var jsonData = JSON.stringify(errorData);
                                JSPluginHelper.sendUnityMessage(objectName, config.errorMethod, jsonData);
                            } catch (e) {
                                NetworkHelper.logError("Failed to report WebSocket creation error: " + e);
                            }
                            
                            return null;
                        }
                    }
                };
                
                return 1;
            } catch (error) {
                console.error("[UnityJSTools] Error initializing network module: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Sends an HTTP request
     * @param {string} requestId - Pointer to request ID string
     * @param {string} url - Pointer to URL string
     * @param {string} method - Pointer to HTTP method string
     * @param {string} headers - Pointer to headers JSON string
     * @param {string} body - Pointer to request body string
     * @param {string} objectId - Pointer to callback object ID string
     * @param {string} callbackMethod - Pointer to callback method string
     * @return {number} 1 if request was sent, 0 otherwise
     */
    JSPluginNetworkSendRequest: function(requestId, url, method, headers, body, objectId, callbackMethod) {
        try {
            var reqId = UTF8ToString(requestId);
            var urlStr = UTF8ToString(url);
            var methodStr = UTF8ToString(method);
            var headersStr = UTF8ToString(headers);
            var bodyStr = UTF8ToString(body);
            var objId = UTF8ToString(objectId);
            var callback = UTF8ToString(callbackMethod);
            
            // Parse headers from JSON
            var headerObj = {};
            try {
                headerObj = JSON.parse(headersStr);
            } catch (e) {
                NetworkHelper.logError("Error parsing headers: " + e);
            }
            
            var config = {
                requestId: reqId,
                url: urlStr,
                method: methodStr,
                headers: headerObj,
                body: bodyStr,
                objectId: objId,
                callbackMethod: callback
            };
            
            window.UnityJSTools.network.request(config);
            return 1;
        } catch (error) {
            NetworkHelper.logError("Error in SendRequest: " + error);
            return 0;
        }
    },
    
    /**
     * Cancels an HTTP request
     * @param {string} requestId - Pointer to request ID string
     * @return {number} 1 if request was cancelled, 0 otherwise
     */
    JSPluginNetworkCancelRequest: function(requestId) {
        try {
            var reqId = UTF8ToString(requestId);
            var result = window.UnityJSTools.network.cancelRequest(reqId);
            return result ? 1 : 0;
        } catch (error) {
            NetworkHelper.logError("Error in CancelRequest: " + error);
            return 0;
        }
    },
    
    /**
     * Creates a WebSocket connection
     * @param {string} socketId - Pointer to socket ID string
     * @param {string} url - Pointer to WebSocket URL string
     * @param {string} protocols - Pointer to protocols string or null
     * @param {string} objectId - Pointer to callback object ID string
     * @param {string} messageMethod - Pointer to message callback method string
     * @param {string} openMethod - Pointer to open callback method string
     * @param {string} closeMethod - Pointer to close callback method string
     * @param {string} errorMethod - Pointer to error callback method string
     * @return {number} 1 if WebSocket was created, 0 otherwise
     */
    JSPluginNetworkCreateWebSocket: function(socketId, url, protocols, objectId, messageMethod, openMethod, closeMethod, errorMethod) {
        try {
            var sockId = UTF8ToString(socketId);
            var urlStr = UTF8ToString(url);
            var protoStr = protocols ? UTF8ToString(protocols) : null;
            var objId = UTF8ToString(objectId);
            var msgMethod = UTF8ToString(messageMethod);
            var openMeth = UTF8ToString(openMethod);
            var closeMeth = UTF8ToString(closeMethod);
            var errMeth = UTF8ToString(errorMethod);
            
            var config = {
                socketId: sockId,
                url: urlStr,
                protocols: protoStr,
                objectId: objId,
                messageMethod: msgMethod,
                openMethod: openMeth,
                closeMethod: closeMeth,
                errorMethod: errMeth
            };
            
            var ws = window.UnityJSTools.network.createWebSocket(config);
            return ws ? 1 : 0;
        } catch (error) {
            NetworkHelper.logError("Error in CreateWebSocket: " + error);
            return 0;
        }
    },
    
    /**
     * Sends a message through a WebSocket
     * @param {string} socketId - Pointer to socket ID string
     * @param {string} message - Pointer to message string
     * @return {number} 1 if message was sent, 0 otherwise
     */
    JSPluginNetworkSendWebSocketMessage: function(socketId, message) {
        try {
            var sockId = UTF8ToString(socketId);
            var msgStr = UTF8ToString(message);
            
            var socketInfo = NetworkState.webSockets[sockId];
            if (!socketInfo) {
                NetworkHelper.logError("WebSocket not found: " + sockId);
                return 0;
            }
            
            socketInfo.socket.send(msgStr);
            return 1;
        } catch (error) {
            NetworkHelper.logError("Error in SendWebSocketMessage: " + error);
            return 0;
        }
    },
    
    /**
     * Closes a WebSocket connection
     * @param {string} socketId - Pointer to socket ID string
     * @param {number} code - Close code
     * @param {string} reason - Pointer to close reason string
     * @return {number} 1 if socket was closed, 0 otherwise
     */
    JSPluginNetworkCloseWebSocket: function(socketId, code, reason) {
        try {
            var sockId = UTF8ToString(socketId);
            var reasonStr = reason ? UTF8ToString(reason) : "";
            
            var socketInfo = NetworkState.webSockets[sockId];
            if (!socketInfo) {
                NetworkHelper.logError("WebSocket not found: " + sockId);
                return 0;
            }
            
            socketInfo.socket.close(code, reasonStr);
            return 1;
        } catch (error) {
            NetworkHelper.logError("Error in CloseWebSocket: " + error);
            return 0;
        }
    }
};

// Proper dependency registration
autoAddDeps(JSPluginNetwork, '$NetworkState');
autoAddDeps(JSPluginNetwork, '$NetworkHelper');
mergeInto(LibraryManager.library, JSPluginNetwork);
