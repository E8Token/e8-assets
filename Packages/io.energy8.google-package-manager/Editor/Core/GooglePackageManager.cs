using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;
using Energy8.GooglePackageManager.Data;
using Energy8.GooglePackageManager.Utilities;
using Energy8.GooglePackageManager.UI;

namespace Energy8.GooglePackageManager.Core
{
    [InitializeOnLoad]
    public class GooglePackageManager
    {
        private static GooglePackageDatabase _packageDatabase;
        private static DateTime _lastUpdateCheck;
        private static bool _isUpdating = false;
        
        // Путь для сохранения локальной базы данных пакетов
        private static readonly string DatabasePath = Path.Combine("Library", "GooglePackageDatabase.json");
        
        static GooglePackageManager()
        {
            EditorApplication.delayCall += Initialize;
        }
        
        private static void Initialize()
        {
            LoadLocalDatabase();
            
            var settings = GooglePackageSettings.Instance;
            if (settings.enableStartupUpdateCheck)
            {
                EditorApplication.delayCall += async () => await CheckForUpdatesAsync(false);
            }
        }
        
        public static GooglePackageDatabase PackageDatabase
        {
            get
            {
                if (_packageDatabase == null)
                {
                    LoadLocalDatabase();
                }
                return _packageDatabase;
            }
        }
        
        public static async Task<bool> RefreshPackageDatabaseAsync()
        {
            if (_isUpdating)
            {
                Debug.LogWarning("Package database update already in progress");
                return false;
            }
            
            _isUpdating = true;
            
            try
            {
                var parser = new GooglePackageParser();
                _packageDatabase = await parser.ParsePackagesAsync();
                
                if (_packageDatabase != null && _packageDatabase.categories.Count > 0)
                {
                    // Обновляем статус установленных пакетов
                    ManifestManager.GetInstalledGooglePackages(_packageDatabase);
                    
                    // Сохраняем обновленную базу данных
                    SaveLocalDatabase();
                    _lastUpdateCheck = DateTime.Now;
                    
                    Debug.Log("Package database refreshed successfully");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to refresh package database");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error refreshing package database: {ex.Message}");
                return false;
            }
            finally
            {
                _isUpdating = false;
            }
        }
        
        public static async Task CheckForUpdatesAsync(bool showNoUpdatesMessage = true)
        {
            try
            {
                var settings = GooglePackageSettings.Instance;
                
                // Проверяем, нужно ли обновляться
                TimeSpan timeSinceLastCheck = DateTime.Now - _lastUpdateCheck;
                if (timeSinceLastCheck.TotalHours < settings.updateCheckIntervalHours && _packageDatabase != null)
                {
                    if (settings.enableDebugLogging)
                    {
                        Debug.Log($"Skipping update check, last check was {timeSinceLastCheck.TotalHours:F1} hours ago");
                    }
                    return;
                }
                
                bool success = await RefreshPackageDatabaseAsync();
                if (!success)
                {
                    return;
                }
                
                // Получаем список пакетов с обновлениями
                var packagesWithUpdates = GetPackagesWithUpdates();
                
                if (packagesWithUpdates.Count > 0 && settings.enableUpdateNotifications)
                {
                    string message = $"Found {packagesWithUpdates.Count} Google package(s) with available updates:\n\n";
                    foreach (var package in packagesWithUpdates)
                    {
                        message += $"• {package.displayName}: {package.installedVersion} → {package.version}\n";
                    }
                    message += "\nWould you like to open the Google Package Manager?";
                    
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorUtility.DisplayDialog("Package Updates Available", message, "Open Manager", "Later"))
                        {
                            GooglePackageManagerWindow.ShowWindow();
                        }
                    };
                }
                else if (showNoUpdatesMessage && settings.showUpdateNotifications)
                {
                    EditorApplication.delayCall += () =>
                    {
                        EditorUtility.DisplayDialog("No Updates", "All Google packages are up to date.", "OK");
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking for updates: {ex.Message}");
            }
        }
        
        public static List<GooglePackageInfo> GetPackagesWithUpdates()
        {
            var packagesWithUpdates = new List<GooglePackageInfo>();
            
            if (_packageDatabase == null)
                return packagesWithUpdates;
            
            foreach (var category in _packageDatabase.categories)
            {
                foreach (var package in category.packages)
                {
                    if (package.isInstalled && package.hasUpdate)
                    {
                        packagesWithUpdates.Add(package);
                    }
                }
            }
            
            return packagesWithUpdates;
        }
        
        public static List<GooglePackageInfo> GetInstalledPackages()
        {
            var installedPackages = new List<GooglePackageInfo>();
            
            if (_packageDatabase == null)
                return installedPackages;
            
            foreach (var category in _packageDatabase.categories)
            {
                foreach (var package in category.packages)
                {
                    if (package.isInstalled)
                    {
                        installedPackages.Add(package);
                    }
                }
            }
            
            return installedPackages;
        }
        
        public static async Task<bool> InstallPackageAsync(GooglePackageInfo packageInfo)
        {
            try
            {
                bool success = await GooglePackageDownloader.DownloadAndInstallPackageAsync(packageInfo);
                
                if (success)
                {
                    // Обновляем статус пакета
                    packageInfo.isInstalled = true;
                    packageInfo.installedVersion = packageInfo.version;
                    packageInfo.hasUpdate = false;
                    
                    // Сохраняем обновленную базу данных
                    SaveLocalDatabase();
                    
                    Debug.Log($"Successfully installed package: {packageInfo.displayName}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing package {packageInfo.displayName}: {ex.Message}");
                return false;
            }
        }
        
        public static bool ConfirmInstallPackage(GooglePackageInfo packageInfo)
        {
            var settings = GooglePackageSettings.Instance;
            
            if (settings.showInstallNotifications)
            {
                if (!EditorUtility.DisplayDialog("Install Package", 
                    $"Are you sure you want to install {packageInfo.displayName} version {packageInfo.version}?", 
                    "Install", "Cancel"))
                {
                    return false;
                }
            }
            
            // Если пакет уже установлен, предлагаем обновить
            if (packageInfo.isInstalled)
            {
                if (!EditorUtility.DisplayDialog("Update Package", 
                    $"{packageInfo.displayName} is already installed (version {packageInfo.installedVersion}).\n" +
                    $"Do you want to update it to version {packageInfo.version}?", 
                    "Update", "Cancel"))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public static async Task<bool> UninstallPackageAsync(GooglePackageInfo packageInfo)
        {
            try
            {
                if (!packageInfo.isInstalled)
                {
                    Debug.LogWarning($"Package {packageInfo.displayName} is not installed");
                    return false;
                }
                
                bool success = await GooglePackageDownloader.UninstallPackageAsync(packageInfo);
                
                if (success)
                {
                    // Обновляем статус пакета
                    packageInfo.isInstalled = false;
                    packageInfo.installedVersion = "";
                    packageInfo.hasUpdate = false;
                    
                    // Сохраняем обновленную базу данных
                    SaveLocalDatabase();
                    
                    Debug.Log($"Successfully uninstalled package: {packageInfo.displayName}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error uninstalling package {packageInfo.displayName}: {ex.Message}");
                return false;
            }
        }
        
        public static bool ConfirmUninstallPackage(GooglePackageInfo packageInfo)
        {
            if (!packageInfo.isInstalled)
            {
                return false;
            }
            
            return EditorUtility.DisplayDialog("Uninstall Package", 
                $"Are you sure you want to uninstall {packageInfo.displayName}?", 
                "Uninstall", "Cancel");
        }
        
        private static void LoadLocalDatabase()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", DatabasePath);
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    _packageDatabase = JsonUtility.FromJson<GooglePackageDatabase>(json);
                    
                    if (_packageDatabase != null)
                    {
                        _lastUpdateCheck = _packageDatabase.lastUpdateCheck;
                        
                        // Обновляем статус установленных пакетов
                        ManifestManager.GetInstalledGooglePackages(_packageDatabase);
                        
                        if (GooglePackageSettings.Instance.enableDebugLogging)
                        {
                            Debug.Log($"Loaded package database with {GetTotalPackagesCount()} packages");
                        }
                    }
                }
                else
                {
                    _packageDatabase = new GooglePackageDatabase();
                    _lastUpdateCheck = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading local package database: {ex.Message}");
                _packageDatabase = new GooglePackageDatabase();
                _lastUpdateCheck = DateTime.MinValue;
            }
        }
        
        private static void SaveLocalDatabase()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", DatabasePath);
                string directory = Path.GetDirectoryName(fullPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                _packageDatabase.lastUpdateCheck = DateTime.Now;
                string json = JsonUtility.ToJson(_packageDatabase, true);
                File.WriteAllText(fullPath, json);
                
                if (GooglePackageSettings.Instance.enableDebugLogging)
                {
                    Debug.Log("Package database saved successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving local package database: {ex.Message}");
            }
        }
        
        private static int GetTotalPackagesCount()
        {
            if (_packageDatabase == null)
                return 0;
            
            int count = 0;
            foreach (var category in _packageDatabase.categories)
            {
                count += category.packages.Count;
            }
            return count;
        }
        
        // Меню Unity
        [MenuItem("Energy8/Google Package Manager/Open Manager")]
        public static void OpenManager()
        {
            GooglePackageManagerWindow.ShowWindow();
        }
        
        [MenuItem("Energy8/Google Package Manager/Refresh Package List")]
        public static async void RefreshPackageList()
        {
            await RefreshPackageDatabaseAsync();
        }
        
        [MenuItem("Energy8/Google Package Manager/Check for Updates")]
        public static async void CheckForUpdatesMenu()
        {
            await CheckForUpdatesAsync(true);
        }
        
        [MenuItem("Energy8/Google Package Manager/Clear Download Cache")]
        public static void ClearDownloadCache()
        {
            GooglePackageDownloader.ClearDownloadCache();
        }
        
        [MenuItem("Energy8/Google Package Manager/Validate Package Files")]
        public static async void ValidatePackageFiles()
        {
            Debug.Log("Starting manual validation of Google package files...");
            bool allValid = await GooglePackageDownloader.ValidateInstalledTgzPackagesAsync();
            
            string message = allValid ? 
                "All Google package files are present and valid." : 
                "Some package files were missing and have been re-downloaded. Check the console for details.";
                
            EditorApplication.delayCall += () =>
            {
                EditorUtility.DisplayDialog("Package Validation", message, "OK");
            };
        }
        
        [MenuItem("Energy8/Google Package Manager/Cleanup Cache")]
        public static void CleanupCache()
        {
            if (EditorUtility.DisplayDialog("Cleanup Cache", 
                "This will remove orphaned .tgz files that are no longer referenced by any installed packages. Continue?", 
                "Yes", "Cancel"))
            {
                GooglePackageDownloader.CleanupOrphanedTgzFiles();
                EditorUtility.DisplayDialog("Cleanup Complete", "Cache cleanup completed.", "OK");
            }
        }
        
        [InitializeOnLoadMethod]
        private static async void OnEditorStartup()
        {
            // Ждем немного, чтобы Unity полностью загрузился
            await Task.Delay(2000);
            
            try
            {
                var settings = GooglePackageSettings.Instance;
                if (settings.validatePackagesOnStartup)
                {
                    Debug.Log("Validating installed Google packages on startup...");
                    await GooglePackageDownloader.ValidateInstalledTgzPackagesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during startup package validation: {ex.Message}");
            }
        }

        public static List<GooglePackageVersion> GetAvailableVersions(string packageName)
        {
            var database = PackageDatabase;
            if (database == null)
                return new List<GooglePackageVersion>();
            
            foreach (var category in database.categories)
            {
                foreach (var package in category.packages)
                {
                    if (package.packageName == packageName)
                    {
                        return package.availableVersions ?? new List<GooglePackageVersion>();
                    }
                }
            }
            
            return new List<GooglePackageVersion>();
        }
        
        public static async Task<bool> InstallSpecificVersionAsync(GooglePackageInfo packageInfo, GooglePackageVersion specificVersion)
        {
            try
            {
                // Создаем временный объект пакета с выбранной версией
                var tempPackageInfo = new GooglePackageInfo
                {
                    packageName = packageInfo.packageName,
                    displayName = packageInfo.displayName,
                    version = specificVersion.version,
                    downloadUrlTgz = specificVersion.downloadUrlTgz,
                    category = packageInfo.category
                };
                
                Debug.Log($"Installing specific version {specificVersion.version} of {packageInfo.displayName}");
                
                bool success = await GooglePackageDownloader.DownloadAndInstallPackageAsync(tempPackageInfo);
                
                if (success)
                {
                    // Обновляем статус пакета
                    packageInfo.isInstalled = true;
                    packageInfo.installedVersion = specificVersion.version;
                    packageInfo.hasUpdate = CompareVersions(packageInfo.version, specificVersion.version) > 0;
                    
                    // Сохраняем обновленную базу данных
                    SaveLocalDatabase();
                    
                    Debug.Log($"Successfully installed specific version {specificVersion.version} of {packageInfo.displayName}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing specific version {specificVersion.version} of {packageInfo.displayName}: {ex.Message}");
                return false;
            }
        }
        
        private static int CompareVersions(string version1, string version2)
        {
            try
            {
                var v1 = new Version(version1);
                var v2 = new Version(version2);
                return v1.CompareTo(v2);
            }
            catch
            {
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
