var WebGLPluginPlatformLib = {
    $WebGLPluginPlatform: null,
    PluginManager_Call: function (pluginNamePtr, methodNamePtr, jsonDataPtr) {
        var pluginName = UTF8ToString(pluginNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var jsonData = UTF8ToString(jsonDataPtr);

        try {
            var message = JSON.stringify({
                plugin: pluginName,
                method: methodName,
                data: JSON.parse(jsonData || '{}')
            });

            SendMessage('PluginManager', 'HandleJSCall', message);

            var successResponse = JSON.stringify({
                success: true,
                data: 'Call sent'
            });
            return stringToNewUTF8(successResponse);
        } catch (e) {
            console.error('Error calling plugin method:', e);
            var errorResponse = JSON.stringify({
                success: false,
                error: e.message
            }); return stringToNewUTF8(errorResponse);
        }
    },
    PluginManager_CallAsync__deps: ['$WebGLPluginPlatform'],

    PluginManager_CallAsync: function (pluginNamePtr, methodNamePtr, jsonDataPtr) {
        var pluginName = UTF8ToString(pluginNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var jsonData = UTF8ToString(jsonDataPtr);

        var callbackId = WebGLPluginPlatform.generateCallbackId();

        try {
            var message = JSON.stringify({
                plugin: pluginName,
                method: methodName,
                data: JSON.parse(jsonData || '{}'),
                callbackId: callbackId
            });

            SendMessage('PluginManager', 'HandleJSCallAsync', message);

            return stringToNewUTF8(callbackId);
        } catch (e) {
            console.error('Error calling async plugin method:', e);
            var errorResponse = JSON.stringify({
                success: false,
                error: e.message
            }); return stringToNewUTF8(errorResponse);
        }
    },

    PluginManager_HandleCallback: function (callbackIdPtr, responsePtr) {
        var callbackId = UTF8ToString(callbackIdPtr);
        var response = UTF8ToString(responsePtr);

        WebGLPluginPlatform.handleUnityResponse(callbackId, response);
    },
    PluginManager_HandleCallback__deps: ['$WebGLPluginPlatform']
};

mergeInto(LibraryManager.library, WebGLPluginPlatformLib);
