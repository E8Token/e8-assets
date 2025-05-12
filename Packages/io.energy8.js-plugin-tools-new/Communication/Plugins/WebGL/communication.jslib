/**
 * Energy8 JS Plugin Tools - Communication Module
 * Модуль для обеспечения канальной коммуникации между Unity и JavaScript
 * Namespace: Energy8.JSPluginTools.Communication
 */

mergeInto(LibraryManager.library, {
  // Инициализация Communication модуля
  __JS_Communication_Init: function() {
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools [Communication]: Cannot initialize - core not initialized");
        return;
      }
      
      // Инициализация и публичный API для Communication
      Energy8JSPluginTools.Communication = {
        // Константа для префикса сообщений
        MESSAGE_PREFIX: 'comm.',
        
        // Обработчики каналов
        channelHandlers: {},
        
        // Публичные методы для консоли и отладки
        getChannels: function() {
            return Object.keys(this.channelHandlers);
        },
        
        // Функция для регистрации обработчика канала
        registerChannelHandler: function(channel, handler) {
          if (!channel || typeof channel !== 'string') {
            console.error("Energy8JSPluginTools [Communication]: Channel name must be a non-empty string");
            return;
          }
          
          if (typeof handler !== 'function') {
            console.error("Energy8JSPluginTools [Communication]: Handler must be a function");
            return;
          }
          
          const messageType = this.MESSAGE_PREFIX + channel;
          
          // Сохраняем обработчик канала
          this.channelHandlers[channel] = handler;
          
          // Регистрируем обработчик сообщений
          Energy8JSPluginTools._internal.messageHandlers[messageType] = function(payload, sendResponse) {
            // Преобразуем payload, если это строка JSON
            let data = payload;
            if (typeof payload === 'string') {
              try {
                data = JSON.parse(payload);
              } catch (e) {
                // Если не удалось разобрать JSON, используем исходную строку
              }
            }
            
            // Вызываем обработчик канала
            const response = handler(data);
            
            // Если есть функция отправки ответа и обработчик вернул результат
            if (typeof sendResponse === 'function') {
              sendResponse(response);
            }
          };
          
          if (Energy8JSPluginTools._internal.debugMode) {
            console.log("Energy8JSPluginTools [Communication]: Registered handler for channel '" + channel + "'");
          }
          
          return true;
        },
        
        // Функция для отмены регистрации обработчика канала
        unregisterChannelHandler: function(channel) {
          if (!channel || typeof channel !== 'string') {
            console.error("Energy8JSPluginTools [Communication]: Channel name must be a non-empty string");
            return false;
          }
          
          const messageType = this.MESSAGE_PREFIX + channel;
          
          // Удаляем обработчик канала
          delete this.channelHandlers[channel];
          
          // Удаляем обработчик сообщений
          delete Energy8JSPluginTools._internal.messageHandlers[messageType];
          
          if (Energy8JSPluginTools._internal.debugMode) {
            console.log("Energy8JSPluginTools [Communication]: Unregistered handler for channel '" + channel + "'");
          }
          
          return true;
        },
        
        // Функция для отправки сообщения в Unity
        send: function(channel, data) {
          if (!channel || typeof channel !== 'string') {
            console.error("Energy8JSPluginTools [Communication]: Channel name must be a non-empty string");
            return false;
          }
          
          // Находим объект Unity и отправляем сообщение через вызов метода
          // Предполагается, что объект с именем CommunicationProxy зарегистрирован в Unity
          var communicationObject = "CommunicationProxy"; 
          
          // Формируем сообщение
          var message = {
            channel: channel,
            data: data
          };
          
          // Преобразуем данные в JSON
          var jsonData = JSON.stringify(message);
          
          // Ищем зарегистрированный объект CommunicationProxy
          for (var objId in Energy8JSPluginTools._internal.registeredObjects) {
            if (Energy8JSPluginTools._internal.registeredObjects[objId] === communicationObject) {
              window.unityInstance.SendMessage(communicationObject, "HandleMessageFromJS", jsonData);
              
              if (Energy8JSPluginTools._internal.debugMode) {
                console.log("Energy8JSPluginTools [Communication]: Sent message to Unity on channel '" + channel + "'");
              }
              return true;
            }
          }
          
          console.error("Energy8JSPluginTools [Communication]: Cannot find registered CommunicationProxy object");
          return false;
        },
        
        // Проверить наличие обработчика канала
        hasChannelHandler: function(channel) {
          return !!this.channelHandlers[channel];
        },
        
        // Версия модуля
        version: "1.0.0",
        
        // Получить информацию о модуле для отладки
        getInfo: function() {
          return {
            version: this.version,
            channelCount: Object.keys(this.channelHandlers).length,
            registeredChannels: this.getChannels()
          };
        }
      };
      
      console.log("Energy8JSPluginTools [Communication]: Module initialized");
      
      // Добавим информацию о доступных командах в консоль
      if (Energy8JSPluginTools._internal.debugMode) {
        console.info(
          "Energy8JSPluginTools.Communication module available.\n" +
          "Try: Energy8JSPluginTools.Communication.getChannels() to see all registered channels\n" +
          "     Energy8JSPluginTools.Communication.send('channel', data) to send a message to Unity\n" +
          "     Energy8JSPluginTools.Communication.getInfo() to see module information"
        );
      }
      
    } catch (e) {
      console.error("Energy8JSPluginTools [Communication]: Error initializing module:", e);
    }
    
    return 0;
  },

  // Функции для работы с каналами коммуникации
  
  // Установить обработчик для канала связи
  SetChannelHandler: function(channelPtr, objectNamePtr) {
    var channel = UTF8ToString(channelPtr);
    var objectName = UTF8ToString(objectNamePtr);
    
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools [Communication]: Cannot set channel handler - environment not initialized");
        return;
      }
      
      // Храним информацию о каналах в _internal
      if (!Energy8JSPluginTools._internal.channels) {
        Energy8JSPluginTools._internal.channels = {};
      }
      
      Energy8JSPluginTools._internal.channels[channel] = objectName;
      
      if (Energy8JSPluginTools._internal.debugMode) {
        console.log("Energy8JSPluginTools [Communication]: Set handler for channel '" + channel + "' to object '" + objectName + "'");
      }
      
      // Регистрируем обработчик сообщений для этого канала в JavaScript
      Energy8JSPluginTools._internal.messageHandlers[channel] = function(payload) {
        try {
          var data = JSON.parse(payload);
          
          // Если это запрос с ожиданием ответа (имеет requestId)
          if (data && data.requestId) {
            var requestData = data.data;
            
            // Здесь мы эмулируем обработку запроса в JavaScript и отправку ответа обратно в Unity
            Energy8JSPluginTools._internal.processChannelRequestResponse = function(response) {
              var responseObj = {
                RequestId: data.requestId,
                Response: response
              };
              
              var responseJson = JSON.stringify(responseObj);
              var responseChannel = channel + "_response";
              
              // Находим Unity объект, зарегистрированный для обработки ответов
              if (Energy8JSPluginTools._internal.channels[responseChannel]) {
                var objectName = Energy8JSPluginTools._internal.channels[responseChannel];
                window.unityInstance.SendMessage(objectName, "HandleMessageFromJS", JSON.stringify({
                  messageType: responseChannel,
                  payload: responseJson
                }));
              }
            };
            
            // Генерируем событие, которое может быть перехвачено JavaScript кодом
            var event = new CustomEvent("energy8:channel-request", {
              detail: {
                channel: channel,
                data: requestData,
                requestId: data.requestId,
                respond: Energy8JSPluginTools._internal.processChannelRequestResponse
              }
            });
            window.dispatchEvent(event);
          } else {
            // Обычное сообщение без ожидания ответа
            var event = new CustomEvent("energy8:channel-message", {
              detail: {
                channel: channel,
                data: data
              }
            });
            window.dispatchEvent(event);
          }
        } catch (e) {
          console.error("Energy8JSPluginTools [Communication]: Error processing channel message:", e);
        }
      };
    } catch (e) {
      console.error("Energy8JSPluginTools [Communication]: Error setting channel handler:", e);
    }
  },
  
  // Удалить обработчик для канала связи
  RemoveChannelHandler: function(channelPtr) {
    var channel = UTF8ToString(channelPtr);
    
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized || 
          !Energy8JSPluginTools._internal.channels) {
        return;
      }
      
      if (Energy8JSPluginTools._internal.channels[channel]) {
        delete Energy8JSPluginTools._internal.channels[channel];
        
        if (Energy8JSPluginTools._internal.debugMode) {
          console.log("Energy8JSPluginTools [Communication]: Removed handler for channel '" + channel + "'");
        }
      }
      
      if (Energy8JSPluginTools._internal.messageHandlers[channel]) {
        delete Energy8JSPluginTools._internal.messageHandlers[channel];
      }
    } catch (e) {
      console.error("Energy8JSPluginTools [Communication]: Error removing channel handler:", e);
    }
  },
  
  // Отправить сообщение с JavaScript в Unity по определенному каналу
  SendChannelMessageToUnity: function(channelPtr, dataPtr) {
    var channel = UTF8ToString(channelPtr);
    var data = UTF8ToString(dataPtr);
    
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized || 
          !Energy8JSPluginTools._internal.channels) {
        console.error("Energy8JSPluginTools [Communication]: Cannot send message - environment not initialized");
        return;
      }
      
      if (Energy8JSPluginTools._internal.channels[channel]) {
        var objectName = Energy8JSPluginTools._internal.channels[channel];
        window.unityInstance.SendMessage(objectName, "HandleMessageFromJS", JSON.stringify({
          messageType: channel,
          payload: data
        }));
        
        if (Energy8JSPluginTools._internal.debugMode) {
          console.log("Energy8JSPluginTools [Communication]: Sent message to Unity on channel '" + channel + "'");
        }
      } else if (Energy8JSPluginTools._internal.debugMode) {
        console.warn("Energy8JSPluginTools [Communication]: No handler registered for channel '" + channel + "'");
      }
    } catch (e) {
      console.error("Energy8JSPluginTools [Communication]: Error sending message to Unity:", e);
    }
  }
});