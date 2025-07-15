#if UNITY_WEBGL //&& !UNITY_EDITOR
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
        private static extern void InitializeTelegramAuth(string botId);

        [DllImport("__Internal")]
        private static extern string GetCurrentUser();

        public event Action<string> OnSignIn;
        public event Action OnSignOut;
        public event Action<string> OnTokenReceived;
        public event Action<string> OnError;
        public event Action<string> OnTelegramAuth;

        // Добавляем событие для успешного завершения автоаутентификации Telegram
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

        public async Task Initialize(string config, string botId = "8114226239")
        {
            Debug.Log("Initialize Firebase WebGL");

            InitializeAuth(
                config,
                gameObject.name,
                nameof(HandleSignIn),
                nameof(HandleSignOut),
                nameof(HandleToken),
                nameof(HandleTelegramAuth),
                nameof(HandleError)
            );

            // Важно: уменьшаем задержку, чтобы быстрее проверить автоаутентификацию
            await UniTask.Delay(500);
            
            // Явно вызываем проверку аутентификации Telegram перед инициализацией виджета
            // Это поможет обработать данные, которые уже могли быть в URL
            CheckForTelegramAuth();
            
            // Затем ждем еще немного перед инициализацией виджета
            await UniTask.Delay(500);

            InitializeTelegramAuth(botId);

            // Даем немного времени для обработки результатов проверки автоаутентификации
            await UniTask.Delay(1000);
            
            // Пытаемся еще раз проверить аутентификацию Telegram на случай,
            // если данные поступили с задержкой
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
        
        // Улучшенный обработчик Telegram аутентификации
        public void HandleTelegramAuth(string userJson)
        {
            Debug.Log($"HandleTelegramAuth called with data: {userJson?.Substring(0, Math.Min(userJson?.Length ?? 0, 100))}...");
            OnTelegramAuth?.Invoke(userJson);
            
            // Вызываем событие завершения автоаутентификации - это поможет согласовать процессы
            // между FirebaseWebGLAuthPlugin и WebGLAuthProvider
            OnTelegramAutoAuthComplete?.Invoke();
        }

        public string GetUser() => GetCurrentUser();
    }
}
#endif