# Energy8 Identity UI_Legacy - Monolithic Architecture Documentation

## 🎯 Обзор Legacy архитектуры

Старая система Identity UI построена на основе **монолитного IdentityUIController** (891 строка кода), который содержит всю логику авторизации и управления пользователем в одном классе.

## 📁 Структура Legacy проекта

```
UI_Legacy/
├── Core/
│   ├── TextButton.cs           # Кнопка с текстом
│   ├── OrientationController.cs # Управление ориентацией
│   └── ImageSpriteAnimation.cs  # Анимация спрайтов
│
├── Runtime/
│   ├── Controllers/
│   │   ├── IdentityUIController.cs      # 🔴 МОНОЛИТ (891 строка)
│   │   ├── IdentityCanvasController.cs  # Управление Canvas
│   │   └── GameIdentityUIController.cs  # Game-specific контроллер
│   │
│   ├── Services/
│   │   ├── IIdentityService.cs          # Интерфейс сервиса
│   │   └── IdentityService.cs           # Реализация сервиса
│   │
│   ├── Views/
│   │   ├── Management/                  # Система управления View
│   │   │   ├── ViewManager.cs           # Центральный менеджер
│   │   │   ├── ViewFactory.cs           # Фабрика View
│   │   │   └── ViewPresenter.cs         # Презентер View
│   │   ├── Base/                        # Базовые View
│   │   │   └── BaseIdentityView.cs      # Базовый класс
│   │   ├── Implementations/             # Конкретные View
│   │   │   ├── SignInView.cs           # Форма входа
│   │   │   ├── UserView.cs             # Профиль пользователя
│   │   │   ├── CodeView.cs             # Ввод кода
│   │   │   ├── SettingsView.cs         # Настройки
│   │   │   └── ErrorView.cs            # Ошибки
│   │   ├── Models/                      # Модели для View
│   │   │   ├── SignInViewParams.cs     # Параметры входа
│   │   │   ├── UserViewParams.cs       # Параметры пользователя
│   │   │   └── CodeViewParams.cs       # Параметры кода
│   │   └── Animation/                   # Анимации View
│   │       ├── ViewScaleAnimation.cs   # Масштабирование
│   │       ├── ViewFadeAnimation.cs    # Прозрачность
│   │       └── RectPositionAnimation.cs # Позиционирование
│   │
│   ├── Extensions/
│   │   └── WithLoading.cs              # Расширения для загрузки
│   │
│   └── Management/
│       └── IdentityUIManager.cs        # Менеджер UI
```

## 🔴 IdentityUIController - Монолитная архитектура

### Основные проблемы монолита:
- **891 строка кода** в одном файле
- **Смешанная ответственность** - UI + бизнес-логика + навигация
- **Сложное тестирование** - все в одном месте
- **Тесная связность** - изменение одной части влияет на все
- **Дублирование кода** - повторяющаяся логика

### Структура IdentityUIController:

```csharp
public class IdentityUIController : MonoBehaviour
{
    // === СОСТОЯНИЕ (50+ полей) ===
    public static IdentityUIController Instance { get; private set; }
    [SerializeField] private bool isLite = false;
    [SerializeField] protected bool debugLogging = false;
    
    protected IHttpClient httpClient;
    private IAuthProvider authProvider;
    protected IUserService userService;
    protected IIdentityService identityService;
    private IAnalyticsService analyticsService;
    private CancellationTokenSource lifetimeCts;
    
    // Canvas управление
    private IdentityCanvasController currentCanvasController;
    
    // Флаги состояния
    private bool isIdentityFlowStarted = false;
    private bool isShowingAuthFlow = false;
    private bool isShowingUserFlow = false;
    
    // События
    public event Action OnSignedOut;
    public event Action OnSignedIn;
    
    // === МЕТОДЫ (40+ методов) ===
    
    // Unity Lifecycle
    protected virtual void Awake() { /* 50 строк */ }
    void Start() { /* 10 строк */ }
    private void OnDestroy() { /* 100 строк */ }
    
    // Canvas Management  
    public void SetCanvasController(IdentityCanvasController canvasController) { /* 30 строк */ }
    public void ToggleOpenState() { /* 10 строк */ }
    public void SetOpenState(bool isOpen) { /* 20 строк */ }
    
    // Identity Flows
    private async UniTask StartIdentityFlow() { /* 80 строк */ }
    private async UniTask ShowAuthFlow(CancellationToken ct) { /* 200 строк */ }
    protected virtual async UniTask ShowUserFlow(CancellationToken ct) { /* 150 строк */ }
    protected async UniTask ShowSettings(CancellationToken ct) { /* 200 строк */ }
    
    // Event Handlers
    private void OnUserSignedIn(FirebaseUser user) { /* 30 строк */ }
    private void OnUserSignedOut() { /* 30 строк */ }
}
```

## 🌊 Legacy Flow Диаграммы

### 1. 🔑 Authentication Flow (ShowAuthFlow)

```mermaid
graph TD
    A[StartIdentityFlow] --> B{User Signed In?}
    B -->|No| C[ShowAuthFlow]
    
    C --> D[Show SignInView]
    D --> E{Authentication Method?}
    
    E -->|Email| F[Email Flow]
    E -->|Google| G[Google Flow]
    E -->|Apple| H[Apple Flow]
    E -->|Telegram| I[Telegram Flow]
    
    F --> F1[StartEmailFlow]
    F1 --> F2[Show CodeView]
    F2 --> F3[ConfirmEmailCode]
    F3 --> J[Authentication Success]
    
    G --> G1[SignInWithGoogle]
    G1 --> J
    
    H --> H1[SignInWithApple]
    H1 --> J
    
    I --> I1[SignInWithTelegram]
    I1 --> J
    
    J --> K[Fire OnSignedIn Event]
    K --> L[ShowUserFlow]
```

### 2. 👤 User Flow (ShowUserFlow)

```mermaid
graph TD
    A[ShowUserFlow] --> B[Get User Data]
    B --> C[Show UserView]
    C --> D{User Action?}
    
    D -->|Open Settings| E[ShowSettings]
    D -->|Sign Out| F[SignOut]
    
    E --> E1[Get User Data]
    E1 --> E2[Show SettingsView]
    E2 --> E3{Settings Action?}
    
    E3 -->|Change Name| G[ShowChangeName]
    E3 -->|Change Email| H[ShowChangeEmail]
    E3 -->|Delete Account| I[ShowDeleteAccount]
    E3 -->|Add Provider| J[Add Auth Provider]
    E3 -->|Close| K[Back to UserView]
    
    G --> G1[Show ChangeNameView]
    G1 --> G2[Update Name]
    G2 --> K
    
    H --> H1[Show ChangeEmailView]
    H1 --> H2[Request Email Change]
    H2 --> H3[Show Email Verification]
    H3 --> H4[Confirm Email Change]
    H4 --> K
    
    I --> I1[Show DeleteAccountView]
    I1 --> I2[Confirm Deletion]
    I2 --> I3[Delete Account]
    I3 --> F
    
    J --> J1[Add Google/Apple/Telegram]
    J1 --> K
    
    F --> F1[Fire OnSignedOut Event]
    F1 --> L[ShowAuthFlow]
```

### 3. 📧 Email Authentication Flow (Детальный)

```mermaid
sequenceDiagram
    participant U as User
    participant V as SignInView
    participant C as IdentityUIController
    participant S as IdentityService
    participant API as Backend API
    participant CV as CodeView
    
    U->>V: Enter Email
    V->>C: Email Submitted
    C->>S: StartEmailFlow(email)
    S->>API: POST /auth/email/start
    API-->>S: Code sent to email
    S-->>C: Flow started
    
    C->>CV: Show CodeView
    CV->>U: Show code input
    
    loop Code Entry
        U->>CV: Enter Code
        CV->>C: Code Submitted
        C->>S: ConfirmEmailCode(code)
        S->>API: POST /auth/email/confirm
        alt Valid Code
            API-->>S: Auth Success + Token
            S-->>C: AuthResult.Success
            C->>C: Fire OnSignedIn
            C->>C: ShowUserFlow()
        else Invalid Code
            API-->>S: Auth Failed
            S-->>C: AuthResult.Failed
            C->>CV: Show Error
            CV->>U: "Invalid code"
        else Resend Code
            U->>CV: Click Resend
            CV->>C: Resend Requested
            C->>S: StartEmailFlow(email)
            S->>API: POST /auth/email/start
        end
    end
```

### 4. ⚙️ Settings Flow (Детальный)

```mermaid
stateDiagram-v2
    [*] --> ShowSettings
    
    ShowSettings --> ChangeNameFlow : Change Name
    ShowSettings --> ChangeEmailFlow : Change Email  
    ShowSettings --> DeleteAccountFlow : Delete Account
    ShowSettings --> AddProviderFlow : Add Provider
    ShowSettings --> [*] : Close
    
    ChangeNameFlow --> ShowChangeName
    ShowChangeName --> ValidateName : Name Entered
    ValidateName --> UpdateName : Valid
    ValidateName --> ShowError : Invalid
    ShowError --> ShowChangeName
    UpdateName --> ShowSettings : Success
    
    ChangeEmailFlow --> ShowChangeEmail
    ShowChangeEmail --> RequestEmailChange : New Email
    RequestEmailChange --> ShowEmailVerification
    ShowEmailVerification --> ConfirmEmailChange : Code Entered
    ConfirmEmailChange --> ShowSettings : Success
    ConfirmEmailChange --> ShowEmailVerification : Invalid Code
    
    DeleteAccountFlow --> ShowDeleteAccount
    ShowDeleteAccount --> ConfirmDeletion : Confirm
    ConfirmDeletion --> DeleteAccount
    DeleteAccount --> SignOut : Success
    SignOut --> [*]
    
    AddProviderFlow --> AddGoogle : Google
    AddProviderFlow --> AddApple : Apple
    AddProviderFlow --> AddTelegram : Telegram
    AddGoogle --> ShowSettings : Success
    AddApple --> ShowSettings : Success
    AddTelegram --> ShowSettings : Success
```

## 🔧 Legacy ViewManager System

### ViewManager архитектура:
```csharp
public class ViewManager : MonoBehaviour
{
    // Показ View с параметрами и ожиданием результата
    public async UniTask<TResult> Show<TView, TParams, TResult>(TParams parameters, CancellationToken ct)
        where TView : BaseIdentityView<TParams, TResult>
    {
        // 1. Создание/поиск View
        var view = GetOrCreateView<TView>();
        
        // 2. Передача параметров
        view.SetParameters(parameters);
        
        // 3. Показ с анимацией
        await view.ShowAsync();
        
        // 4. Ожидание результата
        var result = await view.WaitForResult(ct);
        
        // 5. Скрытие с анимацией
        await view.HideAsync();
        
        return result;
    }
}
```

### Типичное использование ViewManager:
```csharp
// В IdentityUIController
var result = await viewManager.Show<SignInView, SignInViewParams, SignInViewResult>(
    new SignInViewParams(), ct);

switch (result.Method)
{
    case SignInMethod.Email:
        await identityService.StartEmailFlow(result.Email, ct);
        break;
    case SignInMethod.Google:
        await identityService.SignInWithGoogle(false, ct);
        break;
}
```

## 🎭 WithLoading Extension

Legacy система использует расширение для показа загрузки:

```csharp
public static class WithLoadingExtensions
{
    private static ViewManager _viewManager;
    
    public static async UniTask<T> WithLoading<T>(this UniTask<T> task, CancellationToken ct)
    {
        // Показываем LoadingView пока выполняется task
        return (T)(await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
            new ResultLoadingViewParams(task.AsObjectTask()), ct)).Result;
    }
    
    public static async UniTask WithLoading(this UniTask task, CancellationToken ct)
    {
        await _viewManager.Show<LoadingView, LoadingViewParams, LoadingViewResult>(
            new LoadingViewParams(task), ct);
    }
}

// Использование:
await identityService.StartEmailFlow(email, ct).WithLoading(ct);
await identityService.SignInWithGoogle(false, ct).WithLoading(ct);
```

## 🚨 Основные проблемы Legacy архитектуры

### 1. 🔴 Монолитный IdentityUIController:
- **891 строка кода** - слишком большой класс
- **Множественная ответственность** - UI + бизнес-логика + состояние
- **Сложное тестирование** - невозможно протестировать части отдельно
- **Тесная связность** - все компоненты знают друг о друге

### 2. 🔴 Сложная система состояний:
```csharp
// Флаги состояния разбросаны по всему классу
private bool isIdentityFlowStarted = false;
private bool isShowingAuthFlow = false;
private bool isShowingUserFlow = false;

// Проверки состояния в каждом методе
if (isShowingAuthFlow) {
    Debug.LogWarning("ShowAuthFlow already running");
    return;
}
```

### 3. 🔴 Дублирование кода:
```csharp
// Повторяющиеся проверки
if (this == null || !gameObject.activeInHierarchy) {
    if (debugLogging) Debug.Log("Controller destroyed");
    return;
}

// Повторяющиеся паттерны обработки ошибок
try {
    // какая-то логика
} catch (OperationCanceledException) {
    if (debugLogging) Debug.Log("Operation cancelled");
    return;
} catch (SignOutRequiredException) {
    await identityService.SignOut(ct);
    continue;
}
```

### 4. 🔴 Сложная система View:
- **Множественное наследование** - BaseIdentityView<TParams, TResult>
- **Генерики везде** - Show<TView, TParams, TResult>
- **Магические строки** - результаты как строки ("RESEND")
- **Тесная связь** с ViewManager

### 5. 🔴 Проблемы с lifecycle:
```csharp
private void OnDestroy()
{
    // 100+ строк очистки ресурсов
    // Множественные try-catch блоки
    // Проверки на null везде
    // Отписка от 10+ событий
    // Очистка CancellationTokenSource
    // Очистка WithLoadingExtensions
    // И многое другое...
}
```

## 📊 Метрики Legacy кода

### Сложность IdentityUIController:
- **891 строка** общего кода
- **50+ полей** состояния
- **40+ методов** разной ответственности
- **10+ событий** для обработки
- **Cyclomatic Complexity: ~85** (очень высокая)
- **Lines of Code per Method: ~20** (высокая)
- **Number of Dependencies: ~15** (высокая связность)

### Проблемные методы:
1. **ShowAuthFlow()** - 200 строк, множественная ответственность
2. **ShowUserFlow()** - 150 строк, сложная логика состояний
3. **ShowSettings()** - 200 строк, вложенные switch/case
4. **OnDestroy()** - 100 строк, множественная очистка ресурсов
5. **Awake()** - 50 строк, инициализация всего сразу

## 🆚 Сравнение Legacy vs MVP

| Аспект | Legacy (Монолит) | MVP (Новая архитектура) |
|--------|------------------|-------------------------|
| **Размер файлов** | 891 строка | 50-100 строк на компонент |
| **Ответственность** | Все в одном месте | Разделена по компонентам |
| **Тестируемость** | Сложно/невозможно | 100% покрытие |
| **Связность** | Тесная связь всего | Слабая связь через события |
| **Расширяемость** | Изменения затрагивают все | Изменения локализованы |
| **Понимание** | Нужно понимать весь код | Понимание по компонентам |
| **Отладка** | Сложно найти проблему | Проблемы локализованы |
| **Повторное использование** | Невозможно | Компоненты переиспользуются |

## 💡 Уроки из Legacy кода

### ❌ Что НЕ делать:
1. Создавать классы больше 200 строк
2. Смешивать UI логику с бизнес-логикой
3. Использовать глобальное состояние (static Instance)
4. Создавать методы больше 50 строк
5. Использовать множественную ответственность
6. Делать тесную связность между компонентами
7. Дублировать код обработки ошибок

### ✅ Что делать вместо этого:
1. Разбивать на специализированные компоненты
2. Использовать MVP/MVVM паттерны
3. Применять Dependency Injection
4. Создавать юнит-тесты
5. Использовать события для слабой связи
6. Применять Single Responsibility принцип
7. Создавать переиспользуемые утилиты

## 🎯 Заключение

Legacy архитектура IdentityUIController представляет собой классический пример **God Object** anti-pattern'а, где один класс пытается делать все. Это привело к:

- **Низкой поддерживаемости** - изменения сложны и рискованны
- **Отсутствию тестов** - невозможно протестировать части отдельно  
- **Высокой сложности** - новым разработчикам трудно разобраться
- **Дублированию кода** - одинаковая логика в разных местах
- **Тесной связности** - все зависит от всего

**Переход на MVP архитектуру устраняет все эти проблемы, создавая чистую, тестируемую и поддерживаемую систему.** 🚀
