#if UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.JSPluginTools
{
    /// <summary>
    /// WebGL log handler that sends log messages to the browser console
    /// </summary>
    public class JSLogHandler : ILogHandler
    {
        [DllImport("__Internal")]
        private static extern void WebGLConsoleLog(string message);

        [DllImport("__Internal")]
        private static extern void WebGLConsoleWarn(string message);

        [DllImport("__Internal")]
        private static extern void WebGLConsoleError(string message);

        /// <summary>
        /// Formats and handles log messages of different types
        /// </summary>
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            string message = string.Format(format, args);
            string formattedMessage = FormatMessage(logType, message);
            
            switch (logType)
            {
                case LogType.Log:
                    WebGLConsoleLog(formattedMessage);
                    break;
                case LogType.Warning:
                    WebGLConsoleWarn(formattedMessage);
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    WebGLConsoleError(formattedMessage);
                    break;
            }
        }

        /// <summary>
        /// Handles exceptions by formatting and sending them to the browser console
        /// </summary>
        public void LogException(System.Exception exception, Object context)
        {
            string formattedMessage = FormatMessage(LogType.Exception, exception.ToString());
            WebGLConsoleError(formattedMessage);
        }

        /// <summary>
        /// Formats a log message with timestamp
        /// </summary>
        protected string FormatMessage(LogType logType, string message)
        {
            return $"[{System.DateTime.Now:HH:mm:ss.fff}] {message}";
        }
    }
}
#endif
