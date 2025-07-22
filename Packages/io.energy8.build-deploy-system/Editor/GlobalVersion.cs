using UnityEngine;
using UnityEditor;

namespace Energy8.BuildDeploySystem
{
    /// <summary>
    /// ScriptableObject-based implementation for managing global project versioning.
    /// Provides version management functionality with major.minor.build format.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalVersion", menuName = "Energy8/Global Version")]
    public class GlobalVersion : ScriptableObject
    {
        [SerializeField] private string version = "1.0.1";

        /// <summary>
        /// Gets or sets the current version string.
        /// Format: Major.Minor.Build (e.g., "1.0.123")
        /// </summary>
        public string Version
        {
            get => version;
            set 
            { 
                version = value;
                EditorUtility.SetDirty(this);
            }
        }

        private static GlobalVersion instance;

        /// <summary>
        /// Gets the singleton instance of GlobalVersion.
        /// Creates a new instance if none exists.
        /// </summary>
        public static GlobalVersion Instance
        {
            get
            {
                if (instance == null)
                {
                    // Search for existing instance
                    string[] guids = AssetDatabase.FindAssets("t:GlobalVersion");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath<GlobalVersion>(path);
                    }

                    // Create new instance if not found
                    if (instance == null)
                    {
                        instance = CreateInstance<GlobalVersion>();
                        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Settings");
                        }
                        AssetDatabase.CreateAsset(instance, "Assets/Settings/GlobalVersion.asset");
                        AssetDatabase.SaveAssets();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Increments the minor version number and resets the build number to 0.
        /// Example: "1.2.45" becomes "1.3.0"
        /// </summary>
        public void IncrementMinor()
        {
            var parts = version.Split('.');
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[1], out int minor))
                {
                    minor++;
                    if (parts.Length >= 3)
                    {
                        Version = $"{parts[0]}.{minor}.0";
                    }
                    else
                    {
                        Version = $"{parts[0]}.{minor}";
                    }
                }
            }
        }

        /// <summary>
        /// Increments the major version number and resets minor and build numbers to 0.
        /// Example: "1.2.45" becomes "2.0.0"
        /// </summary>
        public void IncrementMajor()
        {
            var parts = version.Split('.');
            if (parts.Length >= 1)
            {
                if (int.TryParse(parts[0], out int major))
                {
                    major++;
                    if (parts.Length >= 3)
                    {
                        Version = $"{major}.0.0";
                    }
                    else if (parts.Length >= 2)
                    {
                        Version = $"{major}.0";
                    }
                    else
                    {
                        Version = major.ToString();
                    }
                }
            }
        }        /// <summary>
        /// Increments the build number.
        /// Example: "1.2.45" becomes "1.2.46"
        /// </summary>
        public void IncrementBuild()
        {
            var parts = version.Split('.');
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[2], out int build))
                {
                    build++;
                    Version = $"{parts[0]}.{parts[1]}.{build}";
                }
            }
            else if (parts.Length == 2)
            {
                // If version is like 1.2, add .1
                Version = $"{version}.1";
            }
        }

        /// <summary>
        /// Generates a bundle identifier based on the current version.
        /// </summary>
        /// <param name="baseBundleId">The base bundle identifier to use.</param>
        /// <returns>A bundle identifier string suitable for mobile platforms.</returns>
        public string GenerateBundleId(string baseBundleId = "com.energy8.game")
        {
            var versionSuffix = version.Replace(".", "");
            return $"{baseBundleId}.v{versionSuffix}";
        }
        
        /// <summary>
        /// Parses the version string into its components.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build number.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool TryParseVersion(out int major, out int minor, out int build)
        {
            major = 0;
            minor = 0;
            build = 0;
            
            var parts = version.Split('.');
            if (parts.Length >= 1 && int.TryParse(parts[0], out major))
            {
                if (parts.Length >= 2 && int.TryParse(parts[1], out minor))
                {
                    if (parts.Length >= 3 && int.TryParse(parts[2], out build))
                    {
                        return true;
                    }
                    return true; // Valid with major.minor format
                }
                return true; // Valid with major format only
            }
            return false;
        }
        
        /// <summary>
        /// Validates the version string format.
        /// </summary>
        /// <returns>True if the version string is in a valid format, false otherwise.</returns>
        public bool IsValidVersion()
        {
            return TryParseVersion(out _, out _, out _);
        }
    }
}
