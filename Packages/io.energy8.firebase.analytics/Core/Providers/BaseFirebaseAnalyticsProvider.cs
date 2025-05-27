using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Analytics.Api;
using Energy8.Firebase.Analytics.Models;
using UnityEngine;

namespace Energy8.Firebase.Analytics.Providers
{
    public abstract class BaseFirebaseAnalyticsProvider : IFirebaseAnalyticsApi
    {
        protected readonly List<AnalyticsEvent> eventQueue = new();
        protected bool isAnalyticsEnabled = true;
        protected string currentUserId;

        public event Action<string> OnEventLogged;
        public event Action<string, Exception> OnEventLogError;

        public abstract Task LogEventAsync(string eventName, CancellationToken ct = default);
        public abstract Task LogEventAsync(string eventName, Dictionary<string, object> parameters, CancellationToken ct = default);
        public abstract Task SetUserIdAsync(string userId, CancellationToken ct = default);
        public abstract Task SetUserPropertyAsync(string name, string value, CancellationToken ct = default);
        public abstract Task SetAnalyticsCollectionEnabledAsync(bool enabled, CancellationToken ct = default);
        public abstract Task SetSessionTimeoutDurationAsync(long milliseconds, CancellationToken ct = default);
        public abstract Task ResetAnalyticsDataAsync(CancellationToken ct = default);

        protected virtual void ValidateEventName(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
                
            if (eventName.Length > 40)
                throw new ArgumentException("Event name cannot exceed 40 characters", nameof(eventName));
        }

        protected virtual void ValidateParameters(Dictionary<string, object> parameters)
        {
            if (parameters == null) return;

            if (parameters.Count > 25)
                throw new ArgumentException("Cannot have more than 25 parameters per event");

            foreach (var kvp in parameters)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                    throw new ArgumentException("Parameter name cannot be null or empty");
                    
                if (kvp.Key.Length > 40)
                    throw new ArgumentException($"Parameter name '{kvp.Key}' cannot exceed 40 characters");

                if (kvp.Value is string stringValue && stringValue.Length > 100)
                    throw new ArgumentException($"String parameter '{kvp.Key}' cannot exceed 100 characters");
            }
        }

        protected void InvokeEventLogged(string eventName)
        {
            OnEventLogged?.Invoke(eventName);
        }

        protected void InvokeEventLogError(string eventName, Exception error)
        {
            OnEventLogError?.Invoke(eventName, error);
        }

        protected virtual void LogDebug(string message)
        {
            if (Configuration.FirebaseAnalyticsConfiguration.EnableDebugLogging)
            {
                Debug.Log($"[FirebaseAnalytics] {message}");
            }
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError($"[FirebaseAnalytics] {message}");
        }
    }
}
