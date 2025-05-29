using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;
#if UNITY_2022_1_OR_NEWER
using UnityEditor.Build;
#endif

namespace Energy8.BuildDeploySystem.Editor
{
    public static class BuildSystemEditor
    {
        public static void BuildWithConfiguration(BuildConfiguration config, bool clean = false)
        {
            if (config == null)
            {
                Debug.LogError("Build configuration is null");
                return;
            }
            
            try
            {
                // Подготовка к сборке
                PrepareBuildEnvironment(config, clean);
                
                // Выполнение сборки
                var result = ExecuteBuild(config);
                
                // Обработка результата
                HandleBuildResult(config, result);
            }            catch (System.Exception ex)
            {
                Debug.LogError($"Build failed: {ex.Message}");
                BuildSystem.TriggerBuildCompleted(config.configName, false);
            }
        }
        
        private static void PrepareBuildEnvironment(BuildConfiguration config, bool clean)
        {
            Debug.Log($"Preparing build environment for {config.configName}");
            
            // Очистка папки сборки если нужно
            if (clean)
            {
                string buildPath = GetBuildPath(config);
                if (Directory.Exists(buildPath))
                {
                    Directory.Delete(buildPath, true);
                    Debug.Log($"Cleaned build directory: {buildPath}");
                }
                Directory.CreateDirectory(buildPath);
            }
            
            // Установка настроек проекта
            ApplyProjectSettings(config);
            
            // Установка настроек платформы
            ApplyPlatformSettings(config);
        }
        
        private static void ApplyProjectSettings(BuildConfiguration config)
        {
            // Установка версии
            if (config.autoIncrement)
            {
                config.IncrementVersion();
                EditorUtility.SetDirty(config);
            }
              PlayerSettings.bundleVersion = config.version;            // Установка Bundle ID
            var buildTargetGroup = GetBuildTargetGroup(config.buildTarget);
            if (buildTargetGroup != BuildTargetGroup.Unknown)
            {
#if UNITY_2022_1_OR_NEWER
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                PlayerSettings.SetApplicationIdentifier(namedBuildTarget, config.bundleId);
#else
                PlayerSettings.applicationIdentifier = config.bundleId;
#endif
            }
            
            Debug.Log($"Applied project settings - Version: {config.version}, Bundle ID: {config.bundleId}");
        }
          private static void ApplyPlatformSettings(BuildConfiguration config)
        {
            switch (config.buildTarget)
            {
                case BuildTargetType.WebGL:
                    ApplyWebGLSettings(config);
                    break;
                case BuildTargetType.Android:
                    ApplyAndroidSettings(config);
                    break;
                case BuildTargetType.iOS:
                    ApplyiOSSettings(config);
                    break;
            }
        }
        
        private static void ApplyWebGLSettings(BuildConfiguration config)
        {
            // Установка профиля сборки
            if (config.buildProfile == "Development")
            {
                EditorUserBuildSettings.development = true;
            }
            else
            {
                EditorUserBuildSettings.development = false;
            }
            
            // Если используется только один метод сжатия, устанавливаем его
            if (config.webglCompressionMethods.Count == 1)
            {
                var method = config.webglCompressionMethods[0];
                SetWebGLTextureCompression(method);
                Debug.Log($"Set WebGL texture compression to: {method}");
            }
        }
        
        private static void ApplyAndroidSettings(BuildConfiguration config)
        {
            if (config.buildProfile == "Development")
            {
                EditorUserBuildSettings.development = true;
                EditorUserBuildSettings.allowDebugging = true;
            }
            else
            {
                EditorUserBuildSettings.development = false;
                EditorUserBuildSettings.allowDebugging = false;
            }
        }
        
        private static void ApplyiOSSettings(BuildConfiguration config)
        {
            if (config.buildProfile == "Development")
            {
                EditorUserBuildSettings.development = true;
            }
            else
            {
                EditorUserBuildSettings.development = false;
            }
        }
        
        private static BuildReport ExecuteBuild(BuildConfiguration config)
        {
            Debug.Log($"Starting build for configuration: {config.configName}");
            
            if (config.buildTarget == BuildTargetType.WebGL && config.webglCompressionMethods.Count > 1)
            {
                return ExecuteWebGLMultiCompressionBuild(config);
            }
            else
            {
                return ExecuteSingleBuild(config);
            }
        }
        
        private static BuildReport ExecuteSingleBuild(BuildConfiguration config)
        {
            string buildPath = GetBuildPath(config);
              var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = buildPath,
                target = ConvertBuildTargetType(config.buildTarget),
                options = GetBuildOptions(config)
            };
            
            Debug.Log($"Building to: {buildPath}");
            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        
        private static BuildReport ExecuteWebGLMultiCompressionBuild(BuildConfiguration config)
        {
            string baseBuildPath = GetBuildPath(config);
            BuildReport lastResult = null;
            
            foreach (var compressionMethod in config.webglCompressionMethods)
            {
                Debug.Log($"Building WebGL with {compressionMethod} compression...");
                
                // Установка метода сжатия
                SetWebGLTextureCompression(compressionMethod);
                
                // Путь для этого метода сжатия
                string compressionBuildPath = Path.Combine(baseBuildPath, compressionMethod.ToString());
                  var buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = GetEnabledScenes(),
                    locationPathName = compressionBuildPath,
                    target = ConvertBuildTargetType(config.buildTarget),
                    options = GetBuildOptions(config)
                };
                
                lastResult = BuildPipeline.BuildPlayer(buildPlayerOptions);
                
                if (lastResult.summary.result != BuildResult.Succeeded)
                {
                    throw new System.Exception($"Build failed for {compressionMethod} compression");
                }
                
                // Копирование и переименование data файлов
                ProcessWebGLDataFiles(compressionBuildPath, baseBuildPath, compressionMethod);
            }
            
            return lastResult;
        }
        
        private static void ProcessWebGLDataFiles(string sourcePath, string basePath, TextureCompressionMethod method)
        {
            // Поиск data файла
            string[] dataFiles = Directory.GetFiles(sourcePath, "*.data", SearchOption.AllDirectories);
            
            foreach (string dataFile in dataFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(dataFile);
                string newFileName = $"{fileName}.{method.ToString().ToLower()}.data";
                string destPath = Path.Combine(basePath, newFileName);
                
                File.Copy(dataFile, destPath, true);
                Debug.Log($"Copied data file: {newFileName}");
            }
            
            // Копирование других необходимых файлов
            CopyWebGLSupportFiles(sourcePath, basePath, method);
        }
        
        private static void CopyWebGLSupportFiles(string sourcePath, string basePath, TextureCompressionMethod method)
        {
            // Копирование wasm файлов
            string[] wasmFiles = Directory.GetFiles(sourcePath, "*.wasm", SearchOption.AllDirectories);
            foreach (string wasmFile in wasmFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(wasmFile);
                string newFileName = $"{fileName}.{method.ToString().ToLower()}.wasm";
                string destPath = Path.Combine(basePath, newFileName);
                
                if (!File.Exists(destPath)) // Копируем только если ещё не существует
                {
                    File.Copy(wasmFile, destPath, true);
                }
            }
        }
        
        private static void SetWebGLTextureCompression(TextureCompressionMethod method)
        {
            switch (method)
            {
                case TextureCompressionMethod.DXT:
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
                    break;
                case TextureCompressionMethod.ASTC:
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
                    break;
                case TextureCompressionMethod.ETC2:
                    EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ETC2;
                    break;
            }
        }
        
        private static void HandleBuildResult(BuildConfiguration config, BuildReport result)
        {
            if (result.summary.result == BuildResult.Succeeded)
            {                Debug.Log($"Build succeeded for {config.configName}!");
                Debug.Log($"Build size: {result.summary.totalSize} bytes");
                Debug.Log($"Build time: {result.summary.totalTime}");
                
                BuildSystem.TriggerBuildCompleted(config.configName, true);
                
                // Автоматический деплой если настроен
                if (config.autoDeployOnSuccess && config.deployConfig.enabled)
                {
                    string buildPath = GetBuildPath(config);
                    DeploymentSystem.Deploy(config.deployConfig, buildPath);
                }
            }
            else
            {                Debug.LogError($"Build failed for {config.configName}!");
                Debug.LogError($"Errors: {result.summary.totalErrors}");
                Debug.LogError($"Warnings: {result.summary.totalWarnings}");
                
                BuildSystem.TriggerBuildCompleted(config.configName, false);
            }
        }
        
        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }
        
        private static BuildOptions GetBuildOptions(BuildConfiguration config)
        {
            var options = BuildOptions.None;
            
            if (config.buildProfile == "Development")
            {
                options |= BuildOptions.Development;
            }
            
            return options;
        }
        
        private static BuildTarget ConvertBuildTargetType(BuildTargetType targetType)
        {
            switch (targetType)
            {
                case BuildTargetType.WebGL:
                    return BuildTarget.WebGL;
                case BuildTargetType.Android:
                    return BuildTarget.Android;
                case BuildTargetType.iOS:
                    return BuildTarget.iOS;
                case BuildTargetType.StandaloneWindows:
                    return BuildTarget.StandaloneWindows;
                case BuildTargetType.StandaloneWindows64:
                    return BuildTarget.StandaloneWindows64;
                case BuildTargetType.StandaloneOSX:
                    return BuildTarget.StandaloneOSX;
                case BuildTargetType.StandaloneLinux64:
                    return BuildTarget.StandaloneLinux64;
                default:
                    return BuildTarget.WebGL;
            }
        }

        private static BuildTargetGroup GetBuildTargetGroup(BuildTargetType targetType)
        {
            var unityTarget = ConvertBuildTargetType(targetType);
            return GetBuildTargetGroup(unityTarget);
        }
        
        private static BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;
                default:
                    return BuildTargetGroup.Unknown;
            }
        }
        
        private static string GetBuildPath(BuildConfiguration config)
        {
            return Path.Combine(Application.dataPath, "..", "Builds", config.configName);
        }
    }
}
