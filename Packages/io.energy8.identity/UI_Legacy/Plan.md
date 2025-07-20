# План модернизации UI модуля Energy8.Identity

## 🎯 Цели модернизации

1. **Разделение ответственностей** - каждый компонент отвечает только за свою задачу
2. **Улучшение тестируемости** - возможность unit-тестирования отдельных частей
3. **Повышение переиспользуемости** - UI компоненты можно использовать в других проектах
4. **Упрощение поддержки** - код становится более понятным и модифицируемым

## 🔍 Анализ текущих проблем

### ❌ Проблемы IdentityUIController (891 строка)

#### Нарушение Single Responsibility Principle
```csharp
public class IdentityUIController : MonoBehaviour
{
    // 1. UI управление
    public bool IsOpen { get; private set; }
    public void SetOpenState(bool isOpen) { }
    
    // 2. Canvas управление  
    private IdentityCanvasController currentCanvasController;
    public void SetCanvasController(IdentityCanvasController canvasController) { }
    
    // 3. Сервисы и зависимости
    protected IHttpClient httpClient;
    private IAuthProvider authProvider;
    protected IUserService userService;
    
    // 4. Бизнес-логика авторизации
    private async UniTask ShowAuthFlow(CancellationToken ct) { }
    
    // 5. Бизнес-логика пользовательских потоков
    protected virtual async UniTask ShowUserFlow(CancellationToken ct) { }
    
    // 6. Lifecycle управление
    protected virtual void Awake() { }
    private void OnDestroy() { }
    
    // 7. Обработка ошибок
    protected async UniTask<ErrorHandlingMethod> ShowErrorAsync() { }
}
```

**Проблема**: Один класс выполняет 7+ различных ролей!

### ❌ Проблемы текущей системы Views

После анализа кода, вот конкретные проблемы с текущей архитектурой Views:

#### 1. Жесткая связанность с ViewManager через UniTaskCompletionSource
```csharp
// ViewBase.cs - Views управляют своим жизненным циклом через CompletionSource
public abstract class ViewBase<TParams, TResult> : MonoBehaviour, IView<TParams, TResult>
{
    private protected UniTaskCompletionSource<TResult> completionSource;
    
    // View должна только отображать данные, но сейчас она:
    public virtual async UniTask<TResult> ProcessAsync(CancellationToken ct)
    {
        // Управляет асинхронным lifecycle'ом!
        var result = await completionSource.Task.AttachExternalCancellation(ct);
        return result;
    }
}

// SignInView.cs - View принимает бизнес-решения
private void OnNextButtonClick()
{
    // View решает, какой результат вернуть - это НЕ ее ответственность!
    completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Email, emailInputField.text));
}
```

**Проблема**: View управляет своим жизненным циклом и принимает бизнес-решения, вместо того чтобы просто уведомлять о пользовательских действиях.

#### 2. ViewResult используется для бизнес-логики
```csharp
// SignInViewResult.cs - содержит бизнес-энумы
public enum SignInMethod
{
    Email,    // Это бизнес-логика, не UI!
    Google,   // View не должна знать о способах аутентификации
    Apple,
    Telegram
}

// В IdentityUIController.cs это используется для бизнес-решений:
switch (result.Method)  // View диктует бизнес-логику!
{
    case SignInMethod.Email:
        await identityService.StartEmailFlow(result.Email, ct);
        break;
    case SignInMethod.Google:
        authResult = await identityService.SignInWithGoogle(false, ct);
        break;
}
```

**Проблема**: View определяет бизнес-поток, хотя должна только сообщать "пользователь нажал кнопку Google".

#### 3. Смешивание UI-валидации с бизнес-валидацией
```csharp
// SignInView.cs
private static bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;
    return Regex.IsMatch(email, EMAIL_PATTERN); // Бизнес-правила в UI!
}

private void OnEmailChanged(string email)
{
    nextButton.interactable = IsValidEmail(email); // View принимает решение о валидности
}
```

**Проблема**: View содержит бизнес-правила валидации, которые должны быть в бизнес-слое.

#### 4. Отсутствие разделения между данными и поведением
```csharp
// ViewBase требует жесткой связки Params и Result
public abstract class ViewBase<TParams, TResult> : MonoBehaviour, IView<TParams, TResult>
    where TParams : ViewParams
    where TResult : ViewResult

// Каждая View ДОЛЖНА иметь Result, даже если она ничего не возвращает
// Пример: LoadingView возвращает LoadingViewResult - зачем?
```

**Проблема**: Принуждение к созданию Result классов даже там, где они не нужны.

#### 5. Монолитный ViewManager с множественными ответственностями
```csharp
// ViewManager.cs делает слишком много:
public class ViewManager : MonoBehaviour
{
    // 1. Создание Views (через ViewFactory)
    private IViewFactory factory;
    
    // 2. Презентация Views (через ViewPresenter)  
    private IViewPresenter presenter;
    
    // 3. Управление lifecycle
    private readonly CancellationTokenSource lifetimeCts = new();
    
    // 4. Обработка результатов и навигация
    public async UniTask<TResult> Show<TView, TParams, TResult>(...)
    {
        // Все в одном методе!
    }
}
```

**Проблема**: ViewManager - это God Object, который делает все.

#### 6. Отсутствие типобезопасности навигации
```csharp
// Текущий код полагается на правильность generic типов:
await viewManager.Show<UserView, UserViewParams, UserViewResult>(params, ct);

// Никто не гарантирует, что:
// - UserView действительно работает с UserViewParams
// - UserView возвращает UserViewResult
// - Типы соответствуют друг другу
```

**Проблема**: Compile-time проверки недостаточно, возможны runtime ошибки.

#### 7. Views содержат Unity-специфичную логику и бизнес-логику одновременно
```csharp
// SignInView.cs смешивает:
public class SignInView : ViewBase<SignInViewParams, SignInViewResult>
{
    // Unity UI логика (правильно)
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private Button nextButton;
    
    // Unity lifecycle (правильно)
    private void BindEvents() { }
    protected override void OnDestroy() { }
    
    // Бизнес-логика (неправильно!)
    private static bool IsValidEmail(string email) { }
    
    // Принятие бизнес-решений (неправильно!)
    private void OnNextButtonClick()
    {
        completionSource?.TrySetResult(new SignInViewResult(SignInMethod.Email, emailInputField.text));
    }
}
```

**Проблема**: Невозможно протестировать бизнес-логику отдельно от Unity компонентов.

### Почему это критично?

1. **Нарушение Single Responsibility**: View делает 4-5 разных задач
2. **Сложность тестирования**: Нельзя тестировать UI логику отдельно от бизнес-логики
3. **Жесткая связанность**: View знает о бизнес-сущностях (SignInMethod, AuthService)
4. **Невозможность переиспользования**: View привязана к конкретному бизнес-контексту
5. **Сложность изменений**: Смена бизнес-логики требует изменения UI кода

## 🎯 Правильный подход

### Как должно быть:
```csharp
// 1. View только отображает и уведомляет о действиях
public class SignInView : BaseView, ISignInView
{
    public event Action<string> OnEmailSubmitted;    // Уведомление, не решение!
    public event Action OnGoogleRequested;           // Уведомление, не метод авторизации!
    
    public void ShowEmailError(string error) { }     // Только отображение
    public void EnableSubmitButton(bool enabled) { } // Только UI состояние
    
    // НИ СЛОВА о бизнес-логике!
}

// 2. Presenter содержит всю логику
public class SignInPresenter : ISignInPresenter
{
    private readonly ISignInView view;
    private readonly IAuthService authService;
    private readonly INavigationService navigation;
    
    public async Task HandleEmailSubmitted(string email)
    {
        // ВСЯ бизнес-логика здесь
        if (!emailValidator.IsValid(email))
        {
            view.ShowEmailError("Invalid email");
            return;
        }
        
        try
        {
            await authService.StartEmailFlow(email);
            await navigation.NavigateToAsync<CodeVerificationView>();
        }
        catch (AuthException ex)
        {
            view.ShowEmailError(ex.Message);
        }
    }
}

// 3. Упрощенная навигация для последовательных Views
public interface INavigationService
{
    Task ShowAsync<TView>() where TView : IView;          // Показать View
    Task ReplaceAsync<TView>() where TView : IView;       // Заменить текущий View
    Task CloseWindowAsync();                              // Закрыть окно
}

// 4. Отдельный сервис управления окном
public interface IWindowService  
{
    Task OpenAsync();     // Открыть окно
    Task CloseAsync();    // Закрыть окно  
    bool IsOpen { get; }  // Состояние окна
}
```

### Преимущества такого подхода:
- ✅ **View тестируется отдельно**: Mock'аем events, проверяем UI изменения
- ✅ **Presenter тестируется отдельно**: Mock'аем View и сервисы  
- ✅ **Переиспользуемость**: View можно использовать в других контекстах
- ✅ **Изоляция изменений**: Смена бизнес-логики не затрагивает UI
- ✅ **Простота понимания**: Каждый класс делает одну вещь

## 🎯 Предлагаемая архитектура

### 📋 Новая структура модуля

```
UI/
├── Core/
│   ├── Interfaces/
│   │   ├── IView.cs                    # Базовый интерфейс для всех Views  
│   │   ├── IPresenter.cs               # Базовый интерфейс для Presenters
│   │   ├── INavigationService.cs       # Сервис навигации
│   │   ├── IViewFactory.cs             # Фабрика Views
│   │   └── IUICoordinator.cs           # Координатор UI системы
│   ├── Models/
│   │   ├── NavigationRequest.cs        # Модель запроса навигации
│   │   ├── ViewState.cs                # Состояние View
│   │   └── UICommand.cs                # Команды UI
│   └── Components/
│       ├── BaseView.cs                 # Базовый класс для Views
│       └── BasePresenter.cs            # Базовый класс для Presenters
├── Runtime/
│   ├── Navigation/
│   │   ├── NavigationService.cs        # Реализация навигации
│   │   ├── NavigationStack.cs          # Стек навигации
│   │   └── RouteDefinitions.cs         # Определения маршрутов
│   ├── Presentation/
│   │   ├── Presenters/                 # Presenters для каждой View
│   │   │   ├── SignInPresenter.cs
│   │   │   ├── UserPresenter.cs
│   │   │   └── SettingsPresenter.cs
│   │   └── ViewModels/                 # ViewModels для данных
│   │       ├── SignInViewModel.cs
│   │       └── UserProfileViewModel.cs
│   ├── Views/
│   │   ├── SignInView.cs               # Только UI логика
│   │   ├── UserView.cs                 # Только UI логика
│   │   └── SettingsView.cs             # Только UI логика
│   ├── Controllers/
│   │   ├── UICoordinator.cs            # Легковесный координатор (< 100 строк)
│   │   ├── AuthFlowController.cs       # Только авторизация
│   │   ├── UserFlowController.cs       # Только пользовательские потоки
│   │   └── CanvasController.cs         # Только Canvas управление
│   ├── Services/
│   │   ├── ViewFactory.cs              # Создание Views
│   │   ├── AnimationService.cs         # Анимации
│   │   └── ThemeService.cs             # Темы и стили
│   └── Extensions/
│       ├── ViewExtensions.cs           # Extension методы для Views
│       └── NavigationExtensions.cs     # Extension методы для навигации
└── Editor/
    └── Tools/
        ├── ViewGenerator.cs            # Генератор Views
        └── NavigationInspector.cs      # Инспектор навигации
```

## 📅 План реализации

### Этап 1: Создание новых интерфейсов и базовых классов (1 неделя)

#### День 1-2: Интерфейсы
```csharp
// 1. Базовые интерфейсы
public interface IView
{
    void Show();
    void Hide();
    bool IsVisible { get; }
}

public interface IView<TViewModel> : IView
{
    void SetViewModel(TViewModel viewModel);
}

public interface IPresenter<TView> where TView : IView
{
    TView View { get; }
    Task InitializeAsync();
    Task ShowAsync();
    Task HideAsync();
}
```

#### День 3-4: Навигационная система
```csharp
public interface INavigationService
{
    Task NavigateToAsync<T>() where T : IView;
    Task NavigateToAsync<T, TParam>(TParam parameter) where T : IView;
    Task NavigateBackAsync();
    bool CanNavigateBack { get; }
}

public class NavigationRequest
{
    public Type ViewType { get; set; }
    public object Parameters { get; set; }
    public NavigationMode Mode { get; set; } // Push, Replace, Modal
}
```

#### День 5: Dependency Injection
```csharp
public interface IServiceContainer
{
    void Register<TInterface, TImplementation>() 
        where TImplementation : class, TInterface;
    T Resolve<T>();
}
```

### Этап 2: Рефакторинг существующих Views (1.5 недели)

#### Пример: SignInView
```csharp
// ДО (текущий подход)
public class SignInView : ViewBase<SignInViewParams, SignInViewResult>
{
    // Смешивает UI и бизнес-логику
    public override async UniTask<SignInViewResult> ShowAsync(SignInViewParams parameters, CancellationToken ct)
    {
        // UI + бизнес-логика в одном методе
    }
}

// ПОСЛЕ (новый подход)
public class SignInView : BaseView, ISignInView
{
    public event Action<string> OnEmailSignInRequested;
    public event Action OnGoogleSignInRequested;  
    public event Action OnAppleSignInRequested;
    
    public void ShowLoading(bool show) { /* Только UI */ }
    public void ShowError(string error) { /* Только UI */ }
    
    // Только UI логика, никакой бизнес-логики!
}

public class SignInPresenter : BasePresenter<ISignInView>
{
    private readonly IAuthService authService;
    
    public SignInPresenter(ISignInView view, IAuthService authService) 
    {
        View = view;
        this.authService = authService;
        
        View.OnEmailSignInRequested += HandleEmailSignIn;
        View.OnGoogleSignInRequested += HandleGoogleSignIn;
    }
    
    private async void HandleEmailSignIn(string email)
    {
        View.ShowLoading(true);
        try
        {
            await authService.StartEmailFlow(email);
            // Навигация к следующему экрану
        }
        catch (Exception ex)
        {
            View.ShowError(ex.Message);
        }
        finally
        {
            View.ShowLoading(false);
        }
    }
}
```

### Этап 3: Создание специализированных контроллеров (1 неделя)

#### AuthFlowController
```csharp
public class AuthFlowController : IAuthFlowController
{
    private readonly INavigationService navigation;
    private readonly IAuthService authService;
    
    public async Task StartAuthFlowAsync()
    {
        await navigation.NavigateToAsync<SignInView>();
    }
    
    public async Task HandleSuccessfulAuth()
    {
        // Только логика авторизационного потока
        await navigation.NavigateToAsync<UserProfileView>();
    }
}
```

#### UserFlowController  
```csharp
public class UserFlowController : IUserFlowController
{
    public async Task ShowUserProfileAsync()
    {
        await navigation.NavigateToAsync<UserProfileView>();
    }
    
    public async Task ShowSettingsAsync()
    {
        await navigation.NavigateToAsync<SettingsView>();
    }
}
```

#### UICoordinator (замена IdentityUIController)
```csharp
public class UICoordinator : MonoBehaviour, IUICoordinator
{
    private IServiceContainer container;
    private IAuthFlowController authFlow;
    private IUserFlowController userFlow;
    
    // ТОЛЬКО координация - никакой бизнес-логики!
    public async Task InitializeAsync()
    {
        container = new ServiceContainer();
        RegisterServices();
        
        authFlow = container.Resolve<IAuthFlowController>();
        userFlow = container.Resolve<IUserFlowController>();
        
        // Определяем начальный поток
        if (await IsUserSignedIn())
        {
            await userFlow.ShowUserProfileAsync();
        }
        else
        {
            await authFlow.StartAuthFlowAsync();
        }
    }
    
    // Цель: < 100 строк кода!
}
```

### Этап 4: Миграция и тестирование (1 неделя)

#### Unit тесты
```csharp
[Test]
public async Task SignInPresenter_HandleEmailSignIn_CallsAuthService()
{
    // Arrange
    var mockView = new Mock<ISignInView>();
    var mockAuthService = new Mock<IAuthService>();
    var presenter = new SignInPresenter(mockView.Object, mockAuthService.Object);
    
    // Act
    await presenter.HandleEmailSignIn("test@example.com");
    
    // Assert
    mockAuthService.Verify(x => x.StartEmailFlow("test@example.com"), Times.Once);
}
```

## 🎯 Ожидаемые результаты

### Метрики улучшения
- **Размер основного контроллера**: 891 → < 100 строк (89% сокращение)
- **Количество ответственностей на класс**: 7+ → 1 (100% соответствие SRP)
- **Покрытие тестами**: 0% → 80%+
- **Время сборки**: Сокращение на 30% за счет модульности
- **Переиспользуемость**: Views можно использовать в других проектах

### Качественные улучшения  
- ✅ **Понятность кода**: Каждый класс делает одну вещь
- ✅ **Тестируемость**: Можно тестировать каждую часть отдельно
- ✅ **Расширяемость**: Легко добавлять новые Views и функции
- ✅ **Поддержка**: Баги локализованы в конкретных классах
- ✅ **Переиспользование**: UI компоненты работают независимо

## 🚀 Следующие шаги

1. **Согласование архитектуры** с командой
2. **Создание proof-of-concept** для одной View (SignIn)  
3. **Итеративная миграция** остальных Views
4. **Написание тестов** для новых компонентов
5. **Удаление старого кода** после полной миграции

---
*Этот план обеспечивает постепенный переход без нарушения существующей функциональности*
