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
using Energy8.EnvironmentConfig.Base;
using Energy8.Identity.Shared.Core.Exceptions;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;

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

        public bool IsSignedIn { get; internal set; } = false;
        public FirebaseUser CurrentUser => auth?.CurrentUser;

        // В нативной версии мы не поддерживаем автоаутентификацию Telegram
        public bool HasTelegramAutoAuthData => false;

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public async UniTask Initialize(CancellationToken ct)
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            var firebaseConfig = config?.FirebaseConfig;
            if (firebaseConfig == null)
            {
                throw new InvalidOperationException("FirebaseConfig is not set in IdentityConfig");
            }

            await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask().ContinueWith(dependencyStatus =>
            {
                if (dependencyStatus == DependencyStatus.Available)
                {
                    string config = firebaseConfig.text;
                    AppOptions appOptions = AppOptions.LoadFromJsonConfig(config);
                    FirebaseApp app = FirebaseApp.Create(appOptions, "Auth");
                    auth = FirebaseAuth.GetAuth(app);
                }
            });

            auth.StateChanged += AuthStateChanged;
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
                var result = await auth.SignInWithCustomTokenAsync(token);
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
            if (auth.CurrentUser != null && auth.CurrentUser.IsValid())
            {
                IsSignedIn = true;
                OnSignedIn?.Invoke(auth.CurrentUser);
            }
            else
            {
                IsSignedIn = false;
                OnSignedOut?.Invoke();
            }
        }
    }
}
#endif
