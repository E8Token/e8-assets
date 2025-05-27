using UnityEditor;
using UnityEngine;
using Energy8.Firebase.Analytics.Configuration;

namespace Energy8.Firebase.Analytics.Editor
{
    public static class FirebaseAnalyticsEditorUtilities
    {
        [MenuItem("Firebase/Analytics/Create Configuration", false, 1)]
        public static void CreateConfiguration()
        {
            CreateConfigurationFile();
        }

        [MenuItem("Firebase/Analytics/Create Configuration", true)]
        public static bool ValidateCreateConfiguration()
        {
            // Показываем пункт меню только если конфигурации еще нет
            var config = Resources.Load<FirebaseAnalyticsConfiguration>("Firebase/Analytics/FirebaseAnalyticsConfiguration");
            return config == null;
        }

        [MenuItem("Firebase/Analytics/Open Project Settings", false, 2)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings("Project/Firebase/Analytics");
        }

        public static FirebaseAnalyticsConfiguration CreateConfigurationFile()
        {
            var config = ScriptableObject.CreateInstance<FirebaseAnalyticsConfiguration>();
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var analyticsPath = "Assets/Resources/Firebase/Analytics";
            
            if (!AssetDatabase.IsValidFolder(resourcesPath))
                AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!AssetDatabase.IsValidFolder(firebasePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!AssetDatabase.IsValidFolder(analyticsPath))
                AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Analytics");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Analytics/FirebaseAnalyticsConfiguration.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Firebase Analytics Configuration created at: {assetPath}");
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            
            return config;
        }
    }
}
