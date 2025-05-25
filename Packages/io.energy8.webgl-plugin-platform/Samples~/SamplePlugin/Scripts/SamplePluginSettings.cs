using UnityEngine;

namespace Energy8.WebGL.PluginPlatform.Samples
{
    /// <summary>
    /// Sample settings for plugin
    /// </summary>
    [System.Serializable]
    public class SamplePluginSettings : ScriptableObject, IPluginSettings
    {
        [SerializeField] private string apiKey = "";
        [SerializeField] private bool debugMode = false;
        [SerializeField] private int maxRetries = 3;
        
        public string ApiKey { get => apiKey; set => apiKey = value; }
        public bool DebugMode { get => debugMode; set => debugMode = value; }
        public int MaxRetries { get => maxRetries; set => maxRetries = value; }
        
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        
        public void ResetToDefaults()
        {
            apiKey = "";
            debugMode = false;
            maxRetries = 3;
        }
    }
}
