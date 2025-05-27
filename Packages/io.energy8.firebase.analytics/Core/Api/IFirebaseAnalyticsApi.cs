using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Energy8.Firebase.Analytics.Api
{
    public interface IFirebaseAnalyticsApi
    {
        /// <summary>
        /// Log an event with the given name
        /// </summary>
        Task LogEventAsync(string eventName, CancellationToken ct = default);

        /// <summary>
        /// Log an event with the given name and parameters
        /// </summary>
        Task LogEventAsync(string eventName, Dictionary<string, object> parameters, CancellationToken ct = default);

        /// <summary>
        /// Set user ID for analytics
        /// </summary>
        Task SetUserIdAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Set user property
        /// </summary>
        Task SetUserPropertyAsync(string name, string value, CancellationToken ct = default);

        /// <summary>
        /// Set analytics collection enabled/disabled
        /// </summary>
        Task SetAnalyticsCollectionEnabledAsync(bool enabled, CancellationToken ct = default);

        /// <summary>
        /// Set session timeout duration in milliseconds
        /// </summary>
        Task SetSessionTimeoutDurationAsync(long milliseconds, CancellationToken ct = default);

        /// <summary>
        /// Reset analytics data
        /// </summary>
        Task ResetAnalyticsDataAsync(CancellationToken ct = default);

        /// <summary>
        /// Events
        /// </summary>
        event Action<string> OnEventLogged;
        event Action<string, Exception> OnEventLogError;
    }
}
