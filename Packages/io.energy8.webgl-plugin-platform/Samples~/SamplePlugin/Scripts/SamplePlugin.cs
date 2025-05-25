using System.Threading.Tasks;
using UnityEngine;

namespace Energy8.WebGL.PluginPlatform.Samples
{    /// <summary>
    /// Sample plugin for demonstrating system functionality
    /// </summary>
    public class SamplePlugin : BasePlugin
    {
        [SerializeField] private SamplePluginSettings pluginSettings;
        
        public override IPluginSettings Settings => pluginSettings;
        
        public override void Initialize()
        {
            Debug.Log($"[{PluginName}] Initialized with priority {Priority}");
            
            if (pluginSettings == null)
            {
                pluginSettings = ScriptableObject.CreateInstance<SamplePluginSettings>();
            }
        }
        
        public override void Enable()
        {
            Debug.Log($"[{PluginName}] Enabled");
        }
        
        public override void Disable()
        {
            Debug.Log($"[{PluginName}] Disabled");
        }
        
        public override void Destroy()
        {
            Debug.Log($"[{PluginName}] Destroyed");
        }
        
        [JSCallable]
        public string GetMessage(string name)
        {
            return $"Hello, {name}! Plugin is working.";
        }
        
        [JSCallable]
        public async Task<string> GetAsyncMessage(string name)
        {
            await Task.Delay(1000); // Simulate async operation
            return $"Async Hello, {name}! Delay completed.";
        }
        
        [JSCallable]
        public bool SetDebugMode(bool enabled)
        {
            if (pluginSettings != null)
            {
                pluginSettings.DebugMode = enabled;
                return true;
            }
            return false;
        }
        
        [JSCallable("getPluginInfo")]
        public string GetPluginInfo()
        {
            return $"Plugin: {PluginName}, Version: {Version}, Priority: {Priority}, Enabled: {IsEnabled}";
        }
    }
}
