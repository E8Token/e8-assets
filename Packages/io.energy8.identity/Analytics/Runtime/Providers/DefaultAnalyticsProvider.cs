using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Analytics.Core.Providers;
using UnityEngine;

namespace Energy8.Identity.Analytics.Runtime.Providers
{
    /// <summary>
    /// Default analytics provider that logs to console with [Analytics] prefix
    /// Used for testing, development, or when no specific analytics service is configured
    /// </summary>
    public class DefaultAnalyticsProvider : IAnalyticsProvider
    {
        public bool IsInitialized { get; private set; }
        public event Action<string> OnError;

        public UniTask Initialize(CancellationToken ct)
        {
            IsInitialized = true;
            Debug.Log("[Analytics] Default Analytics Provider initialized (console logging only)");
            return UniTask.CompletedTask;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Analytics] Cannot log event: Provider not initialized");
                return;
            }

            var paramStr = "";
            if (parameters != null && parameters.Count > 0)
            {
                var paramPairs = new List<string>();
                foreach (var param in parameters)
                {
                    paramPairs.Add($"{param.Key}={param.Value ?? "null"}");
                }
                paramStr = $" | Parameters: {string.Join(", ", paramPairs)}";
            }

            Debug.Log($"[Analytics] Event: {eventName}{paramStr}");
        }

        public void SetUserId(string userId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Analytics] Cannot set user ID: Provider not initialized");
                return;
            }

            Debug.Log($"[Analytics] User ID set: {userId ?? "null"}");
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Analytics] Cannot set user properties: Provider not initialized");
                return;
            }

            if (properties == null || properties.Count == 0)
            {
                Debug.Log("[Analytics] User properties cleared");
                return;
            }

            var propPairs = new List<string>();
            foreach (var prop in properties)
            {
                propPairs.Add($"{prop.Key}={prop.Value ?? "null"}");
            }

            Debug.Log($"[Analytics] User properties set: {string.Join(", ", propPairs)}");
        }

        public void ResetAnalyticsData()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Analytics] Cannot reset: Provider not initialized");
                return;
            }

            Debug.Log("[Analytics] Analytics data reset requested");
        }
    }
}