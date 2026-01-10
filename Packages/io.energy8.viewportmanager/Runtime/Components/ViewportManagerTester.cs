using UnityEngine;
using Energy8.ViewportManager.Core;
using ScreenOrientation = Energy8.ViewportManager.Core.ScreenOrientation;

namespace Energy8.ViewportManager.Components
{
    /// <summary>
    /// Component for testing and debugging ViewportManager functionality.
    /// Displays current viewport context and events in OnGUI.
    /// </summary>
    [AddComponentMenu("Energy8/Viewport Manager/Viewport Manager Tester")]
    public class ViewportManagerTester : MonoBehaviour
    {
        private ViewportContext _currentContext;
        private string _lastLogMessage;
        
        // GUI Scaling
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private float _scaleFactor = 1f;

        private void Start()
        {
            // Ensure ViewportManager is initialized
            if (!ViewportManager.IsInitialized)
            {
                ViewportManager.Initialize();
            }

            _currentContext = ViewportManager.CurrentContext;
            Log("Initial Context: " + _currentContext);

            // Subscribe to events
            ViewportManager.OnContextChanged += OnContextChanged;
            ViewportManager.OnOrientationChanged += OnOrientationChanged;
            ViewportManager.OnScreenSizeChanged += OnScreenSizeChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            ViewportManager.OnContextChanged -= OnContextChanged;
            ViewportManager.OnOrientationChanged -= OnOrientationChanged;
            ViewportManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        private void Update()
        {
            // Force refresh context every frame to detect changes
            ViewportManager.RefreshContext();
        }

        private void OnContextChanged(ViewportContext context)
        {
            _currentContext = context;
            _lastLogMessage = $"Context Changed: {context}";
        }

        private void OnOrientationChanged(ScreenOrientation orientation)
        {
            _lastLogMessage = $"Orientation Changed: {orientation}";
        }

        private void OnScreenSizeChanged(int width, int height)
        {
            _lastLogMessage = $"Screen Size Changed: {width}x{height}";
        }

        private void Log(string message)
        {
            // store the last log message for display and write to the Unity console
            _lastLogMessage = message;
            Debug.Log(message);
        }

        private void OnGUI()
        {
            // Use a fixed reference resolution for GUI scaling
            const float referenceWidth = 1920f;
            const float referenceHeight = 1080f;
            
            // Calculate scale to fit the reference resolution into the current screen
            // We use the smaller scale factor to ensure the UI fits within the screen
            float scaleX = Screen.width / referenceWidth;
            float scaleY = Screen.height / referenceHeight;
            float scale = Mathf.Min(scaleX, scaleY);
            
            // Enforce a minimum scale for readability on very small screens or high DPI
            // If the screen is small (e.g. mobile portrait), we might want to scale up relative to width
            if (Screen.width < Screen.height)
            {
                // Portrait mode: scale based on width to ensure it's readable
                scale = Screen.width / 1080f; 
            }
            
            // Apply scaling
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));

            // Adjust area size based on reference resolution
            float padding = 50f;
            float width = 800f;
            float height = 600f;
            
            Rect area = new Rect(padding, padding, width, height);
            
            GUI.Box(area, "Viewport Manager Debug");
            
            GUILayout.BeginArea(area);
            GUILayout.Space(40f);
            GUILayout.BeginVertical();
            
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 30; // Larger font for better readability
            labelStyle.wordWrap = true;

            GUILayout.Label($"Is Initialized: {ViewportManager.IsInitialized}", labelStyle);
            GUILayout.Label($"Orientation: {_currentContext.orientation}", labelStyle);
            GUILayout.Label($"Device Type: {_currentContext.deviceType}", labelStyle);
            GUILayout.Label($"Platform: {_currentContext.platform}", labelStyle);
            GUILayout.Label($"Resolution: {_currentContext.screenWidth} x {_currentContext.screenHeight}", labelStyle);
            GUILayout.Label($"Device Pixel Ratio: {_currentContext.devicePixelRatio}", labelStyle);
            GUILayout.Label($"Last Detection Time: {ViewportManager.LastDetectionTime:F2}", labelStyle);
            
            GUILayout.Space(20f);
            GUILayout.Label("Last Event:", labelStyle);
            GUILayout.Label(_lastLogMessage, labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
