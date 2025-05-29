using UnityEngine;
using UnityEditor;
using System.IO;

namespace Energy8.BuildDeploySystem.Editor
{
    public static class BuildDeploySystemSettings
    {
        private const string SETTINGS_PATH = "ProjectSettings/BuildDeploySystemSettings.json";
        private static BuildDeploySystemData _data;
        
        public static BuildDeploySystemData Data
        {
            get
            {
                if (_data == null)
                {
                    LoadSettings();
                }
                return _data;
            }
        }
        
        public static void LoadSettings()
        {
            if (File.Exists(SETTINGS_PATH))
            {
                string json = File.ReadAllText(SETTINGS_PATH);
                _data = JsonUtility.FromJson<BuildDeploySystemData>(json);
            }
            else
            {
                _data = new BuildDeploySystemData();
                SaveSettings();
            }
        }
        
        public static void SaveSettings()
        {
            if (_data != null)
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(SETTINGS_PATH, json);
            }
        }
        
        [System.Serializable]
        public class BuildDeploySystemData
        {
            public int selectedConfigurationIndex = 0;
            public string configurationsPath = "Assets/BuildConfigurations";
            public bool autoSaveOnBuild = true;
            public bool showAdvancedOptions = false;
        }
    }
}
