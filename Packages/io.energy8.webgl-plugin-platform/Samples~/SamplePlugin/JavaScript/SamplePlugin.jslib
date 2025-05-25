var SamplePluginLib = {
    // Получить сообщение от Unity
    SamplePlugin_GetMessage: function(namePtr) {
        var name = UTF8ToString(namePtr);
        
        try {
            var result = WebGLPluginPlatform.call('SamplePlugin', 'GetMessage', { name: name });
            return stringToNewUTF8(result.data || result.error || 'Unknown error');
        } catch (e) {
            console.error('Error getting message:', e);
            return stringToNewUTF8('Error: ' + e.message);
        }
    },
    
    // Асинхронное получение сообщения от Unity
    SamplePlugin_GetAsyncMessage: function(namePtr, callbackPtr) {
        var name = UTF8ToString(namePtr);
        
        WebGLPluginPlatform.callAsync('SamplePlugin', 'GetAsyncMessage', { name: name }, function(result) {
            if (callbackPtr) {
                var response = result.data || result.error || 'Unknown error';
                Module.dynCall_vi(callbackPtr, stringToNewUTF8(response));
            }
        });
    },
    
    // Установить режим отладки
    SamplePlugin_SetDebugMode: function(enabled) {
        try {
            var result = WebGLPluginPlatform.call('SamplePlugin', 'SetDebugMode', { enabled: enabled });
            return result.success ? 1 : 0;
        } catch (e) {
            console.error('Error setting debug mode:', e);
            return 0;
        }
    },
    
    // Получить информацию о плагине
    SamplePlugin_GetPluginInfo: function() {
        try {
            var result = WebGLPluginPlatform.call('SamplePlugin', 'getPluginInfo', {});
            return stringToNewUTF8(result.data || result.error || 'Unknown error');
        } catch (e) {
            console.error('Error getting plugin info:', e);
            return stringToNewUTF8('Error: ' + e.message);
        }
    }
};

// Функции-обертки для удобного использования
SamplePlugin.getMessage = function(name) {
    var result = SamplePluginLib.SamplePlugin_GetMessage(stringToNewUTF8(name));
    return UTF8ToString(result);
};

SamplePlugin.getAsyncMessage = function(name, callback) {
    var callbackWrapper = Runtime.addFunction(function(responsePtr) {
        var response = UTF8ToString(responsePtr);
        if (callback) {
            callback(response);
        }
    }, 'vi');
    
    SamplePluginLib.SamplePlugin_GetAsyncMessage(stringToNewUTF8(name), callbackWrapper);
};

SamplePlugin.setDebugMode = function(enabled) {
    return SamplePluginLib.SamplePlugin_SetDebugMode(enabled ? 1 : 0) === 1;
};

SamplePlugin.getPluginInfo = function() {
    var result = SamplePluginLib.SamplePlugin_GetPluginInfo();
    return UTF8ToString(result);
};

mergeInto(LibraryManager.library, SamplePluginLib);
