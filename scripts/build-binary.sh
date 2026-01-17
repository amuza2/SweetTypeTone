#!/bin/bash

# SweetTypeTone Binary Build Script
# This script builds a standalone binary with bundled sound packs

set -e  # Exit on error

VERSION="1.1.0"
ARCH="x86_64"
APP_NAME="SweetTypeTone"
OUTPUT_DIR="./publish/linux-x64"

echo "🎵 Building ${APP_NAME} Binary v${VERSION}..."
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf ${OUTPUT_DIR}
rm -f ${APP_NAME}-${VERSION}-linux-x64.tar.gz

# Publish the application
echo "📦 Publishing application..."
dotnet publish src/SweetTypeTone.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ${OUTPUT_DIR}

echo ""
echo "✅ Build completed successfully!"
echo ""

# Copy bundled sound packs
echo "🎹 Copying bundled sound packs..."
if [ -d "./BundledSoundPacks" ]; then
    mkdir -p ${OUTPUT_DIR}/BundledSoundPacks
    cp -r ./BundledSoundPacks/* ${OUTPUT_DIR}/BundledSoundPacks/
    PACK_COUNT=$(ls -1 BundledSoundPacks | wc -l)
    echo "✅ Copied ${PACK_COUNT} sound packs"
else
    echo "⚠️  Warning: BundledSoundPacks directory not found"
fi

# Create README for the binary package
echo "📝 Creating README..."
cat > ${OUTPUT_DIR}/README.txt << 'EOF'
# SweetTypeTone - Mechanical Keyboard Sounds for Linux

## Quick Start

1. Run the application:
   ./SweetTypeTone

2. First run setup (required):
   - Click "Yes" when prompted to configure permissions
   - Enter your password
   - Log out and log back in
   - Run again

   Or manually:
   sudo usermod -aG input $USER
   (then log out and log back in)

## Bundled Sound Packs

This package includes 20+ pre-installed sound packs in the BundledSoundPacks/ directory.
On first run, these will be automatically copied to ~/.config/SweetTypeTone/SoundPacks/

You can also add custom sound packs to:
~/.config/SweetTypeTone/CustomSoundPacks/

## Features

- Real-time keyboard sound playback
- 20+ bundled mechanical keyboard sound packs
- Volume control and mute toggle
- System tray support
- Mechvibes-compatible sound pack format

## Requirements

- Linux with evdev support
- OpenAL library (usually pre-installed)
- User must be in 'input' group for keyboard monitoring

## Support

- GitHub: https://github.com/amuza2/SweetTypeTone
- Issues: https://github.com/amuza2/SweetTypeTone/issues
- Ko-fi: https://ko-fi.com/codingisamazing

Enjoy your typing sounds! 🎵
EOF

# Copy application icon
echo "🎨 Copying application icon..."
if [ -f "src/SweetTypeTone/Assets/icons8-key-press-96.png" ]; then
    cp src/SweetTypeTone/Assets/icons8-key-press-96.png ${OUTPUT_DIR}/sweettypetone.png
    echo "✅ Icon copied"
fi

# Create installation script
echo "🔧 Creating installation script..."
cat > ${OUTPUT_DIR}/install.sh << 'EOF'
#!/bin/bash

# SweetTypeTone Installation Script

set -e

INSTALL_DIR="$HOME/.local/bin"
SHARE_DIR="$HOME/.local/share"
APP_NAME="SweetTypeTone"

echo "🎵 Installing SweetTypeTone..."
echo ""

# Create directories
mkdir -p "$INSTALL_DIR"
mkdir -p "$SHARE_DIR/applications"
mkdir -p "$SHARE_DIR/icons/hicolor/96x96/apps"
mkdir -p "$SHARE_DIR/SweetTypeTone"

# Copy binary
echo "📋 Installing application files to $SHARE_DIR/SweetTypeTone..."
cp "$APP_NAME" "$SHARE_DIR/SweetTypeTone/"
cp *.so "$SHARE_DIR/SweetTypeTone/" 2>/dev/null || true
chmod +x "$SHARE_DIR/SweetTypeTone/$APP_NAME"

# Create symlink in bin
echo "🔗 Creating symlink in $INSTALL_DIR..."
ln -sf "$SHARE_DIR/SweetTypeTone/$APP_NAME" "$INSTALL_DIR/$APP_NAME"

# Copy icon to multiple sizes
if [ -f "sweettypetone.png" ]; then
    echo "🎨 Installing application icon..."
    for size in 48 64 96 128 256; do
        mkdir -p "$SHARE_DIR/icons/hicolor/${size}x${size}/apps"
        cp sweettypetone.png "$SHARE_DIR/icons/hicolor/${size}x${size}/apps/sweettypetone.png"
    done
    echo "✅ Icon installed in multiple sizes"
fi

# Copy bundled sound packs
BUNDLED_DIR="$SHARE_DIR/SweetTypeTone/BundledSoundPacks"
if [ -d "BundledSoundPacks" ]; then
    echo "🎹 Installing bundled sound packs..."
    mkdir -p "$BUNDLED_DIR"
    cp -r BundledSoundPacks/* "$BUNDLED_DIR/"
    echo "✅ Installed $(ls -1 BundledSoundPacks | wc -l) sound packs"
fi

# Create desktop entry
echo "📝 Creating desktop entry..."
cat > "$SHARE_DIR/applications/sweettypetone.desktop" << 'DESKTOP'
[Desktop Entry]
Version=1.1
Type=Application
Name=SweetTypeTone
Comment=Play keyboard sounds as you type
Exec=SweetTypeTone
Icon=sweettypetone
Terminal=false
Keywords=keyboard;sound;typing;mechanical;
Categories=AudioVideo;Audio;
StartupNotify=false
DESKTOP

# Update desktop database
if command -v update-desktop-database &> /dev/null; then
    update-desktop-database "$SHARE_DIR/applications" 2>/dev/null || true
fi

# Update icon cache
if command -v gtk-update-icon-cache &> /dev/null; then
    gtk-update-icon-cache -f -t "$SHARE_DIR/icons/hicolor" 2>/dev/null || true
fi

# Add to PATH if not already there
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
    echo ""
    echo "⚠️  Add $INSTALL_DIR to your PATH by adding this to ~/.bashrc or ~/.zshrc:"
    echo "   export PATH=\"\$HOME/.local/bin:\$PATH\""
fi

echo ""
echo "✅ Installation complete!"
echo ""
echo "📁 Installation location: $SHARE_DIR/SweetTypeTone"
echo "🚀 To run: $APP_NAME"
echo ""
echo "⚠️  First run setup required:"
echo "   sudo usermod -aG input $USER"
echo "   Then log out and log back in"
echo ""
EOF

chmod +x ${OUTPUT_DIR}/install.sh

# Create archive
echo ""
echo "📦 Creating archive..."
cd ${OUTPUT_DIR}
tar -czf ../../${APP_NAME}-${VERSION}-linux-x64.tar.gz *
cd ../..

echo ""
echo "✅ Binary package created successfully!"
echo ""
echo "📦 Output: ./${APP_NAME}-${VERSION}-linux-x64.tar.gz"
echo "📊 Size: $(du -h ${APP_NAME}-${VERSION}-linux-x64.tar.gz | cut -f1)"
echo ""
echo "📂 Contents:"
echo "   - SweetTypeTone (binary)"
echo "   - BundledSoundPacks/ (20+ sound packs)"
echo "   - README.txt"
echo "   - install.sh"
echo ""
echo "🚀 To test:"
echo "   tar -xzf ${APP_NAME}-${VERSION}-linux-x64.tar.gz -C /tmp/test"
echo "   cd /tmp/test"
echo "   ./SweetTypeTone"
echo ""
echo "📤 To distribute:"
echo "   Upload ${APP_NAME}-${VERSION}-linux-x64.tar.gz to GitHub Releases"
echo ""
