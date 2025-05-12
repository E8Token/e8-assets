/**
 * Energy8 JS Plugin Tools - Network Module
 * Модуль для работы с сетевыми запросами и файлами
 */

mergeInto(LibraryManager.library, {
    // Инициализация модуля Network
    __JS_Network_Init: function() {
        try {
            console.log("Energy8JSPluginTools [Network]: Initializing module");
            
            if (!Energy8JSPluginTools || !Energy8JSPluginTools._internal.initialized) {
                console.error("Energy8JSPluginTools [Network]: Core module not initialized. Make sure core.jslib is loaded first.");
                return;
            }
            
            // Инициализация и публичный API для Network
            Energy8JSPluginTools.Network = {
                // Константа для префикса сообщений
                CHANNEL_PREFIX: 'network.',
                
                // Настройки по умолчанию
                settings: {
                    cachingEnabled: false,
                    timeout: 30000, // 30 секунд по умолчанию
                    maxRetries: 3,
                    retryDelay: 1000 // мс
                },
                
                // Кэш для хранения запросов
                cache: {},
                
                // XMLHttpRequest объекты для отслеживания прогресса
                activeRequests: {},
                
                // Генерация уникального ID для запроса
                generateRequestId: function() {
                    return 'e8_req_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
                },
                
                // Проверка, поддерживается ли XMLHttpRequest
                isXHRSupported: function() {
                    return typeof XMLHttpRequest !== 'undefined';
                },
                
                // Проверка, поддерживается ли Fetch API
                isFetchSupported: function() {
                    return typeof fetch !== 'undefined';
                },
                
                // Отправка сетевого запроса с поддержкой повторных попыток
                sendRequest: function(url, method, data, headers, options) {
                    const self = this;
                    let attempt = 0;
                    const maxAttempts = (options && typeof options.maxRetries === 'number') 
                                     ? options.maxRetries 
                                     : this.settings.maxRetries;
                    const retryDelay = (options && typeof options.retryDelay === 'number')
                                     ? options.retryDelay
                                     : this.settings.retryDelay;
                    
                    return new Promise(function(resolve, reject) {
                        function tryRequest() {
                            attempt++;
                            self._sendSingleRequest(url, method, data, headers, options)
                                .then(resolve)
                                .catch(function(error) {
                                    if (attempt < maxAttempts && self._shouldRetry(error)) {
                                        if (Energy8JSPluginTools._internal.debugMode) {
                                            console.log(`Energy8JSPluginTools [Network]: Retrying request to ${url} (attempt ${attempt+1}/${maxAttempts})`);
                                        }
                                        setTimeout(tryRequest, retryDelay);
                                    } else {
                                        reject(error);
                                    }
                                });
                        }
                        
                        tryRequest();
                    });
                },
                
                // Определение, нужно ли повторять запрос при ошибке
                _shouldRetry: function(error) {
                    // Повторяем запрос только при сетевых ошибках или серверных ошибках (кроме 4xx)
                    return !error.statusCode || error.statusCode >= 500;
                },
                
                // Отправка одиночного запроса (без повторов)
                _sendSingleRequest: function(url, method, data, headers, options) {
                    const self = this;
                    options = options || {};
                    
                    return new Promise(function(resolve, reject) {
                        if (!self.isXHRSupported() && !self.isFetchSupported()) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "Network requests not supported in this environment"
                            });
                            return;
                        }
                        
                        // Проверяем обязательные параметры
                        if (!url || !method) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "Missing required parameters: url, method"
                            });
                            return;
                        }
                        
                        method = (method || "GET").toUpperCase();
                        headers = headers || {};
                        data = data || null;
                        
                        // Проверяем, есть ли запрос в кэше
                        const cacheKey = method + "_" + url + "_" + JSON.stringify(data) + "_" + JSON.stringify(headers);
                        if (self.settings.cachingEnabled && method === "GET" && self.cache[cacheKey]) {
                            if (Energy8JSPluginTools._internal.debugMode) {
                                console.log("Energy8JSPluginTools [Network]: Using cached response for", url);
                            }
                            resolve(self.cache[cacheKey]);
                            return;
                        }
                        
                        if (self.isFetchSupported() && options.preferFetch) {
                            self._sendRequestWithFetch(url, method, data, headers, cacheKey, options)
                                .then(resolve)
                                .catch(reject);
                        } else {
                            self._sendRequestWithXHR(url, method, data, headers, cacheKey, options)
                                .then(resolve)
                                .catch(reject);
                        }
                    });
                },
                
                // Отправка запроса с использованием Fetch API
                _sendRequestWithFetch: function(url, method, data, headers, cacheKey, options) {
                    const self = this;
                    const timeout = (options && typeof options.timeout === 'number') 
                                  ? options.timeout 
                                  : self.settings.timeout;
                    
                    return new Promise(function(resolve, reject) {
                        const fetchOptions = {
                            method: method,
                            headers: headers,
                            body: method !== "GET" && method !== "HEAD" ? data : undefined,
                            credentials: 'same-origin',
                            mode: 'cors',
                            cache: 'no-cache'
                        };
                        
                        // Создаем контроллер прерывания для таймаута
                        const controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
                        if (controller) {
                            fetchOptions.signal = controller.signal;
                        }
                        
                        // Устанавливаем таймаут
                        const timeoutId = setTimeout(function() {
                            if (controller) controller.abort();
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Timeout",
                                data: null,
                                headers: {},
                                error: "Request timeout"
                            });
                        }, timeout);
                        
                        fetch(url, fetchOptions)
                            .then(function(response) {
                                clearTimeout(timeoutId);
                                
                                const responseHeaders = {};
                                response.headers.forEach(function(value, key) {
                                    responseHeaders[key] = value;
                                });
                                
                                return response.text().then(function(responseText) {
                                    const responseObj = {
                                        success: response.ok,
                                        statusCode: response.status,
                                        statusText: response.statusText,
                                        data: responseText,
                                        headers: responseHeaders,
                                        error: response.ok ? null : "Request failed with status " + response.status
                                    };
                                    
                                    // Сохраняем в кэш, если включено кэширование и запрос успешен
                                    if (self.settings.cachingEnabled && method === "GET" && response.ok) {
                                        self.cache[cacheKey] = responseObj;
                                    }
                                    
                                    if (response.ok) {
                                        resolve(responseObj);
                                    } else {
                                        reject(responseObj);
                                    }
                                });
                            })
                            .catch(function(error) {
                                clearTimeout(timeoutId);
                                console.error("Energy8JSPluginTools [Network]: Fetch error:", error);
                                reject({
                                    success: false,
                                    statusCode: 0,
                                    statusText: "Error",
                                    data: null,
                                    headers: {},
                                    error: error.message || "Network error occurred"
                                });
                            });
                    });
                },
                
                // Отправка запроса с использованием XMLHttpRequest
                _sendRequestWithXHR: function(url, method, data, headers, cacheKey, options) {
                    const self = this;
                    const timeout = (options && typeof options.timeout === 'number') 
                                  ? options.timeout 
                                  : self.settings.timeout;
                    
                    return new Promise(function(resolve, reject) {
                        const xhr = new XMLHttpRequest();
                        xhr.open(method, url, true);
                        
                        // Устанавливаем заголовки
                        for (const header in headers) {
                            if (headers.hasOwnProperty(header)) {
                                xhr.setRequestHeader(header, headers[header]);
                            }
                        }
                        
                        // Устанавливаем таймаут
                        xhr.timeout = timeout;
                        
                        // Отслеживаем прогресс для больших запросов
                        if (options && options.onProgress) {
                            xhr.onprogress = options.onProgress;
                        }
                        
                        xhr.onload = function() {
                            const responseHeaders = {};
                            const headerString = xhr.getAllResponseHeaders();
                            const headerPairs = headerString.split('\r\n');
                            
                            for (let i = 0; i < headerPairs.length; i++) {
                                const headerPair = headerPairs[i];
                                if (headerPair.trim().length === 0) continue;
                                const index = headerPair.indexOf(': ');
                                if (index > 0) {
                                    const key = headerPair.substring(0, index).toLowerCase();
                                    const val = headerPair.substring(index + 2);
                                    responseHeaders[key] = val;
                                }
                            }
                            
                            const responseObj = {
                                success: xhr.status >= 200 && xhr.status < 300,
                                statusCode: xhr.status,
                                statusText: xhr.statusText,
                                data: xhr.responseText,
                                headers: responseHeaders,
                                error: xhr.status >= 200 && xhr.status < 300 ? null : "Request failed with status " + xhr.status
                            };
                            
                            // Сохраняем в кэш, если включено кэширование и запрос успешен
                            if (self.settings.cachingEnabled && method === "GET" && xhr.status >= 200 && xhr.status < 300) {
                                self.cache[cacheKey] = responseObj;
                            }
                            
                            if (xhr.status >= 200 && xhr.status < 300) {
                                resolve(responseObj);
                            } else {
                                reject(responseObj);
                            }
                        };
                        
                        xhr.onerror = function() {
                            console.error("Energy8JSPluginTools [Network]: XHR error occurred");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "Network error occurred"
                            });
                        };
                        
                        xhr.ontimeout = function() {
                            console.error("Energy8JSPluginTools [Network]: XHR timeout");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Timeout",
                                data: null,
                                headers: {},
                                error: "Request timeout"
                            });
                        };
                        
                        xhr.send(method !== "GET" && method !== "HEAD" ? data : null);
                    });
                },
                
                // Загрузка файла
                downloadFile: function(url, options) {
                    const self = this;
                    options = options || {};
                    
                    return new Promise(function(resolve, reject) {
                        if (!self.isXHRSupported()) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                binaryDataBase64: null,
                                headers: {},
                                error: "File download not supported in this environment"
                            });
                            return;
                        }
                        
                        // Проверяем обязательные параметры
                        if (!url) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                binaryDataBase64: null,
                                headers: {},
                                error: "Missing required parameter: url"
                            });
                            return;
                        }
                        
                        const requestId = self.generateRequestId();
                        
                        const xhr = new XMLHttpRequest();
                        xhr.open("GET", url, true);
                        xhr.responseType = "arraybuffer";
                        
                        // Устанавливаем таймаут
                        xhr.timeout = options.timeout || self.settings.timeout;
                        
                        // Сохраняем запрос для отслеживания прогресса
                        self.activeRequests[requestId] = xhr;
                        
                        // Отслеживаем прогресс загрузки
                        xhr.onprogress = function(event) {
                            if (event.lengthComputable) {
                                const progress = event.loaded / event.total;
                                if (options.onProgress) {
                                    options.onProgress(progress);
                                }
                                
                                if (Energy8JSPluginTools.Communication) {
                                    Energy8JSPluginTools.Communication.send(
                                        self.CHANNEL_PREFIX + 'downloadProgress',
                                        { 
                                            id: requestId,
                                            url: url,
                                            loaded: event.loaded,
                                            total: event.total,
                                            percent: progress
                                        }
                                    );
                                }
                            }
                        };
                        
                        xhr.onload = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            const responseHeaders = {};
                            const headerString = xhr.getAllResponseHeaders();
                            const headerPairs = headerString.split('\r\n');
                            
                            for (let i = 0; i < headerPairs.length; i++) {
                                const headerPair = headerPairs[i];
                                if (headerPair.trim().length === 0) continue;
                                const index = headerPair.indexOf(': ');
                                if (index > 0) {
                                    const key = headerPair.substring(0, index).toLowerCase();
                                    const val = headerPair.substring(index + 2);
                                    responseHeaders[key] = val;
                                }
                            }
                            
                            if (xhr.status >= 200 && xhr.status < 300) {
                                // Преобразуем бинарные данные в Base64
                                const bytes = new Uint8Array(xhr.response);
                                let binary = '';
                                for (let i = 0; i < bytes.byteLength; i++) {
                                    binary += String.fromCharCode(bytes[i]);
                                }
                                const base64Data = btoa(binary);
                                
                                resolve({
                                    success: true,
                                    statusCode: xhr.status,
                                    statusText: xhr.statusText,
                                    data: null,
                                    binaryDataBase64: base64Data,
                                    headers: responseHeaders,
                                    error: null
                                });
                            } else {
                                reject({
                                    success: false,
                                    statusCode: xhr.status,
                                    statusText: xhr.statusText,
                                    data: null,
                                    binaryDataBase64: null,
                                    headers: responseHeaders,
                                    error: "Request failed with status " + xhr.status
                                });
                            }
                        };
                        
                        xhr.onerror = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            console.error("Energy8JSPluginTools [Network]: XHR error occurred during file download");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                binaryDataBase64: null,
                                headers: {},
                                error: "Network error occurred"
                            });
                        };
                        
                        xhr.ontimeout = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            console.error("Energy8JSPluginTools [Network]: XHR timeout during file download");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Timeout",
                                data: null,
                                binaryDataBase64: null,
                                headers: {},
                                error: "Request timeout"
                            });
                        };
                        
                        xhr.send();
                    });
                },
                
                // Загрузка файла на сервер
                uploadFile: function(url, fileDataBase64, fileName, options) {
                    const self = this;
                    options = options || {};
                    
                    return new Promise(function(resolve, reject) {
                        if (!self.isXHRSupported()) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "File upload not supported in this environment"
                            });
                            return;
                        }
                        
                        // Проверяем обязательные параметры
                        if (!url || !fileDataBase64 || !fileName) {
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "Missing required parameters: url, fileDataBase64, fileName"
                            });
                            return;
                        }
                        
                        const fileField = options.fileField || "file";
                        const formData = options.formData || {};
                        const requestId = self.generateRequestId();
                        
                        // Преобразуем Base64 в бинарные данные
                        const byteCharacters = atob(fileDataBase64);
                        const byteNumbers = new Array(byteCharacters.length);
                        for (let i = 0; i < byteCharacters.length; i++) {
                            byteNumbers[i] = byteCharacters.charCodeAt(i);
                        }
                        const byteArray = new Uint8Array(byteNumbers);
                        
                        // Определение типа файла на основе расширения
                        let fileType = "application/octet-stream";
                        const fileExtension = fileName.split('.').pop().toLowerCase();
                        
                        switch (fileExtension) {
                            case 'jpg':
                            case 'jpeg':
                                fileType = 'image/jpeg';
                                break;
                            case 'png':
                                fileType = 'image/png';
                                break;
                            case 'gif':
                                fileType = 'image/gif';
                                break;
                            case 'pdf':
                                fileType = 'application/pdf';
                                break;
                            case 'txt':
                                fileType = 'text/plain';
                                break;
                            case 'doc':
                            case 'docx':
                                fileType = 'application/msword';
                                break;
                            case 'xls':
                            case 'xlsx':
                                fileType = 'application/vnd.ms-excel';
                                break;
                            case 'zip':
                                fileType = 'application/zip';
                                break;
                            case 'json':
                                fileType = 'application/json';
                                break;
                            case 'xml':
                                fileType = 'application/xml';
                                break;
                            case 'html':
                            case 'htm':
                                fileType = 'text/html';
                                break;
                            case 'css':
                                fileType = 'text/css';
                                break;
                            case 'js':
                                fileType = 'application/javascript';
                                break;
                        }
                        
                        const fileBlob = new Blob([byteArray], { type: fileType });
                        const formDataObj = new FormData();
                        
                        // Добавляем файл в форму
                        formDataObj.append(fileField, fileBlob, fileName);
                        
                        // Добавляем дополнительные поля формы
                        for (const key in formData) {
                            if (formData.hasOwnProperty(key)) {
                                formDataObj.append(key, formData[key]);
                            }
                        }
                        
                        const xhr = new XMLHttpRequest();
                        xhr.open("POST", url, true);
                        
                        // Устанавливаем таймаут
                        xhr.timeout = options.timeout || self.settings.timeout;
                        
                        // Сохраняем запрос для отслеживания прогресса
                        self.activeRequests[requestId] = xhr;
                        
                        // Отслеживаем прогресс загрузки
                        xhr.upload.onprogress = function(event) {
                            if (event.lengthComputable) {
                                const progress = event.loaded / event.total;
                                if (options.onProgress) {
                                    options.onProgress(progress);
                                }
                                
                                if (Energy8JSPluginTools.Communication) {
                                    Energy8JSPluginTools.Communication.send(
                                        self.CHANNEL_PREFIX + 'uploadProgress',
                                        { 
                                            id: requestId,
                                            url: url,
                                            loaded: event.loaded,
                                            total: event.total,
                                            percent: progress
                                        }
                                    );
                                }
                            }
                        };
                        
                        xhr.onload = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            const responseHeaders = {};
                            const headerString = xhr.getAllResponseHeaders();
                            const headerPairs = headerString.split('\r\n');
                            
                            for (let i = 0; i < headerPairs.length; i++) {
                                const headerPair = headerPairs[i];
                                if (headerPair.trim().length === 0) continue;
                                const index = headerPair.indexOf(': ');
                                if (index > 0) {
                                    const key = headerPair.substring(0, index).toLowerCase();
                                    const val = headerPair.substring(index + 2);
                                    responseHeaders[key] = val;
                                }
                            }
                            
                            const responseObj = {
                                success: xhr.status >= 200 && xhr.status < 300,
                                statusCode: xhr.status,
                                statusText: xhr.statusText,
                                data: xhr.responseText,
                                headers: responseHeaders,
                                error: xhr.status >= 200 && xhr.status < 300 ? null : "Request failed with status " + xhr.status
                            };
                            
                            if (xhr.status >= 200 && xhr.status < 300) {
                                resolve(responseObj);
                            } else {
                                reject(responseObj);
                            }
                        };
                        
                        xhr.onerror = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            console.error("Energy8JSPluginTools [Network]: XHR error occurred during file upload");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Error",
                                data: null,
                                headers: {},
                                error: "Network error occurred"
                            });
                        };
                        
                        xhr.ontimeout = function() {
                            // Удаляем запрос из активных
                            delete self.activeRequests[requestId];
                            
                            console.error("Energy8JSPluginTools [Network]: XHR timeout during file upload");
                            reject({
                                success: false,
                                statusCode: 0,
                                statusText: "Timeout",
                                data: null,
                                headers: {},
                                error: "Request timeout"
                            });
                        };
                        
                        xhr.send(formDataObj);
                    });
                },
                
                // Проверка наличия сетевого подключения
                isOnline: function() {
                    try {
                        return (typeof navigator !== 'undefined' && navigator.onLine) || false;
                    } catch (error) {
                        console.error("Energy8JSPluginTools [Network]: Error checking online status:", error);
                        return false;
                    }
                },
                
                // Установка флага кэширования
                setCaching: function(enabled) {
                    try {
                        if (typeof enabled === 'boolean') {
                            this.settings.cachingEnabled = enabled;
                            console.log(`Energy8JSPluginTools [Network]: Caching ${enabled ? "enabled" : "disabled"}`);
                            
                            // Если кэширование отключено, очищаем кэш
                            if (!enabled) {
                                this.clearCache();
                            }
                            
                            return true;
                        }
                        return false;
                    } catch (error) {
                        console.error("Energy8JSPluginTools [Network]: Error setting caching:", error);
                        return false;
                    }
                },
                
                // Установка таймаута для запросов
                setTimeout: function(timeoutInSeconds) {
                    try {
                        if (typeof timeoutInSeconds === 'number' && timeoutInSeconds > 0) {
                            this.settings.timeout = timeoutInSeconds * 1000;
                            console.log(`Energy8JSPluginTools [Network]: Timeout set to ${timeoutInSeconds} seconds`);
                            return true;
                        }
                        return false;
                    } catch (error) {
                        console.error("Energy8JSPluginTools [Network]: Error setting timeout:", error);
                        return false;
                    }
                },
                
                // Очистка кэша запросов
                clearCache: function() {
                    try {
                        this.cache = {};
                        console.log("Energy8JSPluginTools [Network]: Cache cleared");
                        return true;
                    } catch (error) {
                        console.error("Energy8JSPluginTools [Network]: Error clearing cache:", error);
                        return false;
                    }
                },
                
                // Отмена всех активных запросов
                cancelAllRequests: function() {
                    try {
                        let count = 0;
                        for (const requestId in this.activeRequests) {
                            if (this.activeRequests.hasOwnProperty(requestId)) {
                                try {
                                    this.activeRequests[requestId].abort();
                                    count++;
                                } catch (err) {
                                    console.error(`Energy8JSPluginTools [Network]: Error aborting request ${requestId}:`, err);
                                }
                                delete this.activeRequests[requestId];
                            }
                        }
                        
                        console.log(`Energy8JSPluginTools [Network]: Cancelled ${count} active requests`);
                        return count;
                    } catch (error) {
                        console.error("Energy8JSPluginTools [Network]: Error cancelling requests:", error);
                        return 0;
                    }
                },
                
                // Регистрация обработчиков для каналов связи
                _registerHandlers: function() {
                    const self = this;
                    
                    if (Energy8JSPluginTools.Communication) {
                        // Регистрация обработчиков для каждого типа запроса
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'sendRequest',
                            (data) => {
                                return self.sendRequest(
                                    data.url, 
                                    data.method, 
                                    data.data, 
                                    data.headers, 
                                    data.options
                                );
                            }
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'downloadFile',
                            (data) => {
                                return self.downloadFile(data.url, data.options);
                            }
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'uploadFile',
                            (data) => {
                                return self.uploadFile(
                                    data.url, 
                                    data.fileDataBase64, 
                                    data.fileName, 
                                    {
                                        fileField: data.fileField,
                                        formData: data.formData,
                                        timeout: data.timeout
                                    }
                                );
                            }
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'isOnline',
                            () => self.isOnline()
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'setCaching',
                            (data) => self.setCaching(data.enabled)
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'setTimeout',
                            (data) => self.setTimeout(data.timeoutInSeconds)
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'clearCache',
                            () => self.clearCache()
                        );
                        
                        Energy8JSPluginTools.Communication.registerChannelHandler(
                            this.CHANNEL_PREFIX + 'cancelAllRequests',
                            () => self.cancelAllRequests()
                        );
                        
                        if (Energy8JSPluginTools._internal.debugMode) {
                            console.log("Energy8JSPluginTools [Network]: Registered all channel handlers");
                        }
                    } else {
                        console.error("Energy8JSPluginTools [Network]: Communication module not available");
                    }
                }
            };
            
            // Регистрация обработчиков и автоматическая настройка сетевых событий
            Energy8JSPluginTools.Network._registerHandlers();
            
            // Настройка обработчиков изменения состояния сети
            if (typeof window !== 'undefined') {
                window.addEventListener('online', function() {
                    if (Energy8JSPluginTools.Communication) {
                        Energy8JSPluginTools.Communication.send(
                            Energy8JSPluginTools.Network.CHANNEL_PREFIX + 'onlineStatusChanged',
                            { online: true }
                        );
                    }
                    console.log("Energy8JSPluginTools [Network]: Browser is online");
                });
                
                window.addEventListener('offline', function() {
                    if (Energy8JSPluginTools.Communication) {
                        Energy8JSPluginTools.Communication.send(
                            Energy8JSPluginTools.Network.CHANNEL_PREFIX + 'onlineStatusChanged',
                            { online: false }
                        );
                    }
                    console.log("Energy8JSPluginTools [Network]: Browser is offline");
                });
            }
            
            console.log("Energy8JSPluginTools [Network]: Module initialized");
            
            // Добавим информацию о доступных командах в консоль
            if (Energy8JSPluginTools._internal.debugMode) {
                console.info(
                    "Energy8JSPluginTools.Network module available.\n" +
                    "Try: Energy8JSPluginTools.Network.sendRequest('https://example.com', 'GET') to send a network request\n" +
                    "     Energy8JSPluginTools.Network.isOnline() to check if browser is online"
                );
            }
            
        } catch (error) {
            console.error("Energy8JSPluginTools [Network]: Error initializing module:", error);
        }
        
        return 0;
    }
});