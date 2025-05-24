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
using System.Collections;
using Energy8.Identity.Core.Analytics.Services;
using Energy8.Identity.Core.Analytics.Providers;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Core.Auth.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.Runtime.UI.Controllers
{
    [RequireComponent(typeof(Canvas))]
    public class IdentityUIController : MonoBehaviour
    {
        public static IdentityUIController Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] protected ViewManager viewManager;
        [SerializeField] private bool isLite = false;

        [Header("UI")]
        [SerializeField] private Button showButton;
        [SerializeField] private Canvas canvas;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve;

        public bool IsOpen { get; private set; }
        private readonly ILogger<IdentityUIController> logger = new Logger<IdentityUIController>();
        protected IHttpClient httpClient;
        private IAuthProvider authProvider;
        protected IUserService userService;
        protected IIdentityService identityService;
        private IAnalyticsService analyticsService;
        private CancellationTokenSource lifetimeCts;

        private RectTransform containerRectTransform;
        private Coroutine currentAnimationCoroutine;

        public event Action OnSignedOut;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            lifetimeCts = new CancellationTokenSource(); httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
#if UNITY_WEBGL && !UNITY_EDITOR
            authProvider = new WebGLAuthProvider();
            var analyticsProvider = new WebGLAnalyticsProvider();
#else
            authProvider = new NativeAuthProvider(httpClient);
            var analyticsProvider = new DefaultAnalyticsProvider();
#endif

            userService = new UserService(httpClient, authProvider);
            analyticsService = new AnalyticsService(analyticsProvider);
            identityService = new IdentityService(authProvider, userService, httpClient, analyticsService);

            identityService.OnSignedIn += (_) => ShowUserFlow(lifetimeCts.Token).Forget();
            identityService.OnSignedOut += () =>
            {
                OnSignedOut?.Invoke();
                ShowAuthFlow(lifetimeCts.Token).Forget();
            };

            Instance = this;
            DontDestroyOnLoad(gameObject);

            containerRectTransform = viewManager?.GetComponent<RectTransform>();

            // Apply default animation curve if not set
            if (animationCurve == null || animationCurve.keys.Length == 0)
            {
                animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 1),
                    new Keyframe(1, 1, 1, 0)
                );
            }

            InitializeUI();

            // Apply Lite Mode settings if enabled
            if (isLite)
            {
                ApplyLiteMode();
            }

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

            Func<CancellationToken, UniTask<string>> requestEmailChange = async (ct) =>
            {
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

        protected async UniTask<ErrorHandlingMethod> ShowErrorAsync(Energy8Exception e8Exception, CancellationToken ct)
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

            showButton.onClick.AddListener(() => SetOpenState(!IsOpen));
        }

        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;

            IsOpen = isOpen;

            if (containerRectTransform != null)
            {
                // Stop any running animation
                if (currentAnimationCoroutine != null)
                {
                    StopCoroutine(currentAnimationCoroutine);
                }

                // Start new animation
                currentAnimationCoroutine = StartCoroutine(AnimateRectTransform(isOpen));
            }

            logger.LogDebug($"Identity UI state changed to: {(isOpen ? "open" : "closed")}");
        }

        private IEnumerator AnimateRectTransform(bool opening)
        {
            float startTime = Time.time;
            float startX = containerRectTransform.anchoredPosition.x;

            // Calculate the target position based on the new formula: Screen.Width / Canvas.Scale.X
            float targetWidth = containerRectTransform.sizeDelta.x;

            float endX = opening ? 0 : targetWidth;

            logger.LogDebug($"Animating panel: opening={opening}, targetWidth={targetWidth}, canvas scale={canvas.scaleFactor}");

            while (Time.time < startTime + animationDuration)
            {
                float elapsed = (Time.time - startTime) / animationDuration;
                float curveValue = animationCurve.Evaluate(elapsed);

                // Calculate the current position
                float currentX = Mathf.Lerp(startX, endX, curveValue);
                Vector2 newPosition = containerRectTransform.anchoredPosition;
                newPosition.x = currentX;

                // Apply the position
                containerRectTransform.anchoredPosition = newPosition;

                yield return null;
            }

            // Ensure final position is exact
            Vector2 finalPosition = containerRectTransform.anchoredPosition;
            finalPosition.x = endX;
            containerRectTransform.anchoredPosition = finalPosition;

            currentAnimationCoroutine = null;
        }

        private void Reset()
        {
            TryGetComponent(out canvas);

            if (transform.Find("Scroll View").TryGetComponent(out ScrollRect scroll))
            {
                scroll.transform.Find("OpenBut").TryGetComponent(out showButton);
            }
        }

        /// <summary>
        /// Applies the lite mode settings by adjusting the ViewManager's width based on Screen.Width / Canvas.Scale.X
        /// </summary>
        private void ApplyLiteMode()
        {
            if (viewManager != null)
            {
                // Calculate the desired width using the formula Screen.Width / Canvas.Scale.X
                float desiredWidth = Screen.width / canvas.scaleFactor;

                // Get the scroll rect component from the view manager
                var scrollRect = viewManager.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    // Adjust the width of the scroll rect using the calculated width
                    RectTransform rectTransform = scrollRect.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        // Store for animation use
                        containerRectTransform = rectTransform;

                        // Set sizeDelta to the calculated width for the X component
                        Vector2 sizeDelta = rectTransform.sizeDelta;
                        sizeDelta.x = desiredWidth;
                        rectTransform.sizeDelta = sizeDelta;

                        // Position the panel based on current open state without animation
                        Vector2 position = rectTransform.anchoredPosition;
                        position.x = IsOpen ? 0 : desiredWidth;
                        rectTransform.anchoredPosition = position;

                        logger.LogDebug($"Lite mode applied: Set width to {desiredWidth}, position.x={position.x} (IsOpen={IsOpen})");
                    }
                }
            }
        }

        // Add screen size change detection
        private Vector2 lastScreenSize;

        private void Update()
        {
            if (isLite)
            {
                Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

                // Check if screen size has changed
                if (currentScreenSize != lastScreenSize)
                {
                    lastScreenSize = currentScreenSize;
                    ApplyLiteMode(); // Recalculate and apply when screen size changes
                }
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