// ViewportDetection Plugin Namespace
var ViewportDetection = ViewportDetection || {};

// Easy-to-use wrapper functions
ViewportDetection.getUserAgent = function() {
    return WebGLPluginPlatform.call('ViewportDetection', 'GetUserAgent', {});
};

ViewportDetection.getScreenInfo = function() {
    var result = WebGLPluginPlatform.call('ViewportDetection', 'GetScreenInfo', {});
    try {
        return JSON.parse(result);
    } catch (e) {
        console.error('ViewportDetection: Error parsing screen info:', e);
        return {};
    }
};

ViewportDetection.isTouchDevice = function() {
    return WebGLPluginPlatform.call('ViewportDetection', 'IsTouchDevice', {});
};

ViewportDetection.detectDeviceType = function() {
    return WebGLPluginPlatform.call('ViewportDetection', 'DetectDeviceType', {});
};

// Enhanced detection with callback support
ViewportDetection.getDetailedInfo = function(callback) {
    var info = {
        userAgent: ViewportDetection.getUserAgent(),
        screenInfo: ViewportDetection.getScreenInfo(),
        isTouchDevice: ViewportDetection.isTouchDevice(),
        deviceType: ViewportDetection.detectDeviceType()
    };
    
    if (callback && typeof callback === 'function') {
        callback(info);
    }
    
    return info;
};

// Monitor viewport changes
ViewportDetection.onResize = function(callback) {
    if (callback && typeof callback === 'function') {
        window.addEventListener('resize', function() {
            callback({
                screenInfo: ViewportDetection.getScreenInfo(),
                deviceType: ViewportDetection.detectDeviceType()
            });
        });
    }
};

// Monitor orientation changes
ViewportDetection.onOrientationChange = function(callback) {
    if (callback && typeof callback === 'function') {
        window.addEventListener('orientationchange', function() {
            setTimeout(function() { // Wait for orientation to complete
                callback({
                    screenInfo: ViewportDetection.getScreenInfo(),
                    deviceType: ViewportDetection.detectDeviceType()
                });
            }, 100);
        });
    }
};
