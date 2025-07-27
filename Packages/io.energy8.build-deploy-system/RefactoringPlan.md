# План рефакторинга пакета Build Deploy System

## Обзор

Данный документ описывает план поэтапного рефакторинга пакета `io.energy8.build-deploy-system` с целью улучшения архитектуры, разделения ответственности и повышения maintainability кода.

## Текущая структура

```
Packages/io.energy8.build-deploy-system/
├── Editor/
│   ├── BuildConfiguration.cs (224 строки)
│   ├── BuildDeployWindow.cs (830 строк)
│   ├── BuildManager.cs (845 строк)
│   ├── BuildProfileScanner.cs (185 строк)
│   ├── BuildProfileWatcher.cs (78 строк)
│   ├── DeployExtensions.cs (133 строки)
│   ├── DeployManager.cs (625 строк)
│   ├── GlobalVersion.cs (186 строк)
│   └── PlatformSettings.cs (392 строки)
├── Plan.md
├── README.md
└── package.json
```

### Анализ текущих файлов

#### BuildConfiguration.cs
- **Ответственность**: Конфигурация сборки, ссылки на Build Profile, настройки платформ
- **Проблемы**: Смешивает данные и логику, содержит Editor-specific код
- **Размер**: 224 строки

#### BuildDeployWindow.cs
- **Ответственность**: UI для управления сборками и развертыванием
- **Проблемы**: Монолитный класс, смешивает UI логику с бизнес-логикой
- **Размер**: 830 строк (критически большой)

#### BuildManager.cs
- **Ответственность**: Управление процессом сборки, обработка платформ
- **Проблемы**: Слишком много ответственности, сложная логика в одном классе
- **Размер**: 845 строк (критически большой)

#### DeployManager.cs
- **Ответственность**: Управление развертыванием через FTP/SFTP/LocalCopy
- **Проблемы**: Все провайдеры в одном классе, сложная логика
- **Размер**: 625 строк (большой)

#### PlatformSettings.cs
- **Ответственность**: Настройки для всех платформ и развертывания
- **Проблемы**: Смешивает настройки разных платформ в одном файле
- **Размер**: 392 строки

#### Остальные файлы
- **BuildProfileScanner.cs**: Сканирование Build Profiles (185 строк)
- **BuildProfileWatcher.cs**: Отслеживание изменений (78 строк)
- **DeployExtensions.cs**: Расширения для SSH/FTP (133 строки)
- **GlobalVersion.cs**: Управление версиями (186 строк)

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
│   │   │   ├── BuildResult.cs
│   │   │   └── BuildContext.cs
│   │   ├── Services/
│   │   │   ├── BuildManager.cs
│   │   │   ├── VersionManager.cs
│   │   │   └── ProfileManager.cs
│   │   └── Utils/
│   │       ├── PathUtils.cs
│   │       ├── PlatformUtils.cs
│   │       └── ValidationUtils.cs
│   ├── Platform/
│   │   ├── Base/
│   │   │   ├── IPlatformSettings.cs
│   │   │   ├── BasePlatformSettings.cs
│   │   │   └── IPlatformBuilder.cs
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
│   │   ├── Utils/
│   │   │   ├── DeployExtensions.cs
│   │   │   └── CompressionUtils.cs
│   │   └── DeployManager.cs
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
│   ├── Version/
│   │   ├── GlobalVersion.cs
│   │   └── VersionManager.cs
│   └── Energy8.BuildDeploySystem.Editor.asmdef
├── README.md
├── RefactoringPlan.md
└── package.json
```

## Поэтапный план рефакторинга

### Этап 1: Создание структуры папок и перемещение файлов ✅

**Цель**: Создать новую структуру папок и переместить существующие файлы без изменения их содержимого.

**Статус**: Завершен

**Действия**:
1. ✅ Создать все необходимые папки
2. ✅ Переместить файлы в соответствующие папки:
   - ✅ `BuildConfiguration.cs` → `Core/Models/`
   - ✅ `PlatformSettings.cs` → `Platform/Base/`
   - ✅ `BuildManager.cs` → `Core/Services/`
   - ✅ `DeployManager.cs` → `Deploy/Base/`
   - ✅ `GlobalVersion.cs` → `Version/`
   - ✅ `BuildProfileScanner.cs` → `Monitoring/`
   - ✅ `BuildProfileWatcher.cs` → `Monitoring/`
   - ✅ `DeployExtensions.cs` → `Deploy/Utils/`
   - ✅ `BuildDeployWindow.cs` → `UI/Windows/`
3. ⏳ Обновить namespace и using директивы (требует проверки)
4. ⏳ Создать assembly definition файл (требует проверки)

**Критерий завершения**: ⏳ Проект компилируется без ошибок, все функции работают как раньше (требует проверки).

### Этап 2: Разделение PlatformSettings.cs ✅

**Цель**: Разделить монолитный файл PlatformSettings.cs на отдельные файлы по платформам.

**Статус**: Завершен

**Действия**:
1. ✅ Создать `Platform/Base/IPlatformSettings.cs` - базовый интерфейс
2. ✅ Создать `Platform/Base/BasePlatformSettings.cs` - базовый класс
3. ✅ Переместить `WebGLSettings` в `Platform/WebGL/WebGLSettings.cs`
4. ✅ Переместить `AndroidSettings` в `Platform/Mobile/AndroidSettings.cs`
5. ✅ Переместить `IOSSettings` в `Platform/Mobile/IOSSettings.cs`
6. ✅ Переместить `StandaloneSettings` в `Platform/Standalone/StandaloneSettings.cs`
7. ✅ Переместить `DeploySettings` в `Deploy/Base/DeploySettings.cs`
8. ✅ Переместить enums (`DeployMethod`, `AuthenticationMethod`, `CompressionAlgorithm`) в соответствующие файлы
9. ✅ Обновить ссылки в `BuildConfiguration.cs`
10. ✅ Создать файл совместимости с type aliases

**Критерий завершения**: ⏳ Все настройки платформ разделены, проект компилируется, функциональность сохранена (требует проверки).

### Этап 3: Разделение BuildManager.cs

**Цель**: Разделить монолитный BuildManager на более мелкие, специализированные классы.

**Действия**:
1. Создать `Core/Interfaces/IBuildManager.cs`
2. Создать `Core/Models/BuildResult.cs` и `BuildContext.cs`
3. Создать `Core/Utils/PathUtils.cs` - вынести логику обработки путей
4. Создать `Core/Utils/PlatformUtils.cs` - вынести утилиты платформ
5. Создать `Platform/WebGL/WebGLBuilder.cs` - вынести WebGL-специфичную логику
6. Создать `Platform/WebGL/WebGLCompressor.cs` - вынести логику сжатия
7. Создать `Platform/Mobile/MobileBuildUtils.cs` - вынести мобильную логику
8. Создать `Platform/Standalone/StandaloneBuilder.cs` - вынести desktop логику
9. Рефакторить основной `BuildManager.cs` для использования новых классов

**Критерий завершения**: BuildManager стал значительно меньше, логика разделена по ответственности, все сборки работают.

### Этап 4: Разделение DeployManager.cs

**Цель**: Создать систему провайдеров для развертывания.

**Действия**:
1. Создать `Deploy/Base/IDeployProvider.cs`
2. Создать `Deploy/Base/BaseDeployProvider.cs`
3. Создать `Deploy/Providers/FTPDeployProvider.cs` - вынести FTP логику
4. Создать `Deploy/Providers/SFTPDeployProvider.cs` - вынести SFTP логику
5. Создать `Deploy/Providers/LocalCopyDeployProvider.cs` - вынести локальное копирование
6. Создать `Deploy/Utils/CompressionUtils.cs` - вынести утилиты сжатия
7. Рефакторить `DeployManager.cs` для использования провайдеров

**Критерий завершения**: Система развертывания работает через провайдеры, легко добавлять новые методы.

### Этап 5: Разделение BuildDeployWindow.cs

**Цель**: Разделить монолитное UI окно на компоненты.

**Действия**:
1. Создать `UI/Utils/UIUtils.cs` - общие UI утилиты
2. Создать `UI/Components/VersionSection.cs` - секция управления версиями
3. Создать `UI/Components/ConfigurationSection.cs` - секция конфигураций
4. Создать `UI/Components/PlatformSection.cs` - секция настроек платформ
5. Создать `UI/Components/DeploySection.cs` - секция развертывания
6. Рефакторить `BuildDeployWindow.cs` для использования компонентов

**Критерий завершения**: UI окно стало модульным, каждая секция независима.

### Этап 6: Создание сервисов и интерфейсов

**Цель**: Добавить слой абстракции и создать сервисы.

**Действия**:
1. Создать все интерфейсы в `Core/Interfaces/`
2. Создать `Version/VersionManager.cs` - сервис управления версиями
3. Создать `Core/Services/ProfileManager.cs` - сервис управления профилями
4. Создать `Core/Utils/ValidationUtils.cs` - утилиты валидации
5. Создать `Monitoring/ConfigurationValidator.cs` - валидатор конфигураций
6. Обновить существующие классы для реализации интерфейсов

**Критерий завершения**: Архитектура стала более гибкой, добавлены интерфейсы.

### Этап 7: Финальная оптимизация и очистка

**Цель**: Оптимизировать код и добавить документацию.

**Действия**:
1. Обновить все using директивы
2. Добавить XML документацию ко всем публичным методам
3. Оптимизировать производительность
4. Проверить и исправить все предупреждения компилятора
5. Обновить README.md с новой архитектурой
6. Создать assembly definition файл

**Критерий завершения**: Код полностью документирован, оптимизирован, готов к использованию.

## Принципы рефакторинга

### 1. Сохранение работоспособности
- На каждом этапе проект должен компилироваться
- Все существующие функции должны работать
- Изменения только внутренней структуры, API остается стабильным

### 2. Разделение ответственности (SRP)
- Каждый класс отвечает за одну задачу
- Логика сборки отделена от UI
- Настройки платформ изолированы

### 3. Инверсия зависимостей (DIP)
- Использование интерфейсов для основных компонентов
- Провайдеры развертывания реализуют общий интерфейс
- Сервисы зависят от абстракций

### 4. Открытость/Закрытость (OCP)
- Легко добавлять новые платформы
- Легко добавлять новые провайдеры развертывания
- Расширяемая архитектура

### 5. Группировка по функциональности
- **Core** - основная бизнес-логика
- **Platform** - специфика платформ
- **Deploy** - логика развертывания
- **UI** - пользовательский интерфейс
- **Monitoring** - отслеживание и валидация
- **Version** - управление версиями

## Ожидаемые результаты

### Улучшения архитектуры
- Четкое разделение ответственности
- Модульная структура
- Легкость расширения
- Улучшенная тестируемость

### Улучшения maintainability
- Файлы стали меньше и понятнее
- Легче найти нужный код
- Проще добавлять новые функции
- Меньше конфликтов при работе в команде

### Улучшения производительности
- Более эффективная компиляция
- Лучшее кэширование Unity
- Оптимизированные зависимости

## Риски и митигация

### Риск: Поломка существующей функциональности
**Митигация**: Тщательное тестирование на каждом этапе, сохранение API

### Риск: Увеличение сложности
**Митигация**: Четкая документация, логичная структура папок

### Риск: Проблемы с зависимостями
**Митигация**: Постепенное изменение, проверка компиляции на каждом шаге

## Заключение

Данный план рефакторинга позволит значительно улучшить архитектуру пакета Build Deploy System, сделав его более maintainable, расширяемым и производительным, при этом сохранив всю существующую функциональность.