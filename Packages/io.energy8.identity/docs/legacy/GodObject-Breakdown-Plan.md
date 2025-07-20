# План разбиения God Object IdentityUIController

## 🔍 Детальный анализ функционала (891 строка кода)

### Текущая структура IdentityUIController:

```csharp
public class IdentityUIController : MonoBehaviour // ❌ 891 СТРОКА GOD OBJECT!
{
    // 📍 СТРОКИ 45-48: SINGLETON & CONFIGURATION
    public static IdentityUIController Instance { get; private set; }
    [SerializeField] private bool isLite = false;
    [SerializeField] protected bool debugLogging = false;
    public bool IsOpen { get; private set; }

    // 📍 СТРОКИ 49-58: ЗАВИСИМОСТИ (7 СЕРВИСОВ!)
    protected IHttpClient httpClient;
    private IAuthProvider authProvider;
    protected IUserService userService;
    protected IIdentityService identityService;
    private IAnalyticsService analyticsService;
    private CancellationTokenSource lifetimeCts;
    private IdentityCanvasController currentCanvasController;

    // 📍 СТРОКИ 55-57: STATE FLAGS (ПРОБЛЕМА!)
    private bool isIdentityFlowStarted = false;
    private bool isShowingAuthFlow = false;
    private bool isShowingUserFlow = false;

    // 📍 СТРОКИ 59-60: СОБЫТИЯ
    public event Action OnSignedOut;
    public event Action OnSignedIn;
}
```

## 🎯 План разбиения на 5 компонентов

### 1️⃣ **IdentityOrchestrator** (120 строк) - Главный координатор

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `public static IdentityUIController Instance` | 45 | Singleton pattern | `IdentityOrchestrator.Instance` |
| `[SerializeField] private bool isLite` | 47 | Настройка режима | `IdentityOrchestrator.isLite` |
| `[SerializeField] protected bool debugLogging` | 48 | Debug настройки | `IdentityOrchestrator.debugLogging` |
| `public event Action OnSignedOut` | 59 | Событие выхода | `IdentityOrchestrator.OnSignedOut` |
| `public event Action OnSignedIn` | 60 | Событие входа | `IdentityOrchestrator.OnSignedIn` |
| `protected virtual void Awake()` | 179-221 | Unity lifecycle | `IdentityOrchestrator.Awake()` |
| `void Start()` | 223-225 | Unity lifecycle | `IdentityOrchestrator.Start()` |
| `private void OnDestroy()` | 227-308 | Cleanup | `IdentityOrchestrator.OnDestroy()` |
| `OnUserSignedIn()` | 148-162 | Event handler | `IdentityOrchestrator.OnUserSignedIn()` |
| `OnUserSignedOut()` | 164-177 | Event handler | `IdentityOrchestrator.OnUserSignedOut()` |

#### Новая структура IdentityOrchestrator:
```csharp
/// <summary>
/// Главный координатор Identity системы. 
/// Только координация - никакой бизнес-логики!
/// </summary>
public class IdentityOrchestrator : MonoBehaviour
{
    public static IdentityOrchestrator Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool isLite = false;
    [SerializeField] private bool debugLogging = false;
    
    // Менеджеры (инжектируются)
    private ICanvasManager canvasManager;
    private IAuthFlowManager authFlowManager;
    private IUserFlowManager userFlowManager;
    private IStateManager stateManager;
    private IIdentityService identityService;
    private IServiceContainer serviceContainer;
    
    // События (публичный API)
    public event Action OnSignedOut;
    public event Action OnSignedIn;
    
    // Публичные свойства
    public bool IsOpen => canvasManager?.IsOpen ?? false;
    public bool IsLite => isLite;
    
    #region Unity Lifecycle
    
    protected virtual void Awake()
    {
        // Singleton setup (из строк 179-202)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Dependency injection setup
        InitializeServiceContainer();
        InjectDependencies();
        SubscribeToEvents();
    }
    
    void Start()
    {
        // Запуск системы (из строки 223-225)
        stateManager.StartInitialFlow().Forget();
    }
    
    private void OnDestroy()
    {
        // Cleanup logic (из строк 227-308)
        UnsubscribeFromEvents();
        CleanupServices();
        
        if (Instance == this)
            Instance = null;
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnStateChanged(IdentityState oldState, IdentityState newState)
    {
        // Простая координация между менеджерами
        switch (newState)
        {
            case IdentityState.SignedOut:
                OnSignedOut?.Invoke();
                authFlowManager.StartAuthFlowAsync(destroyCancellationToken).Forget();
                break;
                
            case IdentityState.SignedIn:
                OnSignedIn?.Invoke();
                userFlowManager.StartUserFlowAsync(destroyCancellationToken).Forget();
                break;
        }
    }
    
    #endregion
    
    #region Service Management
    
    private void InitializeServiceContainer()
    {
        serviceContainer = new IdentityServiceContainer();
        serviceContainer.ConfigureServices(debugLogging, isLite);
    }
    
    private void InjectDependencies()
    {
        canvasManager = serviceContainer.Resolve<ICanvasManager>();
        authFlowManager = serviceContainer.Resolve<IAuthFlowManager>();
        userFlowManager = serviceContainer.Resolve<IUserFlowManager>();
        stateManager = serviceContainer.Resolve<IStateManager>();
        identityService = serviceContainer.Resolve<IIdentityService>();
    }
    
    #endregion
}
```

---

### 2️⃣ **CanvasManager** (100 строк) - Управление UI

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `public bool IsOpen { get; private set; }` | 49 | UI состояние | `CanvasManager.IsOpen` |
| `private IdentityCanvasController currentCanvasController` | 54 | Canvas reference | `CanvasManager.currentCanvasController` |
| `public void SetCanvasController()` | 82-105 | Canvas setup | `CanvasManager.SetCanvasController()` |
| `public void ToggleOpenState()` | 108-111 | UI toggle | `CanvasManager.ToggleOpenState()` |
| `public void SetOpenState(bool isOpen)` | 113-127 | UI state | `CanvasManager.SetOpenState()` |
| `protected ViewManager GetViewManager()` | 129-132 | ViewManager access | `CanvasManager.GetViewManager()` |
| `private void OnCanvasOpenStateChanged()` | 134-140 | Canvas events | `CanvasManager.OnCanvasOpenStateChanged()` |

#### Новая структура CanvasManager:
```csharp
/// <summary>
/// Управляет Canvas и ViewManager состоянием.
/// Только UI отображение - никакой бизнес-логики!
/// </summary>
public class CanvasManager : ICanvasManager
{
    private IdentityCanvasController currentCanvasController;
    private readonly bool debugLogging;
    
    public bool IsOpen { get; private set; }
    public event Action<bool> OnOpenStateChanged;
    
    public CanvasManager(bool debugLogging)
    {
        this.debugLogging = debugLogging;
    }
    
    #region Canvas Management (из строк 82-140)
    
    public void SetCanvasController(IdentityCanvasController canvasController)
    {
        // Точный перенос из строк 82-105
        if (currentCanvasController != null)
        {
            currentCanvasController.OnOpenStateChanged -= OnCanvasOpenStateChanged;
        }

        currentCanvasController = canvasController;
        
        if (currentCanvasController != null)
        {
            currentCanvasController.OnOpenStateChanged += OnCanvasOpenStateChanged;
            
            var viewManager = currentCanvasController.GetViewManager();
            if (viewManager != null)
            {
                viewManager.InitializeLoading();
            }
            
            if (debugLogging)
                Debug.Log($"Canvas controller set: {currentCanvasController.name}");
        }
    }
    
    public void ToggleOpenState()
    {
        // Точный перенос из строк 108-111
        SetOpenState(!IsOpen);
    }
    
    public void SetOpenState(bool isOpen)
    {
        // Точный перенос из строк 113-127
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
    
    public ViewManager GetViewManager()
    {
        // Точный перенос из строк 129-132
        return currentCanvasController?.GetViewManager();
    }
    
    private void OnCanvasOpenStateChanged(bool isOpen)
    {
        // Точный перенос из строк 134-140
        IsOpen = isOpen;
        
        if (debugLogging)
            Debug.Log($"Canvas state changed, UI state updated to: {(isOpen ? "open" : "closed")}");
            
        OnOpenStateChanged?.Invoke(isOpen);
    }
    
    #endregion
}
```

---

### 3️⃣ **AuthFlowManager** (280 строк) - Авторизационные потоки

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `private bool isShowingAuthFlow = false` | 56 | State flag | `AuthFlowManager.isShowingAuthFlow` |
| `private async UniTask ShowAuthFlow()` | 457-574 | Главный auth flow | `AuthFlowManager.StartAuthFlowAsync()` |
| Email authentication logic | 504-537 | Email flow | `AuthFlowManager.HandleEmailAuth()` |
| Google authentication logic | 539-544 | Google flow | `AuthFlowManager.HandleGoogleAuth()` |
| Apple authentication logic | 546-551 | Apple flow | `AuthFlowManager.HandleAppleAuth()` |
| Telegram authentication logic | 553-559 | Telegram flow | `AuthFlowManager.HandleTelegramAuth()` |

#### Новая структура AuthFlowManager:
```csharp
/// <summary>
/// Управляет всеми авторизационными потоками.
/// Email, Google, Apple, Telegram authentication.
/// </summary>
public class AuthFlowManager : IAuthFlowManager
{
    private readonly IIdentityService identityService;
    private readonly ICanvasManager canvasManager;
    private readonly IStateManager stateManager;
    private readonly IErrorHandler errorHandler;
    private readonly bool debugLogging;
    
    private bool isShowingAuthFlow = false; // Перенос из строки 56
    
    public AuthFlowManager(
        IIdentityService identityService,
        ICanvasManager canvasManager,
        IStateManager stateManager,
        IErrorHandler errorHandler,
        bool debugLogging)
    {
        this.identityService = identityService;
        this.canvasManager = canvasManager;
        this.stateManager = stateManager;
        this.errorHandler = errorHandler;
        this.debugLogging = debugLogging;
    }
    
    #region Main Auth Flow (из строк 457-574)
    
    public async UniTask StartAuthFlowAsync(CancellationToken ct)
    {
        // Точный перенос логики из ShowAuthFlow (строки 457-479)
        if (isShowingAuthFlow)
        {
            Debug.LogWarning("ShowAuthFlow already running, skipping duplicate call");
            return;
        }
        
        isShowingAuthFlow = true;
        
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Проверки из строк 480-495
                    if (!ValidateState(ct))
                        return;
                    
                    var viewManager = GetViewManager();
                    if (viewManager == null)
                    {
                        await WaitAndContinue(ct);
                        continue;
                    }

                    // Canvas management (строка 499)
                    canvasManager.SetOpenState(true);
                    
                    // SignIn View показ (строки 501-503)
                    var result = await viewManager.Show<SignInView, SignInViewParams, SignInViewResult>(
                        new SignInViewParams(), ct);

                    // Обработка методов авторизации (строки 502-574)
                    await ProcessSignInMethod(result, ct);
                    
                    // Закрытие Canvas после успеха (строки 562-563)
                    canvasManager.SetOpenState(false);
                    return;
                }
                catch (OperationCanceledException)
                {
                    if (debugLogging)
                        Debug.Log("ShowAuthFlow cancelled");
                    return;
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
    
    #endregion
    
    #region Authentication Methods
    
    private async UniTask ProcessSignInMethod(SignInViewResult result, CancellationToken ct)
    {
        AuthResult authResult = null;

        switch (result.Method)
        {
            case SignInMethod.Email:
                await HandleEmailAuth(result.Email, ct);
                break;

            case SignInMethod.Google:
                authResult = await HandleGoogleAuth(ct);
                break;

            case SignInMethod.Apple:
                authResult = await HandleAppleAuth(ct);
                break;

            case SignInMethod.Telegram:
                authResult = await HandleTelegramAuth(ct);
                break;
        }
    }
    
    private async UniTask HandleEmailAuth(string email, CancellationToken ct)
    {
        // Точный перенос Email flow логики (строки 504-537)
        Func<CancellationToken, UniTask> startEmailFlow = (ct) => identityService
                .StartEmailFlow(email, ct)
                .WithLoading(ct);
        await startEmailFlow.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

        Func<CancellationToken, UniTask<AuthResult>> confirmEmailCode = async (ct) =>
        {
            string code = null;
            string emailForCode = email;

            while (code == null)
            {
                var codeResult = await GetViewManager().Show<CodeView, CodeViewParams, CodeViewResult>(
                    new CodeViewParams(), ct);

                if (codeResult.Code == "RESEND")
                {
                    await identityService.StartEmailFlow(emailForCode, ct).WithLoading(ct);
                    continue;
                }

                code = codeResult.Code;
            }

            return await identityService
                .ConfirmEmailCode(code, ct)
                .WithLoading(ct);
        };
        await confirmEmailCode.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
    }
    
    private async UniTask<AuthResult> HandleGoogleAuth(CancellationToken ct)
    {
        // Точный перенос Google auth (строки 539-544)
        Func<CancellationToken, UniTask<AuthResult>> signInWithGoogle = (ct) => identityService
            .SignInWithGoogle(false, ct)
            .WithLoading(ct);
        return await signInWithGoogle.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
    }
    
    private async UniTask<AuthResult> HandleAppleAuth(CancellationToken ct)
    {
        // Точный перенос Apple auth (строки 546-551)
        Func<CancellationToken, UniTask<AuthResult>> signInWithApple = (ct) => identityService
            .SignInWithApple(false, ct)
            .WithLoading(ct);
        return await signInWithApple.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
    }
    
    private async UniTask<AuthResult> HandleTelegramAuth(CancellationToken ct)
    {
        // Точный перенос Telegram auth (строки 553-559)
        Func<CancellationToken, UniTask<AuthResult>> signInWithTelegram = (ct) => identityService
            .SignInWithTelegramAsync(false, ct)
            .WithLoading(ct);
        return await signInWithTelegram.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
    }
    
    #endregion
    
    #region Helpers
    
    private ViewManager GetViewManager()
    {
        return canvasManager.GetViewManager();
    }
    
    private bool ValidateState(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return false;
            
        // Можно добавить дополнительные проверки состояния
        return stateManager.CurrentState == IdentityState.AuthenticationInProgress;
    }
    
    private async UniTask WaitAndContinue(CancellationToken ct)
    {
        if (debugLogging)
            Debug.LogWarning("No ViewManager available, waiting...");
        await UniTask.Delay(1000, cancellationToken: ct);
    }
    
    #endregion
}
```

---

### 4️⃣ **UserFlowManager** (350 строк) - Пользовательские потоки

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `private bool isShowingUserFlow = false` | 57 | State flag | `UserFlowManager.isShowingUserFlow` |
| `protected virtual async UniTask ShowUserFlow()` | 576-631 | User profile flow | `UserFlowManager.StartUserFlowAsync()` |
| `protected async UniTask ShowSettings()` | 633-706 | Settings flow | `UserFlowManager.ShowSettingsAsync()` |
| `private async UniTask ShowChangeName()` | 715-723 | Change name flow | `UserFlowManager.ShowChangeNameAsync()` |
| `private async UniTask ShowChangeEmail()` | 725-758 | Change email flow | `UserFlowManager.ShowChangeEmailAsync()` |
| `private async UniTask ShowDeleteAccount()` | 760-781 | Delete account flow | `UserFlowManager.ShowDeleteAccountAsync()` |

#### Новая структура UserFlowManager:
```csharp
/// <summary>
/// Управляет потоками авторизованного пользователя.
/// Profile, Settings, Account management.
/// </summary>
public class UserFlowManager : IUserFlowManager
{
    private readonly IUserService userService;
    private readonly IIdentityService identityService;
    private readonly ICanvasManager canvasManager;
    private readonly IErrorHandler errorHandler;
    private readonly bool debugLogging;
    
    private bool isShowingUserFlow = false; // Перенос из строки 57
    
    public UserFlowManager(
        IUserService userService,
        IIdentityService identityService,
        ICanvasManager canvasManager,
        IErrorHandler errorHandler,
        bool debugLogging)
    {
        this.userService = userService;
        this.identityService = identityService;
        this.canvasManager = canvasManager;
        this.errorHandler = errorHandler;
        this.debugLogging = debugLogging;
    }
    
    #region Main User Flow (из строк 576-631)
    
    public async UniTask StartUserFlowAsync(CancellationToken ct)
    {
        // Точный перенос ShowUserFlow логики (строки 576-597)
        if (isShowingUserFlow)
        {
            Debug.LogWarning("ShowUserFlow already running, skipping duplicate call");
            return;
        }
        
        isShowingUserFlow = true;
        
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Проверки состояния (строки 598-606)
                    if (!ValidateState(ct))
                        return;
                    
                    var viewManager = GetViewManager();
                    if (viewManager == null)
                    {
                        await WaitAndContinue(ct);
                        continue;
                    }

                    // Получение пользователя и показ UserView (строки 608-618)
                    Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                        .GetUserAsync(ct)
                        .WithLoading(ct);

                    var user = await getUser.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

                    var result = await viewManager
                        .Show<UserView, UserViewParams, UserViewResult>(
                            new UserViewParams(user.Name), ct);

                    // Обработка действий пользователя (строки 620-631)
                    await ProcessUserAction(result.Action, ct);
                }
                catch (OperationCanceledException)
                {
                    if (debugLogging)
                        Debug.Log("ShowUserFlow cancelled");
                    return;
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
    
    private async UniTask ProcessUserAction(UserAction action, CancellationToken ct)
    {
        switch (action)
        {
            case UserAction.OpenSettings:
                await ShowSettingsAsync(ct);
                break;

            case UserAction.SignOut:
                await identityService.SignOut(ct);
                return;
        }
    }
    
    #endregion
    
    #region Settings Flow (из строк 633-706)
    
    public async UniTask ShowSettingsAsync(CancellationToken ct)
    {
        // Точный перенос ShowSettings логики (строки 633-650)
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

                // Получение пользователя для настроек (строки 652-656)
                Func<CancellationToken, UniTask<UserDto>> getUser = (ct) => userService
                    .GetUserAsync(ct)
                    .WithLoading(ct);

                var user = await getUser.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

                // Показ SettingsView (строки 658-665)
                var result = await viewManager
                    .Show<SettingsView, SettingsViewParams, SettingsViewResult>(
                        new SettingsViewParams(
                            user.Name,
                            user.Email,
                            user.AuthProviders.Contains("google.com"),
                            user.AuthProviders.Contains("apple.com"),
                            user.AuthProviders.Contains("telegram.org")), ct);

                // Обработка действий в настройках (строки 667-706)
                if (!await ProcessSettingsAction(result.Action, ct))
                    return;
            }
            catch (OperationCanceledException)
            {
                if (debugLogging)
                    Debug.Log("ShowSettings cancelled");
                return;
            }
            catch (SignOutRequiredException)
            {
                await identityService.SignOut(ct);
                continue;
            }
        }
    }
    
    private async UniTask<bool> ProcessSettingsAction(SettingsAction action, CancellationToken ct)
    {
        switch (action)
        {
            case SettingsAction.ChangeName:
                await ShowChangeNameAsync(ct);
                return true;

            case SettingsAction.ChangeEmail:
                await ShowChangeEmailAsync(ct);
                return true;

            case SettingsAction.DeleteAccount:
                await ShowDeleteAccountAsync(ct);
                await identityService.SignOut(ct);
                return false;

            case SettingsAction.AddGoogleProvider:
                await AddProviderAsync(SignInMethod.Google, ct);
                return true;

            case SettingsAction.AddAppleProvider:
                await AddProviderAsync(SignInMethod.Apple, ct);
                return true;

            case SettingsAction.AddTelegramProvider:
                await AddProviderAsync(SignInMethod.Telegram, ct);
                return true;

            case SettingsAction.Close:
                return false;
                
            default:
                return true;
        }
    }
    
    #endregion
    
    #region Account Management Methods
    
    public async UniTask ShowChangeNameAsync(CancellationToken ct)
    {
        // Точный перенос ShowChangeName (строки 715-723)
        var viewManager = GetViewManager();
        if (viewManager == null)
            throw new InvalidOperationException("No ViewManager available");

        var result = await viewManager.Show<ChangeNameView, ChangeNameViewParams, ChangeNameViewResult>(
            new ChangeNameViewParams(), ct);

        await userService.UpdateNameAsync(result.Name, ct);
    }
    
    public async UniTask ShowChangeEmailAsync(CancellationToken ct)
    {
        // Точный перенос ShowChangeEmail (строки 725-758)
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

        token = await requestEmailChange.WithErrorHandler(errorHandler.ShowErrorAsync, ct);

        Func<CancellationToken, UniTask> confirmEmailCode = async (ct) =>
        {
            string code = null;
            while (code == null)
            {
                var result = await viewManager.Show<CodeView, CodeViewParams, CodeViewResult>(
                    new CodeViewParams(), ct);

                if (result.Code == "RESEND")
                {
                    token = await userService.RequestEmailChangeAsync(email, ct).WithLoading(ct);
                    continue;
                }

                code = result.Code;
            }

            await userService
                .ConfirmEmailChangeAsync(token, code, ct)
                .WithLoading(ct);
        };

        await confirmEmailCode.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
    }
    
    public async UniTask ShowDeleteAccountAsync(CancellationToken ct)
    {
        // Точный перенос ShowDeleteAccount (строки 760-781)
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
                continue;
            }

            code = result.Code;
        }

        await userService.ConfirmDeleteAccountAsync(token, code, ct);
        await identityService.SignOut(ct);
    }
    
    private async UniTask AddProviderAsync(SignInMethod method, CancellationToken ct)
    {
        // Логика добавления провайдеров из строк 677-701
        switch (method)
        {
            case SignInMethod.Google:
                Func<CancellationToken, UniTask<AuthResult>> addGoogle = (ct) => identityService
                    .SignInWithGoogle(true, ct)
                    .WithLoading(ct);
                await addGoogle.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
                break;

            case SignInMethod.Apple:
                Func<CancellationToken, UniTask<AuthResult>> addApple = (ct) => identityService
                    .SignInWithApple(true, ct)
                    .WithLoading(ct);
                await addApple.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
                break;

            case SignInMethod.Telegram:
                Func<CancellationToken, UniTask<AuthResult>> addTelegram = (ct) => identityService
                    .SignInWithTelegramAsync(true, ct)
                    .WithLoading(ct);
                await addTelegram.WithErrorHandler(errorHandler.ShowErrorAsync, ct);
                break;
        }
    }
    
    #endregion
    
    #region Helpers
    
    private ViewManager GetViewManager()
    {
        return canvasManager.GetViewManager();
    }
    
    private bool ValidateState(CancellationToken ct)
    {
        return !ct.IsCancellationRequested;
    }
    
    private async UniTask WaitAndContinue(CancellationToken ct)
    {
        if (debugLogging)
            Debug.LogWarning("No ViewManager available, waiting...");
        await UniTask.Delay(1000, cancellationToken: ct);
    }
    
    #endregion
}
```

---

### 5️⃣ **StateManager** (120 строк) - Управление состоянием

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `private bool isIdentityFlowStarted = false` | 55 | Initialization flag | `StateManager` (состояние) |
| `private bool isShowingAuthFlow = false` | 56 | Auth flow flag | Удалить (заменить состоянием) |
| `private bool isShowingUserFlow = false` | 57 | User flow flag | Удалить (заменить состоянием) |
| `private async UniTask StartIdentityFlow()` | 397-456 | Initialization logic | `StateManager.StartInitialFlow()` |
| Event handlers OnUserSignedIn/Out | 148-177 | State transitions | `StateManager` events |

#### Новая структура StateManager:
```csharp
/// <summary>
/// Централизованное управление состоянием Identity системы.
/// Заменяет разбросанные булевые флаги четкой State Machine.
/// </summary>
public class StateManager : IStateManager
{
    private readonly IIdentityService identityService;
    private readonly bool debugLogging;
    
    public IdentityState CurrentState { get; private set; } = IdentityState.Uninitialized;
    public event Action<IdentityState, IdentityState> StateChanged;
    
    // Allowed state transitions
    private readonly Dictionary<IdentityState, List<IdentityState>> allowedTransitions = new()
    {
        [IdentityState.Uninitialized] = new() { IdentityState.Initializing },
        [IdentityState.Initializing] = new() { IdentityState.SignedOut, IdentityState.SignedIn, IdentityState.Error },
        [IdentityState.SignedOut] = new() { IdentityState.AuthenticationInProgress },
        [IdentityState.AuthenticationInProgress] = new() { 
            IdentityState.SignedIn, IdentityState.SignedOut, IdentityState.Error 
        },
        [IdentityState.SignedIn] = new() { IdentityState.UserFlowActive, IdentityState.SignedOut },
        [IdentityState.UserFlowActive] = new() { 
            IdentityState.SettingsOpen, IdentityState.SignedOut, IdentityState.SignedIn 
        },
        [IdentityState.SettingsOpen] = new() { 
            IdentityState.UserFlowActive, IdentityState.SignedOut 
        },
        [IdentityState.Error] = new() { 
            IdentityState.SignedOut, IdentityState.SignedIn, IdentityState.Initializing 
        }
    };
    
    public StateManager(IIdentityService identityService, bool debugLogging)
    {
        this.identityService = identityService;
        this.debugLogging = debugLogging;
        
        // Subscribe to identity service events
        identityService.OnSignedIn += OnUserSignedIn;
        identityService.OnSignedOut += OnUserSignedOut;
    }
    
    #region State Machine
    
    public bool CanTransitionTo(IdentityState newState)
    {
        if (!allowedTransitions.ContainsKey(CurrentState))
            return false;
            
        return allowedTransitions[CurrentState].Contains(newState);
    }
    
    public void TransitionTo(IdentityState newState)
    {
        if (!CanTransitionTo(newState))
        {
            var errorMsg = $"Invalid state transition from {CurrentState} to {newState}";
            if (debugLogging)
                Debug.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }
        
        var oldState = CurrentState;
        CurrentState = newState;
        
        if (debugLogging)
            Debug.Log($"State transition: {oldState} → {newState}");
        
        StateChanged?.Invoke(oldState, newState);
    }
    
    #endregion
    
    #region Initialization (из строк 397-456)
    
    public async UniTask StartInitialFlow()
    {
        // Точный перенос StartIdentityFlow логики (строки 397-418)
        if (CurrentState != IdentityState.Uninitialized)
        {
            Debug.LogWarning("StartInitialFlow already called, skipping duplicate call");
            return;
        }
        
        TransitionTo(IdentityState.Initializing);
        
        try
        {
            if (debugLogging)
                Debug.Log("Initializing identity system");
                
            // Инициализация без WithLoading (строка 420-421)
            await identityService.Initialize(CancellationToken.None);
            
            // Проверка авторизации (строки 431-442)
            if (identityService.IsSignedIn)
            {
                if (debugLogging)
                    Debug.Log("User is already signed in");
                TransitionTo(IdentityState.SignedIn);
            }
            else
            {
                if (debugLogging)
                    Debug.Log("User is not signed in");
                TransitionTo(IdentityState.SignedOut);
            }
        }
        catch (OperationCanceledException)
        {
            if (debugLogging)
                Debug.Log("StartInitialFlow cancelled");
            TransitionTo(IdentityState.Error);
        }
        catch (Exception ex)
        {
            if (debugLogging)
                Debug.LogError($"Error in StartInitialFlow: {ex.Message}");
            TransitionTo(IdentityState.Error);
        }
    }
    
    #endregion
    
    #region Event Handlers (из строк 148-177)
    
    private void OnUserSignedIn(FirebaseUser user)
    {
        // Перенос из OnUserSignedIn (строки 148-162)
        if (debugLogging)
            Debug.Log("User signed in, transitioning to SignedIn state");
            
        if (CanTransitionTo(IdentityState.SignedIn))
        {
            TransitionTo(IdentityState.SignedIn);
        }
    }
    
    private void OnUserSignedOut()
    {
        // Перенос из OnUserSignedOut (строки 164-177)  
        if (debugLogging)
            Debug.Log("User signed out, transitioning to SignedOut state");
            
        if (CanTransitionTo(IdentityState.SignedOut))
        {
            TransitionTo(IdentityState.SignedOut);
        }
    }
    
    #endregion
    
    public void Dispose()
    {
        if (identityService != null)
        {
            identityService.OnSignedIn -= OnUserSignedIn;
            identityService.OnSignedOut -= OnUserSignedOut;
        }
    }
}

public enum IdentityState
{
    Uninitialized,      // Система не инициализирована
    Initializing,       // Происходит инициализация
    SignedOut,          // Пользователь не авторизован
    AuthenticationInProgress,  // Идет процесс авторизации
    SignedIn,           // Пользователь авторизован
    UserFlowActive,     // Активен пользовательский поток
    SettingsOpen,       // Открыты настройки
    Error               // Произошла ошибка
}
```

---

### 6️⃣ **ErrorHandler** (60 строк) - Обработка ошибок

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| `protected async UniTask<ErrorHandlingMethod> ShowErrorAsync()` | 843-860 | Error handling | `ErrorHandler.ShowErrorAsync()` |

#### Новая структура ErrorHandler:
```csharp
/// <summary>
/// Централизованная обработка ошибок для Identity системы.
/// </summary>
public class ErrorHandler : IErrorHandler
{
    private readonly ICanvasManager canvasManager;
    private readonly bool debugLogging;
    
    public ErrorHandler(ICanvasManager canvasManager, bool debugLogging)
    {
        this.canvasManager = canvasManager;
        this.debugLogging = debugLogging;
    }
    
    public async UniTask<ErrorHandlingMethod> ShowErrorAsync(Energy8Exception e8Exception, CancellationToken ct)
    {
        // Точный перенос из ShowErrorAsync (строки 843-860)
        var viewManager = canvasManager.GetViewManager();
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
}
```

---

### 7️⃣ **ServiceContainer** (100 строк) - Dependency Injection

#### Функционал из IdentityUIController:
| **Оригинальный метод/свойство** | **Строки** | **Описание** | **Новое место** |
|--------------------------------|------------|--------------|-----------------|
| Service initialization в Awake | 204-215 | DI setup | `ServiceContainer.ConfigureServices()` |

#### Новая структура ServiceContainer:
```csharp
/// <summary>
/// Dependency Injection контейнер для Identity системы.
/// </summary>
public class IdentityServiceContainer : IServiceContainer
{
    private readonly Dictionary<Type, Func<object>> services = new();
    private readonly Dictionary<Type, object> singletons = new();
    
    public void ConfigureServices(bool debugLogging, bool isLite)
    {
        // Точный перенос создания зависимостей из Awake (строки 204-215)
        
        // Core services
        RegisterSingleton<IHttpClient>(() => new UnityHttpClient(IdentityConfiguration.SelectedIP));
        RegisterSingleton<IAuthProvider>(() => AuthProviderFactory.CreateProvider(Resolve<IHttpClient>()));
        RegisterSingleton<IUserService>(() => new Energy8.Identity.User.Runtime.Services.UserService(
            Resolve<IHttpClient>(), Resolve<IAuthProvider>()));
        
        var analyticsProvider = AnalyticsProviderFactory.CreateProvider();
        RegisterSingleton<IAnalyticsService>(() => new AnalyticsService(analyticsProvider));
        
        RegisterSingleton<IIdentityService>(() => new IdentityService(
            Resolve<IAuthProvider>(), 
            Resolve<IUserService>(), 
            Resolve<IHttpClient>(), 
            Resolve<IAnalyticsService>()));
        
        // UI Managers
        RegisterSingleton<ICanvasManager>(() => new CanvasManager(debugLogging));
        RegisterSingleton<IStateManager>(() => new StateManager(Resolve<IIdentityService>(), debugLogging));
        RegisterSingleton<IErrorHandler>(() => new ErrorHandler(Resolve<ICanvasManager>(), debugLogging));
        
        RegisterSingleton<IAuthFlowManager>(() => new AuthFlowManager(
            Resolve<IIdentityService>(),
            Resolve<ICanvasManager>(),
            Resolve<IStateManager>(),
            Resolve<IErrorHandler>(),
            debugLogging));
            
        RegisterSingleton<IUserFlowManager>(() => new UserFlowManager(
            Resolve<IUserService>(),
            Resolve<IIdentityService>(),
            Resolve<ICanvasManager>(),
            Resolve<IErrorHandler>(),
            debugLogging));
    }
    
    public void RegisterSingleton<T>(Func<T> factory) where T : class
    {
        services[typeof(T)] = () => factory();
    }
    
    public T Resolve<T>() where T : class
    {
        var type = typeof(T);
        
        if (singletons.ContainsKey(type))
            return (T)singletons[type];
            
        if (services.ContainsKey(type))
        {
            var instance = (T)services[type]();
            singletons[type] = instance;
            return instance;
        }
        
        throw new InvalidOperationException($"Service {type.Name} not registered");
    }
}
```

---

## 📊 Результат разбиения

### ❌ До рефакторинга:
```
IdentityUIController.cs: 891 строка
├── Singleton pattern
├── Canvas management  
├── Auth flow logic
├── User flow logic
├── Settings management
├── Account operations
├── State management
├── Error handling
├── Service initialization
└── Unity lifecycle
```

### ✅ После рефакторинга:
```
IdentityOrchestrator.cs: ~120 строк
├── Singleton pattern
├── Service coordination
└── Unity lifecycle

CanvasManager.cs: ~100 строк
├── Canvas management
└── ViewManager access

AuthFlowManager.cs: ~280 строк
├── Email authentication
├── Social authentication
└── Auth error handling

UserFlowManager.cs: ~350 строк  
├── User profile flow
├── Settings management
└── Account operations

StateManager.cs: ~120 строк
├── State machine logic
├── State transitions
└── State events

ErrorHandler.cs: ~60 строк
└── Centralized error handling

ServiceContainer.cs: ~100 строк
└── Dependency injection
```

## 🎯 Преимущества после разбиения

### ✅ **Single Responsibility Principle**
- Каждый компонент отвечает за одну область функциональности
- Легко понять назначение каждого класса

### ✅ **Тестируемость**
- Можно тестировать каждый компонент изолированно
- Легко создавать мок-объекты для зависимостей

### ✅ **Поддерживаемость**  
- Изменения в одной области не затрагивают другие
- Легко добавлять новый функционал

### ✅ **Читаемость**
- Код разбит на логические блоки
- Каждый файл фокусируется на одной задаче

### ✅ **Масштабируемость**
- Просто добавлять новые типы авторизации
- Легко расширять пользовательские потоки

## 📅 План миграции (8 недель)

### Week 1: Интерфейсы и StateManager
- Создать все интерфейсы (ICanvasManager, IAuthFlowManager, etc.)
- Реализовать StateManager с enum IdentityState
- Создать ErrorHandler

### Week 2: CanvasManager  
- Перенести Canvas Management секцию (строки 75-140)
- Тестирование UI состояния

### Week 3: ServiceContainer
- Перенести инициализацию зависимостей (строки 204-215)
- Настроить Dependency Injection

### Week 4-5: AuthFlowManager
- Перенести ShowAuthFlow (строки 457-574)
- Все authentication методы
- Тестирование auth потоков

### Week 6-7: UserFlowManager  
- Перенести ShowUserFlow (строки 576-631)
- ShowSettings (строки 633-706)
- Account management методы (строки 715-781)

### Week 8: IdentityOrchestrator
- Создать главный координатор
- Перенести Unity lifecycle
- Финальное тестирование и удаление старого IdentityUIController

---

**Итог:** 891 строка God Object превращается в 6 специализированных компонентов с четкими обязанностями, тестируемых и поддерживаемых! 🚀
