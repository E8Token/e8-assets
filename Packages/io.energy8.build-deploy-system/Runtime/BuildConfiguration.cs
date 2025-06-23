using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Profile;
#endif

namespace Energy8.BuildDeploySystem
{
    [CreateAssetMenu(fileName = "BuildConfiguration", menuName = "Energy8/Build Configuration")]
    public class BuildConfiguration : ScriptableObject
    {        [Header("Build Profile Reference")]
        [SerializeField] private string buildProfileGUID;
        [SerializeField] private string buildProfileName;
        
        [Header("Build Settings")]
        [SerializeField] private string outputPath;
        [SerializeField] private bool cleanBeforeBuild = true;
        [SerializeField] private string customBuildName;
        
        [Header("Deploy Settings")]
        [SerializeField] private DeploySettings deploySettings = new DeploySettings();
        
        [Header("Platform Specific Settings")]
        [SerializeField] private WebGLSettings webGLSettings = new WebGLSettings();
        [SerializeField] private AndroidSettings androidSettings = new AndroidSettings();
        [SerializeField] private IOSSettings iosSettings = new IOSSettings();
        [SerializeField] private StandaloneSettings standaloneSettings = new StandaloneSettings();

#if UNITY_EDITOR
        private BuildProfile cachedBuildProfile;
        
        public BuildProfile BuildProfile
        {
            get
            {
                if (cachedBuildProfile == null && !string.IsNullOrEmpty(buildProfileGUID))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(buildProfileGUID);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        cachedBuildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);
                    }
                }
                return cachedBuildProfile;
            }
        }
        
        public void SetBuildProfile(BuildProfile profile)
        {
            if (profile != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(profile);                buildProfileGUID = AssetDatabase.AssetPathToGUID(assetPath);
                buildProfileName = profile.name;
                cachedBuildProfile = profile;
                
                // Автоматически генерируем путь сборки
                GenerateOutputPath();
                
                EditorUtility.SetDirty(this);
            }
            else
            {
                buildProfileGUID = string.Empty;
                buildProfileName = string.Empty;
                cachedBuildProfile = null;
                EditorUtility.SetDirty(this);
            }
        }        private void GenerateOutputPath()
        {
            if (BuildProfile != null)
            {
                outputPath = $"Builds/{buildProfileName}";
            }
        }
        
        public void RefreshBuildProfileReference()
        {
            if (!string.IsNullOrEmpty(buildProfileGUID))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(buildProfileGUID);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);                    if (profile != null && profile.name != buildProfileName)
                    {
                        buildProfileName = profile.name;
                        // Автоматически обновляем путь сборки
                        GenerateOutputPath();
                        EditorUtility.SetDirty(this);
                    }
                }
            }
        }
#endif

        public string BuildProfileGUID => buildProfileGUID;
        public string BuildProfileName => buildProfileName;
        
        public string OutputPath
        {
            get => outputPath;
            set 
            { 
                outputPath = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }        }

        public bool CleanBeforeBuild
        {
            get => cleanBeforeBuild;
            set 
            { 
                cleanBeforeBuild = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public string CustomBuildName
        {
            get => customBuildName;
            set 
            { 
                customBuildName = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public WebGLSettings WebGLSettings
        {
            get => webGLSettings;
            set
            {
                webGLSettings = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public AndroidSettings AndroidSettings
        {
            get => androidSettings;
            set
            {
                androidSettings = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public IOSSettings IOSSettings
        {
            get => iosSettings;
            set
            {
                iosSettings = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }        public bool IsValid()
        {
#if UNITY_EDITOR
            return BuildProfile != null && !string.IsNullOrEmpty(outputPath);
#else
            return !string.IsNullOrEmpty(buildProfileGUID) && !string.IsNullOrEmpty(outputPath);
#endif
        }

        /// <summary>
        /// Возвращает отображаемое имя конфигурации
        /// </summary>
        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(buildProfileName) ? 
                buildProfileName : 
                "Unknown Profile";
        }

        public DeploySettings DeploySettings
        {
            get => deploySettings;
            set
            {
                deploySettings = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }        public string GetMainWebGLTextureFormat()
        {
#if UNITY_EDITOR
            if (BuildProfile != null)
            {
                try
                {
                    // Простой fallback - пока не можем надежно читать из Build Profile
                    // Возвращаем формат из текущих настроек редактора
                    Debug.Log($"Getting WebGL texture format for Build Profile: {BuildProfile.name}");
                    
                    // TODO: Implement proper Build Profile texture format reading when Unity API allows it
                    // Пока используем fallback
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to get WebGL texture format from profile: {ex.Message}");
                }
            }
            
            // Fallback к глобальным настройкам если профиль недоступен
            switch (EditorUserBuildSettings.webGLBuildSubtarget)
            {
                case WebGLTextureSubtarget.DXT:
                    return "DXT";
                case WebGLTextureSubtarget.ASTC:
                    return "ASTC";
                case WebGLTextureSubtarget.ETC2:
                    return "ETC2";
                default:
                    return "ETC2";
            }
#else
            return "ETC2";
#endif
        }
    }
}
