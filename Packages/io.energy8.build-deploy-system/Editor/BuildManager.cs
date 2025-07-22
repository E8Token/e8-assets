using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Energy8.BuildDeploySystem;
using Newtonsoft.Json;

namespace Energy8.BuildDeploySystem.Editor
{
    public static class BuildManager
    {
        public static event Action<string> OnBuildStarted;
        public static event Action<string, bool> OnBuildCompleted;
        public static event Action<string> OnCleanStarted;
        public static event Action<string, bool> OnCleanCompleted;        /// <summary>
                                                                          /// Заменяет плейсхолдеры в OutputPath на актуальные значения
                                                                          /// </summary>
                                                                          /// <param name="outputPath">Исходный путь сборки</param>
                                                                          /// <param name="config">Конфигурация сборки для получения информации о платформе</param>
                                                                          /// <returns>Путь с замененными плейсхолдерами</returns>
        private static string ProcessOutputPathTemplate(string outputPath, BuildConfiguration config = null)
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
                        Debug.LogWarning("[BuildSystem] GlobalVersion not found. Cannot replace VERSION placeholder.");
                        processedPath = processedPath.Replace("{{{VERSION}}}", "unknown");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[BuildSystem] Failed to get version for placeholder: {ex.Message}");
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
        private static string GetPlatformName(BuildTarget buildTarget)
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
        public static void BuildConfiguration(BuildConfiguration config)
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
                Debug.Log($"[BuildSystem] Starting build for configuration: {displayName}");                // Clean если нужно
                if (config.CleanBeforeBuild)
                {
                    CleanBuildInternal(config);
                }

                // Обрабатываем шаблоны в OutputPath
                string processedOutputPath = ProcessOutputPathTemplate(config.OutputPath, config);
                string fullOutputPath = Path.GetFullPath(processedOutputPath);
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
                bool success = false; Debug.Log($"[BuildSystem] Build target: {buildTarget}");
                Debug.Log($"[BuildSystem] Has multiple WebGL formats: {HasMultipleWebGLFormats(config)}");

                // Устанавливаем Bundle ID для мобильных платформ
                SetBundleIdForMobilePlatforms(config, buildTarget);

                if (buildTarget == BuildTarget.WebGL && HasMultipleWebGLFormats(config))
                {
                    Debug.Log("[BuildSystem] Starting WebGL multi-format build");
                    success = BuildWebGLWithMultipleTextureFormats(config, buildProfile, executablePath);
                }
                else
                {
                    Debug.Log("[BuildSystem] Starting regular build");
                    success = BuildWithProfile(buildProfile, executablePath);
                }

                if (buildTarget == BuildTarget.WebGL
                    && config.WebGLSettings.CompressionAlgorithms != null
                    && config.WebGLSettings.CompressionAlgorithms.Count > 0)
                {
                    CompressWebGLDataFiles(fullOutputPath, config);
                }

                if (buildTarget == BuildTarget.WebGL)
                    GenerateBuildJson(fullOutputPath);


                // Инкрементируем версию билда если сборка успешна
                if (success)
                {
                    try
                    {
                        GlobalVersion.Instance.IncrementBuild();
                        Debug.Log($"[BuildSystem] Version updated to: {GlobalVersion.Instance.Version}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[BuildSystem] Failed to increment build version: {ex.Message}");
                    }
                }

                // Деплоим если нужно
                if (success && (config.DeploySettings.EnableDeploy || config.DeploySettings.AlwaysDeploy))
                {
                    _ = DeployBuildAsync(config, fullOutputPath);
                }
                if (success)
                {
                    Debug.Log($"[BuildSystem] Build completed successfully: {executablePath}");
                }

                OnBuildCompleted?.Invoke(displayName, success);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Build failed with exception: {ex.Message}");
                OnBuildCompleted?.Invoke(config.GetDisplayName(), false);
            }
        }

        public static void CleanBuild(BuildConfiguration config)
        {
            if (config == null)
            {
                Debug.LogError("[BuildSystem] Build configuration is null");
                return;
            }

            CleanBuildInternal(config);
        }
        private static void CleanBuildInternal(BuildConfiguration config)
        {
            try
            {
                var displayName = config.GetDisplayName();
                OnCleanStarted?.Invoke(displayName); Debug.Log($"[BuildSystem] Cleaning build directory for configuration: {displayName}");

                // Обрабатываем шаблоны в OutputPath
                string processedOutputPath = ProcessOutputPathTemplate(config.OutputPath, config);
                string fullOutputPath = Path.GetFullPath(processedOutputPath); if (Directory.Exists(fullOutputPath))
                {
                    Directory.Delete(fullOutputPath, true);
                    Debug.Log($"[BuildSystem] Cleaned directory: {fullOutputPath}");
                }

                // Пересоздаем директорию
                Directory.CreateDirectory(fullOutputPath);

                OnCleanCompleted?.Invoke(displayName, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Clean failed with exception: {ex.Message}");
                OnCleanCompleted?.Invoke(config.GetDisplayName(), false);
            }
        }
        private static bool BuildWithProfile(BuildProfile profile, string outputPath)
        {
            try
            {
                // Устанавливаем активный Build Profile перед сборкой
                try
                {
                    SetActiveBuildProfile(profile);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[BuildSystem] Failed to set active build profile: {profile?.name}. {ex.Message}");
                    return false;
                }

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
                Debug.LogError($"[BuildSystem] Build with profile failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Устанавливает Bundle ID для мобильных платформ на основе версии
        /// </summary>
        private static void SetBundleIdForMobilePlatforms(BuildConfiguration config, BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.Android || buildTarget == BuildTarget.iOS)
            {
                try
                {
                    string bundleId = GlobalVersion.Instance.GenerateBundleId();
                    string basePackageName = PlayerSettings.applicationIdentifier;

                    // Получаем базовое имя пакета без версии
                    if (basePackageName.Contains("."))
                    {
                        var parts = basePackageName.Split('.');
                        if (parts.Length >= 2)
                        {
                            string baseName = string.Join(".", parts.Take(parts.Length - 1));
                            string newPackageName = $"{baseName}.{bundleId}";
                            var targetGroup = buildTarget == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
                            PlayerSettings.applicationIdentifier = newPackageName; Debug.Log($"[BuildSystem] Set Bundle ID for {buildTarget}: {newPackageName}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[BuildSystem] Failed to set Bundle ID: {ex.Message}");
                }
            }
        }
        private static BuildTarget GetBuildTargetFromProfile(BuildProfile profile)
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
                Debug.LogWarning($"[BuildSystem] Failed to get BuildTarget from profile directly: {ex.Message}");
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

        /// <summary>
        /// Устанавливает активный Build Profile в редакторе Unity
        /// </summary>
        /// <param name="profile">Build Profile для установки как активный</param>
        /// <returns>true если профиль был успешно установлен, false в противном случае</returns>
        /// <summary>
        /// Устанавливает активный Build Profile через официальный API Unity.
        /// Если профиль не переключился — кидает ошибку и останавливает сборку.
        /// </summary>
        private static void SetActiveBuildProfile(BuildProfile profile)
        {
            if (profile == null)
            {
                Debug.LogError("[BuildSystem] BuildProfile is null. Сборка остановлена.");
                throw new System.Exception("BuildProfile is null");
            }

            Debug.Log($"[BuildSystem] Устанавливаем Build Profile: {profile.name}");
            UnityEditor.Build.Profile.BuildProfile.SetActiveBuildProfile(profile);

            // Проверяем, действительно ли профиль стал активным
            if (UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile() != profile)
            {
                Debug.LogError($"[BuildSystem] Не удалось переключить Build Profile на '{profile.name}'. Сборка остановлена.");
                throw new System.Exception($"Не удалось переключить Build Profile на '{profile.name}'");
            }
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
        private static bool BuildWebGLWithMultipleTextureFormats(BuildConfiguration config, BuildProfile profile, string outputPath)
        {
            try
            {
                var formats = GetWebGLTextureFormats(config); if (formats.Length == 0)
                {
                    Debug.LogWarning("[BuildSystem] No texture formats selected for WebGL build");
                    return BuildWithProfile(profile, outputPath);
                }

                Debug.Log($"[BuildSystem] Building WebGL with texture formats: {string.Join(", ", formats)}");

                bool anyBuildSuccess = false;

                // Строим все выбранные форматы
                for (int i = 0; i < formats.Length; i++)
                {
                    string format = formats[i];
                    Debug.Log($"[BuildSystem] Building format {i + 1}/{formats.Length}: {format}");

                    // Для первого формата используем основную папку, для остальных - временные
                    string buildPath = (i == 0) ? outputPath : outputPath + "_" + format;

                    try
                    {
                        // Устанавливаем настройки для текущего формата
                        SetWebGLTextureFormat(format);
                        bool buildSuccess = BuildWithProfileSwitch(profile, buildPath);
                        Debug.Log($"[BuildSystem] Build result for {format}: {buildSuccess}");
                        if (!buildSuccess)
                        {
                            Debug.LogError($"[BuildSystem] Build with {format} format failed! Останавливаем процесс.");
                            // Очищаем временную папку при ошибке
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

                        // Проверим, что файлы действительно создались
                        Debug.Log($"[BuildSystem] Files in build directory {buildPath}:");
                        if (Directory.Exists(buildPath))
                        {
                            var files = Directory.GetFiles(buildPath);
                            foreach (var file in files)
                            {
                                Debug.Log($"[BuildSystem]   - {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
                            }

                            // Также проверим подпапку Build
                            string buildSubPath = Path.Combine(buildPath, "Build");
                            if (Directory.Exists(buildSubPath))
                            {
                                Debug.Log($"[BuildSystem] Files in Build subdirectory:");
                                var buildFiles = Directory.GetFiles(buildSubPath);
                                foreach (var file in buildFiles)
                                {
                                    Debug.Log($"[BuildSystem]     - {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
                                }
                            }
                            else
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
                            // Первый билд - переименовываем основной .data файл
                            Debug.Log($"[BuildSystem] Processing main build (format {format})");
                            RenameMainWebGLDataFile(buildPath, format, config);
                            anyBuildSuccess = true;
                        }
                        else
                        {
                            // Дополнительные билды - копируем .data файл в основную папку
                            Debug.Log($"[BuildSystem] Processing additional build (format {format})");
                            CopyWebGLDataFile(buildPath, outputPath, format, config);

                            // Удаляем временную папку
                            if (Directory.Exists(buildPath))
                            {
                                Debug.Log($"[BuildSystem] Cleaning up temporary directory: {buildPath}");
                                Directory.Delete(buildPath, true);
                            }
                        }

                        Debug.Log($"[BuildSystem] Successfully built {format} format");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[BuildSystem] Error building {format} format: {ex.Message}");

                        // Очищаем временную папку при ошибке
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
                    // Показываем финальный список файлов в основной папке
                    Debug.Log($"[BuildSystem] Final files in output directory {outputPath}:");
                    if (Directory.Exists(outputPath))
                    {
                        var files = Directory.GetFiles(outputPath);
                        foreach (var file in files)
                        {
                            Debug.Log($"[BuildSystem]   - {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
                        }

                        // Также показываем файлы в подпапке Build
                        string finalBuildSubPath = Path.Combine(outputPath, "Build");
                        if (Directory.Exists(finalBuildSubPath))
                        {
                            Debug.Log($"[BuildSystem] Final files in Build subdirectory:");
                            var buildFiles = Directory.GetFiles(finalBuildSubPath);
                            foreach (var file in buildFiles)
                            {
                                Debug.Log($"[BuildSystem]     - {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
                            }
                        }
                    }

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
            var formats = new System.Collections.Generic.List<string>();

            // Добавляем только выбранные форматы
            if (config.WebGLSettings.BuildDXT) formats.Add("DXT");
            if (config.WebGLSettings.BuildASTC) formats.Add("ASTC");
            if (config.WebGLSettings.BuildETC2) formats.Add("ETC2"); Debug.Log($"[BuildSystem] Selected WebGL texture formats: [{string.Join(", ", formats)}]");
            return formats.ToArray();
        }
        private static bool HasMultipleWebGLFormats(BuildConfiguration config)
        {
            int count = 0;
            if (config.WebGLSettings.BuildDXT) count++;
            if (config.WebGLSettings.BuildASTC) count++;
            if (config.WebGLSettings.BuildETC2) count++;
            Debug.Log($"[BuildSystem] WebGL format settings - DXT: {config.WebGLSettings.BuildDXT}, ASTC: {config.WebGLSettings.BuildASTC}, ETC2: {config.WebGLSettings.BuildETC2}");
            Debug.Log($"[BuildSystem] WebGL formats count: {count}");

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
                        Debug.Log("[BuildSystem] Set WebGL texture format to DXT");
                        break;
                    case "ASTC":
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
                        Debug.Log("[BuildSystem] Set WebGL texture format to ASTC");
                        break;
                    case "ETC2":
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ETC2;
                        Debug.Log("[BuildSystem] Set WebGL texture format to ETC2");
                        break;
                    default:
                        Debug.LogWarning($"[BuildSystem] Unknown WebGL texture format: {format}. Using DXT as fallback.");
                        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to set WebGL texture format to {format}: {ex.Message}");
            }
        }
        private static void CopyWebGLDataFile(string sourceBuildPath, string targetBuildPath, string format, BuildConfiguration config)
        {
            try
            {
                Debug.Log($"[BuildSystem] CopyWebGLDataFile: source={sourceBuildPath}, target={targetBuildPath}, format={format}");

                var appName = !string.IsNullOrEmpty(config.CustomBuildName) ?
                    config.CustomBuildName :
                    PlayerSettings.productName;

                Debug.Log($"[BuildSystem] App name: {appName}");                // Поиск .data файла в подпапке Build (WebGL создает структуру BuildPath/Build/)
                string buildSubPath = Path.Combine(sourceBuildPath, "Build");
                string sourceDataFile = Path.Combine(buildSubPath, $"{appName}.data");
                Debug.Log($"[BuildSystem] Looking for source data file: {sourceDataFile}"); if (!File.Exists(sourceDataFile))
                {
                    Debug.Log("[BuildSystem] Expected data file not found, searching for any .data files in Build subfolder");
                    // Попробуем найти любой .data файл с расширениями в подпапке Build
                    var dataFiles = Directory.GetFiles(buildSubPath, "*.data*");
                    Debug.Log($"[BuildSystem] Found {dataFiles.Length} .data files: {string.Join(", ", dataFiles.Select(Path.GetFileName))}");

                    if (dataFiles.Length > 0)
                    {
                        sourceDataFile = dataFiles[0];
                        Debug.Log($"[BuildSystem] Using data file: {Path.GetFileName(sourceDataFile)}");
                    }
                    else
                    {
                        Debug.LogWarning($"[BuildSystem] No .data file found in {buildSubPath}");
                        return;
                    }
                }// Разбираем оригинальное имя файла
                string originalFileName = Path.GetFileName(sourceDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourceDataFile);
                string extension = Path.GetExtension(sourceDataFile);

                Debug.Log($"[BuildSystem] Source file: {originalFileName}, name without ext: {fileNameWithoutExt}, ext: {extension}");                // Проверяем, есть ли дополнительное сжатие (.gz, .br)
                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    // Получаем имя без сжатия: myapp.data.gz -> myapp.data
                    string nameWithoutCompression = Path.GetFileNameWithoutExtension(sourceDataFile);
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutCompression);
                    string dataExt = Path.GetExtension(nameWithoutCompression);
                    Debug.Log($"[BuildSystem] Found compression: {compressionExt}, base name: {fileNameWithoutExt}, data ext: {dataExt}");
                }
                else if (!extension.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    // Если это не .data и не известное сжатие, предупреждаем
                    Debug.LogWarning($"[BuildSystem] Unexpected file extension: {extension} for file {originalFileName}");
                }

                // Извлекаем базовое имя приложения (убираем суффиксы формата, если есть)
                string baseAppName = ExtractBaseAppName(fileNameWithoutExt, appName);
                Debug.Log($"[BuildSystem] Extracted base app name: '{baseAppName}' from file name: '{fileNameWithoutExt}'");

                // Создаем новое имя файла: [baseappname].[texture_type].data[.compression]
                // Например: Assets.astc.data.gz вместо Assets_ASTC.astц.data.gz
                string targetFileName = $"{baseAppName}.{format.ToLower()}.data{compressionExt}";
                // Целевой файл должен быть в подпапке Build основной папки
                string targetBuildSubPath = Path.Combine(targetBuildPath, "Build");
                string targetDataFile = Path.Combine(targetBuildSubPath, targetFileName); Debug.Log($"[BuildSystem] Target file name: {targetFileName}");
                Debug.Log($"[BuildSystem] Target file path: {targetDataFile}");

                // Убеждаемся, что целевая папка Build существует
                if (!Directory.Exists(targetBuildSubPath))
                {
                    Debug.Log($"[BuildSystem] Creating target Build directory: {targetBuildSubPath}");
                    Directory.CreateDirectory(targetBuildSubPath);
                }

                File.Copy(sourceDataFile, targetDataFile, true);
                Debug.Log($"[BuildSystem] ✅ Successfully copied {format} data file: {targetFileName}");
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
                Debug.Log($"[BuildSystem] RenameMainWebGLDataFile: buildPath={buildPath}, format={format}");

                var appName = !string.IsNullOrEmpty(config.CustomBuildName) ?
                    config.CustomBuildName :
                    PlayerSettings.productName;

                Debug.Log($"[BuildSystem] App name: {appName}");                // Ищем основной .data файл в подпапке Build
                string buildSubPath = Path.Combine(buildPath, "Build");
                string originalDataFile = Path.Combine(buildSubPath, $"{appName}.data");
                Debug.Log($"[BuildSystem] Looking for original data file: {originalDataFile}"); if (!File.Exists(originalDataFile))
                {
                    Debug.Log("[BuildSystem] Original data file not found, searching for any .data files in Build subfolder");
                    // Попробуем найти любой .data файл в подпапке Build
                    var dataFiles = Directory.GetFiles(buildSubPath, "*.data*");
                    Debug.Log($"[BuildSystem] Found {dataFiles.Length} .data files: {string.Join(", ", dataFiles.Select(Path.GetFileName))}");

                    if (dataFiles.Length > 0)
                    {
                        originalDataFile = dataFiles[0];
                        Debug.Log($"[BuildSystem] Using data file: {Path.GetFileName(originalDataFile)}");
                    }
                    else
                    {
                        Debug.LogWarning($"[BuildSystem] No .data files found in {buildSubPath}");
                        return;
                    }
                }

                // Разбираем оригинальное имя файла
                string originalFileName = Path.GetFileName(originalDataFile);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalDataFile);
                string extension = Path.GetExtension(originalDataFile); Debug.Log($"[BuildSystem] Original file: {originalFileName}, name without ext: {fileNameWithoutExt}, ext: {extension}");

                // Проверяем, есть ли дополнительное сжатие (.gz, .br)
                string compressionExt = "";
                if (extension == ".gz" || extension == ".br")
                {
                    compressionExt = extension;
                    // Получаем имя без сжатия: myapp.data.gz -> myapp.data
                    string nameWithoutCompression = Path.GetFileNameWithoutExtension(originalDataFile);
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(nameWithoutCompression);
                    string dataExt = Path.GetExtension(nameWithoutCompression);
                    Debug.Log($"[BuildSystem] Found compression: {compressionExt}, base name: {fileNameWithoutExt}, data ext: {dataExt}");
                }
                else if (!extension.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    // Если это не .data и не известное сжатие, предупреждаем
                    Debug.LogWarning($"[BuildSystem] Unexpected file extension: {extension} for file {originalFileName}");
                }

                // Извлекаем базовое имя приложения (убираем суффиксы формата, если есть)
                string baseAppName = ExtractBaseAppName(fileNameWithoutExt, appName);
                Debug.Log($"[BuildSystem] Extracted base app name: '{baseAppName}' from file name: '{fileNameWithoutExt}'");

                // Создаем новое имя: [baseappname].[texture_type].data[.compression]
                // Например: Assets.astc.data.gz вместо Assets_ASTC.astc.data.gz
                string newFileName = $"{baseAppName}.{format.ToLower()}.data{compressionExt}";
                string newDataFile = Path.Combine(buildSubPath, newFileName);

                Debug.Log($"[BuildSystem] New file name: {newFileName}");
                Debug.Log($"[BuildSystem] New file path: {newDataFile}");

                // Переименовываем файл
                if (originalDataFile != newDataFile)
                {
                    File.Move(originalDataFile, newDataFile);
                    Debug.Log($"[BuildSystem] ✅ Successfully renamed: {originalFileName} -> {newFileName}");
                }
                else
                {
                    Debug.Log("[BuildSystem] File names are the same, no rename needed");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to rename main WebGL data file: {ex.Message}");
                Debug.LogError($"[BuildSystem] Stack trace: {ex.StackTrace}");
            }
        }

        private static async System.Threading.Tasks.Task DeployBuildAsync(BuildConfiguration config, string buildPath)
        {
            try
            {
                Debug.Log("[DeploySystem] Starting deployment...");
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[DeploySystem] Deployment error: {ex.Message}");
            }
        }
        private static bool BuildWithProfileSwitch(BuildProfile profile, string outputPath)
        {
            try
            {
                // Отключаем встроенное сжатие WebGL
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

                // Используем BuildPipeline напрямую с указанным профилем
                Debug.Log($"[BuildSystem] Building with profile: {profile.name} to {outputPath}");

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
                    Debug.Log($"[BuildSystem] Build succeeded: {outputPath}");
                    return true;
                }
                else
                {
                    Debug.LogError($"[BuildSystem] Build failed: {result.summary.result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Build with profile switch failed: {ex.Message}");
                return false;
            }
        }

        // --- Новый метод ---
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
                Debug.Log($"[BuildSystem] Found {dataFiles.Length} .data files for compression");

                foreach (var algo in config.WebGLSettings.CompressionAlgorithms)
                {
                    foreach (var dataFile in dataFiles)
                    {
                        if (algo == CompressionAlgorithm.Gzip)
                            RunExternalCompressor("gzip.exe", dataFile, ".gz", 9);
                        if (algo == CompressionAlgorithm.Brotli)
                            RunExternalCompressor("brotli.exe", dataFile, ".br", 11);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Compression error: {ex.Message}");
            }
        }

        private static void RunExternalCompressor(string exeName, string dataFile, string ext, int compressionLevel = 6)
        {
            try
            {
                string exePath = Path.Combine(Application.dataPath, "../Packages/io.energy8.build-deploy-system/Tools", exeName);
                if (!File.Exists(exePath))
                {
                    Debug.LogWarning($"[BuildSystem] Compressor not found: {exePath}");
                    return;
                }
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
                Debug.Log($"[BuildSystem] {exeName} output: {stdout}");
                if (!string.IsNullOrEmpty(stderr))
                    Debug.LogWarning($"[BuildSystem] {exeName} error: {stderr}");
                if (File.Exists(outputFile))
                    Debug.Log($"[BuildSystem] ✅ Compressed: {outputFile}");
                else
                    Debug.LogWarning($"[BuildSystem] Compression failed: {outputFile} not found");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildSystem] Failed to run compressor {exeName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Извлекает базовое имя приложения из имени файла, убирая суффикс формата текстур
        /// </summary>
        /// <param name="fileName">Имя файла без расширения (например: Assets_ASTC)</param>
        /// <param name="expectedAppName">Ожидаемое имя проекта (например: Assets)</param>
        /// <returns>Базовое имя приложения</returns>
        private static string ExtractBaseAppName(string fileName, string expectedAppName)
        {
            Debug.Log($"[BuildSystem] ExtractBaseAppName: fileName='{fileName}', expectedAppName='{expectedAppName}'");

            if (string.IsNullOrEmpty(fileName))
                return expectedAppName ?? "App";

            // Список известных суффиксов форматов текстур Unity
            string[] textureSuffixes = { "_DXT", "_ASTC", "_ETC2", "_PVRTC" };

            // Проверяем, оканчивается ли имя файла на один из суффиксов
            foreach (string suffix in textureSuffixes)
            {
                if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = fileName.Substring(0, fileName.Length - suffix.Length);
                    Debug.Log($"[BuildSystem] Found suffix '{suffix}', extracted base name: '{baseName}'");
                    return baseName;
                }
            }

            // Если суффикс не найден, возвращаем исходное имя
            Debug.Log($"[BuildSystem] No texture suffix found, using original name: '{fileName}'");
            return fileName;
        }

        private static void GenerateBuildJson(string outputPath)
        {
            string buildSubPath = Path.Combine(outputPath, "Build");
            if (!Directory.Exists(buildSubPath))
            {
                Debug.LogWarning($"[BuildSystem] Build subdirectory not found: {buildSubPath}");
                return;
            }

            // basename = последняя папка в buildSubPath
            string baseName = new DirectoryInfo(outputPath).Name;

            var template = new
            {
                wasm = new List<object>(),
                framework = new List<object>(),
                data = new List<object>(),
                symbols = new List<object>()
            };

            // --- wasm ---
            string wasmBase = baseName + ".wasm";
            var wasmFiles = new[] {
                ("none", wasmBase),
                ("gz", wasmBase + ".gz"),
                ("br", wasmBase + ".br")
            };
            foreach (var (compression, filename) in wasmFiles)
                if (File.Exists(Path.Combine(buildSubPath, filename)))
                    template.wasm.Add(new { compression, filename });

            // --- framework ---
            string frameworkBase = baseName + ".framework.js";
            var frameworkFiles = new[] {
                ("none", frameworkBase),
                ("gz", frameworkBase + ".gz"),
                ("br", frameworkBase + ".br")
            };
            foreach (var (compression, filename) in frameworkFiles)
                if (File.Exists(Path.Combine(buildSubPath, filename)))
                    template.framework.Add(new { compression, filename });

            // --- symbols ---
            string symbolsBase = baseName + ".symbols.json";
            var symbolsFiles = new[] {
                ("none", symbolsBase),
                ("gz", symbolsBase + ".gz"),
                ("br", symbolsBase + ".br")
            };
            foreach (var (compression, filename) in symbolsFiles)
                if (File.Exists(Path.Combine(buildSubPath, filename)))
                    template.symbols.Add(new { compression, filename });

            // --- data ---
            var formats = new[] { "dxt", "astc", "etc2" };
            foreach (var format in formats)
            {
                string baseDataName = baseName + $".{format}.data";
                var files = new[] {
                    ("none", baseDataName),
                    ("gz", baseDataName + ".gz"),
                    ("br", baseDataName + ".br")
                };
                foreach (var (compression, filename) in files)
                    if (File.Exists(Path.Combine(buildSubPath, filename)))
                        template.data.Add(new { format, compression, filename });
            }

            // --- Сохраняем JSON ---
            string json = JsonConvert.SerializeObject(template, Formatting.Indented);
            File.WriteAllText(Path.Combine(buildSubPath, "Build.json"), json);
            Debug.Log($"[BuildSystem] Build.json generated: {Path.Combine(buildSubPath, "Build.json")}");
        }
    }
}
