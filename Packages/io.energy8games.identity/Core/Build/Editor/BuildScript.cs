#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Energy8.Identity.Core.Configuration;
using Energy8.Identity.Core.Configuration.Models;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Energy8
{

    public static class BuildScript
    {
        private static string logFilePath;
        private static StreamWriter logWriter;

        public static void BuildProject()
        {
            string configName = string.Empty;
            bool cleanBuild = false;
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-configName"))
                    configName = args[i + 1];
                if (args[i].StartsWith("-cleanBuild"))
                    cleanBuild = bool.Parse(args[i + 1]);
            }
            
            if(string.IsNullOrEmpty(configName))
                throw new Exception("Name of the configuration is not specified.");


            foreach (var config in BuildConfigProjectSettings.Instance.buildConfigurations)
                if (config.name == configName)
                    BuildProject(config, cleanBuild);
        }
        public static void BuildProject(BuildConfig config, bool cleanBuild = false)
        {
            InitializeLog(config);
            RedirectUnityLogs();

            CheckBuildDirectory(config.buildPath, cleanBuild);

            DateTime buildStartTime = DateTime.Now;

            IPType iPType = IdentityConfiguration.SelectedIPType;
            AuthType authType = IdentityConfiguration.SelectedAuthType;

            try
            {
                if (!config.ValidateConfig())
                {
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Config validation failed.");
                    throw new Exception("Config validation failed.");
                }

                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Building project to {config.buildPath}");

                IdentityConfiguration.SelectedIPType = config.ipType;
                IdentityConfiguration.SelectedAuthType = config.ipType switch
                {
                    IPType.LocalPC => AuthType.Local,
                    IPType.LocalNetwork => AuthType.Local,
                    IPType.Debug => AuthType.Debug,
                    IPType.DebugTLS => AuthType.Debug,
                    IPType.Production => AuthType.Production,
                    IPType.ProductionTLS => AuthType.Production,
                    _ => AuthType.Local
                };
                PlayerSettings.insecureHttpOption = config.insecureHttpOption;

                ConfigureScriptingBackend(config);
                ConfigureOptimization(config);

                if (config.buildTargetGroup == BuildTargetGroup.Android)
                    ConfigureAndroidPublishing(config);
                else if (config.buildTargetGroup == BuildTargetGroup.WebGL)
                    ConfigureWebGLPublishing(config);

                List<string> buildScenes;

                if (config.copyScenes)
                    buildScenes = CopyScenesToBuildFolder(config);
                else
                    buildScenes = config.scenesToBuild;

                BuildPlayerOptions buildPlayerOptions = new()
                {
                    scenes = buildScenes.ToArray(),
                    locationPathName = config.buildPath,
                    targetGroup = config.buildTargetGroup,
                    target = config.buildTarget,
                    options = config.compression switch
                    {
                        Compression.Lz4 => BuildOptions.CompressWithLz4,
                        Compression.Lz4HC => BuildOptions.CompressWithLz4HC,
                        _ => BuildOptions.None
                    }
                };

                // Include development build option if enabled
                if (config.enableDevelopmentBuild)
                {
                    buildPlayerOptions.options |= BuildOptions.Development;
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Enabled development build.");
                }

                if (config.buildTargetGroup == BuildTargetGroup.WebGL && config.buildAdditionalMobileData)
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;

                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Starting build...");
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

                if (report.summary.result == BuildResult.Succeeded)
                {
                    if (config.buildTargetGroup == BuildTargetGroup.WebGL && config.buildAdditionalMobileData)
                    {
                        BuildMobileWebGLData(config);
                    }
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Build succeeded.");
                    IncrementVersion();
                    Process.Start("explorer.exe ", $"{new DirectoryInfo(Application.dataPath).Parent.FullName + "\\" + config.buildPath.Replace("/", "\\")}\"");
                }
                else
                {
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Build failed with result: {report.summary.result}");
                    throw new Exception("Build failed.");
                }
            }
            catch (Exception e)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Build error: {e.Message}");
                Debug.LogError(e);
            }
            finally
            {
                RestoreUnityLogs();

                DateTime buildEndTime = DateTime.Now; // Конец таймера
                TimeSpan buildDuration = buildEndTime - buildStartTime;

                logWriter.WriteLine($"Build completed at {buildEndTime.ToLongTimeString()}");
                logWriter.WriteLine($"Build duration: {buildDuration.Hours}:{buildDuration.Minutes}:{buildDuration.Seconds}");
                logWriter.WriteLine($"===============================================");
                logWriter.Close();

                Process.Start(logFilePath);

                IdentityConfiguration.SelectedIPType = iPType;
                IdentityConfiguration.SelectedAuthType = authType;
            }
        }

        private static void InitializeLog(BuildConfig config)
        {
            // Получаем текущую версию
            string version = PlayerSettings.bundleVersion;

            // Формируем название файла журнала
            string platformName = config.buildTarget.ToString();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string logFileName = $"BuildLog_{platformName}_v{version}_{timestamp}.txt";

            // Лог будет сохранен в директорию рядом с папкой сборки
            string logsDirectory = Path.Combine(config.buildPath, "../BuildLogs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            logFilePath = Path.Combine(logsDirectory, logFileName);

            // Открываем лог в режиме добавления (если файл существует)
            logWriter = new StreamWriter(logFilePath, append: true);

            // Начало лога
            logWriter.WriteLine($"===============================================");
            logWriter.WriteLine($"Build started for {platformName} at {DateTime.Now}");
            logWriter.WriteLine($"Version: {version}");
            logWriter.WriteLine($"Build Path: {config.buildPath}");
            logWriter.WriteLine($"===============================================");
            logWriter.Flush(); // Сохраняем сразу
        }
        private static void RedirectUnityLogs()
        {
            // Подписываемся на события логов Unity
            Application.logMessageReceived += HandleUnityLog;
        }

        private static void RestoreUnityLogs()
        {
            // Отписываемся, чтобы избежать утечек
            Application.logMessageReceived -= HandleUnityLog;
        }

        private static void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}][{type}] {logString}");
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning)
                {
                    logWriter.WriteLine(stackTrace);
                }
                logWriter.Flush(); // Сохраняем изменения сразу
            }
        }

        private static void CheckBuildDirectory(string buildPath, bool cleanBuild)
        {
            if (Directory.Exists(buildPath) & cleanBuild)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Cleaning build directory: {buildPath}");
                Directory.Delete(buildPath, true);
            }
            Directory.CreateDirectory(buildPath);
        }

        private static List<string> CopyScenesToBuildFolder(BuildConfig config)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Copying scenes to build folder...");
            List<string> buildScenes = new();

            if (!Directory.Exists(config.buildScenesPath))
            {
                Directory.CreateDirectory(config.buildScenesPath);
            }

            foreach (var scene in config.scenesToBuild)
            {
                string scenePath = Path.Combine(Application.dataPath, scene);
                string destinationPath = Path.Combine(config.buildScenesPath, Path.GetFileName(scene));

                if (File.Exists(scenePath))
                {
                    File.Copy(scenePath, destinationPath, overwrite: true);
                    buildScenes.Add(scenePath);
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Copied scene: {scenePath}");
                }
                else
                {
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Scene {scenePath} not found.");
                    throw new Exception($"Scene {scenePath} not found.");
                }
            }

            return buildScenes;
        }

        private static void BuildMobileWebGLData(BuildConfig config)
        {
            string mobileBuildPath = $"{config.buildPath}_Mobile";
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Building additional mobile data for WebGL at {mobileBuildPath}...");

            // Set texture compression to ETC2 for mobile
            //PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            //PlayerSettings.WebGL.useEmbeddedResources = true;

            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

            BuildPlayerOptions mobileBuildOptions = new()
            {
                scenes = config.scenesToBuild.ToArray(),
                locationPathName = mobileBuildPath,
                targetGroup = BuildTargetGroup.WebGL,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(mobileBuildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Mobile WebGL build succeeded.");

                string dataFileName = new DirectoryInfo(mobileBuildPath).Name + ".data" + config.webGLcompressionFormat switch
                {
                    WebGLCompressionFormat.Gzip => ".gz",
                    WebGLCompressionFormat.Brotli => ".br",
                    _ => ""
                };

                // Copy data file to main build folder
                string dataFilePath = Path.Combine(mobileBuildPath, "Build", dataFileName);
                string destinationPath = Path.Combine(config.buildPath, "Build", dataFileName);
                if (File.Exists(dataFilePath))
                {
                    File.Copy(dataFilePath, destinationPath, overwrite: true);
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Copied mobile data file to main build folder.");
                }
                else
                {
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Mobile data file not found.");
                    throw new Exception("Mobile data file not found.");
                }
                Directory.Delete(mobileBuildPath, true);
            }
            else
            {
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Mobile WebGL build failed.");
                throw new Exception("Mobile WebGL build failed.");
            }
        }

        private static void ConfigureScriptingBackend(BuildConfig config)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configuring scripting backend...");
            var group = config.buildTargetGroup;
            var implementation = config.scriptingBackend;
            var namedGroup = NamedBuildTarget.FromBuildTargetGroup(group);

            if (group == BuildTargetGroup.WebGL || group == BuildTargetGroup.iOS)
                implementation = ScriptingImplementation.IL2CPP;

            PlayerSettings.SetScriptingBackend(namedGroup, implementation);
            PlayerSettings.SetApiCompatibilityLevel(namedGroup, config.compatibilityLevel);

            // For IL2CPP configurations
            if (implementation == ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(namedGroup, config.il2CppCompilerConfiguration);
                PlayerSettings.SetIl2CppCodeGeneration(namedGroup, config.il2CppCodeGeneration);
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configured IL2CPP settings.");
            }
        }

        private static void ConfigureOptimization(BuildConfig config)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configuring optimization settings...");
            BuildTargetGroup group = config.buildTargetGroup;

            PlayerSettings.dedicatedServerOptimizations = config.dedicatedServerOptimizations;
            PlayerSettings.bakeCollisionMeshes = config.prebakeCollisionMeshes;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(group), config.managedStrippingLevel);
            PlayerSettings.stripUnusedMeshComponents = config.optimizeMeshData;
            PlayerSettings.mipStripping = config.textureMipMapStriping;
        }

        private static void ConfigureAndroidPublishing(BuildConfig config)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configuring Android publishing settings...");

            PlayerSettings.Android.useCustomKeystore = config.useCustomKeystore;
            PlayerSettings.Android.minifyRelease = config.minifyType == MinifyType.Release;
            PlayerSettings.Android.minifyDebug = config.minifyType == MinifyType.Debug;

            if (config.useCustomKeystore)
            {
                if (!File.Exists(config.keystoreSettingsFile))
                {
                    logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Keystore settings file not found. Using default keystore.");
                    PlayerSettings.Android.useCustomKeystore = false;
                    return;
                }

                string json = File.ReadAllText(config.keystoreSettingsFile);
                KeystoreConfig keystoreConfig = JsonUtility.FromJson<KeystoreConfig>(json);

                // Set keystore parameters
                PlayerSettings.Android.keystoreName = keystoreConfig.keystorePath;
                PlayerSettings.Android.keystorePass = keystoreConfig.keystorePassword;
                PlayerSettings.Android.keyaliasName = keystoreConfig.keyAlias;
                PlayerSettings.Android.keyaliasPass = keystoreConfig.keyAliasPassword;

                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Custom keystore settings applied.");
            }
        }

        private static void ConfigureWebGLPublishing(BuildConfig config)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configuring WebGL publishing settings...");

            PlayerSettings.WebGL.exceptionSupport = config.webGLExceptionSupport;
            PlayerSettings.WebGL.dataCaching = config.dataCaching;
            PlayerSettings.WebGL.compressionFormat = config.webGLcompressionFormat;
            PlayerSettings.WebGL.debugSymbolMode = config.webGLdebugSymbolsMode;

            if (config.buildAdditionalMobileData)
            {
                // Additional settings for mobile data build
                logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Configuring settings for additional mobile data build.");
            }
        }

        private static void IncrementVersion()
        {
            string currentVersion = PlayerSettings.bundleVersion;

            // Увеличиваем версию
            string newVersion = IncrementPatchVersion(currentVersion);
            PlayerSettings.bundleVersion = newVersion;

            // Обновляем Bundle Version Code
            int versionCode = int.Parse(newVersion.Replace(".", "0"));
            PlayerSettings.Android.bundleVersionCode = versionCode;

            Debug.Log($"Version updated to {newVersion} (Bundle Version Code: {versionCode})");
            logWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Version updated to {newVersion} (Bundle Version Code: {versionCode})");
        }


        private static string IncrementPatchVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length != 3) parts = new[] { "0", "0", "0" };

            int.TryParse(parts[0], out int major);
            int.TryParse(parts[1], out int minor);
            int.TryParse(parts[2], out int patch);

            // Увеличиваем только третью часть версии
            patch++;

            return $"{major}.{minor}.{patch}";
        }
    }
}
#endif
