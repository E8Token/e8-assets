using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public static class BuildManager
    {
        public static event Action<string> OnBuildStarted;
        public static event Action<string, bool> OnBuildCompleted;
        public static event Action<string> OnCleanStarted;
        public static event Action<string, bool> OnCleanCompleted;        public static void BuildConfiguration(BuildConfiguration config)
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
                Debug.Log($"Starting build for configuration: {displayName}");

                // Clean если нужно
                if (config.CleanBeforeBuild)
                {
                    CleanBuildInternal(config);
                }

                // Создаем директорию если её нет
                string fullOutputPath = Path.GetFullPath(config.OutputPath);
                if (!Directory.Exists(fullOutputPath))
                {
                    Directory.CreateDirectory(fullOutputPath);
                }

                // Используем Build Profile для сборки
                var buildProfile = config.BuildProfile;
                if (buildProfile == null)
                {
                    throw new Exception("Build Profile not found");
                }

                // Получаем путь для исполняемого файла
                string executablePath = GetExecutablePath(config, fullOutputPath);                // Проверяем если это WebGL с несколькими форматами текстур
                var buildTarget = GetBuildTargetFromProfile(buildProfile);
                bool success = false;

                if (buildTarget == BuildTarget.WebGL && HasMultipleWebGLFormats(config))
                {
                    success = BuildWebGLWithMultipleTextureFormats(config, buildProfile, executablePath);
                }
                else
                {
                    success = BuildWithProfile(buildProfile, executablePath);
                }                // Создаем ZIP архив если нужно
                if (success && config.CreateZipArchive)
                {
                    CreateZipArchive(config, fullOutputPath);
                }

                // Деплоим если нужно
                if (success && config.DeploySettings.EnableDeploy)
                {
                    _ = DeployBuildAsync(config, fullOutputPath);
                }

                if (success)
                {
                    Debug.Log($"Build completed successfully: {executablePath}");
                }

                OnBuildCompleted?.Invoke(displayName, success);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Build failed with exception: {ex.Message}");
                OnBuildCompleted?.Invoke(config.GetDisplayName(), false);
            }
        }

        public static void CleanBuild(BuildConfiguration config)
        {
            if (config == null)
            {
                Debug.LogError("Build configuration is null");
                return;
            }

            CleanBuildInternal(config);
        }

        private static void CleanBuildInternal(BuildConfiguration config)
        {
            try
            {
                var displayName = config.GetDisplayName();
                OnCleanStarted?.Invoke(displayName);
                Debug.Log($"Cleaning build directory for configuration: {displayName}");

                string fullOutputPath = Path.GetFullPath(config.OutputPath);
                
                if (Directory.Exists(fullOutputPath))
                {
                    Directory.Delete(fullOutputPath, true);
                    Debug.Log($"Cleaned directory: {fullOutputPath}");
                }

                // Пересоздаем директорию
                Directory.CreateDirectory(fullOutputPath);

                OnCleanCompleted?.Invoke(displayName, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Clean failed with exception: {ex.Message}");
                OnCleanCompleted?.Invoke(config.GetDisplayName(), false);
            }
        }

        private static bool BuildWithProfile(BuildProfile profile, string outputPath)
        {
            try
            {
                // Используем Build Profile API для сборки
                // Пока что используем старый API, позже можно будет перейти на новый
                var buildTarget = GetBuildTargetFromProfile(profile);
                var scenes = GetScenesToBuild();

                BuildPlayerOptions buildOptions = new BuildPlayerOptions
                {
                    locationPathName = outputPath,
                    target = buildTarget,
                    scenes = scenes,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(buildOptions);
                return report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Build with profile failed: {ex.Message}");
                return false;
            }
        }        private static BuildTarget GetBuildTargetFromProfile(BuildProfile profile)
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

        private static string GetExecutablePath(BuildConfiguration config, string outputDirectory)
        {
            var buildTarget = GetBuildTargetFromProfile(config.BuildProfile);
            var appName = !string.IsNullOrEmpty(config.CustomBuildName) ? 
                config.CustomBuildName : 
                PlayerSettings.productName;

            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(outputDirectory, $"{appName}.exe");
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(outputDirectory, $"{appName}.app");
                case BuildTarget.StandaloneLinux64:
                    return Path.Combine(outputDirectory, appName);
                case BuildTarget.WebGL:
                    return outputDirectory; // WebGL builds to a folder
                case BuildTarget.Android:
                    return Path.Combine(outputDirectory, $"{appName}.apk");
                case BuildTarget.iOS:
                    return outputDirectory; // iOS builds to a folder
                default:
                    return Path.Combine(outputDirectory, appName);
            }
        }

        private static void CreateZipArchive(BuildConfiguration config, string buildPath)
        {
            try
            {
                var appName = !string.IsNullOrEmpty(config.CustomBuildName) ? 
                    config.CustomBuildName : 
                    PlayerSettings.productName;
                
                var zipPath = Path.Combine(Path.GetDirectoryName(buildPath), $"{appName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                
                ZipFile.CreateFromDirectory(buildPath, zipPath);
                Debug.Log($"Created ZIP archive: {zipPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create ZIP archive: {ex.Message}");
            }
        }

        private static string[] GetScenesToBuild()
        {
            var scenes = EditorBuildSettings.scenes;
            var scenePaths = new string[scenes.Length];
            
            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            return scenePaths;
        }        private static bool BuildWebGLWithMultipleTextureFormats(BuildConfiguration config, BuildProfile profile, string outputPath)
        {
            try
            {
                var formats = GetWebGLTextureFormats(config);
                
                if (formats.Length == 0)
                {
                    Debug.LogWarning("No texture formats selected for WebGL build");
                    return BuildWithProfile(profile, outputPath);
                }

                Debug.Log($"Building WebGL with multiple texture formats: {string.Join(", ", formats)}");

                bool mainBuildSuccess = false;
                string mainFormat = formats[0]; // Первый формат - основной

                // Сборка основного формата
                Debug.Log($"Building main format: {mainFormat}");
                
                // Устанавливаем настройки для основного формата
                SetWebGLTextureFormat(mainFormat);
                
                // Строим основной формат
                mainBuildSuccess = BuildWithProfile(profile, outputPath);
                
                if (!mainBuildSuccess)
                {
                    Debug.LogError($"Main build with {mainFormat} format failed");
                    return false;
                }

                // Строим дополнительные форматы
                for (int i = 1; i < formats.Length; i++)
                {
                    string format = formats[i];
                    Debug.Log($"Building additional format: {format}");
                    
                    // Временная папка для дополнительного формата
                    string tempBuildPath = outputPath + "_" + format;
                      try
                    {
                        // Устанавливаем настройки для дополнительного формата
                        SetWebGLTextureFormat(format);
                        
                        // Переключаемся на нужный Build Profile и строим
                        bool additionalBuildSuccess = BuildWithProfileSwitch(profile, tempBuildPath);
                        
                        if (additionalBuildSuccess)
                        {
                            // Копируем .data файл в основную папку
                            CopyWebGLDataFile(tempBuildPath, outputPath, format, config);
                            
                            // Удаляем временную папку
                            if (Directory.Exists(tempBuildPath))
                            {
                                Directory.Delete(tempBuildPath, true);
                            }
                            
                            Debug.Log($"Successfully built and copied {format} data file");
                        }
                        else
                        {
                            Debug.LogWarning($"Build with {format} format failed, skipping...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error building {format} format: {ex.Message}");
                        
                        // Очищаем временную папку при ошибке
                        if (Directory.Exists(tempBuildPath))
                        {
                            try
                            {
                                Directory.Delete(tempBuildPath, true);
                            }
                            catch (Exception cleanupEx)
                            {
                                Debug.LogWarning($"Failed to cleanup temp directory: {cleanupEx.Message}");
                            }                        }
                    }
                }

                // Переименовываем основной .data файл, чтобы он тоже содержал тип текстуры
                RenameMainWebGLDataFile(outputPath, mainFormat, config);

                Debug.Log("WebGL multi-format build completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebGL multi-format build failed: {ex.Message}");
                return false;
            }
        }        private static string[] GetWebGLTextureFormats(BuildConfiguration config)
        {
            var formats = new System.Collections.Generic.List<string>();
            
            // Добавляем основной формат первым
            string mainFormat = GetCurrentWebGLTextureFormat();
            formats.Add(mainFormat);
            
            // Добавляем дополнительные форматы (исключая основной)
            if (config.WebGLSettings.BuildDXT && mainFormat != "DXT") formats.Add("DXT");
            if (config.WebGLSettings.BuildASTC && mainFormat != "ASTC") formats.Add("ASTC");
            if (config.WebGLSettings.BuildETC2 && mainFormat != "ETC2") formats.Add("ETC2");
            
            return formats.ToArray();
        }private static bool HasMultipleWebGLFormats(BuildConfiguration config)
        {
            int count = 0;
            if (config.WebGLSettings.BuildDXT) count++;
            if (config.WebGLSettings.BuildASTC) count++;
            if (config.WebGLSettings.BuildETC2) count++;
            return count > 0; // Изменено: запускаем мультиформатную сборку если выбран хотя бы один формат
        }

        private static void SetWebGLTextureFormat(string format)
        {
            try
            {
                switch (format.ToUpper())
                {
                    case "DXT":
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
                        Debug.Log("Set WebGL texture format to DXT");
                        break;
                    case "ASTC":
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
                        Debug.Log("Set WebGL texture format to ASTC");
                        break;
                    case "ETC2":
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ETC2;
                        Debug.Log("Set WebGL texture format to ETC2");
                        break;
                    default:
                        Debug.LogWarning($"Unknown WebGL texture format: {format}. Using DXT as fallback.");
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set WebGL texture format to {format}: {ex.Message}");
            }
        }        private static void CopyWebGLDataFile(string sourceBuildPath, string targetBuildPath, string format, BuildConfiguration config)
        {
            try            {                var appName = !string.IsNullOrEmpty(config.CustomBuildName) ? 
                    config.CustomBuildName : 
                    PlayerSettings.productName;
                
                // Поиск .data файла (может иметь различные расширения в зависимости от настроек Build Profile)
                string sourceDataFile = Path.Combine(sourceBuildPath, $"{appName}.data");
                
                if (!File.Exists(sourceDataFile))
                {
                    // Попробуем найти любой .data файл с расширениями
                    var dataFiles = Directory.GetFiles(sourceBuildPath, "*.data*");
                    if (dataFiles.Length > 0)
                    {
                        sourceDataFile = dataFiles[0];
                        Debug.Log($"Found data file: {Path.GetFileName(sourceDataFile)}");
                    }
                    else
                    {
                        Debug.LogWarning($"No .data file found in {sourceBuildPath}");
                        return;
                    }                }

                // Разбираем оригинальное имя файла
                string originalFileName = Path.GetFileName(sourceDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceDataFile);
                string extension = Path.GetExtension(sourceDataFile);
                
                // Проверяем, есть ли дополнительное сжатие (.gz, .br)
                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameWithoutExt);
                    extension = Path.GetExtension(fileNameWithoutExt);
                }                // Создаем новое имя файла: [originalname].[texture_type].data[.compression]
                // Например: myapp.dxt.data или myapp.dxt.data.gz
                string targetFileName = $"{fileNameWithoutExt}.{format.ToLower()}.data{compressionExt}";
                string targetDataFile = Path.Combine(targetBuildPath, targetFileName);

                File.Copy(sourceDataFile, targetDataFile, true);
                Debug.Log($"Copied {format} data file: {targetFileName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to copy WebGL data file for {format}: {ex.Message}");
            }
        }

        private static void RenameMainWebGLDataFile(string buildPath, string format, BuildConfiguration config)
        {
            try
            {
                var appName = !string.IsNullOrEmpty(config.CustomBuildName) ? 
                    config.CustomBuildName : 
                    PlayerSettings.productName;
                
                // Ищем основной .data файл
                string originalDataFile = Path.Combine(buildPath, $"{appName}.data");
                
                if (!File.Exists(originalDataFile))
                {
                    // Попробуем найти любой .data файл
                    var dataFiles = Directory.GetFiles(buildPath, "*.data");
                    if (dataFiles.Length > 0)
                    {
                        originalDataFile = dataFiles[0];
                        Debug.Log($"Found main data file: {Path.GetFileName(originalDataFile)}");
                    }
                    else
                    {
                        Debug.LogWarning($"No main .data file found in {buildPath}");
                        return;
                    }
                }

                // Разбираем оригинальное имя файла
                string originalFileName = Path.GetFileName(originalDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalDataFile);
                string extension = Path.GetExtension(originalDataFile);
                
                // Проверяем, есть ли дополнительное сжатие (.gz, .br)
                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameWithoutExt);
                    extension = Path.GetExtension(fileNameWithoutExt);
                }                // Создаем новое имя: [originalname].[texture_type].data[.compression]
                string newFileName = $"{fileNameWithoutExt}.{format.ToLower()}.data{compressionExt}";
                string newDataFile = Path.Combine(buildPath, newFileName);

                // Переименовываем файл
                if (originalDataFile != newDataFile)
                {
                    File.Move(originalDataFile, newDataFile);
                    Debug.Log($"Renamed main data file: {originalFileName} -> {newFileName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to rename main WebGL data file: {ex.Message}");
            }
        }

        private static async System.Threading.Tasks.Task DeployBuildAsync(BuildConfiguration config, string buildPath)
        {
            try
            {
                Debug.Log("Starting deployment...");
                bool deploySuccess = await DeployManager.DeployBuild(config, buildPath);
                
                if (deploySuccess)
                {
                    Debug.Log("Deployment completed successfully!");
                }
                else
                {
                    Debug.LogError("Deployment failed!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Deployment error: {ex.Message}");
            }
        }        private static bool BuildWithProfileSwitch(BuildProfile profile, string outputPath)
        {
            try
            {
                // Используем BuildPipeline напрямую с указанным профилем
                Debug.Log($"Building with profile: {profile.name} to {outputPath}");
                
                // Создаем BuildPlayerOptions на основе профиля
                var buildOptions = new BuildPlayerOptions
                {
                    locationPathName = outputPath,
                    targetGroup = BuildTargetGroup.WebGL,
                    target = BuildTarget.WebGL,
                    scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray()
                };
                
                var result = BuildPipeline.BuildPlayer(buildOptions);
                
                if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    Debug.Log($"Build succeeded: {outputPath}");
                    return true;
                }
                else
                {
                    Debug.LogError($"Build failed: {result.summary.result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Build with profile switch failed: {ex.Message}");
                return false;
            }
        }

        public static string GetCurrentWebGLTextureFormat()
        {
            switch (EditorUserBuildSettings.webGLBuildSubtarget)
            {
                case WebGLTextureSubtarget.DXT:
                    return "DXT";
                case WebGLTextureSubtarget.ASTC:
                    return "ASTC";
                case WebGLTextureSubtarget.ETC2:
                    return "ETC2";
                default:
                    return "ETC2"; // Значение по умолчанию
            }
        }
    }
}
