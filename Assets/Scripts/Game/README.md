# NeonFruits Game Service - Минимальная версия

Упрощенная версия игрового сервиса NeonFruits с минимальным функционалом.

## Структура проекта

```
Game/
├── Controllers/
│   └── NeonFruitsGameController.cs    # MonoBehaviour для управления сервисом
├── Dto/
│   └── NeonFruitsDto.cs               # Структуры данных
├── Factory/
│   └── NeonFruitsGameServiceFactory.cs # Фабрика для создания сервиса
├── Services/
│   ├── INeonFruitsGameService.cs       # Интерфейс сервиса
│   └── NeonFruitsGameService.cs        # Реализация сервиса
├── GameServiceRegistration.cs          # Регистрация в DI контейнере
└── README.md
```

## Быстрый старт

### 1. Регистрация сервиса

```csharp
// Получаем контейнер сервисов из Identity системы
IServiceContainer container = // получить из Identity системы

// Убедитесь, что HttpClient уже зарегистрирован в контейнере (обычно делается Identity системой)
// Регистрируем сервис в DI контейнере - он будет переиспользовать существующий HttpClient
GameServiceRegistration.RegisterGameService(container, "neon-fruits");

// Получаем сервис из DI
var gameService = GameServiceRegistration.GetGameService(container);
```

### 2. Использование MonoBehaviour контроллера

1. Добавьте `NeonFruitsGameController` на GameObject в сцене
2. Настройте параметры в Inspector:
   - `Game Endpoint`: эндпоинт API (по умолчанию "neon-fruits")
   - `Enable Debug Logging`: включить логирование
3. Используйте Context Menu в Inspector для тестирования:
   - `Get User Data` - получить данные пользователя
   - `Create Session` - создать игровую сессию
   - `Initialize Game` - инициализировать игру
   - `Perform Spin` - выполнить спин
   - `Activate Bonus` - активировать бонус
   - `Get Game Status` - получить статус игры

**Примечание**: Контроллер автоматически получает HttpClient из Identity системы через `IdentityOrchestrator.Instance.ServiceContainer` для переиспользования существующих соединений. Если Identity система не инициализирована, используется fallback с созданием нового HttpClient.

### 3. Программное использование

```csharp
// Рекомендуемый способ: получение HttpClient из DI контейнера (переиспользует существующий HttpClient)
var httpClient = serviceContainer.Resolve<IHttpClient>();
var gameService = NeonFruitsGameServiceFactory.CreateService(httpClient, "neon-fruits");

// Альтернатива: использование DI регистрации
// GameServiceRegistration.RegisterGameService(serviceContainer, "neon-fruits");
// var gameService = GameServiceRegistration.GetGameService(serviceContainer);

// Устаревший способ: прямое создание с новым HttpClient (не рекомендуется)
// var gameService = NeonFruitsGameServiceFactory.CreateService("neon-fruits");

// Получение данных пользователя
var userData = await gameService.GetUserAsync(cancellationToken);

// Создание сессии
var session = await gameService.CreateSessionsAsync(cancellationToken);

// Инициализация игры
var gameStatus = await gameService.InitializeGameAsync(session.SessionId, cancellationToken);

// Выполнение спина
var spinRequest = new NeonFruitsSpinRequestDto(session.SessionId, 100, 20, false, 0);
var spinResult = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(spinRequest, cancellationToken);
```

## Основные компоненты

- **INeonFruitsGameService** - интерфейс игрового сервиса
- **NeonFruitsGameService** - реализация сервиса с HTTP запросами
- **NeonFruitsGameController** - MonoBehaviour для управления сервисом
- **NeonFruitsGameServiceFactory** - фабрика для создания экземпляров сервиса
- **GameServiceRegistration** - утилиты для работы с DI контейнером
- **NeonFruitsDto** - структуры данных для запросов и ответов