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
- Инструменты редактора для UI

## Использование

Модуль предоставляет полнофункциональную систему UI для Identity с поддержкой анимаций, управления представлениями и responsive дизайна.

```csharp
var viewManager = new ViewManager();
var result = await viewManager.Show<UserView, UserViewParams, UserViewResult>(params, ct);
```
