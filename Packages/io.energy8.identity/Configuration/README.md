# Configuration Module

Модуль конфигурации для системы Identity Energy8.

## Структура

### Core
- **IdentityConfiguration.cs** - Основная конфигурация
- **Models/** - Модели конфигурации
- **Editor/** - Настройки сборки и конфигурации

### Runtime
- Компоненты времени выполнения для конфигурации

### Editor
- Инструменты редактора для управления конфигурацией

## Использование

Модуль предоставляет централизованное управление конфигурацией системы Identity.

```csharp
var config = IdentityConfiguration.Instance;
var authConfig = config.AuthConfig;
```
