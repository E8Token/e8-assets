using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public class BuildDeployWindow : EditorWindow
    {
        private List<BuildConfiguration> configurations = new();
        private Vector2 scrollPosition;
        private BuildConfiguration selectedConfiguration;
        private readonly Dictionary<string, bool> configurationFoldouts = new();

        [MenuItem("E8 Tools/Build Deploy System")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildDeployWindow>("Build Deploy System");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            RefreshConfigurations();
            BuildManager.OnBuildCompleted += OnBuildCompleted;
            BuildProfileScanner.OnConfigurationsUpdated += OnConfigurationsUpdated;
        }

        private void OnDisable()
        {
            BuildManager.OnBuildCompleted -= OnBuildCompleted;
            BuildProfileScanner.OnConfigurationsUpdated -= OnConfigurationsUpdated;
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(10);

            DrawGlobalVersionSection();
            EditorGUILayout.Space(10);

            DrawConfigurationsSection();
            EditorGUILayout.Space(10);

            DrawBuildSection();
        }

        private void DrawHeader()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Energy8 Build Deploy System", style);
        }

        private void DrawGlobalVersionSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Project Version Management", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Version:", GUILayout.Width(100));

            string currentVersion = "1.0.1";

            try
            {
                var globalVersion = GlobalVersion.Instance;
                if (globalVersion != null)
                {
                    currentVersion = globalVersion.Version;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildDeployWindow] Failed to get version: {ex.Message}");
            }

            var newVersion = EditorGUILayout.TextField(currentVersion, GUILayout.Width(80));
            if (newVersion != currentVersion)
            {
                try
                {
                    GlobalVersion.Instance.Version = newVersion;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildDeployWindow] Failed to set version: {ex.Message}");
                }
            }

            if (GUILayout.Button("Add Minor", GUILayout.Width(80)))
            {
                try
                {
                    GlobalVersion.Instance?.IncrementMinor();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildDeployWindow] Failed to increment minor version: {ex.Message}");
                }
            }

            if (GUILayout.Button("Major Release", GUILayout.Width(100)))
            {
                try
                {
                    GlobalVersion.Instance?.IncrementMajor();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildDeployWindow] Failed to increment major version: {ex.Message}");
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigurationsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Profiles", EditorStyles.boldLabel);

            if (configurations.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No configurations found.\n" +
                    "Make sure you have Build Profiles in the folder Assets/Settings/Build Profiles\n" +
                    "Click 'Refresh Configurations' to create them.",
                    MessageType.Info);
            }
            else
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                for (int i = 0; i < configurations.Count; i++)
                {
                    DrawConfigurationItem(configurations[i]);
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }
        private void DrawConfigurationItem(BuildConfiguration config)
        {
            if (config == null) return;

            bool isSelected = selectedConfiguration == config;
            var bgColor = isSelected ? new Color(0.3f, 0.5f, 0.8f, 0.3f) : Color.clear;
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = originalColor;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
            {
                selectedConfiguration = config;
            }

            string configKey = config.BuildProfileGUID;
            if (!configurationFoldouts.ContainsKey(configKey))
            {
                configurationFoldouts[configKey] = false;
            }

            var platformIcon = GetPlatformIcon(config);
            var foldoutLabel = $"{platformIcon} {config.BuildProfileName}";

            configurationFoldouts[configKey] = EditorGUILayout.Foldout(
                configurationFoldouts[configKey],
                foldoutLabel,
                true,
                EditorStyles.foldoutHeader
            );

            if (config.IsValid())
            {
                GUILayout.Label("✅", GUILayout.Width(20));
            }
            else
            {
                GUILayout.Label("⚠️", GUILayout.Width(20));
            }

            EditorGUILayout.EndHorizontal();

            if (configurationFoldouts[configKey])
            {
                EditorGUI.indentLevel++;

                DrawEnvironmentSection(config);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Build Profile", config.BuildProfile, typeof(UnityEditor.Build.Profile.BuildProfile), false);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.BeginHorizontal();
                var newOutputPath = EditorGUILayout.TextField("Output Path", config.OutputPath);
                if (newOutputPath != config.OutputPath)
                {
                    config.OutputPath = newOutputPath;
                }

                if (GUILayout.Button("📁", GUILayout.Width(30)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Output Directory", config.OutputPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        config.OutputPath = Path.GetRelativePath(Application.dataPath + "/../", path);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(
                    "💡 Available placeholders: {{{VERSION}}},{{{PLATFORM}}}, {{{DATE}}}, {{{DATETIME}}}", EditorStyles.miniLabel);

                if (string.IsNullOrEmpty(config.OutputPath))
                {
                    EditorGUILayout.LabelField("📋 Example: Builds/{{{PLATFORM}}}/{{{VERSION}}}", EditorStyles.miniLabel);
                }
                if (!string.IsNullOrEmpty(config.OutputPath) &&
                           config.OutputPath.Contains("{{{") &&
                           config.OutputPath.Contains("}}}"))
                {
                    var processedPath = BuildManager.ProcessOutputPathTemplate(config.OutputPath, config);
                    EditorGUILayout.LabelField($"➤ Resolved: {processedPath}", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("🔧 Build Options", EditorStyles.boldLabel);
                var alwaysCleanBuild = EditorGUILayout.Toggle("Always Clean Build", config.AlwaysCleanBuild);
                if (alwaysCleanBuild != config.AlwaysCleanBuild)
                {
                    config.AlwaysCleanBuild = alwaysCleanBuild;
                }

                DrawPlatformSpecificSettings(config);

                if (!config.IsValid())
                {
                    if (config.BuildProfile == null)
                    {
                        EditorGUILayout.HelpBox("❌ Build Profile not found or has been deleted", MessageType.Error);
                    }
                    else if (string.IsNullOrEmpty(config.OutputPath))
                    {
                        EditorGUILayout.HelpBox("❌ Output path not specified", MessageType.Error);
                    }
                }
                else if (isSelected)
                {
                    EditorGUILayout.HelpBox("✅ Configuration is ready for build", MessageType.Info);
                }

                DrawDeploySettings(config);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Actions", EditorStyles.boldLabel);

            if (selectedConfiguration != null)
            {
                EditorGUILayout.LabelField($"Selected: {selectedConfiguration.GetDisplayName()}", EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                if (!selectedConfiguration.AlwaysCleanBuild)
                {
                    if (GUILayout.Button("🧹 Clean Build", GUILayout.Height(35)))
                    {
                        BuildManager.BuildConfiguration(selectedConfiguration,
                            selectedConfiguration.DeploySettings.AlwaysDeploy,
                            true);
                    }
                }

                if (GUILayout.Button("🔨 Build", GUILayout.Height(35)))
                {
                    BuildManager.BuildConfiguration(selectedConfiguration,
                        selectedConfiguration.DeploySettings.AlwaysDeploy,
                        selectedConfiguration.AlwaysCleanBuild);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                DrawDeployButtons();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a configuration to build", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDeployButtons()
        {
            if (selectedConfiguration == null) return;
            if (!selectedConfiguration.DeploySettings.EnableDeploy) return;

            var deploySettings = selectedConfiguration.DeploySettings;

            if (deploySettings.AlwaysDeploy)
            {
                EditorGUILayout.HelpBox("✅ Auto Deploy is enabled - every successful build will be automatically deployed", MessageType.Info);
            }
            else if (deploySettings.EnableDeploy)
            {
                EditorGUILayout.BeginHorizontal();

                if (!selectedConfiguration.AlwaysCleanBuild)
                {
                    if (GUILayout.Button("🧹🔨🚀 Clean Build & Deploy", GUILayout.Height(35)))
                    {
                        BuildManager.BuildConfiguration(selectedConfiguration, true, true);
                    }
                }


                if (GUILayout.Button("🔨🚀 Build & Deploy", GUILayout.Height(30)))
                {
                    BuildManager.BuildConfiguration(selectedConfiguration, true, false);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void RefreshConfigurations()
        {
            configurations = BuildProfileScanner.ScanAndCreateConfigurations();
            Repaint();
        }

        private void OnConfigurationsUpdated(List<BuildConfiguration> newConfigurations)
        {
            configurations = newConfigurations;

            if (selectedConfiguration != null && !configurations.Contains(selectedConfiguration))
            {
                selectedConfiguration = null;
            }

            Repaint();
        }

        private void OnBuildCompleted(string configName, bool success)
        {
            Repaint();

            if (success)
            {
                Debug.Log($"[BuildSystem] Build '{configName}' completed successfully!");
            }
            else
            {
                Debug.LogError($"[BuildSystem] Build '{configName}' failed. Check console for details.");
            }
        }

        private string GetPlatformIcon(BuildConfiguration config)
        {
            if (config.BuildProfile == null) return "🎯";

            var profileName = config.BuildProfileName.ToLower();

            if (profileName.Contains("windows") || profileName.Contains("win"))
                return "🪟";
            if (profileName.Contains("mac") || profileName.Contains("osx"))
                return "🍎";
            if (profileName.Contains("linux"))
                return "🐧";
            if (profileName.Contains("android"))
                return "🤖";
            if (profileName.Contains("ios"))
                return "📱";
            if (profileName.Contains("webgl") || profileName.Contains("web"))
                return "🌐";

            return "🎯";
        }

        private void DrawEnvironmentSection(BuildConfiguration config)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);

            // Получаем список доступных сред
            var availableEnvironments = GetAvailableEnvironments();
            
            var options = new List<string> { "(Use Current)" };
            options.AddRange(availableEnvironments);

            int selectedIndex = 0;
            if (!string.IsNullOrEmpty(config.TargetEnvironment))
            {
                selectedIndex = options.IndexOf(config.TargetEnvironment);
                if (selectedIndex == -1)
                {
                    options.Add(config.TargetEnvironment);
                    selectedIndex = options.Count - 1;
                }
            }

            var newIndex = EditorGUILayout.Popup("Target Environment:", selectedIndex, options.ToArray());
            
            if (newIndex != selectedIndex)
            {
                config.TargetEnvironment = newIndex == 0 ? "" : options[newIndex];
            }

            EditorGUILayout.HelpBox(
                newIndex == 0 
                    ? "Current environment from environment.json will be used" 
                    : $"Will switch to: {options[newIndex]} before build", 
                MessageType.Info);
        }

        private string[] GetAvailableEnvironments()
        {
            var environmentsType = System.Type.GetType("Energy8.EnvironmentConfig.Editor.Settings.EnvironmentSettings,Energy8.EnvironmentConfig.Editor");
            if (environmentsType == null)
                return Array.Empty<string>();

            try
            {
                var settingsMethod = environmentsType.GetMethod("FindSettings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (settingsMethod == null)
                    return Array.Empty<string>();

                var settings = settingsMethod.Invoke(null, null);
                if (settings == null)
                    return Array.Empty<string>();

                var environmentsField = settings.GetType().GetField("environments");
                if (environmentsField == null)
                    return Array.Empty<string>();

                var environments = environmentsField.GetValue(settings) as System.Array;
                if (environments == null || environments.Length == 0)
                    return Array.Empty<string>();

                var names = new List<string>();
                foreach (var env in environments)
                {
                    var envName = env.GetType().GetField("name")?.GetValue(env) as string;
                    if (!string.IsNullOrEmpty(envName))
                        names.Add(envName);
                }

                return names.ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildDeployWindow] Failed to load environments: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private void DrawPlatformSpecificSettings(BuildConfiguration config)
        {
            if (config.BuildProfile == null) return;

            var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Platform Settings", EditorStyles.boldLabel);

            if (buildTarget == BuildTarget.WebGL)
            {
                DrawWebGLSettings(config);
            }
            else if (buildTarget == BuildTarget.Android)
            {
                DrawAndroidSettings(config);
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                DrawIOSSettings(config);
            }
            else if (buildTarget == BuildTarget.StandaloneWindows64 ||
                           buildTarget == BuildTarget.StandaloneOSX ||
                           buildTarget == BuildTarget.StandaloneLinux64)
            {
                DrawStandaloneSettings(config);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Platform-specific settings not available for: {config.BuildProfileName}\n" +
                    "Supported platforms: WebGL, Android, iOS, Windows, macOS, Linux",
                    MessageType.Info
                );
            }
        }
        private void DrawWebGLSettings(BuildConfiguration config)
        {
            var settings = config.WebGLSettings; EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🌐 WebGL Multi-Format Build", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Texture Formats to Build", EditorStyles.boldLabel);

            var newBuildDXT = EditorGUILayout.Toggle("Build DXT (Desktop)", settings.BuildDXT);
            if (newBuildDXT != settings.BuildDXT)
            {
                settings.BuildDXT = newBuildDXT;
                config.WebGLSettings = settings;
            }

            var newBuildASTC = EditorGUILayout.Toggle("Build ASTC (Mobile)", settings.BuildASTC);
            if (newBuildASTC != settings.BuildASTC)
            {
                settings.BuildASTC = newBuildASTC;
                config.WebGLSettings = settings;
            }

            var newBuildETC2 = EditorGUILayout.Toggle("Build ETC2 (Mobile)", settings.BuildETC2);
            if (newBuildETC2 != settings.BuildETC2)
            {
                settings.BuildETC2 = newBuildETC2;
                config.WebGLSettings = settings;
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Compression Algorithms to Build", EditorStyles.boldLabel);
            var availableAlgorithms = new[] { CompressionAlgorithm.Brotli, CompressionAlgorithm.Gzip };
            foreach (var algo in availableAlgorithms)
            {
                bool selected = settings.CompressionAlgorithms.Contains(algo);
                bool newSelected = EditorGUILayout.Toggle(algo.ToString(), selected);
                if (newSelected != selected)
                {
                    if (newSelected)
                        settings.CompressionAlgorithms.Add(algo);
                    else
                        settings.CompressionAlgorithms.Remove(algo);
                    config.WebGLSettings = settings;
                }
            }

            EditorGUILayout.Space(5);
            if (settings.HasMultipleFormats() || settings.CompressionAlgorithms.Count > 0)
            {
                var additionalFormats = settings.GetTextureFormatsToBuild();
                var compressionNames = settings.GetCompressionAlgorithmNames();
                EditorGUILayout.HelpBox(
                    "Multi-Format Build Enabled!\n" +
                    $"Selected formats: {string.Join(", ", additionalFormats)}\n" +
                    $"Compression: {string.Join(", ", compressionNames)}\n" +
                    "Data files will be named: [appname].[format].[compression].data",
                    MessageType.Info
                );
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAndroidSettings(BuildConfiguration config)
        {
            var settings = config.AndroidSettings;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🤖 Android Settings", EditorStyles.boldLabel);

            var newBuildAAB = EditorGUILayout.Toggle("Build AAB", settings.BuildAAB);
            if (newBuildAAB != settings.BuildAAB)
            {
                settings.BuildAAB = newBuildAAB;
                config.AndroidSettings = settings;
            }

            var newBuildAPK = EditorGUILayout.Toggle("Build APK", settings.BuildAPK);
            if (newBuildAPK != settings.BuildAPK)
            {
                settings.BuildAPK = newBuildAPK;
                config.AndroidSettings = settings;
            }

            var newUseCustomKeystore = EditorGUILayout.Toggle("Use Custom Keystore", settings.UseCustomKeystore);
            if (newUseCustomKeystore != settings.UseCustomKeystore)
            {
                settings.UseCustomKeystore = newUseCustomKeystore;
                config.AndroidSettings = settings;
            }

            if (settings.UseCustomKeystore)
            {
                var newKeystorePath = EditorGUILayout.TextField("Keystore Path", settings.KeystorePath);
                if (newKeystorePath != settings.KeystorePath)
                {
                    settings.KeystorePath = newKeystorePath;
                    config.AndroidSettings = settings;
                }
            }

            EditorGUILayout.LabelField("Architecture", EditorStyles.boldLabel);

            var newBuildARM64 = EditorGUILayout.Toggle("Build ARM64", settings.BuildARM64);
            if (newBuildARM64 != settings.BuildARM64)
            {
                settings.BuildARM64 = newBuildARM64;
                config.AndroidSettings = settings;
            }

            var newBuildARMv7 = EditorGUILayout.Toggle("Build ARMv7", settings.BuildARMv7);
            if (newBuildARMv7 != settings.BuildARMv7)
            {
                settings.BuildARMv7 = newBuildARMv7;
                config.AndroidSettings = settings;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawIOSSettings(BuildConfiguration config)
        {
            var settings = config.IOSSettings;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📱 iOS Settings", EditorStyles.boldLabel);

            var newTeamId = EditorGUILayout.TextField("Development Team ID", settings.DevelopmentTeamId);
            if (newTeamId != settings.DevelopmentTeamId)
            {
                settings.DevelopmentTeamId = newTeamId;
                config.IOSSettings = settings;
            }

            var newAutoSigning = EditorGUILayout.Toggle("Automatic Signing", settings.AutomaticSigning);
            if (newAutoSigning != settings.AutomaticSigning)
            {
                settings.AutomaticSigning = newAutoSigning;
                config.IOSSettings = settings;
            }

            if (!settings.AutomaticSigning)
            {
                var newProvisioningProfile = EditorGUILayout.TextField("Provisioning Profile Path", settings.ProvisioningProfilePath);
                if (newProvisioningProfile != settings.ProvisioningProfilePath)
                {
                    settings.ProvisioningProfilePath = newProvisioningProfile;
                    config.IOSSettings = settings;
                }
            }

            EditorGUILayout.LabelField("Architecture", EditorStyles.boldLabel);

            var newBuildDevice = EditorGUILayout.Toggle("Build for Device", settings.BuildDevice);
            if (newBuildDevice != settings.BuildDevice)
            {
                settings.BuildDevice = newBuildDevice;
                config.IOSSettings = settings;
            }

            var newBuildSimulator = EditorGUILayout.Toggle("Build for Simulator", settings.BuildSimulator);
            if (newBuildSimulator != settings.BuildSimulator)
            {
                settings.BuildSimulator = newBuildSimulator;
                config.IOSSettings = settings;
            }

            EditorGUILayout.EndVertical();
        }
        private void DrawStandaloneSettings(BuildConfiguration config)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("💻 Standalone Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Standalone settings will be available in a future update.", MessageType.Info);

            EditorGUILayout.EndVertical();
        }
        private void DrawDeploySettings(BuildConfiguration config)
        {
            var settings = config.DeploySettings;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🚀 Deploy Settings", EditorStyles.boldLabel);

            var newEnableDeploy = EditorGUILayout.Toggle("Enable Deploy", settings.EnableDeploy);
            if (newEnableDeploy != settings.EnableDeploy)
            {
                settings.EnableDeploy = newEnableDeploy;
                config.DeploySettings = settings;
            }

            if (settings.EnableDeploy)
            {
                var newAlwaysDeploy = EditorGUILayout.Toggle("Always Deploy", settings.AlwaysDeploy);
                if (newAlwaysDeploy != settings.AlwaysDeploy)
                {
                    settings.AlwaysDeploy = newAlwaysDeploy;
                    config.DeploySettings = settings;
                }

                EditorGUILayout.Space(5);

                var newDeployMethod = (DeployMethod)EditorGUILayout.EnumPopup("Deploy Method", settings.Method);
                if (newDeployMethod != settings.Method)
                {
                    settings.Method = newDeployMethod;
                    config.DeploySettings = settings;
                }

                if (settings.Method == DeployMethod.LocalCopy)
                {
                    EditorGUILayout.BeginHorizontal();
                    var newLocalPath = EditorGUILayout.TextField("Local Copy Target Path", settings.LocalCopyTargetPath);
                    if (newLocalPath != settings.LocalCopyTargetPath)
                    {
                        settings.LocalCopyTargetPath = newLocalPath;
                        config.DeploySettings = settings;
                    }
                    if (GUILayout.Button("📁", GUILayout.Width(30)))
                    {
                        string path = EditorUtility.OpenFolderPanel("Select Local Deploy Directory", settings.LocalCopyTargetPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            settings.LocalCopyTargetPath = path;
                            config.DeploySettings = settings;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (settings.Method == DeployMethod.FTP || settings.Method == DeployMethod.SFTP)
                {
                    var newServerHost = EditorGUILayout.TextField("Server Host", settings.ServerHost);
                    if (newServerHost != settings.ServerHost)
                    {
                        settings.ServerHost = newServerHost;
                        config.DeploySettings = settings;
                    }

                    var newServerPort = EditorGUILayout.IntField("Server Port", settings.ServerPort);
                    if (newServerPort != settings.ServerPort)
                    {
                        settings.ServerPort = newServerPort;
                        config.DeploySettings = settings;
                    }

                    var newRemotePath = EditorGUILayout.TextField("Remote Path", settings.RemotePath);
                    if (newRemotePath != settings.RemotePath)
                    {
                        settings.RemotePath = newRemotePath;
                        config.DeploySettings = settings;
                    }

                    EditorGUILayout.Space(5);

                    EditorGUILayout.LabelField("Authentication", EditorStyles.boldLabel);

                    var newAuthMethod = (AuthenticationMethod)EditorGUILayout.EnumPopup("Auth Method", settings.AuthMethod);
                    if (newAuthMethod != settings.AuthMethod)
                    {
                        settings.AuthMethod = newAuthMethod;
                        config.DeploySettings = settings;
                    }

                    var newUsername = EditorGUILayout.TextField("Username", settings.Username);
                    if (newUsername != settings.Username)
                    {
                        settings.Username = newUsername;
                        config.DeploySettings = settings;
                    }

                    if (settings.AuthMethod == AuthenticationMethod.Password)
                    {
                        var newPassword = EditorGUILayout.PasswordField("Password", settings.Password);
                        if (newPassword != settings.Password)
                        {
                            settings.Password = newPassword;
                            config.DeploySettings = settings;
                        }
                    }
                    else if (settings.AuthMethod == AuthenticationMethod.PrivateKey)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var newPrivateKeyPath = EditorGUILayout.TextField("Private Key Path", settings.PrivateKeyPath);
                        if (newPrivateKeyPath != settings.PrivateKeyPath)
                        {
                            settings.PrivateKeyPath = newPrivateKeyPath;
                            config.DeploySettings = settings;
                        }

                        if (GUILayout.Button("📁", GUILayout.Width(30)))
                        {
                            string path = EditorUtility.OpenFilePanel("Select Private Key", "", "");
                            if (!string.IsNullOrEmpty(path))
                            {
                                settings.PrivateKeyPath = path;
                                config.DeploySettings = settings;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        var newPassphrase = EditorGUILayout.PasswordField("Key Passphrase (optional)", settings.PrivateKeyPassphrase);
                        if (newPassphrase != settings.PrivateKeyPassphrase)
                        {
                            settings.PrivateKeyPassphrase = newPassphrase;
                            config.DeploySettings = settings;
                        }
                    }
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("🗂️ Deploy Options", EditorStyles.boldLabel);

                // Основные опции очистки и архивирования
                var newDeleteExisting = EditorGUILayout.Toggle("Delete Existing Files", settings.DeleteExistingFiles);
                if (newDeleteExisting != settings.DeleteExistingFiles)
                {
                    settings.DeleteExistingFiles = newDeleteExisting;
                    config.DeploySettings = settings;
                }

                var newCreateBackup = EditorGUILayout.Toggle("Create Backup Before Deploy", settings.CreateBackup);
                if (newCreateBackup != settings.CreateBackup)
                {
                    settings.CreateBackup = newCreateBackup;
                    config.DeploySettings = settings;
                }

                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("📦 Archive Options", EditorStyles.boldLabel);
                
                var newDeployZipOnly = EditorGUILayout.Toggle("Deploy as ZIP Archive", settings.DeployZipOnly);
                if (newDeployZipOnly != settings.DeployZipOnly)
                {
                    settings.DeployZipOnly = newDeployZipOnly;
                    config.DeploySettings = settings;
                }

                EditorGUILayout.Space(3);

                if (!settings.IsValid())
                {
                    EditorGUILayout.HelpBox("❌ Deploy settings are incomplete. Please check server host, username and authentication settings.", MessageType.Error);
                }
                else
                {
                    EditorGUILayout.HelpBox("✅ Deploy settings are valid", MessageType.Info);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private BuildTarget GetBuildTargetFromProfile(UnityEditor.Build.Profile.BuildProfile profile)
        {
            try
            {
                var serializedObject = new SerializedObject(profile);
                var buildTargetProperty = serializedObject.FindProperty("m_BuildTarget");
                if (buildTargetProperty != null)
                {
                    var buildTargetValue = buildTargetProperty.intValue;
                    if (Enum.IsDefined(typeof(BuildTarget), buildTargetValue))
                    {
                        return (BuildTarget)buildTargetValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get BuildTarget from profile directly: {ex.Message}");
            }

            var profileName = profile.name.ToLower();

            if (profileName.Contains("windows") || profileName.Contains("win"))
                return BuildTarget.StandaloneWindows64;
            if (profileName.Contains("mac") || profileName.Contains("osx"))
                return BuildTarget.StandaloneOSX;
            if (profileName.Contains("linux"))
                return BuildTarget.StandaloneLinux64;
            if (profileName.Contains("android"))
                return BuildTarget.Android;
            if (profileName.Contains("ios"))
                return BuildTarget.iOS;
            if (profileName.Contains("webgl") || profileName.Contains("web") || profileName.Contains("dev"))
                return BuildTarget.WebGL;

            return BuildTarget.StandaloneWindows64;
        }
    }
}
