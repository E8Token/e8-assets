using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.Analytics.Core.Providers
{
    public interface IAnalyticsProvider
    {
        bool IsInitialized { get; }
        UniTask Initialize(CancellationToken ct);
        
        // Basic logging methods
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        
        // User management
        void SetUserId(string userId);
        void SetUserProperties(Dictionary<string, object> properties);
        
        // Data management
        void ResetAnalyticsData();
        
        // Error handling
        event Action<string> OnError;
    }
}
