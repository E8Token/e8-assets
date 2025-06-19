using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using HtmlAgilityPack;
using Energy8.GooglePackageManager.Data;

namespace Energy8.GooglePackageManager.Utilities
{
    public class GooglePackageParser
    {
        private const string GOOGLE_PACKAGES_URL = "https://developers.google.com/unity/archive";
        // Настройка для детального логирования
        private static bool EnableDetailedLogging => GooglePackageSettings.Instance.enableDebugLogging;

        public async Task<GooglePackageDatabase> ParsePackagesAsync()
        {
            try
            {
                Debug.Log("Starting to parse Google Unity Packages...");
                var database = new GooglePackageDatabase();

                // Загружаем HTML контент
                string htmlContent = await DownloadWebPageAsync(GOOGLE_PACKAGES_URL);

                if (string.IsNullOrEmpty(htmlContent))
                {
                    Debug.LogError("Failed to download Google Unity Packages page");
                    return database;
                }

                if (EnableDetailedLogging)
                    Debug.Log($"Downloaded webpage content, length: {htmlContent.Length} characters");

                // Парсим динамически с HtmlAgilityPack
                ParseWithHtmlAgilityPack(htmlContent, database);

                database.lastUpdateCheck = DateTime.Now;

                // Итоговая сводка
                int totalPackages = GetTotalPackagesCount(database);
                Debug.Log($"✅ Parsing completed: {database.categories.Count} categories, {totalPackages} packages total");

                if (EnableDetailedLogging)
                {
                    foreach (var category in database.categories)
                    {
                        Debug.Log($"  📁 {category.displayName}: {category.packages.Count} packages");
                    }
                }

                return database;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing Google Unity Packages: {ex.Message}");
                return new GooglePackageDatabase();
            }
        }
        private async Task<string> DownloadWebPageAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError($"Failed to download {url}: {request.error}");
                    return null;
                }
            }
        }
        private void ParseWithHtmlAgilityPack(string htmlContent, GooglePackageDatabase database)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                // Находим все группы (h2 элементы с data-text атрибутом)
                var groupHeaders = doc.DocumentNode.SelectNodes("//h2[@data-text]");

                if (groupHeaders == null || groupHeaders.Count == 0)
                {
                    Debug.LogWarning("No group headers found in HTML");

                    if (EnableDetailedLogging)
                    {
                        // Альтернативный поиск групп для отладки
                        var allH2 = doc.DocumentNode.SelectNodes("//h2");
                        if (allH2 != null)
                        {
                            Debug.Log($"Found {allH2.Count} h2 elements without data-text");
                            foreach (var h2 in allH2)
                            {
                                Debug.Log($"H2: {h2.OuterHtml}");
                            }
                        }
                    }
                    return;
                }

                if (EnableDetailedLogging)
                    Debug.Log($"Found {groupHeaders.Count} group headers");

                foreach (var groupHeader in groupHeaders)
                {
                    try
                    {
                        var groupName = groupHeader.GetAttributeValue("data-text", "");
                        var groupId = groupHeader.GetAttributeValue("id", "");

                        if (EnableDetailedLogging)
                        {
                            Debug.Log($"Processing group: '{groupName}' (ID: '{groupId}')");
                        }

                        if (string.IsNullOrEmpty(groupName))
                        {
                            if (EnableDetailedLogging)
                                Debug.LogWarning("Skipping group with empty name");
                            continue;
                        }

                        var category = new GooglePackageCategory(groupId, groupName);

                        // Находим все пакеты в этой группе (h3 элементы после h2)
                        var packageHeaders = FindPackageHeadersAfterGroup(groupHeader);

                        if (EnableDetailedLogging)
                            Debug.Log($"Found {packageHeaders.Count} package headers for group '{groupName}'");

                        foreach (var packageHeader in packageHeaders)
                        {
                            var package = ParsePackageFromHeader(packageHeader);
                            if (package != null)
                            {
                                package.category = groupName;
                                category.packages.Add(package);

                                if (EnableDetailedLogging)
                                    Debug.Log($"  Added package: {package.displayName} ({package.packageName})");
                            }
                            else if (EnableDetailedLogging)
                            {
                                Debug.LogWarning($"  Failed to parse package from header");
                            }
                        }

                        if (category.packages.Count > 0)
                        {
                            database.categories.Add(category);
                        }
                        else if (EnableDetailedLogging)
                        {
                            Debug.LogWarning($"No packages found for category '{groupName}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing group: {ex.Message}");
                        if (EnableDetailedLogging)
                            Debug.LogError($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ParseWithHtmlAgilityPack: {ex.Message}");
                if (EnableDetailedLogging)
                    Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
        private List<HtmlNode> FindPackageHeadersAfterGroup(HtmlNode groupHeader)
        {
            var packageHeaders = new List<HtmlNode>();

            // Используем XPath для поиска всех h3 элементов после текущей группы
            var allH3Elements = groupHeader.OwnerDocument.DocumentNode.SelectNodes("//h3[@data-text]");

            if (allH3Elements == null)
            {
                if (EnableDetailedLogging)
                    Debug.Log("No h3 elements with data-text found in document");
                return packageHeaders;
            }

            // Получаем позицию текущей группы в документе
            int groupPosition = groupHeader.StreamPosition;

            // Ищем следующую группу для определения границ
            var nextGroupPosition = int.MaxValue;
            var allGroups = groupHeader.OwnerDocument.DocumentNode.SelectNodes("//h2[@data-text]");

            if (allGroups != null)
            {
                foreach (var group in allGroups)
                {
                    if (group.StreamPosition > groupPosition && group.StreamPosition < nextGroupPosition)
                    {
                        nextGroupPosition = group.StreamPosition;
                    }
                }
            }

            if (EnableDetailedLogging)
                Debug.Log($"Group position: {groupPosition}, next group position: {nextGroupPosition}");            // Находим все h3 элементы между текущей и следующей группой
            foreach (var h3 in allH3Elements)
            {
                if (h3.StreamPosition > groupPosition && h3.StreamPosition < nextGroupPosition)
                {
                    var packageName = h3.GetAttributeValue("data-text", "");
                    if (EnableDetailedLogging)
                        Debug.Log($"Found package header: '{packageName}' at position {h3.StreamPosition}");
                    packageHeaders.Add(h3);
                }
            }

            return packageHeaders;
        }

        private GooglePackageInfo ParsePackageFromHeader(HtmlNode packageHeader)
        {
            try
            {
                var packageName = packageHeader.GetAttributeValue("data-text", "");
                var packageId = packageHeader.GetAttributeValue("id", "");

                if (EnableDetailedLogging)
                {
                    Debug.Log($"Parsing package: '{packageName}' (ID: '{packageId}')");
                }

                if (string.IsNullOrEmpty(packageName))
                {
                    if (EnableDetailedLogging)
                        Debug.LogWarning("Empty package name, skipping");
                    return null;
                }
                var package = new GooglePackageInfo
                {
                    displayName = packageName,
                    availableVersions = new List<GooglePackageVersion>()
                };

                // Собираем все следующие элементы до следующего заголовка
                var followingElements = new List<HtmlNode>();
                var currentNode = packageHeader.NextSibling;

                while (currentNode != null)
                {
                    if (currentNode.NodeType == HtmlNodeType.Element)
                    {
                        // Останавливаемся на следующем заголовке
                        if (currentNode.Name.ToLower() == "h2" || currentNode.Name.ToLower() == "h3")
                        {
                            if (EnableDetailedLogging)
                                Debug.Log($"Found next header ({currentNode.Name}), stopping search");
                            break;
                        }

                        followingElements.Add(currentNode);
                    }
                    currentNode = currentNode.NextSibling;
                }

                // Ищем package ID среди следующих элементов
                bool foundPackageId = false;

                foreach (var element in followingElements)
                {
                    if (element.Name.ToLower() == "p")
                    {
                        var codeNodes = element.SelectNodes(".//code");
                        if (codeNodes != null)
                        {
                            foreach (var codeNode in codeNodes)
                            {
                                var text = codeNode.InnerText.Trim();

                                if (EnableDetailedLogging)
                                    Debug.Log($"Code content: '{text}'");

                                // Ищем что-то похожее на package ID (com.* или содержащее точку)
                                if (text.Contains(".") && (text.StartsWith("com.") || text.Contains("google") || text.Contains("firebase")))
                                {
                                    package.packageName = text;
                                    if (EnableDetailedLogging)
                                        Debug.Log($"Found package ID: '{text}'");
                                    foundPackageId = true;
                                    break;
                                }
                            }
                        }

                        if (foundPackageId) break;
                    }
                }

                if (!foundPackageId)
                {
                    // Генерируем ID на основе имени
                    package.packageName = "com.google." + packageName.ToLower().Replace(" ", "").Replace("-", "");
                    if (EnableDetailedLogging)
                        Debug.Log($"Generated package ID: '{package.packageName}'");
                }

                // Ищем таблицу версий среди следующих элементов
                bool foundVersionTable = false;

                foreach (var element in followingElements)
                {
                    if (element.Name.ToLower() == "div")
                    {
                        var classes = element.GetClasses();

                        if (classes.Contains("devsite-table-wrapper"))
                        {
                            var table = element.SelectSingleNode(".//table");
                            if (table != null)
                            {
                                ParseVersionsFromTable(table, package);
                                foundVersionTable = true;
                                break;
                            }
                        }
                    }
                    else if (element.Name.ToLower() == "table")
                    {
                        ParseVersionsFromTable(element, package);
                        foundVersionTable = true;
                        break;
                    }
                }

                if (!foundVersionTable && EnableDetailedLogging)
                {
                    Debug.LogWarning($"Version table not found for '{packageName}'");
                }                // Устанавливаем последнюю версию как основную
                if (package.availableVersions.Count > 0)
                {
                    var latestVersion = package.availableVersions.First();
                    package.version = latestVersion.version;
                    package.minimumUnityVersion = latestVersion.minimumUnityVersion;
                    package.publishDate = latestVersion.publishDate;
                    package.downloadUrlTgz = latestVersion.downloadUrlTgz;
                }

                if (EnableDetailedLogging)
                {
                    Debug.Log($"Package parsing complete: '{package.displayName}' ({package.packageName}) - {package.availableVersions.Count} versions");
                }

                return package;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing package from header: {ex.Message}");
                if (EnableDetailedLogging)
                    Debug.LogError($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private void ParseVersionsFromTable(HtmlNode table, GooglePackageInfo package)
        {
            try
            {
                var rows = table.SelectNodes(".//tr");
                if (rows == null || rows.Count <= 1) // Пропускаем заголовок
                {
                    if (EnableDetailedLogging)
                        Debug.LogWarning($"No data rows found in table (total rows: {rows?.Count ?? 0})");
                    return;
                }                // Анализируем заголовок для понимания структуры (только при детальном логировании)
                if (EnableDetailedLogging && rows.Count > 0)
                {
                    var headerRow = rows[0];
                    var headerCells = headerRow.SelectNodes(".//th | .//td");
                    if (headerCells != null)
                    {
                        Debug.Log($"Header structure ({headerCells.Count} columns):");
                        for (int i = 0; i < headerCells.Count; i++)
                        {
                            Debug.Log($"  Column {i}: '{headerCells[i].InnerText.Trim()}'");
                        }
                    }
                }

                for (int i = 1; i < rows.Count; i++) // Пропускаем заголовок
                {
                    var row = rows[i];
                    var cells = row.SelectNodes(".//td");

                    if (cells == null || cells.Count < 3)
                    {
                        if (EnableDetailedLogging)
                            Debug.LogWarning($"Row {i} has insufficient cells: {cells?.Count ?? 0}");
                        continue;
                    }

                    try
                    {
                        var version = new GooglePackageVersion();

                        // Извлекаем данные из ячеек
                        if (cells.Count > 0)
                        {
                            version.version = cells[0].InnerText.Trim();
                        }

                        if (cells.Count > 1)
                        {
                            version.publishDate = cells[1].InnerText.Trim();
                        }

                        if (cells.Count > 2)
                        {
                            version.minimumUnityVersion = cells[2].InnerText.Trim();
                        }

                        // Извлекаем ссылки на загрузку из 4-й колонки (если есть)
                        if (cells.Count > 3)
                        {
                            ExtractDownloadLinks(cells[3], version);
                        }

                        // Извлекаем зависимости из 5-й колонки (если есть)
                        if (cells.Count > 4)
                        {
                            version.dependencies = ExtractDependencies(cells[4]);
                        }
                        else
                        {
                            version.dependencies = "None";
                        }

                        // Проверяем, что у нас есть минимальные данные
                        if (!string.IsNullOrEmpty(version.version))
                        {
                            package.availableVersions.Add(version);
                            if (EnableDetailedLogging)
                                Debug.Log($"Added version: {version.version} ({version.publishDate})");
                        }
                        else if (EnableDetailedLogging)
                        {
                            Debug.LogWarning($"Skipping row {i} - empty version");
                        }
                    }
                    catch (Exception rowEx)
                    {
                        if (EnableDetailedLogging)
                            Debug.LogWarning($"Error parsing row {i}: {rowEx.Message}");
                    }
                }

                if (EnableDetailedLogging)
                    Debug.Log($"Total versions extracted: {package.availableVersions.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing versions table: {ex.Message}");
                if (EnableDetailedLogging)
                    Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ExtractDownloadLinks(HtmlNode downloadCell, GooglePackageVersion version)
        {
            try
            {
                // Ищем все ссылки в ячейке
                var links = downloadCell.SelectNodes(".//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", "");
                        var text = link.InnerText.Trim();

                        if (EnableDetailedLogging)
                            Debug.Log($"Link: '{text}' -> '{href}'");

                        if (href.EndsWith(".tgz"))
                        {
                            version.downloadUrlTgz = href;
                        }
                        else if (href.EndsWith(".unitypackage"))
                        {
                            version.downloadUrlUnityPackage = href;
                        }
                        else if (href.Contains("download") || href.Contains("package"))
                        {
                            // Возможно, это ссылка на скачивание без явного расширения
                            if (string.IsNullOrEmpty(version.downloadUrlTgz))
                            {
                                version.downloadUrlTgz = href;
                            }
                        }
                    }
                }
                else if (EnableDetailedLogging)
                {
                    Debug.LogWarning("No links found in download cell");
                }

                // Также ищем прямые URL в тексте ячейки (только при детальном логировании)
                if (EnableDetailedLogging)
                {
                    var cellText = downloadCell.InnerText.Trim();
                    if (cellText.Contains("http"))
                    {
                        Debug.Log($"Cell contains URL text: '{cellText}'");
                    }
                }
            }
            catch (Exception ex)
            {
                if (EnableDetailedLogging)
                    Debug.LogWarning($"Error extracting download links: {ex.Message}");
            }
        }

        private string ExtractDependencies(HtmlNode dependenciesCell)
        {
            try
            {
                var dependencies = new List<string>();
                var cells = dependenciesCell.SelectNodes(".//td");

                if (cells != null)
                {
                    foreach (var cell in cells)
                    {
                        var text = cell.InnerText.Trim();
                        if (!string.IsNullOrEmpty(text) && text != "None")
                        {
                            dependencies.Add(text);
                        }
                    }
                }

                return dependencies.Count > 0 ? string.Join(", ", dependencies) : "None";
            }
            catch
            {
                return "None";
            }
        }

        private int GetTotalPackagesCount(GooglePackageDatabase database)
        {
            int count = 0; foreach (var category in database.categories)
            {
                count += category.packages.Count;
            }
            return count;
        }
    }
}
