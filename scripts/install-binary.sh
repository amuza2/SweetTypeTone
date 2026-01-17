#!/bin/bash

# SweetTypeTone Installation Script

set -e

INSTALL_DIR="$HOME/.local/bin"
SHARE_DIR="$HOME/.local/share"
APP_NAME="SweetTypeTone"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "🎵 Installing SweetTypeTone..."
echo ""

# Create directories
mkdir -p "$INSTALL_DIR"
mkdir -p "$SHARE_DIR/applications"
mkdir -p "$SHARE_DIR/icons/hicolor/96x96/apps"
mkdir -p "$SHARE_DIR/$APP_NAME"

# Copy binary and dependencies
echo "📋 Installing application files to $SHARE_DIR/$APP_NAME..."
cp "$SCRIPT_DIR/$APP_NAME" "$SHARE_DIR/$APP_NAME/"
if ls "$SCRIPT_DIR"/*.so 1> /dev/null 2>&1; then
    echo "📦 Installing shared libraries..."
    cp "$SCRIPT_DIR"/*.so "$SHARE_DIR/$APP_NAME/"
fi
chmod +x "$SHARE_DIR/$APP_NAME/$APP_NAME"

# Copy bundled sound packs
if [ -d "$SCRIPT_DIR/BundledSoundPacks" ]; then
    echo "🎹 Installing bundled sound packs..."
    mkdir -p "$SHARE_DIR/$APP_NAME/BundledSoundPacks"
    cp -r "$SCRIPT_DIR/BundledSoundPacks"/* "$SHARE_DIR/$APP_NAME/BundledSoundPacks/"
    echo "✅ Sound packs installed"
fi

# Create symlink in bin
echo "🔗 Creating symlink in $INSTALL_DIR..."
ln -sf "$SHARE_DIR/$APP_NAME/$APP_NAME" "$INSTALL_DIR/$APP_NAME"

# Copy icon to multiple sizes
if [ -f "$SCRIPT_DIR/icon.png" ]; then
    echo "🎨 Installing application icon..."
    for size in 48 64 96 128 256; do
        mkdir -p "$SHARE_DIR/icons/hicolor/${size}x${size}/apps"
        cp "$SCRIPT_DIR/icon.png" "$SHARE_DIR/icons/hicolor/${size}x${size}/apps/sweettypetone.png"
    done
    echo "✅ Icon installed in multiple sizes"
fi

# Create desktop entry
echo "📝 Creating desktop entry..."
cat > "$SHARE_DIR/applications/sweettypetone.desktop" << DESKTOP
[Desktop Entry]
Version=1.1
Type=Application
Name=SweetTypeTone
Comment=Play keyboard sounds as you type
Exec=$APP_NAME
Icon=sweettypetone
Terminal=false
Keywords=keyboard;sound;typing;mechanical;
Categories=AudioVideo;Audio;
StartupNotify=true
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
echo "📁 Installation location: $SHARE_DIR/$APP_NAME"
echo "🚀 To run: $APP_NAME"
echo ""
echo "💡 You can also find SweetTypeTone in your application menu."
echo ""
echo "⚠️  First run: You'll need to add yourself to the 'input' group:"
echo "   sudo usermod -aG input \$USER"
echo "   Then log out and log back in."
echo ""
