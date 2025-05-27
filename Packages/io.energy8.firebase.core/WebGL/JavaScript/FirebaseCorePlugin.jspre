// Firebase Core Plugin JavaScript Pre-file
// Creates global namespace for convenient Firebase Core access

var FirebaseCorePlugin = FirebaseCorePlugin || {};

// Easy-to-use wrapper functions
FirebaseCorePlugin.initializeApp = function(config, appName, callback) {
    var data = {
        config: config,
        appName: appName || null
    };
    
    if (callback) {
        // Async call with callback
        WebGLPluginPlatform.callAsync('FirebaseCorePlugin', 'initializeApp', data, callback);
    } else {
        // Sync call
        return WebGLPluginPlatform.call('FirebaseCorePlugin', 'initializeApp', data);
    }
};

FirebaseCorePlugin.getApp = function(appName, callback) {
    var data = {
        appName: appName || null
    };
    
    if (callback) {
        WebGLPluginPlatform.callAsync('FirebaseCorePlugin', 'getApp', data, callback);
    } else {
        return WebGLPluginPlatform.call('FirebaseCorePlugin', 'getApp', data);
    }
};

FirebaseCorePlugin.deleteApp = function(appName, callback) {
    var data = {
        appName: appName || null
    };
    
    if (callback) {
        WebGLPluginPlatform.callAsync('FirebaseCorePlugin', 'deleteApp', data, callback);
    } else {
        return WebGLPluginPlatform.call('FirebaseCorePlugin', 'deleteApp', data);
    }
};

FirebaseCorePlugin.getAllApps = function(callback) {
    var data = {};
    
    if (callback) {
        WebGLPluginPlatform.callAsync('FirebaseCorePlugin', 'getAllApps', data, callback);
    } else {
        return WebGLPluginPlatform.call('FirebaseCorePlugin', 'getAllApps', data);
    }
};

FirebaseCorePlugin.isAppInitialized = function(appName, callback) {
    var data = {
        appName: appName || null
    };
    
    if (callback) {
        WebGLPluginPlatform.callAsync('FirebaseCorePlugin', 'isAppInitialized', data, callback);
    } else {
        return WebGLPluginPlatform.call('FirebaseCorePlugin', 'isAppInitialized', data);
    }
};

// Direct library functions for advanced use
FirebaseCorePlugin.initializeAppDirect = function(config, appName) {
    return FirebaseCore_InitializeApp(config, appName);
};

FirebaseCorePlugin.getAppDirect = function(appName) {
    return FirebaseCore_GetApp(appName);
};

FirebaseCorePlugin.getAllAppsDirect = function() {
    return FirebaseCore_GetAllApps();
};

FirebaseCorePlugin.deleteAppDirect = function(appName) {
    return FirebaseCore_DeleteApp(appName);
};

FirebaseCorePlugin.isAppInitializedDirect = function(appName) {
    return FirebaseCore_IsAppInitialized(appName);
};
