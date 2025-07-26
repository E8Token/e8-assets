# ViewportManager Quick Start Guide

Get up and running with ViewportManager in 5 minutes.

## What is ViewportManager?

ViewportManager is a Unity package that automatically detects device orientation, platform, and device type, then fires events when these change. Perfect for creating responsive UIs that adapt to different screen orientations and devices.

## 1. Basic Setup (30 seconds)

### Add Bootstrap Component

1. Create an empty GameObject in your scene
2. Name it "ViewportManager"
3. Add the `ViewportManagerBootstrap` component
4. Leave all settings at default

✅ **Done!** The system is now active and detecting viewport changes.

## 2. Listen to Orientation Changes (2 minutes)

### Simple Event Listener

```csharp
using UnityEngine;
using Energy8.ViewportManager.Core;

public class MyOrientationHandler : MonoBehaviour
{
    private void Start()
    {
        ViewportManager.OnContextChanged += OnViewportChanged;
    }
    
    private void OnDestroy()
    {
        ViewportManager.OnContextChanged -= OnViewportChanged;
    }
    
    private void OnViewportChanged(ViewportContext context)
    {
        if (context.orientation == ScreenOrientation.Portrait)
        {
            Debug.Log("Switched to Portrait mode");
            // Your portrait UI logic here
        }
        else
        {
            Debug.Log("Switched to Landscape mode");
            // Your landscape UI logic here
        }
    }
}
```

### Using the Event Listener Base Class

```csharp
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveUI : ViewportEventListener
{
    protected override void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
    {
        if (newOrientation == ScreenOrientation.Portrait)
        {
            ShowPortraitLayout();
        }
        else
        {
            ShowLandscapeLayout();
        }
    }
    
    private void ShowPortraitLayout() { /* Your code */ }
    private void ShowLandscapeLayout() { /* Your code */ }
}
```

## 3. Test Orientation Detection (1 minute)

### Add Test Component

1. Add `ViewportOrientationTester` component to any GameObject
2. Enable "Enable Logging" in inspector
3. Play the scene
4. Rotate your device or resize the Game window
5. Watch the Console for orientation change logs

### Manual Testing

```csharp
// Get current state anytime
var context = ViewportDetector.DetectContext();
Debug.Log($"Device: {context.deviceType}, Platform: {context.platform}, Orientation: {context.orientation}");
```

## 4. Common Use Cases

### Responsive Canvas Scaler

```csharp
using UnityEngine.UI;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class ResponsiveCanvas : ViewportEventListener
{
    [SerializeField] private CanvasScaler canvasScaler;
    
    protected override void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
    {
        if (newOrientation == ScreenOrientation.Portrait)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
        }
        else
        {
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
        }
    }
}
```

### Show/Hide UI Elements

```csharp
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class PlatformUI : ViewportEventListener
{
    [SerializeField] private GameObject mobileUI;
    [SerializeField] private GameObject desktopUI;
    
    protected override void OnDeviceTypeChanged(DeviceType previousDeviceType, DeviceType newDeviceType)
    {
        mobileUI.SetActive(newDeviceType == DeviceType.Mobile);
        desktopUI.SetActive(newDeviceType == DeviceType.Desktop);
    }
}
```

### Adjust Layout Based on Orientation

```csharp
using UnityEngine;
using Energy8.ViewportManager.Components;
using Energy8.ViewportManager.Core;

public class LayoutSwitcher : ViewportEventListener
{
    [SerializeField] private RectTransform buttonContainer;
    
    protected override void OnOrientationChanged(ScreenOrientation previousOrientation, ScreenOrientation newOrientation)
    {
        var layoutGroup = buttonContainer.GetComponent<HorizontalOrVerticalLayoutGroup>();
        
        if (newOrientation == ScreenOrientation.Portrait)
        {
            // Vertical layout for portrait
            if (layoutGroup is HorizontalLayoutGroup)
            {
                DestroyImmediate(layoutGroup);
                buttonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
        }
        else
        {
            // Horizontal layout for landscape
            if (layoutGroup is VerticalLayoutGroup)
            {
                DestroyImmediate(layoutGroup);
                buttonContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
        }
    }
}
```

## 5. Platform Detection

```csharp
// Check what platform you're running on
var context = ViewportDetector.DetectContext();

switch (context.platform)
{
    case Platform.WebGL:
        Debug.Log("Running in browser");
        break;
    case Platform.Android:
        Debug.Log("Running on Android");
        break;
    case Platform.iOS:
        Debug.Log("Running on iOS");
        break;
    case Platform.Windows:
        Debug.Log("Running on Windows");
        break;
}

// Check device type
if (context.deviceType == DeviceType.Mobile)
{
    Debug.Log("Mobile device detected");
}
else
{
    Debug.Log("Desktop device detected");
}
```

## 6. WebGL Enhanced Detection

For WebGL builds, the system automatically uses JavaScript for enhanced detection:

```csharp
// Get detailed device info (WebGL only)
var deviceInfo = ViewportDetector.GetDeviceInfo();
Debug.Log($"User Agent: {deviceInfo.userAgent}");
Debug.Log($"Touch Support: {deviceInfo.touchSupport}");
Debug.Log($"Device Pixel Ratio: {deviceInfo.devicePixelRatio}");
```

## 7. Debug and Testing

### Enable Debug Logging

1. Select your ViewportManagerBootstrap GameObject
2. Enable "Enable Debug Logging"
3. Set "Debug Info Key" (default F12)
4. Press F12 during play to see system info

### Force Refresh Detection

```csharp
// Force the system to check for changes
ViewportManager.RefreshContext();
```

### Get System Information

```csharp
// Get comprehensive debug info
string info = ViewportManager.GetSystemInfo();
Debug.Log(info);
```

## 8. Best Practices

### ✅ Do

- Always unsubscribe from events in `OnDestroy()`
- Use `ViewportEventListener` base class for automatic event management
- Test on actual devices, not just Unity Editor
- Use `DontDestroyOnLoad` for persistent components

### ❌ Don't

- Don't forget to unsubscribe from events (causes memory leaks)
- Don't rely on Unity Editor for mobile orientation testing
- Don't subscribe to events before ViewportManager is initialized

## 9. Troubleshooting

### Events Not Firing?

1. Check that `ViewportManagerBootstrap` is in your scene
2. Ensure "Enable Continuous Detection" is enabled
3. Verify you subscribed after ViewportManager initialization

### Wrong Orientation Detected?

1. Test on actual device, not Unity Editor
2. For WebGL, check browser console for JavaScript errors
3. Try adjusting "Detection Interval" in bootstrap

### Performance Issues?

1. Increase "Detection Interval" (default 1 second)
2. Disable "Enable Continuous Detection" if not needed
3. Unsubscribe unused event listeners

## 10. Next Steps

- Read the [Full Usage Guide](USAGE_GUIDE.md) for advanced features
- Check the [API Reference](API_REFERENCE.md) for complete documentation
- Create custom configuration matrices for specific quality settings
- Implement responsive UI patterns for your game

---

**That's it!** You now have a responsive Unity application that automatically adapts to different devices and orientations. The ViewportManager will handle all the detection work, and you just need to respond to the events.

**Happy coding! 🚀**