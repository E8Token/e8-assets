using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

namespace Energy8.WebGL.PluginPlatform
{
    /// <summary>
    /// Центральный менеджер для управления всеми WebGL плагинами
    /// </summary>
    public class PluginManager : MonoBehaviour
    {
        private static PluginManager instance;
        private Dictionary<string, BasePlugin> registeredPlugins = new Dictionary<string, BasePlugin>();
        private Dictionary<string, Action<string>> callbacks = new Dictionary<string, Action<string>>();
        private int callbackCounter = 0;

        /// <summary>
        /// Синглтон экземпляр PluginManager
        /// </summary>
        public static PluginManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PluginManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("PluginManager");
                        instance = go.AddComponent<PluginManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Получить список всех зарегистрированных плагинов
        /// </summary>
        public IReadOnlyDictionary<string, BasePlugin> RegisteredPlugins => registeredPlugins;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Принудительная инициализация синглтона
            _ = Instance;
        }

        // Импорт JavaScript функций
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PluginManager_HandleCallback(string callbackId, string response);
#endif
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                ScanForPlugins();
                InitializePlugins();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// Автоматическое сканирование всех плагинов в проекте
        /// </summary>
        private void ScanForPlugins()
        {
            // Поиск всех BasePlugin в сцене
            var pluginsInScene = FindObjectsByType<BasePlugin>(FindObjectsSortMode.None);
            foreach (var plugin in pluginsInScene)
            {
                RegisterPlugin(plugin);
            }

            Debug.Log($"Found {pluginsInScene.Length} plugins in scene");
        }

        /// <summary>
        /// Обновление списка плагинов (для GUI)
        /// </summary>
        public void RefreshPlugins()
        {
            ScanForPlugins();
        }

        /// <summary>
        /// Регистрация плагина
        /// </summary>
        public void RegisterPlugin(BasePlugin plugin)
        {
            if (plugin == null) return;

            string pluginName = plugin.PluginName;
            if (registeredPlugins.ContainsKey(pluginName))
            {
                Debug.LogWarning($"Plugin {pluginName} already registered");
                return;
            }

            registeredPlugins[pluginName] = plugin;

            if (plugin.IsEnabled)
            {
                plugin.Initialize();
                plugin.Enable();
            }

            Debug.Log($"Plugin {pluginName} registered with priority {plugin.Priority}");
        }

        /// <summary>
        /// Отмена регистрации плагина
        /// </summary>
        public void UnregisterPlugin(BasePlugin plugin)
        {
            if (plugin == null) return;

            string pluginName = plugin.PluginName;
            if (registeredPlugins.ContainsKey(pluginName))
            {
                plugin.Disable();
                plugin.Destroy();
                registeredPlugins.Remove(pluginName);
                Debug.Log($"Plugin {pluginName} unregistered");
            }
        }

        /// <summary>
        /// Включение плагина
        /// </summary>
        public void EnablePlugin(string pluginName)
        {
            if (registeredPlugins.TryGetValue(pluginName, out BasePlugin plugin))
            {
                plugin.IsEnabled = true;
                plugin.Enable();
            }
        }

        /// <summary>
        /// Выключение плагина
        /// </summary>
        public void DisablePlugin(string pluginName)
        {
            if (registeredPlugins.TryGetValue(pluginName, out BasePlugin plugin))
            {
                plugin.IsEnabled = false;
                plugin.Disable();
            }
        }

        /// <summary>
        /// Вызов метода плагина из JavaScript
        /// </summary>
        public string CallPluginMethod(string pluginName, string methodName, string jsonData)
        {
            try
            {
                if (!registeredPlugins.TryGetValue(pluginName, out BasePlugin plugin))
                {
                    return CreateErrorResponse("Plugin not found");
                }

                if (!plugin.IsEnabled)
                {
                    return CreateErrorResponse("Plugin disabled");
                }

                // Поиск метода с атрибутом JSCallable
                var method = FindJSCallableMethod(plugin, methodName);
                if (method == null)
                {
                    return CreateErrorResponse("Method not found or not JSCallable");
                }

                // Десериализация параметров
                var parameters = DeserializeParameters(method, jsonData);

                // Вызов метода
                var result = method.Invoke(plugin, parameters);

                return CreateSuccessResponse(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error calling plugin method: {ex.Message}");
                return CreateErrorResponse(ex.Message);
            }
        }        /// <summary>
                 /// Асинхронный вызов метода плагина из JavaScript
                 /// </summary>
        public string CallPluginMethodAsync(string pluginName, string methodName, string jsonData, string callbackId)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = CallPluginMethod(pluginName, methodName, jsonData);

                    // Отправка результата через callback в JavaScript
#if UNITY_WEBGL && !UNITY_EDITOR
                    PluginManager_HandleCallback(callbackId, result);
#endif
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in async plugin method call: {ex.Message}");
#if UNITY_WEBGL && !UNITY_EDITOR
                    PluginManager_HandleCallback(callbackId, CreateErrorResponse(ex.Message));
#endif
                }
            });

            return CreateSuccessResponse("Async call initiated");
        }
        // Методы для вызова из JavaScript - используем [MonoPInvokeCallback]
        [MonoPInvokeCallback(typeof(System.Func<string, string, string, string>))]
        public static string PluginManager_CallMethod(string pluginName, string methodName, string jsonData)
        {
            return Instance.CallPluginMethod(pluginName, methodName, jsonData);
        }

        [MonoPInvokeCallback(typeof(System.Func<string, string, string, string, string>))]
        public static string PluginManager_CallMethodAsync(string pluginName, string methodName, string jsonData, string callbackId)
        {
            return Instance.CallPluginMethodAsync(pluginName, methodName, jsonData, callbackId);
        }

        /// <summary>
        /// Регистрация callback для асинхронных вызовов
        /// </summary>
        public string RegisterCallback(Action<string> callback)
        {
            string callbackId = $"callback_{callbackCounter++}";
            callbacks[callbackId] = callback;
            return callbackId;
        }

        private void InitializePlugins()
        {
            // Инициализация плагинов по приоритету
            var sortedPlugins = registeredPlugins.Values
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.Priority)
                .ToList();

            foreach (var plugin in sortedPlugins)
            {
                plugin.Initialize();
                plugin.Enable();
            }
        }

        private MethodInfo FindJSCallableMethod(BasePlugin plugin, string methodName)
        {
            var methods = plugin.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<JSCallableAttribute>();
                if (attr != null)
                {
                    string targetName = string.IsNullOrEmpty(attr.MethodName) ? method.Name : attr.MethodName;
                    if (targetName == methodName)
                    {
                        return method;
                    }
                }
            }

            return null;
        }
        private object[] DeserializeParameters(MethodInfo method, string jsonData)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                return new object[0];

            if (string.IsNullOrEmpty(jsonData))
                return new object[parameters.Length];

            // Try to parse as a named parameter JSON object first
            try
            {
                // For single parameter methods, try to extract the parameter value directly
                if (parameters.Length == 1)
                {
                    var paramName = parameters[0].Name;

                    // Simple parsing for JSON objects like {"name":"Unity"}
                    if (jsonData.StartsWith("{") && jsonData.Contains($"\"{paramName}\""))
                    {
                        // Extract value using simple string parsing
                        var startIndex = jsonData.IndexOf($"\"{paramName}\"") + paramName.Length + 3; // +3 for ":"
                        var valueStart = jsonData.IndexOf("\"", startIndex) + 1;
                        var valueEnd = jsonData.IndexOf("\"", valueStart);

                        if (valueStart > 0 && valueEnd > valueStart)
                        {
                            var value = jsonData.Substring(valueStart, valueEnd - valueStart);
                            return new object[] { ConvertParameter(value, parameters[0].ParameterType) };
                        }
                    }
                }
            }
            catch
            {
                // Fall back to array-based deserialization
            }

            // Fallback: try array-based parameter format
            try
            {
                var data = JsonUtility.FromJson<ParameterData>(jsonData);
                var result = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (data != null && data.parameters != null && i < data.parameters.Length)
                    {
                        result[i] = ConvertParameter(data.parameters[i], parameters[i].ParameterType);
                    }
                    else
                    {
                        result[i] = GetDefaultValue(parameters[i].ParameterType);
                    }
                }

                return result;
            }
            catch
            {
                // Final fallback: return default values
                var result = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    result[i] = GetDefaultValue(parameters[i].ParameterType);
                }
                return result;
            }
        }

        private object ConvertParameter(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(int))
                return int.TryParse(value, out int intVal) ? intVal : 0;
            if (targetType == typeof(float))
                return float.TryParse(value, out float floatVal) ? floatVal : 0f;
            if (targetType == typeof(bool))
                return bool.TryParse(value, out bool boolVal) ? boolVal : false;

            return GetDefaultValue(targetType);
        }

        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private string CreateSuccessResponse(object data)
        {
            var response = new ResponseData
            {
                success = true,
                data = data?.ToString() ?? "",
                error = ""
            };
            return JsonUtility.ToJson(response);
        }

        private string CreateErrorResponse(string error)
        {
            var response = new ResponseData
            {
                success = false,
                data = "",
                error = error
            };
            return JsonUtility.ToJson(response);
        }
        [Serializable]
        private class JSCallMessage
        {
            public string plugin;
            public string method;
            public object data;
        }

        [Serializable]
        private class JSCallAsyncMessage
        {
            public string plugin;
            public string method;
            public object data;
            public string callbackId;
        }

        [Serializable]
        private class ParameterData
        {
            public string[] parameters;
        }

        [Serializable]
        private class ResponseData
        {
            public bool success;
            public string data;
            public string error;
        }

        /// <summary>
        /// Обработка вызова от JavaScript через SendMessage
        /// </summary>
        public void HandleJSCall(string messageJson)
        {
            try
            {
                var message = JsonUtility.FromJson<JSCallMessage>(messageJson);
                var result = CallPluginMethod(message.plugin, message.method, JsonUtility.ToJson(message.data));

                // Для синхронных вызовов результат просто логируется
                Debug.Log($"JS Call Result: {result}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling JS call: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработка асинхронного вызова от JavaScript через SendMessage
        /// </summary>
        public void HandleJSCallAsync(string messageJson)
        {
            try
            {
                var message = JsonUtility.FromJson<JSCallAsyncMessage>(messageJson);

                Task.Run(() =>
                {
                    try
                    {
                        var result = CallPluginMethod(message.plugin, message.method, JsonUtility.ToJson(message.data));

                        // Отправка результата обратно в JavaScript
#if UNITY_WEBGL && !UNITY_EDITOR
                        Application.ExternalCall("WebGLPluginPlatform.handleUnityResponse", message.callbackId, result);
#endif
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = CreateErrorResponse(ex.Message);
#if UNITY_WEBGL && !UNITY_EDITOR
                        Application.ExternalCall("WebGLPluginPlatform.handleUnityResponse", message.callbackId, errorResponse);
#endif
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling async JS call: {ex.Message}");
            }
        }
    }
}
