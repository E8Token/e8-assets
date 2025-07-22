# NeonFruits Game Service - API Methods

## ✅ Все ваши методы реализованы!

Вот прямой доступ к методам, которые вы запрашивали:

### 🎯 1. InitializeGameAsync
```csharp
public async UniTask<NFGameStatusResponseDto> InitializeGameAsync(string sessionId, CancellationToken ct)
```
**Использование:**
```csharp
var gameStatus = await gameService.InitializeGameAsync(sessionId, cancellationToken);
```

### 🎯 2. SpinAsync (Generic метод)
```csharp
public async UniTask<TSpinResponse> SpinAsync<TSpinRequest, TSpinResponse>(TSpinRequest request, CancellationToken ct)
    where TSpinRequest : SpinRequestDto
    where TSpinResponse : GameStatusResponseDto
```
**Использование:**
```csharp
var spinRequest = new NeonFruitsSpinRequestDto(sessionId, betAmount, lines);
var result = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(spinRequest, ct);
```

### 🎯 3. ActivateBonusAsync
```csharp
public async UniTask<NFGameStatusResponseDto> ActivateBonusAsync(string sessionId, CancellationToken ct)
```
**Использование:**
```csharp
var bonusResult = await gameService.ActivateBonusAsync(sessionId, cancellationToken);
```

### 🎯 4. CreateSessionsAsync (Переопределенный)
```csharp
public override async UniTask<GameSessionDto> CreateSessionsAsync(CancellationToken ct)
```
**Использование:**
```csharp
var session = await gameService.CreateSessionsAsync(cancellationToken);
```

## 🎮 Дополнительные методы:

### GetUserAsync (из базового IGameService)
```csharp
var user = await gameService.GetUserAsync(cancellationToken);
```

### GetGameStatusAsync (дополнительный)
```csharp
var status = await gameService.GetGameStatusAsync(sessionId, cancellationToken);
```

### EndGameSessionAsync (дополнительный)
```csharp
var finalStatus = await gameService.EndGameSessionAsync(sessionId, cancellationToken);
```

## 🚀 Быстрый старт:

### Вариант 1: Прямое использование сервиса
```csharp
// Создание сервиса
var gameService = NeonFruitsGameServiceFactory.CreateService("neon-fruits");

// Полный игровой цикл
var user = await gameService.GetUserAsync(ct);
var session = await gameService.CreateSessionsAsync(ct);
var gameStatus = await gameService.InitializeGameAsync(session.SessionId, ct);

var spinRequest = new NeonFruitsSpinRequestDto(session.SessionId, 100, 20);
var spinResult = await gameService.SpinAsync<NeonFruitsSpinRequestDto, NeonFruitsSpinResponseDto>(spinRequest, ct);

var bonusResult = await gameService.ActivateBonusAsync(session.SessionId, ct);
var finalStatus = await gameService.EndGameSessionAsync(session.SessionId, ct);
```

### Вариант 2: Через NeonFruitsGameController
```csharp
// В Unity - добавить NeonFruitsGameController на GameObject
var controller = FindObjectOfType<NeonFruitsGameController>();

// Использование через UI кнопки
controller.StartNewGame(); // Создает сессию + инициализирует
controller.Spin();         // Выполняет спин
controller.ActivateBonus(); // Активирует бонус
controller.EndGame();       // Завершает сессию

// Или программно
await controller.StartNewGameAsync(cancellationToken);
var spinResult = await controller.SpinAsync(betAmount, lines, cancellationToken);
```

### Вариант 3: Через SimpleNeonFruitsUI (с кнопками)
1. Добавьте `SimpleNeonFruitsUI` на GameObject
2. Привяжите UI кнопки в Inspector
3. Нажимайте кнопки для тестирования всех методов

## 🎯 Ваши HTTP вызовы реализованы точно так:

### InitializeGameAsync
```csharp
await httpClient.PutAsync<NFGameStatusResponseDto>(
    $"{Game}/initialize",
    new GameInitializeRequestDto { SessionId = sessionId },
    ct);
```

### SpinAsync
```csharp
await httpClient.PutAsync<TSpinResponse>(
    $"{Game}/spin",
    request,
    ct);
```

### ActivateBonusAsync
```csharp
await httpClient.PostAsync<NFGameStatusResponseDto>(
    $"{Game}/bonus/activate",
    new { sessionId },
    ct);
```

### CreateSessionsAsync (переопределенный)
```csharp
var createDto = new GameSessionCreateDto
{
    ServerId = string.Empty,
    Data = "NeonFruits"
};
return await httpClient.PostAsync<GameSessionDto>($"{Game}/session", createDto, ct);
```

## 📁 Файловая структура:
```
Assets/Scripts/Game/
├── Dto/NeonFruitsDto.cs                    # Все DTO классы
├── Services/INeonFruitsGameService.cs       # Интерфейс с вашими методами
├── Services/NeonFruitsGameService.cs        # Реализация всех методов
├── Factory/NeonFruitsGameServiceFactory.cs  # Создание сервиса
├── Controllers/NeonFruitsGameController.cs  # Unity контроллер
└── Examples/SimpleNeonFruitsUI.cs          # UI пример с кнопками
```

## ✅ Все готово к использованию!

**Никаких Flow менеджеров, никаких лишних оберток - только прямой доступ к вашим методам!** 🎰
