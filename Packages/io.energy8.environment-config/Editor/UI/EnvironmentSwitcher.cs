#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Energy8.EnvironmentConfig.Editor.Settings;
using Energy8.EnvironmentConfig.Editor.Utilities;

namespace Energy8.EnvironmentConfig.Editor.UI
{
    /// <summary>
    /// Simple Environment Switcher window
    /// Shows current environment and allows switching
    /// </summary>
    public class EnvironmentSwitcher : EditorWindow
    {
        private EnvironmentSettings settings;
        private int selectedEnvironmentIndex = 0;
        private string currentEnvironment;

        [MenuItem("E8 Tools/E8 Config/Environment Switcher")]
        public static void Open() => GetWindow<EnvironmentSwitcher>("E8 Environment");

        private void OnEnable()
        {
            settings = EnvironmentSettings.GetOrCreateSettings();
            UpdateCurrentEnvironment();
        }

        private void OnGUI()
        {
            GUILayout.Label("Environment Switcher", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (settings == null || settings.environments == null || settings.environments.Length == 0)
            {
                EditorGUILayout.HelpBox("Environment Settings not found!", MessageType.Error);
                GUILayout.Space(10);
                if (GUILayout.Button("Create Environment Settings", GUILayout.Height(30)))
                {
                    settings = EnvironmentSettings.CreateSettings();
                    UpdateCurrentEnvironment();
                }
                return;
            }

            // Current environment
            DrawCurrentEnvironment();
            GUILayout.Space(20);

            // Environment selector
            DrawEnvironmentSelector();
        }

        private void DrawCurrentEnvironment()
        {
            EditorGUILayout.LabelField("Current Environment", EditorStyles.boldLabel);
            GUILayout.Space(5);

            using (new EditorGUI.DisabledScope(true))
            {
                var bgColor = GUI.backgroundColor;
                var envData = GetEnvironmentData(currentEnvironment);
                if (envData != null)
                {
                    GUI.backgroundColor = envData.color;
                }
                
                EditorGUILayout.TextField(currentEnvironment);
                GUI.backgroundColor = bgColor;
            }
        }

        private void DrawEnvironmentSelector()
        {
            EditorGUILayout.LabelField("Switch Environment", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string[] environmentNames = new string[settings.environments.Length];
            for (int i = 0; i < settings.environments.Length; i++)
            {
                environmentNames[i] = settings.environments[i].name;
            }

            EditorGUI.BeginChangeCheck();
            selectedEnvironmentIndex = EditorGUILayout.Popup("Environment", selectedEnvironmentIndex, environmentNames);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedEnvironmentIndex >= 0 && selectedEnvironmentIndex < settings.environments.Length)
                {
                    ApplyEnvironment(settings.environments[selectedEnvironmentIndex].name);
                }
            }
        }

        private void UpdateCurrentEnvironment()
        {
            currentEnvironment = EnvironmentConfigUtility.GetCurrentEnvironment();
            
            for (int i = 0; i < settings.environments.Length; i++)
            {
                if (settings.environments[i].name == currentEnvironment)
                {
                    selectedEnvironmentIndex = i;
                    break;
                }
            }
        }

        private void ApplyEnvironment(string environmentName)
        {
            EnvironmentConfigUtility.SetCurrentEnvironment(environmentName);
            currentEnvironment = environmentName;
            
            Core.EnvironmentManager.Reload();
            
            EditorUtility.DisplayDialog("Success", $"Environment switched to: {environmentName}\n\nChanges will take effect on next play.", "OK");
            
            Debug.Log($"[EnvironmentSwitcher] Environment applied: {environmentName}");
        }

        private EnvironmentData GetEnvironmentData(string name)
        {
            if (settings == null || settings.environments == null)
                return null;

            foreach (var env in settings.environments)
            {
                if (env.name == name)
                    return env;
            }
            return null;
        }
    }
}
#endif
