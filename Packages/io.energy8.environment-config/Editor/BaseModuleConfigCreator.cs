#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Energy8.EnvironmentConfig.Base;

namespace Energy8.EnvironmentConfig.Editor
{
    /// <summary>
    /// Base class for creating module config templates via context menu
    /// Inherit this class for each module to add context menu item
    /// Creates a single template config that can be manually copied and renamed
    /// </summary>
    public abstract class BaseModuleConfigCreator<T> where T : BaseModuleConfig
    {
        /// <summary>
        /// Creates a single template config asset named {ModuleName}_Environment.asset
        /// The developer can then copy and rename this file for each environment
        /// </summary>
        public static void CreateTemplateConfig()
        {
            var basePath = "Assets/Resources/E8Config";
            Directory.CreateDirectory(basePath);

            var templatePath = $"{basePath}/{typeof(T).Name}_Environment";
            
            // Check if template already exists
            if (AssetDatabase.LoadAssetAtPath<T>(templatePath + ".asset") != null)
            {
                EditorUtility.DisplayDialog("Info", 
                    $"Template config already exists: {typeof(T).Name}_Environment.asset\n" +
                    "You can copy and rename it for each environment.", "OK");
                return;
            }

            try
            {
                var config = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(config, templatePath + ".asset");
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Select the created asset in Project View
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);

                EditorUtility.DisplayDialog("Success", 
                    $"Created template config: {typeof(T).Name}_Environment.asset\n\n" +
                    "Next steps:\n" +
                    "1. Configure the template with default values\n" +
                    "2. Copy and rename it for each environment (e.g., Development, Debug, Production)\n" +
                    "3. Example: {ClassName}_{EnvironmentName}.asset", "OK");
                
                Debug.Log($"[BaseModuleConfigCreator] Created template config: {typeof(T).Name}_Environment.asset");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BaseModuleConfigCreator] Failed to create template config: {e.Message}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to create template config: {e.Message}", "OK");
            }
        }
    }
}
#endif
