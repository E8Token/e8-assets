# Http Module

Модуль HTTP клиента для системы Identity Energy8.

## Структура

### Core
- **IHttpClient.cs** - Интерфейс HTTP клиента
- **UnityHttpClient.cs** - Реализация на Unity WebRequest
- **Models/** - Модели данных для HTTP запросов

### Runtime
- Компоненты времени выполнения для HTTP

### Editor
- Инструменты редактора для настройки HTTP

## Использование

Модуль предоставляет унифицированный интерфейс для выполнения HTTP запросов.

```csharp
IHttpClient httpClient = new UnityHttpClient();
var response = await httpClient.GetAsync<UserDto>("/api/user", cancellationToken);
```
