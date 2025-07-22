# План реструктуризации пакета Build Deploy System

## Текущее состояние

Пакет `io.energy8.build-deploy-system` в настоящее время содержит все файлы в папке `Editor` после переноса из `Runtime`. Структура требует улучшения для лучшей организации кода и разделения ответственности.

### Текущие файлы:
- `BuildConfiguration.cs` - конфигурация сборки
- `GlobalVersion.cs` - управление версиями
- `PlatformSettings.cs` - настройки платформ
- `BuildManager.cs` - управление сборками
- `DeployManager.cs` - управление развертыванием
- `BuildDeployWindow.cs` - UI окно
- `BuildProfileScanner.cs` - сканирование профилей
- `BuildProfileWatcher.cs` - отслеживание изменений
- `DeployExtensions.cs` - расширения для развертывания

## Целевая структура

```
Packages/io.energy8.build-deploy-system/
├── Editor/
│   ├── Core/
│   │   ├── Interfaces/
│   │   │   ├── IBuildConfiguration.cs
│   │   │   ├── IBuildManager.cs
│   │   │   ├── IDeployManager.cs
│   │   │   ├── IVersionManager.cs
│   │   │   └── IPlatformSettings.cs
│   │   ├── Models/
│   │   │   ├── BuildConfiguration.cs
│   │   │   ├── GlobalVersion.cs
│   │   │   └── BuildResult.cs
│   │   ├── Services/
│   │   │   ├── BuildManager.cs
│   │   │   ├── DeployManager.cs
│   │   │   ├── VersionManager.cs
│   │   │   └── ProfileManager.cs
│   │   └── Utils/
│   │       ├── PathUtils.cs
│   │       ├── PlatformUtils.cs
│   │       └── ValidationUtils.cs
│   ├── Platform/
│   │   ├── Base/
│   │   │   ├── IPlatformSettings.cs
│   │   │   └── BasePlatformSettings.cs
│   │   ├── WebGL/
│   │   │   ├── WebGLSettings.cs
│   │   │   ├── WebGLBuilder.cs
│   │   │   └── WebGLCompressor.cs
│   │   ├── Mobile/
│   │   │   ├── AndroidSettings.cs
│   │   │   ├── IOSSettings.cs
│   │   │   └── MobileBuildUtils.cs
│   │   └── Standalone/
│   │       ├── StandaloneSettings.cs
│   │       └── StandaloneBuilder.cs
│   ├── Deploy/
│   │   ├── Base/
│   │   │   ├── IDeployProvider.cs
│   │   │   ├── BaseDeployProvider.cs
│   │   │   └── DeploySettings.cs
│   │   ├── Providers/
│   │   │   ├── FTPDeployProvider.cs
│   │   │   ├── SFTPDeployProvider.cs
│   │   │   └── LocalCopyDeployProvider.cs
│   │   └── Utils/
│   │       ├── DeployExtensions.cs
│   │       └── CompressionUtils.cs
│   ├── UI/
│   │   ├── Windows/
│   │   │   └── BuildDeployWindow.cs
│   │   ├── Components/
│   │   │   ├── VersionSection.cs
│   │   │   ├── ConfigurationSection.cs
│   │   │   ├── PlatformSection.cs
│   │   │   └── DeploySection.cs
│   │   └── Utils/
│   │       └── UIUtils.cs
│   ├── Monitoring/
│   │   ├── BuildProfileWatcher.cs
│   │   ├── BuildProfileScanner.cs
│   │   └── ConfigurationValidator.cs
│   └── Energy8.BuildDeploySystem.Editor.asmdef
├── README.md
├── Plan.md
└── package.json
```

## Этапы реструктуризации

### Этап 1: Создание базовой структуры папок

1. Создать папки:
   - `Core/Interfaces/`
   - `Core/Models/`
   - `Core/Services/`
   - `Core/Utils/`
   - `Platform/Base/`
   - `Platform/WebGL/`
   - `Platform/Mobile/`
   - `Platform/Standalone/`
   - `Deploy/Base/`
   - `Deploy/Providers/`
   - `Deploy/Utils/`
   - `UI/Windows/`
   - `UI/Components/`
   - `UI/Utils/`
   - `Monitoring/`

### Этап 2: Создание интерфейсов

1. **IBuildConfiguration.cs** - интерфейс для конфигурации сборки
2. **IBuildManager.cs** - интерфейс для управления сборками
3. **IDeployManager.cs** - интерфейс для управления развертыванием
4. **IVersionManager.cs** - интерфейс для управления версиями
5. **IPlatformSettings.cs** - базовый интерфейс для настроек платформ
6. **IDeployProvider.cs** - интерфейс для провайдеров развертывания

### Этап 3: Рефакторинг моделей данных

1. **BuildConfiguration.cs** - переместить в `Core/Models/`, очистить от логики
2. **GlobalVersion.cs** - переместить в `Core/Models/`, выделить логику в сервис
3. **BuildResult.cs** - новый класс для результатов сборки

### Этап 4: Разделение настроек платформ

1. **BasePlatformSettings.cs** - базовый класс для всех платформ
2. **WebGLSettings.cs** - переместить в `Platform/WebGL/`
3. **AndroidSettings.cs** - переместить в `Platform/Mobile/`
4. **IOSSettings.cs** - переместить в `Platform/Mobile/`
5. **StandaloneSettings.cs** - переместить в `Platform/Standalone/`
6. **WebGLBuilder.cs** - выделить логику сборки WebGL
7. **WebGLCompressor.cs** - выделить логику сжатия WebGL

### Этап 5: Реорганизация сервисов

1. **BuildManager.cs** - переместить в `Core/Services/`, разделить на методы
2. **DeployManager.cs** - переместить в `Core/Services/`, создать провайдеры
3. **VersionManager.cs** - новый сервис для управления версиями
4. **ProfileManager.cs** - новый сервис для управления профилями

### Этап 6: Создание провайдеров развертывания

1. **BaseDeployProvider.cs** - базовый класс для всех провайдеров
2. **FTPDeployProvider.cs** - провайдер для FTP
3. **SFTPDeployProvider.cs** - провайдер для SFTP
4. **LocalCopyDeployProvider.cs** - провайдер для локального копирования
5. **DeploySettings.cs** - переместить в `Deploy/Base/`

### Этап 7: Разделение UI компонентов

1. **BuildDeployWindow.cs** - переместить в `UI/Windows/`, упростить
2. **VersionSection.cs** - выделить секцию версий
3. **ConfigurationSection.cs** - выделить секцию конфигураций
4. **PlatformSection.cs** - выделить секцию платформ
5. **DeploySection.cs** - выделить секцию развертывания

### Этап 8: Создание утилит

1. **PathUtils.cs** - утилиты для работы с путями
2. **PlatformUtils.cs** - утилиты для работы с платформами
3. **ValidationUtils.cs** - утилиты для валидации
4. **UIUtils.cs** - утилиты для UI
5. **CompressionUtils.cs** - утилиты для сжатия

### Этап 9: Перенос мониторинга

1. **BuildProfileWatcher.cs** - переместить в `Monitoring/`
2. **BuildProfileScanner.cs** - переместить в `Monitoring/`
3. **ConfigurationValidator.cs** - новый класс для валидации

### Этап 10: Финальная оптимизация

1. Обновить все using директивы
2. Проверить зависимости между компонентами
3. Добавить XML документацию
4. Оптимизировать производительность
5. Добавить unit тесты (если требуется)

## Принципы реорганизации

### 1. Разделение ответственности (SRP)
- Каждый класс отвечает за одну задачу
- Логика сборки отделена от UI
- Настройки платформ изолированы

### 2. Инверсия зависимостей (DIP)
- Использование интерфейсов для основных компонентов
- Провайдеры развертывания реализуют общий интерфейс
- Сервисы зависят от абстракций

### 3. Открытость/Закрытость (OCP)
- Легко добавлять новые платформы
- Легко добавлять новые провайдеры развертывания
- Расширяемая архитектура

### 4. Группировка по функциональности
- Core - основная логика
- Platform - специфика платформ
- Deploy - развертывание
- UI - пользовательский интерфейс
- Monitoring - мониторинг и отслеживание

## Ожидаемые преимущества

1. **Лучшая организация кода** - логическое разделение по папкам
2. **Упрощенное тестирование** - изолированные компоненты
3. **Легкость расширения** - добавление новых платформ и провайдеров
4. **Улучшенная читаемость** - четкая структура и ответственность
5. **Повторное использование** - модульные компоненты
6. **Упрощенное сопровождение** - изолированные изменения

## Совместимость

Все изменения будут выполнены с сохранением обратной совместимости API. Публичные методы и свойства останутся доступными через facade паттерн или прямые ссылки.