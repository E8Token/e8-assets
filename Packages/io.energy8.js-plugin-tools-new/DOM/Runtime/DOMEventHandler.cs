using System;
using System.Collections.Generic;
using UnityEngine;
using Energy8.JSPluginTools.Core;
using Newtonsoft.Json;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// MonoBehaviour компонент для обработки событий DOM
    /// </summary>
    [AddComponentMenu("JSPluginTools/DOM Event Handler")]
    public class DOMEventHandler : MonoBehaviour
    {
        private IPluginCore _core;
        private string _objectId;
        private readonly Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();
        
        /// <summary>
        /// Инициализирует обработчик событий DOM
        /// </summary>
        /// <param name="core">Экземпляр ядра плагина</param>
        public void Initialize(IPluginCore core)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            
            // Регистрируем GameObject для получения событий от JavaScript
            _objectId = _core.RegisterGameObject(gameObject.name);
            
            Debug.Log($"JSPluginTools [DOMEventHandler]: Initialized with ID: {_objectId}");
        }
        
        /// <summary>
        /// Регистрирует обработчик события
        /// </summary>
        /// <param name="callbackName">Имя метода обратного вызова</param>
        public void RegisterCallback(string callbackName)
        {
            if (!_callbacks.ContainsKey(callbackName))
            {
                _callbacks[callbackName] = null;
                Debug.Log($"JSPluginTools [DOMEventHandler]: Registered callback '{callbackName}'");
            }
        }
        
        /// <summary>
        /// Отменяет регистрацию обработчика события
        /// </summary>
        /// <param name="callbackName">Имя метода обратного вызова</param>
        public void UnregisterCallback(string callbackName)
        {
            if (_callbacks.ContainsKey(callbackName))
            {
                _callbacks.Remove(callbackName);
                Debug.Log($"JSPluginTools [DOMEventHandler]: Unregistered callback '{callbackName}'");
            }
        }
        
        /// <summary>
        /// Подписывает обработчик на событие
        /// </summary>
        /// <param name="callbackName">Имя метода обратного вызова</param>
        /// <param name="handler">Обработчик события</param>
        public void AddEventHandler(string callbackName, Action<string> handler)
        {
            if (_callbacks.ContainsKey(callbackName))
            {
                _callbacks[callbackName] += handler;
                Debug.Log($"JSPluginTools [DOMEventHandler]: Added handler for callback '{callbackName}'");
            }
            else
            {
                Debug.LogWarning($"JSPluginTools [DOMEventHandler]: Callback '{callbackName}' is not registered");
            }
        }
        
        /// <summary>
        /// Отписывает обработчик от события
        /// </summary>
        /// <param name="callbackName">Имя метода обратного вызова</param>
        /// <param name="handler">Обработчик события</param>
        public void RemoveEventHandler(string callbackName, Action<string> handler)
        {
            if (_callbacks.ContainsKey(callbackName))
            {
                _callbacks[callbackName] -= handler;
                Debug.Log($"JSPluginTools [DOMEventHandler]: Removed handler for callback '{callbackName}'");
            }
            else
            {
                Debug.LogWarning($"JSPluginTools [DOMEventHandler]: Callback '{callbackName}' is not registered");
            }
        }
        
        /// <summary>
        /// Метод, вызываемый из JavaScript при возникновении события
        /// </summary>
        /// <param name="eventData">Данные события в формате JSON</param>
        public void HandleDOMEvent(string eventData)
        {
            if (string.IsNullOrEmpty(eventData))
            {
                Debug.LogWarning("JSPluginTools [DOMEventHandler]: Received empty event data");
                return;
            }
            
            try
            {
                var eventInfo = JsonConvert.DeserializeObject<DOMEventInfo>(eventData);
                
                if (eventInfo == null || string.IsNullOrEmpty(eventInfo.CallbackName))
                {
                    Debug.LogWarning("JSPluginTools [DOMEventHandler]: Invalid event data format");
                    return;
                }
                
                if (_callbacks.TryGetValue(eventInfo.CallbackName, out var handler))
                {
                    if (handler != null)
                    {
                        handler.Invoke(eventInfo.EventData);
                    }
                    else
                    {
                        Debug.LogWarning($"JSPluginTools [DOMEventHandler]: No handlers registered for callback '{eventInfo.CallbackName}'");
                    }
                }
                else
                {
                    Debug.LogWarning($"JSPluginTools [DOMEventHandler]: Callback '{eventInfo.CallbackName}' not found");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOMEventHandler]: Error processing event: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            if (_core != null && !string.IsNullOrEmpty(_objectId))
            {
                _core.UnregisterGameObject(_objectId);
                Debug.Log($"JSPluginTools [DOMEventHandler]: Unregistered with ID: {_objectId}");
            }
        }
    }
    
    /// <summary>
    /// Информация о событии DOM
    /// </summary>
    [Serializable]
    public class DOMEventInfo
    {
        [JsonProperty("callbackName")]
        public string CallbackName { get; set; }
        
        [JsonProperty("eventData")]
        public string EventData { get; set; }
    }
}