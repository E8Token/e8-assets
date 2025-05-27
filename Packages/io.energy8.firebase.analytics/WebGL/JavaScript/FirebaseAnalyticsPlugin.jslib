// Firebase Analytics WebGL Plugin JavaScript Library
var FirebaseAnalyticsPluginLib = {
    
    // Log event without parameters
    FirebaseAnalytics_LogEvent: function(eventNamePtr) {
        try {
            var eventName = UTF8ToString(eventNamePtr);
            
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Logging event:', eventName);
            
            // Log event using gtag
            gtag('event', eventName);
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to log event:', error);
            return false;
        }
    },

    // Log event with parameters
    FirebaseAnalytics_LogEventWithParameters: function(eventNamePtr, parametersPtr) {
        try {
            var eventName = UTF8ToString(eventNamePtr);
            var parametersJson = UTF8ToString(parametersPtr);
            
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Logging event:', eventName, 'with parameters:', parametersJson);
            
            // Parse parameters
            var parameters = JSON.parse(parametersJson);
            
            // Log event with parameters using gtag
            gtag('event', eventName, parameters);
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to log event with parameters:', error);
            return false;
        }
    },

    // Set user ID
    FirebaseAnalytics_SetUserId: function(userIdPtr) {
        try {
            var userId = UTF8ToString(userIdPtr);
            
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Setting user ID:', userId);
            
            // Set user ID using gtag
            gtag('config', 'GA_MEASUREMENT_ID', {
                'user_id': userId
            });
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to set user ID:', error);
            return false;
        }
    },

    // Set user property
    FirebaseAnalytics_SetUserProperty: function(namePtr, valuePtr) {
        try {
            var name = UTF8ToString(namePtr);
            var value = UTF8ToString(valuePtr);
            
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Setting user property:', name, '=', value);
            
            // Set user property using gtag
            var properties = {};
            properties[name] = value;
            
            gtag('config', 'GA_MEASUREMENT_ID', {
                'custom_parameter': properties
            });
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to set user property:', error);
            return false;
        }
    },

    // Set analytics collection enabled
    FirebaseAnalytics_SetAnalyticsCollectionEnabled: function(enabled) {
        try {
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Setting analytics collection enabled:', enabled);
            
            if (enabled) {
                // Enable analytics collection
                gtag('consent', 'update', {
                    'analytics_storage': 'granted'
                });
            } else {
                // Disable analytics collection
                gtag('consent', 'update', {
                    'analytics_storage': 'denied'
                });
            }
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to set analytics collection enabled:', error);
            return false;
        }
    },

    // Set session timeout duration
    FirebaseAnalytics_SetSessionTimeoutDuration: function(milliseconds) {
        try {
            if (typeof gtag === 'undefined') {
                console.error('[FirebaseAnalytics] Google Analytics (gtag) not loaded');
                return false;
            }

            console.log('[FirebaseAnalytics] Setting session timeout duration:', milliseconds, 'ms');
            
            // Convert milliseconds to seconds for gtag
            var seconds = Math.floor(milliseconds / 1000);
            
            gtag('config', 'GA_MEASUREMENT_ID', {
                'session_timeout': seconds
            });
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to set session timeout duration:', error);
            return false;
        }
    },

    // Reset analytics data
    FirebaseAnalytics_ResetAnalyticsData: function() {
        try {
            console.log('[FirebaseAnalytics] Resetting analytics data');
            
            // Clear local storage analytics data
            if (typeof localStorage !== 'undefined') {
                var keys = Object.keys(localStorage);
                for (var i = 0; i < keys.length; i++) {
                    if (keys[i].startsWith('_ga') || keys[i].startsWith('_gid')) {
                        localStorage.removeItem(keys[i]);
                    }
                }
            }
            
            // Clear session storage analytics data
            if (typeof sessionStorage !== 'undefined') {
                var sessionKeys = Object.keys(sessionStorage);
                for (var j = 0; j < sessionKeys.length; j++) {
                    if (sessionKeys[j].startsWith('_ga') || sessionKeys[j].startsWith('_gid')) {
                        sessionStorage.removeItem(sessionKeys[j]);
                    }
                }
            }
            
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to reset analytics data:', error);
            return false;
        }
    },

    // Initialize Firebase Analytics
    FirebaseAnalytics_Initialize: function(configPtr) {
        try {
            var config = UTF8ToString(configPtr);
            console.log('[FirebaseAnalytics] Initializing with config:', config);
            
            // Parse config
            var analyticsConfig = JSON.parse(config);
            
            // Initialize gtag if not already done
            if (typeof gtag === 'undefined') {
                console.warn('[FirebaseAnalytics] gtag not found, please ensure Google Analytics is properly loaded');
                return false;
            }
            
            // Configure gtag with measurement ID from config
            if (analyticsConfig.measurementId) {
                gtag('config', analyticsConfig.measurementId, {
                    'send_page_view': analyticsConfig.enableAutomaticScreenReporting || true
                });
            }
            
            console.log('[FirebaseAnalytics] Initialized successfully');
            return true;
        } catch (error) {
            console.error('[FirebaseAnalytics] Failed to initialize:', error);
            return false;
        }
    }
};
