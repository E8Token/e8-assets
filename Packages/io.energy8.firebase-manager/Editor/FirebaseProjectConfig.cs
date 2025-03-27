using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Energy8.FirebaseManager
{
    /// <summary>
    /// Manages Firebase configuration for the project, tracking required packages and their versions
    /// </summary>
    [Serializable]
    public class FirebaseProjectConfig : ScriptableObject
    {
        [Serializable]
        public class PackageRequirement
        {
            public string PackageId;
            public string MinimumVersion;
            public bool AutoUpdate;
        }

        [SerializeField] private List<PackageRequirement> requiredPackages = new List<PackageRequirement>();
        [SerializeField] private bool checkUpdatesOnStartup = true;
        [SerializeField] private bool autoUpdateRequired = false;

        private const string CONFIG_PATH = "Assets/FirebaseManager/FirebaseConfig.asset";

        // Static accessor for the config instance
        private static FirebaseProjectConfig _instance;
        public static FirebaseProjectConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadOrCreateConfig();
                }
                return _instance;
            }
        }

        public bool CheckUpdatesOnStartup => checkUpdatesOnStartup;
        public bool AutoUpdateRequired => autoUpdateRequired;
        public IReadOnlyList<PackageRequirement> RequiredPackages => requiredPackages;

        private static FirebaseProjectConfig LoadOrCreateConfig()
        {
            // Try to load the config
            var config = AssetDatabase.LoadAssetAtPath<FirebaseProjectConfig>(CONFIG_PATH);
            
            if (config == null)
            {
                // Create a new config
                config = CreateInstance<FirebaseProjectConfig>();
                
                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_PATH));
                
                // Save the config to disk
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                AssetDatabase.SaveAssets();
            }
            
            return config;
        }

        public void SetCheckUpdatesOnStartup(bool value)
        {
            checkUpdatesOnStartup = value;
            SaveConfig();
        }

        public void SetAutoUpdateRequired(bool value)
        {
            autoUpdateRequired = value;
            SaveConfig();
        }

        public bool IsPackageRequired(string packageId)
        {
            return requiredPackages.Any(p => p.PackageId == packageId);
        }

        public void AddRequiredPackage(string packageId, string version, bool autoUpdate = false)
        {
            if (!IsPackageRequired(packageId))
            {
                requiredPackages.Add(new PackageRequirement
                {
                    PackageId = packageId,
                    MinimumVersion = version,
                    AutoUpdate = autoUpdate
                });
                SaveConfig();
            }
            else
            {
                // Update existing requirement
                var existing = requiredPackages.First(p => p.PackageId == packageId);
                existing.MinimumVersion = version;
                existing.AutoUpdate = autoUpdate;
                SaveConfig();
            }
        }

        public void RemoveRequiredPackage(string packageId)
        {
            int initialCount = requiredPackages.Count;
            requiredPackages.RemoveAll(p => p.PackageId == packageId);
            
            if (requiredPackages.Count != initialCount)
            {
                SaveConfig();
            }
        }

        public void UpdateRequiredPackageVersion(string packageId, string version)
        {
            var package = requiredPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (package != null && package.MinimumVersion != version)
            {
                package.MinimumVersion = version;
                SaveConfig();
            }
        }

        public void SetAutoUpdateForPackage(string packageId, bool autoUpdate)
        {
            var package = requiredPackages.FirstOrDefault(p => p.PackageId == packageId);
            if (package != null && package.AutoUpdate != autoUpdate)
            {
                package.AutoUpdate = autoUpdate;
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}