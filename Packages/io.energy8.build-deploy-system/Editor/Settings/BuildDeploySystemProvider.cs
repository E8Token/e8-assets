using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Energy8.BuildDeploySystem.Editor
{
    public class BuildDeploySystemProvider : SettingsProvider
    {
        private BuildConfigurationManager configManager;
        private SerializedObject serializedConfigManager;
        private bool showAdvancedSettings = false;
        
        public BuildDeploySystemProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            LoadConfigurationManager();
        }
        
        private void LoadConfigurationManager()
        {
            if (configManager == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:BuildConfigurationManager");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    configManager = AssetDatabase.LoadAssetAtPath<BuildConfigurationManager>(path);
                }
            }
            
            if (configManager != null)
            {
                serializedConfigManager = new SerializedObject(configManager);
            }
        }
        
        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("Energy8 Build Deploy System", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            if (configManager == null)
            {
                DrawSetupSection();
                return;
            }
            
            DrawConfigurationSection();
            EditorGUILayout.Space(10);
            DrawQuickActionsSection();
            EditorGUILayout.Space(10);
            DrawAdvancedSettingsSection();
            
            if (UnityEngine.GUI.changed && serializedConfigManager != null)
            {
                serializedConfigManager.ApplyModifiedProperties();
                EditorUtility.SetDirty(configManager);
            }
        }
        
        private void DrawSetupSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Setup Required", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("No BuildConfigurationManager found. Create one to get started.", MessageType.Warning);
            
            if (GUILayout.Button("Create Configuration Manager", GUILayout.Height(25)))
            {
                CreateConfigurationManager();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawConfigurationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Build Configurations", EditorStyles.boldLabel);
            
            if (configManager.Configurations.Count == 0)
            {
                EditorGUILayout.HelpBox("No configurations found. Create your first one!", MessageType.Info);
                if (GUILayout.Button("Create Default Configuration"))
                {
                    CreateDefaultConfiguration();
                }
            }
            else
            {
                // Configuration selection
                string[] configNames = new string[configManager.Configurations.Count];
                for (int i = 0; i < configManager.Configurations.Count; i++)
                {
                    configNames[i] = configManager.Configurations[i]?.configName ?? "Unnamed";
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Active Configuration:", GUILayout.Width(120));
                configManager.SelectedConfigurationIndex = EditorGUILayout.Popup(
                    configManager.SelectedConfigurationIndex, configNames);
                EditorGUILayout.EndHorizontal();
                
                var selectedConfig = configManager.SelectedConfiguration;
                if (selectedConfig != null)
                {
                    EditorGUILayout.Space(5);
                    DrawConfigurationInfo(selectedConfig);
                }
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create New"))
            {
                CreateNewConfiguration();
            }
            
            if (GUILayout.Button("Manage Configurations"))
            {
                BuildDeploySystemWindow.ShowWindow();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawConfigurationInfo(BuildConfiguration config)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Configuration Info", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(80));
            EditorGUILayout.LabelField(config.configName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Version:", GUILayout.Width(80));
            EditorGUILayout.LabelField(config.version);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target:", GUILayout.Width(80));
            EditorGUILayout.LabelField(config.buildTarget.ToString());
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Profile:", GUILayout.Width(80));
            EditorGUILayout.LabelField(config.buildProfile);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawQuickActionsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            var selectedConfig = configManager?.SelectedConfiguration;
            EditorGUI.BeginDisabledGroup(selectedConfig == null);
            
            EditorGUILayout.BeginHorizontal();
            
            UnityEngine.GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(selectedConfig, false);
            }
            
            UnityEngine.GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Clean Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(selectedConfig, true);
            }
            
            UnityEngine.GUI.backgroundColor = Color.cyan;
            EditorGUI.BeginDisabledGroup(!selectedConfig?.deployConfig.enabled ?? true);
            if (GUILayout.Button("Deploy", GUILayout.Height(30)))
            {
                string buildPath = System.IO.Path.Combine(Application.dataPath, "..", "Builds", selectedConfig.configName);
                DeploymentSystem.Deploy(selectedConfig.deployConfig, buildPath);
            }
            EditorGUI.EndDisabledGroup();
            
            UnityEngine.GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAdvancedSettingsSection()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings", true);
            
            if (showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var settings = BuildDeploySystemSettings.Data;
                
                settings.autoSaveOnBuild = EditorGUILayout.Toggle("Auto Save on Build", settings.autoSaveOnBuild);
                settings.showAdvancedOptions = EditorGUILayout.Toggle("Show Advanced Options", settings.showAdvancedOptions);
                
                EditorGUILayout.BeginHorizontal();
                settings.configurationsPath = EditorGUILayout.TextField("Configurations Path", settings.configurationsPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Configurations Folder", settings.configurationsPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        settings.configurationsPath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Save Settings"))
                {
                    BuildDeploySystemSettings.SaveSettings();
                    EditorUtility.DisplayDialog("Settings Saved", "Settings saved!", "OK");
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        private void CreateConfigurationManager()
        {
            configManager = ScriptableObject.CreateInstance<BuildConfigurationManager>();
            
            string folderPath = "Assets/BuildConfigurations";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "BuildConfigurations");
            }
            
            string assetPath = $"{folderPath}/BuildConfigurationManager.asset";
            AssetDatabase.CreateAsset(configManager, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            serializedConfigManager = new SerializedObject(configManager);
            
            // Create default configuration
            CreateDefaultConfiguration();
        }
        
        private void CreateDefaultConfiguration()
        {
            if (configManager == null) return;
            
            var config = configManager.CreateNewConfiguration("Default");
            config.buildTarget = BuildTargetType.WebGL;
            config.webglCompressionMethods.Add(TextureCompressionMethod.DXT);
            
            string folderPath = "Assets/BuildConfigurations";
            string assetPath = $"{folderPath}/DefaultConfiguration.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            
            EditorUtility.SetDirty(configManager);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateNewConfiguration()
        {
            if (configManager == null) return;
            
            string name = $"Configuration {configManager.Configurations.Count + 1}";
            var config = configManager.CreateNewConfiguration(name);
            
            string folderPath = "Assets/BuildConfigurations";
            string assetPath = $"{folderPath}/{name.Replace(" ", "")}.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            
            EditorUtility.SetDirty(configManager);
            AssetDatabase.SaveAssets();
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateBuildDeploySystemProvider()
        {
            var provider = new BuildDeploySystemProvider("Project/Build Deploy System", SettingsScope.Project);
            provider.keywords = new HashSet<string>(new[] { "Build", "Deploy", "Energy8", "Configuration" });
            return provider;
        }
    }
}
