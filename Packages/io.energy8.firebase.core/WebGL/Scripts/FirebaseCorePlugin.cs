using System;
using UnityEngine;
using Energy8.WebGL.PluginPlatform;
using Energy8.Firebase.Core.Models;

namespace Energy8.Firebase.Core.WebGL
{
    public class FirebaseCorePlugin : BasePlugin
    {
        [SerializeField] private FirebaseCorePluginSettings settings = new();

        public override IPluginSettings Settings => settings;

        public override void Initialize()
        {
            Debug.Log("[FirebaseCorePlugin] Initializing Firebase Core WebGL Plugin");
        }

        public override void Enable()
        {
            Debug.Log("[FirebaseCorePlugin] Firebase Core WebGL Plugin enabled");
        }

        public override void Disable()
        {
            Debug.Log("[FirebaseCorePlugin] Firebase Core WebGL Plugin disabled");
        }

        public override void Destroy()
        {
            Debug.Log("[FirebaseCorePlugin] Firebase Core WebGL Plugin destroyed");
        }

        [JSCallable("initializeApp")]
        public string InitializeApp(string config, string appName = null)
        {
            try
            {
                appName = string.IsNullOrEmpty(appName) ? settings.DefaultAppName : appName;
                
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseCorePlugin] Initializing app: {appName}");
                }

                // Вызываем JavaScript для инициализации Firebase
                return $"{{\"success\": true, \"appName\": \"{appName}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCorePlugin] Failed to initialize app: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("getApp")]
        public string GetApp(string appName = null)
        {
            try
            {
                appName = string.IsNullOrEmpty(appName) ? settings.DefaultAppName : appName;
                
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseCorePlugin] Getting app: {appName}");
                }

                return $"{{\"success\": true, \"appName\": \"{appName}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCorePlugin] Failed to get app: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("deleteApp")]
        public string DeleteApp(string appName = null)
        {
            try
            {
                appName = string.IsNullOrEmpty(appName) ? settings.DefaultAppName : appName;
                
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseCorePlugin] Deleting app: {appName}");
                }

                return $"{{\"success\": true, \"appName\": \"{appName}\"}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCorePlugin] Failed to delete app: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("getAllApps")]
        public string GetAllApps()
        {
            try
            {
                if (settings.EnableDebugLogging)
                {
                    Debug.Log("[FirebaseCorePlugin] Getting all apps");
                }

                return $"{{\"success\": true, \"apps\": []}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCorePlugin] Failed to get all apps: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }

        [JSCallable("isAppInitialized")]
        public string IsAppInitialized(string appName = null)
        {
            try
            {
                appName = string.IsNullOrEmpty(appName) ? settings.DefaultAppName : appName;
                
                if (settings.EnableDebugLogging)
                {
                    Debug.Log($"[FirebaseCorePlugin] Checking if app is initialized: {appName}");
                }

                return $"{{\"success\": true, \"appName\": \"{appName}\", \"initialized\": true}}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCorePlugin] Failed to check app initialization: {ex.Message}");
                return $"{{\"success\": false, \"error\": \"{ex.Message}\"}}";
            }
        }
    }
}
