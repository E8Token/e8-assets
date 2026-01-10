using UnityEngine;

namespace Energy8.EnvironmentConfig.Editor.Settings
{
    /// <summary>
    /// Represents a single build environment configuration
    /// </summary>
    [System.Serializable]
    public class EnvironmentData
    {
        public string name;
        public Color color = Color.white;

        public EnvironmentData()
        {
        }

        public EnvironmentData(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }
    }

    /// <summary>
    /// Settings asset for managing available environments
    /// Created and stored in Assets/Settings/
    /// </summary>
    [CreateAssetMenu(fileName = "E8EnvironmentSettings", menuName = "E8 Config/Environment Settings")]
    public class EnvironmentSettings : ScriptableObject
    {
        [Header("Available Environments")]
        [Tooltip("List of available build environments")]
        public EnvironmentData[] environments = new EnvironmentData[]
        {
            new EnvironmentData("Development", new Color(0.4f, 0.8f, 0.4f)),
            new EnvironmentData("Debug", new Color(0.8f, 0.6f, 0.2f)),
            new EnvironmentData("Production", new Color(0.8f, 0.2f, 0.2f))
        };

        /// <summary>
        /// Find settings asset in project
        /// </summary>
        public static EnvironmentSettings FindSettings()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EnvironmentSettings");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<EnvironmentSettings>(path);
            }
            return null;
        }

        /// <summary>
        /// Create default settings asset if not exists
        /// </summary>
        public static EnvironmentSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<EnvironmentSettings>();
            string path = "Assets/Settings/E8EnvironmentSettings.asset";
            UnityEditor.AssetDatabase.CreateAsset(settings, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return settings;
        }

        /// <summary>
        /// Get or create settings asset
        /// </summary>
        public static EnvironmentSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateSettings();
            }
            return settings;
        }
    }
}
