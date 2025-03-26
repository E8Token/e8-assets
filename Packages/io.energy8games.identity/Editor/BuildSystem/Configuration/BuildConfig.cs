#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Energy8.Identity.Core.Configuration.Models;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Energy8.BuildSystem.Configuration
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

        [Header("Graphics Settings")]
        public WebGLGraphics webGLGraphics = WebGLGraphics.WebGL2;
        public bool staticBatching = true;
        public bool dynamicBatching = false;
        public int spriteBatchingThreshold = 300;
        public int spriteBatchingMaxVertexCount = 65535;
        public SkinningMethod skinningMethod = SkinningMethod.GPU;  // Changed from bool gpuSkinning
        public bool graphicsJobs = false;

        [Header("Optimization")]
        public bool dedicatedServerOptimizations;
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

        [Header("Development")]
        public bool enableDevelopmentBuild;

        public bool ValidateConfig()
        {
            // Implement validation logic
            return true;
        }
    }

    public enum MinifyType
    {
        None,
        Debug,
        Release
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
        None,
        Lz4,
        Lz4HC
    }

    public enum WebGLGraphics
    {
        WebGL2,
        WebGPU,
        Auto
    }

    public enum SkinningMethod
    {
        CPU,
        GPU,
        GPU_Batched
    }
}
#endif