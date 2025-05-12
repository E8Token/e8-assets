# Storage Module Documentation

## Overview

The Storage module provides a unified interface for accessing browser storage mechanisms in Unity WebGL applications. It enables storing and retrieving data persistently using LocalStorage, SessionStorage, and IndexedDB, giving your Unity applications the ability to save user preferences, game progress, and other data between sessions.

## Features

- **Multiple Storage Types**: Access LocalStorage, SessionStorage, and IndexedDB
- **Simple Key-Value Storage**: Store and retrieve string data easily
- **Structured Data Storage**: Save and load complex data structures with automatic serialization
- **Storage Management**: Check storage availability, remaining space, and clear stored data
- **IndexedDB Support**: Store larger amounts of data with database capabilities
- **Storage Size Information**: Get storage usage and available space information

## Classes and Components

### IStorageService

The main interface for storage operations:

```csharp
public interface IStorageService
{
    Task<bool> IsAvailable(StorageType storageType);
    Task<string> GetItem(string key, StorageType storageType = StorageType.Local);
    Task<T> GetItem<T>(string key, StorageType storageType = StorageType.Local);
    Task SetItem(string key, string value, StorageType storageType = StorageType.Local);
    Task SetItem<T>(string key, T value, StorageType storageType = StorageType.Local);
    Task RemoveItem(string key, StorageType storageType = StorageType.Local);
    Task<bool> HasKey(string key, StorageType storageType = StorageType.Local);
    Task<List<string>> GetAllKeys(StorageType storageType = StorageType.Local);
    Task Clear(StorageType storageType = StorageType.Local);
    Task<long> GetStorageSize(StorageType storageType = StorageType.Local);
    Task<long> GetRemainingSpace(StorageType storageType = StorageType.Local);
    
    // IndexedDB specific methods
    Task OpenDatabase(string dbName, int version = 1);
    Task CreateObjectStore(string storeName, string keyPath = "id");
    Task AddToStore<T>(string storeName, T data, string key = null);
    Task<T> GetFromStore<T>(string storeName, string key);
    Task RemoveFromStore(string storeName, string key);
    Task<List<T>> GetAllFromStore<T>(string storeName);
    Task ClearStore(string storeName);
}
```

### StorageService

The implementation of `IStorageService` that provides storage capabilities through the Communication module.

### IStorageManager

Higher-level interface for storage operations:

```csharp
public interface IStorageManager
{
    void Initialize(IPluginCore core);
    bool IsInitialized { get; }
    event Action OnInitialized;
    
    Task<bool> IsAvailable(StorageType storageType);
    Task<string> GetString(string key, StorageType storageType = StorageType.Local);
    Task<T> GetObject<T>(string key, StorageType storageType = StorageType.Local);
    Task SetString(string key, string value, StorageType storageType = StorageType.Local);
    Task SetObject<T>(string key, T value, StorageType storageType = StorageType.Local);
    Task Delete(string key, StorageType storageType = StorageType.Local);
    Task<bool> Exists(string key, StorageType storageType = StorageType.Local);
    Task<List<string>> GetKeys(StorageType storageType = StorageType.Local);
    Task ClearAll(StorageType storageType = StorageType.Local);
    Task<long> GetUsedSpace(StorageType storageType = StorageType.Local);
    Task<long> GetFreeSpace(StorageType storageType = StorageType.Local);
    
    // IndexedDB operations
    Task InitializeDatabase(string dbName, int version = 1);
    Task CreateStore(string storeName, string keyPath = "id");
    Task SaveToStore<T>(string storeName, T data, string key = null);
    Task<T> LoadFromStore<T>(string storeName, string key);
    Task DeleteFromStore(string storeName, string key);
    Task<List<T>> LoadAllFromStore<T>(string storeName);
    Task ClearStore(string storeName);
}
```

### StorageManager

The implementation of `IStorageManager` that coordinates storage operations through the plugin core.

### StorageServiceBehaviour

A MonoBehaviour wrapper for StorageService that allows finding it with `FindObjectOfType`.

### StorageType

Enum defining the available storage mechanisms:

```csharp
public enum StorageType
{
    Local,    // LocalStorage (persistent across sessions)
    Session,  // SessionStorage (cleared when the session ends)
    IndexedDB // Database storage for larger data
}
```

## Usage

### Basic Storage Operations

```csharp
// Initialize the storage manager
var storageManager = new StorageManager();
storageManager.Initialize(pluginCore);

// Check if storage is available
bool isAvailable = await storageManager.IsAvailable(StorageType.Local);
if (!isAvailable) {
    Debug.LogWarning("LocalStorage is not available. Storage features will be disabled.");
    return;
}

// Store a simple string
await storageManager.SetString("username", "Player1", StorageType.Local);

// Retrieve the stored string
string username = await storageManager.GetString("username", StorageType.Local);
Debug.Log($"Welcome back, {username}!");

// Check if a key exists
bool hasHighScore = await storageManager.Exists("highScore", StorageType.Local);
if (hasHighScore) {
    string highScore = await storageManager.GetString("highScore", StorageType.Local);
    Debug.Log($"Your high score: {highScore}");
}

// Delete a stored item
await storageManager.Delete("temporary_data", StorageType.Local);

// Clear all data in storage
await storageManager.ClearAll(StorageType.Session);
```

### Storing and Loading Complex Objects

```csharp
// Define a class for the player data
[Serializable]
public class PlayerData
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public List<string> Inventory { get; set; }
    public Dictionary<string, int> Skills { get; set; }
}

// Save player data
async void SavePlayerData(PlayerData data)
{
    try {
        await storageManager.SetObject("playerData", data, StorageType.Local);
        Debug.Log("Player data saved successfully");
    } catch (Exception ex) {
        Debug.LogError($"Failed to save player data: {ex.Message}");
    }
}

// Load player data
async Task<PlayerData> LoadPlayerData()
{
    try {
        // Check if data exists
        if (await storageManager.Exists("playerData", StorageType.Local)) {
            var data = await storageManager.GetObject<PlayerData>("playerData", StorageType.Local);
            Debug.Log($"Loaded player: {data.Name}, Level: {data.Level}");
            return data;
        } else {
            Debug.Log("No saved player data found. Creating new player.");
            return CreateNewPlayer();
        }
    } catch (Exception ex) {
        Debug.LogError($"Failed to load player data: {ex.Message}");
        return CreateNewPlayer();
    }
}

// Create a new player
PlayerData CreateNewPlayer()
{
    return new PlayerData {
        Name = "New Player",
        Level = 1,
        Experience = 0,
        Inventory = new List<string>(),
        Skills = new Dictionary<string, int>()
    };
}
```

### Using IndexedDB for Larger Data

```csharp
// Initialize IndexedDB database
async void InitializeGameDatabase()
{
    try {
        await storageManager.InitializeDatabase("gameData", 1);
        await storageManager.CreateStore("saveGames", "saveId");
        await storageManager.CreateStore("gameAssets", "assetId");
        Debug.Log("Game database initialized successfully");
    } catch (Exception ex) {
        Debug.LogError($"Failed to initialize database: {ex.Message}");
    }
}

// Save a game
async void SaveGame(SaveGame saveGame)
{
    try {
        // Generate a unique ID if not provided
        if (string.IsNullOrEmpty(saveGame.SaveId)) {
            saveGame.SaveId = Guid.NewGuid().ToString();
        }
        
        // Save to IndexedDB
        await storageManager.SaveToStore("saveGames", saveGame, saveGame.SaveId);
        Debug.Log($"Game saved with ID: {saveGame.SaveId}");
    } catch (Exception ex) {
        Debug.LogError($"Failed to save game: {ex.Message}");
    }
}

// Load all saved games
async Task<List<SaveGame>> LoadAllSavedGames()
{
    try {
        var savedGames = await storageManager.LoadAllFromStore<SaveGame>("saveGames");
        Debug.Log($"Loaded {savedGames.Count} saved games");
        return savedGames;
    } catch (Exception ex) {
        Debug.LogError($"Failed to load saved games: {ex.Message}");
        return new List<SaveGame>();
    }
}

// Load a specific saved game
async Task<SaveGame> LoadGame(string saveId)
{
    try {
        var saveGame = await storageManager.LoadFromStore<SaveGame>("saveGames", saveId);
        if (saveGame != null) {
            Debug.Log($"Loaded save game: {saveGame.SaveName}");
            return saveGame;
        } else {
            Debug.LogWarning($"Save game with ID {saveId} not found");
            return null;
        }
    } catch (Exception ex) {
        Debug.LogError($"Failed to load save game: {ex.Message}");
        return null;
    }
}

// Delete a saved game
async void DeleteSavedGame(string saveId)
{
    try {
        await storageManager.DeleteFromStore("saveGames", saveId);
        Debug.Log($"Save game {saveId} deleted");
    } catch (Exception ex) {
        Debug.LogError($"Failed to delete save game: {ex.Message}");
    }
}
```

### Storage Size Management

```csharp
// Check storage usage and available space
async void CheckStorageSpace()
{
    try {
        long usedSpace = await storageManager.GetUsedSpace(StorageType.Local);
        long freeSpace = await storageManager.GetFreeSpace(StorageType.Local);
        
        Debug.Log($"Storage usage: {usedSpace / 1024} KB used, {freeSpace / 1024} KB available");
        
        // Warn the user if storage is almost full
        if (freeSpace < 1024 * 100) { // Less than 100 KB remaining
            ShowStorageFullWarning();
        }
    } catch (Exception ex) {
        Debug.LogError($"Failed to check storage space: {ex.Message}");
    }
}

// Clean up old data if storage is getting full
async void CleanupStorage()
{
    try {
        // Get all keys in storage
        var keys = await storageManager.GetKeys(StorageType.Local);
        
        // Find temporary or old keys (based on your naming convention)
        var keysToDelete = keys.Where(k => k.StartsWith("temp_") || k.StartsWith("cache_")).ToList();
        
        // Delete those keys
        foreach (var key in keysToDelete) {
            await storageManager.Delete(key, StorageType.Local);
            Debug.Log($"Deleted temporary key: {key}");
        }
        
        Debug.Log($"Cleanup complete. Removed {keysToDelete.Count} items.");
    } catch (Exception ex) {
        Debug.LogError($"Storage cleanup failed: {ex.Message}");
    }
}
```

## JavaScript API

In JavaScript, the Storage module provides these methods:

```javascript
// Check if storage is available
const isLocalStorageAvailable = Energy8JSPluginTools.Storage.isAvailable("Local");
console.log("LocalStorage available:", isLocalStorageAvailable);

// Store an item
Energy8JSPluginTools.Storage.setItem("username", "Player1", "Local");

// Retrieve an item
const username = Energy8JSPluginTools.Storage.getItem("username", "Local");
console.log("Username:", username);

// Check if a key exists
const hasHighScore = Energy8JSPluginTools.Storage.hasKey("highScore", "Local");
console.log("Has high score:", hasHighScore);

// Get all keys
const keys = Energy8JSPluginTools.Storage.getAllKeys("Local");
console.log("All keys:", keys);

// Get storage size information
const storageSize = Energy8JSPluginTools.Storage.getStorageSize("Local");
const remainingSpace = Energy8JSPluginTools.Storage.getRemainingSpace("Local");
console.log("Storage size:", storageSize, "bytes");
console.log("Remaining space:", remainingSpace, "bytes");

// IndexedDB operations
Energy8JSPluginTools.Storage.openDatabase("gameData", 1);
Energy8JSPluginTools.Storage.createObjectStore("saveGames", "saveId");
Energy8JSPluginTools.Storage.addToStore("saveGames", { saveId: "save1", name: "My Save" });
const save = Energy8JSPluginTools.Storage.getFromStore("saveGames", "save1");
```

## Best Practices

1. Always check storage availability before using it, as some browsers may have storage disabled.
2. Use LocalStorage for small amounts of data that need to persist between sessions.
3. Use SessionStorage for temporary data that should be cleared when the browser is closed.
4. Use IndexedDB for larger datasets, complex queries, or when you need to store more than 5MB of data.
5. Handle storage exceptions gracefully, as operations can fail due to storage limits, permissions, or other errors.
6. Implement a cleanup strategy for old or unused data to avoid filling up the user's storage.
7. Consider offering an export/import feature for important user data to prevent loss.
8. Use versioning for your stored data format to handle schema migrations when your application evolves.
9. Be mindful of the storage limits (usually around 5MB for LocalStorage and SessionStorage).
10. Avoid storing sensitive information as browser storage is not secure from malicious scripts.