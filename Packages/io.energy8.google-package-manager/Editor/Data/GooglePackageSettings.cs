using System;
using UnityEngine;
using UnityEditor;

namespace Energy8.GooglePackageManager.Data
{
    [CreateAssetMenu(fileName = "GooglePackageSettings", menuName = "Energy8/Google Package Manager Settings")]
    public class GooglePackageSettings : ScriptableObject
    {
        [Header("Auto Update Settings")]
        public bool enableAutoUpdateCheck = true;
        public bool enableStartupUpdateCheck = false;
        public bool enableUpdateNotifications = true;
        public int updateCheckIntervalHours = 24;
        
        [Header("Download Settings")]
        public string downloadCachePath = "Library/GooglePackageCache";
        public bool validatePackagesOnStartup = true;
        
        [Header("Notification Settings")]
        public bool showUpdateNotifications = true;
        public bool showInstallNotifications = true;
        public bool showSuccessNotifications = true;
        
        [Header("Advanced Settings")]
        public bool enableDebugLogging = false;
        public int downloadTimeoutSeconds = 300;
        public int maxRetryAttempts = 3;
        
        private static GooglePackageSettings _instance;
        
        public static GooglePackageSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadOrCreateSettings();
                }
                return _instance;
            }
        }
        
        private static GooglePackageSettings LoadOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GooglePackageSettings>(GetSettingsPath());
            if (settings == null)
            {
                settings = CreateInstance<GooglePackageSettings>();
                AssetDatabase.CreateAsset(settings, GetSettingsPath());
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
        
        private static string GetSettingsPath()
        {
            return "Assets/Settings/GooglePackageSettings.asset";
        }
        
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Energy8/Google Package Manager/Settings")]
        public static void OpenSettings()
        {
            var settings = Instance;
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
