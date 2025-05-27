#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Core.Models;
using Energy8.Firebase.Core.Providers;
using Energy8.WebGL.PluginPlatform;
using UnityEngine;

namespace Energy8.Firebase.Core.WebGL
{
    public class WebFirebaseCoreProvider : BaseFirebaseCoreProvider
    {
        private const string PluginName = "FirebaseCorePlugin";        public override async Task<FirebaseAppInfo> InitializeAppAsync(string config, string appName = null, CancellationToken ct = default)
        {
            try
            {
                appName ??= DefaultAppName;

                // Call Firebase Core plugin via WebGL Plugin Platform
                var data = new { config = config, appName = appName };
                var result = PluginManager.Instance.CallPlugin(PluginName, "initializeApp", JsonUtility.ToJson(data));
                
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Failed to initialize Firebase app via WebGL plugin");
                }

                // Parse result
                var response = JsonUtility.FromJson<PluginResponse>(result);
                if (!response.success)
                {
                    throw new Exception(response.error ?? "Unknown error");
                }

                // Create app info from response
                var appInfo = new FirebaseAppInfo
                {
                    Name = appName,
                    IsInitialized = true
                };

                // Store in dictionary
                apps[appName] = appInfo;

                Debug.Log($"[FirebaseCore] Web app '{appName}' initialized successfully");
                InvokeAppInitialized(appInfo);

                return appInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Failed to initialize web app '{appName}': {ex.Message}");
                InvokeInitializationError(appName, ex);
                throw;
            }
        }        public override async Task<bool> DeleteAppAsync(string appName = null, CancellationToken ct = default)
        {
            try
            {
                appName ??= DefaultAppName;

                if (!apps.ContainsKey(appName))
                {
                    return false;
                }

                var data = new { appName = appName };
                var result = PluginManager.Instance.CallPlugin(PluginName, "deleteApp", JsonUtility.ToJson(data));
                
                if (string.IsNullOrEmpty(result))
                {
                    return false;
                }

                var response = JsonUtility.FromJson<PluginResponse>(result);
                if (response.success)
                {
                    apps.Remove(appName);
                    InvokeAppDeleted(appName);
                    Debug.Log($"[FirebaseCore] Web app '{appName}' deleted successfully");
                }

                return response.success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Failed to delete web app '{appName}': {ex.Message}");
                return false;
            }
        }        public override FirebaseAppInfo GetApp(string appName = null)
        {
            appName ??= DefaultAppName;
            
            // Try to get from local cache first
            if (apps.TryGetValue(appName, out var cachedApp))
            {
                return cachedApp;
            }

            // Query via WebGL Plugin Platform
            try
            {
                var data = new { appName = appName };
                var result = PluginManager.Instance.CallPlugin(PluginName, "getApp", JsonUtility.ToJson(data));
                
                if (!string.IsNullOrEmpty(result))
                {
                    var response = JsonUtility.FromJson<PluginResponse>(result);
                    if (response.success)
                    {
                        var appInfo = new FirebaseAppInfo
                        {
                            Name = appName,
                            IsInitialized = true
                        };
                        apps[appName] = appInfo;
                        return appInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Failed to get web app '{appName}': {ex.Message}");
            }

            return null;
        }

        [Serializable]
        private class PluginResponse
        {
            public bool success;
            public string error;
            public string appName;
        }
    }
}
#endif
