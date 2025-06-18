using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Energy8.GooglePackageManager.Data;
using Energy8.GooglePackageManager.Utilities;

namespace Energy8.GooglePackageManager.Core
{
    public class GooglePackageDownloader
    {
        private static readonly string DefaultCachePath = Path.Combine("Library", "GooglePackageCache");
        
        public static async Task<bool> DownloadAndInstallPackageAsync(GooglePackageInfo packageInfo)
        {
            try
            {
                var settings = GooglePackageSettings.Instance;
                // Определяем какой URL использовать
                string downloadUrl = GetBestDownloadUrl(packageInfo, settings);
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Debug.LogError($"No valid download URL found for package: {packageInfo.packageName}");
                    return false;
                }

                string cachePath = Path.Combine(Application.dataPath, "..", settings.downloadCachePath);

                // Создаем папку для кеша если не существует
                if (!Directory.Exists(cachePath))
                {
                    Directory.CreateDirectory(cachePath);
                }                // Всегда выводим URL для диагностики
                Debug.Log($"Downloading package {packageInfo.displayName} from: {downloadUrl}");

                // Обрабатываем registry URLs напрямую
                if (downloadUrl.StartsWith("registry:"))
                {
                    return await InstallRegistryPackageAsync(packageInfo, downloadUrl);
                }

                string fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
                string filePath = Path.Combine(cachePath, fileName);

                // Скачиваем файл
                bool downloadSuccess = await DownloadFileAsync(downloadUrl, filePath, settings.downloadTimeoutSeconds);

                if (!downloadSuccess)
                {
                    Debug.LogError($"Failed to download package: {packageInfo.displayName}");
                    return false;
                }

                // Устанавливаем пакет (только .tgz поддерживается)
                bool installSuccess = await InstallTgzPackageAsync(packageInfo, filePath);

                // .tgz файлы остаются в кеше, так как Unity ссылается на них
                if (settings.enableDebugLogging)
                {
                    Debug.Log($"Keeping .tgz file in cache as Unity references it: {filePath}");
                }

                if (installSuccess && settings.showSuccessNotifications)
                {
                    EditorApplication.delayCall += () =>
                    {
                        EditorUtility.DisplayDialog("Package Installed",
                            $"Successfully installed {packageInfo.displayName} version {packageInfo.version}", "OK");
                    };
                }

                return installSuccess;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error downloading/installing package {packageInfo.displayName}: {ex.Message}");
                return false;
            }
        }
          private static string GetBestDownloadUrl(GooglePackageInfo packageInfo, GooglePackageSettings settings)
        {
            // Проверяем различные типы URL-ов
            if (!string.IsNullOrEmpty(packageInfo.downloadUrlTgz))
            {
                string url = packageInfo.downloadUrlTgz;
                
                // Если это registry URL, возвращаем как есть
                if (url.StartsWith("registry:"))
                {
                    Debug.Log($"Using registry URL for {packageInfo.packageName}: {url}");
                    return url;
                }
                
                Debug.Log($"Using direct download URL for {packageInfo.packageName}: {url}");
                return url;
            }
            else
            {
                Debug.LogWarning($"Package {packageInfo.packageName} does not have a download URL available");
                return null;
            }
        }

        private static async Task<bool> DownloadFileAsync(string url, string filePath, int timeoutSeconds)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = timeoutSeconds;
                    request.downloadHandler = new DownloadHandlerFile(filePath);

                    var operation = request.SendWebRequest();

                    DateTime startTime = DateTime.Now;
                    while (!operation.isDone)
                    {
                        await Task.Delay(100);

                        // Проверяем таймаут
                        if ((DateTime.Now - startTime).TotalSeconds > timeoutSeconds)
                        {
                            Debug.LogError($"Download timeout for URL: {url}");
                            return false;
                        }

                        // Показываем прогресс
                        if (EditorUtility.DisplayCancelableProgressBar("Downloading Package",
                            $"Downloading: {Path.GetFileName(filePath)}", operation.progress))
                        {
                            Debug.Log("Download cancelled by user");
                            EditorUtility.ClearProgressBar();
                            return false;
                        }
                    }

                    EditorUtility.ClearProgressBar();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"Successfully downloaded: {filePath}");
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"Download failed: {request.error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Exception during download: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> InstallTgzPackageAsync(GooglePackageInfo packageInfo, string filePath)
        {
            try
            {
                string packageUrl;

                // Для локальных файлов используем file: URL
                string relativePath = GetRelativePathFromProject(filePath);
                packageUrl = $"file:{relativePath}";
                Debug.Log($"Installing package {packageInfo.packageName} from local file: {packageUrl}");

                // Обновляем или удаляем старую версию
                if (ManifestManager.IsPackageInstalled(packageInfo.packageName))
                {
                    Debug.Log($"Removing existing version of {packageInfo.packageName}");
                    ManifestManager.RemovePackage(packageInfo.packageName);
                    await Task.Delay(1000); // Даем время Unity обработать удаление
                }

                // Добавляем новую версию
                Debug.Log($"Adding package {packageInfo.packageName} version {packageInfo.version}");
                ManifestManager.AddPackage(packageInfo.packageName, packageUrl);

                Debug.Log($"Successfully added package to manifest: {packageInfo.displayName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing package: {ex.Message}");
                return false;
            }
        }
        
        private static async Task<bool> InstallRegistryPackageAsync(GooglePackageInfo packageInfo, string registryUrl)
        {
            try
            {
                // Извлекаем имя пакета и версию из registry URL
                // Формат: registry:packageName@version
                string packageSpec = registryUrl.Substring(9); // убираем "registry:"
                
                Debug.Log($"Installing package from registry: {packageSpec}");

                // Обновляем или удаляем старую версию
                if (ManifestManager.IsPackageInstalled(packageInfo.packageName))
                {
                    Debug.Log($"Removing existing version of {packageInfo.packageName}");
                    ManifestManager.RemovePackage(packageInfo.packageName);
                    await Task.Delay(1000); // Даем время Unity обработать удаление
                }

                // Добавляем новую версию через registry
                Debug.Log($"Adding package {packageInfo.packageName} from registry");
                ManifestManager.AddPackage(packageInfo.packageName, packageSpec);

                Debug.Log($"Successfully added registry package: {packageInfo.displayName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing registry package: {ex.Message}");
                return false;
            }
        }
        
        private static string GetRelativePathFromProject(string absolutePath)
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            if (absolutePath.StartsWith(projectPath))
            {
                return absolutePath.Substring(projectPath.Length + 1).Replace('\\', '/');
            }
            return absolutePath.Replace('\\', '/');
        }

        public static async Task<bool> UninstallPackageAsync(GooglePackageInfo packageInfo)
        {
            try
            {
                if (!ManifestManager.IsPackageInstalled(packageInfo.packageName))
                {
                    Debug.LogWarning($"Package {packageInfo.packageName} is not installed");
                    return false;
                }

                ManifestManager.RemovePackage(packageInfo.packageName);

                // Даем время Unity обработать изменения
                await Task.Delay(1000);

                Debug.Log($"Uninstalled package: {packageInfo.displayName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error uninstalling package {packageInfo.displayName}: {ex.Message}");
                return false;
            }
        }

        public static void ClearDownloadCache()
        {
            try
            {
                var settings = GooglePackageSettings.Instance;
                string cachePath = Path.Combine(Application.dataPath, "..", settings.downloadCachePath);

                if (Directory.Exists(cachePath))
                {
                    Directory.Delete(cachePath, true);
                    Debug.Log("Download cache cleared");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error clearing download cache: {ex.Message}");
            }
        }

        public static async Task<bool> ValidateInstalledTgzPackagesAsync()
        {
            try
            {
                var settings = GooglePackageSettings.Instance;
                string cachePath = Path.Combine(Application.dataPath, "..", settings.downloadCachePath);

                var database = GooglePackageManager.PackageDatabase;
                if (database == null || database.categories == null)
                {
                    Debug.Log("No package database available for validation");
                    return true;
                }

                var missingPackages = new List<GooglePackageInfo>();

                // Проверяем все установленные пакеты
                var manifest = ManifestManager.ReadManifest();

                foreach (var category in database.categories)
                {
                    foreach (var package in category.packages)
                    {
                        if (package == null || string.IsNullOrEmpty(package.packageName)) continue;

                        // Проверяем только установленные пакеты
                        if (manifest.dependencies.ContainsKey(package.packageName))
                        {
                            string dependencyUrl = manifest.dependencies[package.packageName];

                            // Если это file: URL, проверяем наличие файла
                            if (dependencyUrl.StartsWith("file:"))
                            {
                                string relativePath = dependencyUrl.Substring(5); // убираем "file:"
                                string fullPath = Path.Combine(Application.dataPath, "..", relativePath);

                                if (!File.Exists(fullPath))
                                {
                                    Debug.LogWarning($"Missing .tgz file for package {package.packageName}: {fullPath}");
                                    missingPackages.Add(package);
                                }
                                else if (settings.enableDebugLogging)
                                {
                                    Debug.Log($"Validated .tgz file for package {package.packageName}: {fullPath}");
                                }
                            }
                        }
                    }
                }

                // Скачиваем отсутствующие пакеты
                if (missingPackages.Count > 0)
                {
                    Debug.Log($"Found {missingPackages.Count} missing .tgz packages, re-downloading...");

                    foreach (var package in missingPackages)
                    {
                        try
                        {
                            Debug.Log($"Re-downloading missing package: {package.displayName}");
                            bool success = await DownloadAndInstallPackageAsync(package);
                            if (!success)
                            {
                                Debug.LogError($"Failed to re-download package: {package.displayName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error re-downloading package {package.displayName}: {ex.Message}");
                        }
                    }
                }
                else if (settings.enableDebugLogging)
                {
                    Debug.Log("All installed .tgz packages are present and validated");
                }

                return missingPackages.Count == 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error validating .tgz packages: {ex.Message}");
                return false;
            }
        }

        public static void CleanupOrphanedTgzFiles()
        {
            try
            {
                var settings = GooglePackageSettings.Instance;
                string cachePath = Path.Combine(Application.dataPath, "..", settings.downloadCachePath);

                if (!Directory.Exists(cachePath))
                    return;

                var manifest = ManifestManager.ReadManifest();
                var referencedFiles = new HashSet<string>();

                // Собираем все файлы, на которые ссылается manifest
                foreach (var dependency in manifest.dependencies)
                {
                    if (dependency.Value.StartsWith("file:"))
                    {
                        string relativePath = dependency.Value.Substring(5);
                        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
                        referencedFiles.Add(fullPath);
                    }
                }

                // Ищем .tgz файлы в кеше, которые не используются
                var cacheFiles = Directory.GetFiles(cachePath, "*.tgz", SearchOption.AllDirectories);
                foreach (string file in cacheFiles)
                {
                    string fullPath = Path.GetFullPath(file);
                    if (!referencedFiles.Contains(fullPath))
                    {
                        try
                        {
                            File.Delete(file);
                            if (settings.enableDebugLogging)
                            {
                                Debug.Log($"Removed orphaned .tgz file: {file}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Failed to delete orphaned file {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error cleaning up orphaned .tgz files: {ex.Message}");
            }
        }
    }
}
