using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public static class BuildProfileScanner
    {
        private const string BUILD_PROFILES_PATH = "Assets/Settings/Build Profiles";
        private const string BUILD_CONFIGS_PATH = "Assets/Settings/Build & Deploy System";
        
        public static event Action<List<BuildConfiguration>> OnConfigurationsUpdated;

        public static List<BuildConfiguration> ScanAndCreateConfigurations()
        {
            var configurations = new List<BuildConfiguration>();
            
            if (!Directory.Exists(BUILD_CONFIGS_PATH))
            {
                Directory.CreateDirectory(BUILD_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }

            var buildProfiles = FindAllBuildProfiles();
            
            foreach (var profile in buildProfiles)
            {
                var config = GetOrCreateConfigurationForProfile(profile);
                if (config != null)
                {
                    configurations.Add(config);
                }
            }

            CleanupOrphanedConfigurations(buildProfiles);

            OnConfigurationsUpdated?.Invoke(configurations);
            return configurations;
        }

        private static BuildConfiguration GetOrCreateConfigurationForProfile(BuildProfile profile)
        {
            var profilePath = AssetDatabase.GetAssetPath(profile);
            var profileGUID = AssetDatabase.AssetPathToGUID(profilePath);
            
            var existingConfig = FindConfigurationByGUID(profileGUID);
            if (existingConfig != null)
            {
                existingConfig.RefreshBuildProfileReference();
                return existingConfig;
            }

            var config = ScriptableObject.CreateInstance<BuildConfiguration>();
            config.SetBuildProfile(profile);
            
            var fileName = $"Config_{SanitizeFileName(profile.name)}.asset";
            var assetPath = Path.Combine(BUILD_CONFIGS_PATH, fileName);
            
            var counter = 1;
            while (File.Exists(assetPath))
            {
                fileName = $"Config_{SanitizeFileName(profile.name)}_{counter}.asset";
                assetPath = Path.Combine(BUILD_CONFIGS_PATH, fileName);
                counter++;
            }

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created build configuration for profile: {profile.name} at {assetPath}");
            return config;
        }

        private static List<BuildProfile> FindAllBuildProfiles()
        {
            var profiles = new List<BuildProfile>();
            
            if (Directory.Exists(BUILD_PROFILES_PATH))
            {
                var guids = AssetDatabase.FindAssets("t:BuildProfile", new[] { BUILD_PROFILES_PATH });
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }
            }

            return profiles;
        }

        private static BuildConfiguration FindConfigurationByGUID(string profileGUID)
        {
            var configGuids = AssetDatabase.FindAssets("t:BuildConfiguration", new[] { BUILD_CONFIGS_PATH });
            foreach (var configGuid in configGuids)
            {
                var configPath = AssetDatabase.GUIDToAssetPath(configGuid);
                var config = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(configPath);
                if (config != null && config.BuildProfileGUID == profileGUID)
                {
                    return config;
                }
            }
            return null;
        }

        private static void CleanupOrphanedConfigurations(List<BuildProfile> existingProfiles)
        {
            var existingGUIDs = existingProfiles.Select(p => 
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(p))).ToHashSet();

            var configGuids = AssetDatabase.FindAssets("t:BuildConfiguration", new[] { BUILD_CONFIGS_PATH });
            foreach (var configGuid in configGuids)
            {
                var configPath = AssetDatabase.GUIDToAssetPath(configGuid);
                var config = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(configPath);
                
                if (config != null && !string.IsNullOrEmpty(config.BuildProfileGUID))
                {
                    if (!existingGUIDs.Contains(config.BuildProfileGUID))
                    {
                        AssetDatabase.DeleteAsset(configPath);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }
            return sanitized;
        }


        public static List<BuildConfiguration> GetAllConfigurations()
        {
            var configurations = new List<BuildConfiguration>();
            
            if (Directory.Exists(BUILD_CONFIGS_PATH))
            {
                var guids = AssetDatabase.FindAssets("t:BuildConfiguration", new[] { BUILD_CONFIGS_PATH });
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var config = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(assetPath);
                    if (config != null)
                    {
                        configurations.Add(config);
                    }
                }
            }

            return configurations;
        }

        public static bool NeedsUpdate()
        {
            var profiles = FindAllBuildProfiles();
            var configurations = GetAllConfigurations();
            
            if (profiles.Count != configurations.Count)
                return true;

            var profileGUIDs = profiles.Select(p => 
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(p))).ToHashSet();
            var configGUIDs = configurations.Select(c => c.BuildProfileGUID).ToHashSet();

            return !profileGUIDs.SetEquals(configGUIDs);
        }
    }
}
