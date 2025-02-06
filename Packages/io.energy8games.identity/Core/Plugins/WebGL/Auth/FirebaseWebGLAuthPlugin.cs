#if UNITY_WEBGL //&& !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Energy8.Identity.Core.Plugins.WebGL.Auth
{
    public class FirebaseWebGLAuthPlugin : MonoBehaviour
    {
        private static FirebaseWebGLAuthPlugin instance;
        public static FirebaseWebGLAuthPlugin Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("FirebaseWebGLAuthPlugin");
                    instance = go.AddComponent<FirebaseWebGLAuthPlugin>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [DllImport("__Internal")]
        private static extern void InitializeAuth(string config, string objectName,
            string signInCallback, string signOutCallback, string tokenCallback, string telegramAuthCallback,
            string errorCallback);

        [DllImport("__Internal")]
        private static extern void SignInWithTokenAsync(string token);

        [DllImport("__Internal")]
        private static extern void SignInWithGoogle(bool linkProvider);

        [DllImport("__Internal")]
        private static extern void SignInWithApple(bool linkProvider);

        [DllImport("__Internal")]
        private static extern void GetIdToken(bool forceRefresh);

        [DllImport("__Internal")]
        private static extern void SignOut();

        [DllImport("__Internal")]
        private static extern void SignInWithTelegram();

        [DllImport("__Internal")]
        private static extern void InitializeTelegramAuth(string botId);

        [DllImport("__Internal")]
        private static extern string GetCurrentUser();

        public event Action<string> OnSignIn;
        public event Action OnSignOut;
        public event Action<string> OnTokenReceived;
        public event Action<string> OnError;
        public event Action<string> OnTelegramAuth;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public void Initialize(string config, string botId = "8114226239")
        {
            InitializeAuth(
                config,
                gameObject.name,
                nameof(HandleSignIn),
                nameof(HandleSignOut),
                nameof(HandleToken),
                nameof(HandleTelegramAuth),
                nameof(HandleError)
            );

            InitializeTelegramAuth(botId);
        }

        public void SignInWithToken(string token) => SignInWithTokenAsync(token);
        public void SignInWithGoogleProvider(bool linkProvider) => SignInWithGoogle(linkProvider);
        public void SignInWithAppleProvider(bool linkProvider) => SignInWithApple(linkProvider);
        public void SignInWithTelegramProvider() => SignInWithTelegram();
        
        public void GetToken(bool forceRefresh) => GetIdToken(forceRefresh);
        public void SignOutUser() => SignOut();

        // Callbacks from jslib
        public void HandleSignIn(string userJson) => OnSignIn?.Invoke(userJson);
        public void HandleSignOut() => OnSignOut?.Invoke();
        public void HandleToken(string token) => OnTokenReceived?.Invoke(token);
        public void HandleError(string error) => OnError?.Invoke(error);
        public void HandleTelegramAuth(string userJson) => OnTelegramAuth?.Invoke(userJson);

        public string GetUser() => GetCurrentUser();
    }
}
#endif