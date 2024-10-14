#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Energy8
{
    public static class ApplicationConfigProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateApplicationConfigProvider()
        {
            var provider = new SettingsProvider("Project/ApplicationConfig", SettingsScope.Project)
            {
                label = "Application Config",

                guiHandler = (searchContext) =>
                {
                    // Проверяем, загружена ли конфигурация
                    if (ApplicationConfig.Instance == null)
                    {
                        EditorGUILayout.HelpBox("ApplicationConfig asset not found. Please create it in Resources.", MessageType.Warning);
                        return;
                    }

                    // Отображение настроек Google Web API
                    EditorGUILayout.LabelField("Google Web API", EditorStyles.boldLabel);
                    ApplicationConfig.GoogleWebAPI = EditorGUILayout.TextField("Google Web API", ApplicationConfig.GoogleWebAPI);

                    EditorGUILayout.Space();

                    // Отображение выпадающего списка для выбора IPType
                    EditorGUILayout.LabelField("Selected IP Type", EditorStyles.boldLabel);
                    ApplicationConfig.SelectedIPType = (IPType)EditorGUILayout.EnumPopup("IP Type", ApplicationConfig.SelectedIPType);

                    // Отображение текущего IP-адреса на основе выбранного IPType
                    EditorGUILayout.LabelField("Selected IP Address", ApplicationConfig.SelectedIP);

                    EditorGUILayout.Space();

                    if (GUI.changed)
                    {
                        // Сохраняем изменения в ScriptableObject
                        EditorUtility.SetDirty(ApplicationConfig.Instance);
                        AssetDatabase.SaveAssets();
                    }
                },

                keywords = new System.Collections.Generic.HashSet<string>(new[] { "IP", "API", "Application", "Config" })
            };

            return provider;
        }
    }
}
#endif