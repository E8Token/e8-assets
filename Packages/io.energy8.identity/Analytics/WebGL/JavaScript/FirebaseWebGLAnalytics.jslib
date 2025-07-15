mergeInto(LibraryManager.library, {
    InitializeAnalytics: function(objectName, errorCallback) {
        var objectNameStr = UTF8ToString(objectName);
        var errorCallbackStr = UTF8ToString(errorCallback);
        
        try {
            // Check if Firebase is already initialized
            if (!window.firebaseAnalytics) {
                throw new Error('Firebase Analytics is not available. Make sure Firebase is properly initialized.');
            }
            
            // Analytics is already initialized in firebaseHandler.js
            console.log("Firebase Analytics initialized successfully");
        } catch (error) {
            console.error("Error initializing Firebase Analytics:", error);
            if (errorCallbackStr) {
                window.unityInstance.SendMessage(objectNameStr, errorCallbackStr, error.message);
            }
        }
    },
      LogEvent: function(eventName, parameters) {
        var eventNameStr = UTF8ToString(eventName);
        var parametersStr = UTF8ToString(parameters);
        
        try {
            if (!window.firebaseAnalytics) {
                throw new Error('Firebase Analytics not initialized');
            }
            
            var params = {};
            if (parametersStr && parametersStr !== '{}') {
                var parsedParams = JSON.parse(parametersStr);
                if (parsedParams.items && Array.isArray(parsedParams.items)) {
                    parsedParams.items.forEach(function(item) {
                        params[item.key] = item.value;
                    });
                }
            }
            
            window.firebaseAnalytics.logEvent(eventNameStr, params);
            console.log("Firebase Analytics: Logged event", eventNameStr, params);
        } catch (error) {
            console.error("Error logging Firebase Analytics event:", error);
        }
    },
    
    SetUserId: function(userId) {
        var userIdStr = userId ? UTF8ToString(userId) : null;
        
        try {
            if (!window.firebaseAnalytics) {
                throw new Error('Firebase Analytics not initialized');
            }
            
            window.firebaseAnalytics.setUserId(userIdStr);
            console.log("Firebase Analytics: Set user ID", userIdStr);
        } catch (error) {
            console.error("Error setting Firebase Analytics user ID:", error);
        }
    },
    
    SetUserProperties: function(properties) {
        var propertiesStr = UTF8ToString(properties);
        
        try {
            if (!window.firebaseAnalytics) {
                throw new Error('Firebase Analytics not initialized');
            }
            
            var props = {};
            if (propertiesStr && propertiesStr !== '{}') {
                var parsedProps = JSON.parse(propertiesStr);
                if (parsedProps.items && Array.isArray(parsedProps.items)) {
                    parsedProps.items.forEach(function(item) {
                        props[item.key] = item.value;
                    });
                }
            }
            
            window.firebaseAnalytics.setUserProperties(props);
            console.log("Firebase Analytics: Set user properties", props);
        } catch (error) {
            console.error("Error setting Firebase Analytics user properties:", error);
        }
    },
      ResetAnalyticsData: function() {
        try {
            if (!window.firebaseAnalytics) {
                throw new Error('Firebase Analytics not initialized');
            }
            
            // В Firebase JS SDK v10.13.2, метод resetAnalyticsData не имеет прямого эквивалента
            // Мы можем установить случайный идентификатор пользователя, чтобы фактически сбросить данные
            window.firebaseAnalytics.setUserId(null);
            console.log("Firebase Analytics: Reset analytics data (user ID set to null)");
        } catch (error) {
            console.error("Error resetting Firebase Analytics data:", error);
        }
    }
});
