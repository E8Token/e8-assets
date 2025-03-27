#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Energy8.BuildSystem.Configuration;
using Energy8.BuildSystem.Platform;
using Energy8.BuildSystem.Utils;
using Energy8.Identity.Core.Configuration;
using Energy8.Identity.Core.Configuration.Models;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Energy8.BuildSystem.Core
{
    public static class BuildScript
    {
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

            if (string.IsNullOrEmpty(configName))
                throw new Exception("Name of the configuration is not specified.");

            foreach (var config in BuildConfigProjectSettings.Instance.buildConfigurations)
                if (config.name == configName)
                    BuildProject(config, cleanBuild);
        }

        public static void BuildProject(BuildConfig config, bool cleanBuild = false)
        {
            BuildLogger logger = new BuildLogger(config.buildPath, config.buildTarget.ToString());
            DateTime buildStartTime = DateTime.Now;

            IPType iPType = IdentityConfiguration.SelectedIPType;
            AuthType authType = IdentityConfiguration.SelectedAuthType;

            try
            {
                if (!config.ValidateConfig())
                {
                    logger.LogError("Config validation failed.");
                    throw new Exception("Config validation failed.");
                }

                logger.Log($"Building project to {config.buildPath}");

                // Prepare build directory
                CheckBuildDirectory(config.buildPath, cleanBuild, logger);

                // Configure Identity settings
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

                // Configure build settings
                ConfigureScriptingBackend(config, logger);
                ConfigureOptimization(config, logger);
                ConfigureGraphicsSettings(config, logger);

                // Platform-specific configuration
                if (config.buildTargetGroup == BuildTargetGroup.Android)
                    AndroidBuilder.ConfigureAndroidSettings(config, logger);
                else if (config.buildTargetGroup == BuildTargetGroup.WebGL)
                    WebGLBuilder.ConfigureWebGLSettings(config, logger);

                // Handle scenes
                List<string> buildScenes;
                if (config.copyScenes)
                    buildScenes = CopyScenesToBuildFolder(config, logger);
                else
                {
                    buildScenes = new List<string>();
                    foreach (var scene in config.scenesToBuild)
                    {
                        buildScenes.Add("Assets/" + scene);
                    }
                }

                // Configure build options
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
                    logger.Log("Enabled development build.");
                }

                // Set WebGL texture format
                if (config.buildTargetGroup == BuildTargetGroup.WebGL)
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;

                // Start the build
                logger.Log("Starting build...");
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

                // Process build results
                if (report.summary.result == BuildResult.Succeeded)
                {
                    if (config.buildTargetGroup == BuildTargetGroup.WebGL && config.buildAdditionalMobileData)
                    {
                        WebGLBuilder.BuildMobileData(config, logger);
                    }
                    logger.Log("Build succeeded.");
                    IncrementVersion();
                    Process.Start("explorer.exe ", $"{new DirectoryInfo(Application.dataPath).Parent.FullName + "\\" + config.buildPath.Replace("/", "\\")}\"");
                }
                else
                {
                    logger.LogError($"Build failed with result: {report.summary.result}");
                    throw new Exception("Build failed.");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Build error: {e.Message}", e);
                Debug.LogError(e);
            }
            finally
            {
                // Restore original configuration
                IdentityConfiguration.SelectedIPType = iPType;
                IdentityConfiguration.SelectedAuthType = authType;

                // Close the logger
                logger.Close(buildStartTime);

                // Open the log file
                Process.Start(logger.GetLogPath());
            }
        }

        private static void CheckBuildDirectory(string buildPath, bool cleanBuild, BuildLogger logger)
        {
            if (Directory.Exists(buildPath) && cleanBuild)
            {
                logger.Log($"Cleaning build directory: {buildPath}");
                Directory.Delete(buildPath, true);
            }
            Directory.CreateDirectory(buildPath);
        }

        private static List<string> CopyScenesToBuildFolder(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Copying scenes to build folder...");
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
                    logger.Log($"Copied scene: {scenePath}");
                }
                else
                {
                    logger.LogError($"Scene {scenePath} not found.");
                    throw new Exception($"Scene {scenePath} not found.");
                }
            }

            return buildScenes;
        }

        private static void ConfigureScriptingBackend(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Configuring scripting backend...");
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
                logger.Log("Configured IL2CPP settings.");
            }
        }

        private static void ConfigureOptimization(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Configuring optimization settings...");
            BuildTargetGroup group = config.buildTargetGroup;

            PlayerSettings.dedicatedServerOptimizations = config.dedicatedServerOptimizations;
            PlayerSettings.bakeCollisionMeshes = config.prebakeCollisionMeshes;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(group), config.managedStrippingLevel);
            PlayerSettings.stripUnusedMeshComponents = config.optimizeMeshData;
            PlayerSettings.mipStripping = config.textureMipMapStriping;
        }

        private static void ConfigureGraphicsSettings(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Configuring graphics settings...");

            // Configure batching
            PlayerSettings.SetStaticBatchingForPlatform(config.buildTarget, config.staticBatching);
            PlayerSettings.SetDynamicBatchingForPlatform(config.buildTarget, config.dynamicBatching);
            PlayerSettings.spriteBatchVertexThreshold = config.spriteBatchingThreshold;
            
            // Configure GPU skinning - update this section
            switch (config.skinningMethod)
            {
                case SkinningMethod.CPU:
                    PlayerSettings.gpuSkinning = false;
                    PlayerSettings.meshDeformation  = MeshDeformation.CPU;
                    logger.Log("Using CPU skinning");
                    break;
                case SkinningMethod.GPU:
                    PlayerSettings.gpuSkinning = true;
                    PlayerSettings.meshDeformation  = MeshDeformation.GPU;
                    logger.Log("Using GPU skinning");
                    break;
                case SkinningMethod.GPU_Batched:
                    PlayerSettings.gpuSkinning = true;
                    PlayerSettings.meshDeformation  = MeshDeformation.GPUBatched;
                    logger.Log("Using GPU skinning with batching");
                    break;
            }

            // Configure graphics jobs
            PlayerSettings.graphicsJobs = config.graphicsJobs;

            logger.Log("Graphics settings configured successfully.");
        }

        private static void IncrementVersion()
        {
            string version = PlayerSettings.bundleVersion;
            string newVersion = IncrementPatchVersion(version);
            PlayerSettings.bundleVersion = newVersion;
            Debug.Log($"Version incremented: {version} -> {newVersion}");
        }

        private static string IncrementPatchVersion(string version)
        {
            string[] parts = version.Split('.');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int patch))
            {
                parts[2] = (patch + 1).ToString();
                return string.Join(".", parts);
            }
            return version;
        }
    }
}
#endif