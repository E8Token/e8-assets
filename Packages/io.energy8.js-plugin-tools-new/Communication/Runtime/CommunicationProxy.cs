using System;
using Energy8.JSPluginTools.Core;
using UnityEngine;
using Newtonsoft.Json;

namespace Energy8.JSPluginTools.Communication
{
    /// <summary>
    /// Прокси-объект для получения сообщений от JavaScript и их перенаправления в CommunicationManager
    /// </summary>
    [AddComponentMenu("JSPluginTools/Communication Proxy")]
    public class CommunicationProxy : MonoBehaviour
    {
        private ICommunicationManager _communicationManager;
        private IPluginCore _pluginCore;
        private string _objectId;

        /// <summary>
        /// Инициализирует прокси с указанными зависимостями
        /// </summary>
        /// <param name="communicationManager">Экземпляр менеджера коммуникаций</param>
        /// <param name="pluginCore">Экземпляр ядра плагина</param>
        public void Initialize(ICommunicationManager communicationManager, IPluginCore pluginCore)
        {
            _communicationManager = communicationManager ?? throw new ArgumentNullException(nameof(communicationManager));
            _pluginCore = pluginCore ?? throw new ArgumentNullException(nameof(pluginCore));
            
            // Регистрируем объект для коммуникации с JavaScript
            _objectId = _pluginCore.RegisterGameObject(gameObject.name);
            
            Debug.Log($"JSPluginTools [CommunicationProxy]: Initialized with ID: {_objectId}");
        }

        private void OnDestroy()
        {
            if (_pluginCore != null && !string.IsNullOrEmpty(_objectId))
            {
                _pluginCore.UnregisterGameObject(_objectId);
                Debug.Log($"JSPluginTools [CommunicationProxy]: Unregistered with ID: {_objectId}");
            }
        }

        /// <summary>
        /// Метод обработки сообщений, приходящих от JavaScript
        /// </summary>
        /// <param name="jsonMessage">Сообщение в формате JSON</param>
        public void HandleMessageFromJS(string jsonMessage)
        {
            if (string.IsNullOrEmpty(jsonMessage))
            {
                Debug.LogWarning("JSPluginTools [CommunicationProxy]: Received empty message from JavaScript");
                return;
            }

            try
            {
                // Десериализуем сообщение
                var message = JsonConvert.DeserializeObject<ChannelMessage>(jsonMessage);
                
                if (message == null || string.IsNullOrEmpty(message.Channel))
                {
                    Debug.LogWarning("JSPluginTools [CommunicationProxy]: Invalid message format received from JavaScript");
                    return;
                }

                Debug.Log($"JSPluginTools [CommunicationProxy]: Received message on channel '{message.Channel}'");
                
                // Оставляем обработку фактических данных каналам, которые зарегистрированы
                // в CommunicationManager. Они получат данные через _messageBus.
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [CommunicationProxy]: Error processing message from JavaScript: {ex.Message}");
            }
        }

        [Serializable]
        private class ChannelMessage
        {
            [JsonProperty("channel")]
            public string Channel { get; set; }
            
            [JsonProperty("data")]
            public object Data { get; set; }
        }
    }
}