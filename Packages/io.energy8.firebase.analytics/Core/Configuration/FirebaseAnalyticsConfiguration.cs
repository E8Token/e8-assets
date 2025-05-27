using System.Collections.Generic;
using UnityEngine;

namespace Energy8.Firebase.Analytics.Configuration
{
    [CreateAssetMenu(menuName = "Firebase/Analytics Configuration")]
    public class FirebaseAnalyticsConfiguration : ScriptableObject
    {
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private long sessionTimeoutDuration = 1800000; // 30 minutes in milliseconds
        [SerializeField] private bool enableAutomaticScreenReporting = true;
        [SerializeField] private List<string> customParameters = new();

        private const string ConfigPath = "Firebase/Analytics/FirebaseAnalyticsConfiguration";

        private static FirebaseAnalyticsConfiguration instance;
        public static FirebaseAnalyticsConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<FirebaseAnalyticsConfiguration>(ConfigPath);
                    
#if UNITY_EDITOR
                    // Автоматически создаем файл настроек если его нет
                    if (instance == null)
                    {
                        instance = CreateDefaultConfiguration();
                    }
#endif
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        private static FirebaseAnalyticsConfiguration CreateDefaultConfiguration()
        {
            var config = CreateInstance<FirebaseAnalyticsConfiguration>();
            config.enableAnalytics = true;
            config.enableDebugLogging = false;
            config.sessionTimeoutDuration = 1800000;
            config.enableAutomaticScreenReporting = true;
            config.customParameters = new List<string>();
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var analyticsPath = "Assets/Resources/Firebase/Analytics";
            
            if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(firebasePath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(analyticsPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Analytics");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Analytics/FirebaseAnalyticsConfiguration.asset";
            UnityEditor.AssetDatabase.CreateAsset(config, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            return config;
        }
#endif

        // Properties
        public static bool EnableAnalytics
        {
            get => Instance.enableAnalytics;
            set => Instance.enableAnalytics = value;
        }

        public static bool EnableDebugLogging
        {
            get => Instance.enableDebugLogging;
            set => Instance.enableDebugLogging = value;
        }

        public static long SessionTimeoutDuration
        {
            get => Instance.sessionTimeoutDuration;
            set => Instance.sessionTimeoutDuration = value;
        }

        public static bool EnableAutomaticScreenReporting
        {
            get => Instance.enableAutomaticScreenReporting;
            set => Instance.enableAutomaticScreenReporting = value;
        }

        public static List<string> CustomParameters => Instance.customParameters;
    }
}
