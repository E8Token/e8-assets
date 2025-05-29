using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Energy8.BuildDeploySystem
{
    [CreateAssetMenu(fileName = "BuildConfigurationManager", menuName = "Energy8/Build Deploy System/Configuration Manager")]
    public class BuildConfigurationManager : ScriptableObject
    {
        [SerializeField]
        private List<BuildConfiguration> configurations = new List<BuildConfiguration>();
        
        [SerializeField]
        private int selectedConfigurationIndex = 0;
        
        public List<BuildConfiguration> Configurations => configurations;
        
        public BuildConfiguration SelectedConfiguration
        {
            get
            {
                if (selectedConfigurationIndex >= 0 && selectedConfigurationIndex < configurations.Count)
                    return configurations[selectedConfigurationIndex];
                return null;
            }
        }
        
        public int SelectedConfigurationIndex
        {
            get => selectedConfigurationIndex;
            set => selectedConfigurationIndex = Mathf.Clamp(value, 0, configurations.Count - 1);
        }
        
        public void AddConfiguration(BuildConfiguration config)
        {
            if (config != null && !configurations.Contains(config))
            {
                configurations.Add(config);
            }
        }
        
        public void RemoveConfiguration(BuildConfiguration config)
        {
            configurations.Remove(config);
            if (selectedConfigurationIndex >= configurations.Count)
                selectedConfigurationIndex = Mathf.Max(0, configurations.Count - 1);
        }
        
        public void RemoveConfigurationAt(int index)
        {
            if (index >= 0 && index < configurations.Count)
            {
                configurations.RemoveAt(index);
                if (selectedConfigurationIndex >= configurations.Count)
                    selectedConfigurationIndex = Mathf.Max(0, configurations.Count - 1);
            }
        }
        
        public BuildConfiguration CreateNewConfiguration(string name = "New Configuration")
        {
            var config = CreateInstance<BuildConfiguration>();
            config.configName = name;
            configurations.Add(config);
            return config;
        }
        
        public void SaveToJson(string path)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        
        public static BuildConfigurationManager LoadFromJson(string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<BuildConfigurationManager>(json);
            }
            return null;
        }
    }
}
