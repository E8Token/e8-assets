using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Energy8.JSPluginTools;

public class JSPluginTest : MonoBehaviour
{
    private JSPluginObject pluginObject;
    private JSPluginCommunication.JSChannel channel;
    private JSPluginDOM.Element testOverlay;
    private bool initialized = false;
    private string statusMessage = "Ready to test";
    private Vector2 scrollPosition;
    private List<string> testLog = new List<string>();
    
    void Awake()
    {
        Debug.Log("Awake");
    }
    
    void Start()
    {
        Debug.LogWarning("Start");
    }
    
    void OnDestroy()
    {
        if (initialized)
        {
            CleanupAll();
        }
    }
    
    void OnGUI()
    {
        // Set up the main GUI layout
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
        
        // Title
        GUILayout.Label("JSPluginTools Test Suite", GUI.skin.box, GUILayout.ExpandWidth(true));
        
        // Status display
        GUI.backgroundColor = initialized ? Color.green : Color.red;
        GUILayout.Box($"Status: {(initialized ? "Initialized" : "Not Initialized")} - {statusMessage}", GUILayout.ExpandWidth(true));
        GUI.backgroundColor = Color.white;
        
        // Initialize/cleanup buttons
        GUILayout.BeginHorizontal();
        if (!initialized)
        {
            if (GUILayout.Button("Initialize Plugins", GUILayout.Height(40)))
            {
                InitializePlugins();
            }
        }
        else
        {
            if (GUILayout.Button("Cleanup All", GUILayout.Height(40)))
            {
                CleanupAll();
            }
        }
        GUILayout.EndHorizontal();
        
        // Test buttons (only enabled when initialized)
        GUI.enabled = initialized;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Storage", GUILayout.Height(30)))
        {
            TestStorage();
        }
        if (GUILayout.Button("Test DOM", GUILayout.Height(30)))
        {
            TestDOM();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Device", GUILayout.Height(30)))
        {
            TestDevice();
        }
        if (GUILayout.Button("Test Communication", GUILayout.Height(30)))
        {
            TestCommunication();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Network", GUILayout.Height(30)))
        {
            TestNetwork();
        }
        if (GUILayout.Button("Run All Tests", GUILayout.Height(30)))
        {
            StartCoroutine(RunAllTests());
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        
        // Log output
        GUILayout.Label("Test Log:", GUI.skin.box);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        foreach (string logEntry in testLog)
        {
            GUILayout.Label(logEntry);
        }
        GUILayout.EndScrollView();
        
        GUILayout.EndArea();
    }
    
    // Initialize all JSPlugin modules
    private void InitializePlugins()
    {
        if (initialized) return;
        
        try
        {
            LogTest("Initializing all modules");
            bool success = JSPlugin.Initialize(debugMode: true);
            
            if (success)
            {
                LogTest("All modules initialized successfully");
                
                // Register this GameObject with the plugin system
                pluginObject = JSPlugin.CreatePlugin("testPlugin", gameObject);
                
                // Create a communication channel
                channel = JSPlugin.CreateChannel("testChannel", gameObject);
                
                // Configure events forwarding
                pluginObject.ForwardUnityEvents(EventForwardingOptions.CommonEvents);
                
                // Register callbacks
                pluginObject.RegisterCallback("onTestComplete", "HandleTestComplete");
                pluginObject.RegisterErrorHandler("HandleError");
                
                // Add orientation change listener
                JSPluginDevice.AddOrientationChangeListener(pluginObject.ObjectId, "HandleOrientationChange");
                
                initialized = true;
                statusMessage = "JSPluginTools initialized successfully!";
                JSPlugin.ShowToast("JSPluginTools initialized!", 3f, "success");
            }
            else
            {
                LogTest("Failed to initialize modules", true);
                statusMessage = "Initialization failed";
            }
        }
        catch (Exception ex)
        {
            LogTest($"Error during initialization: {ex.Message}", true);
            statusMessage = "Initialization error";
        }
    }
    
    // Test the Storage module
    private void TestStorage()
    {
        if (!EnsureInitialized()) return;
        
        LogTest("Testing Storage module");
        
        // Local Storage (persists between sessions)
        JSPluginStorage.LocalStorage.SetString("testString", "Hello from Unity!");
        JSPluginStorage.LocalStorage.SetInt("testInt", 42);
        JSPluginStorage.LocalStorage.SetFloat("testFloat", 3.14159f);
        JSPluginStorage.LocalStorage.SetBool("testBool", true);
        
        // Store a complex object
        var testObject = new TestData 
        { 
            Name = "Test User", 
            Score = 1000, 
            IsActive = true, 
            LastLoginDate = DateTime.Now.ToString() 
        };
        JSPluginStorage.LocalStorage.SetObject("testObject", testObject);
        
        // Session Storage (cleared when browser is closed)
        JSPluginStorage.SessionStorage.SetString("sessionTest", "Temporary data");
        
        // Read back values to verify
        string storedString = JSPluginStorage.LocalStorage.GetString("testString");
        int storedInt = JSPluginStorage.LocalStorage.GetInt("testInt");
        float storedFloat = JSPluginStorage.LocalStorage.GetFloat("testFloat");
        bool storedBool = JSPluginStorage.LocalStorage.GetBool("testBool");
        var storedObject = JSPluginStorage.LocalStorage.GetObject<TestData>("testObject");
        
        LogTest("Storage test results:");
        LogTest($"  String: {storedString}");
        LogTest($"  Int: {storedInt}");
        LogTest($"  Float: {storedFloat}");
        LogTest($"  Bool: {storedBool}");
        LogTest($"  Object: {(storedObject != null ? $"{storedObject.Name}, {storedObject.Score}" : "null")}");
        
        statusMessage = "Storage test complete";
        JSPlugin.ShowToast("Storage test complete!", 2f, "info");
    }
    
    // Test the DOM module
    private void TestDOM()
    {
        if (!EnsureInitialized()) return;
        
        LogTest("Testing DOM module");
        
        // Remove any previous test overlay
        if (testOverlay != null)
        {
            testOverlay.Remove();
            testOverlay = null;
        }
        
        // Create an overlay with HTML content
        string overlayContent = @"
            <div style='padding: 20px; background-color: rgba(0,0,0,0.7); color: white; border-radius: 8px;'>
                <h2>JSPluginTools DOM Test</h2>
                <p>This overlay was created using the DOM module.</p>
                <button id='testButton' style='padding: 8px 16px; background: #4CAF50; color: white; border: none; border-radius: 4px; cursor: pointer;'>Click Me</button>
            </div>
        ";
        
        testOverlay = JSPlugin.ShowOverlay(overlayContent, "test-overlay");
        
        // Add click event listener to the button
        JSPluginDOM.Element button = JSPluginDOM.GetElement("#testButton");
        if (button != null)
        {
            button.AddEventListener("click", pluginObject.ObjectId, "HandleButtonClick");
        }
        
        // Schedule overlay removal after 10 seconds
        StartCoroutine(RemoveOverlayAfterDelay(10f));
        
        statusMessage = "DOM test - overlay active";
        JSPlugin.ShowToast("DOM test started - overlay will auto-close in 10 seconds", 3f, "info");
    }
    
    // Test the Device module
    private void TestDevice()
    {
        if (!EnsureInitialized()) return;
        
        LogTest("Testing Device module");
        
        // Get device information
        var browserInfo = JSPluginDevice.GetBrowserInfo();
        var osInfo = JSPluginDevice.GetOSInfo();
        var screenInfo = JSPluginDevice.GetScreenInfo();
        
        LogTest("Device information:");
        LogTest($"  Browser: {browserInfo?.Name} {browserInfo?.Version}");
        LogTest($"  OS: {osInfo?.Name} {osInfo?.Version}");
        LogTest($"  Screen: {screenInfo?.Width}x{screenInfo?.Height} ({screenInfo?.ColorDepth} bit)");
        LogTest($"  Device Type: " + 
                (JSPluginDevice.IsMobile() ? "Mobile" : 
                 JSPluginDevice.IsTablet() ? "Tablet" : "Desktop"));
        
        // Test vibration on mobile devices
        if (JSPluginDevice.IsMobile() && JSPluginDevice.IsVibrationSupported())
        {
            JSPluginDevice.Vibrate(200);
            LogTest("Device vibration activated");
        }
        else
        {
            LogTest("Device vibration not supported on this device");
        }
        
        // Add orientation change listener
        JSPluginDevice.AddOrientationChangeListener(pluginObject.ObjectId, "HandleOrientationChange");
        
        statusMessage = "Device test complete";
        JSPlugin.ShowToast("Device test complete!", 2f, "info");
    }
    
    // Test the Communication module
    private void TestCommunication()
    {
        if (!EnsureInitialized()) return;
        
        LogTest("Testing Communication module");
        
        // Send a message through the channel
        channel.Send("testMessage", "Hello from Unity channel!");
        
        // Broadcast a message to all JS listeners
        JSPlugin.Broadcast("globalEvent", "This is a broadcast message");
        
        // Send complex data as JSON
        var userData = new TestData 
        { 
            Name = "Test User", 
            Score = 1000, 
            IsActive = true, 
            LastLoginDate = DateTime.Now.ToString() 
        };
        
        JSPlugin.BroadcastJson("userDataEvent", userData);
        
        // Register a handler for messages from JS
        channel.RegisterHandler("jsMessage", "HandleJSMessage");
        
        LogTest("Messages sent through channel and broadcast");
        LogTest("Handler registered for 'jsMessage' events");
        
        statusMessage = "Communication test complete";
        JSPlugin.ShowToast("Communication test complete!", 2f, "info");
    }
    
    // Test the Network module
    private void TestNetwork()
    {
        if (!EnsureInitialized()) return;
        
        LogTest("Testing Network module");
        
        // Send an HTTP request
        var request = new JSPluginNetwork.RequestData
        {
            Url = "https://httpbin.org/get",
            Method = "GET",
            Headers = new Dictionary<string, string>
            {
                { "X-Test-Header", "Unity-JSPlugin-Test" }
            }
        };
        
        LogTest("Sending HTTP request to httpbin.org");
        JSPluginNetwork.SendRequest(request, HandleNetworkResponse);
        
        statusMessage = "Network test in progress";
        JSPlugin.ShowToast("Network test started - check log for results", 2f, "info");
    }
    
    // Clean up all plugin resources
    private void CleanupAll()
    {
        if (!initialized) return;
        
        LogTest("Cleaning up all resources");
        
        // Remove any active overlays
        if (testOverlay != null)
        {
            testOverlay.Remove();
            testOverlay = null;
        }
        
        // Remove orientation change listener
        JSPluginDevice.RemoveOrientationChangeListener();
        
        // Shut down all modules
        JSPlugin.Shutdown();
        
        initialized = false;
        statusMessage = "All resources cleaned up";
        JSPlugin.ShowToast("All resources cleaned up", 2f, "success");
    }
    
    // Helper method to ensure plugins are initialized
    private bool EnsureInitialized()
    {
        if (!initialized)
        {
            LogTest("Plugins not initialized. Call InitializePlugins first.", true);
            return false;
        }
        return true;
    }
    
    // Run all tests in sequence
    private IEnumerator RunAllTests()
    {
        if (!EnsureInitialized()) yield break;
        
        LogTest("Running all tests in sequence");
        
        TestStorage();
        yield return new WaitForSeconds(1);
        
        TestDevice();
        yield return new WaitForSeconds(1);
        
        TestCommunication();
        yield return new WaitForSeconds(1);
        
        TestNetwork();
        yield return new WaitForSeconds(1);
        
        TestDOM();
        yield return new WaitForSeconds(5);
        
        LogTest("All tests completed");
        statusMessage = "All tests completed";
    }
    
    // Callback handlers
    
    public void HandleButtonClick(string data)
    {
        LogTest($"Button clicked! Data: {data}");
        JSPlugin.ShowToast("Button clicked!", 2f, "success");
    }
    
    public void HandleTestComplete(string result)
    {
        LogTest($"Test completed with result: {result}");
    }
    
    public void HandleError(string error)
    {
        LogTest($"Error received from JavaScript: {error}", true);
    }
    
    public void HandleJSMessage(string message)
    {
        LogTest($"Message from JavaScript: {message}");
    }
    
    public void HandleOrientationChange(string orientationData)
    {
        LogTest($"Orientation changed: {orientationData}");
    }
    
    public void HandleNetworkResponse(JSPluginNetwork.ResponseData response)
    {
        LogTest("Network response received:");
        LogTest($"  Success: {response.IsSuccess}");
        LogTest($"  Status: {response.StatusCode}");
        LogTest($"  Response: {response.Text}");
        
        statusMessage = "Network test complete";
    }
    
    // Helper coroutine to remove overlay after delay
    private IEnumerator RemoveOverlayAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        if (testOverlay != null)
        {
            testOverlay.Remove();
            testOverlay = null;
            LogTest("Overlay removed after timeout");
        }
    }
    
    // Log helper that adds to both Unity console and GUI log
    private void LogTest(string message, bool isError = false)
    {
        if (isError)
        {
            Debug.LogError($"JSPluginTest: {message}");
            testLog.Insert(0, $"<color=red>[ERROR] {message}</color>");
        }
        else
        {
            Debug.Log($"JSPluginTest: {message}");
            testLog.Insert(0, message);
        }
        
        // Keep log at reasonable size
        if (testLog.Count > 50)
        {
            testLog.RemoveAt(testLog.Count - 1);
        }
    }
    
    // Example data class for serialization tests
    [Serializable]
    private class TestData
    {
        public string Name;
        public int Score;
        public bool IsActive;
        public string LastLoginDate;
    }
}
