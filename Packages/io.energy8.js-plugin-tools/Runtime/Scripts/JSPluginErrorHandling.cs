using System;
using System.Collections.Generic;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// Error handling module for JSPluginTools.
    /// Provides centralized error handling and logging functionality across all modules.
    /// </summary>
    public static class JSPluginErrorHandling
    {
        private static bool isInitialized = false;
        private static Dictionary<string, Action<ErrorInfo>> globalErrorHandlers = new Dictionary<string, Action<ErrorInfo>>();
        
        /// <summary>
        /// Error severity levels
        /// </summary>
        public enum ErrorSeverity
        {
            /// <summary>Information message, not an error</summary>
            Info,
            
            /// <summary>Warning that doesn't prevent operation but might indicate a problem</summary>
            Warning,
            
            /// <summary>Standard error that affects a specific operation</summary>
            Error,
            
            /// <summary>Critical error that affects a major subsystem</summary>
            Critical
        }
        
        /// <summary>
        /// Container for error information
        /// </summary>
        public class ErrorInfo
        {
            /// <summary>Source module of the error</summary>
            public string Source { get; set; }
            
            /// <summary>Context where the error occurred</summary>
            public string Context { get; set; }
            
            /// <summary>Error message</summary>
            public string Message { get; set; }
            
            /// <summary>Error severity level</summary>
            public ErrorSeverity Severity { get; set; }
            
            /// <summary>Stack trace if available</summary>
            public string StackTrace { get; set; }
            
            /// <summary>Associated object ID if any</summary>
            public string ObjectId { get; set; }
            
            /// <summary>Timestamp when the error occurred</summary>
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }
        
        /// <summary>
        /// Initializes the error handling module
        /// </summary>
        /// <returns>True if initialization was successful</returns>
        public static bool Initialize()
        {
            if (isInitialized)
                return true;
                
            // Any initialization logic would go here
            
            isInitialized = true;
            LogEvent("JSPluginErrorHandling", "Error handling module initialized", ErrorSeverity.Info);
            return true;
        }
        
        /// <summary>
        /// Shuts down the error handling module
        /// </summary>
        public static void Shutdown()
        {
            if (!isInitialized)
                return;
                
            globalErrorHandlers.Clear();
            isInitialized = false;
        }
        
        /// <summary>
        /// Processes an exception through the error handling system
        /// </summary>
        /// <param name="source">Source module name</param>
        /// <param name="context">Context where the exception occurred</param>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="severity">Error severity</param>
        /// <param name="objectId">Optional associated object ID</param>
        public static void ProcessException(string source, string context, Exception exception, ErrorSeverity severity = ErrorSeverity.Error, string objectId = null)
        {
            if (exception == null)
            {
                ProcessError(source, context, "Null exception", severity, objectId: objectId);
                return;
            }
            
            ProcessError(
                source,
                context,
                exception.Message,
                severity,
                exception.StackTrace,
                objectId
            );
        }
        
        /// <summary>
        /// Processes an error through the error handling system
        /// </summary>
        /// <param name="source">Source module name</param>
        /// <param name="context">Context where the error occurred</param>
        /// <param name="message">Error message</param>
        /// <param name="severity">Error severity</param>
        /// <param name="stackTrace">Optional stack trace</param>
        /// <param name="objectId">Optional associated object ID</param>
        public static void ProcessError(string source, string context, string message, ErrorSeverity severity = ErrorSeverity.Error, string stackTrace = null, string objectId = null)
        {
            if (!isInitialized)
                Initialize();
                
            // Format the log message
            string logPrefix = $"[{source}] ";
            string logMessage = $"{context}: {message}";
            
            // Log to Unity console based on severity
            switch (severity)
            {
                case ErrorSeverity.Info:
                    Debug.Log(logPrefix + logMessage);
                    break;
                case ErrorSeverity.Warning:
                    Debug.LogWarning(logPrefix + logMessage);
                    break;
                case ErrorSeverity.Error:
                case ErrorSeverity.Critical:
                    Debug.LogError(logPrefix + logMessage + (string.IsNullOrEmpty(stackTrace) ? "" : "\n" + stackTrace));
                    break;
            }
            
            // Create error info object
            var errorInfo = new ErrorInfo
            {
                Source = source,
                Context = context,
                Message = message,
                Severity = severity,
                StackTrace = stackTrace,
                ObjectId = objectId
            };
            
            // Notify all registered global error handlers
            foreach (var handler in globalErrorHandlers.Values)
            {
                try
                {
                    handler?.Invoke(errorInfo);
                }
                catch (Exception ex)
                {
                    // Don't use ProcessError here to avoid potential infinite recursion
                    Debug.LogError($"[JSPluginErrorHandling] Error in error handler: {ex.Message}");
                }
            }
            
            // If we have an object ID, try to report the error to JavaScript
            if (!string.IsNullOrEmpty(objectId))
            {
                var pluginObject = JSPluginCore.GetObject(objectId);
                pluginObject?.ReportError(context, message);
            }
        }
        
        /// <summary>
        /// Logs an event message through the error handling system
        /// </summary>
        /// <param name="source">Source module name</param>
        /// <param name="message">Event message</param>
        /// <param name="severity">Message severity</param>
        /// <param name="objectId">Optional associated object ID</param>
        public static void LogEvent(string source, string message, ErrorSeverity severity = ErrorSeverity.Info, string objectId = null)
        {
            ProcessError(source, "Event", message, severity, objectId: objectId);
        }
        
        /// <summary>
        /// Adds a global error handler that will be called for all errors
        /// </summary>
        /// <param name="handlerId">Unique identifier for this handler</param>
        /// <param name="handler">Handler function</param>
        public static void AddGlobalErrorHandler(string handlerId, Action<ErrorInfo> handler)
        {
            if (string.IsNullOrEmpty(handlerId))
            {
                LogEvent("JSPluginErrorHandling", "Handler ID cannot be null or empty", ErrorSeverity.Warning);
                return;
            }
            
            if (handler == null)
            {
                LogEvent("JSPluginErrorHandling", "Handler function cannot be null", ErrorSeverity.Warning);
                return;
            }
            
            globalErrorHandlers[handlerId] = handler;
        }
        
        /// <summary>
        /// Removes a previously registered global error handler
        /// </summary>
        /// <param name="handlerId">Identifier of the handler to remove</param>
        /// <returns>True if the handler was found and removed</returns>
        public static bool RemoveGlobalErrorHandler(string handlerId)
        {
            if (string.IsNullOrEmpty(handlerId))
                return false;
                
            return globalErrorHandlers.Remove(handlerId);
        }
    }
}
