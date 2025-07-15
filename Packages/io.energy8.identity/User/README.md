# User Module

Модуль управления пользователями для системы Identity Energy8.

## Структура

### Core
- **Services/** - Сервисы для работы с пользователями
  - **IUserService.cs** - Интерфейс пользовательского сервиса
  - **UserService.cs** - Реализация пользовательского сервиса

### Runtime
- Компоненты времени выполнения для управления пользователями

### Editor
- Инструменты редактора для настройки пользовательских сервисов

## Использование

Модуль предоставляет API для работы с данными пользователя, профилем и настройками.

```csharp
var userService = new UserService(httpClient);
var user = await userService.GetUserAsync(cancellationToken);
```
