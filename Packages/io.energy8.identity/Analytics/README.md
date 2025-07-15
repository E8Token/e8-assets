# Analytics Module

Модуль аналитики для системы Identity Energy8.

## Структура

### Core
- **Providers/** - Провайдеры аналитики (WebGL, Native)
- **Services/** - Сервисы для работы с аналитикой

### Runtime
- Компоненты времени выполнения для аналитики

### Editor
- Инструменты редактора для настройки аналитики

## Использование

Модуль предоставляет единый интерфейс для работы с различными провайдерами аналитики в зависимости от платформы.

```csharp
var analyticsService = new AnalyticsService();
analyticsService.TrackEvent("user_login", parameters);
```
