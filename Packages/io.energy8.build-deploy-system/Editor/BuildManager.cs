using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;
using Newtonsoft.Json;

namespace Energy8.BuildDeploySystem.Editor
{
    /// <summary>
    /// Manages Unity build operations including configuration processing, platform-specific builds, and deployment integration.
    /// </summary>
    public static class BuildManager
    {
        /// <summary>
        /// Event triggered when a build operation starts.
        /// </summary>
        public static event Action<string> OnBuildStarted;
        
        /// <summary>
        /// Event triggered when a build operation completes.
        /// </summary>
        public static event Action<string, bool> OnBuildCompleted;
        
        /// <summary>
        /// Event triggered when a clean operation starts.
        /// </summary>
        public static event Action<string> OnCleanStarted;
        
        /// <summary>
        /// Event triggered when a clean operation completes.
        /// </summary>
        public static event Action<string, bool> OnCleanCompleted;

        /// <summary>
        /// Replaces placeholders in OutputPath with actual values.
        /// </summary>
        /// <param name="outputPath">Original build output path.</param>
        /// <param name="config">Build configuration for platform information.</param>
        /// <returns>Path with replaced placeholders.</returns>
        public static string ProcessOutputPathTemplate(string outputPath, BuildConfiguration config = null)
        {
            if (string.IsNullOrEmpty(outputPath))
                return outputPath;

            var processedPath = outputPath;

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
                        Debug.LogWarning("[BuildSystem] GlobalVersion not found. Cannot replace VERSION placeholder.");
                        processedPath = processedPath.Replace("{{{VERSION}}}", "unknown");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BuildSystem] Failed to get version for placeholder: {ex.Message}");
                    processedPath = processedPath.Replace("{{{VERSION}}}", "error");
                }
            }

            if (processedPath.Contains("{{{PLATFORM}}}") && config != null)
            {
                var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);
                var platformName = GetPlatformName(buildTarget);
                processedPath = processedPath.Replace("{{{PLATFORM}}}", platformName);
            }

            if (processedPath.Contains("{{{DATE}}}"))
            {
                var dateString = DateTime.Now.ToString("yyyy-MM-dd");
                processedPath = processedPath.Replace("{{{DATE}}}", dateString);
            }

            if (processedPath.Contains("{{{DATETIME}}}"))
            {
                var dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                processedPath = processedPath.Replace("{{{DATETIME}}}", dateTimeString);
            }

            return processedPath;
        }

        /// <summary>
        /// Gets a user-friendly platform name for use in output paths.
        /// </summary>
        /// <param name="buildTarget">The Unity BuildTarget to map.</param>
        /// <returns>A readable platform name string.</returns>
        public static string GetPlatformName(BuildTarget buildTarget)
        {
            return buildTarget switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => "Windows",
                BuildTarget.StandaloneOSX => "macOS",
                BuildTarget.StandaloneLinux64 => "Linux",
                BuildTarget.WebGL => "WebGL",
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "iOS",
                _ => buildTarget.ToString(),
            };
        }

        /// <summary>
        /// Builds a Unity project using the specified configuration with optional deployment and clean build options.
        /// </summary>
        /// <param name="config">The build configuration to use.</param>
        /// <param name="needDeploy">Whether to deploy the build after successful completion.</param>
        /// <param name="clean">Whether to perform a clean build by clearing the build cache.</param>
        public static void BuildConfiguration(BuildConfiguration config, bool needDeploy, bool clean)
        {
            if (config == null || !config.IsValid())
            {
                Debug.LogError("Invalid build configuration");
                return;
            }

            try
            {
                var displayName = config.GetDisplayName();
                OnBuildStarted?.Invoke(displayName);
                Debug.Log($"[BuildSystem] Starting build for configuration: {displayName}");

                string processedOutputPath = ProcessOutputPathTemplate(config.OutputPath, config);
                string fullOutputPath = Path.GetFullPath(processedOutputPath);

                if (clean)
                {
                    Directory.Delete(fullOutputPath, true);
                }
                if (!Directory.Exists(fullOutputPath))
                {
                    Directory.CreateDirectory(fullOutputPath);
                }

                var buildProfile = config.BuildProfile;
                if (buildProfile == null)
                {
                    throw new Exception("Build Profile not found");
                }

                string executablePath = GetExecutablePath(config, fullOutputPath);
                var buildTarget = GetBuildTargetFromProfile(buildProfile);
                bool success = false;

                SetBundleIdForMobilePlatforms(config, buildTarget);

                if (buildTarget == BuildTarget.WebGL)
                {
                    if (HasMultipleWebGLFormats(config))
                        success = BuildWebGLWithMultipleTextureFormats(config, buildProfile, executablePath, clean);
                    else
                        success = BuildWithProfile(buildProfile, executablePath, clean);

                    GenerateDataTypesJson(fullOutputPath);

                    if (config.WebGLSettings.CompressionAlgorithms != null &&
                        config.WebGLSettings.CompressionAlgorithms.Count > 0)
                        CompressWebGLDataFiles(fullOutputPath, config);
                }
                else
                {
                    success = BuildWithProfile(buildProfile, executablePath, clean);
                }

                if (success)
                {
                    try
                    {
                        GlobalVersion.Instance.IncrementBuild();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[BuildSystem] Failed to increment build version: {ex.Message}");
                    }
                }

                if (success && needDeploy)
                {
                    _ = DeployBuildAsync(config, fullOutputPath);
                }

                OnBuildCompleted?.Invoke(displayName, success);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Build failed with exception: {ex.Message}");
                OnBuildCompleted?.Invoke(config.GetDisplayName(), false);
            }
        }

        /// <summary>
        /// Builds the Unity project using the specified build profile.
        /// </summary>
        /// <param name="profile">The build profile to use for the build.</param>
        /// <param name="outputPath">The output path for the build.</param>
        /// <param name="clean">Whether to perform a clean build.</param>
        /// <returns>True if the build succeeded, false otherwise.</returns>
        private static bool BuildWithProfile(BuildProfile profile, string outputPath, bool clean)
        {
            try
            {
                try
                {
                    SetActiveBuildProfile(profile);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildSystem] Failed to set active build profile: {profile?.name}. {ex.Message}");
                    return false;
                }

                var buildTarget = GetBuildTargetFromProfile(profile);
                var scenes = GetScenesToBuild();

                BuildPlayerOptions buildOptions = new()
                {
                    locationPathName = outputPath,
                    target = buildTarget,
                    scenes = scenes,
                    options = clean ? BuildOptions.CleanBuildCache : BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(buildOptions);
                return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Build with profile failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets the bundle identifier for mobile platforms (Android and iOS) based on the global version.
        /// </summary>
        /// <param name="config">The build configuration.</param>
        /// <param name="buildTarget">The target platform for the build.</param>
        private static void SetBundleIdForMobilePlatforms(BuildConfiguration config, BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.Android || buildTarget == BuildTarget.iOS)
            {
                try
                {
                    string bundleId = GlobalVersion.Instance.GenerateBundleId();
                    string basePackageName = PlayerSettings.applicationIdentifier;

                    if (basePackageName.Contains("."))
                    {
                        var parts = basePackageName.Split('.');
                        if (parts.Length >= 2)
                        {
                            string baseName = string.Join(".", parts.Take(parts.Length - 1));
                            string newPackageName = $"{baseName}.{bundleId}";
                            var targetGroup = buildTarget == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
                            PlayerSettings.applicationIdentifier = newPackageName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildSystem] Failed to set Bundle ID: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Extracts the BuildTarget from a BuildProfile, with fallback to name-based inference.
        /// </summary>
        /// <param name="profile">The build profile to analyze.</param>
        /// <returns>The corresponding BuildTarget for the profile.</returns>
        private static BuildTarget GetBuildTargetFromProfile(BuildProfile profile)
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
                Debug.LogWarning($"[BuildSystem] Failed to get BuildTarget from profile directly: {ex.Message}");
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
            if (profileName.Contains("webgl") || profileName.Contains("web"))
                return BuildTarget.WebGL;

            return BuildTarget.StandaloneWindows64;
        }

        /// <summary>
        /// Sets the specified build profile as the active profile in Unity.
        /// </summary>
        /// <param name="profile">The build profile to activate.</param>
        /// <exception cref="Exception">Thrown when the profile cannot be activated.</exception>
        private static void SetActiveBuildProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[BuildSystem] BuildProfile is null.");
                throw new Exception("BuildProfile is null");
            }

            BuildProfile.SetActiveBuildProfile(profile);

            if (BuildProfile.GetActiveBuildProfile() != profile)
            {
                Debug.LogError($"[BuildSystem] Failed to switch Build Profile to '{profile.name}'.");
                throw new Exception($"Failed to switch Build Profile to '{profile.name}'");
            }
        }

        /// <summary>
        /// Generates the executable path based on the build configuration and target platform.
        /// </summary>
        /// <param name="config">The build configuration.</param>
        /// <param name="outputDirectory">The output directory for the build.</param>
        /// <returns>The full path to the executable or build output.</returns>
        private static string GetExecutablePath(BuildConfiguration config, string outputDirectory)
        {
            var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);
            var appName = PlayerSettings.productName;

            return buildTarget switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => Path.Combine(outputDirectory, $"{appName}.exe"),
                BuildTarget.StandaloneOSX => Path.Combine(outputDirectory, $"{appName}.app"),
                BuildTarget.StandaloneLinux64 => Path.Combine(outputDirectory, appName),
                BuildTarget.WebGL => outputDirectory,
                BuildTarget.Android => Path.Combine(outputDirectory, $"{appName}.apk"),
                BuildTarget.iOS => outputDirectory,
                _ => Path.Combine(outputDirectory, appName),
            };
        }

        /// <summary>
        /// Gets the list of scenes to include in the build from Unity's build settings.
        /// </summary>
        /// <returns>An array of scene paths to build.</returns>
        private static string[] GetScenesToBuild()
        {
            var scenes = EditorBuildSettings.scenes;
            var scenePaths = new string[scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            return scenePaths;
        }
        /// <summary>
        /// Builds WebGL with multiple texture formats, creating separate builds for each format.
        /// </summary>
        /// <param name="config">The build configuration containing WebGL settings.</param>
        /// <param name="profile">The build profile to use.</param>
        /// <param name="outputPath">The base output path for builds.</param>
        /// <param name="clean">Whether to perform clean builds.</param>
        /// <returns>True if all format builds succeeded, false otherwise.</returns>
        private static bool BuildWebGLWithMultipleTextureFormats(BuildConfiguration config, BuildProfile profile, string outputPath, bool clean)
        {
            try
            {
                var formats = GetWebGLTextureFormats(config);

                if (formats.Length == 0)
                {
                    Debug.LogWarning("[BuildSystem] No texture formats selected for WebGL build");
                    return BuildWithProfile(profile, outputPath, clean);
                }

                bool anyBuildSuccess = false;

                for (int i = 0; i < formats.Length; i++)
                {
                    string format = formats[i];
                    Debug.Log($"[BuildSystem] Building format {i + 1}/{formats.Length}: {format}");

                    string buildPath = (i == 0) ? outputPath : outputPath + "_" + format;

                    try
                    {
                        SetWebGLTextureFormat(format);
                        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                        bool buildSuccess = BuildWithProfile(profile, buildPath, clean);
                        if (!buildSuccess)
                        {
                            Debug.LogError($"[BuildSystem] Build with {format} format failed!");
                            if (i > 0 && Directory.Exists(buildPath))
                            {
                                try
                                {
                                    Directory.Delete(buildPath, true);
                                }
                                catch (Exception cleanupEx)
                                {
                                    Debug.LogWarning($"[BuildSystem] Failed to cleanup temp directory: {cleanupEx.Message}");
                                }
                            }
                            return false;
                        }

                        if (Directory.Exists(buildPath))
                        {
                            var files = Directory.GetFiles(buildPath);

                            string buildSubPath = Path.Combine(buildPath, "Build");
                            if (!Directory.Exists(buildSubPath))
                            {
                                Debug.LogWarning($"[BuildSystem] Build subdirectory does not exist: {buildSubPath}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[BuildSystem] Build directory {buildPath} does not exist!");
                        }
                        if (i == 0)
                        {
                            RenameMainWebGLDataFile(buildPath, format, config);
                            anyBuildSuccess = true;
                        }
                        else
                        {
                            CopyWebGLDataFile(buildPath, outputPath, format);

                            if (Directory.Exists(buildPath))
                            {
                                Directory.Delete(buildPath, true);
                            }
                        }

                        Debug.Log($"[BuildSystem] Successfully built {format} format");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[BuildSystem] Error building {format} format: {ex.Message}");

                        if (i > 0 && Directory.Exists(buildPath))
                        {
                            try
                            {
                                Directory.Delete(buildPath, true);
                            }
                            catch (Exception cleanupEx)
                            {
                                Debug.LogWarning($"[BuildSystem] Failed to cleanup temp directory: {cleanupEx.Message}");
                            }
                        }
                        return false;
                    }
                }
                if (anyBuildSuccess)
                {
                    Debug.Log("[BuildSystem] WebGL multi-format build completed successfully");
                    return true;
                }
                else
                {
                    Debug.LogError("[BuildSystem] All WebGL format builds failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] WebGL multi-format build failed: {ex.Message}");
                return false;
            }
        }
        private static string[] GetWebGLTextureFormats(BuildConfiguration config)
        {
            var formats = new List<string>();

            if (config.WebGLSettings.BuildDXT) formats.Add("DXT");
            if (config.WebGLSettings.BuildASTC) formats.Add("ASTC");
            if (config.WebGLSettings.BuildETC2) formats.Add("ETC2");

            return formats.ToArray();
        }
        private static bool HasMultipleWebGLFormats(BuildConfiguration config)
        {
            int count = 0;

            if (config.WebGLSettings.BuildDXT) count++;
            if (config.WebGLSettings.BuildASTC) count++;
            if (config.WebGLSettings.BuildETC2) count++;

            return count > 0;
        }

        private static void SetWebGLTextureFormat(string format)
        {
            try
            {
                EditorUserBuildSettings.webGLBuildSubtarget = format.ToUpper() switch
                {
                    "DXT" => WebGLTextureSubtarget.DXT,
                    "ASTC" => WebGLTextureSubtarget.ASTC,
                    "ETC2" => WebGLTextureSubtarget.ETC2,
                    _ => WebGLTextureSubtarget.DXT,
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to set WebGL texture format to {format}: {ex.Message}");
            }
        }
        private static void CopyWebGLDataFile(string sourceBuildPath, string targetBuildPath, string format)
        {
            try
            {
                var appName = PlayerSettings.productName;
                string buildSubPath = Path.Combine(sourceBuildPath, "Build");
                string sourceDataFile = Path.Combine(buildSubPath, $"{appName}.data");
                {
                    var dataFiles = Directory.GetFiles(buildSubPath, "*.data*");

                    if (dataFiles.Length > 0)
                    {
                        sourceDataFile = dataFiles[0];
                    }
                    else
                    {
                        Debug.LogWarning($"[BuildSystem] No .data file found in {buildSubPath}");
                        return;
                    }
                }

                string originalFileName = Path.GetFileName(sourceDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceDataFile);
                string extension = Path.GetExtension(sourceDataFile);

                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    string nameWithoutCompression = Path.GetFileNameWithoutExtension(sourceDataFile);
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutCompression);
                    string dataExt = Path.GetExtension(nameWithoutCompression);
                }
                else if (!extension.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"[BuildSystem] Unexpected file extension: {extension} for file {originalFileName}");
                }

                string baseAppName = ExtractBaseAppName(fileNameWithoutExt, appName);
                string targetFileName = $"{baseAppName}.{format.ToLower()}.data{compressionExt}";
                string targetBuildSubPath = Path.Combine(targetBuildPath, "Build");
                string targetDataFile = Path.Combine(targetBuildSubPath, targetFileName);

                if (!Directory.Exists(targetBuildSubPath))
                {
                    Directory.CreateDirectory(targetBuildSubPath);
                }

                File.Copy(sourceDataFile, targetDataFile, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to copy WebGL data file for {format}: {ex.Message}");
            }
        }
        private static void RenameMainWebGLDataFile(string buildPath, string format, BuildConfiguration config)
        {
            try
            {
                var appName = PlayerSettings.productName;
                string buildSubPath = Path.Combine(buildPath, "Build");
                string originalDataFile = Path.Combine(buildSubPath, $"{appName}.data");

                if (!File.Exists(originalDataFile))
                {
                    var dataFiles = Directory.GetFiles(buildSubPath, "*.data*");

                    if (dataFiles.Length > 0)
                    {
                        originalDataFile = dataFiles[0];
                    }
                    else
                    {
                        return;
                    }
                }

                string originalFileName = Path.GetFileName(originalDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalDataFile);
                string extension = Path.GetExtension(originalDataFile);

                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    string nameWithoutCompression = Path.GetFileNameWithoutExtension(originalDataFile);
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutCompression);
                    string dataExt = Path.GetExtension(nameWithoutCompression);
                }
                else if (!extension.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"[BuildSystem] Unexpected file extension: {extension} for file {originalFileName}");
                }

                string baseAppName = ExtractBaseAppName(fileNameWithoutExt, appName);

                string newFileName = $"{baseAppName}.{format.ToLower()}.data{compressionExt}";
                string newDataFile = Path.Combine(buildSubPath, newFileName);

                if (originalDataFile != newDataFile)
                {
                    File.Move(originalDataFile, newDataFile);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to rename main WebGL data file: {ex.Message}");
            }
        }

        private static async System.Threading.Tasks.Task DeployBuildAsync(BuildConfiguration config, string buildPath)
        {
            try
            {
                Debug.Log("[DeploySystem] Starting deployment.");
                bool deploySuccess = await DeployManager.DeployBuild(config, buildPath);

                if (deploySuccess)
                {
                    Debug.Log("[DeploySystem] Deployment completed successfully!");
                }
                else
                {
                    Debug.LogError("[DeploySystem] Deployment failed!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeploySystem] Deployment error: {ex.Message}");
            }
        }

        private static void CompressWebGLDataFiles(string outputPath, BuildConfiguration config)
        {
            try
            {
                string buildSubPath = Path.Combine(outputPath, "Build");
                if (!Directory.Exists(buildSubPath))
                {
                    Debug.LogWarning($"[BuildSystem] Build subdirectory not found: {buildSubPath}");
                    return;
                }

                var dataFiles = Directory.GetFiles(buildSubPath, "*.data", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.GetFiles(buildSubPath, "*.wasm", SearchOption.TopDirectoryOnly))
                    .Concat(Directory.GetFiles(buildSubPath, "*.framework.js", SearchOption.TopDirectoryOnly))
                    .Concat(Directory.GetFiles(buildSubPath, "*.symbols.json", SearchOption.TopDirectoryOnly))
                    .ToArray();

                var filesToDelete = new List<string>();

                foreach (var algo in config.WebGLSettings.CompressionAlgorithms)
                {
                    string compressionName = algo == CompressionAlgorithm.Gzip ? "GZIP" : "Brotli";
                    string progressTitle = $"Compressing {compressionName}";
                    
                    for (int i = 0; i < dataFiles.Length; i++)
                    {
                        var dataFile = dataFiles[i];
                        string fileName = Path.GetFileName(dataFile);
                        
                        EditorUtility.DisplayProgressBar(progressTitle, $"Compressing {fileName}...", (float)i / dataFiles.Length);
                        
                        if (algo == CompressionAlgorithm.Gzip)
                        {
                            if (RunExternalCompressor("gzip.exe", dataFile, ".gz", 9))
                                filesToDelete.Add(dataFile);
                        }
                        else if (algo == CompressionAlgorithm.Brotli)
                        {
                            if (RunExternalCompressor("brotli.exe", dataFile, ".br", 11))
                                filesToDelete.Add(dataFile);
                        }
                    }
                    
                    EditorUtility.ClearProgressBar();
                }

                // Удаляем несжатые файлы после успешной компрессии
                foreach (var fileToDelete in filesToDelete.Distinct())
                {
                    try
                    {
                        if (File.Exists(fileToDelete))
                        {
                            File.Delete(fileToDelete);
                            Debug.Log($"[BuildSystem] Deleted uncompressed file: {Path.GetFileName(fileToDelete)}");
                        }
                    }
                    catch (Exception deleteEx)
                    {
                        Debug.LogWarning($"[BuildSystem] Failed to delete uncompressed file {fileToDelete}: {deleteEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Compression error: {ex.Message}");
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool RunExternalCompressor(string exeName, string dataFile, string ext, int compressionLevel = 6)
        {
            try
            {
                // Используем exe файлы из PATH вместо локальной папки Tools
                string exePath = exeName;
                string outputFile = dataFile + ext;
                string args;
                if (exeName.ToLower().Contains("brotli"))
                    args = $"\"{dataFile}\" -o \"{outputFile}\" --keep --quality={compressionLevel}";
                else if (exeName.ToLower().Contains("gzip"))
                    args = $"-k -{compressionLevel} \"{dataFile}\"";
                else
                    args = $"\"{dataFile}\"";
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = args;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Debug.LogWarning($"[BuildSystem] {exeName} failed with exit code {process.ExitCode}: {stderr}");
                    if (process.ExitCode == -1 || stderr.Contains("not found") || stderr.Contains("not recognized"))
                    {
                        Debug.LogError($"[BuildSystem] {exeName} not found in PATH. Please ensure {exeName} is installed and available in system PATH.");
                    }
                    return false;
                }
                
                if (!string.IsNullOrEmpty(stderr))
                    Debug.LogWarning($"[BuildSystem] {exeName} warning: {stderr}");
                    
                if (!File.Exists(outputFile))
                {
                    Debug.LogWarning($"[BuildSystem] Compression failed: {outputFile} not found");
                    return false;
                }
                
                Debug.Log($"[BuildSystem] Successfully compressed: {Path.GetFileName(dataFile)} -> {Path.GetFileName(outputFile)}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to run compressor {exeName}: {ex.Message}");
                if (ex.Message.Contains("cannot find") || ex.Message.Contains("not found") || ex is System.ComponentModel.Win32Exception)
                {
                    Debug.LogError($"[BuildSystem] {exeName} not found in PATH. Please ensure {exeName} is installed and available in system PATH.");
                }
                return false;
            }
        }

        private static string ExtractBaseAppName(string fileName, string expectedAppName)
        {
            if (string.IsNullOrEmpty(fileName))
                return expectedAppName ?? "App";

            string[] textureSuffixes = { "_DXT", "_ASTC", "_ETC2", "_PVRTC" };

            foreach (string suffix in textureSuffixes)
            {
                if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = fileName.Substring(0, fileName.Length - suffix.Length);
                    return baseName;
                }
            }

            return fileName;
        }

        private static void GenerateDataTypesJson(string outputPath)
        {
            string buildSubPath = Path.Combine(outputPath, "Build");
            if (!Directory.Exists(buildSubPath))
            {
                Debug.LogWarning($"[BuildSystem] Build subdirectory not found: {buildSubPath}");
                return;
            }

            string baseName = new DirectoryInfo(outputPath).Name;

            // Словарь только для типов data файлов
            var dataTypes = new Dictionary<string, string>();

            // Добавляем только форматы текстур (data файлы)
            var formats = new[] { "dxt", "astc", "etc2" };
            foreach (var format in formats)
            {
                string dataFileName = baseName + $".{format}.data";
                if (File.Exists(Path.Combine(buildSubPath, dataFileName)))
                {
                    dataTypes[format] = dataFileName;
                }
            }

            string json = JsonConvert.SerializeObject(dataTypes, Formatting.Indented);
            File.WriteAllText(Path.Combine(buildSubPath, "data-types.json"), json);
            
            Debug.Log($"[BuildSystem] Generated data-types.json with {dataTypes.Count} data file types");
        }
    }
}
