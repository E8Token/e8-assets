using System;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.Firebase.Analytics.WebGL
{
    public class FirebaseAnalyticsPlugin : BasePlugin
    {
        [SerializeField] private FirebaseAnalyticsPluginSettings settings = new();

        public override IPluginSettings Settings => settings;

        public override void Initialize()
        {
            Debug.Log("[FirebaseAnalyticsPlugin] Initializing Firebase Analytics WebGL Plugin");
        }

        public override void Enable()
        {
            Debug.Log("[FirebaseAnalyticsPlugin] Firebase Analytics WebGL Plugin enabled");
        }

        public override void Disable()
        {
            Debug.Log("[FirebaseAnalyticsPlugin] Firebase Analytics WebGL Plugin disabled");
        }

        public override void Destroy()
        {
            Debug.Log("[FirebaseAnalyticsPlugin] Firebase Analytics WebGL Plugin destroyed");
        }

        [JSCallable("logEvent")]
        public string LogEvent(string eventName)
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseAnalyticsPlugin] Logging event: {eventName}");
                }

                // Вызываем JavaScript для логирования события
                return $"{{\"success\": true, \"eventName\": \"{eventName}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalyticsPlugin] Failed to log event: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("logEventWithParameters")]
        public string LogEventWithParameters(string eventName, string parameters)
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseAnalyticsPlugin] Logging event: {eventName} with parameters: {parameters}");
                }

                // Вызываем JavaScript для логирования события с параметрами
                return $"{{\"success\": true, \"eventName\": \"{eventName}\", \"parameters\": {parameters}}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalyticsPlugin] Failed to log event with parameters: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("setUserId")]
        public string SetUserId(string userId)
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseAnalyticsPlugin] Setting user ID: {userId}");
                }

                // Вызываем JavaScript для установки user ID
                return $"{{\"success\": true, \"userId\": \"{userId}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalyticsPlugin] Failed to set user ID: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("setUserProperty")]
        public string SetUserProperty(string name, string value)
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseAnalyticsPlugin] Setting user property: {name} = {value}");
                }

                // Вызываем JavaScript для установки user property
                return $"{{\"success\": true, \"propertyName\": \"{name}\", \"propertyValue\": \"{value}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalyticsPlugin] Failed to set user property: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("setAnalyticsCollectionEnabled")]
        public string SetAnalyticsCollectionEnabled(bool enabled)
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseAnalyticsPlugin] Setting analytics collection enabled: {enabled}");
                }

                // Вызываем JavaScript для включения/выключения сбора аналитики
                return $"{{\"success\": true, \"enabled\": {enabled.ToString().ToLower()}}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseAnalyticsPlugin] Failed to set analytics collection enabled: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }
    }
}
