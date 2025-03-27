using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Energy8.FirebaseManager
{
    [InitializeOnLoad]
    public static class FirebaseManagerInitializer
    {
        static FirebaseManagerInitializer()
        {
            // Delay the startup check to avoid issues during project loading
            EditorApplication.delayCall += CheckForUpdatesOnStartup;
        }
        
        private static async void CheckForUpdatesOnStartup()
        {
            try
            {
                // Only check if enabled in config
                if (!FirebaseProjectConfig.Instance.CheckUpdatesOnStartup)
                    return;
                
                // Check if we have any required packages
                var requiredPackages = FirebaseProjectConfig.Instance.RequiredPackages;
                if (requiredPackages.Count == 0)
                    return;
                
                // Create needed services
                var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity Firebase Manager");
                
                try
                {
                    var packageParser = new FirebasePackageParser(httpClient);
                    var packageTracker = new FirebasePackageTracker();
                    var packageInstaller = new FirebasePackageInstaller(httpClient);
                    
                    // Fetch Firebase package data
                    string html = await packageParser.FetchHtmlFromFirebaseArchive();
                    var firebasePackages = packageParser.GetFirebasePackages();
                    var packageVersions = packageParser.ParseVersions(html, firebasePackages);
                    
                    // Check for updates
                    var updates = await packageTracker.CheckForUpdates(packageVersions);
                    
                    // If we have updates and auto-update is enabled, install them
                    if (updates.Count > 0)
                    {
                        if (FirebaseProjectConfig.Instance.AutoUpdateRequired)
                        {
                            // Auto-update packages
                            var progress = new Progress<string>(message => 
                                Debug.Log($"[Firebase Manager] {message}"));
                            
                            // Get packages to update with their latest versions
                            var packagesToUpdate = updates.Select(u => u.PackageId).ToList();
                            var selectedVersions = updates.ToDictionary(
                                u => u.PackageId, 
                                u => u.LatestVersion);
                            
                            await packageInstaller.DownloadAndInstallPackages(
                                packagesToUpdate,
                                selectedVersions,
                                packageVersions,
                                progress);
                            
                            Debug.Log($"[Firebase Manager] Auto-updated {updates.Count} Firebase packages");
                        }
                        else
                        {
                            // Show notification with update count
                            ShowUpdateNotification(updates);
                        }
                    }
                }
                finally
                {
                    httpClient.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Firebase Manager] Error checking for updates: {ex.Message}");
            }
        }
        
        private static void ShowUpdateNotification(List<UpdateInfo> updates)
        {
            if (updates.Count == 0)
                return;
                
            string message = updates.Count == 1
                ? $"A Firebase package requires an update: {updates[0].PackageId}"
                : $"{updates.Count} Firebase packages require updates";
                
            // Show a notification that can be clicked to open the Firebase Manager
            if (EditorUtility.DisplayDialog(
                "Firebase Updates Available", 
                $"{message}. Would you like to open Firebase Manager to update them?", 
                "Open Firebase Manager", 
                "Not Now"))
            {
                FirebaseManagerWindow.ShowWindow();
            }
        }
    }
}