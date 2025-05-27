using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Core.Api;
using Energy8.Firebase.Core.Models;

namespace Energy8.Firebase.Core.Providers
{
    public abstract class BaseFirebaseCoreProvider : IFirebaseCoreApi
    {
        protected readonly Dictionary<string, FirebaseAppInfo> apps = new();
        protected const string DefaultAppName = "[DEFAULT]";

        public event Action<FirebaseAppInfo> OnAppInitialized;
        public event Action<string> OnAppDeleted;
        public event Action<string, Exception> OnInitializationError;

        public abstract Task<FirebaseAppInfo> InitializeAppAsync(string config, string appName = null, CancellationToken ct = default);

        public virtual FirebaseAppInfo GetApp(string appName = null)
        {
            appName ??= DefaultAppName;
            apps.TryGetValue(appName, out var app);
            return app;
        }

        public virtual IEnumerable<FirebaseAppInfo> GetAllApps()
        {
            return apps.Values;
        }

        public abstract Task<bool> DeleteAppAsync(string appName = null, CancellationToken ct = default);

        public virtual bool IsAppInitialized(string appName = null)
        {
            appName ??= DefaultAppName;
            return apps.ContainsKey(appName) && apps[appName].IsInitialized;
        }

        protected void InvokeAppInitialized(FirebaseAppInfo appInfo)
        {
            OnAppInitialized?.Invoke(appInfo);
        }

        protected void InvokeAppDeleted(string appName)
        {
            OnAppDeleted?.Invoke(appName);
        }

        protected void InvokeInitializationError(string appName, Exception error)
        {
            OnInitializationError?.Invoke(appName, error);
        }
    }
}
