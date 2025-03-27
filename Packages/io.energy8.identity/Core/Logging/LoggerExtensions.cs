namespace Energy8.Identity.Core.Logging
{
    public static class LoggerExtensions
    {
        public static void LogDebug<T>(this ILogger<T> logger, object header, object message = null) => 
            logger.Log(LogLevel.Debug, header, message);
            
        public static void LogInfo<T>(this ILogger<T> logger, object header, object message = null) => 
            logger.Log(LogLevel.Information, header, message);
            
        public static void LogWarning<T>(this ILogger<T> logger, object header, object message = null) => 
            logger.Log(LogLevel.Warning, header, message);
            
        public static void LogError<T>(this ILogger<T> logger, object header, object message = null) => 
            logger.Log(LogLevel.Error, header, message);
    }
}