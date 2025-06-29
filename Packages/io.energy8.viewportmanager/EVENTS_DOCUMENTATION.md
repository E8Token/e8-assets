# ViewportManager Events Documentation

Документация по работе с событиями ViewportManager для отслеживания изменений ориентации и настроек viewport.

## Обзор событий

ViewportManager предоставляет несколько событий для отслеживания изменений:

```csharp
// События в ViewportManager
public static event Action<ViewportContext> OnContextChanged;     // Изменение контекста (ориентация, устройство, платформа)
public static event Action<ViewportConfiguration> OnConfigurationChanged; // Изменение конфигурации
public static event Action<int> OnQualityChanged;               // Изменение качества (Unity Quality Level)
public static event Action OnInitialized;                       // Инициализация системы
```

## Подписка на события

### 1. Базовая подписка в MonoBehaviour

```csharp
using UnityEngine;
using Energy8.ViewportManager.Core;
using Energy8.ViewportManager.Configuration;

public class ViewportEventListener : MonoBehaviour
{
    private void Start()
    {
        // Подписываемся на события
        ViewportManager.OnContextChanged += OnViewportContextChanged;
        ViewportManager.OnConfigurationChanged += OnViewportConfigurationChanged;
        ViewportManager.OnQualityChanged += OnQualityChanged;
        ViewportManager.OnInitialized += OnViewportManagerInitialized;
    }

    private void OnDestroy()
    {
        // ВАЖНО: Отписываемся от событий во избежание утечек памяти
        ViewportManager.OnContextChanged -= OnViewportContextChanged;
        ViewportManager.OnConfigurationChanged -= OnViewportConfigurationChanged;
        ViewportManager.OnQualityChanged -= OnQualityChanged;
        ViewportManager.OnInitialized -= OnViewportManagerInitialized;
    }

    private void OnViewportContextChanged(ViewportContext context)
    {
        Debug.Log($"Viewport context changed: {context}");
        
        // Проверяем изменения
        if (context.orientation == ScreenOrientation.Portrait)
        {
            // Переключились в портретный режим
            HandlePortraitMode();
        }
        else
        {
            // Переключились в ландшафтный режим
            HandleLandscapeMode();
        }
    }

    private void OnViewportConfigurationChanged(ViewportConfiguration config)
    {
        Debug.Log($"Configuration changed: {config}");
        
        // Настройки пока отключены, но событие срабатывает
        bool isLowQuality = config.unityQualityLevel <= 1;
        if (isLowQuality)
        {
            // Адаптируем UI для слабых устройств
            AdaptUIForLowPerformance();
        }
    }

    private void OnQualityChanged(int qualityLevel)
    {
        Debug.Log($"Quality level changed to: {qualityLevel}");
        
        // Реагируем на изменение качества
        UpdateUIBasedOnQuality(qualityLevel);
    }

    private void OnViewportManagerInitialized()
    {
        Debug.Log("ViewportManager initialized!");
        
        // Получаем начальное состояние
        var currentContext = ViewportManager.CurrentContext;
        var currentConfig = ViewportManager.CurrentConfiguration;
        
        Debug.Log($"Initial state: {currentContext} -> {currentConfig}");
    }

    private void HandlePortraitMode()
    {
        // Логика для портретного режима
        Debug.Log("Switching to portrait layout");
    }

    private void HandleLandscapeMode()
    {
        // Логика для ландшафтного режима
        Debug.Log("Switching to landscape layout");
    }

    private void AdaptUIForLowPerformance()
    {
        // Упрощаем UI для слабых устройств
        Debug.Log("Adapting UI for low performance");
    }

    private void UpdateUIBasedOnQuality(int quality)
    {
        // Обновляем UI в зависимости от качества
        Debug.Log($"Updating UI for quality level: {quality}");
    }
}
```

### 2. Использование в статическом контексте

```csharp
public static class GlobalViewportHandler
{
    private static bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        if (isInitialized) return;
        
        // Подписываемся на события
        ViewportManager.OnContextChanged += HandleContextChange;
        ViewportManager.OnConfigurationChanged += HandleConfigurationChange;
        
        isInitialized = true;
        Debug.Log("Global viewport handler initialized");
    }

    private static void HandleContextChange(ViewportContext context)
    {
        // Глобальная обработка изменений контекста
        Debug.Log($"[Global] Context: {context.deviceType}/{context.platform}/{context.orientation}");
        
        // Можно отправить аналитику
        SendAnalyticsEvent("viewport_change", new {
            device = context.deviceType.ToString(),
            platform = context.platform.ToString(),
            orientation = context.orientation.ToString()
        });
    }

    private static void HandleConfigurationChange(ViewportConfiguration config)
    {
        // Глобальная обработка изменений конфигурации
        Debug.Log($"[Global] Configuration: Quality Level {config.unityQualityLevel}");
    }

    private static void SendAnalyticsEvent(string eventName, object data)
    {
        // Ваша логика аналитики
        Debug.Log($"Analytics: {eventName} - {data}");
    }
}
```

### 3. Компонент для адаптивного UI

```csharp
using UnityEngine;
using UnityEngine.UI;
using Energy8.ViewportManager.Core;

public class AdaptiveUIComponent : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject portraitLayout;
    [SerializeField] private GameObject landscapeLayout;
    [SerializeField] private Slider qualityIndicator;
    
    [Header("Settings")]
    [SerializeField] private bool autoAdapt = true;

    private void Start()
    {
        if (autoAdapt)
        {
            // Подписываемся на изменения
            ViewportManager.OnContextChanged += AdaptToContext;
            ViewportManager.OnConfigurationChanged += AdaptToConfiguration;
            
            // Применяем начальное состояние
            if (ViewportManager.IsInitialized)
            {
                AdaptToContext(ViewportManager.CurrentContext);
                AdaptToConfiguration(ViewportManager.CurrentConfiguration);
            }
        }
    }

    private void OnDestroy()
    {
        if (autoAdapt)
        {
            ViewportManager.OnContextChanged -= AdaptToContext;
            ViewportManager.OnConfigurationChanged -= AdaptToConfiguration;
        }
    }

    private void AdaptToContext(ViewportContext context)
    {
        // Переключаем макеты в зависимости от ориентации
        bool isPortrait = context.orientation == ScreenOrientation.Portrait;
        
        if (portraitLayout != null)
            portraitLayout.SetActive(isPortrait);
            
        if (landscapeLayout != null)
            landscapeLayout.SetActive(!isPortrait);
            
        Debug.Log($"[AdaptiveUI] Switched to {(isPortrait ? "portrait" : "landscape")} layout");
    }

    private void AdaptToConfiguration(ViewportConfiguration config)
    {
        // Обновляем индикатор качества
        if (qualityIndicator != null)
        {
            qualityIndicator.value = config.unityQualityLevel / 5.0f; // Нормализуем 0-5 в 0-1
        }
        
        Debug.Log($"[AdaptiveUI] Quality indicator updated to level {config.unityQualityLevel}");
    }

    // Методы для ручного управления
    [ContextMenu("Force Portrait")]
    public void ForcePortraitLayout()
    {
        if (portraitLayout != null) portraitLayout.SetActive(true);
        if (landscapeLayout != null) landscapeLayout.SetActive(false);
    }

    [ContextMenu("Force Landscape")]
    public void ForceLandscapeLayout()
    {
        if (portraitLayout != null) portraitLayout.SetActive(false);
        if (landscapeLayout != null) landscapeLayout.SetActive(true);
    }
}
```

## Продвинутые паттерны

### 1. Debounced Events (защита от частых срабатываний)

```csharp
public class DebouncedViewportHandler : MonoBehaviour
{
    [SerializeField] private float debounceTime = 0.5f;
    
    private Coroutine debounceCoroutine;
    private ViewportContext lastContext;

    private void Start()
    {
        ViewportManager.OnContextChanged += OnContextChangedDebounced;
    }

    private void OnDestroy()
    {
        ViewportManager.OnContextChanged -= OnContextChangedDebounced;
    }

    private void OnContextChangedDebounced(ViewportContext context)
    {
        lastContext = context;
        
        // Отменяем предыдущий debounce
        if (debounceCoroutine != null)
        {
            StopCoroutine(debounceCoroutine);
        }
        
        // Запускаем новый debounce
        debounceCoroutine = StartCoroutine(DebouncedAction());
    }

    private System.Collections.IEnumerator DebouncedAction()
    {
        yield return new WaitForSeconds(debounceTime);
        
        // Выполняем действие только после задержки
        Debug.Log($"Debounced viewport change: {lastContext}");
        HandleFinalContextChange(lastContext);
        
        debounceCoroutine = null;
    }

    private void HandleFinalContextChange(ViewportContext context)
    {
        // Ваша логика обработки
        Debug.Log("Processing final context change...");
    }
}
```

### 2. Event Aggregator Pattern

```csharp
public class ViewportEventAggregator : MonoBehaviour
{
    // Собственные события с дополнительной информацией
    public System.Action<OrientationChangeData> OnOrientationChanged;
    public System.Action<QualityChangeData> OnQualityChangedDetailed;
    
    public struct OrientationChangeData
    {
        public ScreenOrientation from;
        public ScreenOrientation to;
        public float timestamp;
        public DeviceType deviceType;
    }
    
    public struct QualityChangeData
    {
        public int fromLevel;
        public int toLevel;
        public string reason;
        public float timestamp;
    }

    private ViewportContext previousContext;
    private int previousQuality;

    private void Start()
    {
        // Инициализируем предыдущие значения
        if (ViewportManager.IsInitialized)
        {
            previousContext = ViewportManager.CurrentContext;
            previousQuality = ViewportManager.CurrentQualityLevel;
        }

        // Подписываемся на базовые события
        ViewportManager.OnContextChanged += OnContextChanged;
        ViewportManager.OnQualityChanged += OnQualityChangedHandler;
    }

    private void OnDestroy()
    {
        ViewportManager.OnContextChanged -= OnContextChanged;
        ViewportManager.OnQualityChanged -= OnQualityChangedHandler;
    }

    private void OnContextChanged(ViewportContext newContext)
    {
        // Проверяем изменение ориентации
        if (previousContext.orientation != newContext.orientation)
        {
            var orientationData = new OrientationChangeData
            {
                from = previousContext.orientation,
                to = newContext.orientation,
                timestamp = Time.time,
                deviceType = newContext.deviceType
            };
            
            OnOrientationChanged?.Invoke(orientationData);
            Debug.Log($"Orientation changed: {orientationData.from} → {orientationData.to}");
        }

        previousContext = newContext;
    }

    private void OnQualityChangedHandler(int newQuality)
    {
        var qualityData = new QualityChangeData
        {
            fromLevel = previousQuality,
            toLevel = newQuality,
            reason = "Viewport change",
            timestamp = Time.time
        };
        
        OnQualityChangedDetailed?.Invoke(qualityData);
        Debug.Log($"Quality changed: {qualityData.fromLevel} → {qualityData.toLevel}");
        
        previousQuality = newQuality;
    }
}
```

## Лучшие практики

### 1. Всегда отписывайтесь от событий
```csharp
private void OnDestroy()
{
    // КРИТИЧЕСКИ ВАЖНО для избежания утечек памяти
    ViewportManager.OnContextChanged -= YourHandler;
}
```

### 2. Проверяйте инициализацию
```csharp
private void Start()
{
    if (ViewportManager.IsInitialized)
    {
        // Применяем текущее состояние
        HandleContext(ViewportManager.CurrentContext);
    }
    
    // Подписываемся на будущие изменения
    ViewportManager.OnContextChanged += HandleContext;
}
```

### 3. Используйте null-conditional операторы
```csharp
ViewportManager.OnContextChanged?.Invoke(context);
```

### 4. Логирование для отладки
```csharp
private void OnViewportContextChanged(ViewportContext context)
{
    Debug.Log($"[{gameObject.name}] Viewport changed: {context}", this);
    // Ваша логика...
}
```

## Примеры использования

### Адаптация UI для мобильных устройств
```csharp
private void OnContextChanged(ViewportContext context)
{
    if (context.deviceType == DeviceType.Mobile && context.orientation == ScreenOrientation.Portrait)
    {
        // Мобильный портрет - упрощаем интерфейс
        EnableMobilePortraitMode();
    }
}
```

### Реакция на изменение платформы
```csharp
private void OnContextChanged(ViewportContext context)
{
    switch (context.platform)
    {
        case Platform.WebGL:
            EnableWebGLOptimizations();
            break;
        case Platform.Android:
        case Platform.iOS:
            EnableMobileOptimizations();
            break;
    }
}
```

### Аналитика изменений
```csharp
private void OnContextChanged(ViewportContext context)
{
    Analytics.CustomEvent("viewport_change", new Dictionary<string, object>
    {
        {"device_type", context.deviceType.ToString()},
        {"platform", context.platform.ToString()},
        {"orientation", context.orientation.ToString()},
        {"timestamp", Time.time}
    });
}
```

Эта документация поможет вам эффективно использовать события ViewportManager для создания адаптивных интерфейсов и реагирования на изменения viewport в реальном времени.
