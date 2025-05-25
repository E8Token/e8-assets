// Pre-script для создания неймспейса WebGLPluginPlatform
var WebGLPluginPlatform = WebGLPluginPlatform || {};

WebGLPluginPlatform.callbacks = {};
WebGLPluginPlatform.callbackCounter = 0;

WebGLPluginPlatform.generateCallbackId = function() {
    return "callback_" + (++WebGLPluginPlatform.callbackCounter);
};

WebGLPluginPlatform.registerCallback = function(callback) {
    var callbackId = WebGLPluginPlatform.generateCallbackId();
    WebGLPluginPlatform.callbacks[callbackId] = callback;
    return callbackId;
};

WebGLPluginPlatform.executeCallback = function(callbackId, response) {
    if (WebGLPluginPlatform.callbacks[callbackId]) {
        WebGLPluginPlatform.callbacks[callbackId](response);
        delete WebGLPluginPlatform.callbacks[callbackId];
    }
};

// Функция для обработки ответов от Unity
WebGLPluginPlatform.handleUnityResponse = function(callbackId, response) {
    if (WebGLPluginPlatform.callbacks[callbackId]) {
        try {
            var parsedResponse = JSON.parse(response);
            WebGLPluginPlatform.callbacks[callbackId](parsedResponse);
        } catch (e) {
            console.error('Error parsing Unity response:', e);
            WebGLPluginPlatform.callbacks[callbackId]({success: false, error: e.message});
        }
        delete WebGLPluginPlatform.callbacks[callbackId];
    }
};

// Функции-обертки для удобного использования в JavaScript
WebGLPluginPlatform.call = function(pluginName, methodName, data) {
    var jsonData = JSON.stringify(data || {});
    var result = WebGLPluginPlatformLib.PluginManager_Call(
        stringToNewUTF8(pluginName),
        stringToNewUTF8(methodName),
        stringToNewUTF8(jsonData)
    );
    return JSON.parse(UTF8ToString(result));
};

WebGLPluginPlatform.callAsync = function(pluginName, methodName, data, callback) {
    var jsonData = JSON.stringify(data || {});
    
    var callbackId = WebGLPluginPlatformLib.PluginManager_CallAsync(
        stringToNewUTF8(pluginName),
        stringToNewUTF8(methodName),
        stringToNewUTF8(jsonData)
    );
    
    callbackId = UTF8ToString(callbackId);
    
    if (callback) {
        WebGLPluginPlatform.callbacks[callbackId] = callback;
    }
    
    return callbackId;
};
