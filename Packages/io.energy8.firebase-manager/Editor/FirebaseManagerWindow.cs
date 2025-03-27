using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Energy8.FirebaseManager
{
    public class FirebaseManagerWindow : EditorWindow
    {
        private bool isLoading = false;
        private string statusMessage = "";
        private string errorMessage = "";
        private Vector2 scrollPosition;
        
        private Dictionary<string, FirebasePackage> firebasePackages = new Dictionary<string, FirebasePackage>();
        private Dictionary<string, bool> selectedPackages = new Dictionary<string, bool>();
        private Dictionary<string, List<VersionInfo>> packageVersions = new Dictionary<string, List<VersionInfo>>();
        private Dictionary<string, string> selectedVersions = new Dictionary<string, string>();
        
        private HttpClient httpClient;
        private FirebasePackageParser packageParser;
        private FirebasePackageInstaller packageInstaller;
        private FirebasePackageTracker packageTracker;
        
        private Progress<string> progressReporter;
        
        // UI States
        private bool showSettings = false;
        private bool checkUpdatesOnStartup;
        private bool autoUpdateRequired;
        private List<UpdateInfo> availableUpdates = new List<UpdateInfo>();

        [MenuItem("Window/Energy8/Firebase Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<FirebaseManagerWindow>("Firebase Manager");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        private void OnEnable()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity Firebase Manager");
            
            packageParser = new FirebasePackageParser(httpClient);
            packageInstaller = new FirebasePackageInstaller(httpClient);
            packageTracker = new FirebasePackageTracker();
            progressReporter = new Progress<string>(message => statusMessage = message);
            
            // Load config settings
            checkUpdatesOnStartup = FirebaseProjectConfig.Instance.CheckUpdatesOnStartup;
            autoUpdateRequired = FirebaseProjectConfig.Instance.AutoUpdateRequired;
            
            // Track package updates
            packageTracker.OnPackageListUpdated += OnPackageListUpdated;
            
            FetchPackageData();
        }
        
        private void OnDisable()
        {
            httpClient.Dispose();
            packageTracker.OnPackageListUpdated -= OnPackageListUpdated;
        }
        
        private void OnPackageListUpdated()
        {
            // Force repaint when package list is updated
            Repaint();
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUI.BeginDisabledGroup(isLoading);
            
            DrawToolbar();
            
            if (showSettings)
            {
                DrawSettingsSection();
            }
            else
            {
                DrawMainContent();
            }

            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Firebase Packages Manager", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(showSettings ? "Close Settings" : "Settings", GUILayout.Width(100)))
            {
                showSettings = !showSettings;
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

            if (isLoading)
            {
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
                return;
            }
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                FetchPackageData();
            }
            
            if (GUILayout.Button("Check Updates", EditorStyles.toolbarButton))
            {
                CheckForUpdates();
            }
            
            if (availableUpdates.Count > 0)
            {
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button($"Update All ({availableUpdates.Count})", EditorStyles.toolbarButton))
                {
                    UpdateAllRequiredPackages();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Firebase Manager Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            checkUpdatesOnStartup = EditorGUILayout.Toggle("Check Updates on Startup", checkUpdatesOnStartup);
            if (EditorGUI.EndChangeCheck())
            {
                FirebaseProjectConfig.Instance.SetCheckUpdatesOnStartup(checkUpdatesOnStartup);
            }
            
            EditorGUI.BeginChangeCheck();
            autoUpdateRequired = EditorGUILayout.Toggle("Auto-Update Required Packages", autoUpdateRequired);
            if (EditorGUI.EndChangeCheck())
            {
                FirebaseProjectConfig.Instance.SetAutoUpdateRequired(autoUpdateRequired);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Required Packages", EditorStyles.boldLabel);
            
            if (FirebaseProjectConfig.Instance.RequiredPackages.Count == 0)
            {
                EditorGUILayout.HelpBox("No required packages configured. Mark packages as required in the main view.", MessageType.Info);
            }
            else
            {
                foreach (var package in FirebaseProjectConfig.Instance.RequiredPackages)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    var packageName = firebasePackages.ContainsKey(package.PackageId) 
                        ? firebasePackages[package.PackageId].DisplayName 
                        : package.PackageId;
                    
                    EditorGUILayout.LabelField(packageName);
                    EditorGUILayout.LabelField($"Min v{package.MinimumVersion}");
                    
                    var installedVersion = packageTracker.GetInstalledVersion(package.PackageId);
                    string versionStatus = installedVersion != null 
                        ? $"Installed: v{installedVersion}" 
                        : "Not Installed";
                    
                    EditorGUILayout.LabelField(versionStatus);
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        FirebaseProjectConfig.Instance.RemoveRequiredPackage(package.PackageId);
                        Repaint();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMainContent()
        {
            if (firebasePackages.Count == 0)
            {
                EditorGUILayout.HelpBox("No Firebase packages found. Click 'Refresh' to load available packages.", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Show updates available section
            if (availableUpdates.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Updates Available ({availableUpdates.Count})", EditorStyles.boldLabel);
                
                foreach (var update in availableUpdates)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    var packageName = firebasePackages.ContainsKey(update.PackageId) 
                        ? firebasePackages[update.PackageId].DisplayName 
                        : update.PackageId;
                    
                    string versionInfo = update.CurrentVersion != null
                        ? $"{update.CurrentVersion} → {update.LatestVersion}"
                        : $"Not Installed → {update.LatestVersion}";
                    
                    EditorGUILayout.LabelField(packageName);
                    EditorGUILayout.LabelField(versionInfo);
                    
                    if (GUILayout.Button("Update", GUILayout.Width(70)))
                    {
                        UpdatePackage(update);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Available Firebase Packages", EditorStyles.boldLabel);
            
            foreach (var package in firebasePackages.Values.OrderBy(p => p.DisplayName))
            {
                EditorGUILayout.BeginVertical("box");

                // Package header
                EditorGUILayout.BeginHorizontal();
                bool wasSelected = selectedPackages.ContainsKey(package.PackageId) && selectedPackages[package.PackageId];
                bool isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(20));
                
                if (wasSelected != isSelected)
                {
                    selectedPackages[package.PackageId] = isSelected;
                }

                EditorGUILayout.LabelField(package.DisplayName, EditorStyles.boldLabel);
                
                // Show installed version status
                bool isInstalled = packageTracker.IsPackageInstalled(package.PackageId);
                string installedVersion = packageTracker.GetInstalledVersion(package.PackageId);
                
                GUIStyle versionStyle = new GUIStyle(EditorStyles.miniLabel);
                
                if (isInstalled)
                {
                    versionStyle.normal.textColor = Color.green;
                    EditorGUILayout.LabelField($"v{installedVersion}", versionStyle, GUILayout.Width(70));
                }
                else
                {
                    versionStyle.normal.textColor = Color.gray;
                    EditorGUILayout.LabelField("Not Installed", versionStyle, GUILayout.Width(70));
                }
                
                EditorGUILayout.EndHorizontal();

                // Package details
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Package ID: {package.PackageId}");
                
                // Show version selection
                if (packageVersions.ContainsKey(package.PackageId) && packageVersions[package.PackageId].Count > 0)
                {
                    string[] versions = packageVersions[package.PackageId]
                        .OrderByDescending(v => v.Version, new VersionComparer())
                        .Select(v => v.Version)
                        .ToArray();
                    
                    if (!selectedVersions.ContainsKey(package.PackageId) || !versions.Contains(selectedVersions[package.PackageId]))
                    {
                        selectedVersions[package.PackageId] = versions[0];
                    }

                    int selectedIndex = Array.IndexOf(versions, selectedVersions[package.PackageId]);
                    int newIndex = EditorGUILayout.Popup("Version", selectedIndex, versions);
                    
                    if (newIndex != selectedIndex)
                    {
                        selectedVersions[package.PackageId] = versions[newIndex];
                    }

                    var selectedVersion = packageVersions[package.PackageId]
                        .FirstOrDefault(v => v.Version == selectedVersions[package.PackageId]);
                    
                    if (selectedVersion != null)
                    {
                        if (!string.IsNullOrEmpty(selectedVersion.TgzUrl))
                        {
                            EditorGUILayout.LabelField($"Download URL: {selectedVersion.TgzUrl}");
                        }
                        
                        if (selectedVersion.Dependencies.Count > 0)
                        {
                            EditorGUILayout.LabelField("Dependencies:", EditorStyles.boldLabel);
                            foreach (var dep in selectedVersion.Dependencies)
                            {
                                EditorGUILayout.LabelField($"- {dep.PackageId} ({dep.Version})");
                            }
                        }
                    }
                    
                    // Required package controls
                    EditorGUILayout.BeginHorizontal();
                    
                    bool isRequired = FirebaseProjectConfig.Instance.IsPackageRequired(package.PackageId);
                    string buttonText = isRequired ? "Remove from Required" : "Mark as Required";
                    
                    if (GUILayout.Button(buttonText))
                    {
                        if (isRequired)
                        {
                            FirebaseProjectConfig.Instance.RemoveRequiredPackage(package.PackageId);
                        }
                        else
                        {
                            FirebaseProjectConfig.Instance.AddRequiredPackage(
                                package.PackageId, 
                                selectedVersions[package.PackageId]);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("No versions available");
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Download and Install Selected Packages"))
            {
                DownloadAndInstallSelectedPackages();
            }
        }

        private async void FetchPackageData()
        {
            try
            {
                isLoading = true;
                errorMessage = "";
                statusMessage = "Loading package data...";

                // Get the Firebase packages
                firebasePackages = packageParser.GetFirebasePackages();
                
                // Initialize selection state
                foreach (var packageId in firebasePackages.Keys)
                {
                    if (!selectedPackages.ContainsKey(packageId))
                    {
                        selectedPackages[packageId] = false;
                    }
                }
                
                // Check installed packages
                statusMessage = "Checking installed packages...";
                await packageTracker.RefreshInstalledPackages();
                
                // Download the HTML from Google Developers
                statusMessage = "Downloading Firebase package data from Google Developers...";
                string html = await packageParser.FetchHtmlFromFirebaseArchive();
                
                // Parse package versions
                statusMessage = "Parsing package data...";
                packageVersions = packageParser.ParseVersions(html, firebasePackages);
                
                // Check for updates
                statusMessage = "Checking for updates...";
                await CheckForUpdates();
                
                isLoading = false;
                statusMessage = "";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching package data: {ex}");
                errorMessage = $"Error: {ex.Message}";
                isLoading = false;
            }
        }
        
        private async Task CheckForUpdates()
        {
            try
            {
                isLoading = true;
                errorMessage = "";
                statusMessage = "Checking for updates...";
                
                availableUpdates = await packageTracker.CheckForUpdates(packageVersions);
                
                isLoading = false;
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking for updates: {ex}");
                errorMessage = $"Error checking for updates: {ex.Message}";
                isLoading = false;
            }
        }
        
        private async void UpdatePackage(UpdateInfo update)
        {
            try
            {
                isLoading = true;
                errorMessage = "";
                statusMessage = $"Updating {update.PackageId}...";
                
                Dictionary<string, string> versionsToInstall = new Dictionary<string, string>
                {
                    { update.PackageId, update.LatestVersion }
                };
                
                await packageInstaller.DownloadAndInstallPackages(
                    new List<string> { update.PackageId },
                    versionsToInstall,
                    packageVersions,
                    progressReporter);
                
                // Refresh installed packages
                await packageTracker.RefreshInstalledPackages();
                
                // Update the available updates list
                await CheckForUpdates();
                
                statusMessage = $"{update.PackageId} was updated successfully!";
                await Task.Delay(2000); // Show success message for 2 seconds
                isLoading = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating package: {ex}");
                errorMessage = $"Error: {ex.Message}";
                isLoading = false;
            }
        }
        
        private async void UpdateAllRequiredPackages()
        {
            try
            {
                isLoading = true;
                errorMessage = "";
                statusMessage = "Updating all required packages...";
                
                // Create mapping of package IDs to latest versions
                Dictionary<string, string> versionsToInstall = availableUpdates
                    .ToDictionary(u => u.PackageId, u => u.LatestVersion);
                
                await packageInstaller.DownloadAndInstallPackages(
                    availableUpdates.Select(u => u.PackageId).ToList(),
                    versionsToInstall,
                    packageVersions,
                    progressReporter);
                
                // Refresh installed packages
                await packageTracker.RefreshInstalledPackages();
                
                // Update the available updates list
                await CheckForUpdates();
                
                statusMessage = "All packages were updated successfully!";
                await Task.Delay(2000); // Show success message for 2 seconds
                isLoading = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating packages: {ex}");
                errorMessage = $"Error: {ex.Message}";
                isLoading = false;
            }
        }

        private async void DownloadAndInstallSelectedPackages()
        {
            try
            {
                isLoading = true;
                errorMessage = "";
                
                // Get selected packages
                var selectedPackageIds = selectedPackages
                    .Where(kvp => kvp.Value)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                if (selectedPackageIds.Count == 0)
                {
                    errorMessage = "No packages selected for installation.";
                    isLoading = false;
                    return;
                }

                await packageInstaller.DownloadAndInstallPackages(
                    selectedPackageIds, 
                    selectedVersions, 
                    packageVersions,
                    progressReporter);
                
                // Refresh installed packages
                await packageTracker.RefreshInstalledPackages();
                
                // Update the available updates list
                await CheckForUpdates();

                statusMessage = "All selected packages were installed successfully!";
                await Task.Delay(2000); // Show success message for 2 seconds
                isLoading = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing packages: {ex}");
                errorMessage = $"Error: {ex.Message}";
                isLoading = false;
            }
        }
    }
}