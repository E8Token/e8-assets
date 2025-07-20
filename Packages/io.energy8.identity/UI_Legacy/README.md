# UI Module

Модуль пользовательского интерфейса для системы Identity Energy8.

## Структура

### Core
- **Gradient/** - Компоненты градиентов для UI
- **ImageSpriteAnimation.cs** - Анимация спрайтов
- **OrientationController.cs** - Контроллер ориентации
- **TextButton.cs** - Кнопка с текстом

### Runtime
- **Controllers/** - Контроллеры UI
- **Extensions/** - Расширения для UI
- **Services/** - Сервисы UI
- **Views/** - Представления и модели данных
  - **Animation/** - Анимации представлений
  - **Base/** - Базовые классы представлений
  - **Implementations/** - Конкретные реализации представлений
  - **Management/** - Управление представлениями
  - **Models/** - Модели данных для представлений

### Editor
- Инструменты редактора для UI (пустая папка)

## ⚠️ Проблемы архитектуры

### 🔴 Критические проблемы

#### 1. Монолитный контроллер
- **`IdentityUIController.cs`** - 891 строка кода
- Нарушение принципа Single Responsibility
- Смешивание логики UI, аутентификации, навигации и lifecycle
- Сложность тестирования и поддержки

#### 2. Избыточные зависимости
```csharp
// IdentityUIController имеет прямые зависимости на ВСЕ модули:
"Energy8.Identity.Auth",
"Energy8.Identity.User", 
"Energy8.Identity.Game",
"Energy8.Identity.Analytics",
"Energy8.Identity.Configuration",
"Energy8.Identity.Http"
```

#### 3. Нарушение принципов SOLID
- **SRP**: Один класс управляет UI, авторизацией, навигацией, Canvas
- **OCP**: Сложно расширять без модификации основного кода
- **DIP**: Прямые зависимости на конкретные реализации

#### 4. Смешивание ответственностей
- UI логика + бизнес логика
- Управление состоянием + представление
- Навигация + аутентификация
- Lifecycle управление + пользовательские потоки

### 🟡 Средние проблемы

#### 5. Сложность тестирования
- Невозможно протестировать отдельные части
- Зависимость от Unity компонентов
- Асинхронная логика смешана с UI

#### 6. Жесткая связанность
- Views жестко привязаны к конкретным контроллерам
- Невозможно использовать UI компоненты отдельно
- Сложно заменить части системы

## 🔧 Предлагаемые решения

### 📋 План рефакторинга

#### Этап 1: Разделение ответственностей
```
UI/
├── Core/
│   ├── Interfaces/          # Интерфейсы для всех компонентов
│   ├── Models/             # Модели данных UI
│   └── Components/         # Базовые UI компоненты
├── Runtime/
│   ├── Navigation/         # Система навигации
│   ├── Presentation/       # Presenters и ViewModels
│   ├── Views/             # Только UI представления
│   ├── Controllers/       # Специализированные контроллеры
│   └── Services/          # UI сервисы
└── Editor/
    └── Tools/             # Инструменты разработки
```

#### Этап 2: Новая архитектура контроллеров

**Вместо одного монолитного `IdentityUIController`:**

```csharp
// 1. Координатор системы (легковесный)
public class IdentityUICoordinator : MonoBehaviour
{
    // Только инициализация и координация
}

// 2. Специализированные контроллеры
public class AuthFlowController         // Только логика авторизации
public class UserFlowController         // Только пользовательские потоки  
public class NavigationController       // Только навигация между экранами
public class CanvasLifecycleController  // Только управление Canvas
```

#### Этап 3: Внедрение паттернов

**MVP/MVVM для Views:**
```csharp
public interface ISignInView
{
    event Action<SignInData> OnSignInRequested;
    void ShowLoading(bool show);
    void ShowError(string error);
}

public class SignInPresenter
{
    private readonly ISignInView view;
    private readonly IAuthService authService;
    
    // Только презентационная логика
}
```

**Command Pattern для действий:**
```csharp
public interface IUICommand
{
    UniTask ExecuteAsync(CancellationToken ct);
}

public class SignInCommand : IUICommand
{
    // Инкапсулированная логика команды
}
```

**Factory Pattern для Views:**
```csharp
public interface IViewFactory
{
    TView CreateView<TView>() where TView : IView;
}
```

#### Этап 4: Dependency Injection

```csharp
public class UIServiceContainer
{
    public void RegisterServices()
    {
        Register<INavigationService, NavigationService>();
        Register<IViewFactory, ViewFactory>();
        Register<IAuthFlowService, AuthFlowService>();
    }
}
```

### 🎯 Целевая архитектура

#### Преимущества новой архитектуры:
- ✅ **Модульность**: Каждый компонент отвечает за одну задачу
- ✅ **Тестируемость**: Можно тестировать части отдельно
- ✅ **Расширяемость**: Легко добавлять новые Views и функции
- ✅ **Переиспользование**: UI компоненты можно использовать в других проектах
- ✅ **Поддержка**: Код легче понимать и модифицировать

#### Миграционная стратегия:
1. **Создать новые интерфейсы** (не ломая текущий код)
2. **Постепенно выносить логику** из монолитного контроллера  
3. **Создавать специализированные контроллеры** параллельно
4. **Переключить зависимости** на новую архитектуру
5. **Удалить старый монолитный код**

## Использование

### Текущий API (будет изменен)
Модуль предоставляет полнофункциональную систему UI для Identity с поддержкой анимаций, управления представлениями и responsive дизайна.

```csharp
var viewManager = new ViewManager();
var result = await viewManager.Show<UserView, UserViewParams, UserViewResult>(params, ct);
```

### Планируемый API (после рефакторинга)

```csharp
// Инициализация через DI контейнер
var coordinator = IdentityUICoordinator.Instance;
await coordinator.Initialize();

// Работа с отдельными контроллерами
var authController = coordinator.GetController<IAuthFlowController>();
await authController.StartAuthFlow();

// Навигация между экранами
var navigation = coordinator.GetService<INavigationService>();
await navigation.NavigateTo<UserProfileScreen>();
```

## 🔄 План миграции

### Приоритеты рефакторинга:
1. **Высокий**: Разделение `IdentityUIController` на специализированные контроллеры
2. **Высокий**: Извлечение бизнес-логики из UI слоя
3. **Средний**: Внедрение паттернов MVP/MVVM
4. **Средний**: Создание системы DI
5. **Низкий**: Оптимизация анимаций и UI компонентов

### Временные рамки:
- **Этап 1-2**: 2-3 недели разработки
- **Этап 3-4**: 3-4 недели разработки  
- **Миграция и тестирование**: 1-2 недели

### Критерии успеха:
- Размер основного контроллера < 200 строк
- Покрытие тестами > 80%
- Время сборки модуля сократится на 30%
- Возможность использования UI компонентов отдельно

```csharp
var viewManager = new ViewManager();
var result = await viewManager.Show<UserView, UserViewParams, UserViewResult>(params, ct);
```
