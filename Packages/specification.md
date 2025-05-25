# Техническое задание: Unity WebGL Plugin Platform

## 1. Обзор проекта

Создание Unity Package, представляющего собой платформу для разработки WebGL плагинов, улучшающих и упрощающих связь между Unity и браузером.

### Цель
Разработать базовую архитектуру (модуль Core) с PluginManager и базовыми классами для создания кастомных WebGL плагинов с единообразным API.

### Основные компоненты
- Core модуль с PluginManager
- Базовые классы для плагинов  
- GUI для управления плагинами
- Система регистрации и настройки плагинов

## 2. Архитектура Core модуля

### 2.1 PluginManager
Центральный менеджер для управления всеми WebGL плагинами:

**Основные функции:**
- Регистрация и активация/деактивация плагинов
- Управление приоритетами загрузки плагинов (0-100, где 0 - наивысший приоритет)
- Обработка JSON-сообщений между Unity и браузером
- Маршрутизация вызовов методов через атрибуты
- Система callback'ов для асинхронного общения

**Формат общения:**
- JSON для передачи данных
- Поддержка прямых вызовов методов через атрибуты [JSCallable]
- Система событий с callback'ами для асинхронных операций

### 2.2 Базовые классы плагинов

**BasePlugin (абстрактный класс):**
- Методы Initialize(), Enable(), Disable(), Destroy()
- Приоритет загрузки (Priority property)
- Название и версия плагина
- Система настроек (Settings)

**IPluginSettings (интерфейс):**
- Базовый интерфейс для настроек плагинов
- Сериализация в JSON

**JSCallableAttribute:**
- Атрибут для маркировки методов, доступных из JS
- Поддержка параметров и возвращаемых значений

## 3. Структура файлов кастомных плагинов

### 3.1 Структура папки плагина
Каждый плагин размещается в отдельной папке с обязательной структурой:

```
MyCustomPlugin/
├── MyCustomPlugin.cs                 // Основной класс плагина (наследует BasePlugin)
├── MyCustomPluginSettings.cs         // Настройки плагина (реализует IPluginSettings)
├── MyCustomPlugin.jslib               // JS библиотека с функциями плагина
├── MyCustomPlugin.jspre               // Pre-файл для неймспейсов (опционально)
└── package.json                       // Метаданные плагина
```

### 3.2 Обязательные файлы

**package.json** - метаданные плагина:
```json
{
  "name": "MyCustomPlugin",
  "displayName": "My Custom Plugin",
  "version": "1.0.0",
  "description": "Description of plugin functionality",
  "unity": "2021.3",
  "author": "Author Name",
  "priority": 50
}
```

**MyCustomPlugin.cs** - основной класс:
- Наследует BasePlugin
- Реализует основные методы жизненного цикла
- Содержит методы с атрибутом [JSCallable]

**MyCustomPluginSettings.cs** - настройки:
- Реализует IPluginSettings
- ScriptableObject для сохранения настроек
- Атрибут [CreateAssetMenu] для создания через контекстное меню

**MyCustomPlugin.jslib** - JavaScript код:
- Функции в стиле ES5
- Использование неймспейсов для изоляции
- Методы для общения с Unity через JSON

## 4. GUI для управления плагинами

### 4.1 Расположение в Editor
Окно управления плагинами интегрировано в Project Settings:
- Путь: **Edit > Project Settings > WebGL Plugin Manager**
- Отдельная вкладка в окне Project Settings

### 4.2 Функционал GUI

**Основные секции:**
1. **Список плагинов** - таблица со столбцами:
   - Название плагина
   - Версия
   - Статус (Enabled/Disabled)
   - Приоритет загрузки
   - Кнопки действий (Enable/Disable/Settings)

2. **Панель настроек плагина**:
   - Отображается при выборе плагина
   - Автоматически генерируется на основе Settings класса
   - Кнопки Save/Reset для настроек

3. **Панель информации**:
   - Описание выбранного плагина
   - Версия Unity
   - Автор
   - Зависимости (если есть)

**Дополнительные функции:**
- Поиск плагинов по имени
- Фильтрация по статусу (All/Enabled/Disabled)
- Сортировка по приоритету/имени
- Кнопка "Refresh" для обновления списка плагинов

## 5. Технические требования системы коммуникации

### 5.1 JSON Протокол
Стандартизированный формат сообщений между Unity и JavaScript:

**Структура сообщения от JS к Unity:**
```json
{
  "plugin": "PluginName",
  "method": "MethodName", 
  "data": { /* параметры */ },
  "callbackId": "unique_id" // для асинхронных вызовов
}
```

**Структура ответа от Unity к JS:**
```json
{
  "success": true,
  "data": { /* результат */ },
  "callbackId": "unique_id",
  "error": "error_message" // при ошибке
}
```

### 5.2 Система Callback'ов
- Автоматическая генерация уникальных ID для асинхронных вызовов
- Таймауты для callback'ов (настраиваемые)
- Обработка ошибок и исключений
- Поддержка Promise-подобного API в JavaScript

### 5.3 Атрибуты для методов
**[JSCallable]** - основной атрибут для экспорта методов:
- Автоматическая регистрация в PluginManager
- Валидация параметров
- Поддержка async/await методов
- Автоматическая сериализация/десериализация JSON

## 6. Примеры использования API

### 6.1 Пример плагина на C#
```csharp
public class StoragePlugin : BasePlugin
{
    [JSCallable]
    public string GetItem(string key)
    {
        // Получение данных из хранилища
        return PlayerPrefs.GetString(key);
    }
    
    [JSCallable]
    public async Task<bool> SetItemAsync(string key, string value)
    {
        // Асинхронное сохранение
        PlayerPrefs.SetString(key, value);
        return true;
    }
}
```

### 6.2 Пример JavaScript кода
```javascript
// В .jslib файле
var StorageNamespace = {
    Storage_GetFromUnity: function(key) {
        var keyStr = UTF8ToString(key);
        // Вызов Unity метода через PluginManager
        return PluginManager.call('StoragePlugin', 'GetItem', {key: keyStr});
    },
    
    Storage_SetToUnity: function(key, value, callback) {
        var keyStr = UTF8ToString(key);
        var valueStr = UTF8ToString(value);
        // Асинхронный вызов с callback
        PluginManager.callAsync('StoragePlugin', 'SetItemAsync', 
            {key: keyStr, value: valueStr}, callback);
    }
};

mergeInto(LibraryManager.library, StorageNamespace);
```
