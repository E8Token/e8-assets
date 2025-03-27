using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Energy8.FirebaseManager
{
    public class FirebasePackageInstaller
    {
        private readonly HttpClient httpClient;
        
        public FirebasePackageInstaller(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        
        public async Task DownloadAndInstallPackages(
            IEnumerable<string> selectedPackageIds,
            Dictionary<string, string> selectedVersions,
            Dictionary<string, List<VersionInfo>> packageVersions,
            IProgress<string> progress)
        {
            // Create Firebase directory if it doesn't exist
            string firebaseDir = Path.Combine(Application.dataPath, "..", "Firebase");
            if (!Directory.Exists(firebaseDir))
            {
                Directory.CreateDirectory(firebaseDir);
            }

            // Collect all dependencies to install
            HashSet<string> packagesToInstall = new HashSet<string>();
            Dictionary<string, DependencyInfo> packageDetails = new Dictionary<string, DependencyInfo>();

            foreach (var packageId in selectedPackageIds)
            {
                if (!packageVersions.ContainsKey(packageId) || packageVersions[packageId].Count == 0)
                {
                    continue;
                }

                var selectedVersion = packageVersions[packageId]
                    .FirstOrDefault(v => v.Version == selectedVersions[packageId]);
                
                if (selectedVersion != null)
                {
                    // Add the main package
                    packagesToInstall.Add(packageId);
                    packageDetails[packageId] = new DependencyInfo
                    {
                        PackageId = packageId,
                        Version = selectedVersion.Version,
                        TgzUrl = selectedVersion.TgzUrl
                    };

                    // Add all dependencies
                    foreach (var dep in selectedVersion.Dependencies)
                    {
                        packagesToInstall.Add(dep.PackageId);
                        if (!packageDetails.ContainsKey(dep.PackageId) || 
                            CompareVersions(packageDetails[dep.PackageId].Version, dep.Version) < 0)
                        {
                            packageDetails[dep.PackageId] = dep;
                        }
                    }
                }
            }

            // Now download and install all required packages
            int total = packagesToInstall.Count;
            int current = 0;

            foreach (var packageId in packagesToInstall)
            {
                current++;
                var packageInfo = packageDetails[packageId];
                
                progress.Report($"Downloading {packageId} ({current}/{total})...");
                
                string tgzUrl = packageInfo.TgzUrl;
                string packageFileName = Path.GetFileName(tgzUrl);
                string packageFilePath = Path.Combine(firebaseDir, packageFileName);
                
                // Download the package
                using (var response = await httpClient.GetAsync(tgzUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                // Install package using Package Manager
                progress.Report($"Installing {packageId} ({current}/{total})...");
                
                string packageUri = $"file:{packageFilePath}";
                await InstallPackage(packageUri);
            }
        }
        
        private async Task InstallPackage(string packageUri)
        {
            bool isDone = false;
            Exception exception = null;
            AddRequest addRequest = null;
            
            addRequest = Client.Add(packageUri);
            
            EditorApplication.update += Progress;

            void Progress()
            {
                if (addRequest.IsCompleted)
                {
                    if (addRequest.Status == StatusCode.Failure)
                    {
                        exception = new Exception($"Failed to install package: {addRequest.Error.message}");
                    }
                    
                    EditorApplication.update -= Progress;
                    isDone = true;
                }
            }

            while (!isDone)
            {
                await Task.Delay(100);
            }

            if (exception != null)
            {
                throw exception;
            }
        }
        
        private int CompareVersions(string version1, string version2)
        {
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
}