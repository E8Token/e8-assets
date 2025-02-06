#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Energy8.Identity.Core.Configuration.Models;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Energy8
{
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "Configs/Build Config")]
    public class BuildConfig : ScriptableObject
    {
        public string buildPath = "Builds/";

        [Header("Platform")]
        public BuildTarget buildTarget;
        public BuildTargetGroup buildTargetGroup;
        public StandaloneBuildSubtarget standaloneBuildSubtarget;

        [Space]
        public Compression compression;

        [Header("HTTP")]
        public IPType ipType;
        public InsecureHttpOption insecureHttpOption;

        [Header("Scenes")]
        public List<string> scenesToBuild = new();
        public bool copyScenes;
        public string buildScenesPath = "Assets/Scenes/Build/";

        [Space]
        public ScriptingImplementation scriptingBackend;

        [Header("IL2CPP")]
        public ApiCompatibilityLevel compatibilityLevel;
        public Il2CppCodeGeneration il2CppCodeGeneration;
        public Il2CppCompilerConfiguration il2CppCompilerConfiguration;

        [Header("Optimization")]
        public bool dedicatedServerOptimizations; // For Server
        public bool prebakeCollisionMeshes;
        public ManagedStrippingLevel managedStrippingLevel;
        public bool optimizeMeshData;
        public bool textureMipMapStriping;

        [Header("Publishing")]
        [Header("Android")]
        public bool useCustomKeystore;
        public MinifyType minifyType;
        public string keystoreSettingsFile;

        [Header("WebGL")]
        public WebGLExceptionSupport webGLExceptionSupport;
        public bool dataCaching;
        public WebGLCompressionFormat webGLcompressionFormat;
        public WebGLDebugSymbolMode webGLdebugSymbolsMode;
        public bool buildAdditionalMobileData;

        [Space]
        public bool enableDevelopmentBuild;

        public bool ValidateConfig()
        {
            if (scenesToBuild == null || scenesToBuild.Count < 1)
            {
                Debug.LogWarning("No scenes specified for build.");
                return false;
            }

            if (copyScenes && string.IsNullOrEmpty(buildScenesPath))
            {
                Debug.LogWarning("Build scenes path must be specified if copyScenes is true.");
                return false;
            }

            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.LogWarning("Build path is not specified.");
                return false;
            }

            return true;
        }
    }

    public enum MinifyType
    {
        None,
        Release,
        Debug
    }

    [Serializable]
    public class KeystoreConfig
    {
        public string keystorePath;
        public string keystorePassword;
        public string keyAlias;
        public string keyAliasPassword;
    }

    public enum Compression
    {
        None = 0,
        Lz4 = 2,
        Lz4HC = 3
    }
}
#endif