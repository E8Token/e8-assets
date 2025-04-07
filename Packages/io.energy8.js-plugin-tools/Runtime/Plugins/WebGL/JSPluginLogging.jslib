var JSPluginLogging = {
    // Using shared state with Core
    $JSPluginState: null,
    $JSPluginHelper: null,
    
    /**
     * Logging-specific state object
     * @private
     */
    $LoggingState: {
        logHandlers: [],
        logColors: {
            log: 'color: #ffffff',
            warning: 'color: #ffcc00; font-weight: bold',
            error: 'color: #ff0000; font-weight: bold',
            exception: 'color: #ff00ff; font-weight: bold; text-decoration: underline'
        }
    },
    
    /**
     * Helper functions for logging
     * @private
     */
    $LoggingHelper: {
        /**
         * Formats a log message with timestamp and styling
         * @param {string} type - Log type (log, warning, error, exception)
         * @param {string} message - The message to format
         * @param {string} stackTrace - Optional stack trace
         * @return {Object} Formatted log data object
         */
        formatLog: function(type, message, stackTrace) {
            var timeStr = new Date().toTimeString().split(' ')[0];
            var color = LoggingState.logColors[type] || LoggingState.logColors.log;
            return {
                formatted: `%c[Unity ${timeStr}] ${message}`,
                style: color,
                raw: message,
                stackTrace: stackTrace,
                type: type,
                time: new Date()
            };
        },
        
        /**
         * Dispatches a log message to console and registered handlers
         * @param {string} type - Log type (log, warning, error, exception)
         * @param {string} message - The message to log
         * @param {string} stackTrace - Optional stack trace
         */
        dispatchLog: function(type, message, stackTrace) {
            var logData = this.formatLog(type, message, stackTrace);
            
            // Output to console
            console[type === 'warning' ? 'warn' : type === 'exception' ? 'error' : type](
                logData.formatted, 
                logData.style, 
                stackTrace ? '\n' + stackTrace : ''
            );
            
            // Invoke all registered handlers
            for (var i = 0; i < LoggingState.logHandlers.length; i++) {
                try {
                    LoggingState.logHandlers[i](logData);
                } catch (e) {
                    console.error("Error in log handler:", e);
                }
            }
        }
    },
    
    /**
     * Initializes the logging module and extends the UnityJSTools global object
     * @return {number} 1 if successful, 0 otherwise
     */
    JSPluginInitializeLogging: function() {
        if (typeof window !== 'undefined' && window.UnityJSTools) {
            try {
                // Add logging methods
                /**
                 * Registers a callback to be notified of all log messages
                 * @param {Function} handler - The handler function that receives log data
                 * @return {Function} Function that when called will unregister the handler
                 */
                window.UnityJSTools.onLog = function(handler) {
                    if (typeof handler === 'function') {
                        LoggingState.logHandlers.push(handler);
                        return function() {
                            var idx = LoggingState.logHandlers.indexOf(handler);
                            if (idx >= 0) LoggingState.logHandlers.splice(idx, 1);
                        };
                    }
                    return null;
                };
                
                /**
                 * Logs an informational message
                 * @param {string} message - The message to log
                 */
                window.UnityJSTools.log = function(message) {
                    LoggingHelper.dispatchLog('log', message);
                };
                
                /**
                 * Logs a warning message
                 * @param {string} message - The warning message to log
                 */
                window.UnityJSTools.warn = function(message) {
                    LoggingHelper.dispatchLog('warning', message);
                };
                
                /**
                 * Logs an error message
                 * @param {string} message - The error message to log
                 */
                window.UnityJSTools.error = function(message) {
                    LoggingHelper.dispatchLog('error', message);
                };
                
                return 1;
            } catch (error) {
                console.error("[UnityJSTools] Error initializing logging: " + error);
                return 0;
            }
        }
        return 0;
    },
    
    /**
     * Logs an informational message from Unity
     * @param {string} message - Pointer to message string
     * @param {string} stackTrace - Pointer to stack trace string or null
     */
    JSPluginLog: function(message, stackTrace) {
        var msg = UTF8ToString(message);
        var stack = stackTrace ? UTF8ToString(stackTrace) : null;
        LoggingHelper.dispatchLog('log', msg, stack);
    },
    
    /**
     * Logs a warning message from Unity
     * @param {string} message - Pointer to message string
     * @param {string} stackTrace - Pointer to stack trace string or null
     */
    JSPluginLogWarning: function(message, stackTrace) {
        var msg = UTF8ToString(message);
        var stack = stackTrace ? UTF8ToString(stackTrace) : null;
        LoggingHelper.dispatchLog('warning', msg, stack);
    },
    
    /**
     * Logs an error message from Unity
     * @param {string} message - Pointer to message string
     * @param {string} stackTrace - Pointer to stack trace string or null
     */
    JSPluginLogError: function(message, stackTrace) {
        var msg = UTF8ToString(message);
        var stack = stackTrace ? UTF8ToString(stackTrace) : null;
        LoggingHelper.dispatchLog('error', msg, stack);
    },
    
    /**
     * Logs an exception from Unity
     * @param {string} message - Pointer to exception message string
     * @param {string} stackTrace - Pointer to stack trace string or null
     */
    JSPluginLogException: function(message, stackTrace) {
        var msg = UTF8ToString(message);
        var stack = stackTrace ? UTF8ToString(stackTrace) : null;
        LoggingHelper.dispatchLog('exception', msg, stack);
    }
};

// Register internal variables
autoAddDeps(JSPluginLogging, '$LoggingState');
autoAddDeps(JSPluginLogging, '$LoggingHelper');
mergeInto(LibraryManager.library, JSPluginLogging);
