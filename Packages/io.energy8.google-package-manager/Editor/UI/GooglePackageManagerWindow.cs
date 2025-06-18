using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Energy8.GooglePackageManager.Data;

namespace Energy8.GooglePackageManager.UI
{
    public class GooglePackageManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private GooglePackageCategory _selectedCategory;
        private GooglePackageInfo _selectedPackage;
        private bool _showOnlyInstalled = false;
        private bool _showOnlyUpdatable = false;
        private bool _isRefreshing = false;

        // GUI Styles
        private GUIStyle _categoryButtonStyle;
        private GUIStyle _packageItemStyle;
        private GUIStyle _packageSelectedStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _versionStyle;
        private GUIStyle _descriptionStyle;

        [MenuItem("Energy8/Google Package Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<GooglePackageManagerWindow>("Google Package Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }        private void OnEnable()
        {
            // Инициализация стилей будет происходить в OnGUI для безопасности
            
            // Автоматически обновляем список пакетов при открытии окна
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (Core.GooglePackageManager.PackageDatabase?.categories?.Count == 0)
                    {
                        RefreshPackageList();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Could not refresh package list on enable: {ex.Message}");
                }
            };
        }        private void InitializeStyles()
        {
            try
            {
                // Проверяем, что EditorStyles доступны
                if (EditorStyles.miniButton == null)
                {
                    Debug.LogWarning("EditorStyles not ready, using fallback initialization");
                    SetFallbackStyles();
                    return;
                }
                
                _categoryButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };
                
                _packageItemStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(0, 0, 2, 2)
                };
                
                _packageSelectedStyle = new GUIStyle(_packageItemStyle);
                try
                {
                    var backgroundTexture = MakeTexture(1, 1, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                    if (backgroundTexture != null)
                    {
                        _packageSelectedStyle.normal.background = backgroundTexture;
                    }
                }
                catch
                {
                    // Если не получается создать текстуру, используем стандартный стиль
                    _packageSelectedStyle = new GUIStyle(_packageItemStyle);
                }
                
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold
                };
                
                _versionStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Italic
                };
                
                _descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 11,
                    wordWrap = true
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error initializing styles: {ex.Message}");
                SetFallbackStyles();
            }
        }
        
        private void SetFallbackStyles()
        {
            // Устанавливаем безопасные fallback стили
            try
            {
                _categoryButtonStyle = EditorStyles.miniButton ?? new GUIStyle();
                _packageItemStyle = EditorStyles.helpBox ?? new GUIStyle();
                _packageSelectedStyle = EditorStyles.helpBox ?? new GUIStyle();
                _titleStyle = EditorStyles.boldLabel ?? new GUIStyle();
                _versionStyle = EditorStyles.miniLabel ?? new GUIStyle();
                _descriptionStyle = EditorStyles.wordWrappedLabel ?? new GUIStyle();
            }
            catch
            {
                // Последний резерв - создаем новые стили
                _categoryButtonStyle = new GUIStyle();
                _packageItemStyle = new GUIStyle();
                _packageSelectedStyle = new GUIStyle();
                _titleStyle = new GUIStyle();
                _versionStyle = new GUIStyle();
                _descriptionStyle = new GUIStyle();
            }
        }
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            try
            {
                Color[] pixels = new Color[width * height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = color;
                }

                Texture2D texture = new Texture2D(width, height);
                texture.SetPixels(pixels);
                texture.Apply();
                return texture;
            }
            catch
            {
                // Если не удается создать текстуру, возвращаем null
                return null;
            }
        }        private void OnGUI()
        {
            try
            {
                // Проверяем готовность GUI системы
                if (Event.current == null || GUI.skin == null)
                {
                    GUILayout.Label("Loading...", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                // Инициализируем стили если они еще не инициализированы
                if (_titleStyle == null || _categoryButtonStyle == null)
                {
                    InitializeStyles();
                }
                
                // Если стили все еще не готовы, показываем сообщение
                if (_titleStyle == null)
                {
                    GUILayout.Label("Initializing interface...", EditorStyles.centeredGreyMiniLabel);
                    return;
                }
                
                DrawToolbar();
                
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                
                try
                {
                    // Левая панель - категории
                    DrawCategoriesPanel();
                    
                    // Разделитель
                    EditorGUILayout.Space(5);
                    
                    // Правая панель - пакеты и детали
                    EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                    try
                    {
                        DrawPackagesPanel();
                    }
                    finally
                    {
                        EditorGUILayout.EndVertical();
                    }
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in GooglePackageManagerWindow OnGUI: {ex.Message}");
                
                // В случае ошибки, показываем сообщение об ошибке
                try
                {
                    GUILayout.Label($"Error: {ex.Message}", EditorStyles.helpBox);
                    if (GUILayout.Button("Refresh Window"))
                    {
                        Close();
                        ShowWindow();
                    }
                }
                catch
                {
                    // Если даже это не работает, ничего не делаем
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Кнопка обновления
            GUI.enabled = !_isRefreshing;
            if (GUILayout.Button(_isRefreshing ? "Refreshing..." : "Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RefreshPackageList();
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            // Поиск
            GUILayout.Label("Search:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarTextField, GUILayout.Width(200));

            GUILayout.Space(10);

            // Фильтры
            _showOnlyInstalled = GUILayout.Toggle(_showOnlyInstalled, "Installed Only", EditorStyles.toolbarButton, GUILayout.Width(100));
            _showOnlyUpdatable = GUILayout.Toggle(_showOnlyUpdatable, "Updates Only", EditorStyles.toolbarButton, GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            // Настройки
            if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                GooglePackageSettings.OpenSettings();
            }

            EditorGUILayout.EndHorizontal();
        }
        private void DrawCategoriesPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200), GUILayout.ExpandHeight(true));

            GUILayout.Label("Categories", EditorStyles.boldLabel);

            var database = Core.GooglePackageManager.PackageDatabase;
            if (database == null || database.categories == null || database.categories.Count == 0)
            {
                GUILayout.Label("No categories loaded.\nClick 'Refresh' to load packages.", EditorStyles.helpBox);
            }
            else
            {
                foreach (var category in database.categories)
                {
                    if (category == null || category.packages == null) continue;

                    var filteredPackages = GetFilteredPackages(category.packages);
                    if (filteredPackages.Count == 0 && (_showOnlyInstalled || _showOnlyUpdatable))
                        continue;

                    bool isSelected = _selectedCategory == category;

                    Color originalColor = GUI.backgroundColor;
                    if (isSelected)
                        GUI.backgroundColor = Color.cyan;

                    // Используем безопасный стиль кнопки
                    var buttonStyle = _categoryButtonStyle;
                    if (buttonStyle == null)
                    {
                        buttonStyle = new GUIStyle(EditorStyles.miniButton)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            padding = new RectOffset(10, 10, 5, 5)
                        };
                    }

                    try
                    {
                        if (GUILayout.Button($"{category.displayName ?? "Unknown"} ({filteredPackages.Count})", buttonStyle))
                        {
                            _selectedCategory = category;
                            _selectedPackage = null;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error drawing category button: {ex.Message}");
                        // Fallback к простой кнопке
                        if (GUILayout.Button($"{category.displayName ?? "Unknown"} ({filteredPackages.Count})"))
                        {
                            _selectedCategory = category;
                            _selectedPackage = null;
                        }
                    }

                    GUI.backgroundColor = originalColor;
                }
            }

            EditorGUILayout.EndVertical();
        }
        private void DrawPackagesPanel()
        {
            if (_selectedCategory == null)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                GUILayout.Label("Select a category to view packages", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));

            // Левая часть - список пакетов
            EditorGUILayout.BeginVertical(GUILayout.Width(400), GUILayout.ExpandHeight(true));
            DrawPackagesList();
            EditorGUILayout.EndVertical();

            // Правая часть - детали пакета
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawPackageDetails();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        private void DrawPackagesList()
        {
            GUILayout.Label($"Packages in {_selectedCategory.displayName}", EditorStyles.boldLabel);

            var filteredPackages = GetFilteredPackages(_selectedCategory.packages);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            if (filteredPackages.Count == 0)
            {
                GUILayout.Label("No packages match the current filter.", EditorStyles.helpBox);
            }
            else
            {
                foreach (var package in filteredPackages)
                {
                    DrawPackageItem(package);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        private void DrawPackageItem(GooglePackageInfo package)
        {
            if (package == null) return;

            bool isSelected = _selectedPackage == package;
            var style = isSelected ? _packageSelectedStyle : _packageItemStyle;

            // Используем fallback стили если основные не инициализированы
            if (style == null) style = EditorStyles.helpBox;

            EditorGUILayout.BeginVertical(style);

            if (GUILayout.Button("", GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(60)))
            {
                _selectedPackage = package;
            }

            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x += 5;
            lastRect.y += 5;
            lastRect.width -= 10;
            lastRect.height = 20;

            // Название пакета
            var titleStyle = _titleStyle ?? EditorStyles.boldLabel;
            GUI.Label(lastRect, package.displayName ?? "Unknown Package", titleStyle);

            // Версия и статус
            lastRect.y += 20;
            lastRect.height = 15;
            string versionText = $"v{package.version ?? "0.0.0"}";
            if (package.isInstalled)
            {
                versionText += $" (Installed: v{package.installedVersion ?? "0.0.0"})";
                if (package.hasUpdate)
                {
                    versionText += " - UPDATE AVAILABLE";
                }
            }
            var versionStyle = _versionStyle ?? EditorStyles.miniLabel;
            GUI.Label(lastRect, versionText, versionStyle);

            // Статус индикаторы
            lastRect.y += 15;
            lastRect.height = 15;
            string status = "";
            if (package.isInstalled)
            {
                status += "✓ Installed ";
                if (package.hasUpdate)
                {
                    status += "⚠ Update Available";
                }
            }
            else
            {
                status = "Not Installed";
            }
            GUI.Label(lastRect, status, versionStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawPackageDetails()
        {
            if (_selectedPackage == null)
            {
                GUILayout.Label("Select a package to view details", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var package = _selectedPackage;

            GUILayout.Label("Package Details", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Основная информация
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label(package.displayName, _titleStyle);
            GUILayout.Label($"Package Name: {package.packageName}", EditorStyles.label);
            GUILayout.Label($"Version: {package.version}", EditorStyles.label);
            GUILayout.Label($"Category: {package.category}", EditorStyles.label);
            GUILayout.Label($"Minimum Unity Version: {package.minimumUnityVersion}", EditorStyles.label);
            GUILayout.Label($"Publish Date: {package.publishDate}", EditorStyles.label);

            EditorGUILayout.Space();

            // Статус установки
            if (package.isInstalled)
            {
                EditorGUILayout.HelpBox($"Installed Version: {package.installedVersion}", MessageType.Info);
                if (package.hasUpdate)
                {
                    EditorGUILayout.HelpBox($"New version {package.version} is available!", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Package is not installed", MessageType.None);
            }            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Селектор версий
            DrawVersionSelector(package);

            EditorGUILayout.Space();

            // Кнопки действий
            EditorGUILayout.BeginHorizontal();

            if (package.isInstalled)
            {
                if (package.hasUpdate)
                {
                    if (GUILayout.Button($"Update to v{package.version}", GUILayout.Height(30)))
                    {
                        UpdatePackage(package);
                    }
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Uninstall", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    UninstallPackage(package);
                }
                GUI.backgroundColor = Color.white;
            }            else
            {
                GUI.backgroundColor = Color.green;
                
                // Определяем какую версию устанавливать
                string installVersionText = package.version;
                if (selectedVersionIndices.ContainsKey(package.packageName) && 
                    package.availableVersions != null && 
                    package.availableVersions.Count > 0)
                {
                    var selectedVersion = package.availableVersions[selectedVersionIndices[package.packageName]];
                    installVersionText = selectedVersion.version;
                }
                
                if (GUILayout.Button($"Install v{installVersionText}", GUILayout.Height(30)))
                {
                    InstallSelectedVersion(package);
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Дополнительная информация
            if (!string.IsNullOrEmpty(package.description))
            {
                GUILayout.Label("Description:", EditorStyles.boldLabel);
                GUILayout.Label(package.description, _descriptionStyle);
            }

            // Добавляем UI для выбора версий пакетов
            DrawVersionSelector(package);
        }

        // Добавляем поля для выбора версий
        private Dictionary<string, int> selectedVersionIndices = new Dictionary<string, int>();
        private Dictionary<string, bool> showVersionSelector = new Dictionary<string, bool>();
        
        private void DrawVersionSelector(GooglePackageInfo package)
        {
            if (package.availableVersions == null || package.availableVersions.Count <= 1)
                return;
            
            // Получаем или создаем индекс выбранной версии
            if (!selectedVersionIndices.ContainsKey(package.packageName))
                selectedVersionIndices[package.packageName] = 0;
            
            if (!showVersionSelector.ContainsKey(package.packageName))
                showVersionSelector[package.packageName] = false;
            
            EditorGUILayout.BeginHorizontal();
            
            // Кнопка для показа/скрытия селектора версий
            if (GUILayout.Button($"Versions ({package.availableVersions.Count})", GUILayout.Width(100)))
            {
                showVersionSelector[package.packageName] = !showVersionSelector[package.packageName];
            }
            
            // Показываем текущую выбранную версию
            var selectedVersion = package.availableVersions[selectedVersionIndices[package.packageName]];
            EditorGUILayout.LabelField($"Selected: {selectedVersion.version}", GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();
            
            // Селектор версий (показываем только если развернут)
            if (showVersionSelector[package.packageName])
            {
                EditorGUI.indentLevel++;
                
                string[] versionOptions = package.availableVersions
                    .Select(v => $"{v.version} ({v.publishDate}) [Unity {v.minimumUnityVersion}]")
                    .ToArray();
                
                int newIndex = EditorGUILayout.Popup("Select Version:", selectedVersionIndices[package.packageName], versionOptions);
                
                if (newIndex != selectedVersionIndices[package.packageName])
                {
                    selectedVersionIndices[package.packageName] = newIndex;
                    Debug.Log($"Selected version {package.availableVersions[newIndex].version} for {package.packageName}");
                }
                
                // Показываем детали выбранной версии
                var version = package.availableVersions[selectedVersionIndices[package.packageName]];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Version Details:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Version: {version.version}");
                EditorGUILayout.LabelField($"Publish Date: {version.publishDate}");
                EditorGUILayout.LabelField($"Min Unity: {version.minimumUnityVersion}");
                EditorGUILayout.LabelField($"Dependencies: {version.dependencies}");
                
                if (!string.IsNullOrEmpty(version.downloadUrlTgz))
                {
                    EditorGUILayout.LabelField("Download URL:", EditorStyles.boldLabel);
                    EditorGUILayout.SelectableLabel(version.downloadUrlTgz, GUILayout.Height(20));
                }
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
        }
        
        private List<GooglePackageInfo> GetFilteredPackages(List<GooglePackageInfo> packages)
        {
            if (packages == null) return new List<GooglePackageInfo>();

            var filtered = packages.AsEnumerable();

            // Фильтр по поиску
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                filtered = filtered.Where(p =>
                    p != null &&
                    ((p.displayName?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (p.packageName?.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            // Фильтр "только установленные"
            if (_showOnlyInstalled)
            {
                filtered = filtered.Where(p => p != null && p.isInstalled);
            }

            // Фильтр "только с обновлениями"
            if (_showOnlyUpdatable)
            {
                filtered = filtered.Where(p => p != null && p.hasUpdate);
            }

            return filtered.ToList();
        }

        private async void RefreshPackageList()
        {
            _isRefreshing = true;
            Repaint();
            try
            {
                await Core.GooglePackageManager.RefreshPackageDatabaseAsync();
                _selectedCategory = null;
                _selectedPackage = null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error refreshing package list: {ex.Message}");
                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("Error", "Failed to refresh package list. Check the console for details.", "OK");
                };
            }
            finally
            {
                _isRefreshing = false;
                Repaint();
            }
        }
        private async void InstallPackage(GooglePackageInfo package)
        {
            try
            {
                // Сначала показываем диалог подтверждения в главном потоке
                if (!Core.GooglePackageManager.ConfirmInstallPackage(package))
                {
                    return;
                }

                // Затем выполняем асинхронную установку
                await Core.GooglePackageManager.InstallPackageAsync(package);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing package: {ex.Message}");
                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to install package: {ex.Message}", "OK");
                };
            }
        }
        private async void UpdatePackage(GooglePackageInfo package)
        {
            try
            {
                // Сначала показываем диалог подтверждения в главном потоке
                if (!Core.GooglePackageManager.ConfirmInstallPackage(package))
                {
                    return;
                }

                // Затем выполняем асинхронную установку
                await Core.GooglePackageManager.InstallPackageAsync(package);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating package: {ex.Message}");
                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to update package: {ex.Message}", "OK");
                };
            }
        }
        private async void UninstallPackage(GooglePackageInfo package)
        {
            try
            {
                // Сначала показываем диалог подтверждения в главном потоке
                if (!Core.GooglePackageManager.ConfirmUninstallPackage(package))
                {
                    return;
                }

                // Затем выполняем асинхронное удаление
                await Core.GooglePackageManager.UninstallPackageAsync(package);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error uninstalling package: {ex.Message}");
                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("Error", $"Failed to uninstall package: {ex.Message}", "OK");
                };
            }
        }
        
        private async void InstallSelectedVersion(GooglePackageInfo package)
        {
            try
            {                if (!selectedVersionIndices.ContainsKey(package.packageName))
                {
                    // Используем стандартную установку
                    await Energy8.GooglePackageManager.Core.GooglePackageManager.InstallPackageAsync(package);
                    return;
                }
                
                int versionIndex = selectedVersionIndices[package.packageName];
                var selectedVersion = package.availableVersions[versionIndex];
                
                Debug.Log($"Installing specific version {selectedVersion.version} of {package.displayName}");
                
                bool success = await Energy8.GooglePackageManager.Core.GooglePackageManager.InstallSpecificVersionAsync(package, selectedVersion);
                
                if (success)
                {
                    EditorUtility.DisplayDialog("Success", 
                        $"Successfully installed {package.displayName} version {selectedVersion.version}", "OK");
                    RefreshPackageList();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", 
                        $"Failed to install {package.displayName} version {selectedVersion.version}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error installing specific version: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Installation failed: {ex.Message}", "OK");
            }
        }
    }
}
