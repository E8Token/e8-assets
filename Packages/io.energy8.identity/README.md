# Energy8 Identity Package

Модульная система аутентификации и управления пользователями для Unity проектов Energy8.

## Архитектура

Пакет использует модульную архитектуру, где каждый модуль имеет следующую структуру:

```
Module/
├── Core/       # Основная логика и интерфейсы
├── Runtime/    # Компоненты времени выполнения
└── Editor/     # Инструменты редактора
```

## Модули

### 🔐 [Auth](Auth/README.md)
Система аутентификации с поддержкой различных провайдеров (Native, WebGL/Firebase).

### 📊 [Analytics](Analytics/README.md)
Система аналитики с поддержкой различных платформ и провайдеров.

### 🌐 [Http](Http/README.md)
HTTP клиент для взаимодействия с API серверами.

### 🎮 [Game](Game/README.md)
Игровые сервисы для работы с игровыми данными, сессиями и балансом.

### 👤 [User](User/README.md)
Управление пользователями, профилями и настройками.

### 🎨 [UI](UI/README.md)
Полнофункциональная система UI с анимациями и responsive дизайном.

### ⚙️ [Configuration](Configuration/README.md)
Централизованная система конфигурации.

### 📚 [Shared](Shared/README.md)
Общие компоненты, используемые во всех модулях.

## Быстрый старт

1. **Импортируйте пакет** в ваш Unity проект
2. **Настройте конфигурацию** через Configuration модуль
3. **Инициализируйте Identity Controller**:

```csharp
var identityController = IdentityUIController.Instance;
await identityController.Initialize();
```

## Зависимости

- Unity 6000.0+
- UniTask
- Newtonsoft.Json
- Unity Localization
- Unity UI Toolkit

## Совместимость

- ✅ WebGL
- ✅ Windows Standalone
- ✅ macOS Standalone
- ✅ Linux Standalone
- ✅ Android
- ✅ iOS

## Документация

Подробная документация для каждого модуля доступна в соответствующих README файлах.
