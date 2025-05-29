using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using Energy8.BuildDeploySystem;

namespace Energy8.BuildDeploySystem.Editor.GUI
{
    [InitializeOnLoad]
    public static class PlayerSettingsGUI
    {
        private static BuildConfigurationManager configManager;
        private static Vector2 scrollPosition;
        private static bool showBuildSettings = true;
        private static bool showDeploySettings = true;
        private static bool showVersionSettings = true;
        static PlayerSettingsGUI()
        {
            // Подписываемся на отрисовку Player Settings
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
            LoadConfigurationManager();
        }

        private static void LoadConfigurationManager()
        {
            // Поиск или создание конфигурационного менеджера
            string[] guids = AssetDatabase.FindAssets("t:BuildConfigurationManager");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                configManager = AssetDatabase.LoadAssetAtPath<BuildConfigurationManager>(path);
            }
            else
            {
                CreateDefaultConfigurationManager();
            }
        }

        private static void CreateDefaultConfigurationManager()
        {
            configManager = ScriptableObject.CreateInstance<BuildConfigurationManager>();

            // Создаем папку если её нет
            string folderPath = "Assets/BuildConfigurations";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "BuildConfigurations");
            }

            string assetPath = $"{folderPath}/BuildConfigurationManager.asset";
            AssetDatabase.CreateAsset(configManager, assetPath);

            // Создаем конфигурацию по умолчанию
            var defaultConfig = configManager.CreateNewConfiguration("Default");
            AssetDatabase.CreateAsset(defaultConfig, $"{folderPath}/DefaultConfiguration.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private static void OnPostHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.target is PlayerSettings)
            {
                DrawBuildDeploySystemGUI();
            }
        }

        private static void DrawBuildDeploySystemGUI()
        {
            if (configManager == null)
            {
                LoadConfigurationManager();
                return;
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Build Deploy System", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));

            DrawConfigurationSelection();
            DrawCurrentConfigurationSettings();
            DrawBuildButtons();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static void DrawConfigurationSelection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Configuration:", GUILayout.Width(100));

            var configNames = configManager.Configurations.Select(c => c.configName).ToArray();
            int newIndex = EditorGUILayout.Popup(configManager.SelectedConfigurationIndex, configNames);

            if (newIndex != configManager.SelectedConfigurationIndex)
            {
                configManager.SelectedConfigurationIndex = newIndex;
                EditorUtility.SetDirty(configManager);
            }

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                CreateNewConfiguration();
            }

            if (GUILayout.Button("-", GUILayout.Width(30)) && configManager.Configurations.Count > 1)
            {
                DeleteCurrentConfiguration();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawCurrentConfigurationSettings()
        {
            var config = configManager.SelectedConfiguration;
            if (config == null) return;

            EditorGUILayout.Space(5);

            // Build Settings
            showBuildSettings = EditorGUILayout.Foldout(showBuildSettings, "Build Settings");
            if (showBuildSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                config.configName = EditorGUILayout.TextField("Config Name", config.configName);
                config.buildProfile = EditorGUILayout.TextField("Build Profile", config.buildProfile);
                config.buildTarget = (BuildTargetType)EditorGUILayout.EnumPopup("Build Target", config.buildTarget);
                config.bundleId = EditorGUILayout.TextField("Bundle ID", config.bundleId);
                // WebGL specific settings
                if (config.buildTarget == BuildTargetType.WebGL)
                {
                    EditorGUILayout.LabelField("WebGL Texture Compression Methods:", EditorStyles.boldLabel);
                    DrawTextureCompressionMethods(config);
                }

                EditorGUILayout.EndVertical();
            }

            // Version Settings
            showVersionSettings = EditorGUILayout.Foldout(showVersionSettings, "Version Settings");
            if (showVersionSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                config.version = EditorGUILayout.TextField("Version", config.version);
                config.versionIncrementType = (VersionIncrementType)EditorGUILayout.EnumPopup("Increment Type", config.versionIncrementType);
                config.autoIncrement = EditorGUILayout.Toggle("Auto Increment", config.autoIncrement);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Increment Major"))
                {
                    config.versionIncrementType = VersionIncrementType.Major;
                    config.IncrementVersion();
                }
                if (GUILayout.Button("Increment Minor"))
                {
                    config.versionIncrementType = VersionIncrementType.Minor;
                    config.IncrementVersion();
                }
                if (GUILayout.Button("Increment Patch"))
                {
                    config.versionIncrementType = VersionIncrementType.Auto;
                    config.IncrementVersion();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            // Deploy Settings
            showDeploySettings = EditorGUILayout.Foldout(showDeploySettings, "Deploy Settings");
            if (showDeploySettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                config.autoDeployOnSuccess = EditorGUILayout.Toggle("Auto Deploy on Success", config.autoDeployOnSuccess);
                config.deployConfig.enabled = EditorGUILayout.Toggle("Deploy Enabled", config.deployConfig.enabled);

                if (config.deployConfig.enabled)
                {
                    config.deployConfig.host = EditorGUILayout.TextField("Host", config.deployConfig.host);
                    config.deployConfig.username = EditorGUILayout.TextField("Username", config.deployConfig.username);
                    config.deployConfig.keyPath = EditorGUILayout.TextField("SSH Key Path", config.deployConfig.keyPath);
                    config.deployConfig.remotePath = EditorGUILayout.TextField("Remote Path", config.deployConfig.remotePath);
                    config.deployConfig.port = EditorGUILayout.IntField("Port", config.deployConfig.port);

                    if (GUILayout.Button("Select SSH Key"))
                    {
                        string keyPath = EditorUtility.OpenFilePanel("Select SSH Key", "", "");
                        if (!string.IsNullOrEmpty(keyPath))
                        {
                            config.deployConfig.keyPath = keyPath;
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (UnityEngine.GUI.changed)
            {
                EditorUtility.SetDirty(config);
                EditorUtility.SetDirty(configManager);
            }
        }

        private static void DrawTextureCompressionMethods(BuildConfiguration config)
        {
            EditorGUILayout.BeginVertical();

            bool dxt = config.webglCompressionMethods.Contains(TextureCompressionMethod.DXT);
            bool astc = config.webglCompressionMethods.Contains(TextureCompressionMethod.ASTC);
            bool etc2 = config.webglCompressionMethods.Contains(TextureCompressionMethod.ETC2);

            bool newDxt = EditorGUILayout.Toggle("DXT", dxt);
            bool newAstc = EditorGUILayout.Toggle("ASTC", astc);
            bool newEtc2 = EditorGUILayout.Toggle("ETC2", etc2);

            if (newDxt != dxt)
            {
                if (newDxt) config.webglCompressionMethods.Add(TextureCompressionMethod.DXT);
                else config.webglCompressionMethods.Remove(TextureCompressionMethod.DXT);
            }

            if (newAstc != astc)
            {
                if (newAstc) config.webglCompressionMethods.Add(TextureCompressionMethod.ASTC);
                else config.webglCompressionMethods.Remove(TextureCompressionMethod.ASTC);
            }

            if (newEtc2 != etc2)
            {
                if (newEtc2) config.webglCompressionMethods.Add(TextureCompressionMethod.ETC2);
                else config.webglCompressionMethods.Remove(TextureCompressionMethod.ETC2);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawBuildButtons()
        {
            var config = configManager.SelectedConfiguration;
            if (config == null) return;

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            UnityEngine.GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(config, false);
            }

            UnityEngine.GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Clean Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(config, true);
            }

            UnityEngine.GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Deploy Only", GUILayout.Height(30)) && config.deployConfig.enabled)
            {
                string buildPath = Path.Combine(Application.dataPath, "..", "Builds", config.configName);
                DeploymentSystem.Deploy(config.deployConfig, buildPath);
            }

            UnityEngine.GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private static void CreateNewConfiguration()
        {
            string name = $"Configuration {configManager.Configurations.Count + 1}";
            var config = configManager.CreateNewConfiguration(name);

            string folderPath = "Assets/BuildConfigurations";
            string assetPath = $"{folderPath}/{name}.asset";
            AssetDatabase.CreateAsset(config, assetPath);

            configManager.SelectedConfigurationIndex = configManager.Configurations.Count - 1;

            EditorUtility.SetDirty(configManager);
            AssetDatabase.SaveAssets();
        }

        private static void DeleteCurrentConfiguration()
        {
            var config = configManager.SelectedConfiguration;
            if (config != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(config);
                configManager.RemoveConfiguration(config);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }

                EditorUtility.SetDirty(configManager);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
