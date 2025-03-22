using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Energy8.Identity.Core.Logging;
using Energy8.Identity.Core.Auth;
using Energy8.Identity.Core.Auth.Providers;
using Energy8.Identity.Core.User.Services;
using Energy8.Identity.Runtime.Services;
using Energy8.Identity.Views;
using Energy8.Identity.Views.Implementations;
using Energy8.Identity.Views.Management;
using Energy8.Identity.Views.Models;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.Core.Http;
using System.Net.Http;
using Energy8.Identity.Views.Implementations.User;
using Energy8.Identity.Core.Error;
using Energy8.Identity.Core.Configuration;
using Energy8.Identity.Extensions;
using Energy8.Core.Exceptions;
using Energy8.Contracts.Dto.User;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.Runtime.UI.Controllers
{
    public class IdentityUIController : MonoBehaviour
    {
        public static IdentityUIController Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] protected ViewManager viewManager;

        [Header("UI")]
        [SerializeField] private Button showButton;
        [SerializeField] private new Animation animation;

        [Header("Animation")]
        [SerializeField] private string openClipName = "Open";
        [SerializeField] private string closeClipName = "Close";

        public bool IsOpen { get; private set; }

        private readonly ILogger<IdentityUIController> logger = new Logger<IdentityUIController>();
        protected IHttpClient httpClient;
        private IAuthProvider authProvider;
        protected IUserService userService;
        protected IIdentityService identityService;
        private CancellationTokenSource lifetimeCts;

        public event Action OnSignedOut;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            lifetimeCts = new CancellationTokenSource();

            httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
#if UNITY_WEBGL && !UNITY_EDITOR
            authProvider = new WebGLAuthProvider();
#else
            authProvider = new NativeAuthProvider(httpClient);
#endif

            userService = new UserService(httpClient, authProvider);
            identityService = new IdentityService(authProvider, userService, httpClient);

            identityService.OnSignedIn += (_) => ShowUserFlow(lifetimeCts.Token).Forget();
            identityService.OnSignedOut += () =>
            {
                OnSignedOut?.Invoke();
                ShowAuthFlow(lifetimeCts.Token).Forget();
            };

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUI();

            viewManager.InitializeLoading();
        }

        void Start()
        {
            StartIdentityFlow().Forget();
        }

        private async UniTask StartIdentityFlow()
        {
            logger.LogInfo("Initializing identity system");
            await identityService.Initialize(lifetimeCts.Token)
                .WithLoading(lifetimeCts.Token);
        }

        private async UniTask ShowAuthFlow(CancellationToken ct)
        {
            SetOpenState(true);
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = await viewManager
                        .Show<SignInView, SignInViewParams, SignInViewResult>(
                            new SignInViewParams(), ct);

                    AuthResult authResult = null;

                    switch (result.Method)
                    {
                        case SignInMethod.Email:
                            Func<CancellationToken, UniTask> startEmailFlow = (ct) => identityService
                                    .StartEmailFlow(result.Email, ct)
                                    .WithLoading(ct);
                            await startEmailFlow.WithErrorHandler(ShowErrorAsync, ct);

                            Func<CancellationToken, UniTask<AuthResult>> confirmEmailCode = async (ct) =>
                            {
                                string code = null;
                                string email = result.Email; // Store the email from sign-in result
                                
                                while (code == null)
                                {
                                    var codeResult = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                                        new CodeViewParams(), ct);

                                    if (codeResult.Code == "RESEND")
                                    {
                                        // Resend the authentication code
                                        await identityService.StartEmailFlow(email, ct).WithLoading(ct);
                                        continue;
                                    }
                                    
                                    code = codeResult.Code;
                                }
                                
                                return await identityService
                                    .ConfirmEmailCode(code, ct)
                                    .WithLoading(ct);
                            };
                            authResult = await confirmEmailCode.WithErrorHandler(ShowErrorAsync, ct);

                            break;

                        case SignInMethod.Google:
                            Func<CancellationToken, UniTask<AuthResult>> signInWithGoogle = (ct) => identityService
                                .SignInWithGoogle(false, ct)
                                .WithLoading(ct);
                            authResult = await signInWithGoogle.WithErrorHandler(ShowErrorAsync, ct);
                            break;

                        case SignInMethod.Apple:
                            Func<CancellationToken, UniTask<AuthResult>> signInWithApple = (ct) => identityService
                                .SignInWithApple(false, ct)
                                .WithLoading(ct);
                            authResult = await signInWithApple.WithErrorHandler(ShowErrorAsync, ct);
                            break;

                        case SignInMethod.Telegram:
                            Func<CancellationToken, UniTask<AuthResult>> signInWithTelegram = (ct) => identityService
                                .SignInWithTelegramAsync(false, ct)
                                .WithLoading(ct);
                            authResult = await signInWithTelegram.WithErrorHandler(ShowErrorAsync, ct);
                            break;
                    }

                    return;
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (SignOutRequiredException)
                {
                    await identityService.SignOut(ct);
                    continue;
                }
            }
        }

        protected virtual async UniTask ShowUserFlow(CancellationToken ct)
        {
            SetOpenState(false);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                        .GetUserAsync(ct)
                        .WithLoading(ct);

                    var user = await getUser.WithErrorHandler(ShowErrorAsync, ct);

                    var result = await viewManager
                        .Show<UserView, UserViewParams, UserViewResult>(
                            new UserViewParams(user.Name), ct);

                    switch (result.Action)
                    {
                        case UserAction.OpenSettings:
                            await ShowSettings(ct);
                            break;

                        case UserAction.SignOut:
                            await identityService.SignOut(ct);
                            return;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (SignOutRequiredException)
                {
                    await identityService.SignOut(ct);
                    continue;
                }
            }
        }

        protected async UniTask ShowSettings(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                        .GetUserAsync(ct)
                        .WithLoading(ct);

                    var user = await getUser.WithErrorHandler(ShowErrorAsync, ct);

                    var result = await viewManager
                        .Show<SettingsView, SettingsViewParams, SettingsViewResult>(
                            new SettingsViewParams(
                                user.Name,
                                user.Email,
                                user.AuthProviders.Contains("google.com"),
                                user.AuthProviders.Contains("apple.com"),
                                user.AuthProviders.Contains("telegram.org")), ct);

                    switch (result.Action)
                    {
                        case SettingsAction.ChangeName:
                            await ShowChangeName(ct);
                            break;

                        case SettingsAction.ChangeEmail:
                            await ShowChangeEmail(ct);
                            break;

                        case SettingsAction.DeleteAccount:
                            await ShowDeleteAccount(ct);
                            await identityService.SignOut(ct);
                            return;

                        case SettingsAction.AddGoogleProvider:
                            Func<CancellationToken, UniTask<AuthResult>> addGoogle = (ct) => identityService
                                .SignInWithGoogle(true, ct)
                                .WithLoading(ct);
                            await addGoogle.WithErrorHandler(ShowErrorAsync, ct);
                            break;

                        case SettingsAction.AddAppleProvider:
                            Func<CancellationToken, UniTask<AuthResult>> addApple = (ct) => identityService
                                .SignInWithApple(true, ct)
                                .WithLoading(ct);
                            await addApple.WithErrorHandler(ShowErrorAsync, ct);
                            break;

                        case SettingsAction.AddTelegramProvider:
                            Func<CancellationToken, UniTask<AuthResult>> addTelegram = (ct) => identityService
                                .SignInWithTelegramAsync(true, ct)
                                .WithLoading(ct);
                            await addTelegram.WithErrorHandler(ShowErrorAsync, ct);
                            break;

                        case SettingsAction.Close:
                            return;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (SignOutRequiredException)
                {
                    await identityService.SignOut(ct);
                    continue;
                }
            }
        }

        private async UniTask<string> ShowEmailVerification(CancellationToken ct)
        {
            var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                new CodeViewParams(), ct);

            return result.Code;
        }

        private async UniTask ShowChangeName(CancellationToken ct)
        {
            var result = await viewManager.Show<ChangeNameView, ChangeNameViewParams, ChangeNameViewResult>(
                new ChangeNameViewParams(), ct);

            await userService.UpdateNameAsync(result.Name, ct);
        }

        private async UniTask ShowChangeEmail(CancellationToken ct)
        {
            var emailResult = await viewManager.Show<ChangeEmailView, ChangeEmailViewParams, ChangeEmailViewResult>(
                new ChangeEmailViewParams(), ct);

            string email = emailResult.Email;
            string token = null;
            
            Func<CancellationToken, UniTask<string>> requestEmailChange = async (ct) => {
                return await userService.RequestEmailChangeAsync(email, ct);
            };
            
            token = await requestEmailChange.WithErrorHandler(ShowErrorAsync, ct);

            Func<CancellationToken, UniTask> confirmEmailCode = async (ct) =>
            {
                string code = null;
                while (code == null)
                {
                    var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                        new CodeViewParams(), ct);

                    if (result.Code == "RESEND")
                    {
                        // Request a new email change code with the same email
                        token = await userService.RequestEmailChangeAsync(email, ct).WithLoading(ct);
                        continue;
                    }
                    
                    code = result.Code;
                }
                
                await userService
                    .ConfirmEmailChangeAsync(token, code, ct)
                    .WithLoading(ct);
            };

            await confirmEmailCode.WithErrorHandler(ShowErrorAsync, ct);
        }

        private async UniTask ShowDeleteAccount(CancellationToken ct)
        {
            await viewManager.Show<DeleteAccountView, DeleteAccountViewParams, DeleteAccountViewResult>(
                new DeleteAccountViewParams(), ct);

            string token = null;
            string code = null;
            
            while (code == null)
            {
                token = await userService.RequestDeleteAccountAsync(ct);
                
                var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                    new CodeViewParams(), ct);

                if (result.Code == "RESEND")
                {
                    // Request a new deletion verification code
                    continue;
                }
                
                code = result.Code;
            }
            
            await userService.ConfirmDeleteAccountAsync(token, code, ct);
            await identityService.SignOut(ct);
        }

        private async UniTask<ErrorHandlingMethod> ShowErrorAsync(Energy8Exception e8Exception, CancellationToken ct)
        {
            var result = await viewManager.Show<ErrorView, ErrorViewParams, ErrorViewResult>(
                new ErrorViewParams(
                    e8Exception.Header,
                    e8Exception.Message,
                    e8Exception.CanRetry,
                    e8Exception.CanProceed,
                    e8Exception.MustSignOut), ct);

            return result.Method;
        }

        private void OnDestroy()
        {
            if (showButton != null)
                showButton.onClick.RemoveAllListeners();

            lifetimeCts?.Cancel();
            lifetimeCts?.Dispose();

            WithLoadingExtensions.CleanupLoading();
        }

        private void InitializeUI()
        {
            if (showButton == null)
                throw new ArgumentNullException(nameof(showButton));
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            showButton.onClick.AddListener(() => SetOpenState(!IsOpen));
        }

        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;

            IsOpen = isOpen;
            animation.Play(isOpen ? openClipName : closeClipName);
            logger.LogDebug($"Identity UI state changed to: {(isOpen ? "open" : "closed")}");
        }

        private void Reset()
        {
            if (transform.Find("Scroll View").TryGetComponent(out ScrollRect scroll))
            {
                scroll.TryGetComponent(out animation);
                scroll.transform.Find("OpenBut").TryGetComponent(out showButton);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("SignOut")]
        private void SignOut()
        {
            authProvider.SignOut();
        }
#endif
    }
}