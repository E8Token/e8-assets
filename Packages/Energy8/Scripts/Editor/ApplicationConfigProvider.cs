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

                    EditorGUILayout.Space();

                    // Отображение выпадающего списка для выбора IPType
                    EditorGUILayout.LabelField("Selected IP Type", EditorStyles.boldLabel);
                    ApplicationConfig.SelectedIPType = (IPType)EditorGUILayout.EnumPopup("IP Type", ApplicationConfig.SelectedIPType);

                    // Отображение текущего IP-адреса на основе выбранного IPType
                    EditorGUILayout.LabelField("Selected IP Address", ApplicationConfig.SelectedIP);


                    // Отображение выпадающего списка для выбора IPType
                    EditorGUILayout.LabelField("Selected Auth Type", EditorStyles.boldLabel);
                    ApplicationConfig.SelectedAuthType = (AuthType)EditorGUILayout.EnumPopup("Auth Type", ApplicationConfig.SelectedAuthType);

                    EditorGUILayout.Space();

                    // Отображение текущей версии приложения
                    string currentVersion = PlayerSettings.bundleVersion;
                    EditorGUILayout.LabelField("Application Version", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Current Version: {currentVersion}");

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Global Update"))
                    {
                        currentVersion = IncrementVersion(currentVersion, 0);
                        SaveChanges(currentVersion);
                    }
                    if (GUILayout.Button("Add Minor Update"))
                    {
                        currentVersion = IncrementVersion(currentVersion, 1);
                        SaveChanges(currentVersion);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // Отображение Bundle Version Code (Android)
                    EditorGUILayout.LabelField("Bundle Version Code", PlayerSettings.Android.bundleVersionCode.ToString());

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

        private static string IncrementVersion(string version, int level)
        {
            // Парсинг текущей версии
            var parts = version.Split('.');
            if (parts.Length != 3) parts = new[] { "0", "0", "0" };

            int.TryParse(parts[0], out int major);
            int.TryParse(parts[1], out int minor);
            int.TryParse(parts[2], out int patch);

            if (level == 0)
            {
                major++;
                minor = 0;
                patch = 0;
            }
            else if (level == 1)
            {
                minor++;
                patch = 0;
            }

            // Увеличиваем третью часть версии
            patch++;

            // Формируем новую версию
            return $"{major}.{minor}.{patch}";
        }

        private static void SaveChanges(string newVersion)
        {
            // Устанавливаем новую версию в PlayerSettings
            PlayerSettings.bundleVersion = newVersion;

            // Обновляем Bundle Version Code (заменяем точки на 0)
            int versionCode = int.Parse(newVersion.Replace(".", "0"));
            PlayerSettings.Android.bundleVersionCode = versionCode;

            // Сохраняем изменения
            Debug.Log($"Updated to version {newVersion} (Bundle Version Code: {versionCode})");
        }
    }
}
#endif