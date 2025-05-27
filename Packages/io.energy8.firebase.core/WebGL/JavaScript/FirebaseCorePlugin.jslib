// Firebase Core WebGL Plugin JavaScript Library
var FirebaseCorePluginLib = {
    
    // Initialize Firebase app
    FirebaseCore_InitializeApp: function(configPtr, appNamePtr) {
        try {
            var config = UTF8ToString(configPtr);
            var appName = appNamePtr ? UTF8ToString(appNamePtr) : undefined;
            
            if (typeof firebase === 'undefined') {
                console.error('[FirebaseCore] Firebase SDK not loaded');
                return 0;
            }

            console.log('[FirebaseCore] Initializing app:', appName || '[DEFAULT]');
            
            // Parse config
            var firebaseConfig = JSON.parse(config);
            
            // Initialize Firebase app
            var app;
            if (appName && appName !== '[DEFAULT]') {
                app = firebase.initializeApp(firebaseConfig, appName);
            } else {
                app = firebase.initializeApp(firebaseConfig);
            }
            
            // Return app info as JSON
            var appInfo = {
                name: app.name,
                projectId: firebaseConfig.projectId,
                apiKey: firebaseConfig.apiKey,
                appId: firebaseConfig.appId,
                isInitialized: true
            };
            
            var result = JSON.stringify(appInfo);
            var buffer = _malloc(lengthBytesUTF8(result) + 1);
            stringToUTF8(result, buffer, lengthBytesUTF8(result) + 1);
            return buffer;
            
        } catch (error) {
            console.error('[FirebaseCore] Failed to initialize app:', error);
            return 0;
        }
    },

    // Get Firebase app
    FirebaseCore_GetApp: function(appNamePtr) {
        try {
            var appName = appNamePtr ? UTF8ToString(appNamePtr) : undefined;
            
            if (typeof firebase === 'undefined') {
                console.error('[FirebaseCore] Firebase SDK not loaded');
                return 0;
            }
            
            var app = firebase.app(appName);
            if (!app) return 0;
            
            var appInfo = {
                name: app.name,
                projectId: app.options.projectId,
                apiKey: app.options.apiKey,
                appId: app.options.appId,
                isInitialized: true
            };
            
            var result = JSON.stringify(appInfo);
            var buffer = _malloc(lengthBytesUTF8(result) + 1);
            stringToUTF8(result, buffer, lengthBytesUTF8(result) + 1);
            return buffer;
            
        } catch (error) {
            console.error('[FirebaseCore] Failed to get app:', error);
            return 0;
        }
    },

    // Get all Firebase apps
    FirebaseCore_GetAllApps: function() {
        try {
            if (typeof firebase === 'undefined') {
                console.error('[FirebaseCore] Firebase SDK not loaded');
                return 0;
            }
            
            var apps = firebase.apps;
            var appsInfo = apps.map(function(app) {
                return {
                    name: app.name,
                    projectId: app.options.projectId,
                    apiKey: app.options.apiKey,
                    appId: app.options.appId,
                    isInitialized: true
                };
            });
            
            var result = JSON.stringify(appsInfo);
            var buffer = _malloc(lengthBytesUTF8(result) + 1);
            stringToUTF8(result, buffer, lengthBytesUTF8(result) + 1);
            return buffer;
            
        } catch (error) {
            console.error('[FirebaseCore] Failed to get all apps:', error);
            return 0;
        }
    },

    // Delete Firebase app
    FirebaseCore_DeleteApp: function(appNamePtr) {
        try {
            var appName = appNamePtr ? UTF8ToString(appNamePtr) : undefined;
            
            if (typeof firebase === 'undefined') {
                console.error('[FirebaseCore] Firebase SDK not loaded');
                return false;
            }
            
            var app = firebase.app(appName);
            if (app) {
                firebase.deleteApp(app);
                return true;
            }
            return false;
            
        } catch (error) {
            console.error('[FirebaseCore] Failed to delete app:', error);
            return false;
        }
    },

    // Check if app is initialized
    FirebaseCore_IsAppInitialized: function(appNamePtr) {
        try {
            var appName = appNamePtr ? UTF8ToString(appNamePtr) : undefined;
            
            if (typeof firebase === 'undefined') {
                return false;
            }
            
            var app = firebase.app(appName);
            return app != null;
        } catch (error) {
            return false;
        }
    }
};

mergeInto(LibraryManager.library, FirebaseCorePluginLib);
