#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Plugins.WebGL.Auth;
using Energy8.Identity.Core.Auth.Models;
using Energy8.Identity.Core.Configuration;
using Newtonsoft.Json;
using Energy8.Contracts.Dto.Common;
using Energy8.Contracts.Dto.Auth;

namespace Energy8.Identity.Core.Auth.Providers
{
    public class WebGLAuthProvider : IAuthProvider
    {
        private readonly ILogger<WebGLAuthProvider> logger = new Logger<WebGLAuthProvider>();
        private readonly FirebaseWebGLAuthPlugin plugin;

        public bool IsSignedIn => CurrentUser != null;
        public FirebaseUser CurrentUser { get; private set; }

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public WebGLAuthProvider()
        {
            plugin = FirebaseWebGLAuthPlugin.Instance;
            plugin.OnSignIn += HandleSignIn;
            plugin.OnSignOut += HandleSignOut;
            plugin.OnError += HandleError;
        }

        public UniTask Initialize(CancellationToken ct)
        {
            plugin.Initialize(IdentityConfiguration.AuthConfig);
            return UniTask.CompletedTask;
        }

        public async UniTask<string> GetToken(bool forceRefresh, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<string>();

            void HandleToken(string token) => tcs.TrySetResult(token);
            void HandleError(string error) => tcs.TrySetException(new Exception(error));

            plugin.OnTokenReceived += HandleToken;
            plugin.OnError += HandleError;

            try
            {
                plugin.GetToken(forceRefresh);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            finally
            {
                plugin.OnTokenReceived -= HandleToken;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Token sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithToken(token);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Google sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithGoogleProvider(linkProvider);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<AuthResult>();

            void HandleSuccess(string userJson)
            {
                var user = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
                tcs.TrySetResult(new AuthResult(true, user));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Apple sign in failed: {error}"));

            plugin.OnSignIn += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithAppleProvider(linkProvider);
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public async UniTask<TelegramUserDto> SignInWithTelegram(CancellationToken ct)
        {
            var tcs = new UniTaskCompletionSource<TelegramUserDto>();

            void HandleSuccess(string telegramUserJson)
            {
                if (DtoBase.TryFromJson(telegramUserJson, out TelegramUserDto telegramUser))
                    tcs.TrySetResult(telegramUser);
                else
                    tcs.TrySetException(new Exception("User not parsed."));
            }

            void HandleError(string error) =>
                tcs.TrySetException(new Exception($"Telegram sign in failed: {error}"));

            plugin.OnTelegramAuth += HandleSuccess;
            plugin.OnError += HandleError;

            try
            {
                plugin.SignInWithTelegramProvider();
                return await tcs.Task.AttachExternalCancellation(ct);
            }
            catch
            {
                throw;
            }
            finally
            {
                plugin.OnSignIn -= HandleSuccess;
                plugin.OnError -= HandleError;
            }
        }

        public void SignOut()
        {
            plugin.SignOutUser();
        }

        private void HandleSignIn(string userJson)
        {
            CurrentUser = JsonConvert.DeserializeObject<FirebaseUser>(userJson);
            OnSignedIn?.Invoke(CurrentUser);
        }

        private void HandleSignOut()
        {
            CurrentUser = null;
            OnSignedOut?.Invoke();
        }

        private void HandleError(string error)
        {
            logger.LogError($"Firebase error: {error}");
        }
    }
}
#endif