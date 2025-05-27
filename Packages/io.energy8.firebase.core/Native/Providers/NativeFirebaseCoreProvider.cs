#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Core.Models;
using Energy8.Firebase.Core.Providers;
using UnityEngine;
using Firebase;
using Firebase.Auth;

namespace Energy8.Firebase.Core.Providers
{
    public class NativeFirebaseCoreProvider : BaseFirebaseCoreProvider
    {
        public override async Task<FirebaseAppInfo> InitializeAppAsync(string config, string appName = null, CancellationToken ct = default)
        {
            try
            {
                appName ??= DefaultAppName;                // Check dependencies first
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    throw new Exception($"Firebase dependencies not available: {dependencyStatus}");
                }

                // Parse config and create app options
                var appOptions = AppOptions.LoadFromJsonConfig(config);
                  // Create Firebase app
                FirebaseApp firebaseApp;
                if (appName == DefaultAppName)
                {
                    firebaseApp = FirebaseApp.Create(appOptions);
                }
                else
                {
                    firebaseApp = FirebaseApp.Create(appOptions, appName);
                }

                // Create app info
                var appInfo = new FirebaseAppInfo
                {
                    Name = firebaseApp.Name,
                    ProjectId = appOptions.ProjectId,
                    ApiKey = appOptions.ApiKey,
                    AppId = appOptions.AppId,                    IsInitialized = true,
                    IsDataCollectionEnabled = true // Default value, Firebase doesn't expose this property
                };

                // Store in dictionary
                apps[appName] = appInfo;

                Debug.Log($"[FirebaseCore] Native app '{appName}' initialized successfully");
                InvokeAppInitialized(appInfo);

                return appInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Failed to initialize native app '{appName}': {ex.Message}");
                InvokeInitializationError(appName, ex);
                throw;
            }
        }

        public override async Task<bool> DeleteAppAsync(string appName = null, CancellationToken ct = default)
        {
            try
            {
                appName ??= DefaultAppName;

                if (!apps.ContainsKey(appName))
                {
                    return false;
                }                // Get Firebase app instance
                var firebaseApp = appName == DefaultAppName 
                    ? FirebaseApp.DefaultInstance 
                    : FirebaseApp.GetInstance(appName);

                if (firebaseApp != null)
                {
                    firebaseApp.Dispose();
                }

                apps.Remove(appName);
                InvokeAppDeleted(appName);

                Debug.Log($"[FirebaseCore] Native app '{appName}' deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseCore] Failed to delete native app '{appName}': {ex.Message}");
                return false;
            }
        }
    }
}
#endif
