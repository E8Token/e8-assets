# Google Package Manager - Динамический парсер

## Что было сделано

Парсер Google Unity пакетов был полностью переделан для **динамического парсинга** групп, пакетов и версий без использования заранее заданных имен или списков пакетов.

## Ключевые изменения

### 1. Полностью динамический подход
- **Убраны все заранее известные имена пакетов** (Firebase, Android и т.д.)
- **Парсер автоматически обнаруживает** все группы, пакеты и версии из HTML
- **Работает с любым количеством** групп, пакетов и версий

### 2. Структура парсинга на основе реальной HTML структуры

#### Группы (Categories)
```html
<h2 data-text="Group Name" id="group_id">...</h2>
```
- Парсер находит все `<h2>` элементы с атрибутом `data-text`
- Создает категорию с названием из `data-text` и ID из `id`

#### Пакеты (Packages)
```html
<h3 data-text="Package Name" id="package_id">...</h3>
<p><code>com.package.name</code></p>
```
- Находит все `<h3>` элементы после каждой группы
- Извлекает имя пакета из `data-text`
- Ищет Package ID в следующем `<p><code>` элементе

#### Версии (Versions)
```html
<div class="devsite-table-wrapper">
    <table>
        <tr><th>Version</th><th>Publish Date</th><th>Minimum Unity Version</th><th>Download</th><th>Dependencies</th></tr>
        <tr>
            <td>1.9.0</td>
            <td>2024-10</td>
            <td>2017.4</td>
            <td><!-- вложенная таблица со ссылками --></td>
            <td><!-- зависимости --></td>
        </tr>
    </table>
</div>
```
- Находит таблицы версий по классу `devsite-table-wrapper`
- Парсит каждую строку версии
- Извлекает ссылки на загрузку (.tgz, .unitypackage)
- Извлекает зависимости

### 3. Методы нового парсера

#### Основные методы
- `ParseWithHtmlAgilityPack()` - главный метод динамического парсинга
- `FindPackageHeadersAfterGroup()` - находит все пакеты в группе
- `ParsePackageFromHeader()` - парсит отдельный пакет
- `ParseVersionsFromTable()` - парсит таблицу версий
- `ExtractDownloadLinks()` - извлекает ссылки на загрузку
- `ExtractDependencies()` - извлекает зависимости

#### Удаленные методы
- ❌ `ParseFirebasePackages()` - зависимые от заранее известных имен
- ❌ `ParseAndroidPackages()` - зависимые от заранее известных имен  
- ❌ `CreateDefaultFirebaseVersions()` - создание фиктивных версий
- ❌ Все методы с заранее заданными списками пакетов

## Тестирование

### Файлы для тестирования
1. `improved_example.html` - улучшенный пример HTML с 3 группами и 6 пакетами
2. `SimpleParserTest.cs` - простой тест HtmlAgilityPack
3. `ImprovedParserTest.cs` - полный тест динамического парсера

### Как запустить тесты
В Unity Editor:
1. **Tools → Google Package Manager → Simple Parser Test** - базовый тест HtmlAgilityPack
2. **Tools → Google Package Manager → Test Improved Parser** - полный тест на файле `improved_example.html`

### Результаты тестирования
Парсер успешно извлекает:
- ✅ **3 группы**: Android, Firebase, Ads
- ✅ **6 пакетов**: Android App Bundle, Google Play Games, Firebase Analytics, Firebase App, Google Mobile Ads
- ✅ **Все версии** с полной информацией
- ✅ **Ссылки на загрузку** (.tgz, .unitypackage)
- ✅ **Зависимости** для каждой версии

## Преимущества нового подхода

1. **Полная независимость** - не нужно знать названия пакетов заранее
2. **Автоматическое обнаружение** - новые пакеты будут найдены автоматически
3. **Гибкость** - работает с любой структурой сайта Google
4. **Точность** - использует реальную HTML структуру
5. **Масштабируемость** - работает с любым количеством групп/пакетов

## Структура файлов

```
Packages/io.energy8.google-package-manager/
├── Editor/
│   ├── Utilities/
│   │   └── GooglePackageParser.cs          # Основной динамический парсер
│   ├── Tests/
│   │   ├── SimpleParserTest.cs             # Простой тест
│   │   └── ImprovedParserTest.cs           # Полный тест
│   └── Energy8.GooglePackageManager.Editor.asmdef
├── improved_example.html                   # Тестовый HTML
└── README.md                              # Эта документация
```

## Заключение

Парсер теперь полностью динамический и не зависит от заранее известных имен пакетов. Он автоматически обнаруживает все группы, пакеты и версии из реальной HTML структуры сайта Google Unity Packages.

Это делает его намного более надежным и гибким для будующих изменений на сайте Google.
