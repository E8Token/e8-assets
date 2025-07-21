#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Http.Core;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading;
using UnityEngine;
using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Shared.Core.Exceptions;

namespace Energy8.Identity.Auth.Runtime.Providers
{
    public class NativeAuthProvider : IAuthProvider
    {
        private readonly IHttpClient httpClient;
        private FirebaseAuth auth;

        public NativeAuthProvider(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool IsSignedIn => auth?.CurrentUser != null && auth.CurrentUser.IsValid();
        public FirebaseUser CurrentUser => auth?.CurrentUser;

        // В нативной версии мы не поддерживаем автоаутентификацию Telegram
        public bool HasTelegramAutoAuthData => false;

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public async UniTask Initialize(CancellationToken ct)
        {
            Debug.Log("Initializing Firebase Auth");

            await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask().ContinueWith(dependencyStatus =>
            {
                if (dependencyStatus == DependencyStatus.Available)
                {
                    string config = IdentityConfiguration.AuthConfig;
                    AppOptions appOptions = AppOptions.LoadFromJsonConfig(config);
                    FirebaseApp app = FirebaseApp.Create(appOptions, "Auth");
                    auth = FirebaseAuth.GetAuth(app);
                }
            });

            auth.StateChanged += AuthStateChanged;

            Debug.Log("Firebase Auth initialized");
        }

        public async UniTask<string> GetToken(bool forceRefresh, CancellationToken ct)
        {
            try
            {
                if (!IsSignedIn)
                    throw new InvalidOperationException("User is not signed in");

                return await auth.CurrentUser.TokenAsync(forceRefresh);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get token: {ex.Message}");
                throw;
            }
        }

        public async UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct)
        {
            try
            {
                Debug.Log("[NativeAuthProvider] Starting SignInWithCustomTokenAsync");
                var result = await auth.SignInWithCustomTokenAsync(token);
                Debug.Log($"[NativeAuthProvider] SignInWithCustomTokenAsync completed. User: {result.User?.UserId}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NativeAuthProvider] Sign in with token failed: {ex.Message}");
                throw new Energy8Exception("Sign in failed", ex.Message);
            }
        }

        public async UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct)
        {
            throw new NotImplementedException("Telegram sign in not implemented for native provider");
        }

        public void SignOut()
        {
            if (!IsSignedIn)
                return;

            auth.SignOut();
        }

        private void AuthStateChanged(object sender, EventArgs args)
        {
            Debug.Log($"[NativeAuthProvider] AuthStateChanged called. CurrentUser: {auth.CurrentUser?.UserId}, IsValid: {auth.CurrentUser?.IsValid()}");
            
            if (auth.CurrentUser != null && auth.CurrentUser.IsValid())
            {
                Debug.Log($"[NativeAuthProvider] User signed in, invoking OnSignedIn event for user: {auth.CurrentUser.UserId}");
                OnSignedIn?.Invoke(auth.CurrentUser);
            }
            else
            {
                Debug.Log("[NativeAuthProvider] User signed out, invoking OnSignedOut event");
                OnSignedOut?.Invoke();
            }
        }
    }
}
#endif