using System;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;

namespace Energy8.Firebase.Auth.WebGL.Scripts
{
    [Serializable]
    public class FirebaseAuthPluginSettings : IPluginSettings
    {
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool persistenceEnabled = true;
        [SerializeField] private string languageCode = "en";
          public bool EnableDebugLogging => enableDebugLogging;
        public bool PersistenceEnabled => persistenceEnabled;
        public string LanguageCode => languageCode;

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
            enableDebugLogging = true;
            persistenceEnabled = true;
            languageCode = "en";
        }
    }
}
