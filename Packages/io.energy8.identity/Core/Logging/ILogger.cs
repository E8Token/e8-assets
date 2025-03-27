using System;

namespace Energy8.Identity.Core.Logging
{
    public interface ILogger<T>
    {
        public void Log(LogLevel level, object header, object message = null);
        void LogException(Exception ex, object message = null);
        bool IsEnabled(LogLevel level);
    }
}