using System;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Auth.Providers;
using Energy8.Identity.Core.Http;
using Energy8.Identity.Core.User.Services;
using System.Threading;
using UnityEngine;
using Energy8.Contracts.Dto.Auth;


#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif


namespace Energy8.Identity.Runtime.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly ILogger<IdentityService> logger = new Logger<IdentityService>();
        private readonly IAuthProvider authProvider;
        private readonly IUserService userService;
        private readonly IHttpClient httpClient;
        private string pendingEmailToken;

        public bool IsInitialized { get; private set; }
        public bool IsSignedIn => authProvider.IsSignedIn;
        public FirebaseUser CurrentUser => authProvider.CurrentUser;

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public IdentityService(
            IAuthProvider authProvider,
            IUserService userService,
            IHttpClient httpClient)
        {
            this.authProvider = authProvider;
            this.userService = userService;
            this.httpClient = httpClient;

            authProvider.OnSignedIn += HandleSignedIn;
            authProvider.OnSignedOut += HandleSignedOut;
        }

        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                logger.LogInfo("Initializing Identity Service");
                await authProvider.Initialize(ct);

                IsInitialized = true;
                logger.LogInfo("Identity Service initialized");
            }
            catch (Exception ex)
            {
                logger.LogError($"Initialization failed: {ex.Message}");
                throw;
            }
        }

        public UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            logger.LogInfo($"Starting Google sign in. Link: {linkProvider}");
            return authProvider.SignInWithGoogle(linkProvider, ct);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async UniTask SignOut(CancellationToken ct)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                logger.LogInfo("Signing out");
                authProvider.SignOut();
            }
            catch (Exception ex)
            {
                logger.LogError($"Sign out failed: {ex.Message}");
                throw;
            }
        }

        private async void HandleSignedIn(FirebaseUser user)
        {
            try
            {
                var token = await authProvider.GetToken(false, CancellationToken.None);
                httpClient.SetAuthToken(token);

                OnSignedIn?.Invoke(authProvider.CurrentUser);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to handle sign in: {ex.Message}");
            }
        }

        private void HandleSignedOut()
        {
            httpClient.SetAuthToken(null);
            //User = null;
            OnSignedOut?.Invoke();
        }

        public async UniTask StartEmailFlow(string email, CancellationToken ct)
        {
            logger.LogInfo($"Starting email flow for: {email}");

            var response = await httpClient.PostAsync<EmailVerificationTokenDto>(
                "auth/email/sign-in",
                new EmailSignInDto()
                {
                    Email = email
                }, ct);

            pendingEmailToken = response.Token;
        }

        public async UniTask<AuthResult> ConfirmEmailCode(string code, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(pendingEmailToken))
                throw new InvalidOperationException("No pending email confirmation");

            logger.LogInfo("Confirming email code");

            var response = await httpClient.PostAsync<AccessTokenDto>(
                "auth/email/confirm",
                new EmailConfirmDto()
                {
                    Token = pendingEmailToken,
                    Code = code
                }, ct);

            pendingEmailToken = null;
            return await authProvider.SignInWithToken(response.Token, ct);
        }

        public UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct)
        {
            logger.LogInfo($"Starting Apple sign in. Link: {linkProvider}");
            return authProvider.SignInWithApple(linkProvider, ct);
        }

        public async UniTask<AuthResult> SignInWithTelegramAsync(bool linkProvider, CancellationToken ct)
        {
            logger.LogInfo($"Starting Apple sign in. Link: {linkProvider}");
            var user = await authProvider.SignInWithTelegram(ct);

            TelegramSignInDto telegramUserDto = linkProvider ?
                new TelegramLinkDto(user, CurrentUser.UserId) :
                new TelegramSignInDto(user);

            Debug.Log("TelegramHashRequestModel: " + telegramUserDto);

            if (linkProvider)
            {
                await httpClient.PostAsync(
                    "auth/telegram/sign-in", telegramUserDto, ct);
                return null;
            }
            else
            {
                var response = await httpClient.PostAsync<EmailVerificationTokenDto>(
                    "auth/telegram/sign-in", telegramUserDto, ct);
                return await authProvider.SignInWithToken(response.Token, ct);
            }
        }
    }
}