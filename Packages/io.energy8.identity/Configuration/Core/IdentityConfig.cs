using Newtonsoft.Json;
using UnityEngine;
using Energy8.EnvironmentConfig.Base;

namespace Energy8.Identity.Configuration.Core
{
    [CreateAssetMenu(fileName = "IdentityConfig", menuName = "Identity/Config")]
    public class IdentityConfig : BaseModuleConfig
    {
        [Header("Server Configuration")]
        [Tooltip("Auth server URL for this environment")]
        [JsonProperty("authServerUrl")]
        public string AuthServerUrl = "https://example.com";

        [Header("Firebase Configuration")]
        [Tooltip("Firebase config for native platforms (Android, iOS)")]
        [JsonProperty("firebaseConfig")]
        public TextAsset FirebaseConfig;
        
        [Tooltip("Firebase config for WebGL platform")]
        [JsonProperty("firebaseWebConfig")]
        public TextAsset FirebaseWebConfig;

        [Header("Telegram Configuration")]
        [Tooltip("Telegram Bot ID for authentication")]
        [JsonProperty("telegramBotId")]
        public long TelegramBotId = 0;

        [Header("Analytics Settings")]
        [Tooltip("Enable analytics system")]
        [JsonProperty("enableAnalytics")]
        public bool EnableAnalytics = true;
        
        [Tooltip("Track user interactions and behavior")]
        [JsonProperty("trackUserActions")]
        public bool TrackUserActions = true;
        
        [Tooltip("Track application errors and exceptions")]
        [JsonProperty("trackErrors")]
        public bool TrackErrors = true;
        
        [Tooltip("Track performance metrics and timings")]
        [JsonProperty("trackPerformance")]
        public bool TrackPerformance = false;

        /// <summary>
        /// Log Identity configuration details at startup
        /// </summary>
        public override void LogConfigInfo()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Debug.Log($"[IdentityConfig] Configuration loaded:\n{json}");
        }
    }
}
