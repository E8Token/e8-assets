#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy8.Identity.Analytics.WebGL
{
    public class FirebaseWebGLAnalyticsPlugin : MonoBehaviour
    {
        private static FirebaseWebGLAnalyticsPlugin instance;
        public static FirebaseWebGLAnalyticsPlugin Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("FirebaseWebGLAnalyticsPlugin");
                    instance = go.AddComponent<FirebaseWebGLAnalyticsPlugin>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [DllImport("__Internal")]
        private static extern void InitializeAnalytics(string objectName, string errorCallback);

        [DllImport("__Internal")]
        private static extern void LogEvent(string eventName, string parameters);

        [DllImport("__Internal")]
        private static extern void SetUserId(string userId);

        [DllImport("__Internal")]
        private static extern void SetUserProperties(string properties);

        [DllImport("__Internal")]
        private static extern void ResetAnalyticsData();

        public event Action<string> OnError;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public async Task Initialize()
        {
            InitializeAnalytics(
                gameObject.name,
                nameof(HandleError)
            );

            await UniTask.Delay(500);
        }

        public void LogEventAsync(string eventName, Dictionary<string, object> parameters = null)
        {
            string paramsJson = parameters == null ? "{}" : JsonUtility.ToJson(new SerializableDictionary<string, object>(parameters));
            LogEvent(eventName, paramsJson);
        }

        public void SetUserIdAsync(string userId)
        {
            SetUserId(userId);
        }

        public void SetUserPropertiesAsync(Dictionary<string, object> properties)
        {
            string propsJson = properties == null ? "{}" : JsonUtility.ToJson(new SerializableDictionary<string, object>(properties));
            SetUserProperties(propsJson);
        }

        public void ResetAsync()
        {
            ResetAnalyticsData();
        }

        // Callback from jslib
        public void HandleError(string error)
        {
            Debug.LogError($"Firebase Analytics Error: {error}");
            OnError?.Invoke(error);
        }
    }

    // Helper class for serializing dictionaries
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [Serializable]
        public class KeyValuePair
        {
            public string key;
            public string value;
        }

        public List<KeyValuePair> items = new List<KeyValuePair>();

        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    items.Add(new KeyValuePair
                    {
                        key = pair.Key.ToString(),
                        value = pair.Value?.ToString() ?? ""
                    });
                }
            }
        }
    }
}
#endif
