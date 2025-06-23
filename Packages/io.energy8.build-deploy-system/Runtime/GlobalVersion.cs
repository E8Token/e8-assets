using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Energy8.BuildDeploySystem
{
    /// <summary>
    /// Простое хранение глобальной версии проекта
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalVersion", menuName = "Energy8/Global Version")]
    public class GlobalVersion : ScriptableObject
    {
        [SerializeField] private string version = "1.0.1";

        public string Version
        {
            get => version;
            set 
            { 
                version = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        private static GlobalVersion instance;

        public static GlobalVersion Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    // Ищем существующий
                    string[] guids = AssetDatabase.FindAssets("t:GlobalVersion");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        instance = AssetDatabase.LoadAssetAtPath<GlobalVersion>(path);
                    }

                    // Создаем новый если не найден
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
#endif
                }
                return instance;
            }
        }

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
        }        public void IncrementBuild()
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
                // Если версия типа 1.2, добавляем .1
                Version = $"{version}.1";
            }
        }

        /// <summary>
        /// Генерирует Bundle ID для мобильных платформ на основе версии
        /// </summary>
        public string GenerateBundleId(string baseBundleId = "com.energy8.game")
        {
            var versionSuffix = version.Replace(".", "");
            return $"{baseBundleId}.v{versionSuffix}";
        }
    }
}
