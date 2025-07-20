# Views Migration to MVP Pattern

## 🎯 **Цель миграции**

Выполнить миграцию всех UI компонентов на **MVP (Model-View-Presenter) паттерн** с полным сохранением существующих UI компонентов, но вынесением бизнес-логики в отдельные Presenter'ы.

## 📊 **Текущее состояние Views**

### **Проблемы существующей архитектуры:**

❌ **Смешение ответственности в Views:**
```csharp
// SignInView.cs - ПЛОХО: View содержит бизнес-логику
public class SignInView : ViewBase<SignInViewParams, SignInViewResult>
{
    private void OnGoogleButtonClick()
    {
        // ❌ Бизнес-логика в View
        var authResult = await IdentityService.SignInWithGoogle();
        HandleAuthResult(authResult);
    }
}
```

❌ **Views знают о сервисах:**
- Views напрямую обращаются к `IIdentityService`
- Жесткая связанность с бизнес-логикой
- Невозможно тестировать UI отдельно

❌ **Дублирование логики:**
- Одинаковая обработка ошибок в разных Views
- Повторяющаяся валидация
- Дублированные паттерны работы с async операциями

## 🏗️ **MVP Architecture Plan**

### **Архитектурная диаграмма:**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Model       │    │   Presenter     │    │      View       │
│                 │    │                 │    │                 │
│ - Data          │◄───┤ - Business      │◄───┤ - UI Elements   │
│ - Validation    │    │   Logic         │    │ - User Events   │
│ - State         │    │ - Coordination  │    │ - Rendering     │
│                 │───►│ - Error         │───►│ - Navigation    │
└─────────────────┘    │   Handling      │    └─────────────────┘
                       └─────────────────┘
```

### **Принципы MVP:**
1. **View** - только UI и пользовательские события
2. **Presenter** - вся бизнес-логика и координация
3. **Model** - данные и валидация
4. **View не знает о Model** - вся связь через Presenter

## 📋 **План миграции компонентов**

### **🎯 Этап 1: SignInView → SignInMVP**

#### **Текущий SignInView:**
```csharp
// ТЕКУЩЕЕ СОСТОЯНИЕ
public class SignInView : ViewBase<SignInViewParams, SignInViewResult>
{
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private InputField emailField;
    
    // ❌ Бизнес-логика в View
    private async void OnGoogleClick()
    {
        try 
        {
            var result = await identityService.SignInWithGoogle();
            completionSource.TrySetResult(new SignInViewResult(SignInMethod.Google));
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }
}
```

#### **Новая архитектура:**

**📱 SignInView (только UI):**
```csharp
public class SignInView : ViewBase<SignInViewParams, SignInViewResult>, ISignInView
{
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private InputField emailField;
    
    private ISignInPresenter presenter;
    
    public override void Initialize(SignInViewParams @params)
    {
        base.Initialize(@params);
        
        // Создаем Presenter и связываем с View
        presenter = new SignInPresenter(this, ServiceLocator.Get<IIdentityService>());
        
        // Только подписка на UI события
        googleButton.onClick.AddListener(() => presenter.OnGoogleSignInRequested());
        appleButton.onClick.AddListener(() => presenter.OnAppleSignInRequested());
        emailField.onEndEdit.AddListener(email => presenter.OnEmailSignInRequested(email));
    }
    
    // Интерфейс для Presenter'а
    public void ShowLoading(bool show) { /* UI логика */ }
    public void ShowError(string message) { /* UI логика */ }
    public void SetEmailValid(bool valid) { /* UI логика */ }
    public void CompleteWithResult(SignInViewResult result) 
    {
        completionSource.TrySetResult(result);
    }
}
```

**🧠 ISignInPresenter (интерфейс):**
```csharp
public interface ISignInPresenter
{
    void OnGoogleSignInRequested();
    void OnAppleSignInRequested();
    void OnEmailSignInRequested(string email);
    void OnTelegramSignInRequested();
}
```

**🧠 SignInPresenter (бизнес-логика):**
```csharp
public class SignInPresenter : ISignInPresenter
{
    private readonly ISignInView view;
    private readonly IIdentityService identityService;
    private readonly IAnalyticsService analyticsService;
    
    public SignInPresenter(ISignInView view, IIdentityService identityService)
    {
        this.view = view;
        this.identityService = identityService;
    }
    
    public async void OnGoogleSignInRequested()
    {
        view.ShowLoading(true);
        
        try
        {
            // Вся бизнес-логика здесь
            analyticsService.LogEvent("signin_google_started");
            
            var result = await identityService.SignInWithGoogle(false, CancellationToken.None);
            
            analyticsService.LogEvent("signin_google_success");
            view.CompleteWithResult(new SignInViewResult(SignInMethod.Google));
        }
        catch (Exception ex)
        {
            analyticsService.LogEvent("signin_google_failed", new { error = ex.Message });
            view.ShowError(GetUserFriendlyError(ex));
        }
        finally
        {
            view.ShowLoading(false);
        }
    }
    
    private string GetUserFriendlyError(Exception ex)
    {
        // Централизованная обработка ошибок
        return ex switch
        {
            NetworkException => "Проблемы с интернетом. Попробуйте еще раз.",
            AuthenticationException => "Ошибка авторизации. Проверьте данные.",
            _ => "Произошла ошибка. Попробуйте позже."
        };
    }
}
```

**📊 ISignInView (интерфейс View):**
```csharp
public interface ISignInView
{
    void ShowLoading(bool show);
    void ShowError(string message);
    void SetEmailValid(bool valid);
    void SetEmailErrorText(string error);
    void CompleteWithResult(SignInViewResult result);
}
```

### **🎯 Этап 2: UserView → UserMVP**

**📱 UserView (только UI):**
```csharp
public class UserView : ViewBase<UserViewParams, UserViewResult>, IUserView
{
    [SerializeField] private Text userNameText;
    [SerializeField] private Text userEmailText;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button signOutButton;
    
    private IUserPresenter presenter;
    
    public override void Initialize(UserViewParams @params)
    {
        base.Initialize(@params);
        presenter = new UserPresenter(this, ServiceLocator.Get<IUserService>());
        
        settingsButton.onClick.AddListener(() => presenter.OnSettingsRequested());
        signOutButton.onClick.AddListener(() => presenter.OnSignOutRequested());
        
        presenter.LoadUserData();
    }
    
    // Интерфейс для Presenter'а
    public void DisplayUserName(string name) => userNameText.text = name;
    public void DisplayUserEmail(string email) => userEmailText.text = email;
    public void ShowLoading(bool show) { /* UI логика */ }
    public void NavigateToSettings() 
    {
        completionSource.TrySetResult(new UserViewResult(UserAction.Settings));
    }
}
```

**🧠 UserPresenter (бизнес-логика):**
```csharp
public class UserPresenter : IUserPresenter
{
    private readonly IUserView view;
    private readonly IUserService userService;
    
    public async void LoadUserData()
    {
        view.ShowLoading(true);
        
        try
        {
            var user = await userService.GetCurrentUserAsync();
            view.DisplayUserName(user.DisplayName ?? "Пользователь");
            view.DisplayUserEmail(user.Email);
        }
        catch (Exception ex)
        {
            view.ShowError("Не удалось загрузить данные пользователя");
        }
        finally
        {
            view.ShowLoading(false);
        }
    }
    
    public void OnSettingsRequested()
    {
        view.NavigateToSettings();
    }
}
```

## 📋 **Полный план миграции**

### **📅 Фаза 1 (1-2 недели): Core Views**
1. **SignInView** → SignInMVP
2. **UserView** → UserMVP  
3. **CodeView** → CodeMVP
4. **ErrorView** → ErrorMVP

### **📅 Фаза 2 (1-2 недели): Settings Views**  
5. **SettingsView** → SettingsMVP
6. **ChangeNameView** → ChangeNameMVP
7. **ChangeEmailView** → ChangeEmailMVP
8. **DeleteAccountView** → DeleteAccountMVP

### **📅 Фаза 3 (1 неделя): Utility Views**
9. **LoadingView** → LoadingMVP
10. **AnalyticsView** → AnalyticsMVP

## 🏭 **Общая архитектура Presenters**

### **Base Presenter:**
```csharp
public abstract class BasePresenter<TView> where TView : class
{
    protected readonly TView view;
    protected readonly IServiceContainer services;
    
    protected BasePresenter(TView view, IServiceContainer services)
    {
        this.view = view;
        this.services = services;
    }
    
    protected virtual void HandleError(Exception ex)
    {
        var userMessage = ErrorMessageResolver.GetUserFriendlyMessage(ex);
        view.ShowError(userMessage);
        
        services.Get<IAnalyticsService>()?.LogError(ex);
    }
}
```

### **Factory для Presenters:**
```csharp
public interface IPresenterFactory
{
    TPresenter Create<TPresenter, TView>(TView view) 
        where TPresenter : class 
        where TView : class;
}

public class PresenterFactory : IPresenterFactory
{
    private readonly IServiceContainer services;
    
    public TPresenter Create<TPresenter, TView>(TView view) 
    {
        // Автоматическое создание Presenter'ов с инжекцией зависимостей
        return (TPresenter)Activator.CreateInstance(typeof(TPresenter), view, services);
    }
}
```

## ✅ **Преимущества MVP миграции**

### **🎯 Разделение ответственности:**
- **View**: только UI и события
- **Presenter**: только бизнес-логика
- **Model**: только данные

### **🧪 Тестируемость:**
```csharp
[Test]
public void SignInPresenter_OnGoogleSignIn_CallsIdentityService()
{
    // Arrange
    var mockView = new Mock<ISignInView>();
    var mockService = new Mock<IIdentityService>();
    var presenter = new SignInPresenter(mockView.Object, mockService.Object);
    
    // Act
    presenter.OnGoogleSignInRequested();
    
    // Assert
    mockService.Verify(s => s.SignInWithGoogle(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    mockView.Verify(v => v.ShowLoading(true), Times.Once);
}
```

### **🔄 Переиспользование:**
- Presenters можно переиспользовать с разными Views
- Общая бизнес-логика в базовых классах
- Легкое создание новых UI для существующей логики

### **🛠 Поддерживаемость:**
- Изменения в UI не затрагивают бизнес-логику
- Изменения в API не затрагивают UI
- Четкие границы между слоями

## 🚦 **Критерии готовности**

### **✅ Для каждого View:**
- [ ] Создан интерфейс IView
- [ ] Создан интерфейс IPresenter  
- [ ] Реализован Presenter с бизнес-логикой
- [ ] View содержит только UI код
- [ ] Написаны unit тесты для Presenter
- [ ] Обновлена документация

### **✅ Общие критерии:**
- [ ] Все Views используют MVP паттерн
- [ ] Создана PresenterFactory
- [ ] Обновлены ViewManager и ViewFactory
- [ ] Написаны интеграционные тесты
- [ ] Производительность не ухудшилась

## 📚 **Документация**

После завершения миграции будет создана:
- **MVP-Architecture.md** - подробное описание новой архитектуры
- **Presenter-Testing-Guide.md** - руководство по тестированию
- **View-Creation-Guide.md** - создание новых Views в MVP
- **Migration-Results.md** - результаты и метрики миграции

---

**Эта миграция на MVP паттерн станет финальным штрихом в создании чистой, тестируемой и поддерживаемой архитектуры Identity UI!** 🎯
