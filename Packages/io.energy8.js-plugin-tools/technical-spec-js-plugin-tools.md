# Technical Specification  
**Plugin Name:** `io.energy8.js-plugin-tools`  
**C# Namespace:** `Energy8.JSPluginTools`  
**Target Platform:** Unity WebGL  
**Initial Module:** Core

---

## Overview

The goal is to develop a modular Unity WebGL plugin `io.energy8.js-plugin-tools`, which enables rapid and clean integration of JavaScript plugins into Unity via a common communication framework. It will start with a single base module – **Core** – providing the fundamental APIs and architecture.

---

## Architecture

### C# Side (`Energy8.JSPluginTools`)

- **PluginBase:** All external C# plugins inherit from `JSPluginBase`.
- **ModuleBase:** All plugin modules inherit from `JSPluginModule`.
- **Core Module:** Manages plugin registration and automatic GameObject creation.
- **Namespaces:** All code must be strictly within `Energy8.JSPluginTools`.

### JavaScript Side (via `.jslib`)

- All JavaScript code is bundled via Unity’s `.jslib` mechanism.
- Do **not** expose any API on the global `window` object.
- All JS functionality must be enclosed in a module pattern (e.g., UMD, IIFE, or ES Module compiled structure).
- Internal communication uses Unity `SendMessage` and `Module` or `mergeInto(LibraryManager.library)` as needed.

---

## Core Module Requirements

### Features

1. **Plugin Registration**
   - Allow C# plugins to be registered by name.
   - Automatically instantiate a dedicated GameObject per plugin instance.

2. **JS ↔ C# Communication**
   - Robust two-way messaging:
     - JS can call C# plugin methods (via `SendMessage`).
     - C# can call back JS handlers using standard interop calls (`Application.ExternalCall`, `InvokeJS`, or equivalent).
   - Must support message routing through a core dispatcher.
   - All payloads serialized as JSON.

3. **Debug Mode**
   - Enable a debug mode that exposes internal state (plugin list, last messages) for inspection via DevTools console using a debug hook inside the JS module (but not exposed globally).

---

## Base Classes (C#)

### JSPluginBase

```csharp
namespace Energy8.JSPluginTools
{
    public abstract class JSPluginBase : MonoBehaviour
    {
        /// <summary>
        /// Gets the unique plugin identifier.
        /// </summary>
        public abstract string PluginName { get; }

        /// <summary>
        /// Initializes the plugin instance.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Handles a message from the JavaScript side.
        /// </summary>
        public virtual void OnMessage(string method, string payload) { }
    }
}
```

### JSPluginModule

```csharp
namespace Energy8.JSPluginTools
{
    public abstract class JSPluginModule
    {
        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        /// Called once during module registration.
        /// </summary>
        public virtual void InitializeModule() { }
    }
}
```

---

## Documentation Requirements

- **XML Documentation:** All public classes, properties, and methods must include clear XML documentation in English.
- **Markdown Documentation:** Each module must include a `README.md` (or `[ModuleName].md`) containing:
  - Purpose
  - Integration instructions
  - Public API (JS ↔ C#)
  - No code samples or examples included.

---

## Constraints

- **No `window.*` namespace usage.**
- **No global JS leaks.**
- JS APIs must be registered through Unity’s `.jslib` system and kept fully modular.
- Minimum supported Unity version: `2021.3 LTS`.
- All modules must be plug-and-play and require no modification to the Core module.
- No external libraries or dependencies allowed.
- No test scenes, demo assets, or sample code in the plugin.

---

## Extensibility

- New modules must inherit from `JSPluginModule`.
- New plugins must inherit from `JSPluginBase`.
- All new modules must be self-contained and documented.
- Core module must remain decoupled and unaware of specific modules.

---

## Summary

The `io.energy8.js-plugin-tools` plugin lays the foundation for robust JS ↔ C# interop in Unity WebGL through a modular architecture. It emphasizes structure, clarity, and forward compatibility without polluting the global scope.
