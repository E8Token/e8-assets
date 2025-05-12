/**
 * Energy8 JS Plugin Tools - Storage Module
 * Модуль для работы с различными видами хранилищ (LocalStorage, SessionStorage, IndexedDB)
 */
mergeInto(LibraryManager.library, {
  __JS_Storage_Init: function() {
    try {
      console.log("Energy8JSPluginTools [Storage]: Initializing module");
      
      if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
        console.error("Energy8JSPluginTools [Storage]: Core module not initialized. Make sure core.jslib is loaded first.");
        return;
      }
      
      // Инициализация и публичный API для Storage
      Energy8JSPluginTools.Storage = {
        // Константы
        CHANNEL_PREFIX: 'storage.',
        
        // Проверка доступности хранилища
        isAvailable: function(storageType) {
          try {
            if (storageType === 'Local') {
              const testKey = '__storage_test__';
              localStorage.setItem(testKey, testKey);
              localStorage.removeItem(testKey);
              return true;
            } else if (storageType === 'Session') {
              const testKey = '__storage_test__';
              sessionStorage.setItem(testKey, testKey);
              sessionStorage.removeItem(testKey);
              return true;
            } else if (storageType === 'IndexedDB') {
              return typeof indexedDB !== 'undefined';
            }
            return false;
          } catch (e) {
            return false;
          }
        },
        
        // Получить элемент из хранилища
        getItem: function(key, storageType) {
          try {
            if (storageType === 'Local') {
              return localStorage.getItem(key);
            } else if (storageType === 'Session') {
              return sessionStorage.getItem(key);
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB getItem not supported, use getFromStore instead");
              return null;
            }
            return null;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in getItem:", e);
            return null;
          }
        },
        
        // Записать элемент в хранилище
        setItem: function(key, value, storageType) {
          try {
            if (storageType === 'Local') {
              localStorage.setItem(key, value);
              return true;
            } else if (storageType === 'Session') {
              sessionStorage.setItem(key, value);
              return true;
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB setItem not supported, use addToStore instead");
              return false;
            }
            return false;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in setItem:", e);
            return false;
          }
        },
        
        // Удалить элемент из хранилища
        removeItem: function(key, storageType) {
          try {
            if (storageType === 'Local') {
              localStorage.removeItem(key);
              return true;
            } else if (storageType === 'Session') {
              sessionStorage.removeItem(key);
              return true;
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB removeItem not supported, use removeFromStore instead");
              return false;
            }
            return false;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in removeItem:", e);
            return false;
          }
        },
        
        // Проверка наличия ключа в хранилище
        hasKey: function(key, storageType) {
          try {
            if (storageType === 'Local') {
              return localStorage.getItem(key) !== null;
            } else if (storageType === 'Session') {
              return sessionStorage.getItem(key) !== null;
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB hasKey not supported");
              return false;
            }
            return false;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in hasKey:", e);
            return false;
          }
        },
        
        // Получить все ключи из хранилища
        getAllKeys: function(storageType) {
          try {
            const keys = [];
            if (storageType === 'Local') {
              for (let i = 0; i < localStorage.length; i++) {
                keys.push(localStorage.key(i));
              }
            } else if (storageType === 'Session') {
              for (let i = 0; i < sessionStorage.length; i++) {
                keys.push(sessionStorage.key(i));
              }
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB getAllKeys not immediately supported");
              // Could implement with an async pattern, but not needed for immediate response
            }
            return keys;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in getAllKeys:", e);
            return [];
          }
        },
        
        // Очистить хранилище
        clear: function(storageType) {
          try {
            if (storageType === 'Local') {
              localStorage.clear();
              return true;
            } else if (storageType === 'Session') {
              sessionStorage.clear();
              return true;
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: Direct IndexedDB clear not immediately supported");
              return false;
            }
            return false;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in clear:", e);
            return false;
          }
        },
        
        // Получить размер хранилища в байтах
        getStorageSize: function(storageType) {
          try {
            let size = 0;
            if (storageType === 'Local') {
              for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                const value = localStorage.getItem(key);
                size += this._getStringByteLength(key) + this._getStringByteLength(value);
              }
            } else if (storageType === 'Session') {
              for (let i = 0; i < sessionStorage.length; i++) {
                const key = sessionStorage.key(i);
                const value = sessionStorage.getItem(key);
                size += this._getStringByteLength(key) + this._getStringByteLength(value);
              }
            } else if (storageType === 'IndexedDB') {
              console.warn("Energy8JSPluginTools [Storage]: IndexedDB size calculation not supported");
              return 0;
            }
            return size;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in getStorageSize:", e);
            return 0;
          }
        },
        
        // Получить оставшееся место в хранилище (примерно)
        getRemainingSpace: function(storageType) {
          try {
            // Эта функция дает приблизительную оценку, поскольку браузеры не предоставляют точных данных
            // о квотах хранилищ
            if (storageType === 'Local' || storageType === 'Session') {
              // Стандартная оценка для localStorage - около 5MB, но это зависит от браузера
              const estimatedQuota = 5 * 1024 * 1024; // 5MB в байтах
              const usedSpace = this.getStorageSize(storageType);
              return Math.max(0, estimatedQuota - usedSpace);
            } else if (storageType === 'IndexedDB') {
              // Для IndexedDB получение информации о квоте более сложное и асинхронное
              return -1; // Означает "неизвестно"
            }
            return -1;
          } catch (e) {
            console.error("Energy8JSPluginTools [Storage]: Error in getRemainingSpace:", e);
            return -1;
          }
        },
        
        // Вспомогательная функция для определения размера строки в байтах
        _getStringByteLength: function(str) {
          if (str === null || str === undefined) return 0;
          
          // Используем TextEncoder для точного подсчета байт в UTF-8
          if (typeof TextEncoder !== 'undefined') {
            return new TextEncoder().encode(str).length;
          }
          
          // Запасной вариант для старых браузеров (менее точный)
          let s = str.length;
          for (let i = str.length - 1; i >= 0; i--) {
            const code = str.charCodeAt(i);
            if (code > 0x7f && code <= 0x7ff) s++;
            else if (code > 0x7ff && code <= 0xffff) s += 2;
            if (code >= 0xDC00 && code <= 0xDFFF) i--; // trail surrogate
          }
          return s;
        },
        
        // === IndexedDB специфичные функции ===
        
        // Текущее состояние подключения к IndexedDB
        indexedDB: {
          db: null,
          dbName: '',
          version: 1,
          openRequest: null,
          pendingOperations: []
        },
        
        // Открыть базу данных
        openDatabase: function(dbName, version) {
          return new Promise((resolve) => {
            if (!window.indexedDB) {
              console.error("Energy8JSPluginTools [Storage]: IndexedDB is not supported in this browser");
              resolve(false);
              return;
            }
            
            try {
              const request = window.indexedDB.open(dbName, version);
              this.indexedDB.openRequest = request;
              this.indexedDB.dbName = dbName;
              this.indexedDB.version = version;
              
              request.onerror = (event) => {
                console.error("Energy8JSPluginTools [Storage]: Error opening IndexedDB:", event.target.error);
                resolve(false);
              };
              
              request.onsuccess = (event) => {
                this.indexedDB.db = event.target.result;
                console.log(`Energy8JSPluginTools [Storage]: Successfully opened IndexedDB '${dbName}'`);
                
                // Process any pending operations
                while (this.indexedDB.pendingOperations.length > 0) {
                  const operation = this.indexedDB.pendingOperations.shift();
                  operation();
                }
                
                resolve(true);
              };
              
              request.onupgradeneeded = (event) => {
                console.log(`Energy8JSPluginTools [Storage]: Upgrading IndexedDB from version ${event.oldVersion} to ${event.newVersion}`);
                this.indexedDB.db = event.target.result;
              };
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in openDatabase:", e);
              resolve(false);
            }
          });
        },
        
        // Создать хранилище объектов
        createObjectStore: function(storeName, keyPath) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                console.error("Energy8JSPluginTools [Storage]: No IndexedDB connection");
                resolve(false);
                return;
              }
              
              if (this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.log(`Energy8JSPluginTools [Storage]: Object store '${storeName}' already exists`);
                resolve(true);
                return;
              }
              
              // Need to close and reopen the DB to create a new store
              const dbName = this.indexedDB.dbName;
              const version = this.indexedDB.version + 1;
              this.indexedDB.db.close();
              
              const request = window.indexedDB.open(dbName, version);
              
              request.onerror = (event) => {
                console.error("Energy8JSPluginTools [Storage]: Error upgrading IndexedDB:", event.target.error);
                resolve(false);
              };
              
              request.onsuccess = (event) => {
                this.indexedDB.db = event.target.result;
                this.indexedDB.version = version;
                console.log(`Energy8JSPluginTools [Storage]: Successfully upgraded IndexedDB to version ${version}`);
                resolve(true);
              };
              
              request.onupgradeneeded = (event) => {
                const db = event.target.result;
                try {
                  db.createObjectStore(storeName, { keyPath: keyPath });
                  console.log(`Energy8JSPluginTools [Storage]: Created object store '${storeName}' with keyPath '${keyPath}'`);
                } catch (e) {
                  console.error(`Energy8JSPluginTools [Storage]: Error creating object store '${storeName}':`, e);
                  resolve(false);
                }
              };
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in createObjectStore:", e);
              resolve(false);
            }
          });
        },
        
        // Добавить данные в хранилище
        addToStore: function(storeName, data, key) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                this.indexedDB.pendingOperations.push(() => {
                  this.addToStore(storeName, data, key).then(resolve);
                });
                console.warn("Energy8JSPluginTools [Storage]: Queuing addToStore operation - database not ready");
                return;
              }
              
              if (!this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.error(`Energy8JSPluginTools [Storage]: Object store '${storeName}' does not exist`);
                resolve(false);
                return;
              }
              
              try {
                const transaction = this.indexedDB.db.transaction([storeName], "readwrite");
                const objectStore = transaction.objectStore(storeName);
                
                let dataObj;
                try {
                  dataObj = typeof data === 'string' ? JSON.parse(data) : data;
                } catch (e) {
                  dataObj = { value: data };
                }
                
                let request;
                if (key) {
                  dataObj.id = key;
                  request = objectStore.put(dataObj);
                } else {
                  request = objectStore.add(dataObj);
                }
                
                request.onsuccess = () => {
                  console.log(`Energy8JSPluginTools [Storage]: Added item to store '${storeName}'`);
                  resolve(true);
                };
                
                request.onerror = (event) => {
                  console.error(`Energy8JSPluginTools [Storage]: Error adding item to store '${storeName}':`, event.target.error);
                  resolve(false);
                };
              } catch (e) {
                console.error(`Energy8JSPluginTools [Storage]: Error in transaction with store '${storeName}':`, e);
                resolve(false);
              }
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in addToStore:", e);
              resolve(false);
            }
          });
        },
        
        // Получить данные из хранилища
        getFromStore: function(storeName, key) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                this.indexedDB.pendingOperations.push(() => {
                  this.getFromStore(storeName, key).then(resolve);
                });
                console.warn("Energy8JSPluginTools [Storage]: Queuing getFromStore operation - database not ready");
                return;
              }
              
              if (!this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.error(`Energy8JSPluginTools [Storage]: Object store '${storeName}' does not exist`);
                resolve(null);
                return;
              }
              
              try {
                const transaction = this.indexedDB.db.transaction([storeName], "readonly");
                const objectStore = transaction.objectStore(storeName);
                const request = objectStore.get(key);
                
                request.onsuccess = (event) => {
                  if (event.target.result) {
                    console.log(`Energy8JSPluginTools [Storage]: Retrieved item from store '${storeName}'`);
                    resolve(JSON.stringify(event.target.result));
                  } else {
                    console.log(`Energy8JSPluginTools [Storage]: Item with key '${key}' not found in store '${storeName}'`);
                    resolve(null);
                  }
                };
                
                request.onerror = (event) => {
                  console.error(`Energy8JSPluginTools [Storage]: Error getting item from store '${storeName}':`, event.target.error);
                  resolve(null);
                };
              } catch (e) {
                console.error(`Energy8JSPluginTools [Storage]: Error in transaction with store '${storeName}':`, e);
                resolve(null);
              }
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in getFromStore:", e);
              resolve(null);
            }
          });
        },
        
        // Удалить данные из хранилища
        removeFromStore: function(storeName, key) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                this.indexedDB.pendingOperations.push(() => {
                  this.removeFromStore(storeName, key).then(resolve);
                });
                console.warn("Energy8JSPluginTools [Storage]: Queuing removeFromStore operation - database not ready");
                return;
              }
              
              if (!this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.error(`Energy8JSPluginTools [Storage]: Object store '${storeName}' does not exist`);
                resolve(false);
                return;
              }
              
              try {
                const transaction = this.indexedDB.db.transaction([storeName], "readwrite");
                const objectStore = transaction.objectStore(storeName);
                const request = objectStore.delete(key);
                
                request.onsuccess = () => {
                  console.log(`Energy8JSPluginTools [Storage]: Removed item from store '${storeName}'`);
                  resolve(true);
                };
                
                request.onerror = (event) => {
                  console.error(`Energy8JSPluginTools [Storage]: Error removing item from store '${storeName}':`, event.target.error);
                  resolve(false);
                };
              } catch (e) {
                console.error(`Energy8JSPluginTools [Storage]: Error in transaction with store '${storeName}':`, e);
                resolve(false);
              }
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in removeFromStore:", e);
              resolve(false);
            }
          });
        },
        
        // Получение всех объектов из хранилища
        getAllFromStore: function(storeName) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                this.indexedDB.pendingOperations.push(() => {
                  this.getAllFromStore(storeName).then(resolve);
                });
                console.warn("Energy8JSPluginTools [Storage]: Queuing getAllFromStore operation - database not ready");
                return;
              }
              
              if (!this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.error(`Energy8JSPluginTools [Storage]: Object store '${storeName}' does not exist`);
                resolve(null);
                return;
              }
              
              try {
                const transaction = this.indexedDB.db.transaction([storeName], "readonly");
                const objectStore = transaction.objectStore(storeName);
                const request = objectStore.getAll();
                
                request.onsuccess = (event) => {
                  if (event.target.result) {
                    console.log(`Energy8JSPluginTools [Storage]: Retrieved all items from store '${storeName}'`);
                    resolve(JSON.stringify(event.target.result));
                  } else {
                    console.log(`Energy8JSPluginTools [Storage]: No items found in store '${storeName}'`);
                    resolve(JSON.stringify([]));
                  }
                };
                
                request.onerror = (event) => {
                  console.error(`Energy8JSPluginTools [Storage]: Error getting all items from store '${storeName}':`, event.target.error);
                  resolve(null);
                };
              } catch (e) {
                console.error(`Energy8JSPluginTools [Storage]: Error in transaction with store '${storeName}':`, e);
                resolve(null);
              }
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in getAllFromStore:", e);
              resolve(null);
            }
          });
        },
        
        // Очистка хранилища объектов
        clearStore: function(storeName) {
          return new Promise((resolve) => {
            try {
              if (!this.indexedDB.db) {
                this.indexedDB.pendingOperations.push(() => {
                  this.clearStore(storeName).then(resolve);
                });
                console.warn("Energy8JSPluginTools [Storage]: Queuing clearStore operation - database not ready");
                return;
              }
              
              if (!this.indexedDB.db.objectStoreNames.contains(storeName)) {
                console.error(`Energy8JSPluginTools [Storage]: Object store '${storeName}' does not exist`);
                resolve(false);
                return;
              }
              
              try {
                const transaction = this.indexedDB.db.transaction([storeName], "readwrite");
                const objectStore = transaction.objectStore(storeName);
                const request = objectStore.clear();
                
                request.onsuccess = () => {
                  console.log(`Energy8JSPluginTools [Storage]: Cleared all items from store '${storeName}'`);
                  resolve(true);
                };
                
                request.onerror = (event) => {
                  console.error(`Energy8JSPluginTools [Storage]: Error clearing store '${storeName}':`, event.target.error);
                  resolve(false);
                };
              } catch (e) {
                console.error(`Energy8JSPluginTools [Storage]: Error in transaction with store '${storeName}':`, e);
                resolve(false);
              }
            } catch (e) {
              console.error("Energy8JSPluginTools [Storage]: Error in clearStore:", e);
              resolve(false);
            }
          });
        },
        
        // Регистрирует обработчики для каналов связи
        _registerHandlers: function() {
          const self = this;
          
          if (Energy8JSPluginTools.Communication) {
            // Основные функции для работы с хранилищами
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'isAvailable',
              (data) => self.isAvailable(data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getItem',
              (data) => self.getItem(data.Key, data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'setItem',
              (data) => self.setItem(data.Key, data.Value, data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'removeItem',
              (data) => self.removeItem(data.Key, data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'hasKey',
              (data) => self.hasKey(data.Key, data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getAllKeys',
              (data) => self.getAllKeys(data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'clear',
              (data) => self.clear(data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getStorageSize',
              (data) => self.getStorageSize(data.StorageType)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getRemainingSpace',
              (data) => self.getRemainingSpace(data.StorageType)
            );
            
            // Функции для работы с IndexedDB
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'openDatabase',
              (data) => self.openDatabase(data.DatabaseName, data.Version)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'createObjectStore',
              (data) => self.createObjectStore(data.StoreName, data.KeyPath)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'addToStore',
              (data) => self.addToStore(data.StoreName, data.Data, data.Key)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getFromStore',
              (data) => self.getFromStore(data.StoreName, data.Key)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'removeFromStore',
              (data) => self.removeFromStore(data.StoreName, data.Key)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'getAllFromStore',
              (data) => self.getAllFromStore(data.StoreName)
            );
            
            Energy8JSPluginTools.Communication.registerChannelHandler(
              this.CHANNEL_PREFIX + 'clearStore',
              (data) => self.clearStore(data.StoreName)
            );
            
            console.log("Energy8JSPluginTools [Storage]: Registered all channel handlers");
          } else {
            console.error("Energy8JSPluginTools [Storage]: Communication module not available");
          }
        }
      };
      
      // Регистрация обработчиков событий
      Energy8JSPluginTools.Storage._registerHandlers();
      
      console.log("Energy8JSPluginTools [Storage]: Module initialized");
      
      // Добавим информацию о доступных командах в консоль
      if (Energy8JSPluginTools._internal.debugMode) {
        console.info(
          "Energy8JSPluginTools.Storage module available.\n" +
          "Try: Energy8JSPluginTools.Storage.getItem('key', 'Local') to get an item from localStorage\n" +
          "     Energy8JSPluginTools.Storage.openDatabase('myDB', 1) to open an IndexedDB database"
        );
      }
      
    } catch (e) {
      console.error("Energy8JSPluginTools [Storage]: Error initializing module:", e);
    }
    
    return 0;
  }
});