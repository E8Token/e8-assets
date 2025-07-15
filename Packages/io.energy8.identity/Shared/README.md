# Shared Module

Общие компоненты для всех модулей системы Identity Energy8.

## Структура

### Core
- **Extensions/** - Расширения для различных типов
- **Exceptions/** - Исключения и обработка ошибок
- **Error/** - Система обработки ошибок
- **Models/** - Общие модели данных
- **Contracts/** - DTO и контракты API

### Runtime
- **Services/** - Общие сервисы времени выполнения

### Editor
- Общие инструменты редактора

## Использование

Модуль содержит общую функциональность, используемую во всех других модулях системы Identity.

```csharp
// Использование расширений
await someTask.WithErrorHandling(ErrorHandlingMethod.LogAndContinue);

// Работа с исключениями
throw new AuthenticationException("Invalid credentials");
```
