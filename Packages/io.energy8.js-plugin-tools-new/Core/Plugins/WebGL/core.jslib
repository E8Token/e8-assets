/**
 * Energy8 JS Plugin Tools - Core Module
 * Пространство имен для взаимодействия между Unity и JavaScript
 */
var Energy8JSPluginTools = Energy8JSPluginTools || {
    Core: {},
    Communication: {},
    Device: {},
    DOM: {},
    Network: {},
    Storage: {},
    // Приватные данные, используемые внутри плагина
    _internal: {
        registeredObjects: {},
        messageHandlers: {},
        responseHandlers: {},
        initialized: false,
        debugMode: false
    }
};

mergeInto(LibraryManager.library, {
  // Основные функции для инициализации и управления плагином
  JSPluginInitialize: function() {
    try {
      if (!Energy8JSPluginTools._internal.initialized) {
        // Инициализация и публичный API для Core
        Energy8JSPluginTools.Core = {
            version: "1.0.0",
            
            // Публичные методы для консоли и отладки
            getRegisteredObjects: function() {
                return Object.keys(Energy8JSPluginTools._internal.registeredObjects);
            },
            
            getMessageHandlers: function() {
                return Object.keys(Energy8JSPluginTools._internal.messageHandlers);
            },
            
            isDebugMode: function() {
                return Energy8JSPluginTools._internal.debugMode;
            },
            
            toggleDebug: function() {
                Energy8JSPluginTools._internal.debugMode = !Energy8JSPluginTools._internal.debugMode;
                console.log("Debug mode: " + Energy8JSPluginTools._internal.debugMode);
                return Energy8JSPluginTools._internal.debugMode;
            },
            
            sendMessageToUnity: function(gameObjectName, methodName, message) {
                if (typeof window.unityInstance !== 'undefined') {
                    window.unityInstance.SendMessage(gameObjectName, methodName, 
                        typeof message === 'string' ? message : JSON.stringify(message));
                    return true;
                }
                console.error("Unity instance not found");
                return false;
            }
        };
        
        Energy8JSPluginTools._internal.initialized = true;
        console.log("Energy8JSPluginTools: Core module initialized");
        
        // Сообщаем в консоль информацию о доступных командах
        console.info(
            "Energy8JSPluginTools available in console.\n" +
            "Try: Energy8JSPluginTools.Core.toggleDebug() to enable debug mode\n" +
            "     Energy8JSPluginTools.Core.getRegisteredObjects() to see registered GameObjects\n" +
            "     Energy8JSPluginTools.Core.version to check version"
        );
      } else {
        console.warn("Energy8JSPluginTools: Already initialized");
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error initializing JavaScript environment:", e);
    }
  },

  JSPluginShutdown: function() {
    try {
      if (Energy8JSPluginTools._internal.initialized) {
        // Очистка ресурсов
        Energy8JSPluginTools._internal.registeredObjects = {};
        Energy8JSPluginTools._internal.messageHandlers = {};
        Energy8JSPluginTools._internal.responseHandlers = {};
        Energy8JSPluginTools._internal.initialized = false;
        console.log("Energy8JSPluginTools: Core module shut down");
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error shutting down JavaScript environment:", e);
    }
  },

  // Регистрация и управление объектами Unity
  JSRegisterGameObject: function(gameObjectNamePtr, objectIdPtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    var objectId = UTF8ToString(objectIdPtr);
    
    try {
      if (!Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools: Cannot register GameObject - environment not initialized");
        return;
      }
      
      Energy8JSPluginTools._internal.registeredObjects[objectId] = gameObjectName;
      
      if (Energy8JSPluginTools._internal.debugMode) {
        console.log("Energy8JSPluginTools: Registered GameObject '" + gameObjectName + "' with ID '" + objectId + "'");
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error registering GameObject:", e);
    }
  },

  JSUnregisterGameObject: function(objectIdPtr) {
    var objectId = UTF8ToString(objectIdPtr);
    
    try {
      if (!Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools: Cannot unregister GameObject - environment not initialized");
        return;
      }
      
      if (Energy8JSPluginTools._internal.registeredObjects[objectId]) {
        var gameObjectName = Energy8JSPluginTools._internal.registeredObjects[objectId];
        delete Energy8JSPluginTools._internal.registeredObjects[objectId];
        
        if (Energy8JSPluginTools._internal.debugMode) {
          console.log("Energy8JSPluginTools: Unregistered GameObject '" + gameObjectName + "' with ID '" + objectId + "'");
        }
      } else if (Energy8JSPluginTools._internal.debugMode) {
        console.warn("Energy8JSPluginTools: Attempted to unregister unknown object ID: " + objectId);
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error unregistering GameObject:", e);
    }
  },

  // Функции для работы с сообщениями
  SendMessageToJS: function(messageTypePtr, payloadPtr) {
    var messageType = UTF8ToString(messageTypePtr);
    var payload = UTF8ToString(payloadPtr);
    
    try {
      if (!Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools: Cannot send message - environment not initialized");
        return;
      }
      
      if (Energy8JSPluginTools._internal.debugMode) {
        console.log("Energy8JSPluginTools: Received message '" + messageType + "' from Unity");
      }
      
      // Обработка сообщения в JavaScript
      if (typeof Energy8JSPluginTools._internal.messageHandlers[messageType] === 'function') {
        Energy8JSPluginTools._internal.messageHandlers[messageType](payload);
      } else if (Energy8JSPluginTools._internal.debugMode) {
        console.log("Energy8JSPluginTools: No handler registered for message type '" + messageType + "'");
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error processing message:", e);
    }
  },

  SendMessageWithResponseToJS: function(messageTypePtr, payloadPtr, callbackIdPtr) {
    var messageType = UTF8ToString(messageTypePtr);
    var payload = UTF8ToString(payloadPtr);
    var callbackId = UTF8ToString(callbackIdPtr);
    
    try {
      if (!Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools: Cannot send message - environment not initialized");
        return;
      }
      
      if (Energy8JSPluginTools._internal.debugMode) {
        console.log("Energy8JSPluginTools: Received message with response '" + messageType + "' from Unity");
      }
      
      var parsedPayload;
      try {
        parsedPayload = JSON.parse(payload);
      } catch (parseError) {
        console.error("Energy8JSPluginTools: Error parsing payload JSON:", parseError);
        return;
      }
      
      // Обработка сообщения и отправка ответа
      var sendResponse = function(responseData) {
        try {
          var responseJson = JSON.stringify(responseData);
          
          // Находим объект Unity и отправляем ответ через вызов метода
          // Предполагается, что объект с именем MessageBusProxy зарегистрирован в Unity
          var messageBusObject = "MessageBusProxy"; // По умолчанию
          
          for (var objId in Energy8JSPluginTools._internal.registeredObjects) {
            if (Energy8JSPluginTools._internal.registeredObjects[objId] === messageBusObject) {
              window.unityInstance.SendMessage(messageBusObject, "HandleResponseFromJS", JSON.stringify({
                callbackId: callbackId,
                response: responseJson
              }));
              return;
            }
          }
          
          console.error("Energy8JSPluginTools: Cannot find registered MessageBusProxy object");
        } catch (e) {
          console.error("Energy8JSPluginTools: Error sending response:", e);
        }
      };
      
      if (typeof Energy8JSPluginTools._internal.messageHandlers[messageType] === 'function') {
        Energy8JSPluginTools._internal.messageHandlers[messageType](parsedPayload.data, sendResponse);
      } else {
        if (Energy8JSPluginTools._internal.debugMode) {
          console.log("Energy8JSPluginTools: No handler registered for message type '" + messageType + "'");
        }
        sendResponse(null); // Send null response if no handler exists
      }
    } catch (e) {
      console.error("Energy8JSPluginTools: Error processing message with response:", e);
    }
  },

  // Вспомогательные функции для связи JavaScript с Unity
  SendMessageToUnity: function(objectPtr, methodPtr, messagePtr) {
    var objectName = UTF8ToString(objectPtr);
    var methodName = UTF8ToString(methodPtr);
    var message = UTF8ToString(messagePtr);
    
    try {
      window.unityInstance.SendMessage(objectName, methodName, message);
    } catch (e) {
      console.error("Energy8JSPluginTools: Error sending message to Unity:", e);
    }
  }
});