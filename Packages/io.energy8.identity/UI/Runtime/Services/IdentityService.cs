using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Http.Core;
using Energy8.Identity.User.Core.Services;
using Energy8.Identity.Analytics.Core.Services;
using System.Threading;
using UnityEngine;
using Energy8.Identity.Shared.Core.Contracts.Dto.Auth;
using Energy8.Identity.Shared.Core.Exceptions;
using AuthTelegramSignInDto = Energy8.Identity.Shared.Core.Contracts.Dto.Auth.TelegramSignInDto;
using AuthTelegramLinkDto = Energy8.Identity.Shared.Core.Contracts.Dto.Auth.TelegramSignInLinkDto;


#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IAuthProvider authProvider;
        private readonly IUserService userService;
        private readonly IHttpClient httpClient;
        private readonly IAnalyticsService analyticsService;
        private string pendingEmailToken;

        public bool IsInitialized { get; private set; }
        public bool IsSignedIn => authProvider.IsSignedIn;
        public FirebaseUser CurrentUser => authProvider.CurrentUser;

        public bool HasTelegramAutoAuthData => authProvider.HasTelegramAutoAuthData;

        public event Action<FirebaseUser> OnSignedIn;
        public event Action OnSignedOut;

        public IdentityService(
            IAuthProvider authProvider,
            IUserService userService,
            IHttpClient httpClient,
            IAnalyticsService analyticsService)
        {
            this.authProvider = authProvider;
            this.userService = userService;
            this.httpClient = httpClient;
            this.analyticsService = analyticsService;

            authProvider.OnSignedIn += HandleSignedIn;
            authProvider.OnSignedOut += HandleSignedOut;
        }
        
        /// <summary>
        /// Включает/отключает логирование токенов доступа для отладки
        /// </summary>
        public void EnableTokenLogging(bool enabled)
        {
            httpClient.EnableTokenLogging(enabled);
        }
        
        public async UniTask Initialize(CancellationToken ct)
        {
            try
            {
                await authProvider.Initialize(ct);

                // Initialize Analytics Service
                if (analyticsService != null)
                {
                    try
                    {
                        await analyticsService.Initialize(ct);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Analytics initialization failed, but continuing: {ex.Message}");
                    }
                }

                IsInitialized = true;
                if (HasTelegramAutoAuthData && !IsSignedIn)
                {
                    try
                    {
                        await SignInWithTelegramAsync(false, ct);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Auto sign-in with Telegram failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Initialization failed: {ex.Message}");
                throw;
            }
        }

        public UniTask<AuthResult> SignInWithGoogle(bool linkProvider, CancellationToken ct)
        {
            return authProvider.SignInWithGoogle(linkProvider, ct);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async UniTask SignOut(CancellationToken ct)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                // Log sign-out event to analytics
                if (analyticsService != null && analyticsService.IsInitialized)
                {
                    analyticsService.LogSignOut();
                }

                authProvider.SignOut();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Sign out failed: {ex.Message}");
                throw;
            }
        }
        private async void HandleSignedIn(FirebaseUser user)
        {
            try
            {
                var token = await authProvider.GetToken(false, CancellationToken.None);
                httpClient.SetAuthToken(token);

                // Log sign-in event to analytics
                if (analyticsService != null && analyticsService.IsInitialized)
                {
                    analyticsService.SetUserId(user.UserId);
                    analyticsService.LogSignIn("firebase");

                    var userProps = new Dictionary<string, object>
                    {
                        { "display_name", user.DisplayName ?? "" },
                        { "email", user.Email ?? "" },
                        { "provider_id", user.ProviderId ?? "" }
                    };
                    analyticsService.SetUserProperties(userProps);
                }

                OnSignedIn?.Invoke(authProvider.CurrentUser);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to handle sign in: {ex.Message}");
            }
        }

        private void HandleSignedOut()
        {
            httpClient.SetAuthToken(null);
            OnSignedOut?.Invoke();
        }

        public async UniTask StartEmailFlow(string email, CancellationToken ct)
        {
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

            var response = await httpClient.PostAsync<AccessTokenDto>(
                "auth/email/confirm",
                new EmailConfirmDto()
                {
                    Token = pendingEmailToken,
                    Code = code
                }, ct);

            pendingEmailToken = null;
            var result = await authProvider.SignInWithToken(response.Token, ct);
            return result;
        }

        public UniTask<AuthResult> SignInWithApple(bool linkProvider, CancellationToken ct)
        {
            return authProvider.SignInWithApple(linkProvider, ct);
        }

        public async UniTask<AuthResult> SignInWithTelegramAsync(bool linkProvider, CancellationToken ct)
        {
            try
            {
                var user = await authProvider.SignInWithTelegram(ct);

                if (user == null)
                {
                    Debug.LogError("Telegram sign-in failed: user data is null");
                    throw new Energy8Exception("Sign in failed", "Unable to retrieve Telegram user data");
                }

                AuthTelegramSignInDto telegramUserDto = linkProvider ?
                     new AuthTelegramLinkDto(user, CurrentUser.UserId) :
                     new AuthTelegramSignInDto(user);

                if (linkProvider)
                {
                    await httpClient.PostAsync(
                        "auth/telegram/sign-in", telegramUserDto, ct);
                    return null;
                }
                else
                {
                    try
                    {
                        var response = await httpClient.PostAsync<AccessTokenDto>(
                            "auth/telegram/sign-in", telegramUserDto, ct);

                        // Token received successfully
                        return await authProvider.SignInWithToken(response.Token, ct);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to sign in with Telegram: {ex.Message}");
                        throw new Energy8Exception("Sign in failed", "Telegram authentication error");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Telegram sign-in error: {ex.Message}");

                if (ex is Energy8Exception)
                    throw;

                throw new Energy8Exception("Sign in failed", ex.Message);
            }
        }
    }
}
