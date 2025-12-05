# 📁 SweetTypeTone Project Structure

## Overview
Clean, organized project structure for SweetTypeTone - a mechanical keyboard sound application for Linux.

---

## 📂 Directory Structure

```
SweetTypeTone/
├── .github/                    # GitHub configuration
│   ├── CONTRIBUTING.md         # Contribution guidelines
│   ├── FUNDING.yml            # Sponsorship info
│   ├── ISSUE_TEMPLATE/        # Issue templates
│   └── workflows/             # CI/CD workflows
│       ├── build.yml          # Build workflow
│       └── release.yml        # Release workflow
│
├── BundledSoundPacks/         # Pre-installed sound packs (21 packs)
│   ├── README.md              # Sound pack documentation
│   ├── cherrymx-*/            # Cherry MX switches
│   ├── nk-cream/              # NovelKeys Cream
│   ├── holy-pandas/           # Holy Pandas
│   └── ...                    # More sound packs
│
├── docs/                      # Documentation
│   ├── APPIMAGE.md            # AppImage documentation
│   ├── DEPLOYMENT_GUIDE.md    # Deployment guide
│   ├── DEPLOYMENT_SUMMARY.md  # Quick deployment reference
│   ├── DESKTOP_FILE_UPDATE.md # Desktop file documentation
│   ├── PERFORMANCE_OPTIMIZATIONS.md  # Performance details
│   ├── PERFORMANCE_SUMMARY.md        # Performance summary
│   ├── RELEASE_CHECKLIST.md          # Release checklist
│   └── SOUND_PACK_TROUBLESHOOTING.md # Troubleshooting guide
│
├── install/                   # System installation files
│   ├── sweettypetone-setup    # Permission setup script
│   └── sweettypetone.policy   # Polkit policy
│
├── scripts/                   # Build and utility scripts
│   ├── build-appimage.sh      # Build AppImage
│   ├── build-binary.sh        # Build binary archive
│   ├── setup-permissions.sh   # Setup permissions
│   └── update-desktop-file.sh # Update desktop file
│
├── src/                       # Source code
│   ├── SweetTypeTone/         # Main application
│   │   ├── App.axaml          # Application entry
│   │   ├── Assets/            # Icons and resources
│   │   ├── ViewModels/        # MVVM ViewModels
│   │   ├── Views/             # MVVM Views
│   │   └── SweetTypeTone.csproj
│   │
│   └── SweetTypeTone.Core/    # Core library
│       ├── Interfaces/        # Service interfaces
│       ├── Models/            # Data models
│       ├── Services/          # Service implementations
│       └── SweetTypeTone.Core.csproj
│
├── .gitignore                 # Git ignore rules
├── CHANGELOG.md               # Version history
├── install.sh                 # User installation script
├── LICENSE                    # MIT License
├── PROJECT_STRUCTURE.md       # This file
├── README.md                  # Main documentation
├── sweettypetone.desktop      # Desktop entry template
└── SweetTypeTone.sln          # Solution file
```

---

## 📄 Key Files

### Root Directory

| File | Purpose |
|------|---------|
| `README.md` | Main project documentation |
| `CHANGELOG.md` | Version history and changes |
| `LICENSE` | MIT License |
| `SweetTypeTone.sln` | Visual Studio solution |
| `.gitignore` | Git ignore patterns |
| `install.sh` | User installation script |
| `sweettypetone.desktop` | Desktop entry template |

### Documentation (`docs/`)

| File | Purpose |
|------|---------|
| `APPIMAGE.md` | AppImage packaging details |
| `DEPLOYMENT_GUIDE.md` | Complete deployment guide |
| `DEPLOYMENT_SUMMARY.md` | Quick deployment reference |
| `PERFORMANCE_OPTIMIZATIONS.md` | Technical performance details |
| `PERFORMANCE_SUMMARY.md` | Performance improvements summary |
| `RELEASE_CHECKLIST.md` | Steps for creating releases |
| `SOUND_PACK_TROUBLESHOOTING.md` | Sound pack issues and fixes |

### Scripts (`scripts/`)

| Script | Purpose |
|--------|---------|
| `build-appimage.sh` | Build AppImage with bundled packs |
| `build-binary.sh` | Build binary archive with bundled packs |
| `setup-permissions.sh` | Setup input group permissions |
| `update-desktop-file.sh` | Update desktop file and icon |

### Source Code (`src/`)

| Directory | Purpose |
|-----------|---------|
| `SweetTypeTone/` | Main GUI application (Avalonia UI) |
| `SweetTypeTone.Core/` | Core library (audio, input, services) |

---

## 🚫 Ignored Files (.gitignore)

### Build Artifacts
- `*.AppImage` - AppImage packages
- `*.tar.gz` - Binary archives
- `SweetTypeTone.AppDir/` - AppImage build directory
- `publish/` - .NET publish output
- `build/` - Build output
- `bin/`, `obj/` - .NET build artifacts

### Development Files
- `.vs/`, `.vscode/`, `.idea/` - IDE files
- `*.log` - Log files
- `squashfs-root/` - Extracted AppImage

### Test Scripts
- `test-appimage.sh` - Testing script
- `debug-soundpacks.sh` - Debug script
- `appimage-test.log` - Test logs

---

## 📦 Build Outputs

### AppImage Build
```bash
./scripts/build-appimage.sh
```
**Output:** `SweetTypeTone-1.1.0-x86_64.AppImage` (~34 MB)

### Binary Archive Build
```bash
./scripts/build-binary.sh
```
**Output:** `SweetTypeTone-1.1.0-linux-x64.tar.gz` (~34 MB)

---

## 🎹 Bundled Sound Packs

**Location:** `BundledSoundPacks/`

**Count:** 21 sound packs

**Format:** Mechvibes-compatible (OGG/WAV)

**Packs Include:**
- Cherry MX switches (Black, Blue, Brown, Red - ABS/PBT)
- NovelKeys Cream
- Holy Pandas
- Topre Purple Hybrid
- Everglide Crystal Purple & Oreo
- Fallout Terminal
- And more!

---

## 🔧 Development Workflow

### 1. Clone Repository
```bash
git clone https://github.com/amuza2/SweetTypeTone.git
cd SweetTypeTone
```

### 2. Build for Development
```bash
dotnet build
dotnet run --project src/SweetTypeTone/SweetTypeTone.csproj
```

### 3. Build for Release
```bash
# AppImage
./scripts/build-appimage.sh

# Binary archive
./scripts/build-binary.sh
```

### 4. Test
```bash
# Run AppImage
chmod +x SweetTypeTone-*.AppImage
./SweetTypeTone-*.AppImage

# Extract and test binary
tar -xzf SweetTypeTone-*.tar.gz -C /tmp/test
cd /tmp/test && ./install.sh
```

---

## 📋 File Organization Principles

### ✅ Keep in Root
- Essential documentation (README, CHANGELOG, LICENSE)
- Build configuration (solution file, .gitignore)
- User-facing scripts (install.sh)

### 📁 Organize in Subdirectories
- **docs/** - All documentation
- **scripts/** - All build/utility scripts
- **src/** - All source code
- **BundledSoundPacks/** - Sound packs
- **.github/** - GitHub configuration

### 🚫 Never Commit
- Build artifacts (AppImage, tar.gz)
- IDE files (.vscode, .idea)
- Build directories (bin, obj, publish)
- Test scripts and logs
- Temporary files

---

## 🎯 Clean Repository Benefits

1. **Easy Navigation** - Clear structure, easy to find files
2. **Professional** - Clean root directory
3. **Maintainable** - Organized documentation
4. **CI/CD Friendly** - Scripts in known locations
5. **User Friendly** - Clear README, organized docs

---

## 📊 Repository Statistics

- **Total Lines of Code:** ~15,000
- **Languages:** C# (95%), Shell (3%), Markdown (2%)
- **Dependencies:** .NET 10, Avalonia UI, OpenAL, NVorbis
- **Bundled Assets:** 21 sound packs (~8 MB)
- **Documentation:** 8 markdown files
- **Build Scripts:** 4 shell scripts

---

**Last Updated:** 2025-12-05  
**Version:** 1.1.0  
**Status:** ✅ Clean and Organized
