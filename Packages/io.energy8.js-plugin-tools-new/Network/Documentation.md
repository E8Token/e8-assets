# Network Module Documentation

## Overview

The Network module provides a comprehensive API for making HTTP/HTTPS requests from Unity WebGL applications. It enables seamless communication with web servers, APIs, and other network resources directly from your Unity application running in a browser.

## Features

- **HTTP/HTTPS Requests**: Make GET, POST, PUT, DELETE, and other HTTP requests
- **Request Configuration**: Set headers, timeout, credentials, and other request parameters
- **Form Data**: Send form data and file uploads
- **JSON Processing**: Automatic serialization and deserialization of JSON data
- **Response Handling**: Process different types of responses (text, JSON, binary)
- **Progress Tracking**: Monitor download and upload progress
- **Request Cancellation**: Cancel in-progress requests
- **Online Status**: Detect and respond to network connectivity changes

## Classes and Components

### INetworkManager

The main interface for network operations:

```csharp
public interface INetworkManager
{
    void Initialize(IPluginCore core);
    bool IsInitialized { get; }
    event Action OnInitialized;
    event Action<bool> OnOnlineStatusChanged;
    
    Task<NetworkResponse> Request(NetworkRequest request);
    Task<T> RequestJson<T>(NetworkRequest request);
    Task<byte[]> RequestBinary(NetworkRequest request);
    void CancelRequest(string requestId);
    void CancelAllRequests();
    Task<bool> IsOnline();
}
```

### NetworkManager

The implementation of `INetworkManager` that provides network request functionality through the plugin core.

### NetworkService

A service implementation that uses the Communication module to make network requests.

### NetworkServiceBehaviour

A MonoBehaviour wrapper for NetworkService that allows finding it with `FindObjectOfType`.

### NetworkRequest

Configuration class for network requests:

```csharp
public class NetworkRequest
{
    public string Url { get; set; }
    public string Method { get; set; } = "GET";
    public Dictionary<string, string> Headers { get; set; }
    public string Body { get; set; }
    public object JsonBody { get; set; }
    public Dictionary<string, string> FormData { get; set; }
    public int Timeout { get; set; } = 30000;
    public bool WithCredentials { get; set; } = false;
    public string RequestId { get; set; }
    public Action<float> OnProgress { get; set; }
}
```

### NetworkResponse

Class representing the response from a network request:

```csharp
public class NetworkResponse
{
    public int Status { get; set; }
    public string StatusText { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string ResponseText { get; set; }
    public byte[] ResponseData { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public string RequestId { get; set; }
}
```

## Usage

### Basic HTTP Requests

```csharp
// Initialize the network manager
var networkManager = new NetworkManager();
networkManager.Initialize(pluginCore);

// Make a simple GET request
try {
    var response = await networkManager.Request(new NetworkRequest {
        Url = "https://api.example.com/data",
        Method = "GET"
    });
    
    if (response.IsSuccess) {
        Debug.Log($"Received data: {response.ResponseText}");
    } else {
        Debug.LogError($"Request failed: {response.ErrorMessage}");
    }
} catch (Exception ex) {
    Debug.LogError($"Request threw an exception: {ex.Message}");
}
```

### JSON Request with Typed Response

```csharp
// Make a GET request and deserialize the JSON response
try {
    var users = await networkManager.RequestJson<List<User>>(new NetworkRequest {
        Url = "https://api.example.com/users",
        Method = "GET",
        Headers = new Dictionary<string, string> {
            { "Accept", "application/json" }
        }
    });
    
    foreach (var user in users) {
        Debug.Log($"User: {user.Name}, Email: {user.Email}");
    }
} catch (Exception ex) {
    Debug.LogError($"JSON request failed: {ex.Message}");
}
```

### POST Request with JSON Body

```csharp
// Create a new user with a POST request
var newUser = new User {
    Name = "John Doe",
    Email = "john@example.com",
    Age = 30
};

try {
    var response = await networkManager.RequestJson<CreateUserResponse>(new NetworkRequest {
        Url = "https://api.example.com/users",
        Method = "POST",
        Headers = new Dictionary<string, string> {
            { "Content-Type", "application/json" }
        },
        JsonBody = newUser
    });
    
    Debug.Log($"User created with ID: {response.Id}");
} catch (Exception ex) {
    Debug.LogError($"Failed to create user: {ex.Message}");
}
```

### Form Data Upload

```csharp
// Submit a form with data
var formData = new Dictionary<string, string> {
    { "username", "player1" },
    { "score", "1250" },
    { "level", "5" }
};

try {
    var response = await networkManager.Request(new NetworkRequest {
        Url = "https://api.example.com/submit-score",
        Method = "POST",
        FormData = formData
    });
    
    if (response.IsSuccess) {
        Debug.Log("Score submitted successfully");
    } else {
        Debug.LogError($"Failed to submit score: {response.ErrorMessage}");
    }
} catch (Exception ex) {
    Debug.LogError($"Form submission error: {ex.Message}");
}
```

### Downloading Binary Data

```csharp
// Download an image file
try {
    var imageData = await networkManager.RequestBinary(new NetworkRequest {
        Url = "https://example.com/images/logo.png",
        Method = "GET",
        OnProgress = (progress) => {
            Debug.Log($"Download progress: {progress * 100}%");
        }
    });
    
    // Create a texture from the downloaded data
    var texture = new Texture2D(2, 2);
    texture.LoadImage(imageData);
    
    // Apply the texture to a material
    renderer.material.mainTexture = texture;
} catch (Exception ex) {
    Debug.LogError($"Failed to download image: {ex.Message}");
}
```

### Request with Timeout and Cancellation

```csharp
// Make a request that can be cancelled
string requestId = Guid.NewGuid().ToString();

try {
    var task = networkManager.Request(new NetworkRequest {
        Url = "https://api.example.com/long-operation",
        Method = "GET",
        Timeout = 60000, // 1 minute timeout
        RequestId = requestId
    });
    
    // Cancel the request after 10 seconds if it's still running
    CancelAfterDelay(requestId, 10000);
    
    var response = await task;
    Debug.Log("Request completed successfully");
} catch (OperationCanceledException) {
    Debug.Log("Request was cancelled");
} catch (Exception ex) {
    Debug.LogError($"Request failed: {ex.Message}");
}

// Helper method to cancel a request after a delay
async void CancelAfterDelay(string id, int delayMs) {
    await Task.Delay(delayMs);
    networkManager.CancelRequest(id);
}
```

### Handling Network Connectivity

```csharp
// Subscribe to online status changes
networkManager.OnOnlineStatusChanged += HandleOnlineStatusChanged;

// Check current online status
bool isOnline = await networkManager.IsOnline();
if (isOnline) {
    StartNetworkOperations();
} else {
    ShowOfflineMessage();
}

// Handle online status changes
void HandleOnlineStatusChanged(bool online) {
    if (online) {
        Debug.Log("Device is online. Resuming network operations...");
        HideOfflineMessage();
        RetryFailedRequests();
    } else {
        Debug.Log("Device went offline. Pausing network operations...");
        ShowOfflineMessage();
        PauseNetworkOperations();
    }
}
```

## JavaScript API

In JavaScript, the Network module provides these methods:

```javascript
// Make a simple request
Energy8JSPluginTools.Network.request({
    url: "https://api.example.com/data",
    method: "GET",
    headers: { "Accept": "application/json" }
}).then(response => {
    console.log("Response:", response);
}).catch(error => {
    console.error("Error:", error);
});

// Post JSON data
Energy8JSPluginTools.Network.request({
    url: "https://api.example.com/users",
    method: "POST",
    body: JSON.stringify({ name: "John", email: "john@example.com" }),
    headers: { "Content-Type": "application/json" }
});

// Upload form data
const formData = new FormData();
formData.append("username", "player1");
formData.append("score", "1250");

Energy8JSPluginTools.Network.request({
    url: "https://api.example.com/submit-score",
    method: "POST",
    body: formData
});

// Check online status
const isOnline = Energy8JSPluginTools.Network.isOnline();
console.log("Online status:", isOnline);

// Cancel all pending requests
Energy8JSPluginTools.Network.cancelAllRequests();
```

## Best Practices

1. Always handle network errors and provide appropriate feedback to users.
2. Set reasonable timeout values based on the expected response time of the endpoint.
3. Implement retry mechanisms for transient failures.
4. Use request IDs for requests that might need to be cancelled.
5. Handle online/offline transitions gracefully with appropriate UI feedback.
6. Consider caching responses for frequently accessed resources or offline use.
7. Use typed models with RequestJson<T> for better type safety and code readability.
8. Be mindful of CORS (Cross-Origin Resource Sharing) restrictions in browser environments.
9. For large file uploads or downloads, use the progress callback to provide user feedback.
10. Cancel pending requests when a scene or view is unloaded to prevent memory leaks.