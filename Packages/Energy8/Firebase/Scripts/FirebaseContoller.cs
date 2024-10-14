using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using System.Threading;



#if UNITY_IOS || UNITY_ANDROID
using Firebase;
using Energy8.Firebase;
using Firebase.Auth;
using Google;
#endif
namespace Energy8.Firebase
{
    public static class FirebaseContoller
    {
        static readonly Logger logger = new(null, "FirebaseController", new Color(0.5f, 0.3f, 0.45f));
        static bool isInitialized = false;

        static public Func<Task<string>> GoogleSignIn;

        public static async UniTask InitializeAllAsync(CancellationToken cancellationToken)
        {
            logger.Log("Initialization started");
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                AnalyticsController.Initialize();
                CrashlyticsController.Initialize();
                NotificationsController.Initialize();
                isInitialized = true;
                logger.Log("Initialization ended");
            }
            else
                logger.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        });
#else
            await UniTask.Delay(1000);
#endif
            AuthController.Initialize();
            // AnalyticsController.Initialize();
            // CrashlyticsController.Initialize();
            // NotificationsController.Initialize();

            logger.Log("Initialization ended");
            isInitialized = true;
        }
        public static async UniTask AwaitInitalizationAsync() => await UniTask.WaitUntil(() => isInitialized);

        public static async UniTask AuthByGoogleAsync()
        {
            string userIdToken = await GoogleSignIn.Invoke();

            try
            {
                // Credential credential = GoogleAuthProvider.GetCredential(userIdToken, null);
                // await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}