# Firebase Core Package

Мультиплатформенный пакет для Unity, предоставляющий общий API для работы с Firebase App на разных платформах.

## Возможности

- Унифицированный API для Unity SDK и Web SDK
- Автоматическое переключение между платформами
- Project Settings для управления конфигурациями
- Поддержка множественных Firebase приложений
- Асинхронные операции с CancellationToken

## Быстрый старт

### 1. Настройка конфигураций

Перейдите в **Edit > Project Settings > Firebase > Core** и настройте:

- **SDK Configurations**: для нативных платформ (Android, iOS, Standalone)
- **Web Configurations**: для WebGL

### 2. Базовое использование

```csharp
using Energy8.Firebase.Core;
using Energy8.Firebase.Core.Models;

// Инициализация приложения по умолчанию
var appInfo = await FirebaseCore.InitializeAppAsync(configJson);

// Получение экземпляра приложения
var app = FirebaseCore.DefaultApp;

// Проверка инициализации
if (FirebaseCore.IsAppInitialized())
{
    Debug.Log($"App {app.Name} is ready");
}
```

### 3. Работа с именованными приложениями

```csharp
// Инициализация с именем
var secondApp = await FirebaseCore.InitializeAppAsync(configJson, "SecondApp");

// Получение по имени
var app = FirebaseCore.GetApp("SecondApp");

// Удаление приложения
await FirebaseCore.DeleteAppAsync("SecondApp");
```

### 4. События

```csharp
// Подписка на события
FirebaseCore.OnAppInitialized += (appInfo) => {
    Debug.Log($"App {appInfo.Name} initialized successfully");
};

FirebaseCore.OnInitializationError += (appName, error) => {
    Debug.LogError($"Failed to initialize {appName}: {error.Message}");
};
```

## Платформенные особенности

### Unity SDK (Native платформы)
- Использует Firebase Unity SDK
- Требует google-services.json файл
- Поддерживает все функции Unity SDK

### Web SDK (WebGL)
- Использует Firebase JavaScript SDK через jslib плагин
- Требует Firebase config объект
- Автоматически подключается к Firebase в браузере

## API Reference

### FirebaseCore

| Метод | Описание |
|-------|----------|
| `InitializeAppAsync(config, appName?, ct?)` | Инициализирует Firebase приложение |
| `GetApp(appName?)` | Получает экземпляр приложения |
| `GetAllApps()` | Получает все инициализированные приложения |
| `DeleteAppAsync(appName?, ct?)` | Удаляет приложение |
| `IsAppInitialized(appName?)` | Проверяет инициализацию |

### FirebaseAppInfo

| Свойство | Описание |
|----------|----------|
| `Name` | Имя приложения |
| `ProjectId` | ID проекта Firebase |
| `ApiKey` | API ключ |
| `AppId` | ID приложения |
| `IsInitialized` | Статус инициализации |

## Структура конфигурационных файлов

### SDK Configuration (JSON)
```json
{
  "project_info": {
    "project_number": "123456789",
    "project_id": "your-project-id"
  },
  "client": [
    {
      "client_info": {
        "mobilesdk_app_id": "1:123456789:android:abcdef",
        "android_client_info": {
          "package_name": "com.yourcompany.yourgame"
        }
      },
      "oauth_client": [],
      "api_key": [
        {
          "current_key": "your-api-key"
        }
      ]
    }
  ]
}
```

### Web Configuration (JSON)
```json
{
  "apiKey": "your-api-key",
  "authDomain": "your-project-id.firebaseapp.com",
  "projectId": "your-project-id",
  "storageBucket": "your-project-id.appspot.com",
  "messagingSenderId": "123456789",
  "appId": "1:123456789:web:abcdef"
}
```
