using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Energy8.BuildDeploySystem.Editor
{
    [CustomEditor(typeof(BuildConfiguration))]
    public class BuildConfigurationEditor : UnityEditor.Editor
    {
        private SerializedProperty configNameProp;
        private SerializedProperty buildProfileProp;
        private SerializedProperty autoIncrementProp;
        private SerializedProperty versionProp;
        private SerializedProperty versionIncrementTypeProp;
        private SerializedProperty buildTargetProp;
        private SerializedProperty bundleIdProp;
        private SerializedProperty webglCompressionMethodsProp;
        private SerializedProperty autoDeployOnSuccessProp;
        private SerializedProperty deployConfigProp;
        
        private bool showWebGLSettings = true;
        private bool showDeploySettings = true;
        private bool showVersionSettings = true;
        
        private void OnEnable()
        {
            configNameProp = serializedObject.FindProperty("configName");
            buildProfileProp = serializedObject.FindProperty("buildProfile");
            autoIncrementProp = serializedObject.FindProperty("autoIncrement");
            versionProp = serializedObject.FindProperty("version");
            versionIncrementTypeProp = serializedObject.FindProperty("versionIncrementType");
            buildTargetProp = serializedObject.FindProperty("buildTarget");
            bundleIdProp = serializedObject.FindProperty("bundleId");
            webglCompressionMethodsProp = serializedObject.FindProperty("webglCompressionMethods");
            autoDeployOnSuccessProp = serializedObject.FindProperty("autoDeployOnSuccess");
            deployConfigProp = serializedObject.FindProperty("deployConfig");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var config = target as BuildConfiguration;
            
            EditorGUILayout.LabelField("Build Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // General Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(configNameProp, new GUIContent("Configuration Name"));
            EditorGUILayout.PropertyField(buildProfileProp, new GUIContent("Build Profile"));
            EditorGUILayout.PropertyField(buildTargetProp, new GUIContent("Build Target"));
            EditorGUILayout.PropertyField(bundleIdProp, new GUIContent("Bundle ID"));
            
            EditorGUILayout.EndVertical();
            
            // Version Settings
            EditorGUILayout.Space(5);
            showVersionSettings = EditorGUILayout.Foldout(showVersionSettings, "Version Settings", true);
            if (showVersionSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(versionProp, new GUIContent("Version"));
                EditorGUILayout.PropertyField(versionIncrementTypeProp, new GUIContent("Increment Type"));
                EditorGUILayout.PropertyField(autoIncrementProp, new GUIContent("Auto Increment on Build"));
                
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Increment Major"))
                {
                    config.versionIncrementType = VersionIncrementType.Major;
                    config.IncrementVersion();
                    EditorUtility.SetDirty(config);
                }
                
                if (GUILayout.Button("Increment Minor"))
                {
                    config.versionIncrementType = VersionIncrementType.Minor;
                    config.IncrementVersion();
                    EditorUtility.SetDirty(config);
                }
                
                if (GUILayout.Button("Increment Patch"))
                {
                    config.versionIncrementType = VersionIncrementType.Auto;
                    config.IncrementVersion();
                    EditorUtility.SetDirty(config);
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            
            // WebGL Settings
            if (config.buildTarget == BuildTargetType.WebGL)
            {
                EditorGUILayout.Space(5);
                showWebGLSettings = EditorGUILayout.Foldout(showWebGLSettings, "WebGL Settings", true);
                if (showWebGLSettings)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Texture Compression Methods", EditorStyles.boldLabel);
                    
                    DrawTextureCompressionSettings(config);
                    
                    EditorGUILayout.EndVertical();
                }
            }
            
            // Deploy Settings
            EditorGUILayout.Space(5);
            showDeploySettings = EditorGUILayout.Foldout(showDeploySettings, "Deploy Settings", true);
            if (showDeploySettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(autoDeployOnSuccessProp, new GUIContent("Auto Deploy on Success"));
                
                var deployEnabled = deployConfigProp.FindPropertyRelative("enabled");
                EditorGUILayout.PropertyField(deployEnabled, new GUIContent("Enable Deploy"));
                
                if (deployEnabled.boolValue)
                {
                    EditorGUI.indentLevel++;
                    
                    var host = deployConfigProp.FindPropertyRelative("host");
                    var username = deployConfigProp.FindPropertyRelative("username");
                    var keyPath = deployConfigProp.FindPropertyRelative("keyPath");
                    var remotePath = deployConfigProp.FindPropertyRelative("remotePath");
                    var port = deployConfigProp.FindPropertyRelative("port");
                    
                    EditorGUILayout.PropertyField(host, new GUIContent("Host"));
                    EditorGUILayout.PropertyField(username, new GUIContent("Username"));
                    EditorGUILayout.PropertyField(port, new GUIContent("Port"));
                    EditorGUILayout.PropertyField(remotePath, new GUIContent("Remote Path"));
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(keyPath, new GUIContent("SSH Key Path"));
                    if (GUILayout.Button("Browse", GUILayout.Width(60)))
                    {
                        string path = EditorUtility.OpenFilePanel("Select SSH Key", "", "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            keyPath.stringValue = path;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // Build Actions
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Build Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            UnityEngine.GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(config, false);
            }
            
            UnityEngine.GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Clean Build", GUILayout.Height(30)))
            {
                BuildSystem.StartBuild(config, true);
            }
            
            UnityEngine.GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawTextureCompressionSettings(BuildConfiguration config)
        {
            EditorGUILayout.HelpBox("Select one or more texture compression methods for WebGL builds. Multiple methods will create separate builds with renamed data files.", MessageType.Info);
            
            bool dxt = config.webglCompressionMethods.Contains(TextureCompressionMethod.DXT);
            bool astc = config.webglCompressionMethods.Contains(TextureCompressionMethod.ASTC);
            bool etc2 = config.webglCompressionMethods.Contains(TextureCompressionMethod.ETC2);
            
            EditorGUILayout.BeginVertical();
            
            bool newDxt = EditorGUILayout.Toggle("DXT (Desktop, older mobile)", dxt);
            bool newAstc = EditorGUILayout.Toggle("ASTC (Modern mobile)", astc);
            bool newEtc2 = EditorGUILayout.Toggle("ETC2 (Android, some iOS)", etc2);
            
            EditorGUILayout.EndVertical();
            
            if (newDxt != dxt)
            {
                if (newDxt)
                    config.webglCompressionMethods.Add(TextureCompressionMethod.DXT);
                else
                    config.webglCompressionMethods.Remove(TextureCompressionMethod.DXT);
                EditorUtility.SetDirty(config);
            }
            
            if (newAstc != astc)
            {
                if (newAstc)
                    config.webglCompressionMethods.Add(TextureCompressionMethod.ASTC);
                else
                    config.webglCompressionMethods.Remove(TextureCompressionMethod.ASTC);
                EditorUtility.SetDirty(config);
            }
            
            if (newEtc2 != etc2)
            {
                if (newEtc2)
                    config.webglCompressionMethods.Add(TextureCompressionMethod.ETC2);
                else
                    config.webglCompressionMethods.Remove(TextureCompressionMethod.ETC2);
                EditorUtility.SetDirty(config);
            }
            
            if (config.webglCompressionMethods.Count == 0)
            {
                EditorGUILayout.HelpBox("At least one compression method must be selected for WebGL builds.", MessageType.Warning);
            }
        }
    }
}
