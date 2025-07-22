using UnityEditor;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    public class BuildProfileWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool needsUpdate = false;

            foreach (var asset in importedAssets)
            {
                if (IsBuildProfile(asset))
                {
                    needsUpdate = true;
                    break;
                }
            }

            if (!needsUpdate)
            {
                foreach (var asset in deletedAssets)
                {
                    if (IsBuildProfile(asset))
                    {
                        needsUpdate = true;
                        break;
                    }
                }
            }

            if (!needsUpdate)
            {
                foreach (var asset in movedAssets)
                {
                    if (IsBuildProfile(asset))
                    {
                        needsUpdate = true;
                        break;
                    }
                }
            }

            if (needsUpdate)
            {
                EditorApplication.delayCall += () =>
                {
                    BuildProfileScanner.ScanAndCreateConfigurations();
                };
            }
        }

        private static bool IsBuildProfile(string assetPath)
        {
            return assetPath.StartsWith("Assets/Settings/Build Profiles") && 
                   assetPath.EndsWith(".asset");
        }
    }

    [InitializeOnLoad]
    public static class BuildSystemInitializer
    {
        static BuildSystemInitializer()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            if (BuildProfileScanner.NeedsUpdate())
            {
                BuildProfileScanner.ScanAndCreateConfigurations();
            }
        }
    }
}
