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
                keywords = new HashSet<string>(new[] { "Identity", "IP", "Auth", "Configuration" })
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("IP Configuration", EditorStyles.boldLabel);
            DrawIPConfigs(serializedConfig);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auth Configuration", EditorStyles.boldLabel);
            DrawAuthConfigs(serializedConfig);

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
    }
}
#endif