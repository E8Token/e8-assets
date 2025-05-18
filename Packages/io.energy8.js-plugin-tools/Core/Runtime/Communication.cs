using System;
using UnityEngine;

namespace Energy8.JSPluginTools.Core
{
    /// <summary>
    /// Defines a message structure for communication between JavaScript and Unity.
    /// </summary>
    [Serializable]
    public class JSMessage
    {
        /// <summary>
        /// Gets or sets the ID of the target module.
        /// </summary>
        public string moduleId;
        
        /// <summary>
        /// Gets or sets the action to perform.
        /// </summary>
        public string action;
        
        /// <summary>
        /// Gets or sets the data payload as a JSON string.
        /// </summary>
        public string data;
        
        /// <summary>
        /// Gets or sets an optional callback ID for asynchronous operations.
        /// </summary>
        public string callbackId;
    }
    
    /// <summary>
    /// Defines the contract for plugin modules that can handle messages from JavaScript.
    /// </summary>
    public interface IJSMessageHandler
    {
        /// <summary>
        /// Handles a message received from JavaScript.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        void HandleJSMessage(JSMessage message);
    }
    
    /// <summary>
    /// Интерфейс для обработчика вызовов JavaScript функций
    /// </summary>
    public interface IJSCallHandler
    {
        /// <summary>
        /// Обрабатывает вызов JavaScript функции
        /// </summary>
        /// <param name="functionPath">Путь к вызываемой функции</param>
        /// <param name="args">Аргументы функции</param>
        void HandleCall(string functionPath, object[] args);
    }
      /// <summary>
    /// Provides utilities for communicating between Unity and JavaScript.
    /// </summary>
    public static class ExternalCommunicator
    {
        // Обработчик вызовов JavaScript функций; по умолчанию это стандартный обработчик
        private static IJSCallHandler _callHandler;
        
        // Инициализация стандартного обработчика при первой загрузке класса
        static ExternalCommunicator()
        {
            _callHandler = CreateDefaultHandler();
        }
        
        // Создает стандартный обработчик вызовов JavaScript
        private static IJSCallHandler CreateDefaultHandler()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGLJSCallHandler();
            #else
            return new EditorJSCallHandler();
            #endif
        }
        
        /// <summary>
        /// Устанавливает обработчик вызовов JavaScript (для тестирования)
        /// </summary>
        /// <param name="handler">Обработчик вызовов JavaScript или null для сброса к стандартному</param>
        internal static void SetCallHandler(IJSCallHandler handler)
        {
            _callHandler = handler ?? CreateDefaultHandler();
        }
        
        /// <summary>
        /// Calls a JavaScript function from Unity.
        /// </summary>
        /// <param name="functionPath">The fully qualified path to the JavaScript function.</param>
        /// <param name="args">Optional arguments to pass to the JavaScript function.</param>
        public static void CallJS(string functionPath, params object[] args)
        {
            _callHandler.HandleCall(functionPath, args);
        }
        
        #region Внутренние обработчики
        // Обработчик для WebGL сборки
        private class WebGLJSCallHandler : IJSCallHandler
        {
            public void HandleCall(string functionPath, object[] args)
            {
                try
                {
                    string argsJson = args.Length > 0 ? JsonUtility.ToJson(args) : "[]";
                    Application.ExternalCall("eval", $"{functionPath}({argsJson});");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JSPluginTools] Error calling JavaScript: {ex.Message}");
                }
            }
        }
        
        // Обработчик для редактора Unity
        private class EditorJSCallHandler : IJSCallHandler
        {
            public void HandleCall(string functionPath, object[] args)
            {
                Debug.Log($"[JSPluginTools] Simulating call to JavaScript: {functionPath}");
            }
        }
        #endregion
        
        /// <summary>
        /// Sends a message to JavaScript.
        /// </summary>
        /// <param name="moduleId">The ID of the sending module.</param>
        /// <param name="action">The action to perform.</param>
        /// <param name="data">The data payload as a JSON string.</param>
        /// <param name="callbackId">An optional callback ID for asynchronous operations.</param>
        public static void SendMessageToJS(string moduleId, string action, string data = null, string callbackId = null)
        {
            var message = new JSMessage
            {
                moduleId = moduleId,
                action = action,
                data = data,
                callbackId = callbackId
            };
            
            string json = JsonUtility.ToJson(message);
            CallJS("UnityWebPlugin.Core.receiveMessageFromUnity", json);
        }
    }
}
