#if UNITY_EDITOR
using System;
using System.IO;
using Energy8.BuildSystem.Configuration;
using Energy8.BuildSystem.Utils;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Energy8.BuildSystem.Platform
{
    public static class WebGLBuilder
    {
        public static void ConfigureWebGLSettings(BuildConfig config, BuildLogger logger)
        {
            logger.Log("Configuring WebGL publishing settings...");

            // Configure WebGL-specific settings
            PlayerSettings.WebGL.exceptionSupport = config.webGLExceptionSupport;
            PlayerSettings.WebGL.dataCaching = config.dataCaching;
            PlayerSettings.WebGL.compressionFormat = config.webGLcompressionFormat;
            PlayerSettings.WebGL.debugSymbolMode = config.webGLdebugSymbolsMode;
            
            // Set WebGL graphics API
            switch(config.webGLGraphics)
            {
                case WebGLGraphics.WebGL2:
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
                    logger.Log("Setting WebGL graphics API to WebGL 2.0");
                    break;
                case WebGLGraphics.WebGPU:
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.WebGPU });
                    logger.Log("Setting WebGL graphics API to WebGPU");
                    break;
                case WebGLGraphics.Auto:
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, true);
                    logger.Log("Setting WebGL graphics API to Auto");
                    break;
            }

            if (config.buildAdditionalMobileData)
            {
                logger.Log("Configuring settings for additional mobile data build.");
            }
        }

        public static void BuildMobileData(BuildConfig config, BuildLogger logger)
        {
            string mobileBuildPath = $"{config.buildPath}_Mobile";
            logger.Log($"Building additional mobile data for WebGL at {mobileBuildPath}...");

            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

            BuildPlayerOptions mobileBuildOptions = new()
            {
                scenes = config.scenesToBuild.ToArray(),
                locationPathName = mobileBuildPath,
                targetGroup = BuildTargetGroup.WebGL,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(mobileBuildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                logger.Log("Mobile WebGL build succeeded.");

                string dataFileName = new DirectoryInfo(mobileBuildPath).Name + ".data" + config.webGLcompressionFormat switch
                {
                    WebGLCompressionFormat.Gzip => ".gz",
                    WebGLCompressionFormat.Brotli => ".br",
                    _ => ""
                };

                // Copy data file to main build folder
                string dataFilePath = Path.Combine(mobileBuildPath, "Build", dataFileName);
                string destinationPath = Path.Combine(config.buildPath, "Build", dataFileName);
                if (File.Exists(dataFilePath))
                {
                    File.Copy(dataFilePath, destinationPath, overwrite: true);
                    logger.Log("Copied mobile data file to main build folder.");
                }
                else
                {
                    logger.LogError("Mobile data file not found.");
                    throw new Exception("Mobile data file not found.");
                }
                Directory.Delete(mobileBuildPath, true);
            }
            else
            {
                logger.LogError("Mobile WebGL build failed.");
                throw new Exception("Mobile WebGL build failed.");
            }
        }
    }
}
#endif