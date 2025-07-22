# NeonFruits Game Service

Кастомный игровой сервис для слота NeonFruits, построенный на базе Energy8 Identity архитектуры.

## 📁 Структура проекта

```
Assets/Scripts/Game/
├── Dto/
│   └── NeonFruitsDto.cs          # DTO классы для API запросов/ответов
├── Services/
│   ├── INeonFruitsGameService.cs # Интерфейс кастомного сервиса
│   └── NeonFruitsGameService.cs  # Реализация сервиса
├── Factory/
│   └── NeonFruitsGameServiceFactory.cs # Фабрика для создания сервисов
├── Management/
│   └── NeonFruitsFlowManager.cs  # Интеграция с UI потоками
├── Controllers/
│   └── NeonFruitsGameController.cs # Основной игровой контроллер
└── Examples/
    └── SimpleNeonFruitsExample.cs # Простой пример использования
```

## 🚀 Быстрый старт

### 1. Создание игрового сервиса

```csharp
using Game.Factory;
using Game.Services;

// Простое создание
var gameService = NeonFruitsGameServiceFactory.CreateService("neon-fruits");

// Создание с кастомным HttpClient
var gameService = NeonFruitsGameServiceFactory.CreateService(httpClient, "neon-fruits");

// Создание тестового сервиса
var testService = NeonFruitsGameServiceFactory.CreateTestService();
```

### 2. Базовое использование

```csharp
// Получение данных пользователя
var user = await gameService.GetUserAsync(cancellationToken);

// Создание игровой сессии
var session = await gameService.CreateSessionsAsync(cancellationToken);

// Инициализация игры
var status = await gameService.InitializeGameAsync(session.SessionId, cancellationToken);

// Выполнение спина
var spinRequest = new NeonFruitsSpinRequestDto(session.SessionId, 100, 20);
var result = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(
    spinRequest, cancellationToken);

// Активация бонуса
var bonusResult = await gameService.ActivateBonusAsync(session.SessionId, cancellationToken);
```

### 3. Интеграция с UI (через FlowManager)

```csharp
using Game.Management;

// Создание интеграционного менеджера
var flowManager = new NeonFruitsFlowManager(userFlowManager, gameService, enableLogging: true);

// Все операции с Loading UI
var session = await flowManager.CreateGameSessionAsync(ct);
var status = await flowManager.InitializeGameWithUIAsync(session.SessionId, ct);
var spinResult = await flowManager.SpinWithUIAsync(spinRequest, ct);
```

## 📋 API Методы

### IGameService (базовые методы)

- `GetUserAsync(ct)` - получить данные игрового пользователя
- `CreateSessionsAsync(ct)` - создать игровую сессию

### INeonFruitsGameService (расширенные методы)

- `InitializeGameAsync(sessionId, ct)` - инициализировать игру
- `SpinAsync<TRequest, TResponse>(request, ct)` - выполнить спин
- `ActivateBonusAsync(sessionId, ct)` - активировать бонус
- `GetGameStatusAsync(sessionId, ct)` - получить статус игры
- `EndGameSessionAsync(sessionId, ct)` - завершить сессию

## 📊 DTO Классы

### Запросы

- `GameInitializeRequestDto` - инициализация игры
- `SpinRequestDto` - базовый спин
- `NeonFruitsSpinRequestDto` - расширенный спин с AutoPlay

### Ответы

- `NFGameStatusResponseDto` - статус игры NeonFruits
- `GameStatusResponseDto` - базовый статус
- `NeonFruitsSpinResponseDto` - результат спина с деталями барабанов

## 🎮 Использование в Unity

### В Inspector (NeonFruitsGameController)

1. Добавьте `NeonFruitsGameController` на GameObject
2. Настройте параметры в Inspector:
   - `Game Endpoint` - эндпоинт API
   - `Default Bet Amount` - размер ставки по умолчанию
   - `Default Lines` - количество линий
   - `Enable Debug Logging` - включить логирование

3. Вызывайте публичные методы:
   - `StartNewGame()` - начать новую игру
   - `Spin()` - выполнить спин
   - `ActivateBonus()` - активировать бонус
   - `EndGame()` - завершить игру

### Программно

```csharp
// Получение контроллера
var controller = FindObjectOfType<NeonFruitsGameController>();

// Запуск полного игрового цикла
await controller.StartNewGameAsync(cancellationToken);
var spinResult = await controller.SpinAsync(betAmount: 100, lines: 20, cancellationToken);
await controller.ActivateBonusAsync(cancellationToken);
await controller.EndGameAsync(cancellationToken);
```

## 🔧 Кастомизация

### Создание собственных DTO

```csharp
[System.Serializable]
public class MyCustomSpinRequestDto : SpinRequestDto
{
    public bool TurboMode { get; set; }
    public string SpecialFeature { get; set; }
}
```

### Расширение сервиса

```csharp
public class ExtendedNeonFruitsService : NeonFruitsGameService
{
    public async UniTask<CustomResult> MyCustomMethodAsync(CancellationToken ct)
    {
        // Ваша логика
        return await httpClient.PostAsync<CustomResult>($"{Game}/custom", data, ct);
    }
}
```

## 🧪 Тестирование

### Простой тест (SimpleNeonFruitsExample)

1. Добавьте `SimpleNeonFruitsExample` на GameObject
2. Запустите сцену - в консоли увидите полный пример работы
3. Используйте Context Menu кнопки в Inspector для ручного тестирования

### Unit тесты

```csharp
[Test]
public void TestServiceCreation()
{
    var service = NeonFruitsGameServiceFactory.CreateTestService();
    Assert.IsNotNull(service);
    Assert.IsInstanceOf<INeonFruitsGameService>(service);
}
```

## 🐛 Отладка

### Включение логирования

```csharp
// В коде
var service = NeonFruitsGameServiceFactory.CreateService("neon-fruits");

// Или через Inspector в NeonFruitsGameController
enableDebugLogging = true;
```

### Полезные Context Menu команды

- `Get Game State` - показать текущее состояние игры
- `Test Create Service` - протестировать создание сервиса
- `Test Get User` - протестировать получение пользователя

## 🔗 Интеграция с DI

```csharp
// В вашем DI контейнере
public void RegisterGameServices()
{
    var httpClient = container.Resolve<IHttpClient>();
    var gameService = NeonFruitsGameServiceFactory.CreateService(httpClient, "neon-fruits");
    
    container.RegisterInstance<INeonFruitsGameService>(gameService);
}
```

## ⚡ Производительность

- Все HTTP вызовы асинхронные
- Поддержка CancellationToken для отмены операций
- Graceful degradation при ошибках UI
- Минимальное использование рефлексии

## 🛡️ Обработка ошибок

Все методы бросают `Energy8Exception` с понятными сообщениями:

```csharp
try
{
    await gameService.SpinAsync(request, ct);
}
catch (Energy8Exception ex)
{
    Debug.LogError($"Game error: {ex.UserFriendlyMessage}");
    // Обработка ошибки
}
```

## 📝 TODO

- [ ] Добавить кэширование состояния игры
- [ ] Реализовать offline режим
- [ ] Добавить метрики производительности
- [ ] Создать визуальный отладчик состояний
- [ ] Добавить поддержку мультиплеера

## 🤝 Вклад в проект

1. Создайте новые DTO в `Game.Dto`
2. Расширьте интерфейсы в `Game.Services`
3. Добавьте примеры в `Game.Examples`
4. Обновите документацию

---

**Создано для Energy8 Identity архитектуры** 🎰✨
