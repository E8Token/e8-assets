using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;

namespace Energy8.Identity.Core.Analytics.Providers
{
    public interface IAnalyticsProvider
    {
        bool IsInitialized { get; }
        UniTask Initialize(CancellationToken ct);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserId(string userId);
        void SetUserProperties(Dictionary<string, object> properties);
        void ResetAnalyticsData();
    }

    // Default implementation for non-WebGL platforms
    public class DefaultAnalyticsProvider : IAnalyticsProvider
    {
        private readonly ILogger<DefaultAnalyticsProvider> logger = new Logger<DefaultAnalyticsProvider>();
        
        public bool IsInitialized { get; private set; }

        public UniTask Initialize(CancellationToken ct)
        {
            IsInitialized = true;
            logger.LogInfo("Default Analytics Provider initialized (no-op)");
            return UniTask.CompletedTask;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            logger.LogInfo($"[No-op] Logged event: {eventName}");
        }

        public void SetUserId(string userId)
        {
            logger.LogInfo($"[No-op] Set user ID: {userId}");
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            logger.LogInfo("[No-op] Set user properties");
        }

        public void ResetAnalyticsData()
        {
            logger.LogInfo("[No-op] Reset analytics data");
        }
    }
}
