#if UNITY_WEBGL //&& !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Models.WebGL.Firebase;
using Newtonsoft.Json;
using UnityEngine;


namespace Energy8.Firebase.WebGL
{
    public class FirebaseAuthWebGL : MonoBehaviour
    {
        public delegate void SignInCallback(FirebaseUser user);
        public delegate void SignOutCallback();
        public delegate void ErrorCallback(string errorJson);
        public delegate void TokenCallback(string idToken);

        public static event SignInCallback OnSignInEvent;
        public static event SignOutCallback OnSignOutEvent;
        public static event ErrorCallback OnErrorEvent;
        public static event TokenCallback OnTokenReceivedEvent;

        [DllImport("__Internal")]
        private static extern void Initialize(string config, string objectName, string signInCallback, string signOutCallback);

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

        public static void Initialize(string config)
        {
            Initialize(config, "FirebaseAuthWebGL", "OnSignInCallback", "OnSignOutCallback");
        }

        public static async UniTask<FirebaseUser> SignInByTokenAsync(CancellationToken cancellationToken, string token)
        {
            var tcs = new UniTaskCompletionSource<FirebaseUser>();

            void HandleSignIn(FirebaseUser user) => tcs.TrySetResult(user);
            void HandleError(string errorJson) => tcs.TrySetException(new Exception(errorJson));

            OnSignInEvent += HandleSignIn;
            OnErrorEvent += HandleError;

            try
            {
                SignInByTokenAsync(token, nameof(OnSignInCallback), nameof(OnErrorCallback));
                return await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                OnSignInEvent -= HandleSignIn;
                OnErrorEvent -= HandleError;
            }
        }

        public static async UniTask<string> GetIdTokenAsync(CancellationToken cancellationToken, bool forceRefresh)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleToken(string idToken) => tcs.TrySetResult(idToken);
            void HandleError(string errorJson) => tcs.TrySetException(new Exception(errorJson));

            OnTokenReceivedEvent += HandleToken;
            OnErrorEvent += HandleError;

            try
            {
                GetIdToken(forceRefresh, nameof(OnTokenReceivedCallback), nameof(OnErrorCallback));
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
            Debug.Log("User signed in: " + userJson);
            OnSignInEvent?.Invoke(JsonConvert.DeserializeObject<FirebaseUser>(userJson));
        }

        private void OnSignOutCallback()
        {
            Debug.Log("User signed out");
            OnSignOutEvent?.Invoke();
        }

        private void OnErrorCallback(string errorJson)
        {
            Debug.LogError("Sign in error: " + errorJson);
            OnErrorEvent?.Invoke(errorJson);
        }

        private void OnTokenReceivedCallback(string idToken)
        {
            Debug.Log("Received ID Token: " + idToken);
            OnTokenReceivedEvent?.Invoke(idToken);
        }
    }
}
#endif