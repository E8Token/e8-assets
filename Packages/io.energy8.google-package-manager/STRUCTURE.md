# Package Structure

```
io.energy8.google-package-manager/
├── package.json                                    # Package metadata
├── README.md                                       # Package documentation
├── STRUCTURE.md                                    # This file
└── Editor/
    ├── Energy8.GooglePackageManager.Editor.asmdef  # Assembly definition
    ├── Core/
    │   ├── GooglePackageManager.cs                 # Main manager class
    │   └── GooglePackageDownloader.cs             # Package download & install logic
    ├── Data/
    │   ├── GooglePackageData.cs                   # Data classes & models
    │   └── GooglePackageSettings.cs               # Settings ScriptableObject
    ├── UI/
    │   └── GooglePackageManagerWindow.cs          # Main UI window
    └── Utilities/
        ├── ManifestManager.cs                     # Package manifest manipulation
        └── GooglePackageParser.cs                # Package info parsing from web
```

## Key Features

- **TGZ Only Support**: Only supports .tgz package format installation
- **Local File References**: Packages are cached locally and referenced via file: URLs
- **Automatic Updates**: Background checking for package updates
- **Unity Package Manager Integration**: Works through Unity's built-in Package Manager
- **Web Scraping**: Parses package information from Google's developer pages

## Dependencies

- Unity 2020.1+
- Newtonsoft.Json (built into Unity)
- UnityEngine.Networking
- UnityEditor (Editor-only package)

## Architecture

1. **GooglePackageManager**: Central controller class
2. **GooglePackageDownloader**: Handles downloading and installing packages
3. **GooglePackageParser**: Parses package data from web sources
4. **ManifestManager**: Manages Unity's package manifest file
5. **GooglePackageManagerWindow**: Main UI interface
6. **GooglePackageSettings**: Configuration and settings management
