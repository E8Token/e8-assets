mergeInto(LibraryManager.library, {
    // Internal storage for plugin references and state
    $JSPluginToolsInternal: {
        plugins: {},
        namespaces: {},
        debugMode: false,
        exposeToWindow: false,
        lastMessages: [],
        maxStoredMessages: 20,

        log: function(message) {
            if (this.debugMode) {
                console.log("[Energy8.JSPluginTools] " + message);
            }
        },

        storeMessage: function(direction, pluginName, method, payload) {
            if (this.debugMode) {
                // Store message for debug purposes
                this.lastMessages.push({
                    timestamp: Date.now(),
                    direction: direction,
                    plugin: pluginName,
                    method: method,
                    payload: payload
                });
                
                // Limit the number of stored messages
                if (this.lastMessages.length > this.maxStoredMessages) {
                    this.lastMessages.shift();
                }
            }
        },

        // Internal debug utility - accessible only via console
        getDebugInfo: function() {
            if (!this.debugMode) {
                return "Debug mode is disabled. Enable it from Unity with JSPluginManager.Instance.DebugMode = true";
            }
            
            return {
                registeredPlugins: Object.keys(this.plugins),
                registeredNamespaces: Object.keys(this.namespaces),
                lastMessages: this.lastMessages,
                debugMode: this.debugMode,
                exposeToWindow: this.exposeToWindow
            };
        },

        // Organize plugins by namespace
        getNamespace: function(namespace) {
            if (!this.namespaces[namespace]) {
                this.namespaces[namespace] = {};
                this.log("Created namespace: " + namespace);
            }
            return this.namespaces[namespace];
        },

        // Expose all plugins/namespaces to window when in debug mode with exposeToWindow enabled
        exposeToGlobalScope: function() {
            if (!this.debugMode || !this.exposeToWindow) return;
            if (!window.Energy8) window.Energy8 = {};
            window.Energy8.JSPluginTools = {
                plugins: this.plugins,
                namespaces: this.namespaces,
                debug: this.getDebugInfo()
            };
            this.log("Exposed JSPluginTools to window.Energy8.JSPluginTools");
        },
        cleanupGlobalScope: function() {
            if (window.Energy8 && window.Energy8.JSPluginTools) {
                delete window.Energy8.JSPluginTools;
                this.log("Removed JSPluginTools from window.Energy8");
                if (Object.keys(window.Energy8).length === 0) {
                    delete window.Energy8;
                    this.log("Removed empty Energy8 namespace from window");
                }
            }
        }
    },

    // Called from C# to register a plugin
    RegisterPluginWithJS: function(pluginNamePtr) {
        var pluginName = UTF8ToString(pluginNamePtr);
        JSPluginToolsInternal.plugins[pluginName] = true;
        JSPluginToolsInternal.log("Registered plugin: " + pluginName);
    },

    // Called from C# to register a plugin with a namespace
    RegisterPluginWithNamespace: function(pluginNamePtr, namespacePtr) {
        var pluginName = UTF8ToString(pluginNamePtr);
        var namespace = UTF8ToString(namespacePtr);
        JSPluginToolsInternal.plugins[pluginName] = true;
        var ns = JSPluginToolsInternal.getNamespace(namespace);
        ns[pluginName] = true;
        JSPluginToolsInternal.log("Registered plugin: " + pluginName + " in namespace: " + namespace);
        if (JSPluginToolsInternal.debugMode && JSPluginToolsInternal.exposeToWindow) {
            JSPluginToolsInternal.exposeToGlobalScope();
        }
    },

    // Called from C# to send a message to JavaScript
    SendMessageToJS: function(pluginNamePtr, methodPtr, payloadPtr) {
        var pluginName = UTF8ToString(pluginNamePtr);
        var method = UTF8ToString(methodPtr);
        var payload = UTF8ToString(payloadPtr);
        
        JSPluginToolsInternal.storeMessage("C#->JS", pluginName, method, payload);
        JSPluginToolsInternal.log("Received message from C#: " + pluginName + "." + method);
        
        // Process the message (plugins would register handlers for these)
        // In a real implementation, we'd dispatch to registered handlers
        try {
            // Here we'd call the plugin's handler if it exists
            JSPluginToolsInternal.log("Message processed successfully");
        } catch (e) {
            console.error("[Energy8.JSPluginTools] Error processing message:", e);
        }
    },

    // Called from C# to send a message to JavaScript with namespace
    SendMessageToNamespace: function(namespacePtr, pluginNamePtr, methodPtr, payloadPtr) {
        var namespace = UTF8ToString(namespacePtr);
        var pluginName = UTF8ToString(pluginNamePtr);
        var method = UTF8ToString(methodPtr);
        var payload = UTF8ToString(payloadPtr);
        
        JSPluginToolsInternal.storeMessage("C#->JS", namespace + "." + pluginName, method, payload);
        JSPluginToolsInternal.log("Received message from C#: " + namespace + "." + pluginName + "." + method);
        
        // Process the message (plugins would register handlers for these)
        // In a real implementation, we'd dispatch to registered handlers
        try {
            // Here we'd call the plugin's handler if it exists
            JSPluginToolsInternal.log("Message processed successfully");
        } catch (e) {
            console.error("[Energy8.JSPluginTools] Error processing message:", e);
        }
    },

    // Called from C# to enable/disable debug mode
    SetDebugMode: function(enabled) {
        JSPluginToolsInternal.debugMode = !!enabled;
        JSPluginToolsInternal.log("Debug mode " + (JSPluginToolsInternal.debugMode ? "enabled" : "disabled"));
        
        // Expose debug info via a special hook that doesn't pollute global namespace
        if (JSPluginToolsInternal.debugMode) {
            // Use a Symbol to prevent name collisions and global namespace pollution
            var debugSymbol = Symbol.for("Energy8_JSPluginTools_Debug");
            Object[debugSymbol] = function() {
                return JSPluginToolsInternal.getDebugInfo();
            };
            JSPluginToolsInternal.log("Debug hook available via: Object[Symbol.for('Energy8_JSPluginTools_Debug')]()");
            if (JSPluginToolsInternal.exposeToWindow) {
                JSPluginToolsInternal.exposeToGlobalScope();
            }
        } else {
            // Clean up debug hook when disabled
            if (Symbol.for("Energy8_JSPluginTools_Debug") in Object) {
                delete Object[Symbol.for("Energy8_JSPluginTools_Debug")];
            }
            JSPluginToolsInternal.cleanupGlobalScope();
        }
    },

    // Called from C# to enable/disable exposing to window object
    SetExposeToWindow: function(enabled) {
        JSPluginToolsInternal.exposeToWindow = !!enabled;
        JSPluginToolsInternal.log("Expose to window " + (JSPluginToolsInternal.exposeToWindow ? "enabled" : "disabled"));
        if (JSPluginToolsInternal.debugMode) {
            if (JSPluginToolsInternal.exposeToWindow) {
                JSPluginToolsInternal.exposeToGlobalScope();
            } else {
                JSPluginToolsInternal.cleanupGlobalScope();
            }
        }
    }
});
