#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy8.Identity.Auth.WebGL.Plugins
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
        private static extern void CheckForTelegramAuth();

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

        // We're not using CheckForTelegramAuth from C# directly - it's called automatically in JavaScript
        // No need for a DllImport for this function

        [DllImport("__Internal")]
        private static extern void InitializeTelegramAuth(long botId);

        [DllImport("__Internal")]
        private static extern string GetCurrentUser();

        public event Action<string> OnSignIn;
        public event Action OnSignOut;
        public event Action<string> OnTokenReceived;
        public event Action<string> OnError;
        public event Action<string> OnTelegramAuth;

        public event Action OnTelegramAutoAuthComplete;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public async Task Initialize(string config, long botId)
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

            CheckForTelegramAuth();
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
        
        // Обработчик Telegram аутентификации
        public void HandleTelegramAuth(string userJson)
        {
            OnTelegramAuth?.Invoke(userJson);
        }

        // Обработчик завершения проверки Telegram автоаутентификации
        public void HandleTelegramAutoAuthComplete()
        {
            OnTelegramAutoAuthComplete?.Invoke();
        }

        public string GetUser() => GetCurrentUser();
    }
}
#endif
