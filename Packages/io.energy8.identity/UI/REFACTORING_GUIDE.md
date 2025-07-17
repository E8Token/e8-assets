# Identity UI Refactoring Guide

## Обзор изменений

Была проведена реструктуризация Identity UI системы для отделения `IdentityUIController` от Canvas и создания единого экземпляра на протяжении всего lifecycle приложения.

## Новая архитектура

### 1. IdentityUIController
- **Теперь**: Singleton, живет на протяжении всего lifecycle приложения
- **Ответственность**: Управление логикой аутентификации и пользовательскими потоками
- **Не содержит**: Canvas, UI компоненты, анимации
- **Местоположение**: `Packages/io.energy8.identity/UI/Runtime/Controllers/IdentityUIController.cs`

### 2. IdentityCanvasController (новый)
- **Назначение**: Управление Canvas и UI представлением
- **Ответственность**: Анимации, UI состояния, ViewManager
- **Содержит**: Canvas, Button, ViewManager, анимации
- **Местоположение**: `Packages/io.energy8.identity/UI/Runtime/Controllers/IdentityCanvasController.cs`

### 3. IdentityViewportManager (обновлен)
- **Теперь**: Управляет Canvas контроллерами вместо UI контроллеров
- **Создает**: IdentityCanvasController для разных ориентаций
- **Подключает**: Canvas контроллеры к единому IdentityUIController
- **Местоположение**: `Packages/io.energy8.identity/UI/Runtime/Controllers/IdentityViewportManager.cs`

### 4. IdentityUIManager (новый)
- **Назначение**: Инициализация единственного экземпляра IdentityUIController
- **Использование**: Размещается на сцене для автоматической инициализации
- **Местоположение**: `Packages/io.energy8.identity/UI/Runtime/Management/IdentityUIManager.cs`

## Миграция

### Префабы
1. **Старые префабы** с `IdentityUIController` нужно переделать
2. **Новые префабы** должны содержать `IdentityCanvasController`
3. **ViewportManager** теперь работает с `portraitCanvasPrefab` и `landscapeCanvasPrefab`

### Настройка сцены
1. Добавьте `IdentityUIManager` на сцену для автоматической инициализации
2. Настройте `IdentityViewportManager` с новыми Canvas префабами
3. Убедитесь, что префабы содержат `IdentityCanvasController` вместо `IdentityUIController`

### Использование в коде
```csharp
// Получение единственного экземпляра (как и раньше)
var identityController = IdentityUIController.Instance;

// Получение текущего Canvas контроллера
var canvasController = identityController.CurrentCanvasController;

// Управление состоянием UI (API не изменился)
identityController.SetOpenState(true);
identityController.ToggleOpenState();
```

## Преимущества новой архитектуры

1. **Единый экземпляр**: IdentityUIController существует один раз на протяжении всего приложения
2. **Отделение логики от UI**: Бизнес-логика отделена от визуального представления
3. **Гибкость**: Canvas можно менять в зависимости от ориентации, не теряя состояние
4. **Сохранение состояния**: Состояние аутентификации сохраняется при смене ориентации
5. **Простота управления**: ViewportManager автоматически управляет Canvas контроллерами

## Обратная совместимость

- **API IdentityUIController**: Остался прежним для внешнего использования
- **События и методы**: Сохранены все публичные методы и события
- **GameIdentityUIController**: Автоматически работает с новой архитектурой

## Отладка

### Debug методы:
```csharp
// В IdentityUIController
[ContextMenu("Debug State")]
private void DebugState()

// В IdentityViewportManager  
[ContextMenu("Debug State Info")]
private void DebugStateInfo()

// В IdentityUIManager
[ContextMenu("Debug Info")]
private void EditorDebugInfo()
```

### Логирование:
Установите `debugLogging = true` для детального логирования работы системы.

## Проблемы и решения

### Если IdentityUIController.Instance == null:
1. Проверьте, что `IdentityUIManager` присутствует на сцене
2. Убедитесь, что `autoInitialize = true` в IdentityUIManager
3. Вызовите `IdentityUIManager.InitializeIdentityUIController()` вручную

### Если ViewManager недоступен:
1. Проверьте, что Canvas префабы содержат `IdentityCanvasController`
2. Убедитесь, что `IdentityViewportManager` правильно настроен
3. Проверьте, что Canvas контроллер подключен к IdentityUIController

### При смене ориентации:
1. Убедитесь, что `preserveState = true` в IdentityViewportManager
2. Проверьте корректность префабов для разных ориентаций
3. Используйте debug методы для отслеживания состояния
