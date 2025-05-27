# Firebase.Auth Package

Firebase Authentication package for Unity, following the Energy8 Firebase architecture patterns.

## Overview

This package provides Firebase Authentication functionality for Unity projects, with support for multiple platforms including WebGL, Native iOS/Android, and Unity Editor.

## Architecture

The package follows the same architectural patterns as `Firebase.Core`:

- **Core Module**: Platform-agnostic authentication logic and models
- **Native Module**: Native platform implementations (iOS/Android)
- **WebGL Module**: WebGL-specific implementations with JavaScript interop
- **Editor Module**: Unity Editor tools and configuration

## Key Components

### Authentication Models
- `FirebaseUser`: Represents an authenticated user
- `AuthResult`: Result of authentication operations
- `AuthCredential`: Authentication credentials
- `UserMetadata`: User creation and sign-in timestamps

### Authentication Providers
- `EmailAuthProvider`: Email/password authentication
- `GoogleAuthProvider`: Google Sign-In
- `AnonymousAuthProvider`: Anonymous authentication

### Core API
- `FirebaseAuth`: Main static entry point
- `IFirebaseAuthApi`: Platform provider interface
- `BaseFirebaseAuthProvider`: Base implementation for all providers

## Platform Support

### WebGL
- JavaScript interop through `FirebaseAuthPlugin`
- Full Firebase JS SDK integration
- Emulator support for development

### Native Platforms
- Stub implementations ready for native Firebase SDK integration
- Automatic provider registration
- Platform-specific initialization

### Editor
- Configuration management through Project Settings
- Testing and debugging utilities
- Emulator connection tools

## Configuration

The package includes a comprehensive configuration system:

### General Settings
- Auto Sign-In: Automatically sign in users on app start
- Persist User: Keep users signed in between sessions

### Emulator Settings
- Use Emulator: Connect to Firebase Auth Emulator
- Host/Port: Emulator connection details

### Provider Settings
- Enabled Providers: Configure which authentication methods are available

## Usage

### Basic Setup

```csharp
// Initialize Firebase Auth
await FirebaseAuth.InitializeAsync();

// Sign in with email/password
var result = await FirebaseAuth.SignInWithEmailAndPasswordAsync("user@example.com", "password");
if (result?.User != null)
{
    Debug.Log($"Signed in: {result.User.Email}");
}

// Sign in anonymously
var anonResult = await FirebaseAuth.SignInAnonymouslyAsync();

// Sign out
await FirebaseAuth.SignOutAsync();
```

### Unity Component Integration

Use the `FirebaseAuthManager` component for easy Unity integration:

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private FirebaseAuthManager authManager;
    
    private void Start()
    {
        authManager.OnUserSignedIn.AddListener(OnUserSignedIn);
        authManager.OnUserSignedOut.AddListener(OnUserSignedOut);
    }
    
    private void OnUserSignedIn(string userId)
    {
        Debug.Log($"User signed in: {userId}");
    }
    
    private void OnUserSignedOut()
    {
        Debug.Log("User signed out");
    }
}
```

### Event Handling

```csharp
// Listen for authentication state changes
FirebaseAuth.OnAuthStateChanged += (user) =>
{
    if (user != null)
    {
        Debug.Log($"User signed in: {user.Uid}");
    }
    else
    {
        Debug.Log("User signed out");
    }
};
```

## Dependencies

- `Energy8.Firebase.Core`: Core Firebase functionality
- `Energy8.WebGL.PluginPlatform`: WebGL plugin infrastructure

## Development Tools

### Unity Menu Items
- `Firebase/Auth/Create Configuration`: Create authentication configuration
- `Firebase/Auth/Open Project Settings`: Open Firebase Auth settings
- `Firebase/Auth/Test Connection`: Test Firebase connection
- `Firebase/Auth/Clear Cached User`: Clear cached authentication data

### Project Settings
Navigate to `Project Settings > Firebase > Auth` to configure:
- Authentication providers
- Emulator settings
- Auto sign-in behavior
- User persistence options

## WebGL Integration

For WebGL builds, ensure Firebase JS SDK is included in your HTML template:

```html
<script src="https://www.gstatic.com/firebasejs/9.0.0/firebase-app.js"></script>
<script src="https://www.gstatic.com/firebasejs/9.0.0/firebase-auth.js"></script>
```

The package includes JavaScript plugins for seamless Unity-Firebase integration.

## Error Handling

The package provides comprehensive error handling through `FirebaseAuthException`:

```csharp
try
{
    var result = await FirebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
}
catch (FirebaseAuthException ex)
{
    switch (ex.ErrorCode)
    {
        case AuthErrorCode.UserNotFound:
            Debug.LogError("User not found");
            break;
        case AuthErrorCode.WrongPassword:
            Debug.LogError("Invalid password");
            break;
        default:
            Debug.LogError($"Auth error: {ex.Message}");
            break;
    }
}
```

## Package Structure

```
io.energy8.firebase.auth/
├── Core/
│   ├── Api/                    # Core interfaces
│   ├── Components/             # Unity components
│   ├── Configuration/          # Configuration system
│   ├── Models/                 # Data models
│   └── Providers/             # Authentication providers
├── Editor/
│   ├── Settings/              # Project settings UI
│   └── *EditorUtilities.cs   # Editor tools
├── Native/
│   └── Providers/             # Native implementations
├── WebGL/
│   ├── Plugins/               # JavaScript plugins
│   ├── Providers/             # WebGL implementations
│   └── Scripts/               # WebGL utilities
└── package.json               # Package definition
```

## Version

Current version: 1.0.0

Compatible with Unity 2022.3 LTS and higher.
