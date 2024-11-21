using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using System.Threading;
using Firebase;



#if UNITY_WEBGL && !UNITY_EDITOR
#else
using Energy8.Firebase;
using Firebase.Auth;
using Google;
#endif
namespace Energy8.Firebase
{
    public static class FirebaseController
    {
        static readonly Logger logger = new(null, "FirebaseController", new Color(0.5f, 0.3f, 0.45f));
        static bool isInitialized = false;

        static public Func<Task<string>> GoogleSignIn;

        public static async UniTask InitializeAllAsync(CancellationToken cancellationToken)
        {

            logger.Log("Initialization started");
#if UNITY_WEBGL && !UNITY_EDITOR
#else
            await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask().ContinueWith(dependencyStatus =>
            {
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    string config = ApplicationConfig.AuthConfig;
                    AppOptions appOptions = AppOptions.LoadFromJsonConfig(config);
                    FirebaseApp authApp = FirebaseApp.Create(appOptions, "Auth");
                    AuthController.Initialize(authApp);
                    AnalyticsController.Initialize();
                    CrashlyticsController.Initialize();
                    NotificationsController.Initialize();
                    isInitialized = true;
                    logger.Log("Initialization ended");
                }
                else
                    logger.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            });
#endif
        }
        public static async UniTask AwaitInitalizationAsync() => await UniTask.WaitUntil(() => isInitialized);
    }
}