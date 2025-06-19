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
        private Vector2 _versionsScrollPosition;
        private string _searchFilter = "";
        private GooglePackageInfo _selectedPackage;
        private bool _showOnlyInstalled = false;
        private bool _showOnlyUpdatable = false;
        private bool _isRefreshing = false;
        
        // Новые поля для управления группами
        private Dictionary<string, bool> _expandedCategories = new Dictionary<string, bool>();

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
        }

        private void OnEnable()
        {
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
        }

        private void InitializeStyles()
        {
            try
            {
                _categoryButtonStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
                
                _packageItemStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(20, 10, 5, 5),
                    margin = new RectOffset(0, 0, 1, 1)
                };
                
                _packageSelectedStyle = new GUIStyle(_packageItemStyle);
                var selectedTexture = MakeTexture(1, 1, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                if (selectedTexture != null)
                {
                    _packageSelectedStyle.normal.background = selectedTexture;
                }
                
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16
                };
                
                _versionStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Italic
                };
                
                _descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 12
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing styles: {ex.Message}");
                SetFallbackStyles();
            }
        }

        private void SetFallbackStyles()
        {
            _categoryButtonStyle = new GUIStyle(GUI.skin.button);
            _packageItemStyle = new GUIStyle(GUI.skin.box);
            _packageSelectedStyle = new GUIStyle(GUI.skin.box);
            _titleStyle = new GUIStyle(GUI.skin.label);
            _versionStyle = new GUIStyle(GUI.skin.label);
            _descriptionStyle = new GUIStyle(GUI.skin.label);
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            try
            {
                Color[] pixels = new Color[width * height];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = color;

                Texture2D texture = new Texture2D(width, height);
                texture.SetPixels(pixels);
                texture.Apply();
                return texture;
            }
            catch
            {
                return null;
            }
        }

        private void OnGUI()
        {
            try
            {
                if (_titleStyle == null)
                {
                    InitializeStyles();
                }
                
                DrawToolbar();
                  EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
                
                // Левая панель - список пакетов с группировкой (увеличена ширина)
                EditorGUILayout.BeginVertical(GUILayout.Width(500), GUILayout.ExpandHeight(true));
                DrawPackagesListPanel();
                EditorGUILayout.EndVertical();
                
                // Разделитель
                EditorGUILayout.Space(5);
                
                // Правая панель - детали и версии выбранного пакета
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                DrawPackageDetailsPanel();
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.EndHorizontal();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in GooglePackageManagerWindow OnGUI: {ex.Message}");
                GUILayout.Label($"Error: {ex.Message}", EditorStyles.helpBox);
                if (GUILayout.Button("Refresh Window"))
                {
                    Close();
                    ShowWindow();
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.enabled = !_isRefreshing;
            if (GUILayout.Button(_isRefreshing ? "Refreshing..." : "Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RefreshPackageList();
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            GUILayout.Label("Search:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarTextField, GUILayout.Width(200));

            GUILayout.Space(10);

            _showOnlyInstalled = GUILayout.Toggle(_showOnlyInstalled, "Installed Only", EditorStyles.toolbarButton, GUILayout.Width(100));
            _showOnlyUpdatable = GUILayout.Toggle(_showOnlyUpdatable, "Updates Only", EditorStyles.toolbarButton, GUILayout.Width(100));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Settings", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                GooglePackageSettings.OpenSettings();
            }

            EditorGUILayout.EndHorizontal();
        }        private void DrawPackagesListPanel()
        {
            GUILayout.Label("Packages", EditorStyles.boldLabel);
            
            var database = Core.GooglePackageManager.PackageDatabase;
            if (database == null || database.categories == null || database.categories.Count == 0)
            {
                GUILayout.Label("No packages loaded.\nClick 'Refresh' to load packages.", EditorStyles.helpBox);
                return;
            }

            // Используем ExpandHeight для того, чтобы скролл занимал всю доступную высоту
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            
            foreach (var category in database.categories)
            {
                if (category == null || category.packages == null) continue;

                var filteredPackages = GetFilteredPackages(category.packages);
                if (filteredPackages.Count == 0 && (_showOnlyInstalled || _showOnlyUpdatable))
                    continue;

                // Получаем состояние развернутости категории
                if (!_expandedCategories.ContainsKey(category.name))
                    _expandedCategories[category.name] = true;

                // Заголовок категории с foldout
                bool expanded = _expandedCategories[category.name];
                bool newExpanded = EditorGUILayout.Foldout(expanded, $"{category.displayName} ({filteredPackages.Count})", _categoryButtonStyle);
                
                if (newExpanded != expanded)
                {
                    _expandedCategories[category.name] = newExpanded;
                }

                // Пакеты в категории
                if (newExpanded)
                {
                    foreach (var package in filteredPackages)
                    {
                        DrawPackageItem(package);
                    }
                }
                
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
        }private void DrawPackageItem(GooglePackageInfo package)
        {
            bool isSelected = _selectedPackage == package;
            var style = isSelected ? _packageSelectedStyle : _packageItemStyle;

            // Делаем весь блок кликабельным
            var rect = EditorGUILayout.BeginHorizontal(style);
            
            // Проверяем клик по всему блоку
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                _selectedPackage = package;
                Event.current.Use();
                Repaint();
            }
            
            // Иконка статуса
            string statusIcon = "";
            Color iconColor = Color.white;
            
            if (package.isInstalled)
            {
                statusIcon = package.hasUpdate ? "⚠" : "✓";
                iconColor = package.hasUpdate ? Color.yellow : Color.green;
            }

            var originalColor = GUI.color;
            GUI.color = iconColor;
            GUILayout.Label(statusIcon, GUILayout.Width(20));
            GUI.color = originalColor;

            // Название пакета - теперь просто лейбл, так как весь блок кликабельный
            GUILayout.Label(package.displayName, EditorStyles.label);

            GUILayout.FlexibleSpace();

            // Версия
            string versionText = package.isInstalled ? $"v{package.installedVersion}" : $"v{package.version}";
            GUILayout.Label(versionText, _versionStyle, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageDetailsPanel()
        {
            if (_selectedPackage == null)
            {
                GUILayout.Label("Select a package to view details", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUILayout.BeginVertical();

            // Заголовок пакета
            GUILayout.Label(_selectedPackage.displayName, _titleStyle);
            GUILayout.Label($"Package: {_selectedPackage.packageName}", EditorStyles.miniLabel);
            
            GUILayout.Space(10);

            // Информация о пакете
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Latest Version: {_selectedPackage.version}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Статус установки
            if (_selectedPackage.isInstalled)
            {
                var color = _selectedPackage.hasUpdate ? Color.yellow : Color.green;
                var originalColor = GUI.color;
                GUI.color = color;
                GUILayout.Label(_selectedPackage.hasUpdate ? 
                    $"Installed: v{_selectedPackage.installedVersion} (Update Available)" : 
                    $"Installed: v{_selectedPackage.installedVersion} (Up to date)", EditorStyles.boldLabel);
                GUI.color = originalColor;
            }
            else
            {
                GUILayout.Label("Not Installed", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Кнопка установки последней версии
            EditorGUILayout.BeginHorizontal();
            
            string buttonText;
            if (_selectedPackage.isInstalled)
            {
                buttonText = _selectedPackage.hasUpdate ? "Update to Latest" : "Reinstall Latest";
            }
            else
            {
                buttonText = "Install Latest";
            }

            if (GUILayout.Button(buttonText, GUILayout.Height(30)))
            {
                InstallPackage(_selectedPackage);
            }

            // Кнопка удаления (если установлен)
            if (_selectedPackage.isInstalled)
            {
                if (GUILayout.Button("Uninstall", GUILayout.Height(30), GUILayout.Width(80)))
                {
                    UninstallPackage(_selectedPackage);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // Список версий
            if (_selectedPackage.availableVersions != null && _selectedPackage.availableVersions.Count > 0)
            {
                GUILayout.Label("Available Versions:", EditorStyles.boldLabel);
                
                _versionsScrollPosition = EditorGUILayout.BeginScrollView(_versionsScrollPosition, GUILayout.Height(200));
                
                foreach (var version in _selectedPackage.availableVersions)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    // Версия
                    GUILayout.Label($"v{version.version}", EditorStyles.boldLabel, GUILayout.Width(80));
                    
                    // Дата публикации
                    if (!string.IsNullOrEmpty(version.publishDate))
                    {
                        GUILayout.Label(version.publishDate, EditorStyles.miniLabel, GUILayout.Width(100));
                    }
                    
                    // Unity версия
                    if (!string.IsNullOrEmpty(version.minimumUnityVersion))
                    {
                        GUILayout.Label($"Unity {version.minimumUnityVersion}+", EditorStyles.miniLabel, GUILayout.Width(100));
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    // Кнопка установки конкретной версии
                    if (GUILayout.Button("Install", GUILayout.Width(60)))
                    {
                        InstallSpecificVersion(_selectedPackage, version);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private List<GooglePackageInfo> GetFilteredPackages(List<GooglePackageInfo> packages)
        {
            return packages.Where(p => 
            {
                if (p == null) return false;
                
                // Фильтр по тексту поиска
                if (!string.IsNullOrEmpty(_searchFilter) && 
                    !p.displayName.ToLower().Contains(_searchFilter.ToLower()) &&
                    !p.packageName.ToLower().Contains(_searchFilter.ToLower()))
                {
                    return false;
                }

                // Фильтр "только установленные"
                if (_showOnlyInstalled && !p.isInstalled)
                    return false;

                // Фильтр "только с обновлениями"
                if (_showOnlyUpdatable && (!p.isInstalled || !p.hasUpdate))
                    return false;

                return true;
            }).ToList();
        }

        private async void RefreshPackageList()
        {
            _isRefreshing = true;
            try
            {
                await Core.GooglePackageManager.RefreshPackageDatabaseAsync();
                Repaint();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async void InstallPackage(GooglePackageInfo package)
        {
            if (!Core.GooglePackageManager.ConfirmInstallPackage(package))
                return;

            try
            {
                await Core.GooglePackageManager.InstallPackageAsync(package);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to install package: {ex.Message}");
            }
        }

        private async void InstallSpecificVersion(GooglePackageInfo package, GooglePackageVersion version)
        {
            try
            {
                await Core.GooglePackageManager.InstallSpecificVersionAsync(package, version);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to install specific version: {ex.Message}");
            }
        }

        private async void UninstallPackage(GooglePackageInfo package)
        {
            if (!Core.GooglePackageManager.ConfirmUninstallPackage(package))
                return;

            try
            {
                await Core.GooglePackageManager.UninstallPackageAsync(package);
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to uninstall package: {ex.Message}");
            }
        }
    }
}

