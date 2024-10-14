#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static void BuildProject(BuildConfig config, bool cleanBuild = false)
        {
            CheckBuildDirectory(config.buildPath, cleanBuild);

            // Create log file for build process
            logFilePath = Path.Combine(config.buildPath, "BuildLog.txt");
            logWriter = new StreamWriter(logFilePath, append: false);
            logWriter.WriteLine($"Build started at {DateTime.Now}");

            try
            {
                if (!config.ValidateConfig())
                {
                    logWriter.WriteLine("Config validation failed.");
                    throw new Exception("Config validation failed.");
                }

                logWriter.WriteLine($"Building project to {config.buildPath}");
                ApplicationConfig.SelectedIPType = config.ipType;
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
                    logWriter.WriteLine("Enabled development build.");
                }

                if (config.buildTargetGroup == BuildTargetGroup.WebGL && config.buildAdditionalMobileData)
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;

                logWriter.WriteLine("Starting build...");
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

                if (report.summary.result == BuildResult.Succeeded)
                {
                    logWriter.WriteLine("Build succeeded.");
                }
                else
                {
                    logWriter.WriteLine($"Build failed with result: {report.summary.result}");
                    throw new Exception("Build failed.");
                }

                // Handle WebGL additional mobile data build if necessary
                if (config.buildTargetGroup == BuildTargetGroup.WebGL && config.buildAdditionalMobileData)
                {
                    BuildMobileWebGLData(config);
                }
            }
            catch (Exception e)
            {
                logWriter.WriteLine($"Build error: {e.Message}");
                Debug.LogError(e);
            }
            finally
            {
                logWriter.WriteLine($"Build completed at {DateTime.Now}");
                logWriter.Close();

                // Open log file
                Process.Start(logFilePath);
            }
        }

        private static void CheckBuildDirectory(string buildPath, bool cleanBuild)
        {
            if (Directory.Exists(buildPath) & cleanBuild)
            {
                logWriter.WriteLine($"Cleaning build directory: {buildPath}");
                Directory.Delete(buildPath, true);
            }
            Directory.CreateDirectory(buildPath);
        }

        private static List<string> CopyScenesToBuildFolder(BuildConfig config)
        {
            logWriter.WriteLine("Copying scenes to build folder...");
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
                    logWriter.WriteLine($"Copied scene: {scenePath}");
                }
                else
                {
                    logWriter.WriteLine($"Scene {scenePath} not found.");
                    throw new Exception($"Scene {scenePath} not found.");
                }
            }

            return buildScenes;
        }

        private static void BuildMobileWebGLData(BuildConfig config)
        {
            string mobileBuildPath = $"{config.buildPath}_Mobile";
            logWriter.WriteLine($"Building additional mobile data for WebGL at {mobileBuildPath}...");

            // Set texture compression to ETC2 for mobile
            //PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            //PlayerSettings.WebGL.useEmbeddedResources = true;

            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

            BuildPlayerOptions mobileBuildOptions = new BuildPlayerOptions
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
                logWriter.WriteLine("Mobile WebGL build succeeded.");

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
                    logWriter.WriteLine("Copied mobile data file to main build folder.");
                }
                else
                {
                    logWriter.WriteLine("Mobile data file not found.");
                    throw new Exception("Mobile data file not found.");
                }
            }
            else
            {
                logWriter.WriteLine("Mobile WebGL build failed.");
                throw new Exception("Mobile WebGL build failed.");
            }
        }

        private static void ConfigureScriptingBackend(BuildConfig config)
        {
            logWriter.WriteLine("Configuring scripting backend...");
            var group = config.buildTargetGroup;
            var implementation = config.scriptingBackend;

            if (group == BuildTargetGroup.WebGL || group == BuildTargetGroup.iOS)
                implementation = ScriptingImplementation.IL2CPP;

            PlayerSettings.SetScriptingBackend(group, implementation);
            PlayerSettings.SetApiCompatibilityLevel(group, config.compatibilityLevel);

            // For IL2CPP configurations
            if (implementation == ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(group, config.il2CppCompilerConfiguration);
                PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.FromBuildTargetGroup(group), config.il2CppCodeGeneration);
                logWriter.WriteLine("Configured IL2CPP settings.");
            }
        }

        private static void ConfigureOptimization(BuildConfig config)
        {
            logWriter.WriteLine("Configuring optimization settings...");
            BuildTargetGroup group = config.buildTargetGroup;

            PlayerSettings.dedicatedServerOptimizations = config.dedicatedServerOptimizations;
            PlayerSettings.bakeCollisionMeshes = config.prebakeCollisionMeshes;
            PlayerSettings.SetManagedStrippingLevel(group, config.managedStrippingLevel);
            PlayerSettings.stripUnusedMeshComponents = config.optimizeMeshData;
            PlayerSettings.mipStripping = config.textureMipMapStriping;
        }

        private static void ConfigureAndroidPublishing(BuildConfig config)
        {
            logWriter.WriteLine("Configuring Android publishing settings...");

            PlayerSettings.Android.useCustomKeystore = config.useCustomKeystore;
            PlayerSettings.Android.minifyRelease = config.minifyType == MinifyType.Release;
            PlayerSettings.Android.minifyDebug = config.minifyType == MinifyType.Debug;

            if (config.useCustomKeystore)
            {
                if (!File.Exists(config.keystoreSettingsFile))
                {
                    logWriter.WriteLine("Keystore settings file not found. Using default keystore.");
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

                logWriter.WriteLine("Custom keystore settings applied.");
            }
        }

        private static void ConfigureWebGLPublishing(BuildConfig config)
        {
            logWriter.WriteLine("Configuring WebGL publishing settings...");

            PlayerSettings.WebGL.exceptionSupport = config.webGLExceptionSupport;
            PlayerSettings.WebGL.dataCaching = config.dataCaching;
            PlayerSettings.WebGL.compressionFormat = config.webGLcompressionFormat;
            PlayerSettings.WebGL.debugSymbolMode = config.webGLdebugSymbolsMode;

            if (config.buildAdditionalMobileData)
            {
                // Additional settings for mobile data build
                logWriter.WriteLine("Configuring settings for additional mobile data build.");
            }
        }
    }
}
#endif
