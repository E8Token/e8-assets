var JSPluginStorage = {
    // Use the shared core module state objects with properly defined dependencies
    $StorageState: {
        // Unique IDs for async operations
        storageCallbacks: {},
        nextStorageCallbackId: 1,
        // Cache for allocated strings
        allocatedStrings: {}
    },
    
    /**
     * Helper functions for storage module
     */
    $StorageHelper: {
        logError: function(message) {
            console.error("[UnityJSTools/Storage] " + message);
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
            var id = StorageState.nextStorageCallbackId++;
            StorageState.allocatedStrings[id] = buffer;
            
            // Schedule cleanup on next frame
            setTimeout(function() {
                if (StorageState.allocatedStrings[id]) {
                    _free(StorageState.allocatedStrings[id]);
                    delete StorageState.allocatedStrings[id];
                }
            }, 0);
            
            return buffer;
        }
    },
    
    /**
     * Initializes the localStorage module and extends the UnityJSTools global object.
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginInitializeStorage: function() {
        if (typeof window !== 'undefined' && window.UnityJSTools) {
            try {
                // Add localStorage methods
                window.UnityJSTools.localStorage = {
                    /**
                     * Sets an item in the browser's localStorage.
                     * @param {string} key - The key to store the value under
                     * @param {string} value - The value to store
                     * @return {boolean} True if successful, false otherwise
                     */
                    setItem: function(key, value) {
                        try {
                            localStorage.setItem(key, value);
                            return true;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.setItem: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Gets an item from the browser's localStorage.
                     * @param {string} key - The key to look up
                     * @param {string} defaultValue - Value to return if key doesn't exist
                     * @return {string} The stored value or defaultValue if not found
                     */
                    getItem: function(key, defaultValue) {
                        try {
                            var value = localStorage.getItem(key);
                            return value !== null ? value : defaultValue;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.getItem: " + e);
                            return defaultValue;
                        }
                    },
                    
                    /**
                     * Checks if a key exists in localStorage.
                     * @param {string} key - The key to check
                     * @return {boolean} True if the key exists, false otherwise
                     */
                    hasKey: function(key) {
                        try {
                            return localStorage.getItem(key) !== null;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.hasKey: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Removes an item from localStorage.
                     * @param {string} key - The key to remove
                     * @return {boolean} True if successful, false otherwise
                     */
                    deleteItem: function(key) {
                        try {
                            localStorage.removeItem(key);
                            return true;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.deleteItem: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Clears all items from localStorage.
                     * @return {boolean} True if successful, false otherwise
                     */
                    clear: function() {
                        try {
                            localStorage.clear();
                            return true;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.clear: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Gets all keys from localStorage.
                     * @return {Array<string>} Array of keys
                     */
                    getKeys: function() {
                        try {
                            var keys = [];
                            for (var i = 0; i < localStorage.length; i++) {
                                keys.push(localStorage.key(i));
                            }
                            return keys;
                        } catch (e) {
                            StorageHelper.logError("Error in localStorage.getKeys: " + e);
                            return [];
                        }
                    }
                };
                
                return 1;
            } catch (error) {
                console.error("[UnityJSTools] Error initializing storage: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Sets an item in localStorage.
     * @param {string} key - Pointer to key string
     * @param {string} value - Pointer to value string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginLocalStorageSetItem: function(key, value) {
        try {
            var keyStr = UTF8ToString(key);
            var valueStr = UTF8ToString(value);
            
            localStorage.setItem(keyStr, valueStr);
            return 1;
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageSetItem: " + error);
            return 0;
        }
    },

    /**
     * Gets an item from localStorage.
     * @param {string} key - Pointer to key string
     * @param {string} defaultValue - Pointer to default value string
     * @return {number} Pointer to the result string (must be freed by caller)
     */
    JSPluginLocalStorageGetItem: function(key, defaultValue) {
        try {
            var keyStr = UTF8ToString(key);
            var defaultStr = defaultValue ? UTF8ToString(defaultValue) : "";
            
            var result = localStorage.getItem(keyStr);
            if (result === null) {
                return StorageHelper.allocateString(defaultStr);
            }
            
            return StorageHelper.allocateString(result);
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageGetItem: " + error);
            return StorageHelper.allocateString(defaultStr);
        }
    },

    /**
     * Checks if a key exists in localStorage.
     * @param {string} key - Pointer to key string
     * @return {number} 1 if the key exists, 0 otherwise
     */
    JSPluginLocalStorageHasKey: function(key) {
        try {
            var keyStr = UTF8ToString(key);
            return localStorage.getItem(keyStr) !== null ? 1 : 0;
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageHasKey: " + error);
            return 0;
        }
    },

    /**
     * Removes an item from localStorage.
     * @param {string} key - Pointer to key string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginLocalStorageDeleteItem: function(key) {
        try {
            var keyStr = UTF8ToString(key);
            localStorage.removeItem(keyStr);
            return 1;
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageDeleteItem: " + error);
            return 0;
        }
    },

    /**
     * Clears all items from localStorage.
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginLocalStorageClear: function() {
        try {
            localStorage.clear();
            return 1;
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageClear: " + error);
            return 0;
        }
    },

    /**
     * Gets all keys from localStorage.
     * @return {number} Pointer to the JSON array string (must be freed by caller)
     */
    JSPluginLocalStorageGetKeys: function() {
        try {
            var keys = [];
            for (var i = 0; i < localStorage.length; i++) {
                keys.push(localStorage.key(i));
            }
            var keysJson = JSON.stringify(keys);
            
            return StorageHelper.allocateString(keysJson);
        } catch (error) {
            StorageHelper.logError("Error in LocalStorageGetKeys: " + error);
            return StorageHelper.allocateString("[]");
        }
    }
};

// Proper dependency registration
autoAddDeps(JSPluginStorage, '$StorageState');
autoAddDeps(JSPluginStorage, '$StorageHelper');
mergeInto(LibraryManager.library, JSPluginStorage);
