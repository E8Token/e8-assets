#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Build;

namespace Energy8
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
                EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

                // Scenes to Build
                EditorGUILayout.LabelField("Scenes to Build");
                for (int i = 0; i < selectedConfig.scenesToBuild.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Scene path
                    selectedConfig.scenesToBuild[i] = EditorGUILayout.TextField($"Scene {i + 1}", selectedConfig.scenesToBuild[i]);

                    // Remove scene
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        selectedConfig.scenesToBuild.RemoveAt(i);
                        i--; // Adjust index after removal
                    }

                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add Scene"))
                {
                    string scenePath = "Scenes/";
                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        selectedConfig.scenesToBuild.Add(scenePath);
                    }
                }

                // Copy scenes option
                selectedConfig.copyScenes = EditorGUILayout.Toggle("Copy Scenes", selectedConfig.copyScenes);
                if (selectedConfig.copyScenes)
                {
                    selectedConfig.buildScenesPath = EditorGUILayout.TextField("Build Scenes Path", selectedConfig.buildScenesPath);
                }

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

                    selectedConfig.compatibilityLevel = (ApiCompatibilityLevel)EditorGUILayout.EnumPopup("API Compatibility Level", selectedConfig.compatibilityLevel);
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
            config.name = EditorGUILayout.TextField("Configuration Name", config.name);
            if (config.name != oldConfigName)
            {
                string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(config), config.name);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"Failed to rename configuration: {error}");
                    config.name = oldConfigName;
                }
                else
                {
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
    }
}
#endif