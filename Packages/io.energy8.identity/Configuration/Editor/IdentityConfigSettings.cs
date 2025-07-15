#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Energy8.Identity.Configuration.Core.Editor
{
    public static class IdentityConfigSettings
    {
        private const string ConfigPath = "/Identity/Configuration";
        private const string ConfigName = "IdentityConfiguration.asset";

        [SettingsProvider]
        public static SettingsProvider CreateIdentityConfigProvider()
        {
            var provider = new SettingsProvider("Project/Identity Configuration", SettingsScope.Project)
            {
                label = "Identity Configuration",
                guiHandler = (searchContext) =>
                {
                    EnsureConfigExists();
                    DrawConfigurationGUI();
                },
                keywords = new HashSet<string>(new[] { "Identity", "IP", "Auth", "Analytics", "Configuration" })
            };

            return provider;
        }

        private static void EnsureConfigExists()
        {
            if (IdentityConfiguration.Instance == null)
            {
                string[] folders = ("Assets/Resources" + ConfigPath).Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string folderName = folders[i];
                    string newPath = $"{currentPath}/{folderName}";

                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folderName);
                    }

                    currentPath = newPath;
                }

                var config = ScriptableObject.CreateInstance<IdentityConfiguration>();
                AssetDatabase.CreateAsset(config, $"Assets/Resources{ConfigPath}/{ConfigName}");
                AssetDatabase.SaveAssets();
            }
        }

        private static void DrawConfigurationGUI()
        {
            var config = IdentityConfiguration.Instance;
            if (config == null)
                return;

            SerializedObject serializedConfig = new(config);

            // Configuration Header
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Identity Configuration", EditorStyles.largeLabel);
            
            // Validation Status
            if (config.IsValid)
            {
                GUILayout.Label("✅ Valid", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label("⚠️ Invalid", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            // Show validation errors if any
            if (!config.IsValid)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox($"Configuration Errors:\n• {string.Join("\n• ", config.ValidationErrors)}", MessageType.Error);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("IP Configuration", EditorStyles.boldLabel);
            DrawIPConfigs(serializedConfig);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auth Configuration", EditorStyles.boldLabel);
            DrawAuthConfigs(serializedConfig);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Analytics Configuration", EditorStyles.boldLabel);
            DrawAnalyticsConfigs(serializedConfig);

            if (serializedConfig.hasModifiedProperties)
            {
                serializedConfig.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }

        private static void DrawIPConfigs(SerializedObject serializedObject)
        {
            var ipConfigsProp = serializedObject.FindProperty("ipConfigs");
            EditorGUILayout.PropertyField(ipConfigsProp, true);

            EditorGUILayout.Space();
            var selectedIPTypeProp = serializedObject.FindProperty("selectedIPType");
            EditorGUILayout.PropertyField(selectedIPTypeProp);
            EditorGUILayout.LabelField("Selected IP Address", IdentityConfiguration.SelectedIP);
        }

        private static void DrawAuthConfigs(SerializedObject serializedObject)
        {
            var authConfigsProp = serializedObject.FindProperty("firebaseAuthConfigs");
            EditorGUILayout.PropertyField(authConfigsProp, true);

            var webAuthConfigsProp = serializedObject.FindProperty("firebaseWebAuthConfigs");
            EditorGUILayout.PropertyField(webAuthConfigsProp, true);

            EditorGUILayout.Space();
            var selectedAuthTypeProp = serializedObject.FindProperty("selectedAuthType");
            EditorGUILayout.PropertyField(selectedAuthTypeProp);
        }

        private static void DrawAnalyticsConfigs(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Main Analytics Toggle
            var enableAnalyticsProp = serializedObject.FindProperty("enableAnalytics");
            EditorGUILayout.PropertyField(enableAnalyticsProp, new GUIContent("Enable Analytics", "Enable or disable analytics system"));
            
            // Show current status
            if (IdentityConfiguration.EnableAnalytics)
            {
                EditorGUILayout.HelpBox("✅ Analytics is ENABLED", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Analytics is DISABLED", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Debug and Logging Settings
            EditorGUILayout.LabelField("Debug & Logging", EditorStyles.boldLabel);
            
            var enableDebugLoggingProp = serializedObject.FindProperty("enableDebugLogging");
            EditorGUILayout.PropertyField(enableDebugLoggingProp, new GUIContent("Enable Debug Logging", "Show analytics debug messages in console"));

            EditorGUILayout.Space();

            // Tracking Settings
            EditorGUILayout.LabelField("Tracking Options", EditorStyles.boldLabel);
            
            var trackUserActionsProp = serializedObject.FindProperty("trackUserActions");
            EditorGUILayout.PropertyField(trackUserActionsProp, new GUIContent("Track User Actions", "Track user interactions and behavior"));
            
            var trackErrorsProp = serializedObject.FindProperty("trackErrors");
            EditorGUILayout.PropertyField(trackErrorsProp, new GUIContent("Track Errors", "Track application errors and exceptions"));
            
            var trackPerformanceProp = serializedObject.FindProperty("trackPerformance");
            EditorGUILayout.PropertyField(trackPerformanceProp, new GUIContent("Track Performance", "Track performance metrics and timings"));

            EditorGUILayout.Space();

            // Analytics Summary
            EditorGUILayout.LabelField("Analytics Summary", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Debug Logging: {(IdentityConfiguration.EnableDebugLogging ? "ON" : "OFF")}");
            EditorGUILayout.LabelField($"User Actions: {(IdentityConfiguration.TrackUserActions ? "ON" : "OFF")}");
            EditorGUILayout.LabelField($"Error Tracking: {(IdentityConfiguration.TrackErrors ? "ON" : "OFF")}");
            EditorGUILayout.LabelField($"Performance: {(IdentityConfiguration.TrackPerformance ? "ON" : "OFF")}");
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Enable All Tracking"))
            {
                trackUserActionsProp.boolValue = true;
                trackErrorsProp.boolValue = true;
                trackPerformanceProp.boolValue = true;
                enableDebugLoggingProp.boolValue = true;
            }
            
            if (GUILayout.Button("Disable All Tracking"))
            {
                trackUserActionsProp.boolValue = false;
                trackErrorsProp.boolValue = false;
                trackPerformanceProp.boolValue = false;
                enableDebugLoggingProp.boolValue = false;
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
#endif