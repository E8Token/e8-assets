// Pre-script для создания неймспейса SamplePlugin
var SamplePlugin = SamplePlugin || {};

// Константа с названием плагина
SamplePlugin.PLUGIN_NAME = 'SamplePlugin';

// Лямбда-функции для вызовов WebGLPluginPlatform
SamplePlugin.getMessage = (name) => WebGLPluginPlatform.call(SamplePlugin.PLUGIN_NAME, 'GetMessage', { name: name });

SamplePlugin.getAsyncMessage = (name, callback) => WebGLPluginPlatform.callAsync(SamplePlugin.PLUGIN_NAME, 'GetAsyncMessage', { name: name }, callback);

SamplePlugin.setDebugMode = (enabled) => WebGLPluginPlatform.call(SamplePlugin.PLUGIN_NAME, 'SetDebugMode', { enabled: enabled });

SamplePlugin.getPluginInfo = () => WebGLPluginPlatform.call(SamplePlugin.PLUGIN_NAME, 'getPluginInfo', {});
