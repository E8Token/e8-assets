using UnityEditor;
using UnityEngine;

namespace Energy8.BuildDeploySystem.Editor
{
    /// <summary>
    /// Отслеживает изменения в Build Profiles и автоматически обновляет конфигурации
    /// </summary>
    public class BuildProfileWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool needsUpdate = false;

            // Проверяем импортированные ассеты
            foreach (var asset in importedAssets)
            {
                if (IsBuildProfile(asset))
                {
                    needsUpdate = true;
                    break;
                }
            }

            // Проверяем удаленные ассеты
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

            // Проверяем перемещенные ассеты
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
                    Debug.Log("Build Profiles changed, updating configurations...");
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

    /// <summary>
    /// Инициализатор который запускается при загрuzke редактора
    /// </summary>
    [InitializeOnLoad]
    public static class BuildSystemInitializer
    {
        static BuildSystemInitializer()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            // Проверяем нужно ли обновить конфигурации при старте
            if (BuildProfileScanner.NeedsUpdate())
            {
                Debug.Log("Initializing Build Deploy System configurations...");
                BuildProfileScanner.ScanAndCreateConfigurations();
            }
        }
    }
}
