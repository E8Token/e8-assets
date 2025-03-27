using System;
using UnityEngine;

namespace Energy8.Identity.Core.Logging
{
    public class Logger<T> : ILogger<T>
    {
        private readonly string scope;
        private const string TimeFormat = "HH:mm:ss.fff";

        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
        public static bool ShowFullTypeName { get; set; } = false;

        public Logger(string scope = null)
        {
            this.scope = scope;
        }

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel;

        private string FormatLogLevel(LogLevel level) => level switch
        {
            LogLevel.Debug => "<mark=#222266AA><color=#FFFFFF>  DEBUG  </color></mark>",
            LogLevel.Information => "<mark=#225522AA><color=#FFFFFF>  INFO   </color></mark>",
            LogLevel.Warning => "<mark=#666622AA><color=#FFFFFF>  WARN   </color></mark>",
            LogLevel.Error => "<mark=#662222AA><color=#FFFFFF>  ERROR  </color></mark>",
            _ => level.ToString()
        };

        private string GetTypeDisplayName()
        {
            var type = typeof(T);
            return ShowFullTypeName ? type.FullName : type.Name;
        }

        public void Log(LogLevel level, object header, object message = null)
        {
            if (!IsEnabled(level)) return;

            var time = DateTime.Now.ToString(TimeFormat);
            var typeName = GetTypeDisplayName();
            var levelFormat = FormatLogLevel(level);
            
            var scopeDisplay = string.IsNullOrEmpty(scope) ? "" : $" [{scope}]";
            var headerText = header?.ToString() ?? "";
            
            // Format multiline messages with indentation
            var messageText = "";
            if (message != null)
            {
                var lines = message.ToString().Split('\n');
                messageText = lines.Length > 1
                    ? $"\n  {string.Join("\n  ", lines)}"
                    : message.ToString();
            }

            var logMessage = $"[{time}] {levelFormat}{scopeDisplay} <b>{typeName}</b>\n" +
                             $"  <b>{headerText}</b>" +
                             (message != null ? $"{messageText}" : "");

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Information:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
            }
        }

        public void LogException(Exception ex, object message = null)
        {
            if (!IsEnabled(LogLevel.Error)) return;

            var contextMessage = message != null ? $"{message}" : "Exception occurred";
            
            // Format exception information
            var exceptionDetails = FormatException(ex);
            
            Log(LogLevel.Error, contextMessage, exceptionDetails);
        }
        
        private string FormatException(Exception ex)
        {
            if (ex == null) return "null";
            
            return $"Exception: {ex.GetType().Name}\n" +
                   $"Message: {ex.Message}\n" + 
                   $"StackTrace:\n  {ex.StackTrace?.Replace("\n", "\n  ")}\n" +
                   (ex.InnerException != null ? $"Inner: {FormatException(ex.InnerException)}" : "");
        }
        
        // Extension methods for common log levels
        public void Debug(object message) => Log(LogLevel.Debug, message);
        public void Debug(object header, object details) => Log(LogLevel.Debug, header, details);
        
        public void Info(object message) => Log(LogLevel.Information, message);
        public void Info(object header, object details) => Log(LogLevel.Information, header, details);
        
        public void Warning(object message) => Log(LogLevel.Warning, message);
        public void Warning(object header, object details) => Log(LogLevel.Warning, header, details);
        
        public void Error(object message) => Log(LogLevel.Error, message);
        public void Error(object header, object details) => Log(LogLevel.Error, header, details);
    }
}