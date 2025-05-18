# План тестирования JSPluginTools

## Разработанные тесты

В рамках проекта разработаны следующие тесты:

### Edit Mode тесты:
1. `ExternalCommunicatorTests` - тесты для класса ExternalCommunicator
2. `JSMessageTests` - тесты для сериализации/десериализации сообщений
3. `JSMessageHandlerTests` - тесты для интерфейса IJSMessageHandler
4. `JSCallHandlersTests` - тесты для внутренних обработчиков JS вызовов

### Play Mode тесты:
1. `CommunicationPluginManagerTests` - интеграция Communication API с PluginManager
2. `JSIntegrationTests` - двусторонняя коммуникация между Unity и JavaScript
3. `EnvironmentTests` - проверка работы в разных средах (WebGL/редактор)

## Запуск тестов

### Запуск через Test Runner в Unity Editor:
1. Открыть окно Test Runner через Window > General > Test Runner
2. Запустить Edit Mode тесты
3. Запустить Play Mode тесты

### Командная строка (Unity Test Framework):
```powershell
# Запуск всех Edit Mode тестов
Unity.exe -batchmode -projectPath "E:\Projects\Energy8\e8-assets" -runTests -testPlatform EditMode -testCategory "JSPluginTools" -logFile editmode_test_results.log

# Запуск всех Play Mode тестов
Unity.exe -batchmode -projectPath "E:\Projects\Energy8\e8-assets" -runTests -testPlatform PlayMode -testCategory "JSPluginTools" -logFile playmode_test_results.log
```

### Запуск тестов на WebGL платформе:
1. Создать WebGL сборку проекта с тестами
2. Загрузить сборку на веб-сервер
3. Открыть сборку в браузере
4. Наблюдать результаты в консоли браузера

## Категории тестов

### Функциональные тесты:
- Проверка корректности отправки сообщений в JavaScript
- Проверка обработки сообщений от JavaScript
- Проверка сериализации/десериализации сообщений
- Проверка регистрации и инициализации модулей

### Интеграционные тесты:
- Проверка взаимодействия между PluginManager и модулями
- Проверка двусторонней коммуникации Unity-JavaScript
- Проверка работы коллбэков (обратных вызовов)

### Окружение:
- Проверка работы в редакторе Unity (симуляция)
- Проверка работы в WebGL сборке (реальное взаимодействие)

## Анализ покрытия кода

Для анализа покрытия кода тестами можно использовать:
1. CodeCoverage пакет из Unity Package Manager
2. Встроенное покрытие кода в Unity Test Framework

Команда для запуска с покрытием:
```powershell
Unity.exe -batchmode -projectPath "E:\Projects\Energy8\e8-assets" -runTests -testPlatform EditMode -testCategory "JSPluginTools" -enableCodeCoverage -coverageResultsPath Coverage -coverageOptions generateAdditionalMetrics;generateHtmlReport;generateBadgeReport
```

## Важные замечания

- Для максимального покрытия следует запускать тесты как в редакторе, так и в WebGL сборке
- Некоторые тесты специфичны для WebGL и будут пропущены в редакторе
- Системные исключения и ошибки следует проверять в журнале Unity
- Для тестирования WebGL интеграции необходимо проверять консоль браузера
