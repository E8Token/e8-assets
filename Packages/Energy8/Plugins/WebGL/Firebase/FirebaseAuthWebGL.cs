#if UNITY_WEBGL
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy8.Firebase.WebGL
{
    public class FirebaseAuthWebGL : MonoBehaviour
    {
        public delegate void SignInCallback(string userId);
        public delegate void SignOutCallback();
        public delegate void ErrorCallback(string errorJson);
        public delegate void TokenCallback(string idToken);

        public static event SignInCallback OnSignIn;
        public static event SignOutCallback OnSignOut;
        public static event ErrorCallback OnError;
        public static event TokenCallback OnTokenReceived;

        [DllImport("__Internal")]
        private static extern void Initialize(string objectName, string signInCallback, string signOutCallback);

        [DllImport("__Internal")]
        private static extern void SignInByTokenAsync(string token, string callback, string fallback);

        [DllImport("__Internal")]
        private static extern void GetIdToken(bool forceRefresh, string callback, string fallback);
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

        public static void Initialize()
        {
            Initialize(Instance.gameObject.name, nameof(OnSignInCallback), nameof(OnSignOutCallback));
        }

        public static async UniTask<string> SignInByTokenAsync(CancellationToken cancellationToken, string token)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleSignIn(string userJson) => tcs.TrySetResult(userJson);
            void HandleError(string errorJson) => tcs.TrySetException(new Exception(errorJson));

            OnSignIn += HandleSignIn;
            OnError += HandleError;

            try
            {
                SignInByTokenAsync(token, nameof(OnSignInCallback), nameof(OnErrorCallback));
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnSignIn -= HandleSignIn;
                OnError -= HandleError;
            }
        }

        public static async UniTask<string> GetIdTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleToken(string idToken) => tcs.TrySetResult(idToken);
            void HandleError(string errorJson) => tcs.TrySetException(new Exception(errorJson));

            OnTokenReceived += HandleToken;
            OnError += HandleError;

            try
            {
                GetIdToken(forceRefresh, nameof(OnTokenReceivedCallback), nameof(OnErrorCallback));
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnTokenReceived -= HandleToken;
                OnError -= HandleError;
            }
        }

        private void OnSignInCallback(string userJson)
        {
            Debug.Log("User signed in: " + userJson);
            OnSignIn?.Invoke(userJson);
        }

        private void OnSignOutCallback()
        {
            Debug.Log("User signed out");
            OnSignOut?.Invoke();
        }

        private void OnErrorCallback(string errorJson)
        {
            Debug.LogError("Sign in error: " + errorJson);
            OnError?.Invoke(errorJson);
        }

        private void OnTokenReceivedCallback(string idToken)
        {
            Debug.Log("Received ID Token: " + idToken);
            OnTokenReceived?.Invoke(idToken);
        }
    }
}
#endif