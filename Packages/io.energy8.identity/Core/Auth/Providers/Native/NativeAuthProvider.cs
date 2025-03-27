#if !UNITY_WEBGL || UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Http;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading;
using Energy8.Identity.Core.Configuration;
using Energy8.Contracts.Dto.Auth;
using Energy8.Core.Exceptions;

namespace Energy8.Identity.Core.Auth.Providers
{
    public class NativeAuthProvider : IAuthProvider
    {
        private readonly ILogger<NativeAuthProvider> logger = new Logger<NativeAuthProvider>();
        private readonly IHttpClient httpClient;
        private FirebaseAuth auth;

        public NativeAuthProvider(IHttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public bool IsSignedIn => auth?.CurrentUser != null && auth.CurrentUser.IsValid();
        public FirebaseUser CurrentUser => auth?.CurrentUser;

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public async UniTask Initialize(CancellationToken ct)
        {
            logger.LogInfo("Initializing Firebase Auth");

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

            logger.LogDebug("Firebase Auth initialized");
        }

        public async UniTask<string> GetToken(bool forceRefresh, CancellationToken ct)
        {
            try
            {
                if (!IsSignedIn)
                    throw new InvalidOperationException("User is not signed in");

                logger.LogDebug($"Getting auth token. Force refresh: {forceRefresh}");
                return await auth.CurrentUser.TokenAsync(forceRefresh);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get token: {ex.Message}");
                throw;
            }
        }

        public async UniTask<AuthResult> SignInWithToken(string token, CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Signing in with token");
                var result = await auth.SignInWithCustomTokenAsync(token);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError($"Sign in with token failed: {ex.Message}");
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
            logger.LogInfo("User signed out");
        }

        private void AuthStateChanged(object sender, EventArgs args)
        {
            if (auth.CurrentUser != null && auth.CurrentUser.IsValid())
            {
                logger.LogInfo($"User signed in: {auth.CurrentUser.UserId}");
                OnSignedIn?.Invoke(auth.CurrentUser);
            }
            else
            {
                logger.LogInfo("User signed out");
                OnSignedOut?.Invoke();
            }
        }
    }
}
#endif