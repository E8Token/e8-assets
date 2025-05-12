using System;
using System.Threading.Tasks;
using Energy8.JSPluginTools.Core;
using Energy8.JSPluginTools.Core.Implementation;
using Newtonsoft.Json;
using UnityEngine;

namespace Energy8.JSPluginTools.DOM
{
    /// <summary>
    /// Реализация интерфейса для работы с DOM-элементами веб-страницы
    /// </summary>
    public class DOMManager : IDOMManager
    {
        private IPluginCore _core;
        private IMessageBus _messageBus;
        private bool _isInitialized = false;

        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;

        /// <inheritdoc/>
        public event Action OnInitialized;

        private const string MESSAGE_PREFIX = "dom.";
        
        /// <summary>
        /// Объект для регистрации обработчиков событий DOM
        /// </summary>
        private DOMEventHandler _eventHandler;

        /// <inheritdoc/>
        public void Initialize(IPluginCore core)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("JSPluginTools [DOM]: Already initialized");
                return;
            }

            _core = core ?? throw new ArgumentNullException(nameof(core));
            
            if (_core is PluginCore pluginCore)
            {
                _messageBus = pluginCore.MessageBus;
            }
            else
            {
                throw new InvalidOperationException("JSPluginTools [DOM]: Core implementation does not provide access to MessageBus");
            }
            
            // Создаем и инициализируем обработчик событий DOM
            GameObject eventHandlerObject = new GameObject("DOMEventHandler");
            UnityEngine.Object.DontDestroyOnLoad(eventHandlerObject);
            
            _eventHandler = eventHandlerObject.AddComponent<DOMEventHandler>();
            _eventHandler.Initialize(_core);

            _isInitialized = true;
            Debug.Log("JSPluginTools [DOM]: Module initialized");
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public async Task<bool> CreateElementAsync(string elementType, string id, string parentSelector = null, string attributes = null, string styles = null, string content = null)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(elementType))
            {
                throw new ArgumentException("Element type cannot be null or empty", nameof(elementType));
            }
            
            try
            {
                string method = "createElement";
                var parameters = new
                {
                    elementType,
                    id,
                    parentSelector,
                    attributes,
                    styles,
                    content
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error creating element '{elementType}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveElementAsync(string selector)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            try
            {
                string method = "removeElement";
                var parameters = new
                {
                    selector
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error removing element '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateContentAsync(string selector, string content)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            try
            {
                string method = "updateContent";
                var parameters = new
                {
                    selector,
                    content
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error updating content for '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAttributesAsync(string selector, string attributes)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(attributes))
            {
                throw new ArgumentException("Attributes cannot be null or empty", nameof(attributes));
            }
            
            try
            {
                string method = "updateAttributes";
                var parameters = new
                {
                    selector,
                    attributes
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error updating attributes for '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateStylesAsync(string selector, string styles)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(styles))
            {
                throw new ArgumentException("Styles cannot be null or empty", nameof(styles));
            }
            
            try
            {
                string method = "updateStyles";
                var parameters = new
                {
                    selector,
                    styles
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error updating styles for '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AddClassAsync(string selector, string className)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name cannot be null or empty", nameof(className));
            }
            
            try
            {
                string method = "addClass";
                var parameters = new
                {
                    selector,
                    className
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error adding class '{className}' to '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveClassAsync(string selector, string className)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name cannot be null or empty", nameof(className));
            }
            
            try
            {
                string method = "removeClass";
                var parameters = new
                {
                    selector,
                    className
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error removing class '{className}' from '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ToggleClassAsync(string selector, string className)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Class name cannot be null or empty", nameof(className));
            }
            
            try
            {
                string method = "toggleClass";
                var parameters = new
                {
                    selector,
                    className
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error toggling class '{className}' for '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetAttributeAsync(string selector, string attributeName)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));
            }
            
            try
            {
                string method = "getAttribute";
                var parameters = new
                {
                    selector,
                    attributeName
                };
                
                return await SendDOMRequestAsync<string>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting attribute '{attributeName}' from '{selector}': {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetContentAsync(string selector, string contentType = "innerHTML")
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            try
            {
                string method = "getContent";
                var parameters = new
                {
                    selector,
                    contentType
                };
                
                return await SendDOMRequestAsync<string>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting content from '{selector}': {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ElementExistsAsync(string selector)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            try
            {
                string method = "elementExists";
                var parameters = new
                {
                    selector
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error checking if element exists '{selector}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> AddEventListenerAsync(string selector, string eventName, string callbackName)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
            }
            
            if (string.IsNullOrEmpty(callbackName))
            {
                throw new ArgumentException("Callback name cannot be null or empty", nameof(callbackName));
            }
            
            try
            {
                string method = "addEventListener";
                var parameters = new
                {
                    selector,
                    eventName,
                    callbackName
                };
                
                // Регистрируем обработчик события в Unity
                _eventHandler.RegisterCallback(callbackName);
                
                // Регистрируем обработчик в JavaScript
                return await SendDOMRequestAsync<string>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error adding event listener '{eventName}' to '{selector}': {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveEventListenerAsync(string handlerId)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(handlerId))
            {
                throw new ArgumentException("Handler ID cannot be null or empty", nameof(handlerId));
            }
            
            try
            {
                string method = "removeEventListener";
                var parameters = new
                {
                    handlerId
                };
                
                // Удаляем обработчик в JavaScript
                bool result = await SendDOMRequestAsync<bool>(method, parameters);
                
                // Если удаление успешно, удаляем обработчик в Unity
                if (result)
                {
                    _eventHandler.UnregisterCallback(handlerId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error removing event listener '{handlerId}': {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AddCSSAsync(string cssText, string id = null)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(cssText))
            {
                throw new ArgumentException("CSS text cannot be null or empty", nameof(cssText));
            }
            
            try
            {
                string method = "addCSS";
                var parameters = new
                {
                    cssText,
                    id
                };
                
                return await SendDOMRequestAsync<bool>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error adding CSS: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<DOMRect> GetBoundingClientRectAsync(string selector)
        {
            CheckInitialized();
            
            if (string.IsNullOrEmpty(selector))
            {
                throw new ArgumentException("Selector cannot be null or empty", nameof(selector));
            }
            
            try
            {
                string method = "getBoundingClientRect";
                var parameters = new
                {
                    selector
                };
                
                return await SendDOMRequestAsync<DOMRect>(method, parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error getting bounding client rect for '{selector}': {ex.Message}");
                return null;
            }
        }

        private async Task<T> SendDOMRequestAsync<T>(string method, object parameters)
        {
            string messageType = GetMessageType(method);
            
            var tcs = new TaskCompletionSource<T>();
            
            try
            {
                _messageBus.SendMessageWithResponse<object, T>(
                    messageType,
                    parameters,
                    response => tcs.SetResult(response));
                    
                Debug.Log($"JSPluginTools [DOM]: Sent request '{method}'");
                
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSPluginTools [DOM]: Error sending DOM request '{method}': {ex.Message}");
                tcs.SetException(ex);
                throw;
            }
        }
        
        private string GetMessageType(string method)
        {
            return $"{MESSAGE_PREFIX}{method}";
        }
        
        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("JSPluginTools [DOM]: Module not initialized. Call Initialize() first");
            }
        }
    }
}