# Energy8 Identity UI Module

## Описание
Модуль `io.energy8.identity/UI` — это масштабируемая, модульная система управления пользовательским интерфейсом для авторизации, профиля, обновлений и сервисных flow в Energy8. Построен на принципах чистой архитектуры, DI, разделения слоёв, высокой тестируемости и поддержки нескольких платформ.

---

## Структура

```
UI/
├── Core/         # Интерфейсы, абстракции, базовые типы, модели
│   ├── Management/   # IViewManager, ICanvasManager, ...
│   ├── Views/        # IView, ViewParams, ViewResult, ...
│   ├── State/        # IStateManager, IdentityState, ...
│   ├── Services/     # IIdentityService, IUpdateService, ...
│   └── ...
├── Runtime/      # Реализации, MonoBehaviour, бизнес-логика
│   ├── Management/   # ViewManager, CanvasManager, ...
│   ├── Views/        # ViewBase, Implementations, ...
│   ├── Controllers/  # IdentityOrchestrator, IdentityCanvasController, ...
│   ├── Flows/        # AuthFlowManager, UserFlowManager, ...
│   ├── Services/     # IdentityService, UpdateService, ...
│   ├── State/        # StateManager, ...
│   └── ...
├── Editor/       # Редакторские тулзы, инспекторы, генераторы
├── Tests/        # Unit/integration тесты для всех слоёв
└── docs/         # Документация, схемы, планы
```

---

## Архитектура и слои

- **Core/** — только интерфейсы, абстракции, модели. Нет зависимостей на UnityEngine.
- **Runtime/** — только реализации, MonoBehaviour, конкретные классы, flow, сервисы, UI.
- **Editor/** — редакторские скрипты, инспекторы, генераторы.
- **Tests/** — unit/integration тесты для всех слоёв.

### Главные слои:

- **Orchestrator** — главный координатор, управляет жизненным циклом UI и flow, интеграцией с сервисами.
- **CanvasManager / IdentityCanvasController** — управление Canvas, состоянием UI, анимациями, событиями открытия/закрытия.
- **ViewManager** — централизованный менеджер окон, поддерживает DI, тестируемость, управление жизненным циклом View.
- **FlowManager-ы** — отдельные потоки для авторизации, профиля, обновлений, аналитики, настроек.
- **StateManager** — централизованная state machine для UI flow, строгая валидация переходов.
- **Services** — сервисы для работы с Identity, Update, Analytics, платформенными адаптерами.

---

## Основные компоненты

### IdentityOrchestrator
- Singleton, живет весь lifecycle приложения.
- Координирует все flow, сервисы, UI.
- Управляет состоянием, событиями, DI.

### CanvasManager / IdentityCanvasController
- Управляет состоянием Canvas, открытием/закрытием UI.
- Реализует анимации, события, интеграцию с ViewManager.
- Позволяет кастомизировать визуальное поведение.

### ViewManager
- Централизованный менеджер окон.
- DI-friendly, легко тестируется.
- Управляет жизненным циклом View, презентерами, фабриками.

### FlowManager-ы
- AuthFlowManager — авторизация (email, google, apple, telegram).
- UserFlowManager — профиль, настройки, управление аккаунтом.
- UpdateFlowManager — обновления приложения.
- AnalyticsFlowManager — запрос разрешения на аналитику.
- Каждый flow — отдельный слой, легко расширять.

### StateManager
- Строгая state machine для UI flow.
- Валидация переходов, события, интеграция с flow.

### Services
- IdentityService — работа с авторизацией, токенами, пользователем.
- UpdateService — проверка и обработка обновлений.
- AnalyticsPermissionService — запрос разрешения на аналитику.
- Все сервисы реализуют интерфейсы из Core.

---

## Примеры интеграции

### Запуск Identity UI
```csharp
var orchestrator = FindObjectOfType<IdentityOrchestrator>();
orchestrator.SetOpenState(true); // Открыть UI
```

### Получение состояния UI
```csharp
if (orchestrator.IsOpen)
{
    // UI открыт, можно показывать пользовательские flow
}
```

### Работа с FlowManager
```csharp
var authFlowManager = serviceContainer.Resolve<IAuthFlowManager>();
await authFlowManager.StartAuthFlowAsync(CancellationToken.None);
```

### Получение ViewManager из CanvasManager
```csharp
var canvasManager = serviceContainer.Resolve<ICanvasManager>();
var viewManager = canvasManager.GetViewManager();
await viewManager.Show<SignInView, SignInViewParams, SignInViewResult>(new SignInViewParams(), ct);
```

---

## Flow и жизненный цикл

1. **Инициализация** — Orchestrator создает все сервисы, flow, менеджеры через DI.
2. **Переходы состояний** — StateManager управляет переходами между Uninitialized, Initializing, PreAuthentication, AuthCheck, SignedIn, SignedOut, UserFlowActive, Error.
3. **Flow** — каждый flow запускается только в нужном состоянии, flow-менеджеры не смешивают логику.
4. **UI** — CanvasManager и ViewManager управляют визуализацией, анимациями, окнами.
5. **События** — все ключевые события (OnSignedIn, OnSignedOut, OnOpenStateChanged) доступны для подписки.

---

## Best Practices

- **SRP/DI** — каждый компонент отвечает только за свою задачу, все зависимости внедряются через интерфейсы.
- **Тестируемость** — все ключевые слои покрыты unit/integration тестами, легко подменять реализации.
- **Масштабируемость** — легко добавлять новые flow, сервисы, UI-компоненты, платформенные адаптеры.
- **Документированность** — подробная документация, схемы, планы развития, примеры интеграции.
- **Платформенная независимость** — все платформенные зависимости инкапсулированы в сервисах.

---

## FAQ

**Q:** Как добавить новый flow?
**A:** Создайте интерфейс в Core/Flows, реализацию в Runtime/Flows, зарегистрируйте в DI-контейнере, интегрируйте в Orchestrator.

**Q:** Как протестировать UI?
**A:** Используйте mock-реализации интерфейсов из Core, запускайте unit/integration тесты из Tests/.

**Q:** Как расширить ViewManager?
**A:** Реализуйте новый интерфейс в Core/Management, добавьте реализацию в Runtime/Management, зарегистрируйте в DI.

**Q:** Как интегрировать с бизнес-логикой?
**A:** Используйте FlowManager-ы и сервисы через DI, подписывайтесь на события Orchestrator.

**Q:** Как добавить кастомный View?
**A:** Реализуйте IView в Core/Views, добавьте реализацию в Runtime/Views/Implementations, зарегистрируйте в ViewManager.

---

## Контакты и поддержка
- Вопросы, баги, предложения — через Issues или напрямую команде Energy8.
- Для pull request — следуйте архитектурным принципам и соглашениям из этого README.

---

**Вся документация, схемы, примеры — теперь только в этом README!**
