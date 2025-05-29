using UnityEngine;
using UnityEditor;
using System.IO;

namespace Energy8.BuildDeploySystem.Editor
{
    public class BuildDeploySystemWindow : EditorWindow
    {
        private BuildConfigurationManager configManager;
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private string[] tabNames = { "Build", "Deploy", "Settings", "Logs" };
        
        private string buildLog = "";
        private bool isBuilding = false;
        private bool isDeploying = false;
        
        [MenuItem("Window/Energy8/Build Deploy System")]
        public static void ShowWindow()
        {
            GetWindow<BuildDeploySystemWindow>("Build Deploy System");
        }
        
        private void OnEnable()
        {
            LoadConfigurationManager();
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void LoadConfigurationManager()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildConfigurationManager");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                configManager = AssetDatabase.LoadAssetAtPath<BuildConfigurationManager>(path);
            }
        }
        
        private void SubscribeToEvents()
        {
            BuildSystem.OnBuildStarted += OnBuildStarted;
            BuildSystem.OnBuildCompleted += OnBuildCompleted;
            BuildSystem.OnBuildProgress += OnBuildProgress;
            DeploymentSystem.OnDeployStarted += OnDeployStarted;
            DeploymentSystem.OnDeployCompleted += OnDeployCompleted;
            DeploymentSystem.OnDeployProgress += OnDeployProgress;
        }
        
        private void UnsubscribeFromEvents()
        {
            BuildSystem.OnBuildStarted -= OnBuildStarted;
            BuildSystem.OnBuildCompleted -= OnBuildCompleted;
            BuildSystem.OnBuildProgress -= OnBuildProgress;
            DeploymentSystem.OnDeployStarted -= OnDeployStarted;
            DeploymentSystem.OnDeployCompleted -= OnDeployCompleted;
            DeploymentSystem.OnDeployProgress -= OnDeployProgress;
        }
        
        private void OnGUI()
        {
            if (configManager == null)
            {
                EditorGUILayout.HelpBox("BuildConfigurationManager not found. Please create one first.", MessageType.Warning);
                if (GUILayout.Button("Create Configuration Manager"))
                {
                    CreateConfigurationManager();
                }
                return;
            }
            
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            
            EditorGUILayout.Space(10);
            
            switch (selectedTab)
            {
                case 0:
                    DrawBuildTab();
                    break;
                case 1:
                    DrawDeployTab();
                    break;
                case 2:
                    DrawSettingsTab();
                    break;
                case 3:
                    DrawLogsTab();
                    break;
            }
        }
        
        private void DrawBuildTab()
        {
            EditorGUILayout.LabelField("Build Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (configManager.Configurations.Count == 0)
            {
                EditorGUILayout.HelpBox("No build configurations found. Create one to get started.", MessageType.Info);
                if (GUILayout.Button("Create Default Configuration"))
                {
                    CreateDefaultConfiguration();
                }
                return;
            }
            
            // Configuration selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Configuration:", GUILayout.Width(130));
            
            string[] configNames = new string[configManager.Configurations.Count];
            for (int i = 0; i < configManager.Configurations.Count; i++)
            {
                configNames[i] = configManager.Configurations[i].configName;
            }
            
            configManager.SelectedConfigurationIndex = EditorGUILayout.Popup(
                configManager.SelectedConfigurationIndex, configNames);
            
            EditorGUILayout.EndHorizontal();
            
            var selectedConfig = configManager.SelectedConfiguration;
            if (selectedConfig != null)
            {
                EditorGUILayout.Space(10);
                
                // Quick info
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Configuration Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {selectedConfig.configName}");
                EditorGUILayout.LabelField($"Version: {selectedConfig.version}");
                EditorGUILayout.LabelField($"Target: {selectedConfig.buildTarget}");
                EditorGUILayout.LabelField($"Profile: {selectedConfig.buildProfile}");
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(10);
                
                // Build buttons
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Build Actions", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(isBuilding || isDeploying);
                
                if (GUILayout.Button("Build", GUILayout.Height(30)))
                {
                    BuildSystem.StartBuild(selectedConfig, false);
                }
                
                if (GUILayout.Button("Clean Build", GUILayout.Height(30)))
                {
                    BuildSystem.StartBuild(selectedConfig, true);
                }
                
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
                
                // Build status
                if (isBuilding)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Building...", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Please wait while the build is in progress.");
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
        private void DrawDeployTab()
        {
            EditorGUILayout.LabelField("Deployment Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            var selectedConfig = configManager?.SelectedConfiguration;
            if (selectedConfig == null)
            {
                EditorGUILayout.HelpBox("No configuration selected.", MessageType.Warning);
                return;
            }
            
            // Deploy configuration
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Deploy Configuration", EditorStyles.boldLabel);
            
            selectedConfig.deployConfig.enabled = EditorGUILayout.Toggle("Enable Deploy", selectedConfig.deployConfig.enabled);
            
            if (selectedConfig.deployConfig.enabled)
            {
                selectedConfig.deployConfig.host = EditorGUILayout.TextField("Host", selectedConfig.deployConfig.host);
                selectedConfig.deployConfig.username = EditorGUILayout.TextField("Username", selectedConfig.deployConfig.username);
                selectedConfig.deployConfig.port = EditorGUILayout.IntField("Port", selectedConfig.deployConfig.port);
                selectedConfig.deployConfig.remotePath = EditorGUILayout.TextField("Remote Path", selectedConfig.deployConfig.remotePath);
                
                EditorGUILayout.BeginHorizontal();
                selectedConfig.deployConfig.keyPath = EditorGUILayout.TextField("SSH Key Path", selectedConfig.deployConfig.keyPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string path = EditorUtility.OpenFilePanel("Select SSH Key", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        selectedConfig.deployConfig.keyPath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                selectedConfig.autoDeployOnSuccess = EditorGUILayout.Toggle("Auto Deploy on Build Success", selectedConfig.autoDeployOnSuccess);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Deploy actions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Deploy Actions", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(!selectedConfig.deployConfig.enabled || isBuilding || isDeploying);
            
            if (GUILayout.Button("Deploy Now", GUILayout.Height(30)))
            {
                string buildPath = Path.Combine(Application.dataPath, "..", "Builds", selectedConfig.configName);
                DeploymentSystem.Deploy(selectedConfig.deployConfig, buildPath);
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
            
            // Deploy status
            if (isDeploying)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Deploying...", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Please wait while deployment is in progress.");
                EditorGUILayout.EndVertical();
            }
            
            if (UnityEngine.GUI.changed)
            {
                EditorUtility.SetDirty(selectedConfig);
            }
        }
        
        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("System Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            var settings = BuildDeploySystemSettings.Data;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            
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
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Save Settings"))
            {
                BuildDeploySystemSettings.SaveSettings();
                EditorUtility.DisplayDialog("Settings", "Settings saved successfully!", "OK");
            }
        }
        
        private void DrawLogsTab()
        {
            EditorGUILayout.LabelField("Build & Deploy Logs", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Console Output", EditorStyles.boldLabel);
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                buildLog = "";
            }
            EditorGUILayout.EndHorizontal();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            EditorGUILayout.TextArea(buildLog, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
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
        }
        
        private void CreateDefaultConfiguration()
        {
            var config = configManager.CreateNewConfiguration("Default");
            config.buildTarget = BuildTargetType.WebGL;
            config.webglCompressionMethods.Add(TextureCompressionMethod.DXT);
            
            string folderPath = "Assets/BuildConfigurations";
            string assetPath = $"{folderPath}/DefaultConfiguration.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            
            EditorUtility.SetDirty(configManager);
            AssetDatabase.SaveAssets();
        }
        
        // Event handlers
        private void OnBuildStarted(string configName)
        {
            isBuilding = true;
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] Build started for configuration: {configName}\n";
            Repaint();
        }
        
        private void OnBuildCompleted(string configName, bool success)
        {
            isBuilding = false;
            string status = success ? "SUCCESS" : "FAILED";
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] Build {status} for configuration: {configName}\n";
            Repaint();
        }
        
        private void OnBuildProgress(string message)
        {
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
            Repaint();
        }
        
        private void OnDeployStarted(string host)
        {
            isDeploying = true;
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] Deploy started to: {host}\n";
            Repaint();
        }
        
        private void OnDeployCompleted(string host, bool success)
        {
            isDeploying = false;
            string status = success ? "SUCCESS" : "FAILED";
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] Deploy {status} to: {host}\n";
            Repaint();
        }
        
        private void OnDeployProgress(string message)
        {
            buildLog += $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
            Repaint();
        }
    }
}
