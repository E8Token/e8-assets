using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Energy8.GooglePackageManager.Data;

namespace Energy8.GooglePackageManager.Utilities
{
    public class ManifestManager
    {
        private static readonly string ManifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        
        [Serializable]
        public class ManifestData
        {
            public Dictionary<string, string> dependencies = new Dictionary<string, string>();
            public List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();
        }
        
        [Serializable]
        public class ScopedRegistry
        {
            public string name;
            public string url;
            public List<string> scopes = new List<string>();
            public bool overrideBuiltIns;
        }
        
        public static ManifestData ReadManifest()
        {
            try
            {
                if (!File.Exists(ManifestPath))
                {
                    Debug.LogError($"Manifest file not found at: {ManifestPath}");
                    return new ManifestData();
                }
                
                string jsonContent = File.ReadAllText(ManifestPath);
                var jsonObject = JObject.Parse(jsonContent);
                
                var manifest = new ManifestData();
                
                // Читаем dependencies
                if (jsonObject["dependencies"] is JObject deps)
                {
                    foreach (var dep in deps)
                    {
                        manifest.dependencies[dep.Key] = dep.Value.ToString();
                    }
                }
                
                // Читаем scopedRegistries
                if (jsonObject["scopedRegistries"] is JArray registries)
                {
                    foreach (var registry in registries)
                    {
                        var scopedRegistry = new ScopedRegistry
                        {
                            name = registry["name"]?.ToString() ?? "",
                            url = registry["url"]?.ToString() ?? "",
                            overrideBuiltIns = registry["overrideBuiltIns"]?.ToObject<bool>() ?? false
                        };
                        
                        if (registry["scopes"] is JArray scopes)
                        {
                            foreach (var scope in scopes)
                            {
                                scopedRegistry.scopes.Add(scope.ToString());
                            }
                        }
                        
                        manifest.scopedRegistries.Add(scopedRegistry);
                    }
                }
                
                return manifest;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading manifest: {ex.Message}");
                return new ManifestData();
            }
        }
        
        public static void WriteManifest(ManifestData manifest)
        {
            try
            {
                var jsonObject = new JObject();
                
                // Записываем dependencies
                var depsObject = new JObject();
                foreach (var dep in manifest.dependencies)
                {
                    depsObject[dep.Key] = dep.Value;
                }
                jsonObject["dependencies"] = depsObject;
                
                // Записываем scopedRegistries
                if (manifest.scopedRegistries.Count > 0)
                {
                    var registriesArray = new JArray();
                    foreach (var registry in manifest.scopedRegistries)
                    {
                        var registryObject = new JObject
                        {
                            ["name"] = registry.name,
                            ["url"] = registry.url,
                            ["scopes"] = new JArray(registry.scopes),
                            ["overrideBuiltIns"] = registry.overrideBuiltIns
                        };
                        registriesArray.Add(registryObject);
                    }
                    jsonObject["scopedRegistries"] = registriesArray;
                }
                
                string jsonContent = jsonObject.ToString(Formatting.Indented);
                File.WriteAllText(ManifestPath, jsonContent);
                
                // Обновляем Asset Database
                AssetDatabase.Refresh();
                
                Debug.Log("Manifest updated successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing manifest: {ex.Message}");
            }
        }
        
        public static bool IsPackageInstalled(string packageName)
        {
            var manifest = ReadManifest();
            return manifest.dependencies.ContainsKey(packageName);
        }
        
        public static string GetInstalledPackageVersion(string packageName)
        {
            var manifest = ReadManifest();
            return manifest.dependencies.TryGetValue(packageName, out string version) ? version : null;
        }
        
        public static void AddPackage(string packageName, string version)
        {
            var manifest = ReadManifest();
            manifest.dependencies[packageName] = version;
            WriteManifest(manifest);
        }
        
        public static void RemovePackage(string packageName)
        {
            var manifest = ReadManifest();
            if (manifest.dependencies.ContainsKey(packageName))
            {
                manifest.dependencies.Remove(packageName);
                WriteManifest(manifest);
                Debug.Log($"Removed package: {packageName}");
            }
        }
        
        public static void UpdatePackage(string packageName, string newVersion)
        {
            var manifest = ReadManifest();
            if (manifest.dependencies.ContainsKey(packageName))
            {
                string oldVersion = manifest.dependencies[packageName];
                manifest.dependencies[packageName] = newVersion;
                WriteManifest(manifest);
                Debug.Log($"Updated package {packageName} from {oldVersion} to {newVersion}");
            }
            else
            {
                AddPackage(packageName, newVersion);
                Debug.Log($"Added new package: {packageName} version {newVersion}");
            }
        }
        
        public static void AddScopedRegistry(string name, string url, List<string> scopes, bool overrideBuiltIns = false)
        {
            var manifest = ReadManifest();
            
            // Проверяем, существует ли уже такой registry
            var existingRegistry = manifest.scopedRegistries.Find(r => r.name == name || r.url == url);
            if (existingRegistry != null)
            {
                // Обновляем существующий
                existingRegistry.scopes = scopes;
                existingRegistry.overrideBuiltIns = overrideBuiltIns;
            }
            else
            {
                // Добавляем новый
                manifest.scopedRegistries.Add(new ScopedRegistry
                {
                    name = name,
                    url = url,
                    scopes = scopes,
                    overrideBuiltIns = overrideBuiltIns
                });
            }
            
            WriteManifest(manifest);
            Debug.Log($"Added/Updated scoped registry: {name}");
        }
        
        public static List<GooglePackageInfo> GetInstalledGooglePackages(GooglePackageDatabase database)
        {
            var installedPackages = new List<GooglePackageInfo>();
            var manifest = ReadManifest();
            
            foreach (var category in database.categories)
            {
                foreach (var package in category.packages)
                {
                    if (manifest.dependencies.ContainsKey(package.packageName))
                    {
                        package.isInstalled = true;
                        package.installedVersion = manifest.dependencies[package.packageName];
                        package.hasUpdate = CompareVersions(package.version, package.installedVersion) > 0;
                        installedPackages.Add(package);
                    }
                    else
                    {
                        package.isInstalled = false;
                        package.installedVersion = "";
                        package.hasUpdate = false;
                    }
                }
            }
            
            return installedPackages;
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
