# Tests for JS Plugin Tools

This directory contains unit tests for Energy8.JSPluginTools modules.

## Structure

The tests are organized into two main categories:

- **Editor Tests** - Tests that run in the Unity Editor environment
- **PlayMode Tests** - Tests that require the Unity runtime environment

## Editor Tests

Editor tests cover the following areas:

- `ExternalCommunicatorTests.cs` - Tests for the ExternalCommunicator class
- `JSMessageTests.cs` - Tests for serialization/deserialization of JSMessage objects
- `JSMessageHandlerTests.cs` - Tests for the IJSMessageHandler interface implementation
- `JSCallHandlersTests.cs` - Tests for the internal JS call handlers
- `ErrorHandlingTests.cs` - Tests for error handling and edge cases

## PlayMode Tests

PlayMode tests cover the following areas:

- `CommunicationPluginManagerTests.cs` - Tests for integration between Communication API and PluginManager
- `JSIntegrationTests.cs` - Tests for two-way communication between Unity and JavaScript
- `EnvironmentTests.cs` - Tests for proper behavior in different environments (WebGL/Editor)
- `ContextInteractionTests.cs` - Tests for contextual interaction with Unity objects and components

## Running Tests

### Using Unity Test Runner
1. Open the Unity Test Runner window (Window > General > Test Runner)
2. Select the "EditMode" tab for editor tests or "PlayMode" tab for runtime tests
3. Click "Run All" or select specific tests to run

### Using Command Line (PowerShell)
To run tests from the command line, use the provided `RunTests.ps1` script:

```powershell
# Running Edit Mode tests
.\RunTests.ps1 -EditMode

# Running Play Mode tests
.\RunTests.ps1 -PlayMode

# Running all tests with code coverage
.\RunTests.ps1 -All -Coverage

# Running tests for a specific category
.\RunTests.ps1 -All -Category "JSPluginTools.Core"
```

Note: The script requires Unity installation path to be updated if different from default.

### Important PowerShell Script Fixes
If you encounter issues with the PowerShell script, make the following modifications:

1. Rename function `Run-Tests` to `Start-TestRun` (approved PowerShell verb)
2. Rename variable `$args` to `$unityArgs` (avoid built-in variable conflict)

### WebGL Testing
For WebGL testing:
1. Build the project for WebGL
2. Upload to a web server or use the Unity WebGL test server
3. Run the tests in the built application

## Testing Considerations

- Editor tests use simulated JavaScript calls
- PlayMode tests can use both simulated and real JavaScript calls (when running in WebGL)
- Some tests are platform-specific and will only run on certain platforms

## Adding New Tests

When adding new tests:

1. Follow the naming convention: `{Component}Tests.cs`
2. Place editor tests in the Tests/Editor directory
3. Place runtime tests in the Tests/Runtime directory
4. Update this README with information about the new tests

## Notes

- Tests marked with `[UnityPlatform(RuntimePlatform.WebGLPlayer)]` will only run in a WebGL build
- Tests use reflection to access private members when necessary
- Mock implementations are provided for testing components that depend on external systems
