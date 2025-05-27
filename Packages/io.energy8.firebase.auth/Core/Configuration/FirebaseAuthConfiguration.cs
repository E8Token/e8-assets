using System.Collections.Generic;
using UnityEngine;
using Energy8.Firebase.Core.Configuration.Models;

namespace Energy8.Firebase.Auth.Configuration
{
    [CreateAssetMenu(menuName = "Firebase/Auth Configuration")]
    public class FirebaseAuthConfiguration : ScriptableObject
    {
        [SerializeField] private bool enableAutoSignIn = false;
        [SerializeField] private bool persistUser = true;
        [SerializeField] private bool useEmulator = false;
        [SerializeField] private string emulatorHost = "localhost";
        [SerializeField] private int emulatorPort = 9099;
        [SerializeField] private List<string> enabledProviders = new List<string> { "email", "anonymous" };

        private const string ConfigPath = "Firebase/Auth/FirebaseAuthConfiguration";

        private static FirebaseAuthConfiguration instance;
        public static FirebaseAuthConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<FirebaseAuthConfiguration>(ConfigPath);
                    
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
        private static FirebaseAuthConfiguration CreateDefaultConfiguration()
        {
            var config = CreateInstance<FirebaseAuthConfiguration>();
            config.enableAutoSignIn = false;
            config.persistUser = true;
            config.useEmulator = false;
            config.emulatorHost = "localhost";
            config.emulatorPort = 9099;
            config.enabledProviders = new List<string> { "email", "anonymous" };
            
            // Создаем папки если их нет
            var resourcesPath = "Assets/Resources";
            var firebasePath = "Assets/Resources/Firebase";
            var authPath = "Assets/Resources/Firebase/Auth";
            
            if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(firebasePath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "Firebase");
                
            if (!UnityEditor.AssetDatabase.IsValidFolder(authPath))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources/Firebase", "Auth");
            
            // Сохраняем файл конфигурации
            var assetPath = "Assets/Resources/Firebase/Auth/FirebaseAuthConfiguration.asset";
            UnityEditor.AssetDatabase.CreateAsset(config, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            return config;
        }
#endif

        // Properties
        public static bool EnableAutoSignIn
        {
            get => Instance.enableAutoSignIn;
            set => Instance.enableAutoSignIn = value;
        }

        public static bool PersistUser
        {
            get => Instance.persistUser;
            set => Instance.persistUser = value;
        }

        public static bool UseEmulator
        {
            get => Instance.useEmulator;
            set => Instance.useEmulator = value;
        }

        public static string EmulatorHost
        {
            get => Instance.emulatorHost;
            set => Instance.emulatorHost = value;
        }

        public static int EmulatorPort
        {
            get => Instance.emulatorPort;
            set => Instance.emulatorPort = value;
        }

        public static List<string> EnabledProviders => Instance.enabledProviders;

        // Get emulator URL
        public static string GetEmulatorUrl()
        {
            return $"http://{EmulatorHost}:{EmulatorPort}";
        }

        // Check if provider is enabled
        public static bool IsProviderEnabled(string providerId)
        {
            return EnabledProviders.Contains(providerId);
        }
    }
}
