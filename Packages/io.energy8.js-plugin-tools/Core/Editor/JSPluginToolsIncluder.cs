using System.IO;
using UnityEditor;
using UnityEngine;

namespace Energy8.JSPluginTools.Core.Editor
{
    /// <summary>
    /// Utility for including the JS Plugin Tools JavaScript library in WebGL builds.
    /// </summary>
    [InitializeOnLoad]
    public class JSPluginToolsIncluder
    {
        private const string ResourcesPath = "Packages/io.energy8.js-plugin-tools/Core/Runtime/Resources/unity-web-plugin.js";
        private const string TemplateSourcePath = "Packages/io.energy8.js-plugin-tools/Core/Runtime/Resources/webgl-template";
        private const string TemplateFileName = "JSPluginTools";
        
        static JSPluginToolsIncluder()
        {
            EditorApplication.delayCall += Initialize;
        }
        
        private static void Initialize()
        {
            // Register for build events
            BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
        }
        
        private static void OnBuildPlayer(BuildPlayerOptions options)
        {
            // Only process for WebGL builds
            if (options.target != BuildTarget.WebGL)
                return;
            
            Debug.Log("[JSPluginTools] Setting up WebGL build...");
            
            // Ensure the JS file is included in the build
            IncludeJsLibrary();
            
            // Build the player
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
        
        private static void IncludeJsLibrary()
        {
            // Make sure the JS file is included in the WebGL template
            if (!PlayerSettings.WebGL.template.Contains(TemplateFileName))
            {
                Debug.LogWarning($"[JSPluginTools] WebGL template is not set to {TemplateFileName}. " +
                                "JS Plugin Tools functionality may not work correctly.");
            }
            
            // Copy the JS file to the correct location
            var jsLibraryPath = Path.Combine(Application.streamingAssetsPath, "unity-web-plugin.js");
            
            // Ensure StreamingAssets directory exists
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            
            try
            {
                File.Copy(ResourcesPath, jsLibraryPath, true);
                Debug.Log("[JSPluginTools] JavaScript library copied to StreamingAssets");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JSPluginTools] Error copying JavaScript library: {ex.Message}");
            }
        }
        
        [MenuItem("Tools/JS Plugin Tools/Install WebGL Template")]
        public static void InstallWebGLTemplate()
        {
            var templateDir = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/WebGLSupport/BuildTools/WebGLTemplates");
            var destPath = Path.Combine(templateDir, TemplateFileName);
            
            try
            {
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                
                foreach (var file in Directory.GetFiles(TemplateSourcePath))
                {
                    var fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(destPath, fileName), true);
                }
                
                Debug.Log("[JSPluginTools] WebGL template installed successfully");
                
                // Check if we need to set the template
                if (!PlayerSettings.WebGL.template.Contains(TemplateFileName))
                {
                    PlayerSettings.WebGL.template = TemplateFileName;
                    Debug.Log("[JSPluginTools] WebGL template set to JSPluginTools");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JSPluginTools] Error installing WebGL template: {ex.Message}");
            }
        }
    }
}
