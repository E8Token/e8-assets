#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Build;
using Energy8.Identity.Core.Configuration.Models;
using Energy8.BuildSystem.Core;
using System.Linq;
using System;
using UnityEditor.SceneManagement;

namespace Energy8.BuildSystem.Configuration
{
    [CreateAssetMenu(fileName = "BuildConfigSettings", menuName = "Configs/Build Config Settings")]
    public class BuildConfigProjectSettings : ScriptableObject
    {
        public List<BuildConfig> buildConfigurations = new();
        public int selectedConfigIndex = 0;

        private const string SettingsPath = "Assets/Resources/Configuration/BuildConfigSettings.asset";

        private static BuildConfigProjectSettings _instance;
        public static BuildConfigProjectSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadSettings();
                }
                return _instance;
            }
        }

        public static BuildConfigProjectSettings LoadSettings()
        {
            BuildConfigProjectSettings settings = AssetDatabase.LoadAssetAtPath<BuildConfigProjectSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<BuildConfigProjectSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return settings;
        }
    }

    static class BuildConfigSettingsProvider
    {
        private const string ConfigsPath = "Assets/Resources/Configuration/BuildConfigs/";
        private static readonly Color sceneListHeaderBg = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        private static List<UnityEngine.Object> tempSceneAssets = new List<UnityEngine.Object>();

        [SettingsProvider]
        public static SettingsProvider CreateBuildConfigSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Build Configurations", SettingsScope.Project)
            {
                label = "Build Configurations",
                guiHandler = (searchContext) =>
                {
                    DrawSettingsGUI();
                },
                keywords = new HashSet<string>(new[] { "Build", "Configurations", "Platforms", "Scenes" })
            };
            return provider;
        }

        private static void DrawSettingsGUI()
        {
            var settings = BuildConfigProjectSettings.Instance;
            int selectedConfigIndex = settings.selectedConfigIndex;

            EditorGUILayout.LabelField("Build Configurations", EditorStyles.boldLabel);

            selectedConfigIndex = EditorGUILayout.Popup("Select Configuration", selectedConfigIndex, GetConfigurationNames());
            settings.selectedConfigIndex = selectedConfigIndex;

            if (GUILayout.Button("Add New Configuration"))
            {
                BuildConfig newConfig = ScriptableObject.CreateInstance<BuildConfig>();
                newConfig.name = "Configuration" + (settings.buildConfigurations.Count + 1);
                settings.buildConfigurations.Add(newConfig);

                AssetDatabase.CreateAsset(newConfig, ConfigsPath + $"{newConfig.name}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (settings.buildConfigurations.Count > 0)
            {
                BuildConfig selectedConfig = settings.buildConfigurations[selectedConfigIndex];

                DrawConfigurationName(selectedConfig);
                DrawBuildPath(selectedConfig);
                DrawTargetPlatform(selectedConfig);
                DrawCompression(selectedConfig);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Application Config", EditorStyles.boldLabel);

                // IP Type and HTTP Option
                selectedConfig.ipType = (IPType)EditorGUILayout.EnumPopup("IP Type", selectedConfig.ipType);
                selectedConfig.insecureHttpOption = (InsecureHttpOption)EditorGUILayout.EnumPopup("Allow HTTP", selectedConfig.insecureHttpOption);

                EditorGUILayout.Space();
                DrawScenesList(selectedConfig);

                EditorGUILayout.Space();

                if (selectedConfig.buildTargetGroup != BuildTargetGroup.iOS &
                    selectedConfig.buildTargetGroup != BuildTargetGroup.WebGL)
                    selectedConfig.scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", selectedConfig.scriptingBackend);

                // IL2CPP Configuration
                if (selectedConfig.scriptingBackend == ScriptingImplementation.IL2CPP |
                    selectedConfig.buildTargetGroup == BuildTargetGroup.iOS |
                    selectedConfig.buildTargetGroup == BuildTargetGroup.WebGL)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("IL2CPP Configuration", EditorStyles.boldLabel);

                    DrawApiCompatibilityLevel(selectedConfig);
                    selectedConfig.il2CppCodeGeneration = (Il2CppCodeGeneration)EditorGUILayout.EnumPopup("IL2CPP Code Generation", selectedConfig.il2CppCodeGeneration);
                    selectedConfig.il2CppCompilerConfiguration = (Il2CppCompilerConfiguration)EditorGUILayout.EnumPopup("IL2CPP Compiler Configuration", selectedConfig.il2CppCompilerConfiguration);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Optimization Configuration", EditorStyles.boldLabel);

                if (selectedConfig.buildTargetGroup == BuildTargetGroup.Standalone &
                    selectedConfig.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
                    selectedConfig.dedicatedServerOptimizations = EditorGUILayout.Toggle("Dedicated Server Optimizations", selectedConfig.dedicatedServerOptimizations);
                selectedConfig.prebakeCollisionMeshes = EditorGUILayout.Toggle("Prebake Collision Meshes", selectedConfig.prebakeCollisionMeshes);
                selectedConfig.managedStrippingLevel = (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Managed Stripping Level", selectedConfig.managedStrippingLevel);
                selectedConfig.optimizeMeshData = EditorGUILayout.Toggle("Optimize Mesh Data", selectedConfig.optimizeMeshData);
                selectedConfig.textureMipMapStriping = EditorGUILayout.Toggle("Texture Mipmap Stripping", selectedConfig.textureMipMapStriping);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Graphics Configuration", EditorStyles.boldLabel);

                // Graphics API (WebGL only)
                if (selectedConfig.buildTargetGroup == BuildTargetGroup.WebGL)
                {
                    selectedConfig.webGLGraphics = (WebGLGraphics)EditorGUILayout.EnumPopup("WebGL Graphics API", selectedConfig.webGLGraphics);
                }

                // Batching settings
                selectedConfig.staticBatching = EditorGUILayout.Toggle("Static Batching", selectedConfig.staticBatching);
                selectedConfig.dynamicBatching = EditorGUILayout.Toggle("Dynamic Batching", selectedConfig.dynamicBatching);

                // Sprite batching settings
                EditorGUI.indentLevel++;
                selectedConfig.spriteBatchingThreshold = EditorGUILayout.IntSlider("Sprite Batching Threshold", 
                    selectedConfig.spriteBatchingThreshold, 300, 8000);
                selectedConfig.spriteBatchingMaxVertexCount = EditorGUILayout.IntSlider("Sprite Batching Max Vertex Count", 
                    selectedConfig.spriteBatchingMaxVertexCount, 1024, 65535);
                EditorGUI.indentLevel--;

                // Other graphics options
                selectedConfig.skinningMethod = (SkinningMethod)EditorGUILayout.EnumPopup("Skinning Method", selectedConfig.skinningMethod);
                selectedConfig.graphicsJobs = EditorGUILayout.Toggle("Graphics Jobs", selectedConfig.graphicsJobs);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Publishing Configuration", EditorStyles.boldLabel);

                if (selectedConfig.buildTargetGroup == BuildTargetGroup.Android)
                {
                    selectedConfig.minifyType = (MinifyType)EditorGUILayout.EnumPopup("Minify Type", selectedConfig.minifyType);
                    selectedConfig.useCustomKeystore = EditorGUILayout.Toggle("Use Custom Keystore", selectedConfig.useCustomKeystore);
                    if (selectedConfig.useCustomKeystore)
                    {
                        selectedConfig.keystoreSettingsFile = EditorGUILayout.TextField("Keystore Settings File", selectedConfig.keystoreSettingsFile);

                        if (GUILayout.Button("Browse Keystore Settings File"))
                        {
                            string path = EditorUtility.OpenFilePanel("Select Keystore Settings File", "", "json");
                            if (!string.IsNullOrEmpty(path))
                            {
                                selectedConfig.keystoreSettingsFile = path;
                            }
                        }
                    }
                }
                else if (selectedConfig.buildTargetGroup == BuildTargetGroup.WebGL)
                {
                    selectedConfig.webGLExceptionSupport = (WebGLExceptionSupport)EditorGUILayout.EnumPopup("WebGL Exception Support", selectedConfig.webGLExceptionSupport);
                    selectedConfig.webGLcompressionFormat = (WebGLCompressionFormat)EditorGUILayout.EnumPopup("WebGL Compression Format", selectedConfig.webGLcompressionFormat);
                    selectedConfig.dataCaching = EditorGUILayout.Toggle("WebGL Data Caching", selectedConfig.dataCaching);
                    selectedConfig.webGLdebugSymbolsMode = (WebGLDebugSymbolMode)EditorGUILayout.EnumPopup("WebGL Debug Symbol Mode", selectedConfig.webGLdebugSymbolsMode);
                    selectedConfig.buildAdditionalMobileData = EditorGUILayout.Toggle("WebGL Build Additional Mobile Data", selectedConfig.buildAdditionalMobileData);
                }

                EditorGUILayout.Space();

                selectedConfig.enableDevelopmentBuild = EditorGUILayout.Toggle("Development Build", selectedConfig.enableDevelopmentBuild);

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Build"))
                {
                    BuildScript.BuildProject(selectedConfig, false);
                }
                if (GUILayout.Button("Clean Build"))
                {
                    BuildScript.BuildProject(selectedConfig, true);
                }
                EditorGUILayout.EndHorizontal();

                // Save configuration
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No configurations available. Please create a new one.");
            }
        }

        private static void DrawConfigurationName(BuildConfig config)
        {
            EditorGUILayout.Space();
            string oldConfigName = config.name;
            
            string newName = EditorGUILayout.DelayedTextField("Configuration Name", config.name);
            
            if (newName != oldConfigName)
            {
                string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(config), newName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"Failed to rename configuration: {error}");
                }
                else
                {
                    config.name = newName;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        
        private static void DrawBuildPath(BuildConfig config)
        {
            EditorGUILayout.Space();
            config.buildPath = EditorGUILayout.TextField("Build Path", config.buildPath);
        }
        
        private static void DrawTargetPlatform(BuildConfig config)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Target Platform", EditorStyles.boldLabel);

            config.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", config.buildTarget);
            config.buildTargetGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup("Build Target Group", config.buildTargetGroup);
            if (config.buildTargetGroup == BuildTargetGroup.Standalone)
                config.standaloneBuildSubtarget = (StandaloneBuildSubtarget)EditorGUILayout.EnumPopup("Target Platform", config.standaloneBuildSubtarget);
            if (config.buildTargetGroup == BuildTargetGroup.Standalone)
            {
                config.standaloneBuildSubtarget = (StandaloneBuildSubtarget)EditorGUILayout.EnumPopup("Standalone Subtarget", config.standaloneBuildSubtarget);
            }
        }
        
        private static void DrawCompression(BuildConfig config)
        {
            EditorGUILayout.Space();
            config.compression = (Compression)EditorGUILayout.EnumPopup("Compression", config.compression);
        }

        private static string[] GetConfigurationNames()
        {
            var settings = BuildConfigProjectSettings.Instance;
            var names = new string[settings.buildConfigurations.Count];
            for (int i = 0; i < settings.buildConfigurations.Count; i++)
            {
                names[i] = settings.buildConfigurations[i].name;
            }
            return names;
        }

        private static void DrawScenesList(BuildConfig config)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);
            
            // Synchronize the temporary list with actual scene paths
            SyncTempScenesList(config);
            
            EditorGUI.indentLevel++;
            
            // Standard Unity-style list with +/- buttons
            for (int i = 0; i < config.scenesToBuild.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Scene object field
                EditorGUI.BeginChangeCheck();
                SceneAsset sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(
                    $"Scene {i + 1}", 
                    tempSceneAssets[i], 
                    typeof(SceneAsset), 
                    false);
                    
                if (EditorGUI.EndChangeCheck())
                {
                    if (sceneAsset != null)
                    {
                        string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                        string relativePath = scenePath.Replace("Assets/", "");
                        config.scenesToBuild[i] = relativePath;
                        tempSceneAssets[i] = sceneAsset;
                        EditorUtility.SetDirty(config);
                    }
                    else
                    {
                        // User cleared the field
                        config.scenesToBuild[i] = "";
                        tempSceneAssets[i] = null;
                        EditorUtility.SetDirty(config);
                    }
                }
                
                // Remove button
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    config.scenesToBuild.RemoveAt(i);
                    tempSceneAssets.RemoveAt(i);
                    EditorUtility.SetDirty(config);
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Add button row
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Scene", GUILayout.Width(120)))
            {
                config.scenesToBuild.Add("");
                tempSceneAssets.Add(null);
                EditorUtility.SetDirty(config);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // Copy scenes option
            config.copyScenes = EditorGUILayout.Toggle("Copy Scenes", config.copyScenes);
            if (config.copyScenes)
            {
                EditorGUI.indentLevel++;
                config.buildScenesPath = EditorGUILayout.TextField("Build Scenes Path", config.buildScenesPath);
                EditorGUI.indentLevel--;
            }
        }

        private static void SyncTempScenesList(BuildConfig config)
        {
            // Initialize or reset the temporary list
            if (tempSceneAssets == null)
                tempSceneAssets = new List<UnityEngine.Object>();
                
            // Ensure the lists have the same count
            while (tempSceneAssets.Count < config.scenesToBuild.Count)
            {
                tempSceneAssets.Add(null);
            }
            
            while (tempSceneAssets.Count > config.scenesToBuild.Count)
            {
                tempSceneAssets.RemoveAt(tempSceneAssets.Count - 1);
            }
            
            // Fill the list with scene assets based on current paths
            for (int i = 0; i < config.scenesToBuild.Count; i++)
            {
                string scenePath = config.scenesToBuild[i];
                if (!string.IsNullOrEmpty(scenePath))
                {
                    string fullPath = "Assets/" + scenePath;
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(fullPath);
                    tempSceneAssets[i] = sceneAsset;
                }
            }
        }

        /// <summary>
        /// Draws a clean API Compatibility Level dropdown without deprecated options
        /// </summary>
        private static void DrawApiCompatibilityLevel(BuildConfig config)
        {
            // Define current, non-deprecated API compatibility options in display order
            var compatibilityOptions = new[]
            {
                new { Label = ".NET Standard 2.1", Value = ApiCompatibilityLevel.NET_Standard },
                new { Label = ".NET Framework 4.8", Value = ApiCompatibilityLevel.NET_Unity_4_8 },
                new { Label = ".NET 2.0", Value = ApiCompatibilityLevel.NET_2_0 },
                new { Label = ".NET 2.0 Subset", Value = ApiCompatibilityLevel.NET_2_0_Subset },
                new { Label = ".NET Micro", Value = ApiCompatibilityLevel.NET_Micro },
                new { Label = ".NET Web", Value = ApiCompatibilityLevel.NET_Web }
            };
            
            // Get display names for the popup
            string[] displayOptions = compatibilityOptions.Select(o => o.Label).ToArray();
            
            // Find the index of the current value
            int currentIndex = Array.FindIndex(compatibilityOptions, o => o.Value == config.compatibilityLevel);
            if (currentIndex < 0) currentIndex = 0; // Default to first option if not found
            
            // Draw the popup
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("API Compatibility Level", currentIndex, displayOptions);
            if (EditorGUI.EndChangeCheck())
            {
                // Map the selected index back to the enum value
                config.compatibilityLevel = compatibilityOptions[newIndex].Value;
                EditorUtility.SetDirty(config);
            }
        }
    }
}
#endif