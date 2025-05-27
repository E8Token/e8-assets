using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Energy8.Firebase.Core.Models;

namespace Energy8.Firebase.Core.Api
{
    public interface IFirebaseCoreApi
    {
        /// <summary>
        /// Initialize Firebase app with config
        /// </summary>
        Task<FirebaseAppInfo> InitializeAppAsync(string config, string appName = null, CancellationToken ct = default);

        /// <summary>
        /// Get Firebase app instance
        /// </summary>
        FirebaseAppInfo GetApp(string appName = null);

        /// <summary>
        /// Get all Firebase app instances
        /// </summary>
        IEnumerable<FirebaseAppInfo> GetAllApps();

        /// <summary>
        /// Delete Firebase app instance
        /// </summary>
        Task<bool> DeleteAppAsync(string appName = null, CancellationToken ct = default);

        /// <summary>
        /// Check if app is initialized
        /// </summary>
        bool IsAppInitialized(string appName = null);

        /// <summary>
        /// Events
        /// </summary>
        event Action<FirebaseAppInfo> OnAppInitialized;
        event Action<string> OnAppDeleted;
        event Action<string, Exception> OnInitializationError;
    }
}
