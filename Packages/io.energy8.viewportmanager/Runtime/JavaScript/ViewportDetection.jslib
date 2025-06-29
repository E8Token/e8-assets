// ViewportDetection Plugin JavaScript Library
mergeInto(LibraryManager.library, {
    ViewportDetectionGetUserAgent: function() {
        try {
            var userAgent = navigator.userAgent || "";
            var buffer = _malloc(lengthBytesUTF8(userAgent) + 1);
            stringToUTF8(userAgent, buffer, lengthBytesUTF8(userAgent) + 1);
            return buffer;
        } catch (e) {
            console.error('ViewportDetection: Error getting user agent:', e);
            var empty = "";
            var buffer = _malloc(lengthBytesUTF8(empty) + 1);
            stringToUTF8(empty, buffer, lengthBytesUTF8(empty) + 1);
            return buffer;
        }
    },
    
    ViewportDetectionGetScreenInfo: function() {
        try {
            var info = {
                width: screen.width || window.innerWidth,
                height: screen.height || window.innerHeight,
                availWidth: screen.availWidth || window.innerWidth,
                availHeight: screen.availHeight || window.innerHeight,
                devicePixelRatio: window.devicePixelRatio || 1,
                orientationAngle: screen.orientation ? screen.orientation.angle : 0,
                isTouchDevice: ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0),
                dpi: 96, // Default DPI for web
                orientation: (screen.width || window.innerWidth) > (screen.height || window.innerHeight) ? "Landscape" : "Portrait"
            };
            
            var infoString = JSON.stringify(info);
            var buffer = _malloc(lengthBytesUTF8(infoString) + 1);
            stringToUTF8(infoString, buffer, lengthBytesUTF8(infoString) + 1);
            return buffer;
        } catch (e) {
            console.error('ViewportDetection: Error getting screen info:', e);
            var empty = "{}";
            var buffer = _malloc(lengthBytesUTF8(empty) + 1);
            stringToUTF8(empty, buffer, lengthBytesUTF8(empty) + 1);
            return buffer;
        }
    },
    
    ViewportDetectionIsTouchDevice: function() {
        try {
            return ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0) ? 1 : 0;
        } catch (e) {
            console.error('ViewportDetection: Error detecting touch device:', e);
            return 0;
        }
    },
    
    ViewportDetectionDetectDeviceType: function() {
        try {
            var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
            var isTouch = ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0);
            var isSmallScreen = (screen.width || window.innerWidth) < 1024;
            
            var deviceType = (isMobile || (isTouch && isSmallScreen)) ? "Mobile" : "Desktop";
            var buffer = _malloc(lengthBytesUTF8(deviceType) + 1);
            stringToUTF8(deviceType, buffer, lengthBytesUTF8(deviceType) + 1);
            return buffer;
        } catch (e) {
            console.error('ViewportDetection: Error detecting device type:', e);
            var fallback = "Desktop";
            var buffer = _malloc(lengthBytesUTF8(fallback) + 1);
            stringToUTF8(fallback, buffer, lengthBytesUTF8(fallback) + 1);
            return buffer;
        }
    }
});
