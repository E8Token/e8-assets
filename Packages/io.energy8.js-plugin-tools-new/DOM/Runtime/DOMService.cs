using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Communication;
using Energy8.JSPluginTools.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// Implementation of IDOMService interface for manipulating browser DOM elements
    /// </summary>
    public class DOMService : IDOMService
    {
        private const string CHANNEL_PREFIX = "dom.";
        private readonly ICommunicationService _communicationService;
        private readonly Dictionary<string, Dictionary<string, Action<DOMEventData>>> _eventHandlers = 
            new Dictionary<string, Dictionary<string, Action<DOMEventData>>>();
        
        /// <summary>
        /// Creates a new DOMService instance
        /// </summary>
        /// <param name="communicationService">Communication service for interacting with JavaScript</param>
        public DOMService(ICommunicationService communicationService)
        {
            _communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
            
            // Register event handler to receive DOM events from JavaScript
            _communicationService.Subscribe<DOMEventMessage>(CHANNEL_PREFIX + "event", OnDOMEvent);
        }
        
        /// <inheritdoc/>
        public async Task<string> CreateElement(string tagName, string id = null, string className = null, string style = null, string parentId = null)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException("Tag name cannot be null or empty", nameof(tagName));
            }

            try
            {
                var request = new DOMRequest
                {
                    TagName = tagName,
                    Id = id,
                    ClassName = className,
                    Style = style,
                    ParentId = parentId
                };
                
                string response = await _communicationService.SendWithResponseAsync<DOMRequest, string>(
                    CHANNEL_PREFIX + "createElement",
                    request
                );
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error creating element: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> SetContent(string elementId, string content, bool append = false)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    Content = content,
                    Append = append
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "setContent",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error setting content: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> GetContent(string elementId)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId
                };
                
                string content = await _communicationService.SendWithResponseAsync<DOMRequest, string>(
                    CHANNEL_PREFIX + "getContent",
                    request
                );
                
                return content;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting content: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> SetAttribute(string elementId, string attributeName, string attributeValue)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    AttributeName = attributeName,
                    AttributeValue = attributeValue
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "setAttribute",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error setting attribute: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> GetAttribute(string elementId, string attributeName)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    AttributeName = attributeName
                };
                
                string value = await _communicationService.SendWithResponseAsync<DOMRequest, string>(
                    CHANNEL_PREFIX + "getAttribute",
                    request
                );
                
                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting attribute: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> SetStyle(string elementId, string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Style property name cannot be null or empty", nameof(propertyName));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    StyleProperty = propertyName,
                    StyleValue = propertyValue
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "setStyle",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error setting style: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> GetStyle(string elementId, string propertyName)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Style property name cannot be null or empty", nameof(propertyName));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    StyleProperty = propertyName
                };
                
                string value = await _communicationService.SendWithResponseAsync<DOMRequest, string>(
                    CHANNEL_PREFIX + "getStyle",
                    request
                );
                
                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting style: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> AddEventListener(string elementId, string eventType, Action<DOMEventData> eventHandler)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(eventType))
            {
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
            }
            
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            try
            {
                // Store the handler locally for when events are received from JavaScript
                if (!_eventHandlers.ContainsKey(elementId))
                {
                    _eventHandlers[elementId] = new Dictionary<string, Action<DOMEventData>>();
                }
                
                _eventHandlers[elementId][eventType] = eventHandler;
                
                var request = new DOMRequest
                {
                    Id = elementId,
                    EventType = eventType
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "addEventListener",
                    request
                );
                
                if (!success)
                {
                    // Clean up if the operation failed
                    _eventHandlers[elementId].Remove(eventType);
                    if (_eventHandlers[elementId].Count == 0)
                    {
                        _eventHandlers.Remove(elementId);
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error adding event listener: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> RemoveEventListener(string elementId, string eventType)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }
            
            if (string.IsNullOrEmpty(eventType))
            {
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
            }

            try
            {
                // Remove the local handler
                if (_eventHandlers.TryGetValue(elementId, out var elementHandlers))
                {
                    elementHandlers.Remove(eventType);
                    
                    if (elementHandlers.Count == 0)
                    {
                        _eventHandlers.Remove(elementId);
                    }
                }
                
                var request = new DOMRequest
                {
                    Id = elementId,
                    EventType = eventType
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "removeEventListener",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error removing event listener: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> RemoveElement(string elementId)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "removeElement",
                    request
                );
                
                // Clean up any registered event handlers
                if (success && _eventHandlers.ContainsKey(elementId))
                {
                    _eventHandlers.Remove(elementId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error removing element: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> SetVisible(string elementId, bool visible)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    Visible = visible
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "setVisible",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error setting visibility: {ex.Message}");
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<List<string>> QuerySelectorAll(string cssSelector)
        {
            if (string.IsNullOrEmpty(cssSelector))
            {
                throw new ArgumentException("CSS selector cannot be null or empty", nameof(cssSelector));
            }

            try
            {
                var request = new DOMRequest
                {
                    CssSelector = cssSelector
                };
                
                var elementIds = await _communicationService.SendWithResponseAsync<DOMRequest, List<string>>(
                    CHANNEL_PREFIX + "querySelectorAll",
                    request
                );
                
                return elementIds ?? new List<string>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error querying elements: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <inheritdoc/>
        public async Task<DOMElementPosition> GetSetPosition(string elementId, DOMElementPosition position = null)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId,
                    Position = position
                };
                
                var currentPosition = await _communicationService.SendWithResponseAsync<DOMRequest, DOMElementPosition>(
                    CHANNEL_PREFIX + "getSetPosition",
                    request
                );
                
                return currentPosition;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting/setting position: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<Vector2> GetElementSize(string elementId)
        {
            if (string.IsNullOrEmpty(elementId))
            {
                throw new ArgumentException("Element ID cannot be null or empty", nameof(elementId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = elementId
                };
                
                var size = await _communicationService.SendWithResponseAsync<DOMRequest, Vector2>(
                    CHANNEL_PREFIX + "getElementSize",
                    request
                );
                
                return size;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting element size: {ex.Message}");
                return Vector2.zero;
            }
        }
        
        /// <inheritdoc/>
        public async Task<string> CreateModal(ModalOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            try
            {
                var modalId = await _communicationService.SendWithResponseAsync<ModalOptions, string>(
                    CHANNEL_PREFIX + "createModal",
                    options
                );
                
                return modalId;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error creating modal: {ex.Message}");
                return null;
            }
        }
        
        /// <inheritdoc/>
        public async Task<bool> CloseModal(string modalId)
        {
            if (string.IsNullOrEmpty(modalId))
            {
                throw new ArgumentException("Modal ID cannot be null or empty", nameof(modalId));
            }

            try
            {
                var request = new DOMRequest
                {
                    Id = modalId
                };
                
                bool success = await _communicationService.SendWithResponseAsync<DOMRequest, bool>(
                    CHANNEL_PREFIX + "closeModal",
                    request
                );
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error closing modal: {ex.Message}");
                return false;
            }
        }
        
        // Handler for DOM events received from JavaScript
        private void OnDOMEvent(DOMEventMessage message)
        {
            try
            {
                if (_eventHandlers.TryGetValue(message.ElementId, out var elementHandlers))
                {
                    if (elementHandlers.TryGetValue(message.EventData.Type, out var handler))
                    {
                        handler.Invoke(message.EventData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error handling DOM event: {ex.Message}");
            }
        }
        
        // Internal request class for DOM operations
        [Serializable]
        private class DOMRequest
        {
            public string Id;
            public string TagName;
            public string ClassName;
            public string Style;
            public string ParentId;
            public string Content;
            public bool Append;
            public string AttributeName;
            public string AttributeValue;
            public string StyleProperty;
            public string StyleValue;
            public string EventType;
            public bool Visible;
            public string CssSelector;
            public DOMElementPosition Position;
        }
        
        // Class for DOM events received from JavaScript
        [Serializable]
        private class DOMEventMessage
        {
            public string ElementId;
            public DOMEventData EventData;
        }
    }
}