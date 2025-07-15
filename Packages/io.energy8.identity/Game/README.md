# Game Module

Модуль игровых сервисов для системы Identity Energy8.

## Структура

### Core
- **IGameService.cs** - Интерфейс игрового сервиса
- **GameService.cs** - Реализация игрового сервиса

### Runtime
- Компоненты времени выполнения для игровых сервисов

### Editor
- Инструменты редактора для настройки игровых сервисов

## Использование

Модуль предоставляет API для работы с игровыми данными пользователя, сессиями и балансом.

```csharp
var gameService = new GameService<GameUserDto, GameSessionDto>(httpClient);
var gameUser = await gameService.GetUserAsync(cancellationToken);
var session = await gameService.CreateSessionsAsync(cancellationToken);
```
