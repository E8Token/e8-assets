# Changelog

Все значимые изменения в проекте Energy8 Build Deploy System будут документированы в этом файле.

Формат основан на [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
и этот проект придерживается [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-05-27

### Added
- Базовая система управления конфигурациями сборки
- Поддержка множественных профилей сборки
- Автоматическое управление версиями (Major.Minor.Patch)
- Автоматическая установка Bundle ID для Android и iOS
- WebGL специфичные возможности:
  - Поддержка множественных методов сжатия текстур (DXT, ASTC, ETC2)
  - Автоматическое создание отдельных сборок для каждого метода
  - Переименование и копирование data файлов
- Система SSH деплоя с поддержкой ключей
- Автоматический деплой после успешной сборки
- GUI интеграция в Player Settings
- Отдельное окно управления Build Deploy System
- Система логирования процессов сборки и деплоя
- Custom Inspector для BuildConfiguration
- События для отслеживания процесса сборки и деплоя
- Настройки системы с сохранением в ProjectSettings

### Technical Details
- Runtime assembly: Energy8.BuildDeploySystem
- Editor assembly: Energy8.BuildDeploySystem.Editor
- Зависимости: Unity.Nuget.Newtonsoft-Json
- Поддержка Unity 2021.3+

### File Structure
```
Runtime/
├── BuildConfiguration.cs - Конфигурация сборки
├── BuildConfigurationManager.cs - Менеджер конфигураций
├── BuildSystem.cs - Основная система сборки
└── DeploymentSystem.cs - Система деплоя

Editor/
├── BuildDeploySystemSettings.cs - Настройки системы
├── BuildDeploySystemWindow.cs - Главное окно управления
├── BuildSystemEditor.cs - Editor интеграция для сборки
├── BuildConfigurationEditor.cs - Custom Inspector
└── GUI/PlayerSettingsGUI.cs - GUI в Player Settings
```

### Known Issues
- Деплой работает только в Windows среде с установленным SSH клиентом
- WebGL множественная компрессия требует ручного управления data файлами в некоторых случаях

### Future Plans
- Поддержка FTP деплоя
- Интеграция с CI/CD системами
- Поддержка Steam деплоя
- Автоматическое создание архивов
- Поддержка pre/post build скриптов
