using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Energy8.FirebaseManager
{
    /// <summary>
    /// Tracks installed Firebase packages in the Unity Package Manager
    /// </summary>
    public class FirebasePackageTracker
    {
        private Dictionary<string, string> installedVersions = new Dictionary<string, string>();
        private bool isRefreshing = false;
        
        public event Action OnPackageListUpdated;
        
        public IReadOnlyDictionary<string, string> InstalledVersions => installedVersions;
        public bool IsRefreshing => isRefreshing;
        
        public bool IsPackageInstalled(string packageId)
        {
            return installedVersions.ContainsKey(packageId);
        }
        
        public string GetInstalledVersion(string packageId)
        {
            return installedVersions.TryGetValue(packageId, out string version) ? version : null;
        }
        
        /// <summary>
        /// Checks if a package needs update based on minimum required version
        /// </summary>
        public bool NeedsUpdate(string packageId, string minimumVersion)
        {
            if (!IsPackageInstalled(packageId))
                return true;
                
            string installedVersion = GetInstalledVersion(packageId);
            return CompareVersions(installedVersion, minimumVersion) < 0;
        }
        
        public async Task RefreshInstalledPackages()
        {
            try
            {
                isRefreshing = true;
                
                // Get all packages from Package Manager
                var packages = await ListPackages();
                
                // Filter and store Firebase packages
                installedVersions.Clear();
                foreach (var package in packages)
                {
                    if (package.name.StartsWith("com.google.firebase") || 
                        package.name == "com.google.external-dependency-manager")
                    {
                        installedVersions[package.name] = package.version;
                    }
                }
                
                OnPackageListUpdated?.Invoke();
            }
            finally
            {
                isRefreshing = false;
            }
        }
        
        /// <summary>
        /// Checks for updates for all required packages
        /// </summary>
        public async Task<List<UpdateInfo>> CheckForUpdates(
            Dictionary<string, List<VersionInfo>> availableVersions)
        {
            List<UpdateInfo> updates = new List<UpdateInfo>();
            
            // Make sure we have the latest package info
            await RefreshInstalledPackages();
            
            foreach (var requirement in FirebaseProjectConfig.Instance.RequiredPackages)
            {
                string packageId = requirement.PackageId;
                
                // Skip if not in available versions list
                if (!availableVersions.ContainsKey(packageId) || 
                    availableVersions[packageId].Count == 0)
                    continue;
                
                // Get the latest version from the available versions
                var latestVersion = availableVersions[packageId]
                    .OrderByDescending(v => v.Version, new VersionComparer())
                    .FirstOrDefault();
                
                if (latestVersion == null)
                    continue;
                
                // Check if currently installed
                if (installedVersions.TryGetValue(packageId, out string currentVersion))
                {
                    // Check if installed version is older than latest available
                    if (CompareVersions(currentVersion, latestVersion.Version) < 0)
                    {
                        updates.Add(new UpdateInfo
                        {
                            PackageId = packageId,
                            CurrentVersion = currentVersion,
                            LatestVersion = latestVersion.Version,
                            DownloadUrl = latestVersion.TgzUrl
                        });
                    }
                }
                else
                {
                    // Package not installed but required
                    updates.Add(new UpdateInfo
                    {
                        PackageId = packageId,
                        CurrentVersion = null,
                        LatestVersion = latestVersion.Version,
                        DownloadUrl = latestVersion.TgzUrl
                    });
                }
            }
            
            return updates;
        }
        
        private async Task<List<PackageInfo>> ListPackages()
        {
            TaskCompletionSource<List<PackageInfo>> tcs = new TaskCompletionSource<List<PackageInfo>>();
            ListRequest request = Client.List();
            
            EditorApplication.update += CheckRequest;
            
            void CheckRequest()
            {
                if (!request.IsCompleted)
                    return;
                
                EditorApplication.update -= CheckRequest;
                
                if (request.Status == StatusCode.Success)
                {
                    tcs.SetResult(request.Result.ToList());
                }
                else
                {
                    tcs.SetException(new Exception($"Package list failed: {request.Error.message}"));
                }
            }
            
            return await tcs.Task;
        }
        
        public int CompareVersions(string version1, string version2)
        {
            // Handle null versions
            if (string.IsNullOrEmpty(version1) && string.IsNullOrEmpty(version2))
                return 0;
            if (string.IsNullOrEmpty(version1))
                return -1;
            if (string.IsNullOrEmpty(version2))
                return 1;
            
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
            
            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                int v1 = i < v1Parts.Length ? v1Parts[i] : 0;
                int v2 = i < v2Parts.Length ? v2Parts[i] : 0;
                
                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }
            
            return 0;
        }
    }
    
    public class UpdateInfo
    {
        public string PackageId;
        public string CurrentVersion;
        public string LatestVersion;
        public string DownloadUrl;
    }
    
    public class VersionComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var v1Parts = x.Split('.').Select(int.Parse).ToArray();
            var v2Parts = y.Split('.').Select(int.Parse).ToArray();
            
            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                int v1 = i < v1Parts.Length ? v1Parts[i] : 0;
                int v2 = i < v2Parts.Length ? v2Parts[i] : 0;
                
                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }
            
            return 0;
        }
    }
}