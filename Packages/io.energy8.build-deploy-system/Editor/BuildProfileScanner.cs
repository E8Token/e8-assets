using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    /// <summary>
    /// Автоматически сканирует Build Profiles и создает/обновляет конфигурации
    /// </summary>
    public static class BuildProfileScanner
    {
        private const string BUILD_PROFILES_PATH = "Assets/Settings/Build Profiles";
        private const string BUILD_CONFIGS_PATH = "Assets/BuildSystem/Configs";
        
        public static event Action<List<BuildConfiguration>> OnConfigurationsUpdated;

        /// <summary>
        /// Сканирует папку с Build Profiles и создает конфигурации
        /// </summary>
        public static List<BuildConfiguration> ScanAndCreateConfigurations()
        {
            var configurations = new List<BuildConfiguration>();
            
            // Создаем папку для конфигураций если её нет
            if (!Directory.Exists(BUILD_CONFIGS_PATH))
            {
                Directory.CreateDirectory(BUILD_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }

            // Находим все Build Profiles
            var buildProfiles = FindAllBuildProfiles();
            
            foreach (var profile in buildProfiles)
            {
                var config = GetOrCreateConfigurationForProfile(profile);
                if (config != null)
                {
                    configurations.Add(config);
                }
            }

            // Удаляем конфигурации для которых больше нет профилей
            CleanupOrphanedConfigurations(buildProfiles);

            OnConfigurationsUpdated?.Invoke(configurations);
            return configurations;
        }

        /// <summary>
        /// Получает существующую или создает новую конфигурацию для профиля
        /// </summary>
        private static BuildConfiguration GetOrCreateConfigurationForProfile(BuildProfile profile)
        {
            var profilePath = AssetDatabase.GetAssetPath(profile);
            var profileGUID = AssetDatabase.AssetPathToGUID(profilePath);
            
            // Ищем существующую конфигурацию по GUID
            var existingConfig = FindConfigurationByGUID(profileGUID);
            if (existingConfig != null)
            {
                // Обновляем имя если изменилось
                existingConfig.RefreshBuildProfileReference();
                return existingConfig;
            }

            // Создаем новую конфигурацию
            var config = ScriptableObject.CreateInstance<BuildConfiguration>();
            config.SetBuildProfile(profile);
            
            // Создаем имя файла на основе имени профиля
            var fileName = $"Config_{SanitizeFileName(profile.name)}.asset";
            var assetPath = Path.Combine(BUILD_CONFIGS_PATH, fileName);
            
            // Если файл уже существует, добавляем суффикс
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

        /// <summary>
        /// Находит все Build Profiles в проекте
        /// </summary>
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

        /// <summary>
        /// Находит конфигурацию по GUID профиля
        /// </summary>
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

        /// <summary>
        /// Удаляет конфигурации для которых больше нет профилей
        /// </summary>
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
                        Debug.Log($"Removing orphaned configuration: {config.name}");
                        AssetDatabase.DeleteAsset(configPath);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Очищает имя файла от недопустимых символов
        /// </summary>
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

        /// <summary>
        /// Получает все существующие конфигурации
        /// </summary>
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

        /// <summary>
        /// Проверяет нужно ли обновить конфигурации
        /// </summary>
        public static bool NeedsUpdate()
        {
            var profiles = FindAllBuildProfiles();
            var configurations = GetAllConfigurations();
            
            // Проверяем количество
            if (profiles.Count != configurations.Count)
                return true;

            // Проверяем соответствие GUID'ов
            var profileGUIDs = profiles.Select(p => 
                AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(p))).ToHashSet();
            var configGUIDs = configurations.Select(c => c.BuildProfileGUID).ToHashSet();

            return !profileGUIDs.SetEquals(configGUIDs);
        }
    }
}
