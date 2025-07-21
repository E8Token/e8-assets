---
applyTo: '**'
---

## Unity Project Rules & C# Guidelines

This is a Unity project. Follow these strict rules when generating or reviewing code.

---

### Read-Only Files (Never Modify)

- `*.meta`, `*.unity`, `*.prefab`, `*.asset`  
- `*.csproj`, `*.sln`  
- Folders: `Library/`, `Temp/`, `Obj/`, `Build/`, `ProjectSettings/`  
- `Packages/manifest.json`  

All changes to these must be made manually by the user in the Unity Editor.

---

### Editor & Build Limits

- Never run Unity Editor via CLI or scripts  
- Do not build, import, or export assets  
- Never create or edit Unity assets (`.prefab`, `.mat`, `.shader`, etc.)  
- Only create `.cs` scripts or plain text configs when explicitly asked

---

### Project Structure

- `Assets/Scripts/` — C# logic  
- `Assets/Scenes/`, `Prefabs/` — Read-only  
- `Assets/Editor/` — Editor tools  
- `Assets/Plugins/` — External libraries  
- `Packages/` — Dependencies (read-only)

---

### External Packages

- Do not assume black-box behavior  
- Ask user for purpose, inputs/outputs, and side effects

---

### C# Style (Unity)

- Classes, Methods: `PascalCase`  
- Fields: `camelCase`, `[SerializeField]`: `_camelCase`  
- Interfaces: `IName`  
- Method order: `Awake` → `Start` → `Update` → public → private  
- Public methods must have XML docs, for example:

  ```csharp
  /// <summary>Deals damage.</summary>
  public void TakeDamage(int amount) { ... }
