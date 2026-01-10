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

**Примечание:** Базовый URL (например, `https://api.dev.energy8.io/v1`) настраивается в конфигурации Identity (`AuthServerUrl`), а эндпоинты указываются относительно него.

```csharp
// baseUrl будет взят из конфига: https://api.dev.energy8.io/v1
// Итоговый URL: https://api.dev.energy8.io/v1/auth/user
IHttpClient httpClient = new UnityHttpClient();
var response = await httpClient.GetAsync<UserDto>("auth/user", cancellationToken);
```
