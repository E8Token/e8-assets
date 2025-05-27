using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Analytics.Providers;
using UnityEngine;

namespace Energy8.Firebase.Analytics.Providers
{
    public class WebFirebaseAnalyticsProvider : BaseFirebaseAnalyticsProvider
    {
        public override async Task LogEventAsync(string eventName, CancellationToken ct = default)
        {
            ValidateEventName(eventName);
            
            if (!isAnalyticsEnabled)
            {
                LogDebug($"Analytics disabled, skipping event: {eventName}");
                return;
            }

            try
            {
                LogDebug($"Logging event: {eventName}");
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.LogEvent(eventName);
                
                await Task.CompletedTask;
                InvokeEventLogged(eventName);
            }
            catch (Exception ex)
            {
                LogError($"Failed to log event '{eventName}': {ex.Message}");
                InvokeEventLogError(eventName, ex);
                throw;
            }
        }

        public override async Task LogEventAsync(string eventName, Dictionary<string, object> parameters, CancellationToken ct = default)
        {
            ValidateEventName(eventName);
            ValidateParameters(parameters);
            
            if (!isAnalyticsEnabled)
            {
                LogDebug($"Analytics disabled, skipping event: {eventName}");
                return;
            }

            try
            {
                LogDebug($"Logging event: {eventName} with {parameters?.Count ?? 0} parameters");
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // var parametersJson = ConvertParametersToJson(parameters);
                // FirebaseAnalyticsPlugin.LogEventWithParameters(eventName, parametersJson);
                
                await Task.CompletedTask;
                InvokeEventLogged(eventName);
            }
            catch (Exception ex)
            {
                LogError($"Failed to log event '{eventName}': {ex.Message}");
                InvokeEventLogError(eventName, ex);
                throw;
            }
        }

        public override async Task SetUserIdAsync(string userId, CancellationToken ct = default)
        {
            try
            {
                LogDebug($"Setting user ID: {userId}");
                currentUserId = userId;
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.SetUserId(userId);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to set user ID: {ex.Message}");
                throw;
            }
        }

        public override async Task SetUserPropertyAsync(string name, string value, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Property name cannot be null or empty", nameof(name));

            try
            {
                LogDebug($"Setting user property: {name} = {value}");
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.SetUserProperty(name, value);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to set user property '{name}': {ex.Message}");
                throw;
            }
        }

        public override async Task SetAnalyticsCollectionEnabledAsync(bool enabled, CancellationToken ct = default)
        {
            try
            {
                LogDebug($"Setting analytics collection enabled: {enabled}");
                isAnalyticsEnabled = enabled;
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.SetAnalyticsCollectionEnabled(enabled);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to set analytics collection enabled: {ex.Message}");
                throw;
            }
        }

        public override async Task SetSessionTimeoutDurationAsync(long milliseconds, CancellationToken ct = default)
        {
            if (milliseconds < 0)
                throw new ArgumentException("Session timeout duration cannot be negative", nameof(milliseconds));

            try
            {
                LogDebug($"Setting session timeout duration: {milliseconds}ms");
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.SetSessionTimeoutDuration(milliseconds);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to set session timeout duration: {ex.Message}");
                throw;
            }
        }

        public override async Task ResetAnalyticsDataAsync(CancellationToken ct = default)
        {
            try
            {
                LogDebug("Resetting analytics data");
                
                // В реальной реализации здесь будет вызов JavaScript функции
                // FirebaseAnalyticsPlugin.ResetAnalyticsData();
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogError($"Failed to reset analytics data: {ex.Message}");
                throw;
            }
        }

        private static string ConvertParametersToJson(Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "{}";

            var json = "{";
            var first = true;
            
            foreach (var kvp in parameters)
            {
                if (!first) json += ",";
                first = false;
                
                json += $"\"{kvp.Key}\":";
                
                switch (kvp.Value)
                {
                    case string stringValue:
                        json += $"\"{stringValue.Replace("\"", "\\\"")}\"";
                        break;
                    case bool boolValue:
                        json += boolValue.ToString().ToLower();
                        break;
                    case null:
                        json += "null";
                        break;
                    default:
                        json += kvp.Value.ToString();
                        break;
                }
            }
            
            json += "}";
            return json;
        }
    }
}
