using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using Energy8.Identity.Auth.Core.Providers;
using Energy8.Identity.Auth.Runtime.Factory;
using Energy8.Identity.User.Core.Services;
using Energy8.Identity.UI.Runtime.Services;
using Energy8.Identity.UI.Runtime.Views.Base;
using Energy8.Identity.UI.Runtime.Views.Implementations;
using Energy8.Identity.UI.Runtime.Views.Management;
using Energy8.Identity.UI.Runtime.Views.Models;
using UnityEngine;
using UnityEngine.UI;
using Energy8.Identity.Http.Core;
using Energy8.Identity.Http.Runtime.Clients;
using System.Net.Http;
using Energy8.Identity.UI.Runtime.Views.Implementations.User;
using Energy8.Identity.Shared.Core.Error;
using Energy8.Identity.UI.Runtime.Extensions;
using Energy8.Identity.Shared.Core.Contracts.Dto.User;
using System.Collections;
using Energy8.Identity.Analytics.Core.Services;
using Energy8.Identity.Analytics.Runtime.Services;
using Energy8.Identity.Analytics.Runtime.Factory;
using Energy8.Identity.Configuration.Core;
using Energy8.Identity.Shared.Core.Exceptions;

#if UNITY_WEBGL && !UNITY_EDITOR
using Energy8.Identity.Auth.Core.Models;
#else
using Firebase.Auth;
#endif

namespace Energy8.Identity.UI.Runtime.Controllers
{
    /// <summary>
    /// Основной контроллер Identity UI системы. Живет на протяжении всего lifecycle приложения.
    /// Управляет логикой аутентификации и пользовательскими потоками, но не привязан к Canvas.
    /// Canvas управляется через IdentityCanvasController.
    /// </summary>
    public class IdentityUIController : MonoBehaviour
    {
        public static IdentityUIController Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] private bool isLite = false;
        [SerializeField] protected bool debugLogging = false;

        public bool IsOpen { get; private set; }
        protected IHttpClient httpClient;
        private IAuthProvider authProvider;
        protected IUserService userService;
        protected IIdentityService identityService;
        private IAnalyticsService analyticsService;
        private CancellationTokenSource lifetimeCts;

        // Canvas управление
        private IdentityCanvasController currentCanvasController;
        
        // Флаги для предотвращения дублирования
        private bool isIdentityFlowStarted = false;
        private bool isShowingAuthFlow = false;
        private bool isShowingUserFlow = false;

        public event Action OnSignedOut;

        /// <summary>
        /// Получает текущий Canvas контроллер
        /// </summary>
        public IdentityCanvasController CurrentCanvasController => currentCanvasController;

        /// <summary>
        /// Получает информацию о том, является ли режим Lite
        /// </summary>
        public bool IsLite => isLite;

        #region Canvas Management

        /// <summary>
        /// Устанавливает Canvas контроллер для управления UI
        /// </summary>
        public void SetCanvasController(IdentityCanvasController canvasController)
        {
            if (currentCanvasController != null)
            {
                currentCanvasController.OnOpenStateChanged -= OnCanvasOpenStateChanged;
            }

            currentCanvasController = canvasController;
            
            if (currentCanvasController != null)
            {
                currentCanvasController.OnOpenStateChanged += OnCanvasOpenStateChanged;
                
                if (debugLogging)
                    Debug.Log($"Canvas controller set: {currentCanvasController.name}");
            }
        }

        /// <summary>
        /// Переключает состояние открытия/закрытия UI
        /// </summary>
        public void ToggleOpenState()
        {
            SetOpenState(!IsOpen);
        }

        /// <summary>
        /// Устанавливает состояние открытия/закрытия UI
        /// </summary>
        public void SetOpenState(bool isOpen)
        {
            if (isOpen == IsOpen)
                return;

            IsOpen = isOpen;
            
            if (currentCanvasController != null)
            {
                currentCanvasController.SetOpenState(isOpen);
            }
            
            if (debugLogging)
                Debug.Log($"Identity UI state set to: {(isOpen ? "open" : "closed")}");
        }

        /// <summary>
        /// Получает ViewManager из текущего Canvas контроллера
        /// </summary>
        protected ViewManager GetViewManager()
        {
            return currentCanvasController?.GetViewManager();
        }

        private void OnCanvasOpenStateChanged(bool isOpen)
        {
            IsOpen = isOpen;
            
            if (debugLogging)
                Debug.Log($"Canvas state changed, UI state updated to: {(isOpen ? "open" : "closed")}");
        }

        #endregion

        #region Event Handlers
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        private void OnUserSignedIn(string userId)
        #else
        private void OnUserSignedIn(FirebaseUser user)
        #endif
        {
            // Проверяем, что CancellationTokenSource не был очищен и что не показываем уже UserFlow
            if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested && !isShowingUserFlow)
            {
                // Сбрасываем флаг AuthFlow чтобы разрешить показ UserFlow
                isShowingAuthFlow = false;
                ShowUserFlow(lifetimeCts.Token).Forget();
            }
        }
        
        private void OnUserSignedOut()
        {
            OnSignedOut?.Invoke();
            
            // Проверяем, что CancellationTokenSource не был очищен и что не показываем уже AuthFlow
            if (lifetimeCts != null && !lifetimeCts.IsCancellationRequested && !isShowingAuthFlow)
            {
                // Сбрасываем флаг UserFlow чтобы разрешить показ AuthFlow
                isShowingUserFlow = false;
                ShowAuthFlow(lifetimeCts.Token).Forget();
            }
        }
        
        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Устанавливаем Instance СРАЗУ, чтобы предотвратить создание дубликатов
            Instance = this;
            DontDestroyOnLoad(gameObject);

            lifetimeCts = new CancellationTokenSource();
            httpClient = new UnityHttpClient(IdentityConfiguration.SelectedIP);
            authProvider = AuthProviderFactory.CreateProvider(httpClient);

            userService = new Energy8.Identity.User.Runtime.Services.UserService(httpClient, authProvider);
            var analyticsProvider = AnalyticsProviderFactory.CreateProvider();
            analyticsService = new AnalyticsService(analyticsProvider);
            identityService = new IdentityService(authProvider, userService, httpClient, analyticsService);

            // Подписываемся на события ПОСЛЕ установки Instance
            identityService.OnSignedIn += OnUserSignedIn;
            identityService.OnSignedOut += OnUserSignedOut;

            if (debugLogging)
                Debug.Log("IdentityUIController initialized as singleton");
        }

        void Start()
        {
            StartIdentityFlow().Forget();
        }

        private void OnDestroy()
        {
            // Отписываемся от событий ПЕРЕД очисткой ресурсов
            if (identityService != null)
            {
                identityService.OnSignedIn -= OnUserSignedIn;
                identityService.OnSignedOut -= OnUserSignedOut;
            }

            // Отписываемся от Canvas контроллера
            if (currentCanvasController != null)
            {
                currentCanvasController.OnOpenStateChanged -= OnCanvasOpenStateChanged;
            }

            // Очищаем Instance если это текущий экземпляр
            if (Instance == this)
            {
                Instance = null;
            }

            // Очищаем токен отмены
            lifetimeCts?.Cancel();
            lifetimeCts?.Dispose();
            lifetimeCts = null;

            WithLoadingExtensions.CleanupLoading();
            
            if (debugLogging)
                Debug.Log("IdentityUIController destroyed");
        }

        #endregion

        #region Identity Flow

        private async UniTask StartIdentityFlow()
        {
            if (isIdentityFlowStarted)
            {
                Debug.LogWarning("StartIdentityFlow already called, skipping duplicate call");
                return;
            }
            
            isIdentityFlowStarted = true;
            
            if (debugLogging)
                Debug.Log("Initializing identity system");
                
            await identityService.Initialize(lifetimeCts.Token)
                .WithLoading(lifetimeCts.Token);
                
            // Проверяем, авторизован ли пользователь
            if (identityService.IsSignedIn)
            {
                if (debugLogging)
                    Debug.Log("User is already signed in, showing user flow");
                ShowUserFlow(lifetimeCts.Token).Forget();
            }
            else
            {
                if (debugLogging)
                    Debug.Log("User is not signed in, showing auth flow");
                ShowAuthFlow(lifetimeCts.Token).Forget();
            }
        }

        private async UniTask ShowAuthFlow(CancellationToken ct)
        {
            if (isShowingAuthFlow)
            {
                Debug.LogWarning("ShowAuthFlow already running, skipping duplicate call");
                return;
            }
            
            isShowingAuthFlow = true;
            isShowingUserFlow = false; // Сброс флага UserFlow
            
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var viewManager = GetViewManager();
                        if (viewManager == null)
                        {
                            if (debugLogging)
                                Debug.LogWarning("No ViewManager available, waiting...");
                            await UniTask.Delay(1000, cancellationToken: ct);
                            continue;
                        }

                        // Открываем окно только когда нужно показать форму авторизации
                        SetOpenState(true);
                        
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

                        // Закрываем окно после успешной авторизации
                        SetOpenState(false);
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
            finally
            {
                isShowingAuthFlow = false;
            }
        }

        protected virtual async UniTask ShowUserFlow(CancellationToken ct)
        {
            if (isShowingUserFlow)
            {
                Debug.LogWarning("ShowUserFlow already running, skipping duplicate call");
                return;
            }
            
            isShowingUserFlow = true;
            isShowingAuthFlow = false; // Сброс флага AuthFlow
            
            try
            {
                // Не закрываем окно автоматически - пользователь может хотеть видеть профиль
                
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var viewManager = GetViewManager();
                        if (viewManager == null)
                        {
                            if (debugLogging)
                                Debug.LogWarning("No ViewManager available, waiting...");
                            await UniTask.Delay(1000, cancellationToken: ct);
                            continue;
                        }

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
            finally
            {
                isShowingUserFlow = false;
            }
        }

        protected async UniTask ShowSettings(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var viewManager = GetViewManager();
                    if (viewManager == null)
                    {
                        if (debugLogging)
                            Debug.LogWarning("No ViewManager available for settings");
                        return;
                    }

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
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

            var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                new CodeViewParams(), ct);

            return result.Code;
        }

        private async UniTask ShowChangeName(CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

            var result = await viewManager.Show<ChangeNameView, ChangeNameViewParams, ChangeNameViewResult>(
                new ChangeNameViewParams(), ct);

            await userService.UpdateNameAsync(result.Name, ct);
        }

        private async UniTask ShowChangeEmail(CancellationToken ct)
        {
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

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
            var viewManager = GetViewManager();
            if (viewManager == null)
                throw new InvalidOperationException("No ViewManager available");

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
            var viewManager = GetViewManager();
            if (viewManager == null)
            {
                Debug.LogError($"No ViewManager available to show error: {e8Exception.Message}");
                return ErrorHandlingMethod.Close;
            }

            var result = await viewManager.Show<ErrorView, ErrorViewParams, ErrorViewResult>(
                new ErrorViewParams(
                    e8Exception.Header,
                    e8Exception.Message,
                    e8Exception.CanRetry,
                    e8Exception.CanProceed,
                    e8Exception.MustSignOut), ct);

            return result.Method;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [ContextMenu("SignOut")]
        private void SignOut()
        {
            authProvider?.SignOut();
        }

        [ContextMenu("Debug State")]
        private void DebugState()
        {
            Debug.Log($"IdentityUIController State:\n" +
                      $"IsOpen: {IsOpen}\n" +
                      $"IsLite: {isLite}\n" +
                      $"Canvas Controller: {(currentCanvasController != null ? currentCanvasController.name : "null")}\n" +
                      $"Identity Flow Started: {isIdentityFlowStarted}\n" +
                      $"Showing Auth Flow: {isShowingAuthFlow}\n" +
                      $"Showing User Flow: {isShowingUserFlow}\n" +
                      $"Is Signed In: {identityService?.IsSignedIn ?? false}");
        }
#endif

        #endregion
    }
}
