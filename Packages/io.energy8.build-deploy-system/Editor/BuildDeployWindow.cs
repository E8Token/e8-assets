using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public class BuildDeployWindow : EditorWindow
    {
        private List<BuildConfiguration> configurations = new List<BuildConfiguration>();
        private Vector2 scrollPosition;
        private bool isBuilding = false;
        private string currentBuildConfig = "";
        private BuildConfiguration selectedConfiguration;
        private Dictionary<string, bool> configurationFoldouts = new Dictionary<string, bool>();

        [MenuItem("Energy8/Build Deploy System")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildDeployWindow>("Build Deploy System");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            RefreshConfigurations();
            BuildManager.OnBuildStarted += OnBuildStarted;
            BuildManager.OnBuildCompleted += OnBuildCompleted;
            BuildProfileScanner.OnConfigurationsUpdated += OnConfigurationsUpdated;
        }

        private void OnDisable()
        {
            BuildManager.OnBuildStarted -= OnBuildStarted;
            BuildManager.OnBuildCompleted -= OnBuildCompleted;
            BuildProfileScanner.OnConfigurationsUpdated -= OnConfigurationsUpdated;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10); DrawHeader();
            EditorGUILayout.Space(10);

            DrawGlobalVersionSection();
            EditorGUILayout.Space(10);

            DrawRefreshSection();
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

            var infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Italic
            };

            EditorGUILayout.LabelField("Автоматически синхронизируется с Build Profiles", infoStyle);
        }

        private void DrawGlobalVersionSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📋 Project Version Management", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Version:", GUILayout.Width(100));

            // Редактируемое поле версии
            string currentVersion = "1.0.1"; try
            {
                var globalVersion = Energy8.BuildDeploySystem.GlobalVersion.Instance;
                if (globalVersion != null)
                {
                    currentVersion = globalVersion.Version;
                }
            }
            catch (System.Exception ex)
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
                catch (System.Exception ex)
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
                catch (System.Exception ex)
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
                catch (System.Exception ex)
                {
                    Debug.LogError($"[BuildDeployWindow] Failed to increment major version: {ex.Message}");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Версия проекта управляется глобально для всех конфигураций. Каждая успешная сборка автоматически инкрементирует build номер.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }

        private void DrawRefreshSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Управление", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔄 Обновить конфигурации", GUILayout.Height(25)))
            {
                RefreshConfigurations();
            }

            if (GUILayout.Button("📁 Открыть папку конфигураций", GUILayout.Height(25)))
            {
                EditorUtility.RevealInFinder(Path.GetFullPath("Assets/BuildSystem/Configs"));
            }

            EditorGUILayout.EndHorizontal();

            // Показываем статистику
            EditorGUILayout.LabelField($"Найдено конфигураций: {configurations.Count}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawConfigurationsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Конфигурации Build Profiles", EditorStyles.boldLabel);

            if (configurations.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Конфигурации не найдены.\n" +
                    "Убедитесь что у вас есть Build Profiles в папке Assets/Settings/Build Profiles\n" +
                    "Нажмите 'Обновить конфигурации' для создания.",
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

            // Header with foldout
            EditorGUILayout.BeginHorizontal();

            // Selection radio button
            if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
            {
                selectedConfiguration = config;
            }

            // Foldout for configuration details
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

            // Status indicator
            if (config.IsValid())
            {
                GUILayout.Label("✅", GUILayout.Width(20));
            }
            else
            {
                GUILayout.Label("⚠️", GUILayout.Width(20));
            }

            EditorGUILayout.EndHorizontal();

            // Configuration details (shown when expanded)
            if (configurationFoldouts[configKey])
            {
                EditorGUI.indentLevel++;

                // Build Profile reference (read-only)
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Build Profile", config.BuildProfile, typeof(UnityEditor.Build.Profile.BuildProfile), false);
                EditorGUI.EndDisabledGroup();

                // Custom build name
                var newCustomName = EditorGUILayout.TextField("Custom Name", config.CustomBuildName);
                if (newCustomName != config.CustomBuildName)
                {
                    config.CustomBuildName = newCustomName;
                }                // Output path
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
                EditorGUILayout.EndHorizontal();                // Подсказка о плейсхолдерах
                EditorGUILayout.LabelField("💡 Available placeholders: {{{VERSION}}}, {{{PLATFORM}}}, {{{DATE}}}, {{{DATETIME}}}", EditorStyles.miniLabel);

                // Пример использования
                if (string.IsNullOrEmpty(config.OutputPath))
                {
                    EditorGUILayout.LabelField("📋 Example: Builds/{{{PLATFORM}}}/{{{VERSION}}}", EditorStyles.miniLabel);
                }// Показываем обработанный путь если есть плейсхолдеры
                if (!string.IsNullOrEmpty(config.OutputPath) && (config.OutputPath.Contains("{{{") && config.OutputPath.Contains("}}}")))
                {
                    var processedPath = ProcessOutputPathTemplate(config.OutputPath, config);
                    EditorGUILayout.LabelField($"➤ Resolved: {processedPath}", EditorStyles.miniLabel);
                }

                // Build Options
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("🔧 Build Options", EditorStyles.boldLabel);
                var newCleanBefore = EditorGUILayout.Toggle("Clean Before Build", config.CleanBeforeBuild);
                if (newCleanBefore != config.CleanBeforeBuild)
                {
                    config.CleanBeforeBuild = newCleanBefore;
                }

                // Platform-specific settings
                DrawPlatformSpecificSettings(config);

                // Deploy settings
                DrawDeploySettings(config);

                // Validation status
                if (!config.IsValid())
                {
                    if (config.BuildProfile == null)
                    {
                        EditorGUILayout.HelpBox("❌ Build Profile не найден или был удален", MessageType.Error);
                    }
                    else if (string.IsNullOrEmpty(config.OutputPath))
                    {
                        EditorGUILayout.HelpBox("❌ Не указан путь для вывода", MessageType.Error);
                    }
                }
                else if (isSelected)
                {
                    EditorGUILayout.HelpBox("✅ Конфигурация готова к сборке", MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Действия сборки", EditorStyles.boldLabel); if (selectedConfiguration != null)
            {
                EditorGUILayout.LabelField($"Выбрано: {selectedConfiguration.GetDisplayName()}", EditorStyles.helpBox);

                if (isBuilding)
                {
                    EditorGUILayout.HelpBox($"🔨 Выполняется сборка: {currentBuildConfig}...", MessageType.Info);
                }

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = !isBuilding && selectedConfiguration.IsValid();

                // Clean Build кнопка доступна только если не включен Clean Before Build
                if (!selectedConfiguration.CleanBeforeBuild)
                {
                    if (GUILayout.Button("🧹 Clean Build", GUILayout.Height(35)))
                    {
                        BuildManager.CleanBuild(selectedConfiguration);
                    }
                }

                if (GUILayout.Button("🔨 Build", GUILayout.Height(35)))
                {
                    BuildManager.BuildConfiguration(selectedConfiguration);
                }

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                // Deploy buttons
                EditorGUILayout.Space(5);
                DrawDeployButtons();
            }
            else
            {
                EditorGUILayout.HelpBox("Выберите конфигурацию для сборки", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDeployButtons()
        {
            if (selectedConfiguration == null) return;

            var deploySettings = selectedConfiguration.DeploySettings;

            if (deploySettings.AlwaysDeploy)
            {
                EditorGUILayout.HelpBox("✅ Auto Deploy включен - каждая успешная сборка будет автоматически задеплоена", MessageType.Info);
            }
            else if (deploySettings.EnableDeploy)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.enabled = !isBuilding;

                if (GUILayout.Button("🚀 Deploy Only", GUILayout.Height(30)))
                {
                    DeploySelectedConfiguration();
                }

                if (GUILayout.Button("🔨🚀 Build & Deploy", GUILayout.Height(30)))
                {
                    // Временно включаем AlwaysDeploy для этой сборки
                    bool originalAlwaysDeploy = deploySettings.AlwaysDeploy;
                    deploySettings.AlwaysDeploy = true;

                    BuildManager.BuildConfiguration(selectedConfiguration);

                    // Возвращаем настройку обратно
                    deploySettings.AlwaysDeploy = originalAlwaysDeploy;
                }

                GUI.enabled = true;
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

            // Проверяем что выбранная конфигурация еще существует
            if (selectedConfiguration != null && !configurations.Contains(selectedConfiguration))
            {
                selectedConfiguration = null;
            }

            Repaint();
        }

        private void OnBuildStarted(string configName)
        {
            isBuilding = true;
            currentBuildConfig = configName;
            Repaint();
        }
        private void OnBuildCompleted(string configName, bool success)
        {
            isBuilding = false;
            currentBuildConfig = "";
            Repaint();

            // Только логи, никаких диалогов
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
        private void DrawPlatformSpecificSettings(BuildConfiguration config)
        {
            if (config.BuildProfile == null) return;

            // Используем тот же метод определения платформы, что и в BuildManager
            var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Platform Settings", EditorStyles.boldLabel);

            // Debug info для проверки
            EditorGUILayout.LabelField($"Profile: {config.BuildProfileName}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Detected Platform: {buildTarget}", EditorStyles.miniLabel);

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
            else if (buildTarget == BuildTarget.StandaloneWindows64 || buildTarget == BuildTarget.StandaloneOSX || buildTarget == BuildTarget.StandaloneLinux64)
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

            EditorGUILayout.HelpBox(
                "Configure texture formats for WebGL build. " +
                "The system will create separate builds for each selected format and merge data files.",
                MessageType.Info
            );

            EditorGUILayout.LabelField("Texture Formats to Build", EditorStyles.boldLabel);

            // Показываем все форматы для выбора
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
                    "✨ Multi-Format Build Enabled!\n" +
                    $"Selected formats: {string.Join(", ", additionalFormats)}\n" +
                    $"Compression: {string.Join(", ", compressionNames)}\n" +
                    "Data files will be named: [appname].[format].[compression].data",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Select one or more texture formats and compression algorithms above to enable multi-format build.",
                    MessageType.Warning
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

                if (settings.AlwaysDeploy)
                {
                    EditorGUILayout.HelpBox("✅ Always Deploy включен - каждая успешная сборка будет автоматически задеплоена", MessageType.Info);
                }

                EditorGUILayout.Space(5);

                // Deploy Method
                var newDeployMethod = (DeployMethod)EditorGUILayout.EnumPopup("Deploy Method", settings.Method);
                if (newDeployMethod != settings.Method)
                {
                    settings.Method = newDeployMethod;
                    config.DeploySettings = settings;
                }

                // LocalCopy: только путь и файловый браузер
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
                else
                {
                    // Server Settings
                    EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);

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

                    // Authentication
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

                // Deploy Options
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Deploy Options", EditorStyles.boldLabel);

                var newDeleteExisting = EditorGUILayout.Toggle("Delete Existing Files", settings.DeleteExistingFiles);
                if (newDeleteExisting != settings.DeleteExistingFiles)
                {
                    settings.DeleteExistingFiles = newDeleteExisting;
                    config.DeploySettings = settings;
                }

                var newCreateBackup = EditorGUILayout.Toggle("Create Backup", settings.CreateBackup);
                if (newCreateBackup != settings.CreateBackup)
                {
                    settings.CreateBackup = newCreateBackup;
                    config.DeploySettings = settings;
                }

                var newDeployZipOnly = EditorGUILayout.Toggle("Deploy ZIP Only", settings.DeployZipOnly);
                if (newDeployZipOnly != settings.DeployZipOnly)
                {
                    settings.DeployZipOnly = newDeployZipOnly;
                    config.DeploySettings = settings;
                }


                // Validation
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
        private async void DeploySelectedConfiguration()
        {
            if (selectedConfiguration == null || !selectedConfiguration.DeploySettings.EnableDeploy)
            {
                Debug.LogWarning("Deploy is not enabled for selected configuration.");
                return;
            }            // Обрабатываем шаблоны в OutputPath
            string processedOutputPath = ProcessOutputPathTemplate(selectedConfiguration.OutputPath, selectedConfiguration);
            string buildPath = Path.GetFullPath(processedOutputPath);
            if (!Directory.Exists(buildPath))
            {
                EditorUtility.DisplayDialog("Deploy Error",
                    "Build directory not found. Please build the project first.", "OK");
                return;
            }
            try
            {
                Debug.Log("Starting manual deployment...");

                // Открываем окно мониторинга деплоя
                var monitorWindow = DeployMonitorWindow.GetInstance();
                monitorWindow.StartDeployMonitoring($"{selectedConfiguration.DeploySettings.Username}@{selectedConfiguration.DeploySettings.ServerHost}:{selectedConfiguration.DeploySettings.RemotePath}");

                // Запускаем деплой с мониторингом
                bool success = await DeployManager.DeployBuild(selectedConfiguration, buildPath, monitorWindow);

                // Окно мониторинга само покажет результат
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Deploy error: {ex.Message}");

                // В случае исключения всё равно покажем окно с ошибкой
                var monitorWindow = DeployMonitorWindow.GetInstance();
                monitorWindow.AddLog($"❌ Deployment failed with error: {ex.Message}", LogType.Error);
                monitorWindow.CompleteDeployment(false);
            }
        }        /// <summary>
                 /// Заменяет плейсхолдеры в OutputPath на актуальные значения
                 /// </summary>
                 /// <param name="outputPath">Исходный путь сборки</param>
                 /// <param name="config">Конфигурация сборки для получения информации о платформе</param>
                 /// <returns>Путь с замененными плейсхолдерами</returns>
        private string ProcessOutputPathTemplate(string outputPath, BuildConfiguration config = null)
        {
            if (string.IsNullOrEmpty(outputPath))
                return outputPath;

            var processedPath = outputPath;
            // Заменяем {{{VERSION}}} на текущую версию проекта
            if (processedPath.Contains("{{{VERSION}}}"))
            {
                try
                {
                    var globalVersion = GlobalVersion.Instance;
                    if (globalVersion != null)
                    {
                        processedPath = processedPath.Replace("{{{VERSION}}}", globalVersion.Version);
                    }
                    else
                    {
                        processedPath = processedPath.Replace("{{{VERSION}}}", "unknown");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[BuildDeployWindow] Failed to get version for placeholder: {ex.Message}");
                    processedPath = processedPath.Replace("{{{VERSION}}}", "error");
                }
            }

            // Заменяем {{{PLATFORM}}} на название платформы
            if (processedPath.Contains("{{{PLATFORM}}}") && config != null)
            {
                var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);
                var platformName = GetPlatformName(buildTarget);
                processedPath = processedPath.Replace("{{{PLATFORM}}}", platformName);
            }

            // Заменяем {{{DATE}}} на текущую дату в формате yyyy-MM-dd
            if (processedPath.Contains("{{{DATE}}}"))
            {
                var dateString = DateTime.Now.ToString("yyyy-MM-dd");
                processedPath = processedPath.Replace("{{{DATE}}}", dateString);
            }

            // Заменяем {{{DATETIME}}} на текущую дату и время в формате yyyy-MM-dd_HH-mm
            if (processedPath.Contains("{{{DATETIME}}}"))
            {
                var dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                processedPath = processedPath.Replace("{{{DATETIME}}}", dateTimeString);
            }

            return processedPath;
        }

        /// <summary>
        /// Получает дружественное название платформы для использования в путях
        /// </summary>
        private string GetPlatformName(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "macOS";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                default:
                    return buildTarget.ToString();
            }
        }
        private BuildTarget GetBuildTargetFromProfile(UnityEditor.Build.Profile.BuildProfile profile)
        {
            // Пытаемся получить BuildTarget из профиля напрямую
            try
            {
                // В Unity 2022.3+ Build Profiles могут иметь свойство для получения платформы
                var serializedObject = new SerializedObject(profile);
                var buildTargetProperty = serializedObject.FindProperty("m_BuildTarget");
                if (buildTargetProperty != null)
                {
                    var buildTargetValue = buildTargetProperty.intValue;
                    if (System.Enum.IsDefined(typeof(BuildTarget), buildTargetValue))
                    {
                        return (BuildTarget)buildTargetValue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to get BuildTarget from profile directly: {ex.Message}");
            }

            // Fallback: определяем BuildTarget на основе имени профиля
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

            // По умолчанию
            return BuildTarget.StandaloneWindows64;
        }
    }
}
