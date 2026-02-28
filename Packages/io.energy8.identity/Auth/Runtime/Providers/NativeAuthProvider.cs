#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Http.Core;
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

        public NativeAuthProvider(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool IsSignedIn { get; internal set; } = false;

        // В нативной версии мы не поддерживаем автоаутентификацию Telegram
        public bool HasTelegramAutoAuthData => false;

        public event Action<object> OnSignedIn;
        public event Action OnSignedOut;

        public async UniTask Initialize(CancellationToken ct)
        {
            var config = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");
            var firebaseConfig = config?.FirebaseConfig;
            if (firebaseConfig == null)
            {
                throw new InvalidOperationException("FirebaseConfig is not set in IdentityConfig");
            }
        }

        public async UniTask<string> GetToken(bool forceRefresh, CancellationToken ct)
        {
            try
            {
                if (!IsSignedIn)
                    throw new InvalidOperationException("User is not signed in");

                return "";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get token: {ex.Message}");
                throw;
            }
        }

        public async UniTask<object> SignInWithToken(string token, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async UniTask<object> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async UniTask<object> SignInWithApple(bool linkProvider, CancellationToken ct)
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
        }

        private void AuthStateChanged(object sender, EventArgs args)
        {
                IsSignedIn = false;
                OnSignedOut?.Invoke();
        }
    }
}
#endif
