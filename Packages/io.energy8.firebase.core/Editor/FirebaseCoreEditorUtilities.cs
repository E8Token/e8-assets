using UnityEditor;
using UnityEngine;
using Energy8.Firebase.Core.Configuration;

namespace Energy8.Firebase.Core.Editor
{
    public static class FirebaseCoreEditorUtilities
    {
        [MenuItem("Firebase/Core/Create Configuration", false, 1)]
        public static void CreateConfiguration()
        {
            CreateConfigurationFile();
        }

        [MenuItem("Firebase/Core/Create Configuration", true)]
        public static bool ValidateCreateConfiguration()
        {
            // Показываем пункт меню только если конфигурации еще нет
            var config = Resources.Load<FirebaseCoreConfiguration>("Firebase/Core/FirebaseCoreConfiguration");
            return config == null;
        }

        [MenuItem("Firebase/Core/Open Project Settings", false, 2)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings("Project/Firebase/Core");
        }

        public static FirebaseCoreConfiguration CreateConfigurationFile()
        {
            var config = ScriptableObject.CreateInstance<FirebaseCoreConfiguration>();
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var corePath = "Assets/Resources/Firebase/Core";
            
            if (!AssetDatabase.IsValidFolder(resourcesPath))
                AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!AssetDatabase.IsValidFolder(firebasePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!AssetDatabase.IsValidFolder(corePath))
                AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Core");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Core/FirebaseCoreConfiguration.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[FirebaseCore] Created configuration at: {assetPath}");
            
            // Автоматически выбираем созданный файл в Inspector
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            
            return config;
        }
    }
}
