# HTTP Client с Middleware Pipeline

## Обзор

Пакет `Energy8.Identity.Http` предоставляет гибкую и расширяемую архитектуру HTTP клиента с использованием паттерна Middleware Pipeline. Это позволяет легко добавлять, удалять и настраивать обработчики HTTP запросов в виде цепочки middleware.

## Архитектура

### Основные компоненты

#### 1. `IHttpClient` - Интерфейс HTTP клиента
Определяет основной контракт для HTTP клиентов с поддержкой:
- CRUD операций (GET, POST, PUT, DELETE)
- Настраиваемой сериализации данных
- Опций запроса (RequestOptions)
- Управления авторизацией

#### 2. `IRequestSerializer` - Интерфейс сериализатора
Позволяет использовать различные форматы данных:
- `WWWFormSerializer` - формат `application/x-www-form-urlencoded` (по умолчанию)
- `JsonSerializer` - формат `application/json`

#### 3. `IHttpMiddleware` - Интерфейс middleware
Базовый контракт для всех middleware:
```csharp
public interface IHttpMiddleware
{
    string Name { get; }
    UniTask<HttpResponse> ProcessAsync(HttpRequest request, HttpMiddlewareDelegate next, CancellationToken ct);
}
```

#### 4. `HttpPipeline` - Конвейер middleware
Управляет цепочкой middleware и порядком их выполнения:
- `Use()` - добавить middleware в конец
- `UseFirst()` - добавить middleware в начало
- `Remove()` - удалить middleware по имени
- `Clear()` - очистить все middleware

### Встроенные Middleware

#### `ValidationMiddleware` (всегда первый)
Проверяет корректность запроса перед отправкой:
- Валидация URL формата
- Проверка HTTP метода
- Проверка таймаута
- Валидация заголовков

#### `CircuitBreakerMiddleware` (второй)
Реализует паттерн Circuit Breaker:
- **Closed**: все запросы проходят нормально
- **Open**: все запросы блокируются при достижении порога ошибок
- **HalfOpen**: проверочное состояние для восстановления

Параметры:
- `failureThreshold`: количество ошибок для открытия circuit (по умолчанию: 5)
- `openTimeout`: время в состоянии Open (по умолчанию: 1 минута)
- `halfOpenMaxCalls`: успешных запросов для возврата в Closed (по умолчанию: 3)

#### `TimeoutMiddleware` (третий)
Управляет таймаутами запросов:
- Создаёт связанный CancellationToken с таймаутом
- Возвращает `RequestTimeout` при истечении времени

#### `RetryMiddleware` (четвёртый)
Автоматический retry при временных ошибках:
- Экспоненциальный backoff между попытками
- Retry на: `ServiceUnavailable`, `BadGateway`, `GatewayTimeout`, connection errors

Параметры:
- `maxRetries`: количество попыток (по умолчанию: 3)
- `baseDelayMs`: базовая задержка в ms (по умолчанию: 1000)
- `maxDelayMs`: максимальная задержка в ms (по умолчанию: 5000)

#### `StatisticsMiddleware` (пятый)
Собирает статистику HTTP запросов:
- `TotalRequests`: всего запросов
- `SuccessfulRequests`: успешных
- `FailedRequests`: неуспешных
- `AverageResponseTime`: среднее время ответа
- `LastRequestTime`: время последнего запроса

#### `LoggingMiddleware` (всегда последний)
Логирует все HTTP запросы и ответы:
- Детальное или краткое логирование
- Маскирование токенов авторизации
- Запись в Unity Console

## Использование

### Базовое использование

```csharp
// Создаём HTTP клиент
var client = new UnityHttpClient("https://api.example.com");

// Устанавливаем токен авторизации
client.SetAuthToken("your-access-token");

// Делаем запрос
var result = await client.PostAsync<MyResponseType>("endpoint", myData, cancellationToken);

// Очищаем токен
client.ClearAuthToken();
```

### Изменение формата сериализации

```csharp
// По умолчанию используется WWWForm
client.Serializer = new WWWFormSerializer();

// Для JSON API
client.Serializer = new JsonSerializer();
```

### Доступ к статистике

```csharp
var stats = client.Statistics;
Debug.Log($"Total: {stats.TotalRequests}");
Debug.Log($"Success: {stats.SuccessfulRequests}");
Debug.Log($"Failed: {stats.FailedRequests}");
Debug.Log($"Avg Time: {stats.AverageResponseTime.TotalMilliseconds}ms");
```

### Доступ к Circuit Breaker

```csharp
// Для доступа к circuit breaker нужно сохранить ссылку при создании
// или добавить метод в UnityHttpClient для доступа к middleware
var circuitBreaker = client.GetMiddleware<CircuitBreakerMiddleware>();
var state = circuitBreaker.GetState();

// Ручной сброс
circuitBreaker.Reset();

// Получение статистики
var stats = circuitBreaker.GetStats();
```

### Настройка параметров запроса

```csharp
var options = new RequestOptions(
    timeoutSeconds: 60,    // таймаут запроса
    retryCount: 5,          // количество retry (переопределяет RetryMiddleware)
    detailedLogging: true     // детальное логирование
);

// Использовать для одного запроса
var result = await client.GetAsync<MyType>("endpoint", cancellationToken, options);

// Или установить как дефолтные
client.SetDefaultOptions(options);
```

### Управление логированием

```csharp
// Включить логирование токенов (по умолчанию отключено)
client.EnableTokenLogging(true);
```

## Порядок выполнения Middleware

При создании `UnityHttpClient` middleware устанавливаются в следующем порядке:

1. **ValidationMiddleware** - проверка параметров запроса
2. **CircuitBreakerMiddleware** - проверка состояния circuit
3. **TimeoutMiddleware** - установка таймаута
4. **RetryMiddleware** - retry при ошибках
5. **StatisticsMiddleware** - сбор статистики
6. **LoggingMiddleware** - логирование результата
7. **FinalHandler** - выполнение фактического HTTP запроса

Каждый middleware может:
- Обработать запрос перед передачей следующему
- Изменить запрос
- Прервать цепочку и вернуть ответ/ошибку
- Обработать ответ после возврата от следующего

## Расширяемость

### Создание собственного Middleware

```csharp
public class CustomHeaderMiddleware : IHttpMiddleware
{
    public string Name => "CustomHeader";
    
    public async UniTask<HttpResponse> ProcessAsync(
        HttpRequest request, 
        HttpMiddlewareDelegate next, 
        CancellationToken ct)
    {
        // Добавляем кастомный заголовок
        request.Headers["X-Custom-Header"] = "value";
        
        // Передаём управление следующему middleware
        return await next(request, ct);
    }
}
```

### Добавление собственного Middleware

Для добавления кастомного middleware нужно расширить `UnityHttpClient`:

```csharp
public class ExtendedHttpClient : UnityHttpClient
{
    public ExtendedHttpClient(string baseUrl) : base(baseUrl)
    {
        // Добавляем кастомный middleware
        var customMiddleware = new CustomHeaderMiddleware();
        pipeline.Use(customMiddleware);
    }
}
```

Или создать метод в `UnityHttpClient` для доступа к pipeline и middleware:

```csharp
public void AddMiddleware(IHttpMiddleware middleware)
{
    pipeline.Use(middleware);
}

public T GetMiddleware<T>() where T : IHttpMiddleware
{
    return pipeline.GetMiddlewares().OfType<T>().FirstOrDefault();
}
```

## Обработка ошибок

Middleware преобразуют ошибки в `Energy8Exception` с соответствующим типом:
- `ValidationException` - ошибки валидации (400)
- `AuthenticationException` - ошибки аутентификации (401)
- `AuthorizationException` - ошибки авторизации (403)
- `NotFoundException` - ресурс не найден (404)
- `ServerException` - ошибки сервера (500, 502, 503, 504)
- `Energy8Exception` - общие ошибки с параметрами canRetry, canShowUser, canReport

## Модели данных

### `HttpRequest`
```csharp
public class HttpRequest
{
    public string Method { get; set; }
    public string Url { get; set; }
    public object Data { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public int TimeoutSeconds { get; set; }
}
```

### `HttpResponse`
```csharp
public class HttpResponse
{
    public object Data { get; set; }
    public string ResponseBody { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    public long DurationMs { get; set; }
}

public class HttpResponse<T> : HttpResponse
{
    public new T Data { get; set; }
}
```

## Производительность

### Оптимизация производительности

1. **Connection Pooling**: UnityWebRequest автоматически переиспользует соединения
2. **Circuit Breaker**: предотвращает бесполезные запросы при проблемах сервера
3. **Retry с Backoff**: снижает нагрузку на сервер при временных проблемах
4. **Logging по требованию**: минимальное влияние на производительность в release

### Рекомендации

1. Использовать `JsonSerializer` для REST API с JSON
2. Настраивать `CircuitBreaker` при работе с нестабильными API
3. Ограничивать количество retry для критических операций
4. Включать детальное логирование только при отладке
5. Маскировать токены в production логах

## Безопасность

1. **Маскирование токенов**: по умолчанию в логах
2. **HTTPS**: всегда использовать HTTPS endpoints
3. **Timeout**: защита от бесконечного ожидания
4. **Circuit Breaker**: защита от DDAP эффектов при ошибках

## Логирование

Все HTTP запросы логируются в Unity Console:
- `[Identity HTTP]` - префикс для фильтрации
- Успешные запросы: `Debug.Log`
- Ошибки: `Debug.LogError`
- Токены маскируются: `*** (masked) ***`

Пример лога:
```
[Identity HTTP] POST https://api.example.com/users - Status: 200, Duration: 245ms, Success: True
```

Детальный лог (JSON):
```json
{
  "Method": "POST",
  "Url": "https://api.example.com/users",
  "Headers": {
    "Authorization": "*** (masked) ***",
    "Content-Type": "application/json"
  },
  "ResponseCode": 200,
  "Duration": 245,
  "Success": true,
  "ErrorMessage": null,
  "ResponseBody": "{...}"
}
