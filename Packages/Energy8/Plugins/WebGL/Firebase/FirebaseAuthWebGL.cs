#if UNITY_WEBGL //&& !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Models.SignIn;
using Energy8.Models.WebGL.Firebase;
using Newtonsoft.Json;
using UnityEngine;


namespace Energy8.Plugins.WebGL.Firebase
{
    public class FirebaseAuthWebGL : MonoBehaviour
    {
        public delegate void SignInCallback(FirebaseUser user);
        public delegate void TelegramAuthCallback(TelegramUserData user, string hash);
        public delegate void SignOutCallback();
        public delegate void ErrorCallback(string errorJson);
        public delegate void TokenCallback(string idToken);

        public event SignInCallback OnSignInEvent;
        public event SignOutCallback OnSignOutEvent;
        public event ErrorCallback OnErrorEvent;
        public event TelegramAuthCallback OnTelegramAuthEvent;
        public event TokenCallback OnTokenReceivedEvent;

        [DllImport("__Internal")]
        private static extern void InitializeAuth(string config, string objectName,
            string signInCallback, string signOutCallback, string telegramAuthCallback, string errorCallback);

        [DllImport("__Internal")]
        private static extern void SignInWithTokenAsync(string token);

        [DllImport("__Internal")]
        private static extern void SignInWithGoogle(bool addProvider);
        [DllImport("__Internal")]
        private static extern void SignInWithApple(bool addProvider);
        [DllImport("__Internal")]
        private static extern void SignInWithTelegram();

        [DllImport("__Internal")]
        private static extern void GetIdToken(bool forceRefresh);
        [DllImport("__Internal")]
        public static extern void SignOut();

        public static FirebaseAuthWebGL Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(string config)
        {
            InitializeAuth(config, gameObject.name, nameof(OnSignInCallback),
                nameof(OnSignOutCallback), nameof(OnTelegramAuthCallback), nameof(OnErrorCallback));
        }

        public async UniTask<FirebaseUser> SignInWithGoogleAsync(CancellationToken cancellationToken, bool addProvider)
        {
            var tcs = new UniTaskCompletionSource<FirebaseUser>();

            void HandleSignIn(FirebaseUser user) => tcs.TrySetResult(user);
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            OnSignInEvent += HandleSignIn;
            OnErrorEvent += HandleError;

            try
            {
                SignInWithGoogle(addProvider);
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnSignInEvent -= HandleSignIn;
                OnErrorEvent -= HandleError;
            }
        }

        public async UniTask<FirebaseUser> SignInWithAppleAsync(CancellationToken cancellationToken, bool addProvider)
        {
            var tcs = new UniTaskCompletionSource<FirebaseUser>();

            void HandleSignIn(FirebaseUser user) => tcs.TrySetResult(user);
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            OnSignInEvent += HandleSignIn;
            OnErrorEvent += HandleError;

            try
            {
                SignInWithApple(addProvider);
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnSignInEvent -= HandleSignIn;
                OnErrorEvent -= HandleError;
            }
        }

        public async UniTask<(TelegramUserData, string)> SignInWithTelegramAsync(CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource<(TelegramUserData, string)>();

            void HandleTelegramAuth(TelegramUserData user, string hash) => tcs.TrySetResult((user, hash));
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            OnTelegramAuthEvent += HandleTelegramAuth;
            OnErrorEvent += HandleError;

            try
            {
                SignInWithTelegram();
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnTelegramAuthEvent -= HandleTelegramAuth;
                OnErrorEvent -= HandleError;
            }
        }

        public async UniTask<FirebaseUser> SignInWithTokenAsync(CancellationToken cancellationToken, string token)
        {
            var tcs = new UniTaskCompletionSource<FirebaseUser>();

            void HandleSignIn(FirebaseUser user) => tcs.TrySetResult(user);
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            OnSignInEvent += HandleSignIn;
            OnErrorEvent += HandleError;

            try
            {
                SignInWithTokenAsync(token);
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnSignInEvent -= HandleSignIn;
                OnErrorEvent -= HandleError;
            }
        }

        public async UniTask<string> GetIdTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleToken(string idToken) => tcs.TrySetResult(idToken);
            void HandleError(string errorJson) => tcs.TrySetException(new Exception(errorJson));

            OnTokenReceivedEvent += HandleToken;
            OnErrorEvent += HandleError;

            try
            {
                GetIdToken(forceRefresh);
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnTokenReceivedEvent -= HandleToken;
                OnErrorEvent -= HandleError;
            }
        }

        private void OnSignInCallback(string userJson)
        {
            OnSignInEvent?.Invoke(JsonConvert.DeserializeObject<FirebaseUser>(userJson));
        }

        private void OnSignOutCallback()
        {
            OnSignOutEvent?.Invoke();
        }

        private void OnErrorCallback(string error)
        {
            OnErrorEvent?.Invoke(error);
        }

        private void OnTokenReceivedCallback(string idToken)
        {
            OnTokenReceivedEvent?.Invoke(idToken);
        }

        private void OnTelegramAuthCallback(string telegramUserJson)
        {
            OnTelegramAuthEvent?.Invoke(
                JsonConvert.DeserializeObject<TelegramUserData>(telegramUserJson),
                (string)JsonConvert.DeserializeObject<Dictionary<string, object>>(telegramUserJson)["hash"]);
        }
    }
}
#endif