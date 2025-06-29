var ViewportDetectionPlugin = {
    
    // Get the user agent string
    GetUserAgentJS: function() {
        var userAgent = navigator.userAgent || "";
        var bufferSize = lengthBytesUTF8(userAgent) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(userAgent, buffer, bufferSize);
        return buffer;
    },
    
    // Get comprehensive device information
    GetDeviceInfoJS: function() {
        var deviceInfo = {
            userAgent: navigator.userAgent || "",
            screenWidth: screen.width || 0,
            screenHeight: screen.height || 0,
            devicePixelRatio: window.devicePixelRatio || 1.0,
            isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent),
            isTablet: /(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(navigator.userAgent),
            platform: navigator.platform || "",
            touchSupport: ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0),
            availableWidth: screen.availWidth || 0,
            availableHeight: screen.availHeight || 0
        };
        
        var jsonString = JSON.stringify(deviceInfo);
        var bufferSize = lengthBytesUTF8(jsonString) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(jsonString, buffer, bufferSize);
        return buffer;
    },
    
    // Check if device is mobile
    IsMobileDeviceJS: function() {
        // Check user agent for mobile indicators
        var isMobileUA = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        
        // Check screen size (mobile typically < 768px width)
        var isMobileScreen = screen.width < 768 || window.innerWidth < 768;
        
        // Check touch support
        var hasTouchSupport = ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0);
        
        // Consider mobile if any condition is true
        return isMobileUA || (isMobileScreen && hasTouchSupport);
    },
    
    // Get screen orientation
    GetScreenOrientationJS: function() {
        var orientation = "landscape";
        
        // Try different methods to get orientation
        if (screen.orientation && screen.orientation.type) {
            orientation = screen.orientation.type.includes("portrait") ? "portrait" : "landscape";
        } else if (window.orientation !== undefined) {
            // window.orientation: 0 and 180 are portrait, 90 and -90 are landscape
            orientation = (Math.abs(window.orientation) === 90) ? "landscape" : "portrait";
        } else {
            // Fallback to comparing width and height
            orientation = (window.innerWidth > window.innerHeight) ? "landscape" : "portrait";
        }
        
        var bufferSize = lengthBytesUTF8(orientation) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(orientation, buffer, bufferSize);
        return buffer;
    }
};

mergeInto(LibraryManager.library, ViewportDetectionPlugin);
