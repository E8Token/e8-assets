# Identity Configuration

Простая система конфигурации окружений для Identity пакета Energy8.

## Обзор

Система использует Unity ScriptableObject для хранения конфигураций окружений и define symbols для переключения между ними.

## Компоненты

### IdentityConfig

ScriptableObject конфигурации окружения. Содержит:

- **AuthServerUrl** - URL сервера авторизации
- **FirebaseConfig** - конфиг Firebase для нативных платформ (Android, iOS)
- **FirebaseWebConfig** - конфиг Firebase для WebGL
- **EnableAnalytics** - включить аналитику
- **EnableDebugLogging** - показывать дебаг логи
- **TrackUserActions** - отслеживать действия пользователя
- **TrackErrors** - отслеживать ошибки
- **TrackPerformance** - отслеживать производительность

### IdentityConfigManager

Менеджер для загрузки текущей конфигурации на основе define symbols.

**Как это работает:**
- Генератор создает файлы в папку: `Packages/io.energy8.identity/Resources/Identity/Configuration/`
- Менеджер загружает их через: `Resources.Load<IdentityConfig>("Identity/Configuration/IdentityConfig_Example")`
- `Resources.Load` ищет файлы в ЛЮБОЙ папке `Resources` по ОТНОСИТЕЛЬНОМУ пути
- **ЭТО ОДИН И ТОТ ЖЕ ФАЙЛ**, просто разный способ обращения

Путь в коде:
```csharp
Resources.Load<IdentityConfig>("Identity/Configuration/IdentityConfig_Example");
```

Физический файл:
```
Packages/io.energy8.identity/Resources/Identity/Configuration/IdentityConfig_Example.asset
```

Пути СОВПАДАЮТ! Генератор и менеджер работают с ОДНИМ И ТЕМ ЖЕ набором файлов.

```csharp
var config = IdentityConfigManager.CurrentConfig;
Debug.Log($"Server: {config.AuthServerUrl}");
```

Примечание: При переключении окружений в Editor вызовите `IdentityConfigManager.Reload()` для перезагрузки конфигурации.

### E8EnvironmentSwitcher

Окно переключения окружений (Editor-only).

Меню: `E8 Tools → Environment Switcher`

Позволяет быстро менять define symbols: DEVELOPMENT, DEBUG, PRODUCTION.

## Создание конфигураций

### Способ 1: Генератор Example

1. Выберите `E8 Tools → Identity → Generate Example Config`
2. **Файл создастся по пути:**
   ```
   Packages/io.energy8.identity/Resources/Identity/Configuration/IdentityConfig_Example.asset
   ```
   
   **В Unity Project Window вы найдете его в:**
   ```
   Packages/io.energy8.identity → Resources → Identity → Configuration → IdentityConfig_Example
   ```
   
3. Скопируйте этот файл (Ctrl+D или Duplicate) 2 раза для создания конфигов для всех окружений:
   - `IdentityConfig_Development.asset`
   - `IdentityConfig_Debug.asset`
   - `IdentityConfig_Production.asset`
   
4. Откройте каждый файл в Inspector и настройте:
   - **AuthServerUrl** - URL вашего сервера авторизации для окружения
   - **FirebaseConfig** - выберите JSON конфиг Firebase (для Android/iOS)
     ```
     Assets/Resources/Identity/Configuration/Firebase/Auth/Auth_Local.json
     ```
   - **FirebaseWebConfig** - выберите JSON конфиг Firebase (для WebGL)
     ```
     Assets/Resources/Identity/Configuration/Firebase/Auth/Web_Auth_Local.json
     ```
   - **EnableAnalytics**, **EnableDebugLogging**, **TrackUserActions**, **TrackErrors**, **TrackPerformance** - настройки логирования

### Способ 2: Ручное создание

1. Правый клик в Project Window
2. Create → Identity → Config
3. Дайте файлу имя: `IdentityConfig_Development` (или `_Debug`, `_Production`)
4. Переместите созданный файл в:
   ```
   Packages/io.energy8.identity/Resources/Identity/Configuration/
   ```
5. Настройте URL и Firebase конфиги

## Именование конфигов

IdentityConfigManager ищет конфиги по названию с учетом define symbols:

| Define Symbol | Имя конфига | Fallback |
|--------------|-------------|----------|
| DEVELOPMENT   | `IdentityConfig_Development` | Development |
| DEBUG         | `IdentityConfig_Debug`       | Debug |
| PRODUCTION    | `IdentityConfig_Production`  | Production |

Если конфиг не найден, менеджер возвращает `null`.

## Переключение окружений

### Через переключатель

1. Откройте `E8 Tools → Environment Switcher`
2. Выберите окружение: Development, Debug или Production
3. Нажмите "Apply Environment"

### Вручную

Зайдите в Player Settings → Other Settings → Scripting Define Symbols и добавьте:
- `DEVELOPMENT` - для разработки
- `DEBUG` - для отладки
- `PRODUCTION` - для продакшна

## Использование в коде

```csharp
using Energy8.Identity.Configuration.Core;

// Получить текущую конфигурацию
var config = IdentityConfigManager.CurrentConfig;

if (config != null)
{
    Debug.Log($"Auth Server: {config.AuthServerUrl}");
    
    // Получить Firebase конфиг для текущей платформы
    var firebaseConfig = config.GetFirebaseConfig();
    
    if (config.EnableAnalytics)
    {
        // Инициализировать аналитику
    }
}
else
{
    Debug.LogWarning("No IdentityConfig found for current environment!");
}

// Проверить текущее окружение
if (IdentityConfigManager.CurrentEnvironment == EnvironmentType.Debug)
{
    // Код только для Debug окружения
}
```

## Структура папок

Конфигурационные файлы должны находиться в:
```
Packages/io.energy8.identity/Resources/Identity/Configuration/
    IdentityConfig_Development.asset      - конфиг для DEVELOPMENT
    IdentityConfig_Debug.asset            - конфиг для DEBUG
    IdentityConfig_Production.asset     - конфиг для PRODUCTION
```

Firebase конфиги хранятся отдельно в проекте:
```
Assets/Resources/Identity/Configuration/Firebase/
    Auth/
        Auth_Local.json                 - Firebase конфиг для Development (Android/iOS)
        Auth_Debug.json                - Firebase конфиг для Debug (Android/iOS)
        Auth_Production.json           - Firebase конфиг для Production (Android/iOS)
    Web/
        Web_Auth_Local.json           - Firebase конфиг для Development (WebGL)
        Web_Auth_Debug.json          - Firebase конфиг для Debug (WebGL)
        Web_Auth_Production.json     - Firebase конфиг для Production (WebGL)
```

## Firebase конфиги

Firebase конфиги хранятся отдельно:

```
Assets/Resources/Identity/Configuration/Firebase/
    Auth/
        Auth_Local.json
        Auth_Debug.json
        Auth_Production.json
    Web/
        Web_Auth_Local.json
        Web_Auth_Debug.json
        Web_Auth_Production.json
```

Ссылка на конфиг устанавливается в поле `FirebaseConfig` или `FirebaseWebConfig` IdentityConfig.

## Удаление старых файлов

При миграции со старой системы удалите:

- `Core/IdentityConfiguration.cs`
- `Core/Models/` (папка целиком)
- `Core/Interfaces/` (папка целиком)
- `Runtime/` (папка целиком)
- `Editor/IdentityBuildSettings.cs`
- `Editor/IdentityConfigSettings.cs`
