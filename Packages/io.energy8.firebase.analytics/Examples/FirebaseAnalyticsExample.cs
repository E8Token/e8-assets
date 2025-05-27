using System.Collections.Generic;
using UnityEngine;
using Energy8.Firebase.Analytics;
using Energy8.Firebase.Analytics.Models;

namespace Energy8.Firebase.Analytics.Examples
{
    /// <summary>
    /// Example script demonstrating Firebase Analytics usage
    /// </summary>
    public class FirebaseAnalyticsExample : MonoBehaviour
    {
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool enableDebugLogging = true;

        private async void Start()
        {
            if (initializeOnStart)
            {
                await InitializeAnalytics();
            }
        }

        /// <summary>
        /// Initialize Firebase Analytics
        /// </summary>
        public async void InitializeAnalytics()
        {
            Debug.Log("[AnalyticsExample] Initializing Firebase Analytics...");
            
            var success = await FirebaseAnalytics.InitializeAsync();
            if (success)
            {
                Debug.Log("[AnalyticsExample] Firebase Analytics initialized successfully!");
                
                // Set up event listeners
                FirebaseAnalytics.OnEventLogged += OnEventLogged;
                FirebaseAnalytics.OnEventLogError += OnEventLogError;
                
                // Log initialization event
                await LogCustomEvent("analytics_initialized");
            }
            else
            {
                Debug.LogError("[AnalyticsExample] Failed to initialize Firebase Analytics");
            }
        }

        /// <summary>
        /// Log a custom event with no parameters
        /// </summary>
        public async void LogCustomEvent(string eventName)
        {
            if (enableDebugLogging)
                Debug.Log($"[AnalyticsExample] Logging event: {eventName}");
                
            await FirebaseAnalytics.LogEventAsync(eventName);
        }

        /// <summary>
        /// Log a select content event
        /// </summary>
        public async void LogSelectContentEvent(string itemId, string itemName, string contentType)
        {
            var parameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.ITEM_ID, itemId },
                { AnalyticsParameterNames.ITEM_NAME, itemName },
                { AnalyticsParameterNames.CONTENT_TYPE, contentType }
            };

            var eventData = new AnalyticsEvent(AnalyticsEventNames.SELECT_CONTENT, parameters);
            await FirebaseAnalytics.LogEventAsync(eventData);
        }

        /// <summary>
        /// Log a purchase event
        /// </summary>
        public async void LogPurchaseEvent(string itemId, string currency, double value)
        {
            var parameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.ITEM_ID, itemId },
                { AnalyticsParameterNames.CURRENCY, currency },
                { AnalyticsParameterNames.VALUE, value }
            };

            var eventData = new AnalyticsEvent(AnalyticsEventNames.PURCHASE, parameters);
            await FirebaseAnalytics.LogEventAsync(eventData);
        }

        /// <summary>
        /// Log a level start event
        /// </summary>
        public async void LogLevelStartEvent(int level)
        {
            var parameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.LEVEL, level }
            };

            var eventData = new AnalyticsEvent(AnalyticsEventNames.LEVEL_START, parameters);
            await FirebaseAnalytics.LogEventAsync(eventData);
        }

        /// <summary>
        /// Log a level end event with score
        /// </summary>
        public async void LogLevelEndEvent(int level, int score)
        {
            var parameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.LEVEL, level },
                { AnalyticsParameterNames.SCORE, score }
            };

            var eventData = new AnalyticsEvent(AnalyticsEventNames.LEVEL_END, parameters);
            await FirebaseAnalytics.LogEventAsync(eventData);
        }

        /// <summary>
        /// Set user ID for analytics
        /// </summary>
        public async void SetUserId(string userId)
        {
            if (enableDebugLogging)
                Debug.Log($"[AnalyticsExample] Setting user ID: {userId}");
                
            await FirebaseAnalytics.SetUserIdAsync(userId);
        }

        /// <summary>
        /// Set user property
        /// </summary>
        public async void SetUserProperty(string propertyName, string propertyValue)
        {
            if (enableDebugLogging)
                Debug.Log($"[AnalyticsExample] Setting user property: {propertyName} = {propertyValue}");
                
            await FirebaseAnalytics.SetUserPropertyAsync(propertyName, propertyValue);
        }

        /// <summary>
        /// Example: Log a complete game session
        /// </summary>
        public async void LogGameSession(int level, int score, float sessionDuration)
        {
            // Log level start
            await LogLevelStartEvent(level);
            
            // Log level end with score
            await LogLevelEndEvent(level, score);
            
            // Log session duration as custom event
            var sessionParameters = new Dictionary<string, object>
            {
                { AnalyticsParameterNames.LEVEL, level },
                { AnalyticsParameterNames.SCORE, score },
                { "session_duration", sessionDuration }
            };

            var sessionEvent = new AnalyticsEvent("game_session_complete", sessionParameters);
            await FirebaseAnalytics.LogEventAsync(sessionEvent);
        }

        // Event handlers
        private void OnEventLogged(string eventName)
        {
            if (enableDebugLogging)
                Debug.Log($"[AnalyticsExample] Event logged successfully: {eventName}");
        }

        private void OnEventLogError(string eventName, System.Exception error)
        {
            Debug.LogError($"[AnalyticsExample] Failed to log event '{eventName}': {error.Message}");
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (FirebaseAnalytics.IsInitialized)
            {
                FirebaseAnalytics.OnEventLogged -= OnEventLogged;
                FirebaseAnalytics.OnEventLogError -= OnEventLogError;
            }
        }
    }
}
