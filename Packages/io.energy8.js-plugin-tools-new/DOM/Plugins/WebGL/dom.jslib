/**
 * Energy8 JS Plugin Tools - DOM Module
 * Модуль для манипуляции DOM элементами из Unity
 */

mergeInto(LibraryManager.library, {
  __JS_DOM_Init: function() {
    try {
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools [DOM]: Cannot initialize - core not initialized");
        return;
      }
      
      // Инициализация и публичный API для DOM
      Energy8JSPluginTools.DOM = {
        // Константы
        CHANNEL_PREFIX: 'dom.',
        EVENT_CHANNEL: 'dom.event',
        
        // Хранилище для отслеживания зарегистрированных обработчиков событий
        eventHandlers: {},
        
        // Генератор уникальных идентификаторов
        generateId: function(prefix) {
          return (prefix || 'e8_element_') + Math.random().toString(36).substr(2, 9);
        },
        
        // Создание нового DOM-элемента
        createElement: function(tagName, id, className, style, parentId) {
          try {
            // Валидация параметров
            if (!tagName) {
              console.error("Energy8JSPluginTools [DOM]: Missing required parameter: tagName");
              return null;
            }
            
            // Создание элемента
            const element = document.createElement(tagName);
            
            // Установка ID (генерация, если не указан)
            const elementId = id || this.generateId();
            element.id = elementId;
            
            // Установка класса, если указан
            if (className) {
              element.className = className;
            }
            
            // Установка стилей, если указаны
            if (style) {
              element.setAttribute('style', style);
            }
            
            // Добавление в родительский элемент
            let parent;
            if (parentId) {
              parent = document.getElementById(parentId);
              if (!parent) {
                console.warn(`Energy8JSPluginTools [DOM]: Parent element with ID '${parentId}' not found, appending to body instead`);
                parent = document.body;
              }
            } else {
              parent = document.body;
            }
            
            parent.appendChild(element);
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Created element '${tagName}' with ID '${elementId}'`);
            }
            return elementId;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error creating element:", e);
            return null;
          }
        },
        
        // Публичный API для получения всех созданных элементов
        getCreatedElements: function() {
          try {
            const elements = document.querySelectorAll('[id^="e8_element_"]');
            const result = [];
            elements.forEach(el => {
              result.push({
                id: el.id,
                tagName: el.tagName.toLowerCase(),
                className: el.className
              });
            });
            return result;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting created elements:", e);
            return [];
          }
        },
        
        // Установка содержимого элемента
        setContent: function(elementId, content, append) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            if (append) {
              element.innerHTML += content;
            } else {
              element.innerHTML = content;
            }
            
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error setting content:", e);
            return false;
          }
        },
        
        // Получение содержимого элемента
        getContent: function(elementId) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return null;
            }
            
            return element.innerHTML;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting content:", e);
            return null;
          }
        },
        
        // Установка атрибута элемента
        setAttribute: function(elementId, attributeName, attributeValue) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            if (attributeValue === null || attributeValue === undefined) {
              element.removeAttribute(attributeName);
            } else {
              element.setAttribute(attributeName, attributeValue);
            }
            
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error setting attribute:", e);
            return false;
          }
        },
        
        // Получение атрибута элемента
        getAttribute: function(elementId, attributeName) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return null;
            }
            
            return element.getAttribute(attributeName);
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting attribute:", e);
            return null;
          }
        },
        
        // Установка стиля элемента
        setStyle: function(elementId, propertyName, propertyValue) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            // Преобразование имени свойства из kebab-case в camelCase для style API
            const camelCaseProp = propertyName.replace(/-([a-z])/g, function(g) {
              return g[1].toUpperCase();
            });
            
            element.style[camelCaseProp] = propertyValue;
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error setting style:", e);
            return false;
          }
        },
        
        // Получение стиля элемента
        getStyle: function(elementId, propertyName) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return null;
            }
            
            const styles = window.getComputedStyle(element);
            return styles.getPropertyValue(propertyName);
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting style:", e);
            return null;
          }
        },
        
        // Добавление обработчика события
        addEventListener: function(elementId, eventType) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            // Создаем структуру для хранения обработчиков
            if (!this.eventHandlers[elementId]) {
              this.eventHandlers[elementId] = {};
            }
            
            // Проверяем, не зарегистрирован ли уже обработчик для этого события
            if (this.eventHandlers[elementId][eventType]) {
              console.warn(`Energy8JSPluginTools [DOM]: Event handler for '${eventType}' already exists on element '${elementId}'`);
              return true;
            }
            
            // Создаем функцию-обработчик
            const handler = (event) => this._handleDOMEvent(elementId, event);
            this.eventHandlers[elementId][eventType] = handler;
            
            // Регистрируем обработчик
            element.addEventListener(eventType, handler);
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Added event listener for '${eventType}' on element '${elementId}'`);
            }
            
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error adding event listener:", e);
            return false;
          }
        },
        
        // Обработчик событий DOM
        _handleDOMEvent: function(elementId, event) {
          try {
            // Создаем объект с данными о событии для передачи в Unity
            const eventData = {
              Type: event.type,
              ElementId: elementId,
              MouseX: event.clientX || 0,
              MouseY: event.clientY || 0,
              Value: event.target.value || '',
              IsTrusted: event.isTrusted,
              Timestamp: event.timeStamp || Date.now(),
              AdditionalData: {}
            };
            
            // Добавляем дополнительные данные в зависимости от типа события
            switch (event.type) {
              case 'click':
                eventData.AdditionalData.button = event.button;
                eventData.AdditionalData.detail = event.detail;
                break;
              case 'keydown':
              case 'keyup':
              case 'keypress':
                eventData.AdditionalData.key = event.key;
                eventData.AdditionalData.keyCode = event.keyCode;
                eventData.AdditionalData.altKey = event.altKey;
                eventData.AdditionalData.ctrlKey = event.ctrlKey;
                eventData.AdditionalData.shiftKey = event.shiftKey;
                break;
              case 'change':
              case 'input':
                eventData.Value = event.target.value || '';
                if (event.target.checked !== undefined) {
                  eventData.AdditionalData.checked = event.target.checked;
                }
                break;
            }
            
            // Отправляем событие в Unity
            const message = {
              ElementId: elementId,
              EventData: eventData
            };
            
            if (Energy8JSPluginTools.Communication) {
              if (Energy8JSPluginTools.Communication.send) {
                Energy8JSPluginTools.Communication.send(this.EVENT_CHANNEL, message);
              }
            }
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error handling DOM event:", e);
          }
        },
        
        // Удаление обработчика события
        removeEventListener: function(elementId, eventType) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            if (!this.eventHandlers[elementId] || !this.eventHandlers[elementId][eventType]) {
              console.warn(`Energy8JSPluginTools [DOM]: No event handler for '${eventType}' found on element '${elementId}'`);
              return false;
            }
            
            // Удаляем обработчик
            element.removeEventListener(eventType, this.eventHandlers[elementId][eventType]);
            delete this.eventHandlers[elementId][eventType];
            
            // Очищаем запись об элементе, если нет больше обработчиков
            if (Object.keys(this.eventHandlers[elementId]).length === 0) {
              delete this.eventHandlers[elementId];
            }
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Removed event listener for '${eventType}' from element '${elementId}'`);
            }
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error removing event listener:", e);
            return false;
          }
        },
        
        // Удаление элемента
        removeElement: function(elementId) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            // Удаляем все обработчики событий
            if (this.eventHandlers[elementId]) {
              for (const eventType in this.eventHandlers[elementId]) {
                element.removeEventListener(eventType, this.eventHandlers[elementId][eventType]);
              }
              delete this.eventHandlers[elementId];
            }
            
            // Удаляем элемент
            element.parentNode.removeChild(element);
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Removed element with ID '${elementId}'`);
            }
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error removing element:", e);
            return false;
          }
        },
        
        // Показать или скрыть элемент
        setVisible: function(elementId, visible) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return false;
            }
            
            element.style.display = visible ? '' : 'none';
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error setting visibility:", e);
            return false;
          }
        },
        
        // Поиск элементов по CSS-селектору
        querySelectorAll: function(cssSelector) {
          try {
            const elements = document.querySelectorAll(cssSelector);
            const ids = [];
            
            // Сохраняем или генерируем ID для найденных элементов
            elements.forEach((element) => {
              let id = element.id;
              if (!id) {
                id = this.generateId();
                element.id = id;
              }
              ids.push(id);
            });
            
            return ids;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error querying elements:", e);
            return [];
          }
        },
        
        // Получение или установка позиции элемента
        getSetPosition: function(elementId, position) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return null;
            }
            
            // Если передана позиция для установки
            if (position) {
              // Устанавливаем position CSS свойство, если указано
              if (position.Position) {
                element.style.position = position.Position;
              } else if (!element.style.position) {
                // По умолчанию используем absolute
                element.style.position = 'absolute';
              }
              
              // Устанавливаем координаты
              if (position.Left !== undefined) element.style.left = position.Left + 'px';
              if (position.Top !== undefined) element.style.top = position.Top + 'px';
              if (position.Right !== undefined) element.style.right = position.Right + 'px';
              if (position.Bottom !== undefined) element.style.bottom = position.Bottom + 'px';
            }
            
            // Получаем текущую позицию
            const rect = element.getBoundingClientRect();
            const styles = window.getComputedStyle(element);
            
            return {
              Left: Math.round(rect.left),
              Top: Math.round(rect.top),
              Right: Math.round(rect.right),
              Bottom: Math.round(rect.bottom),
              Position: styles.position
            };
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting/setting position:", e);
            return null;
          }
        },
        
        // Получение размеров элемента
        getElementSize: function(elementId) {
          try {
            const element = document.getElementById(elementId);
            if (!element) {
              console.error(`Energy8JSPluginTools [DOM]: Element with ID '${elementId}' not found`);
              return { x: 0, y: 0 };
            }
            
            const rect = element.getBoundingClientRect();
            return {
              x: Math.round(rect.width),
              y: Math.round(rect.height)
            };
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error getting element size:", e);
            return { x: 0, y: 0 };
          }
        },
        
        // Создание модального окна
        createModal: function(options) {
          try {
            // Валидация параметров
            if (!options) {
              options = {};
            }
            
            // Генерация ID для модального окна
            const modalId = options.CustomId || this.generateId('modal_');
            
            // Создание оверлея (фона) для модального окна
            if (options.Backdrop !== false) {
              const backdrop = document.createElement('div');
              backdrop.id = modalId + '_backdrop';
              backdrop.style.position = 'fixed';
              backdrop.style.top = '0';
              backdrop.style.left = '0';
              backdrop.style.width = '100%';
              backdrop.style.height = '100%';
              backdrop.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
              backdrop.style.zIndex = '1000';
              document.body.appendChild(backdrop);
            }
            
            // Создание контейнера модального окна
            const modal = document.createElement('div');
            modal.id = modalId;
            modal.className = 'energy8-modal ' + (options.CustomClass || '');
            modal.style.position = 'fixed';
            modal.style.top = '50%';
            modal.style.left = '50%';
            modal.style.transform = 'translate(-50%, -50%)';
            modal.style.backgroundColor = 'white';
            modal.style.borderRadius = '5px';
            modal.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.2)';
            modal.style.zIndex = '1001';
            modal.style.overflow = 'hidden';
            modal.style.width = options.Width || 'auto';
            modal.style.height = options.Height || 'auto';
            modal.style.minWidth = '300px';
            
            // Создание шапки модального окна с заголовком
            if (options.Title || options.ShowCloseButton !== false) {
              const header = document.createElement('div');
              header.style.padding = '10px 15px';
              header.style.borderBottom = '1px solid #e5e5e5';
              header.style.display = 'flex';
              header.style.justifyContent = 'space-between';
              header.style.alignItems = 'center';
              
              // Заголовок
              if (options.Title) {
                const title = document.createElement('h3');
                title.style.margin = '0';
                title.style.fontSize = '18px';
                title.textContent = options.Title;
                header.appendChild(title);
              }
              
              // Кнопка закрытия
              if (options.ShowCloseButton !== false) {
                const closeBtn = document.createElement('button');
                closeBtn.textContent = '×';
                closeBtn.style.border = 'none';
                closeBtn.style.background = 'none';
                closeBtn.style.fontSize = '24px';
                closeBtn.style.cursor = 'pointer';
                closeBtn.style.padding = '0 5px';
                closeBtn.style.lineHeight = '1';
                closeBtn.onclick = () => this.closeModal(modalId);
                header.appendChild(closeBtn);
              }
              
              modal.appendChild(header);
            }
            
            // Содержимое модального окна
            const body = document.createElement('div');
            body.style.padding = '15px';
            body.innerHTML = options.Content || '';
            modal.appendChild(body);
            
            // Кнопки модального окна
            if (options.Buttons && options.Buttons.Items && options.Buttons.Items.length > 0) {
              const footer = document.createElement('div');
              footer.style.padding = '10px 15px';
              footer.style.borderTop = '1px solid #e5e5e5';
              footer.style.textAlign = 'right';
              
              options.Buttons.Items.forEach((buttonConfig) => {
                const button = document.createElement('button');
                button.textContent = buttonConfig.Text || 'Button';
                button.id = buttonConfig.Id || this.generateId('btn_');
                button.className = 'energy8-modal-btn ' + (buttonConfig.Class || '');
                button.style.marginLeft = '5px';
                button.style.padding = '6px 12px';
                button.style.border = '1px solid #ccc';
                button.style.borderRadius = '4px';
                button.style.cursor = 'pointer';
                
                // Обработчик клика на кнопку
                button.onclick = (event) => {
                  // Отправляем событие в Unity
                  this._handleDOMEvent(button.id, event);
                  
                  // Закрываем модальное окно, если указано
                  if (buttonConfig.CloseOnClick !== false) {
                    this.closeModal(modalId);
                  }
                };
                
                footer.appendChild(button);
              });
              
              modal.appendChild(footer);
            }
            
            // Опционально делаем модальное окно перетаскиваемым
            if (options.Draggable) {
              this._makeElementDraggable(modal);
            }
            
            // Добавляем модальное окно в DOM
            document.body.appendChild(modal);
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Created modal with ID '${modalId}'`);
            }
            return modalId;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error creating modal:", e);
            return null;
          }
        },
        
        // Закрытие модального окна
        closeModal: function(modalId) {
          try {
            // Удаляем модальное окно
            const modal = document.getElementById(modalId);
            if (modal) {
              modal.parentNode.removeChild(modal);
            }
            
            // Удаляем оверлей
            const backdrop = document.getElementById(modalId + '_backdrop');
            if (backdrop) {
              backdrop.parentNode.removeChild(backdrop);
            }
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log(`Energy8JSPluginTools [DOM]: Closed modal with ID '${modalId}'`);
            }
            return true;
          } catch (e) {
            console.error("Energy8JSPluginTools [DOM]: Error closing modal:", e);
            return false;
          }
        },
        
        // Сделать элемент перетаскиваемым
        _makeElementDraggable: function(element) {
          let offsetX = 0, offsetY = 0;
          
          // Функция для начала перетаскивания
          const dragMouseDown = (e) => {
            e.preventDefault();
            
            // Получаем начальное положение курсора
            offsetX = e.clientX;
            offsetY = e.clientY;
            
            // Добавляем обработчики событий для перетаскивания и окончания перетаскивания
            document.addEventListener('mousemove', dragElement);
            document.addEventListener('mouseup', closeDragElement);
          };
          
          // Функция для перетаскивания элемента
          const dragElement = (e) => {
            e.preventDefault();
            
            // Вычисляем новую позицию элемента
            const deltaX = offsetX - e.clientX;
            const deltaY = offsetY - e.clientY;
            offsetX = e.clientX;
            offsetY = e.clientY;
            
            const rect = element.getBoundingClientRect();
            element.style.top = (rect.top - deltaY) + 'px';
            element.style.left = (rect.left - deltaX) + 'px';
            element.style.transform = 'none';
            element.style.margin = '0';
          };
          
          // Функция для завершения перетаскивания
          const closeDragElement = () => {
            document.removeEventListener('mousemove', dragElement);
            document.removeEventListener('mouseup', closeDragElement);
          };
          
          // Находим заголовок модального окна или используем сам элемент для начала перетаскивания
          const headerElement = element.querySelector('div');
          const dragHandle = headerElement || element;
          
          dragHandle.style.cursor = 'move';
          dragHandle.addEventListener('mousedown', dragMouseDown);
        },
        
        // Регистрирует обработчики для каналов связи
        _registerHandlers: function() {
          const self = this;
          
          if (Energy8JSPluginTools.Communication) {
            // Регистрация обработчиков для каждого типа запроса
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'createElement',
              (data) => self.createElement(data.TagName, data.Id, data.ClassName, data.Style, data.ParentId)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'setContent',
              (data) => self.setContent(data.Id, data.Content, data.Append)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getContent',
              (data) => self.getContent(data.Id)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'setAttribute',
              (data) => self.setAttribute(data.Id, data.AttributeName, data.AttributeValue)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getAttribute',
              (data) => self.getAttribute(data.Id, data.AttributeName)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'setStyle',
              (data) => self.setStyle(data.Id, data.StyleProperty, data.StyleValue)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getStyle',
              (data) => self.getStyle(data.Id, data.StyleProperty)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'addEventListener',
              (data) => self.addEventListener(data.Id, data.EventType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'removeEventListener',
              (data) => self.removeEventListener(data.Id, data.EventType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'removeElement',
              (data) => self.removeElement(data.Id)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'setVisible',
              (data) => self.setVisible(data.Id, data.Visible)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'querySelectorAll',
              (data) => self.querySelectorAll(data.CssSelector)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getSetPosition',
              (data) => self.getSetPosition(data.Id, data.Position)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getElementSize',
              (data) => self.getElementSize(data.Id)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'createModal',
              (data) => self.createModal(data)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'closeModal',
              (data) => self.closeModal(data.Id)
            );
            
            if (Energy8JSPluginTools._internal.debugMode) {
              console.log("Energy8JSPluginTools [DOM]: Registered all channel handlers");
            }
          } else {
            console.error("Energy8JSPluginTools [DOM]: Communication module not available");
          }
        }
      };
      
      // Регистрация обработчиков событий
      Energy8JSPluginTools.DOM._registerHandlers();
      
      console.log("Energy8JSPluginTools [DOM]: Module initialized");
      
      // Добавим информацию о доступных командах в консоль
      if (Energy8JSPluginTools._internal.debugMode) {
        console.info(
          "Energy8JSPluginTools.DOM module available.\n" +
          "Try: Energy8JSPluginTools.DOM.createElement('div') to create a div element\n" +
          "     Energy8JSPluginTools.DOM.getCreatedElements() to see all created elements\n" +
          "     Energy8JSPluginTools.DOM.createModal({Title:'Test'}) to create a modal dialog"
        );
      }
      
    } catch (e) {
      console.error("Energy8JSPluginTools [DOM]: Error initializing module:", e);
    }
    
    return 0;
  }
});