using System;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.Firebase.Analytics.WebGL
{
    [Serializable]
    public class FirebaseAnalyticsPluginSettings : IPluginSettings
    {
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private bool enableAutomaticScreenReporting = true;
        [SerializeField] private long sessionTimeoutDuration = 1800000; // 30 minutes
        [SerializeField] private string defaultAppName = "[DEFAULT]";

        public bool EnableDebugLogging
        {
            get => enableDebugLogging;
            set => enableDebugLogging = value;
        }

        public bool EnableAutomaticScreenReporting
        {
            get => enableAutomaticScreenReporting;
            set => enableAutomaticScreenReporting = value;
        }

        public long SessionTimeoutDuration
        {
            get => sessionTimeoutDuration;
            set => sessionTimeoutDuration = value;
        }

        public string DefaultAppName
        {
            get => defaultAppName;
            set => defaultAppName = value;
        }        public void LoadSettings()
        {
            // Загружаем настройки из конфигурации Firebase Analytics
            enableDebugLogging = Configuration.FirebaseAnalyticsConfiguration.EnableDebugLogging;
            enableAutomaticScreenReporting = Configuration.FirebaseAnalyticsConfiguration.EnableAutomaticScreenReporting;
            sessionTimeoutDuration = Configuration.FirebaseAnalyticsConfiguration.SessionTimeoutDuration;
        }public void SaveSettings()
        {
            // В реальной реализации здесь можно сохранить настройки
            // или синхронизировать с основной конфигурацией
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public void ResetToDefaults()
        {
            enableDebugLogging = false;
            enableAutomaticScreenReporting = true;
            sessionTimeoutDuration = 1800000; // 30 minutes
            defaultAppName = "[DEFAULT]";
        }
    }
}
