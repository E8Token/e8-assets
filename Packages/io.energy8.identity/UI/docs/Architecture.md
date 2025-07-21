
# Identity UI Architecture Documentation

> **Внимание!**
> Этот документ описывает новую архитектуру системы управления UI после рефакторинга 2024-2025 гг.
>
> - **Для миграции и архитектуры Views (MVP):** см. [Views-Plan.md](Views-Plan.md)
> - **Исторические проблемы и сравнение до/после:** см. [Problems.md](Problems.md)


## 🏗️ **Новая архитектура системы управления UI**

После рефакторинга God Object `IdentityUIController` (891 строка) был успешно разбит на **8 специализированных компонентов** с четкими обязанностями.

## 📊 **Обзор компонентов**

### **Главная диаграмма архитектуры:**
```
IdentityOrchestrator (Coordinator)
├── StateManager (State Machine)
├── CanvasManager (UI Control) 
├── AuthFlowManager (Authentication Flows)
├── UserFlowManager (User Management Flows)
├── ErrorHandler (Error Management)
└── ServiceContainer (Dependency Injection)
```

---

## 🎯 **1. IdentityOrchestrator** 
**📁 Файл:** `Controllers/IdentityOrchestrator.cs` (120 строк)

### **Назначение:**
Главный координатор системы. Singleton который связывает все компоненты и предоставляет публичный API.

### **Обязанности:**
- **Singleton Pattern** - единая точка доступа к Identity системе
- **Dependency Injection** - создание и связывание всех компонентов
- **Public API** - предоставление методов для внешнего использования
- **Lifecycle Management** - управление жизненным циклом системы

### **Публичный API:**
```csharp
public static IdentityOrchestrator Instance { get; }
public bool IsSignedIn { get; }
public bool IsOpen { get; }
public FirebaseUser CurrentUser { get; }

// Управление UI
public void SetOpenState(bool isOpen)
public void ToggleOpenState()
public void SetCanvasController(IdentityCanvasController controller)

// События
public event Action<FirebaseUser> OnSignedIn
public event Action OnSignedOut
```

### **Внутренние зависимости:**
```csharp
private IStateManager stateManager;
private ICanvasManager canvasManager;
private IAuthFlowManager authFlowManager;
private IUserFlowManager userFlowManager;
private IErrorHandler errorHandler;
private IServiceContainer serviceContainer;
```

---

## 🔄 **2. StateManager**
**📁 Файлы:** 
- `State/IStateManager.cs` (интерфейс)
- `State/StateManager.cs` (120 строк)
- `State/IdentityState.cs` (enum)

### **Назначение:**
Управляет состоянием всей Identity системы с помощью State Machine pattern.

### **Состояния системы:**
```csharp
public enum IdentityState
{
    NotInitialized,           // Система не инициализирована
    Initializing,             // Процесс инициализации
    InitializationFailed,     // Ошибка инициализации
    AuthenticationInProgress, // Идет процесс авторизации
    Authenticated,            // Пользователь авторизован
    UserFlowInProgress,       // Показ пользовательского интерфейса
    SigningOut,               // Процесс выхода
    SignedOut                 // Пользователь вышел
}
```

### **API:**
```csharp
IdentityState CurrentState { get; }
bool CanTransitionTo(IdentityState newState)
void TransitionTo(IdentityState newState)
event Action<IdentityState, IdentityState> OnStateChanged
```

### **Валидация переходов:**
- Строгие правила перехода между состояниями
- Предотвращение некорректных состояний
- Логирование всех переходов для отладки

---

## 🖼️ **3. CanvasManager**
**📁 Файлы:**
- `Canvas/ICanvasManager.cs` (интерфейс)
- `Canvas/CanvasManager.cs` (100 строк)

### **Назначение:**
Управляет UI Canvas и ViewManager. Отвечает за показ/скрытие UI и делегирование команд ViewManager.

### **Обязанности:**
- **Canvas Control** - управление открытием/закрытием UI
- **ViewManager Integration** - связь с системой View
- **State Synchronization** - синхронизация состояния UI

### **API:**
```csharp
bool IsOpen { get; }
void SetCanvasController(IdentityCanvasController controller)
void SetOpenState(bool isOpen)
void ToggleOpenState()
ViewManager GetViewManager()
event Action<bool> OnOpenStateChanged
```

### **Интеграция:**
- Подключается к `IdentityCanvasController` в сцене
- Управляет анимациями открытия/закрытия
- Делегирует показ View через ViewManager

---

## 🔐 **4. AuthFlowManager**
**📁 Файлы:**
- `Flows/IAuthFlowManager.cs` (интерфейс)
- `Flows/AuthFlowManager.cs` (280 строк)

### **Назначение:**
Управляет всеми потоками авторизации: Email, Google, Apple, Telegram.

### **Поддерживаемые методы авторизации:**
- **Email** - авторизация по email с кодом подтверждения
- **Google** - OAuth через Google
- **Apple** - Sign In with Apple  
- **Telegram** - Telegram авторизация

### **Основные потоки:**
```csharp
public async UniTask StartAuthFlowAsync(CancellationToken ct)

// Внутренние методы для каждого типа авторизации
private async UniTask HandleEmailAuth(string email, CancellationToken ct)
private async UniTask HandleGoogleAuth(CancellationToken ct)
private async UniTask HandleAppleAuth(CancellationToken ct)  
private async UniTask HandleTelegramAuth(CancellationToken ct)
```

### **Интеграция с LoadingView:**
- Автоматически показывает LoadingView во время async операций
- Управляет показом спиннера загрузки
- Корректно закрывает Loading при завершении/ошибке

### **Error Handling:**
- Интеграция с ErrorHandler для показа ошибок
- Обработка специфичных исключений авторизации
- Graceful fallback при ошибках

---

## 👤 **5. UserFlowManager**  
**📁 Файлы:**
- `Flows/IUserFlowManager.cs` (интерфейс)
- `Flows/UserFlowManager.cs` (350 строк)

### **Назначение:**
Управляет всеми потоками авторизованного пользователя: профиль, настройки, управление аккаунтом.

### **Основные потоки:**
- **User Profile** - показ информации о пользователе
- **Settings** - настройки аккаунта
- **Change Name** - изменение имени
- **Change Email** - изменение email с подтверждением
- **Delete Account** - удаление аккаунта с подтверждением
- **Add Providers** - привязка дополнительных способов авторизации

### **API:**
```csharp
public async UniTask StartUserFlowAsync(CancellationToken ct)
public async UniTask ShowSettingsAsync(CancellationToken ct)

// Специфичные операции
private async UniTask ShowChangeNameAsync(CancellationToken ct)
private async UniTask ShowChangeEmailAsync(CancellationToken ct)
private async UniTask ShowDeleteAccountAsync(CancellationToken ct)
private async UniTask AddProviderAsync(SignInMethod method, CancellationToken ct)
```

### **Интеграция:**
- Работает с `IUserService` для API вызовов
- Использует LoadingView для длительных операций
- Интегрируется с ErrorHandler для обработки ошибок

---

## ⚠️ **6. ErrorHandler**
**📁 Файлы:**
- `Error/IErrorHandler.cs` (интерфейс)
- `Error/ErrorHandler.cs` (60 строк)

### **Назначение:**
Централизованная обработка всех ошибок в Identity системе.

### **Типы обработки:**
```csharp
public enum ErrorHandlingMethod
{
    Continue,    // Продолжить выполнение
    Retry,       // Повторить операцию
    Cancel       // Отменить выполнение
}
```

### **API:**
```csharp
public async UniTask<ErrorHandlingMethod> ShowErrorAsync(
    Energy8Exception exception, 
    CancellationToken ct)
```

### **Возможности:**
- Показ ErrorView с детализированным сообщением
- Логирование ошибок для отладки
- Возврат стратегии обработки ошибки
- Интеграция со всеми Flow Managers

---

## 🏭 **7. ServiceContainer**
**📁 Файлы:**
- `DI/IServiceContainer.cs` (интерфейс)
- `DI/IdentityServiceContainer.cs` (100 строк)

### **Назначение:**
Dependency Injection container для всех сервисов Identity системы.

### **Регистрируемые сервисы:**
```csharp
// Core Services
IIdentityService - авторизация и управление пользователем
IUserService - API для пользовательских операций  
IHttpClient - HTTP клиент с авторизацией
IAuthProvider - провайдер авторизации (Firebase/WebGL)
IAnalyticsService - аналитика и метрики

// Configuration
ConfigurationManager - настройки системы
```

### **API:**
```csharp
void RegisterServices()
T GetService<T>() where T : class
void Dispose()
```

### **Особенности:**
- Singleton management для всех сервисов
- Ленивая инициализация
- Автоматический dispose при уничтожении
- Конфигурация через isLite флаг

---

## 🎨 **8. IdentityCanvasController**
**📁 Файл:** `Controllers/IdentityCanvasController.cs` (160 строк)

### **Назначение:**
MonoBehaviour компонент для управления UI Canvas в Unity сцене.

### **Обязанности:**
- **UI Lifecycle** - управление жизненным циклом Canvas
- **Animation System** - анимации открытия/закрытия
- **ViewManager Integration** - связь с ViewManager
- **Auto-Registration** - автоматическая регистрация в Orchestrator

### **API:**
```csharp
bool IsOpen { get; }
ViewManager ViewManager { get; }
Canvas Canvas { get; }

void SetOpenState(bool isOpen)
ViewManager GetViewManager()
void SetCanvasEnabled(bool enabled)
void SetActive(bool active)

event Action<bool> OnOpenStateChanged
```

### **Анимационная система:**
- Плавные анимации slide in/out
- Настраиваемые AnimationCurve
- Настраиваемая длительность
- Остановка предыдущих анимаций

---

## 📋 **Сравнение: ДО и ПОСЛЕ**

### **❌ ДО (God Object):**
```
IdentityUIController.cs: 891 строка
├── Singleton + DI + State + Canvas + Auth + User + Settings + Errors
├── 7 зависимостей в конструкторе
├── 25+ методов разной ответственности  
├── 3 булевых флага состояния
├── Невозможно тестировать изолированно
└── Нарушение всех SOLID принципов
```

### **✅ ПОСЛЕ (Specialized Components):**
```
8 специализированных компонентов:
├── IdentityOrchestrator: 120 строк (Coordination)
├── StateManager: 120 строк (State Machine)  
├── CanvasManager: 100 строк (UI Control)
├── AuthFlowManager: 280 строк (Authentication)
├── UserFlowManager: 350 строк (User Management)
├── ErrorHandler: 60 строк (Error Handling)
├── ServiceContainer: 100 строк (Dependency Injection)
└── CanvasController: 160 строк (Unity Integration)
```

## 🎯 **Достоинства новой архитектуры**

### ✅ **SOLID Compliance**
- **Single Responsibility**: каждый компонент имеет одну четкую обязанность
- **Open/Closed**: легко расширять без изменения существующего кода
- **Liskov Substitution**: все компоненты работают через интерфейсы
- **Interface Segregation**: интерфейсы сфокусированы на конкретных задачах
- **Dependency Inversion**: зависимости инжектируются через DI

### ✅ **Тестируемость**
- Каждый компонент может тестироваться изолированно
- Все зависимости инжектируются через интерфейсы
- Легко создавать mock-объекты для unit тестов
- Четкие границы между компонентами

### ✅ **Поддерживаемость**
- Четкое разделение ответственности
- Изменения в одном компоненте не затрагивают другие
- Простая навигация по коду
- Понятная структура для новых разработчиков

### ✅ **Расширяемость**
- Легко добавлять новые типы авторизации
- Простое расширение пользовательских потоков
- Возможность добавления новых состояний
- Модульная архитектура

### ✅ **Backward Compatibility**
- Весь существующий код продолжает работать
- Плавная миграция без breaking changes
- Старый API помечен как deprecated

## 🚦 **Состояние системы**

- **🟢 Готово**: Все 8 компонентов реализованы и интегрированы
- **🟡 В разработке**: LoadingView интеграция и отладка
- **🔴 Требует внимания**: Unit тесты и документация

## 🎯 **Следующие этапы**

### **� Запланированные улучшения:**
1. **MVP Migration** - миграция Views на MVP паттерн (см. [Views-Plan.md](Views-Plan.md))
2. **LoadingManager** - выделение LoadingView логики в отдельный компонент
3. **UserFlowManager разбиение** - дальнейшее разделение на ProfileManager, SettingsManager, AccountManager
4. **Unit Testing** - создание комплексных тестов для всех компонентов

## �📚 **Дополнительная документация**

- [Проблемы старой архитектуры](Problems.md)
- [План миграции Views на MVP](Views-Plan.md)
- [Руководство по миграции](Migration-Guide.md) *(планируется)*
- [Unit Testing Guide](Testing-Guide.md) *(планируется)*
- [API Reference](API-Reference.md) *(планируется)*
