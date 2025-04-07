var JSPluginDevice = {
    // Use the shared core module state objects
    $JSPluginState: null,
    $JSPluginHelper: null,
    
    /**
     * Device-specific state object
     * @private
     */
    $DeviceState: {
        // Listener for orientation changes
        orientationListener: null,
        
        // Callback information for orientation changes
        orientationCallback: {
            objectId: null,
            methodName: null
        },
        
        // Allocated strings
        allocatedStrings: {},
        
        // Counter for generating unique IDs
        nextId: 1
    },
    
    /**
     * Helper functions for device module
     * @private
     */
    $DeviceHelper: {
        /**
         * Log an error message
         * @param {string} message - Error message
         */
        logError: function(message) {
            console.error("[UnityJSTools/Device] " + message);
        },
        
        /**
         * Log a debug message
         * @param {string} message - Debug message
         */
        logDebug: function(message) {
            if (JSPluginState.debugEnabled) {
                console.log("[UnityJSTools/Device] " + message);
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
            var id = DeviceState.nextId++;
            DeviceState.allocatedStrings[id] = buffer;
            
            // Schedule cleanup on next frame
            setTimeout(function() {
                if (DeviceState.allocatedStrings[id]) {
                    _free(DeviceState.allocatedStrings[id]);
                    delete DeviceState.allocatedStrings[id];
                }
            }, 0);
            
            return buffer;
        },
        
        /**
         * Detects if the device is a mobile device
         * @return {boolean} True if mobile
         */
        isMobileDevice: function() {
            var userAgent = navigator.userAgent || navigator.vendor || window.opera;
            return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini/i.test(userAgent.toLowerCase());
        },
        
        /**
         * Detects if the device is a tablet
         * @return {boolean} True if tablet
         */
        isTabletDevice: function() {
            var userAgent = navigator.userAgent || navigator.vendor || window.opera;
            var isTablet = /ipad|android(?!.*mobile)/i.test(userAgent.toLowerCase());
            
            // Also check for tablet-sized screens
            if (!isTablet) {
                isTablet = this.isMobileDevice() && Math.min(window.screen.width, window.screen.height) >= 600;
            }
            
            return isTablet;
        },
        
        /**
         * Gets information about the current browser
         * @return {Object} Browser information
         */
        getBrowserInfo: function() {
            var browserInfo = {
                Name: "Unknown",
                Version: "Unknown",
                Engine: "Unknown",
                Language: navigator.language || navigator.userLanguage || "Unknown",
                CookiesEnabled: navigator.cookieEnabled || false,
                LocalStorageAvailable: false
            };
            
            try {
                // Test for localStorage availability
                browserInfo.LocalStorageAvailable = !!window.localStorage;
            } catch (e) {
                // Local storage is disabled or blocked
                browserInfo.LocalStorageAvailable = false;
            }
            
            var userAgent = navigator.userAgent;
            
            // Detect browser name and version
            if (userAgent.indexOf("Edge") > -1) {
                browserInfo.Name = "Edge";
                browserInfo.Engine = "EdgeHTML";
                var edgeMatch = userAgent.match(/Edge\/(\d+\.\d+)/);
                if (edgeMatch) browserInfo.Version = edgeMatch[1];
            } else if (userAgent.indexOf("Chrome") > -1) {
                browserInfo.Name = "Chrome";
                browserInfo.Engine = "Blink";
                var chromeMatch = userAgent.match(/Chrome\/(\d+\.\d+)/);
                if (chromeMatch) browserInfo.Version = chromeMatch[1];
            } else if (userAgent.indexOf("Firefox") > -1) {
                browserInfo.Name = "Firefox";
                browserInfo.Engine = "Gecko";
                var ffMatch = userAgent.match(/Firefox\/(\d+\.\d+)/);
                if (ffMatch) browserInfo.Version = ffMatch[1];
            } else if (userAgent.indexOf("Safari") > -1) {
                browserInfo.Name = "Safari";
                browserInfo.Engine = "WebKit";
                var safariMatch = userAgent.match(/Version\/(\d+\.\d+)/);
                if (safariMatch) browserInfo.Version = safariMatch[1];
            } else if (userAgent.indexOf("MSIE") > -1 || userAgent.indexOf("Trident") > -1) {
                browserInfo.Name = "IE";
                browserInfo.Engine = "Trident";
                var ieMatch = userAgent.match(/(?:MSIE |rv:)(\d+\.\d+)/);
                if (ieMatch) browserInfo.Version = ieMatch[1];
            } else if (userAgent.indexOf("Opera") > -1 || userAgent.indexOf("OPR") > -1) {
                browserInfo.Name = "Opera";
                browserInfo.Engine = "Presto";
                var operaMatch = userAgent.match(/(?:Opera|OPR)\/(\d+\.\d+)/);
                if (operaMatch) browserInfo.Version = operaMatch[1];
            }
            
            return browserInfo;
        },
        
        /**
         * Gets information about the current operating system
         * @return {Object} OS information
         */
        getOSInfo: function() {
            var osInfo = {
                Name: "Unknown",
                Version: "Unknown",
                Architecture: "Unknown",
                Platform: "Unknown"
            };
            
            var userAgent = navigator.userAgent;
            
            // Detect OS and version
            if (userAgent.indexOf("Win") > -1) {
                osInfo.Name = "Windows";
                osInfo.Platform = "Desktop";
                
                if (userAgent.indexOf("Windows NT 10.0") > -1) osInfo.Version = "10";
                else if (userAgent.indexOf("Windows NT 6.3") > -1) osInfo.Version = "8.1";
                else if (userAgent.indexOf("Windows NT 6.2") > -1) osInfo.Version = "8";
                else if (userAgent.indexOf("Windows NT 6.1") > -1) osInfo.Version = "7";
                else if (userAgent.indexOf("Windows NT 6.0") > -1) osInfo.Version = "Vista";
                else if (userAgent.indexOf("Windows NT 5.1") > -1) osInfo.Version = "XP";
            } else if (userAgent.indexOf("Mac") > -1) {
                osInfo.Name = "macOS";
                osInfo.Platform = "Desktop";
                
                var macMatch = userAgent.match(/Mac OS X (\d+[._]\d+[._]?\d*)/);
                if (macMatch) {
                    osInfo.Version = macMatch[1].replace(/_/g, ".");
                }
            } else if (userAgent.indexOf("iPhone") > -1 || userAgent.indexOf("iPad") > -1 || userAgent.indexOf("iPod") > -1) {
                osInfo.Name = "iOS";
                osInfo.Platform = userAgent.indexOf("iPad") > -1 ? "Tablet" : "Mobile";
                
                var iosMatch = userAgent.match(/OS (\d+[._]\d+[._]?\d*)/);
                if (iosMatch) {
                    osInfo.Version = iosMatch[1].replace(/_/g, ".");
                }
            } else if (userAgent.indexOf("Android") > -1) {
                osInfo.Name = "Android";
                osInfo.Platform = this.isTabletDevice() ? "Tablet" : "Mobile";
                
                var androidMatch = userAgent.match(/Android (\d+\.\d+)/);
                if (androidMatch) osInfo.Version = androidMatch[1];
            } else if (userAgent.indexOf("Linux") > -1) {
                osInfo.Name = "Linux";
                osInfo.Platform = "Desktop";
            }
            
            // Detect architecture
            if (userAgent.indexOf("x64") > -1 || userAgent.indexOf("x86_64") > -1 || userAgent.indexOf("Win64") > -1) {
                osInfo.Architecture = "64-bit";
            } else if (userAgent.indexOf("x86") > -1 || userAgent.indexOf("WOW64") > -1) {
                osInfo.Architecture = "32-bit";
            } else if (userAgent.indexOf("arm") > -1 || userAgent.indexOf("ARM") > -1) {
                osInfo.Architecture = "ARM";
            }
            
            return osInfo;
        },
        
        /**
         * Gets information about the device screen
         * @return {Object} Screen information
         */
        getScreenInfo: function() {
            var screenInfo = {
                Width: window.screen.width,
                Height: window.screen.height,
                ColorDepth: window.screen.colorDepth || 24,
                PixelRatio: window.devicePixelRatio || 1,
                Orientation: (window.screen.orientation && window.screen.orientation.type) || 
                             (window.innerWidth > window.innerHeight ? "landscape" : "portrait"),
                TouchSupport: "not supported"
            };
            
            // Detect touch support
            if ('ontouchstart' in window || navigator.maxTouchPoints > 0) {
                screenInfo.TouchSupport = "supported";
            }
            
            return screenInfo;
        }
    },
    
    /**
     * Initializes the device module and extends the UnityJSTools global object
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceInitialize: function() {
        if (typeof window !== 'undefined' && window.UnityJSTools) {
            try {
                // Add device methods
                window.UnityJSTools.device = {
                    /**
                     * Gets information about the current browser
                     * @return {Object} Browser information
                     */
                    getBrowserInfo: function() {
                        return DeviceHelper.getBrowserInfo();
                    },
                    
                    /**
                     * Gets information about the current operating system
                     * @return {Object} OS information
                     */
                    getOSInfo: function() {
                        return DeviceHelper.getOSInfo();
                    },
                    
                    /**
                     * Gets information about the device screen
                     * @return {Object} Screen information
                     */
                    getScreenInfo: function() {
                        return DeviceHelper.getScreenInfo();
                    },
                    
                    /**
                     * Checks if the current device is a mobile device
                     * @return {boolean} True if the device is mobile
                     */
                    isMobile: function() {
                        return DeviceHelper.isMobileDevice();
                    },
                    
                    /**
                     * Checks if the current device is a tablet
                     * @return {boolean} True if the device is a tablet
                     */
                    isTablet: function() {
                        return DeviceHelper.isTabletDevice();
                    },
                    
                    /**
                     * Requests fullscreen mode for the application
                     * @return {boolean} True if request was successful
                     */
                    requestFullscreen: function() {
                        try {
                            var canvas = Module.canvas;
                            
                            if (canvas.requestFullscreen) {
                                canvas.requestFullscreen();
                                return true;
                            } else if (canvas.mozRequestFullScreen) {
                                canvas.mozRequestFullScreen();
                                return true;
                            } else if (canvas.webkitRequestFullscreen) {
                                canvas.webkitRequestFullscreen();
                                return true;
                            } else if (canvas.msRequestFullscreen) {
                                canvas.msRequestFullscreen();
                                return true;
                            }
                            
                            return false;
                        } catch (e) {
                            DeviceHelper.logError("Error requesting fullscreen: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Exits fullscreen mode
                     * @return {boolean} True if exit was successful
                     */
                    exitFullscreen: function() {
                        try {
                            if (document.exitFullscreen) {
                                document.exitFullscreen();
                                return true;
                            } else if (document.mozCancelFullScreen) {
                                document.mozCancelFullScreen();
                                return true;
                            } else if (document.webkitExitFullscreen) {
                                document.webkitExitFullscreen();
                                return true;
                            } else if (document.msExitFullscreen) {
                                document.msExitFullscreen();
                                return true;
                            }
                            
                            return false;
                        } catch (e) {
                            DeviceHelper.logError("Error exiting fullscreen: " + e);
                            return false;
                        }
                    },
                    
                    /**
                     * Makes the device vibrate
                     * @param {number} milliseconds - Duration of vibration
                     * @return {boolean} True if vibration was supported
                     */
                    vibrate: function(milliseconds) {
                        try {
                            if (navigator.vibrate) {
                                navigator.vibrate(milliseconds);
                                return true;
                            }
                            return false;
                        } catch (e) {
                            DeviceHelper.logError("Error vibrating device: " + e);
                            return false;
                        }
                    }
                };
                
                return 1;
            } catch (error) {
                console.error("[UnityJSTools] Error initializing device module: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Gets the user agent string
     * @return {number} Pointer to user agent string
     */
    JSPluginDeviceGetUserAgent: function() {
        try {
            return DeviceHelper.allocateString(navigator.userAgent || "");
        } catch (error) {
            DeviceHelper.logError("Error in GetUserAgent: " + error);
            return 0;
        }
    },
    
    /**
     * Gets browser information as JSON
     * @return {number} Pointer to JSON string
     */
    JSPluginDeviceGetBrowserInfo: function() {
        try {
            var info = DeviceHelper.getBrowserInfo();
            return DeviceHelper.allocateString(JSON.stringify(info));
        } catch (error) {
            DeviceHelper.logError("Error in GetBrowserInfo: " + error);
            return 0;
        }
    },
    
    /**
     * Gets OS information as JSON
     * @return {number} Pointer to JSON string
     */
    JSPluginDeviceGetOSInfo: function() {
        try {
            var info = DeviceHelper.getOSInfo();
            return DeviceHelper.allocateString(JSON.stringify(info));
        } catch (error) {
            DeviceHelper.logError("Error in GetOSInfo: " + error);
            return 0;
        }
    },
    
    /**
     * Checks if the device is mobile
     * @return {number} 1 if mobile, 0 if not
     */
    JSPluginDeviceIsMobile: function() {
        try {
            return DeviceHelper.isMobileDevice() ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in IsMobile: " + error);
            return 0;
        }
    },
    
    /**
     * Checks if the device is a tablet
     * @return {number} 1 if tablet, 0 if not
     */
    JSPluginDeviceIsTablet: function() {
        try {
            return DeviceHelper.isTabletDevice() ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in IsTablet: " + error);
            return 0;
        }
    },
    
    /**
     * Checks if the device is a desktop
     * @return {number} 1 if desktop, 0 if not
     */
    JSPluginDeviceIsDesktop: function() {
        try {
            return (!DeviceHelper.isMobileDevice() && !DeviceHelper.isTabletDevice()) ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in IsDesktop: " + error);
            return 0;
        }
    },
    
    /**
     * Adds an orientation change listener
     * @param {string} objectId - Pointer to callback object ID string
     * @param {string} methodName - Pointer to callback method string
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceAddOrientationChangeListener: function(objectId, methodName) {
        try {
            var objId = UTF8ToString(objectId);
            var method = UTF8ToString(methodName);
            
            // Remove existing listener if any
            if (DeviceState.orientationListener) {
                JSPluginDeviceRemoveOrientationChangeListener();
            }
            
            // Store callback info
            DeviceState.orientationCallback.objectId = objId;
            DeviceState.orientationCallback.methodName = method;
            
            // Create orientation change handler
            DeviceState.orientationListener = function() {
                try {
                    var screenInfo = DeviceHelper.getScreenInfo();
                    var orientationData = {
                        Orientation: screenInfo.Orientation,
                        Width: window.innerWidth,
                        Height: window.innerHeight,
                        Timestamp: Date.now()
                    };
                    
                    var jsonData = JSON.stringify(orientationData);
                    
                    var objectName = JSPluginState.objects[objId];
                    if (!objectName) {
                        DeviceHelper.logError("Unity object not found for ID: " + objId);
                        return;
                    }
                    
                    JSPluginHelper.sendUnityMessage("JSPluginDevice", "OnOrientationChanged", jsonData);
                } catch (e) {
                    DeviceHelper.logError("Error in orientation change listener: " + e);
                }
            };
            
            // Add listener for both orientation and resize events
            window.addEventListener("orientationchange", DeviceState.orientationListener);
            window.addEventListener("resize", DeviceState.orientationListener);
            
            return 1;
        } catch (error) {
            DeviceHelper.logError("Error in AddOrientationChangeListener: " + error);
            return 0;
        }
    },
    
    /**
     * Removes the orientation change listener
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceRemoveOrientationChangeListener: function() {
        try {
            if (DeviceState.orientationListener) {
                window.removeEventListener("orientationchange", DeviceState.orientationListener);
                window.removeEventListener("resize", DeviceState.orientationListener);
                DeviceState.orientationListener = null;
            }
            
            DeviceState.orientationCallback.objectId = null;
            DeviceState.orientationCallback.methodName = null;
            
            return 1;
        } catch (error) {
            DeviceHelper.logError("Error in RemoveOrientationChangeListener: " + error);
            return 0;
        }
    },
    
    /**
     * Gets screen information as JSON
     * @return {number} Pointer to JSON string
     */
    JSPluginDeviceGetScreenInfo: function() {
        try {
            var info = DeviceHelper.getScreenInfo();
            return DeviceHelper.allocateString(JSON.stringify(info));
        } catch (error) {
            DeviceHelper.logError("Error in GetScreenInfo: " + error);
            return 0;
        }
    },
    
    /**
     * Requests fullscreen mode
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceRequestFullscreen: function() {
        try {
            return window.UnityJSTools.device.requestFullscreen() ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in RequestFullscreen: " + error);
            return 0;
        }
    },
    
    /**
     * Exits fullscreen mode
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceExitFullscreen: function() {
        try {
            return window.UnityJSTools.device.exitFullscreen() ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in ExitFullscreen: " + error);
            return 0;
        }
    },
    
    /**
     * Checks if currently in fullscreen mode
     * @return {number} 1 if in fullscreen, 0 if not
     */
    JSPluginDeviceIsFullscreen: function() {
        try {
            var isFullscreen = !!(
                document.fullscreenElement ||
                document.mozFullScreenElement ||
                document.webkitFullscreenElement ||
                document.msFullscreenElement
            );
            return isFullscreen ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in IsFullscreen: " + error);
            return 0;
        }
    },
    
    /**
     * Makes the device vibrate
     * @param {number} milliseconds - Duration of vibration
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginDeviceVibrate: function(milliseconds) {
        try {
            return window.UnityJSTools.device.vibrate(milliseconds) ? 1 : 0;
        } catch (error) {
            DeviceHelper.logError("Error in Vibrate: " + error);
            return 0;
        }
    }
};

// Proper dependency registration
autoAddDeps(JSPluginDevice, '$DeviceState');
autoAddDeps(JSPluginDevice, '$DeviceHelper');
mergeInto(LibraryManager.library, JSPluginDevice);
