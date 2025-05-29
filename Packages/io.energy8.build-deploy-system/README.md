# Energy8 Build Deploy System

Система автоматизации сборки и деплоя для Unity проектов с поддержкой множественных профилей сборки, различных методов сжатия текстур для WebGL и SSH деплоя.

## Возможности

### Управление сборками
- **Множественные конфигурации сборки** с индивидуальными настройками
- **Автоматическое управление версиями** с поддержкой Major.Minor.Patch инкремента
- **Автоматическая установка Bundle ID** для Android и iOS
- **Поддержка различных профилей сборки** (Development, Release, etc.)

### WebGL специфичные возможности
- **Множественные методы сжатия текстур**: DXT, ASTC, ETC2
- **Автоматическое создание нескольких сборок** с разными методами сжатия
- **Переименование и копирование data файлов** для каждого метода сжатия

### Система деплоя
- **SSH деплой по ключу** с настраиваемыми параметрами
- **Автоматический деплой** после успешной сборки
- **Гибкие настройки подключения** (хост, порт, пути)

### Пользовательский интерфейс
- **Интеграция в Player Settings** для быстрого доступа
- **Отдельное окно управления** с детальными настройками
- **Логирование процессов** сборки и деплоя в реальном времени

## Установка

1. Пакет должен быть размещен в папке `Packages/io.energy8.build-deploy-system/`
2. Unity автоматически обнаружит и импортирует пакет
3. Откройте Player Settings для доступа к UI системы

## Использование

### Создание конфигурации сборки

1. Откройте **Player Settings**
2. Прокрутите вниз до секции **Build Deploy System**
3. Нажмите кнопку **"+"** для создания новой конфигурации
4. Настройте параметры конфигурации:
   - **Configuration Name**: Имя конфигурации
   - **Build Profile**: Профиль сборки (Development, Release, etc.)
   - **Build Target**: Целевая платформа
   - **Bundle ID**: Идентификатор приложения

### Настройка версий

1. В секции **Version Settings**:
   - **Version**: Текущая версия в формате Major.Minor.Patch
   - **Increment Type**: Тип инкремента (Major, Minor, Auto)
   - **Auto Increment**: Автоматический инкремент при сборке

2. Используйте кнопки быстрого инкремента:
   - **Increment Major**: Увеличить основную версию
   - **Increment Minor**: Увеличить минорную версию  
   - **Increment Patch**: Увеличить патч версию

### WebGL настройки

Для WebGL сборок доступны специальные настройки сжатия текстур:

1. Выберите один или несколько методов сжатия:
   - **DXT**: Для десктопных браузеров и старых мобильных устройств
   - **ASTC**: Для современных мобильных устройств
   - **ETC2**: Для Android и некоторых iOS устройств

2. При выборе нескольких методов система создаст отдельные сборки для каждого метода с соответствующими data файлами.

### Настройка деплоя

1. В секции **Deploy Settings**:
   - **Enable Deploy**: Включить функциональность деплоя
   - **Auto Deploy on Success**: Автоматический деплой после успешной сборки
   
2. Настройки SSH подключения:
   - **Host**: IP адрес или домен сервера
   - **Username**: Имя пользователя для SSH
   - **SSH Key Path**: Путь к приватному SSH ключу
   - **Remote Path**: Путь на сервере для размещения файлов
   - **Port**: Порт SSH (по умолчанию 22)

### Запуск сборки

1. В Player Settings или окне Build Deploy System:
   - **Build**: Обычная сборка
   - **Clean Build**: Сборка с очисткой папки билда
   - **Deploy Only**: Только деплой без сборки

### Расширенное управление

Откройте окно **Window → Energy8 → Build Deploy System** для:
- Детального управления конфигурациями
- Мониторинга процесса сборки
- Просмотра логов сборки и деплоя
- Настройки системы

## API для разработчиков

### Создание конфигурации программно

```csharp
using Energy8.BuildDeploySystem;

var config = ScriptableObject.CreateInstance<BuildConfiguration>();
config.configName = "My Configuration";
config.buildTarget = BuildTarget.WebGL;
config.version = "1.0.0";
config.webglCompressionMethods.Add(TextureCompressionMethod.DXT);
```

### Запуск сборки программно

```csharp
using Energy8.BuildDeploySystem;

// Запуск обычной сборки
BuildSystem.StartBuild(config, false);

// Запуск чистой сборки
BuildSystem.StartBuild(config, true);
```

### Подписка на события

```csharp
using Energy8.BuildDeploySystem;

BuildSystem.OnBuildStarted += (configName) => {
    Debug.Log($"Build started: {configName}");
};

BuildSystem.OnBuildCompleted += (configName, success) => {
    Debug.Log($"Build completed: {configName}, Success: {success}");
};

DeploymentSystem.OnDeployCompleted += (host, success) => {
    Debug.Log($"Deploy to {host}, Success: {success}");
};
```

## Структура файлов

```
Packages/io.energy8.build-deploy-system/
├── package.json
├── README.md
├── Runtime/
│   ├── Energy8.BuildDeploySystem.asmdef
│   ├── BuildConfiguration.cs
│   ├── BuildConfigurationManager.cs
│   ├── BuildSystem.cs
│   └── DeploymentSystem.cs
└── Editor/
    ├── Energy8.BuildDeploySystem.Editor.asmdef
    ├── BuildDeploySystemSettings.cs
    ├── BuildDeploySystemWindow.cs
    ├── BuildSystemEditor.cs
    ├── BuildConfigurationEditor.cs
    └── GUI/
        └── PlayerSettingsGUI.cs
```

## Требования

- Unity 2021.3 или выше
- Newtonsoft Json пакет
- SSH клиент (для деплоя)

## Поддержка

Для вопросов и поддержки обращайтесь к команде Energy8.
