# AppImage Build Guide

This guide explains how to build and distribute SweetTypeTone as an AppImage.

## What is AppImage?

AppImage is a universal Linux package format that works on all major distributions without installation. Users simply download, make executable, and run.

## Prerequisites

- .NET 10 SDK
- `appimagetool` (will be downloaded automatically if not found)

## Building the AppImage

Run the build script:

```bash
./build-appimage.sh
```

This will:
1. Publish the application as a self-contained binary
2. Create the AppDir structure
3. Copy all dependencies and bundled sound packs
4. Package everything into `SweetTypeTone-1.0.0-x86_64.AppImage`

## What's Included

The AppImage contains:
- ✅ SweetTypeTone executable
- ✅ All .NET runtime dependencies
- ✅ All shared libraries (OpenAL, etc.)
- ✅ 14 bundled sound packs (21MB)
- ✅ Application icon and desktop file

## Distribution

### Upload to GitHub Releases

```bash
# The AppImage is ready to distribute
ls -lh SweetTypeTone-1.0.0-x86_64.AppImage

# Upload to GitHub Releases page
```

### User Installation

Users can run the AppImage with:

```bash
# Download
wget https://github.com/amuza2/SweetTypeTone/releases/latest/download/SweetTypeTone-1.0.0-x86_64.AppImage

# Make executable
chmod +x SweetTypeTone-1.0.0-x86_64.AppImage

# Run
./SweetTypeTone-1.0.0-x86_64.AppImage
```

## Permissions Setup

Users still need to setup input permissions once:

```bash
# Extract setup script from AppImage
./SweetTypeTone-1.0.0-x86_64.AppImage --appimage-extract

# Run setup
sudo squashfs-root/usr/bin/setup-permissions.sh

# Log out and log back in
```

Or provide the `setup-permissions.sh` script separately.

## Testing

Test the AppImage on different distributions:

```bash
# Run the AppImage
./SweetTypeTone-1.0.0-x86_64.AppImage

# Check if sound packs are loaded
# Check if audio works
# Check if keyboard monitoring works (after permissions setup)
```

## Troubleshooting

### AppImage won't run
- Make sure it's executable: `chmod +x SweetTypeTone-1.0.0-x86_64.AppImage`
- Check if FUSE is installed: `sudo apt install fuse libfuse2` (Ubuntu/Debian)

### No sound packs
- Sound packs are bundled and copied to `~/.config/SweetTypeTone/SoundPacks/` on first run
- Check console output for errors

### Keyboard monitoring doesn't work
- Run `setup-permissions.sh` and log out/in
- Check if user is in `input` group: `groups`

## File Size

Expected AppImage size: ~45-50MB (includes .NET runtime + sound packs)

## Updating the Version

Edit `build-appimage.sh` and change:
```bash
VERSION="1.0.0"  # Update this
```

Then rebuild the AppImage.
