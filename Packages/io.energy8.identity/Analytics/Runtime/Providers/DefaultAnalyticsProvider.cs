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
            return UniTask.CompletedTask;
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                return;
            }
        }

        public void SetUserId(string userId)
        {
            if (!IsInitialized)
            {
                return;
            }
        }

        public void SetUserProperties(Dictionary<string, object> properties)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (properties == null || properties.Count == 0)
            {
                return;
            }
        }

        public void ResetAnalyticsData()
        {
            if (!IsInitialized)
            {
                return;
            }
        }
    }
}
