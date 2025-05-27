var SamplePluginLib = {    // Получить сообщение от Unity
    SamplePlugin_GetMessage: function (namePtr) {
        var name = UTF8ToString(namePtr);
        try {
            var result = SamplePlugin.getMessage(name);
            return stringToNewUTF8(result.data || result.error || 'Unknown error');
        } catch (e) {
            console.error('Error getting message:', e);
            return stringToNewUTF8('Error: ' + e.message);
        }
    },
    // Асинхронное получение сообщения от Unity
    SamplePlugin_GetAsyncMessage: function (namePtr, callbackPtr) {
        var name = UTF8ToString(namePtr);

        SamplePlugin.getAsyncMessage(name, function (result) {
            if (callbackPtr) {
                var response = result.data || result.error || 'Unknown error';
                Module.dynCall_vi(callbackPtr, stringToNewUTF8(response));
            }
        });
    },
    // Установить режим отладки
    SamplePlugin_SetDebugMode: function (enabled) {
        try {
            var result = SamplePlugin.setDebugMode(enabled);
            return result.success ? 1 : 0;
        } catch (e) {
            console.error('Error setting debug mode:', e);
            return 0;
        }
    },
    // Получить информацию о плагине
    SamplePlugin_GetPluginInfo: function () {
        try {
            var result = SamplePlugin.getPluginInfo();
            return stringToNewUTF8(result.data || result.error || 'Unknown error');
        } catch (e) {
            console.error('Error getting plugin info:', e);
            return stringToNewUTF8('Error: ' + e.message);
        }
    }
};

mergeInto(LibraryManager.library, SamplePluginLib);
