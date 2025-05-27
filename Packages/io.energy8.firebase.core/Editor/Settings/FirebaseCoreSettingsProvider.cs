using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Energy8.Firebase.Core.Configuration;
using Energy8.Firebase.Core.Configuration.Models;
using Energy8.Firebase.Core.Editor;

namespace Energy8.Firebase.Core.Editor.Settings
{
    public class FirebaseCoreSettingsProvider : SettingsProvider
    {
        private SerializedObject serializedObject;
        private FirebaseCoreConfiguration configuration;

        public FirebaseCoreSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateFirebaseCoreSettingsProvider()
        {
            var provider = new FirebaseCoreSettingsProvider("Project/Firebase/Core", SettingsScope.Project,
                GetSearchKeywordsFromGUIContentProperties<FirebaseCoreSettingsProvider>());
            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            configuration = FirebaseCoreConfiguration.Instance;
            if (configuration != null)
            {
                serializedObject = new SerializedObject(configuration);
            }
        }        public override void OnGUI(string searchContext)
        {
            if (configuration == null)
            {
                EditorGUILayout.HelpBox("Firebase Core Configuration not found.", MessageType.Warning);
                if (GUILayout.Button("Create Configuration"))
                {
                    configuration = CreateConfigurationFile();
                    if (configuration != null)
                    {
                        serializedObject = new SerializedObject(configuration);
                    }
                }
                return;
            }

            serializedObject.Update();

            EditorGUILayout.LabelField("Firebase Core Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // General Settings
            DrawGeneralSettings();
            EditorGUILayout.Space();

            // Environment Configurations
            DrawEnvironmentConfigurations();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneralSettings()
        {
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            
            FirebaseCoreConfiguration.AutoInitialize = EditorGUILayout.Toggle("Auto Initialize", FirebaseCoreConfiguration.AutoInitialize);
            FirebaseCoreConfiguration.DefaultEnvironment = (FirebaseEnvironment)EditorGUILayout.EnumPopup("Default Environment", FirebaseCoreConfiguration.DefaultEnvironment);
        }

        private void DrawEnvironmentConfigurations()
        {
            EditorGUILayout.LabelField("Environment Configurations", EditorStyles.boldLabel);
            
            // SDK Configurations
            EditorGUILayout.LabelField("SDK Platform", EditorStyles.miniBoldLabel);
            DrawPlatformConfigurations(FirebasePlatform.SDK);
            
            EditorGUILayout.Space();
            
            // Web Configurations
            EditorGUILayout.LabelField("Web Platform", EditorStyles.miniBoldLabel);
            DrawPlatformConfigurations(FirebasePlatform.Web);
        }

        private void DrawPlatformConfigurations(FirebasePlatform platform)
        {
            var environments = Enum.GetValues(typeof(FirebaseEnvironment)).Cast<FirebaseEnvironment>();
            
            foreach (var environment in environments)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Environment label
                EditorGUILayout.LabelField(environment.ToString(), GUILayout.Width(80));
                
                // Get current config
                var currentConfig = FirebaseCoreConfiguration.GetEnvironmentConfig(environment, platform);
                var isConfigured = currentConfig != null && currentConfig.Config != null;
                var isEnabled = currentConfig?.IsEnabled ?? false;
                
                // Enabled toggle
                var newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));
                if (newEnabled != isEnabled)
                {
                    FirebaseCoreConfiguration.SetEnvironmentEnabled(environment, platform, newEnabled);
                    EditorUtility.SetDirty(configuration);
                }
                
                // Configuration file field
                var newConfig = (TextAsset)EditorGUILayout.ObjectField(
                    currentConfig?.Config, 
                    typeof(TextAsset), 
                    false);
                
                if (newConfig != currentConfig?.Config)
                {
                    FirebaseCoreConfiguration.SetConfigForEnvironment(environment, platform, newConfig);
                    EditorUtility.SetDirty(configuration);
                }
                
                // Status indicator
                if (isConfigured && isEnabled)
                {
                    EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                }
                else if (isConfigured && !isEnabled)
                {
                    EditorGUILayout.LabelField("○", GUILayout.Width(20));
                }
                else
                {
                    EditorGUILayout.LabelField("×", GUILayout.Width(20));
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Summary for this platform
            var configuredCount = environments.Count(env => 
                FirebaseCoreConfiguration.IsEnvironmentConfigured(env, platform));
            
            EditorGUILayout.LabelField($"Configured: {configuredCount}/{environments.Count()}", EditorStyles.miniLabel);
        }        public override void OnDeactivate()
        {
            if (serializedObject != null)
            {
                serializedObject.Dispose();
                serializedObject = null;
            }
        }

        private static FirebaseCoreConfiguration CreateConfigurationFile()
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