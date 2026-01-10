# Energy8 Environment Config

Пакет для управления конфигурацией Unity проекта в зависимости от окружения (development, staging, production).

## Обзор

Этот пакет предоставляет унифицированную систему для управления конфигурацией модулей в разных средах. Вместо ручного переключения между конфигурационными файлами, вы просто выбираете окружение через UI, и все модули автоматически используют правильную конфигурацию.

## Основные возможности

- **Управление окружениями**: Создание и переключение между development, staging, production и другими окружениями
- **Единый базовый класс для всех конфигов**: `BaseModuleConfig` обеспечивает структуру и поведение
- **Автоматическая загрузка**: `ModuleConfigManager<T>` автоматически загружает нужную конфигурацию на основе текущего окружения
- **Простое создание новых конфигов**: `BaseModuleConfigCreator<T>` позволяет создавать конфиги для всех окружений одной командой
- **Автогенерация названий файлов**: Файлы конфигураций именуются автоматически по шаблону `{ClassName}_{EnvironmentName}`

## Архитектура

```
io.energy8.environment-config/
├── Runtime/
│   ├── Base/
│   │   ├── BaseModuleConfig.cs          # Базовый класс для всех конфигов
│   │   └── ModuleConfigManager.cs      # Менеджер для загрузки конфигов
│   └── Core/
│       └── EnvironmentManager.cs        # Управление текущим окружением
├── Editor/
│   ├── Base/
│   │   └── BaseModuleConfigCreator.cs   # Базовый класс для создания конфигов
│   ├── Settings/
│   │   └── EnvironmentSettings.cs       # ScriptableObject для настроек окружений
│   ├── UI/
│   │   └── EnvironmentSwitcher.cs        # Окно переключения окружений
│   └── Utilities/
│       └── EnvironmentConfigUtility.cs # Вспомогательные методы
```

## Быстрый старт

### 1. Создание окружений

1. Откройте `Tools → Environment Switcher`
2. Нажмите "Add Environment" для создания нового окружения
3. Укажите имя (например: Development, Staging, Production)
4. Повторите для всех нужных окружений
5. Нажмите "Save Settings"

### 2. Создание конфигурации для нового модуля

Предположим, вы хотите создать конфигурацию для модуля `AnalyticsConfig`:

#### Шаг 1: Создайте класс конфигурации

```csharp
using Newtonsoft.Json;
using UnityEngine;
using Energy8.EnvironmentConfig.Base;

public class AnalyticsConfig : BaseModuleConfig
{
    [JsonProperty("analyticsApiKey")]
    public string AnalyticsApiKey;

    [JsonProperty("enableLogging")]
    public bool EnableLogging = true;
}
```

#### Шаг 2: Создайте класс-создатель конфигов

```csharp
#if UNITY_EDITOR
using Energy8.EnvironmentConfig.Editor;

public class AnalyticsConfigCreator : BaseModuleConfigCreator<AnalyticsConfig>
{
}
#endif
```

#### Шаг 3: Создайте конфигурации для всех окружений

1. В Project View кликните правой кнопкой мыши
2. Выберите `E8 Config → Create/AnalyticsConfig`
3. Система автоматически создаст файлы:
   - `Assets/Resources/E8Config/AnalyticsConfig_Development.asset`
   - `Assets/Resources/E8Config/AnalyticsConfig_Staging.asset`
   - `Assets/Resources/E8Config/AnalyticsConfig_Production.asset`

#### Шаг 4: Настройте параметры для каждого окружения

Откройте каждый файл и настройте параметры для соответствующего окружения.

### 3. Использование конфигурации в коде

```csharp
using Energy8.EnvironmentConfig.Base;

public class AnalyticsService
{
    public void Initialize()
    {
        var config = ModuleConfigManager<AnalyticsConfig>.GetCurrentConfig("Analytics");
        
        if (config != null)
        {
            Debug.Log($"Initializing with API Key: {config.AnalyticsApiKey}");
        }
    }
}
```

## Поддерживаемые окружения

По умолчанию поддерживаются следующие окружения:

- **Development**: Для локальной разработки и тестирования
- **Staging**: Для предварительного тестирования на staging серверах
- **Production**: Для production окружения

Вы можете создавать любые дополнительные окружения через Environment Switcher.

## UI - Environment Switcher

`Tools → Environment Switcher` предоставляет удобный интерфейс для:

- Создания новых окружений
- Удаления существующих окружений
- Переименования окружений
- Переключения активного окружения
- Визуального отображения текущего окружения

## Формат имён файлов

Конфигурационные файлы хранятся в папке `Assets/Resources/E8Config/` и именуются по шаблону:

```
{ClassName}_{EnvironmentName}.asset
```

Примеры:
- `IdentityConfig_Development.asset`
- `AnalyticsConfig_Production.asset`
- `BillingConfig_Staging.asset`

## Миграция существующих конфигураций

Если у вас есть существующие конфигурации без поддержки окружений:

1. Создайте окружения через Environment Switcher
2. Для каждой конфигурации создайте наследника `BaseModuleConfig`
3. Создайте наследника `BaseModuleConfigCreator<T>` для вашего класса конфигурации
4. Используйте контекстное меню для создания конфигов для всех окружений
5. Скопируйте значения из старых конфигов в новые файлы для каждого окружения

## Примеры использования

### Identity модуль

```csharp
// Получение конфигурации Identity
var identityConfig = ModuleConfigManager<IdentityConfig>.GetCurrentConfig("Identity");

// Использование параметров
var authUrl = identityConfig?.AuthServerUrl ?? "http://localhost";
```

### Analytics модуль

```csharp
// Получение конфигурации Analytics
var analyticsConfig = ModuleConfigManager<AnalyticsConfig>.GetCurrentConfig("Analytics");

// Проверка включенности аналитики
if (analyticsConfig?.EnableAnalytics ?? false)
{
    // Инициализация аналитики
}
```

### Billing модуль

```csharp
// Получение конфигурации Billing
var billingConfig = ModuleConfigManager<BillingConfig>.GetCurrentConfig("Billing");

// Использование параметров биллинга
var productId = billingConfig?.PremiumProductId ?? "premium";
```

## Требования

- Unity 2022.3 или выше
- Newtonsoft.Json (включен в Unity)

## Логирование

Все операции загрузки конфигураций логируются в Unity Console:

```
[ModuleConfigManager] Loaded Development configuration for Identity
[ModuleConfigManager] Loaded Staging configuration for Analytics
```

## Советы и лучшие практики

1. **Дефолтные значения**: Всегда задавайте дефолтные значения для необязательных полей в классе конфигурации
2. **JsonProperty**: Используйте атрибут `[JsonProperty]` для всех полей, которые будут сериализованы в JSON
3. **Именование**: Используйте описательные имена для окружений
4. **Версионирование**: Для разных версий API в разных окружениях можно использовать отдельные конфиги
5. **Безопасность**: Конфигурации с секретными ключами не должны коммититься в Git (добавьте в .gitignore)

## Troubleshooting

### Конфигурация не загружается

Убедитесь что:
1. Файл конфигурации находится в папке `Resources/E8Config/`
2. Имя файла соответствует шаблону `{ClassName}_{EnvironmentName}.asset`
3. Окружение с таким именем существует в Environment Settings
4. Класс конфигурации наследуется от `BaseModuleConfig`

### Контекстное меню не появляется

Убедитесь что:
1. Класс-создатель находится в папке `Editor/` или имеет `#if UNITY_EDITOR`
2. Класс создателя наследуется от `BaseModuleConfigCreator<T>`
3. T параметр указан правильно (тип вашего конфига)

## Лицензия

Energy8 Environment Config - внутренний пакет Energy8.
