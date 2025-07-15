using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Energy8.Identity.Analytics.Core.Services
{
    /// <summary>
    /// Interface for analytics service that provides high-level analytics operations
    /// </summary>
    public interface IAnalyticsService
    {
        bool IsInitialized { get; }
        UniTask Initialize(CancellationToken ct);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
        void SetUserId(string userId);
        void SetUserProperties(Dictionary<string, object> properties);
        void ResetAnalyticsData();
        
        // Common events
        void LogSignIn(string method);
        void LogSignOut();
        void LogScreenView(string screenName);
        void LogUserAction(string action, Dictionary<string, object> parameters = null);
    }
}
