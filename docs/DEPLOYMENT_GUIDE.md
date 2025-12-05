# 🚀 SweetTypeTone Deployment Guide

This guide covers building and deploying SweetTypeTone with bundled sound packs in both AppImage and binary formats.

## 📦 Build Formats

SweetTypeTone supports two deployment formats:

1. **AppImage** (Recommended) - Universal Linux package
2. **Binary Archive** - Standalone tarball with installer

Both formats include **20+ pre-installed sound packs** for immediate use.

---

## 🔨 Building Locally

### Prerequisites

- .NET 10 SDK
- Linux with evdev support
- `wget` (for downloading appimagetool)

### Build AppImage

```bash
./build-appimage.sh
```

**Output:** `SweetTypeTone-1.0.0-x86_64.AppImage`

**What it does:**
- Publishes the .NET application as a self-contained single-file binary
- Creates AppDir structure with bundled sound packs
- Bundles OpenAL libraries
- Creates permission setup helper
- Packages everything into a universal AppImage

**Size:** ~34 MB (includes binary + 20+ sound packs + libraries)

### Build Binary Archive

```bash
./build-binary.sh
```

**Output:** `SweetTypeTone-1.0.0-linux-x64.tar.gz`

**What it does:**
- Publishes the .NET application as a self-contained single-file binary
- Copies bundled sound packs
- Creates README and installation script
- Packages everything into a tarball

**Size:** ~30 MB (includes binary + 20+ sound packs)

---

## 🤖 Automated Releases (GitHub Actions)

### Triggering a Release

1. **Update version** in `build-appimage.sh` and `build-binary.sh` if needed
2. **Create and push a tag:**

```bash
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

3. **GitHub Actions will automatically:**
   - Build both AppImage and binary archive
   - Include all bundled sound packs
   - Create a GitHub Release
   - Upload both artifacts
   - Generate release notes

### Release Workflow

The `.github/workflows/release.yml` workflow:

1. **Builds binary archive:**
   - Publishes .NET app
   - Copies 20+ bundled sound packs
   - Creates tarball

2. **Builds AppImage:**
   - Runs `build-appimage.sh`
   - Includes bundled sound packs
   - Creates universal AppImage

3. **Creates GitHub Release:**
   - Uploads both artifacts
   - Generates comprehensive release notes
   - Lists all bundled sound packs
   - Includes setup instructions

---

## 🎹 Bundled Sound Packs

### Location

Sound packs are stored in `BundledSoundPacks/` directory.

### Included Packs (20+)

- **Cherry MX Black** (ABS & PBT)
- **Cherry MX Blue** (ABS & PBT)
- **Cherry MX Brown** (ABS & PBT)
- **Cherry MX Red** (ABS & PBT)
- **NovelKeys Cream**
- **Holy Pandas**
- **Topre Purple Hybrid (PBT)**
- **Everglide Crystal Purple**
- **Everglide Oreo**
- **Fallout Terminal**
- **Cream Travel**
- **MX Black Travel**
- **MX Blue Travel**
- **MX Brown Travel**
- **Turquoise**
- **HLKey**
- **Bruh**

### How Bundling Works

1. **AppImage:**
   - Sound packs copied to `AppDir/usr/share/BundledSoundPacks/`
   - Environment variable `SWEETTYPETONE_BUNDLED_PACKS` set in AppRun
   - On first run, packs copied to `~/.config/SweetTypeTone/SoundPacks/`

2. **Binary Archive:**
   - Sound packs included in `BundledSoundPacks/` directory
   - Application detects them in executable directory
   - On first run, packs copied to `~/.config/SweetTypeTone/SoundPacks/`

### Adding New Sound Packs

1. Add sound pack directory to `BundledSoundPacks/`
2. Ensure it has `config.json` (Mechvibes format) or individual sound files
3. Update `BundledSoundPacks/README.md`
4. Rebuild AppImage/binary

---

## 📤 Distribution

### GitHub Releases

**Recommended:** Use GitHub Actions for automated releases.

**Manual upload:**
1. Build locally: `./build-appimage.sh` and `./build-binary.sh`
2. Go to GitHub → Releases → Create new release
3. Upload both files:
   - `SweetTypeTone-x.x.x-x86_64.AppImage`
   - `SweetTypeTone-x.x.x-linux-x64.tar.gz`

### File Naming Convention

- AppImage: `SweetTypeTone-{VERSION}-x86_64.AppImage`
- Binary: `SweetTypeTone-{VERSION}-linux-x64.tar.gz`

Example: `SweetTypeTone-1.0.0-x86_64.AppImage`

---

## 🧪 Testing Builds

### Test AppImage

```bash
# Build
./build-appimage.sh

# Make executable
chmod +x SweetTypeTone-1.0.0-x86_64.AppImage

# Run
./SweetTypeTone-1.0.0-x86_64.AppImage

# Verify bundled packs are loaded
# Check: ~/.config/SweetTypeTone/SoundPacks/ should contain all packs
```

### Test Binary Archive

```bash
# Build
./build-binary.sh

# Extract to test directory
mkdir -p /tmp/test-sweettypetone
tar -xzf SweetTypeTone-1.0.0-linux-x64.tar.gz -C /tmp/test-sweettypetone

# Test
cd /tmp/test-sweettypetone
./SweetTypeTone

# Or test installer
./install.sh
~/.local/bin/SweetTypeTone
```

### Verification Checklist

- [ ] Application launches successfully
- [ ] Sound packs appear in UI (20+ packs)
- [ ] Can select and play different sound packs
- [ ] Volume control works
- [ ] System tray icon appears
- [ ] Keyboard sounds play when typing
- [ ] Permission setup dialog works (first run)

---

## 🔧 Troubleshooting

### AppImage Issues

**Problem:** AppImage won't run
```bash
# Extract and run manually
./SweetTypeTone-*.AppImage --appimage-extract
./squashfs-root/AppRun
```

**Problem:** Sound packs not loading
```bash
# Check environment variable
./SweetTypeTone-*.AppImage --appimage-extract
cat squashfs-root/AppRun  # Verify SWEETTYPETONE_BUNDLED_PACKS is set
ls squashfs-root/usr/share/BundledSoundPacks/  # Verify packs exist
```

### Binary Archive Issues

**Problem:** Sound packs not found
```bash
# Verify BundledSoundPacks directory exists
tar -tzf SweetTypeTone-*.tar.gz | grep BundledSoundPacks

# Check application can find them
./SweetTypeTone  # Should auto-detect BundledSoundPacks/ in same directory
```

### Build Issues

**Problem:** .NET SDK not found
```bash
# Install .NET 10 SDK
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

**Problem:** appimagetool fails
```bash
# Download manually
wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod +x appimagetool-x86_64.AppImage
mv appimagetool-x86_64.AppImage appimagetool
```

---

## 📊 Size Optimization

Current sizes:
- **AppImage:** ~34 MB (binary + packs + libs)
- **Binary Archive:** ~30 MB (binary + packs)

### Reducing Size

1. **Remove unused sound packs** from `BundledSoundPacks/`
2. **Enable aggressive trimming:**
   ```xml
   <PublishTrimmed>true</PublishTrimmed>
   <TrimMode>link</TrimMode>
   ```
3. **Compress sound files** (OGG has better compression than WAV)

---

## 🎯 Release Checklist

Before creating a release:

- [ ] Update version in `build-appimage.sh`
- [ ] Update version in `build-binary.sh`
- [ ] Update CHANGELOG.md
- [ ] Test both builds locally
- [ ] Verify all sound packs load correctly
- [ ] Create git tag
- [ ] Push tag to trigger GitHub Actions
- [ ] Verify GitHub Release created successfully
- [ ] Test downloads from GitHub Releases
- [ ] Update README.md with new version number

---

## 📝 Notes

- **First-run behavior:** Bundled packs are copied to user config on first run
- **Marker file:** `.bundled_packs_copied` prevents re-copying on subsequent runs
- **Custom packs:** Users can still add custom packs to `~/.config/SweetTypeTone/CustomSoundPacks/`
- **Updates:** New releases with updated packs won't override user's existing packs
- **Permissions:** Both formats include permission setup helpers for input group access

---

## 🔗 Resources

- **Build Scripts:**
  - `build-appimage.sh` - AppImage builder
  - `build-binary.sh` - Binary archive builder
  
- **Workflows:**
  - `.github/workflows/release.yml` - Automated release workflow
  
- **Documentation:**
  - `BundledSoundPacks/README.md` - Sound pack information
  - `APPIMAGE.md` - AppImage-specific details
  - `README.md` - User-facing documentation

---

**Happy Deploying! 🚀**
