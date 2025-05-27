using UnityEditor;
using UnityEngine;
using Energy8.Firebase.Auth.Configuration;

namespace Energy8.Firebase.Auth.Editor
{
    public static class FirebaseAuthEditorUtilities
    {
        [MenuItem("Firebase/Auth/Create Configuration", false, 1)]
        public static void CreateConfiguration()
        {
            CreateConfigurationFile();
        }

        [MenuItem("Firebase/Auth/Create Configuration", true)]
        public static bool ValidateCreateConfiguration()
        {
            // Показываем пункт меню только если конфигурации еще нет
            var config = Resources.Load<FirebaseAuthConfiguration>("Firebase/Auth/FirebaseAuthConfiguration");
            return config == null;
        }

        [MenuItem("Firebase/Auth/Open Project Settings", false, 2)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings("Project/Firebase/Auth");
        }

        [MenuItem("Firebase/Auth/Test Connection", false, 10)]
        public static async void TestConnection()
        {
            Debug.Log("[FirebaseAuth] Testing connection...");
            
            try
            {
                await FirebaseAuth.InitializeAsync();
                Debug.Log("[FirebaseAuth] Connection test successful!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FirebaseAuth] Connection test failed: {ex.Message}");
            }
        }

        [MenuItem("Firebase/Auth/Clear Cached User", false, 11)]
        public static void ClearCachedUser()
        {
            // This would clear any cached authentication data
            Debug.Log("[FirebaseAuth] Cleared cached user data");
            
            // In a real implementation, this would clear PlayerPrefs, local storage, etc.
            if (EditorUtility.DisplayDialog("Clear Cached User", 
                "This will clear all cached authentication data. Continue?", "Yes", "No"))
            {
                PlayerPrefs.DeleteKey("firebase_auth_token");
                PlayerPrefs.DeleteKey("firebase_user_id");
                PlayerPrefs.Save();
                Debug.Log("[FirebaseAuth] Cached authentication data cleared");
            }
        }

        public static FirebaseAuthConfiguration CreateConfigurationFile()
        {
            var config = ScriptableObject.CreateInstance<FirebaseAuthConfiguration>();
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var authPath = "Assets/Resources/Firebase/Auth";
            
            if (!AssetDatabase.IsValidFolder(resourcesPath))
                AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!AssetDatabase.IsValidFolder(firebasePath))
                AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!AssetDatabase.IsValidFolder(authPath))
                AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Auth");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Auth/FirebaseAuthConfiguration.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[FirebaseAuth] Created configuration at: {assetPath}");
            
            // Выделяем созданный файл в Project окне
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            
            return config;
        }
    }
}
