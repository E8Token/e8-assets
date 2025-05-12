# JSPluginTools - Руководство по модульной архитектуре

## Обзор архитектуры

Energy8 JSPluginTools теперь использует модульную систему с внедрением зависимостей для более гибкой и расширяемой архитектуры. Это руководство поможет разработчикам понять новую структуру и создать свои собственные модули.

## Ключевые компоненты

### IPluginCore и PluginCore

Ядро системы, которое управляет жизненным циклом плагина, хранит зависимости в контейнере и обеспечивает общие функции для всех модулей:

- Инициализация и завершение работы
- Коммуникация с JavaScript
- Управление памятью
- Контейнер зависимостей (DI)
- Шина сообщений (MessageBus)

### BaseModuleManager

Базовый класс для всех модулей, который предоставляет:

- Стандартный жизненный цикл модуля (инициализация, завершение)
- Управление логированием через ErrorHandler
- Безопасную обработку ошибок
- Автоматическую регистрацию в DI-контейнере
- Обмен сообщениями через MessageBus
- Доступ к другим модулям через контейнер зависимостей

### IModuleManager

Общий интерфейс для всех модулей, который определяет базовые возможности:

- Свойство IsInitialized
- Методы Initialize и Shutdown
- События жизненного цикла (OnInitialized, OnShutdown, OnInitializationError)

### ServiceContainer

Простой контейнер зависимостей, который позволяет:

- Регистрировать сервисы по интерфейсу
- Получать сервисы из контейнера
- Создавать сервисы по требованию через фабрики

## Создание нового модуля

Для создания нового модуля следуйте этим шагам:

1. Создайте интерфейс модуля, наследующийся от IModuleManager
2. Создайте класс модуля, наследующийся от BaseModuleManager
3. Реализуйте абстрактные методы и логику вашего модуля
4. Зарегистрируйте свой модуль в DI-контейнере

### Пример интерфейса модуля

```csharp
public interface IMyModule : IModuleManager
{
    // Определите методы вашего модуля
    Task<string> DoSomething(string parameter);
    
    // События, которые может генерировать модуль
    event Action<string> OnSomethingHappened;
}
```

### Пример реализации модуля

```csharp
public class MyModule : BaseModuleManager, IMyModule
{
    // Объявляем события
    public event Action<string> OnSomethingHappened;
    
    // Регистрируем модуль в DI-контейнере
    protected override void RegisterModule()
    {
        _core.RegisterService<IMyModule>(this);
    }
    
    // Инициализируем модуль
    protected override void OnInitializeInternal()
    {
        // Регистрируем обработчики сообщений
        RegisterMessageHandler<string>("somethingHappened", HandleSomethingHappened);
        
        // Остальная логика инициализации
    }
    
    // Освобождаем ресурсы
    protected override void OnShutdownInternal()
    {
        // Логика очистки ресурсов
    }
    
    // Обработчик события от JavaScript
    private void HandleSomethingHappened(string data)
    {
        OnSomethingHappened?.Invoke(data);
    }
    
    // Реализация метода из интерфейса
    public Task<string> DoSomething(string parameter)
    {
        return ExecuteSafe("DoSomething", async () =>
        {
            // Логика метода
            var result = await SendRequestAsync<object, string>(
                "doSomething", 
                new { parameter }
            );
            
            return result;
        });
    }
}
```

## Использование DI-контейнера

### Регистрация сервисов

Сервисы регистрируются в контейнере зависимостей ядра:

```csharp
// Регистрация по интерфейсу
_core.RegisterService<IMyService>(myServiceInstance);

// Регистрация фабрики для ленивого создания
_core.RegisterServiceFactory<IMyLazyService>(() => new MyLazyService());
```

### Получение сервисов

В модулях вы можете получить доступ к другим сервисам через метод GetService:

```csharp
// В любом методе модуля
var otherModule = GetService<IOtherModule>();
if (otherModule != null)
{
    await otherModule.DoSomethingElse();
}
```

## Обработка ошибок и логирование

Используйте ErrorHandler для логирования и обработки ошибок:

```csharp
// Информационное сообщение
ErrorHandler.LogInfo(_moduleName, "Operation completed successfully");

// Предупреждение
ErrorHandler.LogWarning(_moduleName, "Something might be wrong");

// Подробное логирование
ErrorHandler.LogVerbose(_moduleName, "Details of operation: ...");

// Безопасное выполнение с обработкой ошибок
return ExecuteSafe("MethodName", () => {
    // Потенциально опасный код
    return result;
});
```

## Взаимодействие с JavaScript

### Отправка сообщений

```csharp
// Отправка сообщения без ожидания ответа
SendMessage("eventName", data);

// Отправка запроса и ожидание ответа
var response = await SendRequestAsync<RequestType, ResponseType>("requestName", request);
```

### Прием сообщений от JavaScript

```csharp
// В методе OnInitializeInternal
RegisterMessageHandler<DataType>("eventName", HandleEvent);

// Обработчик события
private void HandleEvent(DataType data)
{
    // Обработка данных
    OnEventReceived?.Invoke(data);
}
```

## Рекомендации и лучшие практики

1. **Интерфейс перед реализацией**: Всегда определяйте интерфейс модуля перед его реализацией
2. **Регистрация в DI**: Регистрируйте модуль в контейнере для доступа из других модулей
3. **Ошибки**: Используйте ExecuteSafe для безопасной обработки ошибок
4. **Асинхронность**: Асинхронные методы для операций, требующих взаимодействия с JavaScript
5. **Префиксы сообщений**: Имена сообщений автоматически получают префикс с именем модуля
6. **Документация**: Добавляйте XML-комментарии ко всем публичным интерфейсам и методам

## Примеры

См. `TemplateModule.cs` для полного примера реализации модуля по новому стандарту.