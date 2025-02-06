using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Energy8.Identity.Core.Logging
{
    public class Logger<T> : ILogger<T>
    {
        private readonly string scope;
        private const string TimeFormat = "HH:mm:ss.fff";

        public static LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

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

        public void Log(LogLevel level, object header, object message = null)
        {
            if (!IsEnabled(level)) return;

            var time = DateTime.Now.ToString(TimeFormat);
            var typeName = typeof(T).FullName;
            var levelFormat = FormatLogLevel(level);

            var logMessage = $"<b>{levelFormat}</b> [{time}]\n" +
                             $"<b>{header}</b>\n" +
                             (message is null ? "" : $"{message}\n") +
                             $"{typeName,-20} {scope}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(logMessage);
                    break;
            }
        }

        public void LogException(Exception ex, object message = null)
        {
            var logMessage = message != null
                ? $"{message}\n{ex}"
                : ex.ToString();

            Log(LogLevel.Error, logMessage);
        }
    }
}