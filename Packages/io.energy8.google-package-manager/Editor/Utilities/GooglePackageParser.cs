using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Energy8.GooglePackageManager.Data;

namespace Energy8.GooglePackageManager.Utilities
{
    public class GooglePackageParser
    {
        private const string GOOGLE_PACKAGES_URL = "https://developers.google.com/unity/packages";
          public async Task<GooglePackageDatabase> ParsePackagesAsync()
        {
            try
            {
                Debug.Log("Starting to parse Google Unity Packages...");
                var database = new GooglePackageDatabase();
                
                // Загружаем HTML контент
                string htmlContent = await DownloadWebPageAsync(GOOGLE_PACKAGES_URL);
                
                if (string.IsNullOrEmpty(htmlContent))
                {
                    Debug.LogError("Failed to download Google Unity Packages page");
                    return database;
                }
                
                Debug.Log($"Downloaded webpage content, length: {htmlContent.Length} characters");
                
                // Парсим категории и пакеты
                ParseCategories(htmlContent, database);
                
                database.lastUpdateCheck = DateTime.Now;
                
                Debug.Log($"Successfully parsed {GetTotalPackagesCount(database)} packages from Google Unity Packages");
                return database;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing Google Unity Packages: {ex.Message}");
                return new GooglePackageDatabase();
            }
        }
        
        private async Task<string> DownloadWebPageAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"Failed to download {url}: {request.error}");
                    return null;
                }
            }
        }
        
        private void ParseCategories(string htmlContent, GooglePackageDatabase database)
        {
            // Firebase пакеты
            var firebaseCategory = new GooglePackageCategory("firebase", "Firebase");
            ParseFirebasePackages(htmlContent, firebaseCategory);
            if (firebaseCategory.packages.Count > 0)
                database.categories.Add(firebaseCategory);
            
            // Android пакеты
            var androidCategory = new GooglePackageCategory("android", "Android");
            ParseAndroidPackages(htmlContent, androidCategory);
            if (androidCategory.packages.Count > 0)
                database.categories.Add(androidCategory);
            
            // Google Play пакеты
            var playCategory = new GooglePackageCategory("googleplay", "Google Play");
            ParseGooglePlayPackages(htmlContent, playCategory);
            if (playCategory.packages.Count > 0)
                database.categories.Add(playCategory);
            
            // AR пакеты
            var arCategory = new GooglePackageCategory("ar", "AR");
            ParseARPackages(htmlContent, arCategory);
            if (arCategory.packages.Count > 0)
                database.categories.Add(arCategory);
            
            // Tools пакеты
            var toolsCategory = new GooglePackageCategory("tools", "Tools");
            ParseToolsPackages(htmlContent, toolsCategory);
            if (toolsCategory.packages.Count > 0)
                database.categories.Add(toolsCategory);
            
            // Advertising пакеты
            var adCategory = new GooglePackageCategory("advertising", "Advertising");
            ParseAdvertisingPackages(htmlContent, adCategory);
            if (adCategory.packages.Count > 0)
                database.categories.Add(adCategory);
        }
        
        private void ParseFirebasePackages(string htmlContent, GooglePackageCategory category)
        {
            // Firebase пакеты имеют стандартный формат
            var firebasePackages = new Dictionary<string, string>
            {
                {"com.google.firebase.analytics", "Google Analytics"},
                {"com.google.firebase.auth", "Firebase Authentication"},
                {"com.google.firebase.database", "Firebase Realtime Database"},
                {"com.google.firebase.firestore", "Cloud Firestore"},
                {"com.google.firebase.functions", "Cloud Functions for Firebase"},
                {"com.google.firebase.storage", "Cloud Storage for Firebase"},
                {"com.google.firebase.messaging", "Firebase Cloud Messaging"},
                {"com.google.firebase.crashlytics", "Firebase Crashlytics"},
                {"com.google.firebase.remote-config", "Firebase Remote Config"},
                {"com.google.firebase.dynamic-links", "Firebase Dynamic Links"},
                {"com.google.firebase.installations", "Firebase Installations"},
                {"com.google.firebase.app-check", "Firebase App Check"},
                {"com.google.firebase.firebaseai", "Firebase AI Logic"}
            };
            
            foreach (var package in firebasePackages)
            {
                var packageInfo = ParseFirebasePackageInfo(htmlContent, package.Key, package.Value);
                if (packageInfo != null)
                {
                    category.packages.Add(packageInfo);
                }
            }
        }        private GooglePackageInfo ParseFirebasePackageInfo(string htmlContent, string packageName, string displayName)
        {
            try
            {
                Debug.Log($"Parsing Firebase package: {packageName} ({displayName})");
                
                var packageInfo = new GooglePackageInfo
                {
                    packageName = packageName,
                    displayName = displayName,
                    category = "Firebase",
                    availableVersions = new List<GooglePackageVersion>()
                };
                
                // Пытаемся парсить таблицу версий (если есть)
                ParsePackageVersionTable(htmlContent, packageInfo);
                
                // Если не удалось найти версии, создаем версии на основе известных данных
                if (packageInfo.availableVersions.Count == 0)
                {
                    CreateDefaultFirebaseVersions(packageInfo);
                }
                
                if (packageInfo.availableVersions.Count > 0)
                {
                    // Берем самую новую версию как основную
                    var latestVersion = packageInfo.availableVersions[0];
                    packageInfo.version = latestVersion.version;
                    packageInfo.minimumUnityVersion = latestVersion.minimumUnityVersion;
                    packageInfo.publishDate = latestVersion.publishDate;
                    packageInfo.downloadUrlTgz = latestVersion.downloadUrlTgz;
                    
                    Debug.Log($"Package {packageName} parsed with {packageInfo.availableVersions.Count} versions, latest: {packageInfo.version}");
                }
                
                Debug.Log($"Generated download URL for {packageName}: {packageInfo.downloadUrlTgz}");
                
                return packageInfo;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse Firebase package {packageName}: {ex.Message}");
                return null;
            }
        }
        
        private void CreateDefaultFirebaseVersions(GooglePackageInfo packageInfo)
        {
            try
            {
                // Создаем несколько известных стабильных версий Firebase
                var stableVersions = new string[] { "12.3.0", "12.2.0", "12.1.0", "11.9.0", "11.8.0" };
                
                foreach (var ver in stableVersions)
                {
                    var version = new GooglePackageVersion
                    {
                        version = ver,
                        publishDate = "2025-06",
                        minimumUnityVersion = "2020.3",
                        dependencies = "None"
                    };
                    
                    // Генерируем ссылки для Firebase
                    GenerateFirebaseDownloadLinks(packageInfo.packageName, version);
                    
                    packageInfo.availableVersions.Add(version);
                }
                
                Debug.Log($"Created {packageInfo.availableVersions.Count} default versions for {packageInfo.packageName}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error creating default Firebase versions: {ex.Message}");
            }
        }
        
        private string GetFirebasePackageFileName(string packageName)
        {
            var packageMap = new Dictionary<string, string>
            {
                {"com.google.firebase.analytics", "FirebaseAnalytics"},
                {"com.google.firebase.auth", "FirebaseAuth"},
                {"com.google.firebase.database", "FirebaseDatabase"},
                {"com.google.firebase.firestore", "FirebaseFirestore"},
                {"com.google.firebase.functions", "FirebaseFunctions"},
                {"com.google.firebase.storage", "FirebaseStorage"},
                {"com.google.firebase.messaging", "FirebaseMessaging"},
                {"com.google.firebase.crashlytics", "FirebaseCrashlytics"},
                {"com.google.firebase.remote-config", "FirebaseRemoteConfig"},
                {"com.google.firebase.dynamic-links", "FirebaseDynamicLinks"},
                {"com.google.firebase.installations", "FirebaseInstallations"},
                {"com.google.firebase.app-check", "FirebaseAppCheck"},
                {"com.google.firebase.firebaseai", "FirebaseAI"}
            };
            
            return packageMap.TryGetValue(packageName, out string fileName) ? fileName : "Firebase";
        }
        
        private string GetFirebasePackagePath(string packageName)
        {
            var pathMap = new Dictionary<string, string>
            {
                {"com.google.firebase.analytics", "analytics"},
                {"com.google.firebase.auth", "auth"},
                {"com.google.firebase.crashlytics", "crashlytics"},
                {"com.google.firebase.database", "database"},
                {"com.google.firebase.firestore", "firestore"},
                {"com.google.firebase.functions", "functions"},
                {"com.google.firebase.messaging", "messaging"},
                {"com.google.firebase.remote-config", "remote-config"},
                {"com.google.firebase.storage", "storage"},
                {"com.google.firebase.app", "app"}
            };
            
            return pathMap.ContainsKey(packageName) ? pathMap[packageName] : packageName.Replace("com.google.firebase.", "");
        }
          private bool IsPackageAvailableInRegistry(string packageName)
        {
            // Firebase пакеты которые доступны через официальный Unity Registry
            var registryPackages = new HashSet<string>
            {
                "com.google.firebase.app",
                "com.google.firebase.analytics",
                "com.google.firebase.auth",
                "com.google.firebase.firestore",
                "com.google.firebase.functions",
                "com.google.firebase.messaging",
                "com.google.firebase.storage",
                "com.google.firebase.database",
                "com.google.firebase.crashlytics",
                "com.google.firebase.remote-config"
            };
            
            bool isAvailable = registryPackages.Contains(packageName);
            Debug.Log($"Package {packageName} registry availability: {isAvailable}");
            return isAvailable;
        }
        
        private void ParseAndroidPackages(string htmlContent, GooglePackageCategory category)
        {
            // Android Performance Tuner
            var perfTuner = new GooglePackageInfo
            {
                packageName = "com.google.android.performancetuner",
                displayName = "Android Performance Tuner",
                category = "Android",
                version = "1.1.2",
                minimumUnityVersion = "2017.4",
                publishDate = "2021-05",
                downloadUrlTgz = GenerateNonFirebaseArchiveUrl("com.google.android.performancetuner", "1.1.2")
            };
            category.packages.Add(perfTuner);
        }
        
        private void ParseGooglePlayPackages(string htmlContent, GooglePackageCategory category)
        {
            // Play Instant
            var playInstant = new GooglePackageInfo
            {
                packageName = "com.google.play.instant",
                displayName = "Play Instant",
                category = "Google Play",
                version = "1.8.0",
                minimumUnityVersion = "2019.1",
                publishDate = "2024-03",
                downloadUrlTgz = GenerateNonFirebaseArchiveUrl("com.google.play.instant", "1.8.0")
            };
            category.packages.Add(playInstant);
            
            // Input SDK
            var inputSdk = new GooglePackageInfo
            {
                packageName = "com.google.play.inputmapping",
                displayName = "Input SDK",
                category = "Google Play",
                version = "1.0.0-beta",
                minimumUnityVersion = "2019.1",
                publishDate = "2024-03",
                downloadUrlTgz = GenerateNonFirebaseArchiveUrl("com.google.play.inputmapping", "1.0.0-beta")
            };
            category.packages.Add(inputSdk);
        }
        
        private void ParseARPackages(string htmlContent, GooglePackageCategory category)
        {
            // ARCore Extensions
            var arCore = new GooglePackageInfo
            {
                packageName = "com.google.ar.core.arfoundation.extensions-lite",
                displayName = "ARCore Extensions for AR Foundation",
                category = "AR",
                version = "1.30.0",
                minimumUnityVersion = "2019.4",
                publishDate = "2022-03",
                downloadUrlTgz = "https://github.com/google-ar/arcore-unity-extensions/releases/download/v1.30.0/arcore-unity-extension-1.30.0-without-edm4u.tgz"
            };
            category.packages.Add(arCore);
        }
          private void ParseToolsPackages(string htmlContent, GooglePackageCategory category)
        {
            // External Dependency Manager - парсим реальные данные
            var edm = new GooglePackageInfo
            {
                packageName = "com.google.external-dependency-manager",
                displayName = "External Dependency Manager for Unity",
                category = "Tools",
                availableVersions = new List<GooglePackageVersion>()
            };
            
            // Парсим таблицу версий для EDM
            ParsePackageVersionTable(htmlContent, edm);
            
            if (edm.availableVersions.Count > 0)
            {
                // Берем самую новую версию
                var latestVersion = edm.availableVersions[0];
                edm.version = latestVersion.version;
                edm.minimumUnityVersion = latestVersion.minimumUnityVersion;
                edm.publishDate = latestVersion.publishDate;
                edm.downloadUrlTgz = latestVersion.downloadUrlTgz;
                
                Debug.Log($"EDM parsed with {edm.availableVersions.Count} versions, latest: {edm.version}");
            }
            else
            {
                // Fallback данные
                edm.version = "1.2.186";
                edm.minimumUnityVersion = "2019.1";
                edm.publishDate = "2025-05";
                edm.downloadUrlTgz = "https://dl.google.com/games/registry/unity/com.google.external-dependency-manager/com.google.external-dependency-manager-1.2.186.tgz";
                
                Debug.LogWarning("Failed to parse EDM versions, using fallback data");
            }
            
            category.packages.Add(edm);
        }
        
        private void ParseAdvertisingPackages(string htmlContent, GooglePackageCategory category)
        {
            // В данный момент AdMob не имеет прямых ссылок на странице, пропускаем
        }
        
        private int GetTotalPackagesCount(GooglePackageDatabase database)
        {
            int count = 0;
            foreach (var category in database.categories)
            {
                count += category.packages.Count;
            }
            return count;
        }        private string GenerateGoogleArchiveUrl(string packageName, string version)
        {
            // Для Firebase пакетов - используем официальную UPM registry или известные рабочие URLs
            if (packageName.StartsWith("com.google.firebase."))
            {
                string stableVersion = GetStableVersionWithTgz(packageName, version);
                
                // Попробуем использовать официальный Unity Package Manager registry
                // Большинство Firebase пакетов теперь доступны через official registry
                if (IsPackageAvailableInRegistry(packageName))
                {
                    Debug.Log($"Using UPM registry for Firebase package {packageName}");
                    return $"registry:{packageName}@{stableVersion}";
                }
                
                // Fallback: пытаемся найти .tgz версии в известных местах
                // Попробуем несколько альтернативных URL структур
                string[] fallbackUrls = {
                    // Официальный CDN Google
                    $"https://dl.google.com/firebase/sdk/unity/{stableVersion}/firebase_unity_sdk.zip",
                    // GitHub releases (альтернативный формат)
                    $"https://github.com/firebase/firebase-unity-sdk/releases/download/{stableVersion}/firebase_unity_sdk_{stableVersion}.zip",
                    // Прямая ссылка на .unitypackage (если .tgz недоступен)
                    $"https://github.com/firebase/firebase-unity-sdk/releases/download/{stableVersion}/FirebaseAnalytics.unitypackage"
                };
                
                string selectedUrl = fallbackUrls[0]; // Используем первый URL
                
                Debug.Log($"Firebase package {packageName} not found in registry, using fallback: {selectedUrl}");
                Debug.Log($"Alternative fallback URLs: {string.Join("; ", fallbackUrls.Skip(1))}");
                Debug.LogWarning($"Note: {packageName} may require manual installation from Firebase Console");
                
                return selectedUrl;
            }
            
            // Для других Google пакетов
            return GenerateNonFirebaseArchiveUrl(packageName, version);
        }
          private string GetStableVersionWithTgz(string packageName, string requestedVersion)
        {
            // Мапинг пакетов к последним стабильным версиям с .tgz поддержкой
            var stableVersions = new Dictionary<string, string>
            {
                {"com.google.firebase.analytics", "12.3.0"},
                {"com.google.firebase.auth", "12.3.0"},
                {"com.google.firebase.app", "12.3.0"},
                {"com.google.firebase.firestore", "12.3.0"},
                {"com.google.firebase.functions", "12.3.0"},
                {"com.google.firebase.storage", "12.3.0"},
                {"com.google.firebase.database", "12.3.0"},
                {"com.google.firebase.messaging", "12.3.0"},
                {"com.google.firebase.crashlytics", "12.3.0"},
                {"com.google.firebase.remote-config", "12.3.0"},
                {"com.google.firebase.dynamic-links", "12.3.0"},
                {"com.google.firebase.installations", "12.3.0"}
            };
            
            string version = stableVersions.ContainsKey(packageName) ? stableVersions[packageName] : "12.3.0";
            Debug.Log($"Using stable version {version} for package {packageName}");
            return version;
        }
        
        private string GenerateNonFirebaseArchiveUrl(string packageName, string version)
        {
            string baseUrl = "https://dl.google.com/games/registry/unity";
            
            if (packageName.StartsWith("com.google.android."))
            {
                return $"{baseUrl}/{packageName}/{packageName}-{version}.tgz";
            }
            else if (packageName.StartsWith("com.google.play."))
            {
                return $"{baseUrl}/{packageName}/{packageName}-{version}.tgz";
            }
            else if (packageName == "com.google.external-dependency-manager")
            {
                // External Dependency Manager как .tgz
                return $"{baseUrl}/{packageName}/{packageName}-{version}.tgz";
            }
            
            // Fallback для всех остальных пакетов
            return $"{baseUrl}/{packageName}/{packageName}-{version}.tgz";
        }        private void ParsePackageVersionTable(string htmlContent, GooglePackageInfo packageInfo)
        {
            try
            {
                Debug.Log($"Starting to parse version table for {packageInfo.packageName}");
                
                // Ищем таблицы с версиями по характерным признакам
                string[] tableSections = htmlContent.Split(new string[] { "<table" }, StringSplitOptions.RemoveEmptyEntries);
                
                Debug.Log($"Found {tableSections.Length} table sections in HTML content");
                
                foreach (string section in tableSections)
                {
                    if (section.Contains("Version") && section.Contains("Download") && section.Contains("Minimum Unity"))
                    {
                        Debug.Log($"Found version table for {packageInfo.packageName}, parsing rows...");
                        ParseVersionRowsSimple(section, packageInfo);
                        
                        if (packageInfo.availableVersions.Count > 0)
                        {
                            Debug.Log($"Successfully parsed {packageInfo.availableVersions.Count} versions");
                            break;
                        }
                    }
                }
                
                Debug.Log($"Final result: Parsed {packageInfo.availableVersions.Count} versions for {packageInfo.packageName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing version table for {packageInfo.packageName}: {ex.Message}");
            }
        }
        
        private void ParseVersionRowsSimple(string tableSection, GooglePackageInfo packageInfo)
        {
            try
            {
                // Разбиваем по строкам таблицы
                string[] rows = tableSection.Split(new string[] { "<tr" }, StringSplitOptions.RemoveEmptyEntries);
                
                int validRowCount = 0;
                foreach (string row in rows)
                {
                    // Пропускаем заголовочные строки
                    if (row.Contains("<th") || row.Contains("Version") && row.Contains("Download"))
                        continue;
                    
                    // Ищем строки с версиями (содержат номер версии в формате X.X.X)
                    if (Regex.IsMatch(row, @"\b\d+\.\d+\.\d+\b"))
                    {
                        var version = ExtractVersionFromRow(row);
                        if (version != null && !string.IsNullOrEmpty(version.version))
                        {
                            packageInfo.availableVersions.Add(version);
                            validRowCount++;
                            Debug.Log($"Extracted version: {version.version} | Date: {version.publishDate} | Unity: {version.minimumUnityVersion} | Has TGZ: {!string.IsNullOrEmpty(version.downloadUrlTgz)}");
                        }
                    }
                }
                
                Debug.Log($"Processed {rows.Length} row sections, extracted {validRowCount} valid versions");
                
                // Сортируем версии по убыванию
                packageInfo.availableVersions.Sort((v1, v2) => 
                {
                    try
                    {
                        var ver1 = new Version(v1.version);
                        var ver2 = new Version(v2.version);
                        return ver2.CompareTo(ver1);
                    }
                    catch
                    {
                        return string.Compare(v2.version, v1.version, StringComparison.OrdinalIgnoreCase);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing version rows: {ex.Message}");
            }
        }
        
        private GooglePackageVersion ExtractVersionFromRow(string rowHtml)
        {
            try
            {
                var version = new GooglePackageVersion();
                
                // Извлекаем номер версии
                var versionMatch = Regex.Match(rowHtml, @"\b(\d+\.\d+\.\d+)\b");
                if (!versionMatch.Success)
                    return null;
                
                version.version = versionMatch.Groups[1].Value;
                
                // Извлекаем дату публикации (формат: 2025-05)
                var dateMatch = Regex.Match(rowHtml, @"\b(\d{4}-\d{2})\b");
                if (dateMatch.Success)
                    version.publishDate = dateMatch.Groups[1].Value;
                
                // Извлекаем минимальную версию Unity (формат: 2019.1)
                var unityMatch = Regex.Match(rowHtml, @"\b(\d{4}\.\d+)\b");
                if (unityMatch.Success)
                    version.minimumUnityVersion = unityMatch.Groups[1].Value;
                
                // Извлекаем ссылки на скачивание
                ExtractDownloadLinksSimple(rowHtml, version);
                
                return version;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error extracting version from row: {ex.Message}");
                return null;
            }
        }
          private void ExtractDownloadLinksSimple(string rowHtml, GooglePackageVersion version)
        {
            try
            {
                // Генерируем ссылки на основе известных паттернов Google
                if (!string.IsNullOrEmpty(version.version))
                {
                    // Для External Dependency Manager - известный паттерн ссылок
                    string baseUrl = "https://dl.google.com/games/registry/unity/com.google.external-dependency-manager";
                    version.downloadUrlTgz = $"{baseUrl}/com.google.external-dependency-manager-{version.version}.tgz";
                    
                    // GitHub ссылка на .unitypackage
                    string githubUrl = "https://github.com/googlesamples/unity-jar-resolver/raw";
                    version.downloadUrlUnityPackage = $"{githubUrl}/v{version.version}/external-dependency-manager-{version.version}.unitypackage";
                    
                    Debug.Log($"Generated URLs for version {version.version}:");
                    Debug.Log($"  TGZ: {version.downloadUrlTgz}");
                    Debug.Log($"  UnityPackage: {version.downloadUrlUnityPackage}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error generating download links: {ex.Message}");
            }
        }
        
        private void GenerateFirebaseDownloadLinks(string packageName, GooglePackageVersion version)
        {
            try
            {
                // Для Firebase пакетов используем разные стратегии
                if (packageName.StartsWith("com.google.firebase."))
                {
                    // Пытаемся использовать registry если доступен
                    if (IsPackageAvailableInRegistry(packageName))
                    {
                        version.downloadUrlTgz = $"registry:{packageName}@{version.version}";
                        Debug.Log($"Firebase package {packageName} will use registry");
                    }
                    else
                    {
                        // Fallback на известные ссылки GitHub или CDN
                        version.downloadUrlTgz = $"https://github.com/firebase/firebase-unity-sdk/releases/download/{version.version}/firebase_unity_sdk_{version.version}.zip";
                        Debug.Log($"Firebase package {packageName} will use GitHub fallback");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error generating Firebase download links: {ex.Message}");
            }
        }// Удаляем старые методы - заменены на более простые выше
    }
}
