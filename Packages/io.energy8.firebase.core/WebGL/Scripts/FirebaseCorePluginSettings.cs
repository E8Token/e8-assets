using System;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.Firebase.Core.WebGL
{
    [Serializable]
    public class FirebaseCorePluginSettings : IPluginSettings
    {
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private string defaultAppName = "[DEFAULT]";
        [SerializeField] private bool enableDebugLogging = false;

        public bool AutoInitialize 
        { 
            get => autoInitialize; 
            set => autoInitialize = value; 
        }

        public string DefaultAppName 
        { 
            get => defaultAppName; 
            set => defaultAppName = value; 
        }

        public bool EnableDebugLogging 
        { 
            get => enableDebugLogging; 
            set => enableDebugLogging = value; 
        }

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
            autoInitialize = true;
            defaultAppName = "[DEFAULT]";
            enableDebugLogging = false;
        }
    }
}
